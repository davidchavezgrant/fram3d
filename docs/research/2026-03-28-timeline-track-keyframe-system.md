# Research: Timeline, Track, and Keyframe System — Current State

**Date**: 2026-03-28
**Purpose**: Document the complete current state of the timeline, track, keyframe, shot, camera, and element systems across Core, Engine, and UI layers.

---

## Summary

The timeline system is fully implemented across all three layers: pure C# domain types in `Core/Timelines/` and `Core/Shots/`, a thin Engine bridge in `ShotEvaluator`, and a complete UI Toolkit presentation layer in `UI/Timeline/`. The architecture follows the split model — `Timeline` (Core) owns all interaction state machines (scrub, boundary drag, shot reorder, playback), while UI views are thin: they forward pointer events in and read state out. Camera animation is per-shot (local time); element animation is per-element on the global timeline. Both use the same generic `KeyframeManager<T>` with linear interpolation. There are ~2,900 lines of xUnit tests and ~1,360 lines of Play Mode tests covering timeline and shot types.

---

## 1. Core Domain Types

### 1.1 Value Objects (`Core/Common/`)

| Type | File | Purpose |
|------|------|---------|
| `TimePosition` | `Common/TimePosition.cs` | Non-negative time in seconds. `IEquatable`, `IComparable`, equality operators. Has `Add()`, `Subtract()`, `ToFrame(FrameRate)`. Static `ZERO`. Epsilon comparison (1e-9). |
| `FrameRate` | `Common/FrameRate.cs` | Positive FPS. Static presets: `FPS_24`, `FPS_25`, `FPS_29_97`, `FPS_30`, `FPS_48`, `FPS_59_94`, `FPS_60`. `FrameDuration` property, `SnapToFrame(TimePosition)` method. |
| `KeyframeId` | `Common/KeyframeId.cs` | GUID wrapper rejecting `Guid.Empty`. `IEquatable<KeyframeId>`. Same pattern as `ElementId` and `ShotId`. |
| `ShotId` | `Common/ShotId.cs` | GUID wrapper rejecting `Guid.Empty`. `IEquatable<ShotId>`. |

### 1.2 Event System (`Core/Common/`)

| Type | File | Purpose |
|------|------|---------|
| `Subject<T>` | `Common/Subject.cs` | Minimal `IObservable<T>` implementation. Synchronous delivery. List of observers, `OnNext(T)`. Unsubscriber via `IDisposable`. |
| `ObservableExtensions` | `Common/ObservableExtensions.cs` | `.Subscribe(Action<T>)` convenience extension wrapping an `ActionObserver<T>`. |

### 1.3 Keyframe Types (`Core/Timelines/`)

**`Keyframe<T>`** (`Timelines/Keyframe.cs`)
- Immutable. Stores `Id` (KeyframeId), `Time` (TimePosition), `Value` (T).
- `IComparable<Keyframe<T>>` — compares by Time.
- `WithTime(TimePosition)` / `WithValue(T)` — create copies with same ID.

**`KeyframeManager<T>`** (`Timelines/KeyframeManager.cs`)
- Dual storage: `Dictionary<KeyframeId, Keyframe<T>>` for O(1) ID lookup + sorted `List<Keyframe<T>>` for iteration.
- API: `Add()`, `Update()`, `SetOrMerge()`, `RemoveById()`, `Clear()`, `GetById()`, `GetInRange()`.
- `Evaluate(TimePosition, Func<T,T,float,T> lerp)` — linear interpolation between surrounding keyframes. Clamps beyond range. Returns `default(T)` if empty.
- `SetOrMerge` handles both update-by-ID and merge-at-same-time (replaces existing keyframe at same time).

### 1.4 Track Types (`Core/Timelines/`)

