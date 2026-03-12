# Fram3d Architecture

**Date**: 2026-03-10
**Companion to**: `fram3d-roadmap.md`

---

## 1. Platform & Framework

- **Engine**: Unity 6000.1.11f1
- **Camera system**: Cinemachine 3.x (`CinemachineCamera`)
- **Input**: Unity Input System (`UnityEngine.InputSystem`) with direct `Keyboard.current` / `Mouse.current` access
- **UI framework**: Unity legacy UI (`UnityEngine.UI`) — Canvas, Image, Text, Button, InputField
- **UI construction**: All UI built programmatically at runtime. No prefabs, no UXML.

---

## 2. Confirmed Decisions

> **File format**: Deferred to implementation. Human-readable preferred for git diffing and debugging. Must serialize full project state (see 4.2 spec).

> **Playback frame dropping**: When scenes are too heavy for real-time playback, drop frames (maintain timing, skip visual updates) rather than slowing down. The playback system tracks wall-clock time and evaluates the timeline at the correct time regardless of render performance.

> **Rotation storage**: Quaternions internally, pan/tilt/roll displayed in UI. Prevents gimbal lock transparently.

> **Recording**: Only changed properties get keyframed (not all properties). Per-track stopwatch model (AE/Premiere). Stopwatches default to off (not recording).

> **Undo model**: Global stack, not per-shot. Cross-shot undo does not auto-switch the active shot.

> **Delete behavior**: Delete key = keyframe only. Cmd+Delete = selected element. Context-sensitive to avoid ambiguity.

---

## 3. Patterns to Reuse

Good patterns from the prior codebase worth carrying forward.

**`ICameraState` interface isolating Cinemachine.** The interface exposes only `Position`, `Rotation`, `FieldOfView`, `Forward`, `Right`, `Translate()`, `RotateAround()`. The concrete `CinemachineCameraState` is a 49-line adapter. Every controller depends on the interface — Cinemachine is never imported outside the adapter. This is the cleanest boundary in the old codebase.

**`VirtualCameraRig` as a unified facade.** Four sub-controllers (Movement, Lens, Focus, Shake) behind a single facade that exposes named cinema operations: `Dolly()`, `Pan()`, `Tilt()`, `Orbit()`, `Crane()`, `Roll()`, `DollyZoom()`, `Zoom()`, `FocusOn()`. No caller reaches into sub-controllers directly.

**`CameraAspectRatio` — closed type hierarchy.** Abstract class with private constructor and sealed nested subclasses instead of an enum. Each value carries `DisplayName` and `Value`, supports `GetNext()` cycling and implicit float conversion. No switch statements needed.

**`KeyframeManager<T>` — dual storage.** Maintains both a `List<T>` (ordered, for iteration/evaluation) and a `Dictionary<KeyframeId, T>` (for O(1) lookup). `AddKeyframe()` removes any existing keyframe at the same time before inserting.

**`AnimationFrameTracker` — frame boundary sentinel.** Records `Time.frameCount` when animation is applied; trackers check it to distinguish animation-driven movement from user-driven movement. 9 lines, no polling or delta comparison.

**`CompositeAnimationCurveSet` — extensible multi-track curves.** `RebuildCurves()` iterates an array of `IAnimationCurveSet` implementations and populates them from keyframes. Adding a new animated property = implement the interface, add to the array. Clean extension point.

**Value objects with validation.** `KeyframeId` wraps `Guid`, rejects `Guid.Empty` at construction, implements full equality. `TimePosition` rejects negative values, provides `Add()`/`Subtract()` (clamps to zero), `ToFrames(frameRate)`, comparison operators. Prevents raw-float time bugs.

---

## 4. Anti-patterns from the Prior Codebase

**God object state classes.** `TimelineState` held shot list, selection indices, current time, IsEvaluating flag, duration editing state, thumbnail references, AND keyframe marker dictionaries — all in one class. The aggregate design (Scene, ShotSetup, Project) exists to prevent this.

**Routing internal operations through the command pattern.** `ScrubTime`, `EvaluateShotAtTime`, `RefreshThumbnails`, `RefreshKeyframeEditor`, `ReorderThumbnails`, and `UpdateTimeline` were all `ICommand` implementations. None are user actions. None should be undoable. Commands are exclusively for user-initiated state changes (the list in spec 4.1.1). Everything else is a plain method call.

