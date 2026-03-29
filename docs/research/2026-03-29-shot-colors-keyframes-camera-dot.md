# Research: Shot Colors, Camera Track Yellow Dot, Keyframe System, and Shot Model

**Date**: 2026-03-29
**Purpose**: Document how shot colors are assigned, where the camera track yellow dot is rendered, how the keyframe system works end-to-end, and how shots are modeled.

---

## Summary

Shot colors are assigned by **position index** in the shot list — not stored on the Shot model itself. An 8-color palette in `ShotColorPalette` cycles via `index % 8`. The "yellow dot" on the camera track is a CSS-styled `track-type-dot--camera` element rendered in the `TrackRow` header (rgb 220, 200, 60). Keyframes are managed by generic `KeyframeManager<T>` instances owned by `Shot` (5 camera properties) and `ElementTrack` (3 element properties). Recording happens through `KeyframeRecorder` (stopwatch-based auto-recording) and `ForceRecordCamera`/`ForceRecordElement` (manual C/V key). Shots have `ShotId`, `Name`, `Duration`, and camera keyframe managers — but no color property.

---

## 1. Shot Colors

### How colors are assigned

Colors are **not stored on the Shot model**. The `Shot` class has only `ShotId`, `Name`, `Duration`, `DefaultCameraPosition`, `DefaultCameraRotation`, `CameraStopwatch`, and five `KeyframeManager<T>` instances. There is no color field.

Instead, color is determined at render time by the shot's **positional index** in the ordered list:

```
ShotColorPalette.GetColor(index) => COLORS[index % 8]
```

### Palette definition

`/Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotColorPalette.cs` — static 8-color array:

| Index | Name          | RGBA                        |
|-------|---------------|-----------------------------|
| 0     | steel blue    | (0.30, 0.50, 0.70, 1)      |
| 1     | muted purple  | (0.55, 0.35, 0.60, 1)      |
| 2     | sage green    | (0.35, 0.58, 0.45, 1)      |
| 3     | burnt orange  | (0.70, 0.45, 0.30, 1)      |
| 4     | olive         | (0.50, 0.55, 0.35, 1)      |
| 5     | dusty rose    | (0.60, 0.35, 0.40, 1)      |
| 6     | teal          | (0.35, 0.55, 0.60, 1)      |
| 7     | khaki         | (0.55, 0.50, 0.35, 1)      |

### Where colors are consumed

1. **Shot blocks** (`ShotBlock` constructor, line 22): `ShotColorPalette.GetColor(colorIndex)` sets `style.backgroundColor` on the block element. The `colorIndex` is the shot's position in `ShotTrack.Shots`.

2. **Camera track keyframe diamonds** (`TimelineSectionView.SyncTrackVisuals()`, line 788): `ShotColorPalette.GetColor(shotIndex)` is passed as `keyframeColor` to `TrackRow.UpdateMainDiamonds()`. Camera keyframe dots match the active shot's palette color.

3. **Track shot segments** (`TimelineSectionView.BuildShotSegments()`, line 849): Each shot's background strip on the track area uses `ShotColorPalette.GetColor(i)`. Active shot gets alpha 0.18; inactive gets alpha 0.08.

4. **Element track keyframe diamonds** (`TimelineSectionView.SyncTrackVisuals()`, line 822): Hard-coded green `new Color(0.31f, 0.78f, 0.31f)` — does NOT use the palette.

### Implication

Reordering shots changes their colors. If Shot_01 (steel blue) and Shot_02 (muted purple) are swapped, Shot_01 becomes purple and Shot_02 becomes blue.

---

## 2. Camera Track Yellow Dot

### What it is

The "yellow dot" is a **track type indicator** — a small colored circle in the track header that identifies the track type (camera vs element). It is NOT a keyframe.

### Where it's rendered

`/Unity/Fram3d/Assets/Scripts/UI/Timeline/TrackRow.cs:68-71`:

```csharp
var typeDot = new VisualElement();
typeDot.AddToClassList("track-type-dot");
typeDot.AddToClassList(isCamera ? "track-type-dot--camera" : "track-type-dot--element");
labels.Add(typeDot);
```

### How the color is set

Pure CSS in `/Unity/Fram3d/Assets/Resources/fram3d.uss:1024-1038`:

```css
.track-type-dot {
    width: 8px;
    height: 8px;
    border-radius: 4px;   /* makes it circular */
    margin-right: 4px;
    flex-shrink: 0;
}

.track-type-dot--camera {
    background-color: rgb(220, 200, 60);   /* yellow */
}

.track-type-dot--element {
    background-color: rgb(80, 200, 80);    /* green */
}
```

### Header layout order

The track header contains (left to right): collapse arrow, stopwatch circle, type dot, name label. Then the track content area (where keyframe diamonds live).

---

## 3. Keyframe System

### Core model

**`Keyframe<T>`** (`Core/Timelines/Keyframe.cs`) — Immutable generic keyframe. Stores `Id` (KeyframeId), `Time` (TimePosition), `Value` (T). Has `WithTime()` and `WithValue()` for creating copies with same ID but different time/value.