**`ElementTrack`** (`Timelines/ElementTrack.cs`)
- Per-element track on the global timeline. Identified by `ElementId`.
- Two `KeyframeManager` instances: `PositionKeyframes` (`Vector3`) and `RotationKeyframes` (`Quaternion`).
- `EvaluatePosition(TimePosition globalTime)` — uses `Vector3.Lerp`.
- `EvaluateRotation(TimePosition globalTime)` — uses `Quaternion.Slerp`.
- `HasKeyframes` / `KeyframeCount` computed properties.

**`ElementTimeline`** (`Timelines/ElementTimeline.cs`)
- Registry of `ElementTrack` keyed by `ElementId`.
- `GetOrCreateTrack(ElementId)`, `GetTrack(ElementId)`, `HasTrack()`, `RemoveTrack()`.
- `TrackCount` / `Tracks` (IReadOnlyCollection).

**`ShotTrack`** (`Timelines/ShotTrack.cs`)
- Ordered `List<Shot>` with CRUD operations.
- `AddShot(Vector3 cameraPosition, Quaternion cameraRotation)` — auto-names `Shot_01`, `Shot_02`, etc.
- `RemoveShot`, `Reorder`, `SetCurrentShot`, `GetById`, `IndexOf`.
- Global time lookups: `GetGlobalStartTime(ShotId)`, `GetGlobalEndTime(ShotId)`, `GetShotAtGlobalTime(TimePosition)` — returns `(Shot, TimePosition localTime)?`.
- Track operations: `FindEdgeAtTime(double time, double tolerance)`, `FindInsertionIndex(double time)`, `ResizeShotAtEdge(int index, double newEndTime)`.
- Formatting: `FormatShotTooltip(Shot)`, `FormatResizeTooltip(int, bool)`.
- Observables: `ShotAdded`, `ShotRemoved`, `CurrentShotChanged`, `Reordered`.
- `TotalDuration` computed by summing all shot durations.

### 1.5 Timeline Orchestrator (`Core/Timelines/Timeline.cs`)

The `Timeline` class is the central orchestrator. It owns:
- `ShotTrack` (private, exposed via delegated properties)
- `Playhead` (public)
- `ElementTimeline` (public, as `Elements`)
- `ViewRange` (internal, for zoom/pan/pixel conversion)

**Interaction state machines** — all pointer logic lives here, not in UI:
- Scrub: `BeginScrub()`, `ScrubToPixel(double px)`, `EndScrub()`
- Shot track clicks/drags: `ShotTrackPointerDown()`, `ShotTrackPointerMove()`, `ShotTrackPointerUp()` — returns `ShotTrackAction` sealed class instances.
- Boundary drag: `BeginBoundaryDrag(int edgeIndex)`, driven by pointer state machine.
- Shot drag reorder: hold 200ms or move 5px to initiate. `DragTargetIndex` exposed for drop indicator.
- Double-click: fits view to shot. 350ms threshold.

**Playback**: `TogglePlayback()`, `Advance(double deltaSeconds)` — advances playhead and fires `CameraEvaluationRequested` / `ElementEvaluationRequested` observables. Auto-scrolls view when playhead exits visible range.

**Observables**:
- `CameraEvaluationRequested` — emits `CameraEvaluation(Shot, TimePosition localTime)` on scrub/playback.
- `ElementEvaluationRequested` — emits `ElementEvaluation(TimePosition globalTime)`.
- Plus delegated: `ShotAdded`, `ShotRemoved`, `CurrentShotChanged`, `Reordered`, `ViewChanged`.

**View range**: `PixelsPerSecond`, `ViewStart`/`ViewEnd`/`VisibleDuration`, `TimeToPixel()`/`PixelToTime()`, `ZoomAtPoint()`, `Pan()`, `FitAll()`, `FitRange()`, `FitToShot()`, `EnsureVisible()`.

### 1.6 Evaluation Messages (`Core/Timelines/`)