**`FindObjectOfType` for dependency resolution.** `TimelineState.EvaluateShot()` called `Object.FindObjectOfType<ApplicationController>()` to reach back into the scene graph — domain model depending on the Unity scene. `SceneElement.Awake()` made four separate `FindObjectOfType` calls. Dependencies should be injected at construction or wiring time, not scraped from the scene at runtime.

**UI references in the domain model.** `TimelineState` stored `Dictionary<AnimationKeyframe, KeyframeMarker>` where `KeyframeMarker` is a UI element. This means the domain can't be tested without Unity UI. The mapping between domain objects and their visual representation belongs in the UI layer.

**Transient command objects allocated every `Update()`.** `new TimelineInteractionHandler(...).Execute()` and `new UpdateTimeline(...).Execute()` were called every frame, allocating heap objects that immediately die. These are stateless procedures dressed as objects — they should be reused instances or static methods.

**Mixed input systems.** The project uses the New Input System (`Keyboard.current` / `Mouse.current`) in `UserInputDriver`, but `UpdateTimeline`, `TimelineInteractionHandler`, and `CameraInfoView` use legacy `UnityEngine.Input.GetKeyDown()`. Pick one input system and use it everywhere.

**Rotation shake drift.** Position shake is applied as an additive offset and reverted each frame. Rotation shake compounds via `Rotation *= Quaternion.Euler(...)` without reverting. The camera slowly drifts in orientation while shake is enabled. Both must use the revert-then-apply pattern.

**Side effects in predicates.** `ScreenState.HasStateChanged()` updates internal `_last*` cache fields when called. Calling it twice in one frame returns `true` then `false`. Predicates should be pure — separate the query from the cache update.

---

## 5. Tuned Constants

Values from the prior codebase that were empirically tuned. Starting points — expect to re-tune.

**Camera movement speeds:**
- `DollyScrollSpeed`: 0.01
- `PanTiltSpeed`: 0.2
- `RollSpeed`: 0.03
- `CraneSpeed`: 0.02
- `TruckSpeed`: 0.02
- `DefaultPosition`: (0, 1.6, -5)
- `DefaultRotation`: Identity quaternion

**Focus:**
- Lerp speed: 2.0
- Distance multiplier: 1.5x (breathing room when focusing on an element)
- Minimum distance: 0.1 units

**Camera shake:**
- Perlin noise-based
- Default amplitude: 0.1, frequency: 1.0
- Position scale: 0.01
- Rotation scale: 0.5 (X/Y only, no Z roll)
- Rotation time offset for decorrelation: 100.0

**Lens:**
- Default focal length: 35mm
- Common focal lengths: {14, 18, 24, 35, 50, 85, 100, 135, 200}
- Focal length adjustment multiplier: 0.5 per scroll unit
- Dolly zoom speed: 0.5
- Lens smoothing lerp speed: 10

**Recording thresholds:**
- Near existing keyframe: 0.1 seconds. If near: update existing. If not: create new.
- Camera change detection: position 0.001 units, rotation 0.01 degrees, focal length 0.01mm
- Element change detection: position 0.001, rotation 0.01, scale 0.001

**Timeline:**
- Default shot duration: 5.0 seconds
- Minimum shot duration: 0.1 seconds
- Keyframe time tolerance (same-time conflict): 0.01 seconds

**Input sensitivities:**
- Drag sensitivity: 0.2
- Scroll sensitivity: 0.02
- Scroll deadzone: 0.01
- Sideways movement threshold: 0.01

**Undo coalescing:** 1000ms inactivity timeout for scroll gestures.

---

## 6. Implementation Details

**Keyframe interaction rules:**
- Moving a main keyframe moves all child property keyframes
- Moving a child keyframe creates a new main keyframe at the target time containing only that property; the original slot empties
- Deleting a main keyframe deletes all children
- Deleting all children deletes the main keyframe
- Dragging onto an existing keyframe silently merges
- Snap to 0.1s during drag

**Element linking constraints:** Max chain depth: 4.