**`KeyframeId`** (`Core/Common/KeyframeId.cs`) — Guid wrapper rejecting `Guid.Empty`. Typed identity.

**`KeyframeManager<T>`** (`Core/Timelines/KeyframeManager.cs`) — Dual-storage collection (sorted list + dictionary by ID). Full mutation API:
- `Add(Keyframe<T>)` — new keyframe, throws if ID exists or time occupied
- `Update(Keyframe<T>)` — replace by ID at new time
- `SetOrMerge(Keyframe<T>)` — removes same ID and same time, then inserts (drag-merge)
- `RemoveById(KeyframeId)` — delete
- `Clear()` — remove all
- `Evaluate(TimePosition, Func<T,T,float,T>)` — linear interpolation between surrounding keyframes
- `GetAtTime(TimePosition)` — find keyframe at exact time
- `GetInRange(TimePosition, TimePosition)` — range query

### Shot owns 5 camera keyframe managers

`/Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs`:
- `CameraPositionKeyframes` — `KeyframeManager<Vector3>`
- `CameraRotationKeyframes` — `KeyframeManager<Quaternion>`
- `CameraFocalLengthKeyframes` — `KeyframeManager<float>`
- `CameraApertureKeyframes` — `KeyframeManager<float>`
- `CameraFocusDistanceKeyframes` — `KeyframeManager<float>`

Shot also has `DefaultCameraPosition` and `DefaultCameraRotation` — used when no keyframes exist for position/rotation respectively.

### ElementTrack owns 3 element keyframe managers

`/Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs`:
- `PositionKeyframes` — `KeyframeManager<Vector3>`
- `RotationKeyframes` — `KeyframeManager<Quaternion>`
- `ScaleKeyframes` — `KeyframeManager<float>`

### Recording mechanisms

**Stopwatch-based auto-recording** (`StopwatchState` + `KeyframeRecorder`):
- Each Shot has a `CameraStopwatch` (5 properties: position, rotation, focal length, focus distance, aperture).
- Each ElementTrack has its own `Stopwatch` (3 properties: position, rotation, scale).
- `StopwatchState` is a bool array indexed by property index. `AnyRecording`/`AllRecording` computed properties.
- `KeyframeRecorder.RecordCamera()` compares `CameraSnapshot` current vs previous, checks per-property stopwatch state, checks change thresholds, and calls `RecordToManager()` which either merges into nearby keyframes (within `MERGE_WINDOW`) or creates new ones.

**Manual force-record** (C and V keys):
- `C key` → `KeyboardShortcutRouter.HandleKeyframeShortcuts()` → `CameraSnapshot.FromCamera(camera)` → `Timeline.ForceRecordCamera(snap)` → `KeyframeRecorder.ForceRecordCamera()` — records ALL 5 camera properties at current playhead time, regardless of stopwatch state.
- `V key` → `KeyboardShortcutRouter.HandleKeyframeShortcuts()` → `GizmoController.GetSelectedElementSnapshot()` → `Timeline.AddElementKeyframeAtPlayhead(id, snapshot)` → `Timeline.ForceRecordElement()` → `KeyframeRecorder.ForceRecordElement()` — records all 3 element properties.

**Stopwatch UI** (toggle button on track header):
- `TrackRow` renders a stopwatch circle in the header (`track-stopwatch` CSS class).
- Click fires `StopwatchClicked` event.
- `TimelineSectionView.HandleCameraStopwatchClick()` toggles `shot.CameraStopwatch.SetAll(true/false)`.
- When turning off with existing keyframes, shows a confirm dialog (clear all keyframes?).
- When turning on, immediately force-records a keyframe at current playhead time.
- Visual states: off (default grey), partial (`track-stopwatch--partial`, dark red), all on (`track-stopwatch--on`, bright red).

### No dedicated "Add Keyframe" button

There is no standalone "add keyframe" or "record" button in the timeline UI. Keyframes are created through:
1. C key (camera) / V key (element) — manual force-record
2. Stopwatch toggle — records initial keyframe when turned on
3. Camera/element manipulation while stopwatch is active — auto-recording via `KeyframeRecorder`

### Keyframe selection and interaction

`KeyframeSelection` (`Core/Timelines/KeyframeSelection.cs`) — single-selection model. Stores one `KeyframeId` + `TrackId` + `TimePosition` at a time.

`KeyframeDiamond` (`UI/Timeline/KeyframeDiamond.cs`) — VisualElement with outer 22px hit area and inner 10px visible dot. Color set programmatically via `SetColor(Color)`. Selected state via CSS class `keyframe-diamond--selected` (white with cyan border).

Click flow: PointerDown on `TrackRow._content` → `FindDiamondIndex()` → `DiamondClicked` event → `TimelineSectionView.OnDiamondClicked()` → `Timeline.SelectKeyframe()` → scrubs playhead to keyframe time.

Drag flow: PointerDown captures, PointerMove after 4px threshold fires `DiamondDragging`, PointerUp fires `DiamondDropped`.