| Type | Fields | Purpose |
|------|--------|---------|
| `CameraEvaluation` | `Shot`, `LocalTime` (TimePosition) | Emitted when camera should evaluate at shot-local time |
| `ElementEvaluation` | `GlobalTime` (TimePosition) | Emitted when elements should evaluate at absolute global time |

### 1.7 Supporting Types

**`ShotTrackAction`** (`Timelines/ShotTrackAction.cs`) — sealed class enum for pointer event results: `NONE`, `POTENTIAL_CLICK`, `CLICK`, `DRAG_START`, `DRAG_MOVE`, `DRAG_COMPLETE`, `BOUNDARY_DRAG`, `BOUNDARY_COMPLETE`, `NEAR_EDGE`.

**`ViewRange`** (`Timelines/ViewRange.cs`) — internal. Manages visible time window. `Initialize(trackWidth, totalDuration)`, zoom/pan with clamping, pixel<->time conversion. Min visible duration 0.5s, zoom factor 1.15.

### 1.8 Shot Model (`Core/Shots/`)

**`Shot`** (`Shots/Shot.cs`) — aggregate root.
- Constructor: `Shot(ShotId, string name, Vector3 cameraPosition, Quaternion cameraRotation)`.
- Creates mandatory initial keyframes at t=0 for both position and rotation.
- Duration: `[0.1s, 300s]`, default 5.0s. Shortening below existing keyframes does NOT delete them.
- Name: non-empty, max 32 chars. Setting empty is a no-op.
- Camera keyframes: `CameraPositionKeyframes` (`KeyframeManager<Vector3>`), `CameraRotationKeyframes` (`KeyframeManager<Quaternion>`).
- `EvaluateCameraPosition(TimePosition localTime)`, `EvaluateCameraRotation(TimePosition localTime)`.
- `TotalCameraKeyframeCount` — position + rotation counts.

**`ShotRegistry`** (`Shots/ShotRegistry.cs`) — standalone registry with the same operations as `ShotTrack` but with stricter validation (throws on invalid reorder indices). Note: `ShotTrack` appears to be the version actually used by `Timeline`; `ShotRegistry` may be an earlier or parallel implementation with slightly different behavior.

---

## 2. Camera State Model (`Core/Cameras/`)

**`CameraElement`** (`Cameras/CameraElement.cs`) — extends `Element`.
- **Position/Rotation**: inherited from `Element`. `System.Numerics.Vector3`/`Quaternion`. Position Y-clamped to `GroundOffset` (min 0.1f for cameras). Default position `(0, 1.6, 5)`.
- **Lens**: `FocalLength` (14-400mm via `LensController`), `Aperture` (f/1.4-f/22 discrete stops), `FocusDistance` (0.1-100m), `SnapFocalLength` flag. Prime lenses snap to discrete values; zoom lenses allow continuous.
- **DOF**: `DofEnabled` bool, computed `FocusAtInfinity`.
- **FOV**: `HorizontalFov` / `VerticalFov` computed from sensor dimensions + focal length (radians).
- **Sensor**: `SensorWidth`/`SensorHeight` via `BodyController`. Camera body + sensor mode.
- **Shake**: `ShakeEnabled`, `ShakeAmplitude` (default 0.1), `ShakeFrequency` (default 1.0). Pan/tilt noise applied in Engine layer.
- **Movement methods**: `Pan()`, `Tilt()`, `Roll()`, `Dolly()`, `Truck()`, `Crane()`, `Orbit()`, `DollyZoom()`. All operate on `this.Position`/`this.Rotation`.
- **Equipment**: `SetBody()`, `SetLensSet()`, `SetSensorMode()`, step/cycle methods for focal length and aperture.
- Internal delegation: `BodyController` (sensor, aspect ratio, sensor mode), `LensController` (focal length, aperture, focus, lens set).

**`Element`** (`Common/Element.cs`) — base class.
- `Id` (ElementId), `Name`, `Position` (Vector3, Y-clamped), `Rotation` (Quaternion), `Scale` (float, default 1), `BoundingRadius`, `GroundOffset`.