**Input mappings:**
- Mouse (scene): Scroll Y = focal length, +Alt = dolly, +Shift = crane, +Ctrl = roll, +Cmd+Alt = dolly zoom, Scroll X+Cmd = truck, Ctrl-drag = pan/tilt, Alt-drag = orbit (around selected or world origin)
- Mouse (timeline): Scroll = zoom at cursor, Shift+Scroll = pan horizontally, Middle-click drag = pan in the track area and shot track
- Keyboard (scene): Space = play/pause, QWER = active tool (Select, Translate, Rotate, Scale), F = focus, 1-9 = focal length presets, C = camera keyframe, V = element keyframe, Arrows = scrub 1 frame, Delete = context-sensitive delete, Ctrl+D = duplicate, Ctrl+R = reset camera, Cmd+Z / Cmd+Shift+Z = undo/redo
- Keyboard (overlays): A/Shift+A = aspect ratio cycle, G = composition guides, H = camera info toggle, P = camera path, D = Director View
- Keyboard (panels): O = Elements panel, T = timeline, Tab = toggle all panels, Home = start, End = end, \\ = zoom to fit
- Keyboard (multi-camera): Shift+1/2/3/4 = switch active camera

**Shot track interaction:**
- Single-click a camera row = preview that camera (dimmed display, non-destructive)
- Double-click a camera row = activate that camera AND zoom to shot (8% padding)
- Shot track auto-adjusts row height based on max camera count across shots

**Boundary drag (ripple editing):**
- Dragging a shot boundary shifts all downstream content: shots, camera keyframes, active angle segments, and linked periods
- Hold Shift = shots only (element keyframes stay in place)
- Snaps to frame boundaries
- Resize tooltip shows shot name, duration (seconds + frames), and ripple mode

**Keyframe interpolation shapes (AE-style):**
- Three shapes: diamond (linear), circle (smooth), square (hold)
- Alt+click a keyframe to cycle through shapes
- Default heuristic: camera keyframes → smooth, single-keyframe elements → hold, multi-keyframe elements → linear
- Between-keyframe curve indicators on expanded sub-tracks: ─ linear, ⌒ ease-in, ⌓ ease-out, ~ ease-in-out, ∿ bezier

**Live interpolated values:**
- When a track is expanded, sub-track labels show the real-time interpolated value at the playhead position (e.g., `Position (1.2, 0.9, -1.5)`)

**Dual timecode display:**
- Transport bar: shot-local elapsed / duration
- Camera info overlay: sequence-global timecode
- Format: semicolon-separated `HH;MM;SS;FF`

**Playback auto-scroll:**
- During playback, if the playhead exits the visible range, the view shifts to follow while maintaining zoom level

**View layout system:**
- Three layouts: single, side-by-side, three-view (top-wide + two bottom)
- Camera View is a movable DOM element — only one instance exists
- When reassigning Camera View to a new view, the old view receives the displaced view type (smart swap)
- Camera View must always exist in exactly one view

**Panel system (gutters):**
- JetBrains-style vertical label strips on left/right workspace edges
- Left gutter: Overview toggle
- Right gutter: Elements, Assets toggles
- Mutual exclusion: opening Elements closes Assets and vice versa
- Tab key toggles all panels simultaneously

**Timeline and panel resize:**
- Vertical drag handle between the view area and timeline. Min 80px, max 80vh.
- Side panels have horizontal drag edges. Min 150px, max 500px.

---

## 7. Architecture Considerations

### 7.1 Scene Serialization & Lazy Loading

**Source**: Multi-Scene Project Structure spec — unlimited scenes, lazy load/lazy render

Scenes must be independently loadable without pulling in sibling scene data. Only the active scene's heavy data (meshes, textures, keyframes) should be in memory; inactive scenes keep only lightweight metadata (element list, shot count, character assignments) for fast tab switching.

**Implications for file format:**
- The project file format needs clear scene boundaries — either separate chunks within a single file, or separate files per scene within a project bundle (e.g., `project.fram3d/scene-1.json`, `project.fram3d/scene-2.json`)
- Scene-level save should be possible without re-serializing the entire project
- Consider: a project manifest file (settings, character definitions, scene order) + individual scene files. This also makes scene duplication and deletion cheap (copy/delete a file rather than splicing a monolithic blob)
- Character definitions live at project level but are referenced by scenes — avoid duplicating character data across scene files

**Resolve before Save/Load spec (Milestone 4.2).**

### 7.2 Infrastructure to Build Early

Cross-cutting systems that multiple downstream features depend on. Build during Phase 4 timeframe to avoid reimplementing for each feature.

- **Settings / Preferences panel:** Centralized settings panel so new settings can be added incrementally as features ship. Avoids scattering configuration UIs across the application.
- **Panel / docking system:** General panel system with docking support. Downstream features (Elements panel, pose library, Assets panel) all require dockable panels.

