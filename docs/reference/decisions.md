# Architectural Decisions

Confirmed decisions that shape the implementation.

---

## Assembly Structure

> **Four assemblies, layered:** `Fram3d.Core` (pure C#) → `Fram3d.Services` (pure C#) → `Fram3d.Engine` (Unity) → `Fram3d.UI` (Unity). Each references only downward. Three compile-time boundaries: Core can't touch Unity, Services can't touch Unity, Engine can't reference UI. See design-sessions/01 for rationale.

> **Core is one assembly with namespaces:** Common, Timeline, Assets, Camera, Character, Shot, Scene. Natural domain couplings (camera reads character positions, linking reads bone transforms) are allowed within the assembly. Discipline enforced via `internal` access modifiers on type internals.

> **Sequencing context dissolved:** Timeline data lives in Core.Timeline and Core.Shot. Timeline UI lives in Fram3d.UI.Timeline. The old Sequencing bounded context no longer exists.

## Element Model

> **Element inheritance:** Base `Element` class in Core.Common. Derived types in domain namespaces: `CameraElement` in Core.Camera, `CharacterElement` in Core.Character, `LightElement` in Core.Scene. UI and Engine typeswitch for specialized behavior.

> **CameraElement IS the rig:** No separate CameraRig class. CameraElement has properties AND operations (Dolly, Pan, Tilt, Crane, Truck, Roll, Orbit, DollyZoom, FocusOn, Follow, Watch, MountSnorricam). Internally delegates to private helpers for organization. Follows prior codebase lesson: unified facade.

> **Transforms on Element using System.Numerics:** `Vector3` position, `Quaternion` rotation, `float` scale on the Element base class. Core stays Unity-free. Conversion to UnityEngine types at the integration boundary via extension methods.

> **Element.BoundingRadius:** Float set by Engine on element creation. Used by FocusController, AI camera positioning, marquee selection.

## Data Management

> **Registries for shared data:** `ElementRegistry` and `ShotRegistry` in Core.Common with typed queries and `IObservable` change streams. Context-private data uses plain collections.

> **Identity types in Common:** `ElementId`, `ShotId`, `KeyframeId` all in Core.Common. Prevents downstream namespaces from needing upstream references for identity resolution.

> **Cross-context communication via IObservable on the source:** No central event bus. Registries expose `IObservable<T>` streams directly. Synchronous `OnNext()` delivery. If sibling-to-sibling communication is ever needed, a central bus can be added on top.

> **IInterpolatable\<T\> for generic timeline evaluation:** Domain types implement their own lerp. Timeline stays generic — calls `T.Lerp()`, never knows what T represents.

## Commands & Undo

> **CommandStack (not UndoStack):** `ICommand`, `CommandStack`, `CompoundCommand` in Core.Common. CommandStack has `Execute()`, `Undo()`, `Redo()` and publishes `IObservable` streams (Executed, Undone, Redone). Each namespace defines its own commands. Persistence is purely save/load.

> **Undo model:** Global stack, not per-shot. Cross-shot undo does not auto-switch the active shot.

> **Delete behavior:** Delete key = keyframe only. Cmd+Delete = element. Context-sensitive to avoid ambiguity.

## Evaluation Pipeline

> **SceneEvaluator directly calls domain types:** No interfaces, no per-element self-evaluation. Holds references to registries and domain objects, calls methods in explicit order. ~30 lines. Lives in Engine.Evaluation.

> **Event-driven evaluation triggers:** SceneEvaluator subscribes to CommandStack.Executed/Undone/Redone and Playhead.Scrubbed. No domain code sets dirty flags. During playback, evaluates every frame. During scrub/command, evaluates once. Sync always runs (covers gizmo drags).

> **Gizmo drag flow:** During drag, gizmo writes directly to Element.Position (no command, no evaluation). On release, command created with before/after state. CompoundCommand wraps move + keyframe if stopwatch is recording. Evaluation pipeline skips during drag (no triggers fire).

> **Export evaluation:** Bake procedural effects (shake, follow, snorricam) into keyframes first. Step through frames at 1/fps increments via SceneEvaluator.EvaluateAtTime(). Same evaluation logic, different time source.

## Camera & Animation

> **No Cinemachine:** Removed from tech stack. All camera computation in domain code (Core.Camera). CameraBehaviour syncs to plain `UnityEngine.Camera` with `usePhysicalProperties = true`. DOF via URP post-processing Volume (Bokeh mode — accepts focal length, aperture, focus distance directly). Every feature Cinemachine provided, we compute ourselves.

> **Follow/watch/shake state on CameraElement:** Not on Angle or Shot. Prevents Camera → Shot dependency. Per-shot behavior handled by Engine: when switching shots, Engine updates CameraElement from the shot's configuration.

> **Timeline is data structures, not a controller:** Timeline provides `Keyframe<T>`, `KeyframeManager<T>`, `Track`, evaluation. SceneEvaluator orchestrates per-frame updates. Timeline doesn't know what T represents.

> **Shot reordering and the global timeline:** Shots swap time ranges. Camera keyframes travel with their shot (per-shot data). Element keyframes stay at original global timeline positions. Matches real film editing: reorder coverage, action stays the same.

> **Keyframe snap granularity:** Frame boundaries (1/fps), not fixed time intervals. At 24fps, keyframes snap to 1/24s increments.

## Rendering & Format

> **Playback frame dropping:** When scenes are too heavy for real-time playback, drop frames (maintain timing, skip visual updates) rather than slowing down.

> **Rotation storage:** Quaternions internally, pan/tilt/roll displayed in UI. Prevents gimbal lock transparently.

> **Recording:** Only changed properties get keyframed (not all properties). Per-track stopwatch model (AE/Premiere). Stopwatches default to off.

> **UI framework:** UI Toolkit (`UnityEngine.UIElements`), not legacy UI. All UI built programmatically in C# — no UXML, no USS files.

> **File format:** Deferred to implementation. Human-readable preferred for git diffing and debugging. Must serialize full project state (see 4.2 spec).

---

## Pending

### Scene Serialization & Lazy Loading

Scenes must be independently loadable. Only the active scene's heavy data in memory; inactive scenes keep lightweight metadata. Consider project manifest + individual scene files. Character definitions project-level, referenced by scenes.

**Resolve before Save/Load spec (Milestone 4.2).**

### Infrastructure to Build Early

- **Settings / Preferences panel:** Centralized so new settings can be added incrementally.
- **Properties panel:** Contextual sidebar showing editable properties for the selected element, changing content by selection type.