---

## 3. Element/Scene Model (`Core/Scenes/`)

**`Selection`** (`Scenes/Selection.cs`)
- Tracks `HoveredId` and `SelectedId` (both `ElementId`).
- `Hover(id)` — no hover on selected element. `ClearHover()`, `Select(id)`, `Deselect()`.

**Other Scene types** (not keyframe-related): `ActiveTool` (SELECT/TRANSLATE/ROTATE/SCALE sealed class), `GizmoState`, `GizmoAxis`, `DragSession`, `ClickDetector`, `ClickResult`, `ViewMode` (CAMERA/DIRECTOR).

No `ElementRegistry` exists yet. Selection is single-element only (`ElementId`, not a collection).

---

## 4. Engine Integration Layer

**`ShotEvaluator`** (`Engine/Integration/ShotEvaluator.cs`) — the bridge between Core and Unity.
- `Awake()`: creates `Timeline(FrameRate.FPS_24)`.
- `Start()`: finds `CameraBehaviour`, subscribes to `CameraEvaluationRequested`, `ElementEvaluationRequested`, `CurrentShotChanged`. Adds initial shot from current camera position/rotation.
- `OnCameraEvaluationRequested`: evaluates shot camera position/rotation at local time, writes to `CameraBehaviour.ShotCamera`.
- `OnElementEvaluationRequested`: finds all `ElementBehaviour` instances, evaluates each element's track at global time, writes position/rotation.
- `OnCurrentShotChanged`: evaluates camera at t=0 of new shot.
- Exposes `Controller` (Timeline), `BottomInsetPixels` for viewport rect calculation.

**`CameraBehaviour`** (`Engine/Integration/CameraBehaviour.cs`) — MonoBehaviour on the Unity Camera.
- `Awake()`: creates two `CameraElement` instances — `_cameraElement` (shot camera) and `_directorCamera` (free utility). Loads `CameraDatabase`, sets up physical camera properties, DOF volume, frustum wireframe.
- `LateUpdate()` → `Sync()`: reads `ActiveCamera` (shot or director based on `ViewMode`), applies position/rotation to `transform`, syncs focal length (with lerp), sensor size, viewport rect (with right/bottom insets), DOF, and shake.
- `ShotCamera` / `DirectorCamera` / `ActiveCamera` properties expose the Core elements.
- Viewport rect calculated from screen dimensions minus panel insets.

---

## 5. UI Timeline Types (`UI/Timeline/`)

### 5.1 TimelineSectionView (`UI/Timeline/TimelineSectionView.cs`)
- MonoBehaviour, top-level view. Gets `ShotEvaluator.Controller` (Timeline) reference.
- **BuildLayout**: creates transport bar, ruler, shot track strip, track content area, zoom bar, tooltips.
- **Update()**: advances playback via `controller.Advance(deltaTime)`, handles input system scroll, calls `SyncVisuals()`.
- **SyncVisuals**: reads all state from `Timeline` and positions UI elements (playhead, out-of-range, blocks). Zero domain logic.
- Subscribes to `ShotAdded`, `ShotRemoved`, `Reordered`, `CurrentShotChanged` for rebuilds.
- Public API (called by `KeyboardShortcutRouter`): `Toggle()`, `TogglePlayback()`, `JumpToStart()`, `JumpToEnd()`, `FitAll()`, `ZoomIn()`, `ZoomOut()`.
- `IsPointerOverUI` — screen-to-panel hit test for input blocking.