---

## 8. Domain Modeling Approach

DDD-informed, not DDD-orthodox. The cinema domain is rich enough to warrant modeling discipline; Unity is opinionated enough that full layered architecture would fight the engine.

### 8.1 What We Use

**Ubiquitous language.** Cinema terminology is consistent from roadmap to spec to class name to method name. Dolly, crane, truck, angle, blocking, stopwatch — these terms mean the same thing everywhere. This is the single highest-value DDD concept.

**Bounded contexts via Assembly Definitions.** Each context is a separate `.asmdef`, enforcing boundaries at compile time:

| Context | Core Concepts | Assembly | Milestones |
|---------|--------------|----------|------------|
| Camera | Rig, lens, focus, shake, DOF | `Fram3d.Camera` | 1.1, 1.2, 6.2, 7.2, 9.1 |
| Sequencing | Shot, keyframe, track, playback, stopwatch | `Fram3d.Sequencing` | 3.1, 3.2, 8.4 |
| Scene | Element, selection, gizmo, ground plane | `Fram3d.Scene` | 2.1, 5.1, 6.3, 8.1, 10.1 |
| Viewport | Panel system, layout, views | `Fram3d.Viewport` | 2.2, 8.2 |
| Characters | Mannequin, pose, expression, skeleton | `Fram3d.Characters` | 6.1, 7.1, 12.2 |
| Persistence | Project, scene file, asset bundle | `Fram3d.Persistence` | 4.1, 4.2, 8.3 |
| Assets | Import, library, environments | `Fram3d.Assets` | 4.3, 5.2, 5.3, 12.3 |
| Export | Renderer, storyboard, EDL | `Fram3d.Export` | 4.4 |
| AI | Shot description, blocking, suggestions | `Fram3d.AI` | 11.1, 11.2, 11.3, 12.1 |

If Characters can't reference Camera internals, you physically can't create the wrong coupling.

**Aggregates.** Domain objects with clear ownership boundaries:

| Aggregate Root | Owns |
|---------------|------|
| `ShotSetup` | `CameraAnimation`, `ShotObjectManager`, per-element `ObjectAnimation` instances |
| `Character` | Pose state, expression state, skeleton mapping, customization |
| `Scene` | Elements, shots, lighting, timelines |
| `Project` | Scenes, character definitions, settings |

**Value objects.** Immutable types that carry meaning without identity:

- `ShotId`, `KeyframeId` — GUID-based identity
- `CameraKeyframeState`, `ObjectState` — state snapshots
- `FocalLength`, `AspectRatio`, `SensorDimensions` — typed domain values
- `Pose`, `Expression` — character state snapshots

**Domain events.** Cross-context communication without coupling:

- Shot-level: `DurationChanged`, `KeyframeAdded`, `KeyframeRemoved`, `KeyframeMoved`
- Selection: `ElementSelected`, `SelectionCleared`
- Scene-level: events for element add/remove, lighting changes

**Command pattern.** `ICommand` with `Execute()` / `Undo()` / `Redo()` for all user actions. Enables undo stack and action replay.

### 8.2 What We Skip

**Repositories.** Unity manages object lifecycles through `MonoBehaviour`, `Instantiate`, `Destroy`, and scene serialization. Wrapping that in repositories duplicates what the engine already does.

**Application services / use case classes.** The `ApplicationController.Update()` frame loop is inherently imperative. Routing every frame tick through application service abstractions adds indirection in a hot path.

**Full layered architecture** (domain → application → infrastructure → presentation). Unity blurs these layers by design. `MonoBehaviour` is simultaneously presentation, infrastructure, and sometimes domain logic.

### 8.3 The Split Model

The key architectural pattern: **pure C# for domain logic, thin MonoBehaviour wrappers for scene graph integration.**

- **Pure C# domain layer**: `ShotSetup`, `CameraAnimation`, `KeyframeManager<T>`, `Character`, `Pose`, value objects. No Unity dependencies. Testable with standard xUnit without Unity's test runner.
- **MonoBehaviour integration layer**: `SceneElement`, `VirtualCameraRig`, `ApplicationController`. Thin wrappers that delegate to the domain layer. These need Unity to run but contain minimal logic.

This gives testability without the ceremony of formal DDD layering.
