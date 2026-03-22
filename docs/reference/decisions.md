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

## Aspect Ratio Masks & Sensor Modes

> **Gate ratio from resolution, not sensor_area_mm:** Many cameras (DSLRs, mirrorless) have a photo sensor wider than their video active area. The 5D Mk III has a 3:2 sensor but only outputs 16:9 video. The gate ratio (maximum visible area) comes from the sensor mode's resolution, not from `sensor_area_mm`. The `sensor_area_mm` is used for FOV scale (physical width → horizontal FOV), while the resolution determines the gate's aspect ratio.

> **Sensor crop scaling for modes without sensor_area_mm:** When a sensor mode has no `sensor_area_mm`, `ComputeGateWidth` derives it from the first mode (open gate) scaled by the resolution ratio: `openGate.SensorAreaWidthMm × (mode.ResolutionWidth / openGate.ResolutionWidth)`. This is correct for sensor-windowed cameras (RED, Phantom, Blackmagic) where lower resolutions read a smaller center portion of the sensor. It is NOT correct for cameras that downsample from the full sensor (most mirrorless/DSLR HD modes) — those modes must have explicit `sensor_area_mm` in the database. See `fix-hd-sensor-areas.py`.

> **GateFit.Overscan:** Unity's physical camera `gateFit` is set to `Overscan`. This ensures the entire sensor gate is visible within the viewport. When the sensor aspect doesn't match the screen, the camera renders extra content beyond the sensor in one dimension. Mask bars (UI Toolkit overlay) cover the excess. Combined with `Camera.rect` constraining the viewport to the sensor aspect, the horizontal FOV is locked to the sensor width regardless of screen shape.

> **Camera.rect constrains the viewport:** `SyncViewportRect` sets `Camera.rect` to match the effective sensor aspect ratio. Without this, Unity derives hFov from vFov × screenAspect, producing wider-than-sensor horizontal content when the sensor is narrower than the screen. This is critical for physically accurate FOV.

> **SyncEffectiveSensor computes the delivery crop:** When a delivery ratio (16:9, 2.39:1, etc.) is selected, `SyncEffectiveSensor` computes the effective sensor dimensions as a crop of the gate. Wider-than-gate ratios use full gate width with reduced height. Narrower-than-gate ratios use full gate height with reduced width. This changes the camera's FOV to match what the real camera would see in that delivery format.

> **Full Screen = open gate:** "Full Screen" uses the sensor mode's native aspect ratio (open gate), not "fill the window." The FOV shows exactly what the sensor captures. On a wider screen, pillarbox bars appear.

> **DOF unaffected by sensor changes:** DOF parameters (focal length, aperture, focus distance) are set directly on the URP Volume override. Changing `Camera.sensorSize` for FOV purposes does not affect the DOF computation.

> **Known limitation — crop mode sensor areas:** Cameras with center-crop video modes (5D Mk IV 4K at 1.74x, EOS R 4K at 1.75x, etc.) need accurate `sensor_area_mm` in the database reflecting the crop area, not the full photo sensor. Incorrect `sensor_area_mm` produces wrong FOV scale (objects appear too small). The gate RATIO is still correct (from resolution), but the FOV SCALE is wrong. See `fix-crop-sensor-areas.py`.

> **Anamorphic interaction (not yet implemented):** When anamorphic lenses are added, the squeeze factor affects horizontal FOV (wider than the sensor would normally produce). The delivery format should be derived from sensor aspect × squeeze factor. The sensor mode → FOV → mask pipeline will need anamorphic awareness. See the TODO in `CameraElement.VerticalFov`.

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