### 5.2 ShotTrackStrip (`UI/Timeline/ShotTrackStrip.cs`)
- VisualElement. Renders shot blocks, playhead, out-of-range zone, drop indicator, boundary handles.
- `Bind(Timeline)` — registers pointer down/move/up callbacks.
- Pointer events → `controller.ShotTrackPointerDown/Move/Up()` — **all state machine logic lives in Core**.
- `RebuildBlocks()` — clears and recreates `ShotBlock` instances with context menus (Delete Shot), hover events, duration edit callbacks.
- `RebuildBoundaryHandles()` — one handle per shot boundary, with separate pointer callbacks for boundary drag.
- `TrackLocalX()` — Input System screen → panel → element coordinate conversion.
- Cursor management: `CursorService.SetCursor(CursorType.ResizeHorizontal)` for edge hover/drag.
- Events out: `AddShotRequested`, `BoundaryDragStarted/Ended`, `ShotHoverStarted/Ended`, `TrackAreaResized`.

### 5.3 ShotBlock (`UI/Timeline/ShotBlock.cs`)
- VisualElement per shot. Displays name label, duration label.
- Color from `ShotColorPalette.GetColor(index)` — 8 cycling muted tones.
- Active state via CSS class `shot-block--active` (full opacity + white border).
- Duration click opens inline `TextField` for timecode editing. Parses flexible timecode input (frames, `s;ff`, `m;ss;ff`).
- `Refresh()` updates name/duration text.

### 5.4 Ruler (`UI/Timeline/Ruler.cs`)
- Time ruler with adaptive tick marks. Tick interval scales with visible duration (frame-level at <2s, up to 10s intervals).
- Frame dividers shown when pixel density >= 4px/frame.
- Click/drag on ruler fires `ScrubRequested` event.
- Updates playhead position, out-of-range overlay, tick marks + labels.

### 5.5 TransportBar (`UI/Timeline/TransportBar.cs`)
- Play/stop button (unicode triangle/square), timecode display (HH;MM;SS;FF format), shot name label.
- `UpdateTransport(Playhead, Timeline)` — shows shot-local timecode and duration.

### 5.6 ZoomBar (`UI/Timeline/ZoomBar.cs`)
- Mini-map bar with draggable thumb representing visible range.
- Thumb size proportional to visible/total duration. Min width 30px.
- Drag fires `PanRequested` with pixel delta.

### 5.7 ShotColorPalette (`UI/Timeline/ShotColorPalette.cs`)
- Static 8-color palette cycling by index.

---

## 6. USS Stylesheet

Single stylesheet: `Unity/Fram3d/Assets/Resources/fram3d.uss` (915 lines).

Timeline-specific CSS classes:
- `.timeline-section` — absolute positioned bottom bar, dark background (rgb 40,40,40)
- `.timeline-transport`, `.timeline-transport__play`, `.timeline-transport__time`, `.timeline-transport__shot` — transport bar
- `.timeline-ruler-row`, `.timeline-ruler`, `.timeline-ruler__tick`, `.timeline-ruler__label`, `.timeline-ruler__frame-tick` — ruler
- `.timeline-shot-row` (60px height), `.timeline-shot-strip` — shot track container
- `.shot-block`, `.shot-block:hover` (opacity 0.55 → 0.75), `.shot-block--active` (opacity 1.0 + white border)
- `.shot-block__name`, `.shot-block__duration`, `.shot-block__duration:hover`, `.shot-block__duration-edit` — block internals
- `.timeline-playhead` (2px wide, rgb 255,68,102 red), `.timeline-playhead__head` — playhead with inverted-triangle head
- `.timeline-out-of-range` — black 35% alpha overlay past total duration
- `.timeline-zoom-row`, `.timeline-zoom-bar`, `.timeline-zoom-thumb`, `.timeline-zoom-playhead`
- `.timeline-label-column` (140px fixed width), `.timeline-label-column__title`, `.timeline-label-column__subtitle`
- `.shot-track__drop-indicator` (blue line), `.shot-track__boundary` (24px hit area, centered on edge)
- `.shot-tooltip`, `.boundary-tooltip` — floating tooltips
- `.shot-label-overlay`, `.sequence-timecode-container`, `.sequence-timecode-label` — Camera View overlays