---

## 4. Shot Model

### Identity and properties

**`ShotId`** (`Core/Common/ShotId.cs`) — sealed class wrapping `Guid`, rejects `Guid.Empty`, implements `IEquatable<ShotId>` with equality operators.

**`Shot`** (`Core/Shots/Shot.cs`) — sealed class, aggregate root.

Properties:
- `Id` (ShotId) — immutable, set at construction
- `Name` (string) — non-empty, max 32 chars, setting empty is a no-op
- `Duration` (double) — clamped [0.1s, 300s], default 5.0s
- `DefaultCameraPosition` (Vector3) — fallback when no position keyframes exist
- `DefaultCameraRotation` (Quaternion) — fallback when no rotation keyframes exist
- `CameraStopwatch` (StopwatchState) — per-property recording state (5 slots)
- 5 `KeyframeManager<T>` instances (position, rotation, focal length, aperture, focus distance)
- `TotalCameraKeyframeCount` — computed sum across all 5 managers

No color property. No index property.

### Shot lifecycle

**Creation**: `ShotTrack.AddShot()` → auto-names `Shot_01`, `Shot_02`, etc. → creates `Shot(new ShotId(Guid.NewGuid()), name)` → no initial keyframes (constructor just creates empty KeyframeManagers) → appends to ordered list → sets as current shot → fires `ShotAdded` observable.

**Initial seeding**: `ShotEvaluator.Start()` calls `Timeline.AddShot()` (through `ShotTrack`) to create the first shot. When the stopwatch is activated (or C key pressed), the current camera state is captured and recorded as keyframes.

**Selection**: `ShotTrack.SetCurrentShot(ShotId)` → fires `CurrentShotChanged`. The "current shot" determines which camera keyframes are shown in the timeline and evaluated during playback.

**Removal**: `ShotTrack.RemoveShot(ShotId)` → removes from list → fires `ShotRemoved` → if was current, selects adjacent shot → fires `CurrentShotChanged`.

**Reorder**: `ShotTrack.Reorder(ShotId, newIndex)` → move in list → fires `Reordered`. This changes the shot's effective color since color is index-based.

**Duration edit**: `ShotBlock.BeginDurationEdit()` opens inline timecode text field → commit parses flexible timecode → `shot.Duration = newDuration` (clamped).

**Boundary drag**: `ShotTrack.ResizeShotAtEdge(shotIndex, newEndTime)` → snaps to frame boundary → sets `shot.Duration`.

---

## Key References

### Shot Colors
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotColorPalette.cs` — 8-color palette, `GetColor(index)`
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotBlock.cs:19-25` — color assigned by index in constructor
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs:787-791` — camera keyframe color from shot index
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs:845-849` — segment color from shot index
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotSegmentInfo.cs` — data struct for track row background segments

### Camera Track Yellow Dot
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TrackRow.cs:68-71` — `track-type-dot--camera` element creation
- `/Unity/Fram3d/Assets/Resources/fram3d.uss:1024-1038` — CSS styles (8px circle, rgb 220,200,60 for camera, rgb 80,200,80 for element)

### Keyframe System
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/Keyframe.cs` — immutable generic keyframe with `WithTime()`/`WithValue()`
- `/Unity/Fram3d/Assets/Scripts/Core/Common/KeyframeId.cs` — GUID-based typed identity
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeManager.cs` — dual-storage collection with CRUD + interpolation
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeRecorder.cs` — stateless recording utility (auto + force)
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/CameraSnapshot.cs` — value struct capturing all animatable camera properties
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/StopwatchState.cs` — per-property recording toggle array
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/CameraProperty.cs` — sealed class enum (POSITION=0, ROTATION=1, FOCAL_LENGTH=2, FOCUS_DISTANCE=3, APERTURE=4)
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeSelection.cs` — single-selection model
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs:242-261` — `ForceRecordCamera()` method
- `/Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs:188-214` — C and V key handlers
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/KeyframeDiamond.cs` — visual element (22px hit area, 10px dot)
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TrackRow.cs:202-254` — `UpdateMainDiamonds()` with pooling, color, selection, drag

### Shot Model
- `/Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs` — aggregate root (5 keyframe managers, name, duration, stopwatch)
- `/Unity/Fram3d/Assets/Scripts/Core/Common/ShotId.cs` — GUID-based identity
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/ShotTrack.cs` — ordered shot collection with CRUD, global time math, observables
- `/Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotEvaluator.cs` — Core-to-Unity bridge, initial shot seeding

---

## Open Questions

1. **Shot color stability** — colors change when shots are reordered because they're index-based, not stored on the Shot. If a stable color per shot is needed (e.g., for multi-camera angle tracks), a color field would need to be added to Shot or a separate mapping maintained.

2. **Element keyframe color** — element track diamonds use a hard-coded green `(0.31, 0.78, 0.31)` rather than a configurable palette. This differs from camera track diamonds which inherit the shot's palette color.

*Last updated: 2026-03-29*