Design tokens defined as CSS custom properties on `:root`.

---

## 7. Tests

### 7.1 xUnit Core Tests (`tests/Fram3d.Core.Tests/`)

| File | Lines | Coverage |
|------|-------|----------|
| `Timelines/KeyframeTests.cs` | 96 | Constructor validation, CompareTo, WithTime, WithValue |
| `Timelines/KeyframeManagerTests.cs` | 344 | Add/Update/SetOrMerge/RemoveById/Clear/Evaluate/GetById/GetInRange. Lerp evaluation, clamping, time-collision rules |
| `Timelines/ElementTrackTests.cs` | 144 | Position/rotation evaluation with lerp/slerp, HasKeyframes, KeyframeCount |
| `Timelines/ElementTimelineTests.cs` | 147 | GetOrCreateTrack, GetTrack, HasTrack, RemoveTrack, TrackCount |
| `Timelines/PlayheadTests.cs` | 205 | Scrub (frame snapping, clamping), Advance, TogglePlayback, Reset, TimeChanged observable |
| `Timelines/TimelineTests.cs` | 1087 | Shot lifecycle, reorder, scrub, playback, view range (zoom/pan/fit), pointer state machines, boundary drag, drop indicator, evaluation events, tooltip formatting |
| `Shots/ShotTests.cs` | 264 | Constructor, name validation, duration clamping, initial keyframes, camera evaluation |
| `Shots/ShotRegistryTests.cs` | 597 | AddShot, RemoveShot, Reorder, CurrentShot tracking, global time computations, observables |

**Total xUnit timeline/shot tests: ~2,884 lines**

### 7.2 Play Mode Tests (`Unity/Fram3d/Assets/Tests/PlayMode/`)

| File | Lines | Coverage |
|------|-------|----------|
| `Engine/ShotEvaluatorTests.cs` | 173 | Awake/Start lifecycle, timeline creation, initial shot seeding, camera evaluation routing, element evaluation, subscription cleanup |
| `UI/ShotTrackStripTests.cs` | 342 | Block rebuilding, boundary handles, pointer forwarding, drop indicator |
| `UI/ShotBlockTests.cs` | 202 | Color, active state, name/duration display, timecode parsing, duration edit field |
| `UI/RulerTests.cs` | 178 | Tick generation, playhead positioning, scrub callbacks |
| `UI/TransportBarTests.cs` | 158 | Timecode display, play button state, shot name |
| `UI/ZoomBarTests.cs` | 77 | Thumb positioning, drag callbacks |
| `UI/TimelineSectionViewTests.cs` | 180 | Layout building, visibility toggle, controller wiring |
| `UI/ShotColorPaletteTests.cs` | 51 | Color cycling, index wrapping |

**Total Play Mode timeline tests: ~1,361 lines**

---

## 8. Data Flow Summary

### Adding a Shot
```
UI: OnAddShot() [TimelineSectionView]
  → reads CameraBehaviour.ShotCamera.Position / .Rotation
  → Timeline.AddShot(position, rotation)
    → ShotTrack.AddShot() — creates Shot with initial keyframes at t=0
      → fires ShotAdded observable
  → UI subscribes: ShotTrackStrip.RebuildBlocks()
```

### Scrubbing
```
UI: Ruler click → ScrubRequested event → TimelineSectionView.OnScrub(px)
  → Timeline.BeginScrub()
  → Timeline.ScrubToPixel(px)
    → ViewRange.PixelToTime(px)
    → Playhead.Scrub(rawTime, totalDuration) — snaps to frame boundary
    → Timeline.EvaluateCamera()
      → ShotTrack.GetShotAtGlobalTime(currentTime) → (Shot, localTime)
      → fires CameraEvaluationRequested(CameraEvaluation)
      → fires ElementEvaluationRequested(ElementEvaluation)
  Engine: ShotEvaluator.OnCameraEvaluationRequested()
    → shot.EvaluateCameraPosition(localTime) → Vector3
    → shot.EvaluateCameraRotation(localTime) → Quaternion
    → writes to CameraBehaviour.ShotCamera.Position / .Rotation
  Engine: CameraBehaviour.LateUpdate() → Sync()
    → reads ActiveCamera.Position/Rotation → transform
```

### Shot Track Interaction (clicks, drag reorder, boundary resize)
```
UI: ShotTrackStrip pointer callbacks → Timeline.ShotTrackPointerDown/Move/Up(px, timestampMs)
  Core state machine resolves: POTENTIAL_CLICK → CLICK / DRAG_START → DRAG_MOVE → DRAG_COMPLETE / BOUNDARY_DRAG → BOUNDARY_COMPLETE
  UI reads Timeline.IsDragging / IsBoundaryDragging / DragTargetIndex for visual updates
```

### Playback
```
UI: TimelineSectionView.Update()
  → if Playhead.IsPlaying: Timeline.Advance(deltaTime)
    → Playhead.Advance() — advances time, clamps at end
    → Timeline.EvaluateCamera() — same path as scrubbing
    → auto-scrolls ViewRange if playhead exits visible area
```

---

## Key References

- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs` — central orchestrator (430 lines)
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeManager.cs` — generic keyframe collection with dual storage and interpolation
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/ShotTrack.cs` — ordered shot collection with global time math
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/Playhead.cs` — playback + scrub + frame snapping
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs` — per-element position/rotation tracks
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTimeline.cs` — registry of element tracks
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Timelines/ViewRange.cs` — zoom/pan/pixel geometry (internal)
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs` — shot aggregate with camera keyframes
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Shots/ShotRegistry.cs` — standalone shot registry (may be unused)
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Cameras/CameraElement.cs` — full camera state model
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Common/Element.cs` — base element (position/rotation/scale)
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotEvaluator.cs` — Core-to-Unity bridge
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs` — Unity camera sync
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs` — top-level timeline MonoBehaviour
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotTrackStrip.cs` — shot block rendering + pointer forwarding
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotBlock.cs` — individual shot block with inline editing
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Timeline/Ruler.cs` — time ruler with adaptive ticks
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Timeline/TransportBar.cs` — play/timecode/shot name
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Timeline/ZoomBar.cs` — mini-map zoom thumb
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Resources/fram3d.uss` — all USS styles (915 lines)

---

## Open Questions

1. **`ShotRegistry` vs `ShotTrack` duplication** — `Core/Shots/ShotRegistry.cs` and `Core/Timelines/ShotTrack.cs` have overlapping responsibilities (both manage ordered shot lists with add/remove/reorder/current tracking/global time lookup). `Timeline` uses `ShotTrack`; it's unclear if `ShotRegistry` is actively used elsewhere or is a leftover from an earlier iteration.

2. **No focal length keyframes** — `Shot` only stores position and rotation keyframes. Focal length, aperture, focus distance, DOF enabled, and shake parameters are not keyframeable yet. The `CameraElement` stores these as mutable state but they aren't captured in the shot model.

3. **No camera keyframe recording** — there is no recording/stopwatch mechanism that automatically creates keyframes when the user manipulates the camera at a non-zero playhead position. Camera keyframes can only be added programmatically through the `KeyframeManager` API.

4. **Element evaluation finds all ElementBehaviours each frame** — `ShotEvaluator.OnElementEvaluationRequested()` calls `FindObjectsByType<ElementBehaviour>()` on every evaluation. No cached registry of scene elements exists yet.

5. **Interpolation is linear only** — `KeyframeManager.Evaluate()` takes a lerp function. All current usage passes `Vector3.Lerp` and `Quaternion.Slerp`. No ease curves, bezier, or other interpolation modes are implemented.

*Last updated: 2026-03-28*
