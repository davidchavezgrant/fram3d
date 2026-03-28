# FRA-61: Per-track Stopwatch — Implementation Plan

> **For Claude:** Implement this plan task-by-task. Complete each task fully before moving to the next. Pause at phase boundaries for manual verification.

**Goal:** Add per-property stopwatch toggles to timeline tracks so manipulations create/update keyframes only when recording is on.

**Spec:** `docs/specs/milestone-3.2-keyframe-animation-spec.md` lines 246–395+

**Architecture:** `StopwatchState` (Core) tracks per-property recording state with top-level toggle. `KeyframeRecorder` (Core) creates/updates keyframes based on which properties changed, applying merge threshold (0.1s) and change detection thresholds. Input handlers call the recorder at manipulation endpoints (drag end, scroll end). UI shows stopwatch icons with on/off/partial states and a confirmation dialog when turning off with existing keyframes.

**Tech Stack:** C# 9, System.Numerics, Unity 6 UI Toolkit, xUnit + FluentAssertions

---

## Current State Analysis

### What exists
- `Shot` has 5 KeyframeManagers: CameraPositionKeyframes (Vector3), CameraRotationKeyframes (Quaternion), CameraFocalLengthKeyframes (float), CameraApertureKeyframes (float), CameraFocusDistanceKeyframes (float)
- `ElementTrack` has 3 KeyframeManagers: PositionKeyframes (Vector3), RotationKeyframes (Quaternion), ScaleKeyframes (float)
- `KeyframeManager<T>` supports Add, SetOrMerge, RemoveById, Clear, Evaluate, Count, Keyframes (sorted list)
- `Timeline` owns Shot track, Playhead, ElementTimeline, Selection, Expansion
- `CameraInputHandler` mutates camera via `ApplyScrollAction` (scroll) and `HandleDragInput` (drag). Both called from `Tick()` in `Update()`.
- `GizmoBehaviour.EndDrag()` is called when gizmo drag completes. `DragSession` has `StartPosition`/`StartRotation`/`StartScale` for before-state.
- `KeyboardShortcutRouter.Route()` handles focal presets (digit keys), aperture ([ ]), reset (Ctrl+R).
- No manipulation/recording infrastructure exists.

### Key Discoveries
- Camera drag (pan/tilt/orbit) happens per-frame in `HandleDragInput` (`CameraInputHandler.cs:254-282`). Record at drag end = need to detect when drag stops (mouse released).
- Camera scroll actions are discrete events via `ApplyScrollAction` (`CameraInputHandler.cs:95-130`). Each scroll event = one manipulation = record immediately after.
- Element gizmo drag: `GizmoBehaviour.EndDrag` (`GizmoBehaviour.cs:45-49`) fires once. Record here.
- `DragSession` snapshots `StartPosition`/`StartRotation`/`StartScale` — useful for change detection.
- `DollyZoom` mutates both position AND focal length atomically (`CameraElement.cs:118-140`).
- `Playhead.IsPlaying` exists for the playback guard.
- No `ICommand` pattern yet (4.1 milestone) — keyframes are written directly.

### Camera property → KeyframeManager mapping
| Manipulation | Properties changed | Manager(s) |
|---|---|---|
| Pan, Tilt, Roll, Orbit | Rotation | CameraRotationKeyframes |
| Dolly, Truck, Crane | Position | CameraPositionKeyframes |
| DollyZoom | Position + Focal Length | CameraPositionKeyframes + CameraFocalLengthKeyframes |
| Focal Length scroll/preset | Focal Length | CameraFocalLengthKeyframes |
| Focus Distance scroll | Focus Distance | CameraFocusDistanceKeyframes |
| Aperture step | Aperture | CameraApertureKeyframes |

### Element property → KeyframeManager mapping
| Manipulation | Properties changed | Manager(s) |
|---|---|---|
| Translate gizmo | Position | PositionKeyframes |
| Rotate gizmo | Rotation | RotationKeyframes |
| Scale gizmo | Scale | ScaleKeyframes |

## What We're NOT Doing

- **Undo/redo integration** — that's 4.1
- **Per-property sub-track stopwatch icons** — the spec describes these but they're a UI refinement. We implement the Core state (individual property toggles) but only render the top-level stopwatch icon on the track header for now. Sub-track icons can be added in a follow-up.
- **"Don't show again" persistence** — the dialog has the option but we store it in memory only (not project settings). Persistence is a 4.2 concern.
- **Stopwatch state persistence** — save/load is 4.2

---

## Phase 1: Core — StopwatchState

### Overview
A per-property recording state type. Camera tracks have 9 property slots, element tracks have 7. Top-level toggle sets all. Individual toggles. Partial state query.

### Task 1.1: CameraProperty and ElementProperty sealed classes

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/CameraProperty.cs`
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementProperty.cs`

These are closed value sets (sealed class with static readonly instances per CLAUDE.md).

`CameraProperty` instances: POSITION, ROTATION, FOCAL_LENGTH, FOCUS_DISTANCE, APERTURE
`ElementProperty` instances: POSITION, ROTATION, SCALE

Note: Position covers X/Y/Z together (they share one KeyframeManager<Vector3>). Rotation covers pan/tilt/roll together (one KeyframeManager<Quaternion>). Individual axis sub-track stopwatches are a UI concern that maps to these 5/3 property groups.

### Task 1.2: StopwatchState

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/StopwatchState.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/StopwatchStateTests.cs`

**API:**
```csharp
public sealed class StopwatchState
{
    // Construction
    public StopwatchState(int propertyCount)

    // Query
    public bool IsRecording(int propertyIndex)
    public bool AnyRecording       // true if any property is recording
    public bool AllRecording       // true if all properties are recording

    // Mutation
    public void SetAll(bool recording)
    public void Set(int propertyIndex, bool recording)
    public void Toggle(int propertyIndex)
    public void ToggleAll()        // if any on → all off, if all off → all on
}
```

Uses a `bool[]` internally. Property indices map to `CameraProperty`/`ElementProperty` ordinals.

**Tests:**
- `IsRecording__ReturnsFalse__When__Default`
- `AnyRecording__ReturnsFalse__When__Default`
- `SetAll__EnablesAll__When__CalledWithTrue`
- `AllRecording__ReturnsTrue__When__AllEnabled`
- `Set__EnablesSingleProperty__When__Called`
- `AnyRecording__ReturnsTrue__When__OneEnabled`
- `AllRecording__ReturnsFalse__When__OneEnabled`
- `ToggleAll__EnablesAll__When__NoneEnabled`
- `ToggleAll__DisablesAll__When__AnyEnabled`
- `Toggle__FlipsSingle__When__Called`

### Phase 1 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` — all pass

> **Pause here.**

---

## Phase 2: Core — KeyframeRecorder

### Overview
The recording brain. Takes the current state of a camera or element, determines which properties changed above threshold, and creates/updates keyframes in the appropriate managers.

### Task 2.1: RecordingThresholds constants

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/RecordingThresholds.cs`

Static class with constants from the spec:
```csharp
public static class RecordingThresholds
{
    public const float POSITION       = 0.001f;  // per axis
    public const float ROTATION_DEG   = 0.01f;   // degrees
    public const float FOCAL_LENGTH   = 0.01f;   // mm
    public const float SCALE          = 0.001f;
    public const double MERGE_WINDOW  = 0.1;     // seconds
}
```

### Task 2.2: KeyframeRecorder — camera recording

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeRecorder.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/KeyframeRecorderTests.cs`

The recorder is a stateless utility. It operates on a `Shot` + `StopwatchState` + playhead time.

**API:**
```csharp
public static class KeyframeRecorder
{
    /// <summary>
    /// Records camera state changes into the shot's keyframe managers.
    /// Only records properties that (a) have their stopwatch on, and
    /// (b) changed above threshold compared to previousState.
    /// If within MERGE_WINDOW of an existing keyframe, updates it.
    /// Otherwise creates a new keyframe.
    /// </summary>
    public static void RecordCamera(
        Shot shot,
        StopwatchState stopwatch,
        TimePosition playheadTime,
        CameraSnapshot current,
        CameraSnapshot previous)

    /// <summary>
    /// Records element state changes into the element track's keyframe managers.
    /// </summary>
    public static void RecordElement(
        ElementTrack track,
        StopwatchState stopwatch,
        TimePosition globalTime,
        ElementSnapshot current,
        ElementSnapshot previous)
}
```

### Task 2.3: CameraSnapshot and ElementSnapshot value types

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/CameraSnapshot.cs`
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementSnapshot.cs`

Simple structs capturing the property values at a moment:

```csharp
public struct CameraSnapshot
{
    public Vector3    Position;
    public Quaternion Rotation;
    public float      FocalLength;
    public float      FocusDistance;
    public float      Aperture;

    public static CameraSnapshot FromCamera(CameraElement cam) => new()
    {
        Position     = cam.Position,
        Rotation     = cam.Rotation,
        FocalLength  = cam.FocalLength,
        FocusDistance = cam.FocusDistance,
        Aperture     = cam.Aperture
    };
}

public struct ElementSnapshot
{
    public Vector3    Position;
    public Quaternion Rotation;
    public float      Scale;

    public static ElementSnapshot FromElement(Element element) => new()
    {
        Position = element.Position,
        Rotation = element.Rotation,
        Scale    = element.Scale
    };
}
```

Note: C# 9 structs — no `record struct`, no `init`. Use plain mutable struct fields.

### Task 2.4: KeyframeRecorder tests

**Key test cases:**
- `RecordCamera__CreatesKeyframe__When__StopwatchOnAndPositionChanged`
- `RecordCamera__DoesNotRecord__When__StopwatchOff`
- `RecordCamera__DoesNotRecord__When__ChangeBelowThreshold`
- `RecordCamera__UpdatesExisting__When__WithinMergeWindow`
- `RecordCamera__CreatesNew__When__OutsideMergeWindow`
- `RecordCamera__RecordsOnlyChanged__When__MultiplePropertiesButOneChanged`
- `RecordCamera__RecordsPositionAndFocal__When__DollyZoomChanges`
- `RecordCamera__SkipsProperty__When__PropertyStopwatchOff`
- `RecordElement__CreatesKeyframe__When__PositionChanged`
- `RecordElement__DoesNotRecord__When__StopwatchOff`
- `RecordElement__UpdatesExisting__When__WithinMergeWindow`

**Implementation details for RecordCamera:**
1. For each camera property (POSITION, ROTATION, FOCAL_LENGTH, FOCUS_DISTANCE, APERTURE):
   - Skip if `!stopwatch.IsRecording(property.Index)`
   - Check if change exceeds threshold (position: any axis > 0.001, rotation: euler delta > 0.01deg, etc.)
   - If changed: find nearest existing keyframe within MERGE_WINDOW of playheadTime
   - If found: update it via `SetOrMerge`
   - If not found: create new via `Add`

2. For rotation threshold: decompose both current and previous to EulerAngles, compare pan/tilt/roll.

3. For merge window: iterate `Keyframes` list, find any where `Math.Abs(kf.Time.Seconds - playheadTime.Seconds) < MERGE_WINDOW`.

### Phase 2 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` — all pass

> **Pause here.**

---

## Phase 3: Core — Wire into Timeline

### Overview
Add StopwatchState to Shot (camera) and ElementTrack (elements). Add recording API to Timeline that checks playback state.

### Task 3.1: StopwatchState on Shot and ElementTrack

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs`
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs`

Add to `Shot`:
```csharp
public StopwatchState CameraStopwatch { get; } = new(CameraProperty.COUNT);
```

Add to `ElementTrack`:
```csharp
public StopwatchState Stopwatch { get; } = new(ElementProperty.COUNT);
```

### Task 3.2: Recording API on Timeline

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs`

Add methods to Timeline:
```csharp
/// <summary>
/// Records camera state if the current shot's stopwatch allows it.
/// No-op during playback.
/// </summary>
public void RecordCameraManipulation(CameraSnapshot current, CameraSnapshot previous)
{
    if (this.Playhead.IsPlaying) { return; }
    if (this.CurrentShot == null) { return; }

    var shot = this.CurrentShot;
    var localTime = this.GetLocalPlayheadTime();
    if (localTime == null) { return; }

    KeyframeRecorder.RecordCamera(shot, shot.CameraStopwatch, localTime, current, previous);
}

public void RecordElementManipulation(ElementId elementId, ElementSnapshot current, ElementSnapshot previous)
{
    if (this.Playhead.IsPlaying) { return; }

    var track = this.Elements.GetOrCreateTrack(elementId);
    var globalTime = new TimePosition(this.Playhead.CurrentTime);

    KeyframeRecorder.RecordElement(track, track.Stopwatch, globalTime, current, previous);
}
```

Add helper:
```csharp
public TimePosition GetLocalPlayheadTime()
{
    var result = this.ResolveShot();
    if (!result.HasValue) { return null; }
    return result.Value.localTime;
}
```

### Task 3.3: ClearAllKeyframes on Shot and ElementTrack

For the "turn off stopwatch deletes all keyframes" behavior:

Add to `Shot`:
```csharp
public void ClearAllCameraKeyframes()
{
    this.CameraPositionKeyframes.Clear();
    this.CameraRotationKeyframes.Clear();
    this.CameraFocalLengthKeyframes.Clear();
    this.CameraApertureKeyframes.Clear();
    this.CameraFocusDistanceKeyframes.Clear();
}
```

Add to `ElementTrack`:
```csharp
public void ClearAllKeyframes()
{
    this.PositionKeyframes.Clear();
    this.RotationKeyframes.Clear();
    this.ScaleKeyframes.Clear();
}
```

**Tests:**
- `RecordCameraManipulation__DoesNotRecord__When__Playing`
- `RecordCameraManipulation__RecordsKeyframe__When__StopwatchOnAndChanged`
- `RecordElementManipulation__DoesNotRecord__When__Playing`
- `ClearAllCameraKeyframes__ClearsAll__When__Called`
- `ClearAllKeyframes__ClearsAll__When__Called`

### Phase 3 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` — all pass

> **Pause here.**

---

## Phase 4: Engine/UI — Hook into Input Handlers

### Overview
Wire the input handlers to take before/after snapshots and call `Timeline.RecordCameraManipulation` / `RecordElementManipulation`.

### Task 4.1: CameraInputHandler — snapshot and record

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`

**Approach:** CameraInputHandler needs access to `Timeline` (via `ShotEvaluator.Controller`). Add a `_timeline` field set in `Start()`.

For scroll actions — snapshot before, mutate, snapshot after, record:
```csharp
private void ApplyScrollAction(ScrollAction action)
{
    var before = CameraSnapshot.FromCamera(this._camera);

    // ... existing mutation code unchanged ...

    var after = CameraSnapshot.FromCamera(this._camera);
    this._timeline?.RecordCameraManipulation(after, before);
}
```

For drag — track drag state. Take snapshot when drag starts (mouse button pressed with drag detected), record when drag ends (mouse button released):
- Add `_cameraBeforeDrag` field (CameraSnapshot)
- Add `_isCameraDragging` field (bool)
- In `HandleDragInput`: when action is not NONE and `!_isCameraDragging`, set `_isCameraDragging = true`, snapshot before.
- In `HandleDragInput` or `Tick`: when drag ends (action is NONE and `_isCameraDragging`), record after snapshot, set `_isCameraDragging = false`.

### Task 4.2: GizmoBehaviour.EndDrag — element recording

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoBehaviour.cs`

`EndDrag` already has the `DragSession` which stores `StartPosition`/`StartRotation`/`StartScale`. Construct before/after snapshots and call `Timeline.RecordElementManipulation`.

GizmoBehaviour needs access to `Timeline`. Add a reference set from `SelectionInputHandler` or directly from `Start()`.

```csharp
public void EndDrag()
{
    if (this._activeDrag != null && this._timeline != null)
    {
        var before = new ElementSnapshot
        {
            Position = this._activeDrag.StartPosition,
            Rotation = this._activeDrag.StartRotation,
            Scale    = this._activeDrag.StartScale
        };
        var after = ElementSnapshot.FromElement(this._activeDrag.Element);
        this._timeline.RecordElementManipulation(
            this._activeDrag.Element.Id, after, before);
    }

    this._highlighter.ClearDrag();
    this._activeDrag = null;
}
```

### Task 4.3: KeyboardShortcutRouter — camera mutation recording

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs`

For focal length presets, aperture steps, and reset — wrap in before/after snapshot:

Add `_timeline` field to KeyboardShortcutRouter, set via `Configure`.

In `HandleFocalLengthPresets`, `HandleToggles` (aperture `[`/`]`), and `HandleReset`:
```csharp
var before = CameraSnapshot.FromCamera(camera);
// ... existing mutation ...
var after = CameraSnapshot.FromCamera(camera);
this._timeline?.RecordCameraManipulation(after, before);
```

### Task 4.4: C and V keyboard shortcuts

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs`

Add `HandleKeyframeShortcuts` method, called from `Route` before other handlers:

- **C key** (no modifiers): Create camera keyframe at playhead. Snapshots current camera state and records ALL properties (force-record regardless of change threshold). Disabled during playback.
- **V key** (no modifiers): Create element keyframe at playhead for selected element. Same force-record behavior. Disabled during playback.

For force-recording, add a `forceRecord` parameter to `RecordCamera`/`RecordElement`, or create a separate `ForceRecordCamera`/`ForceRecordElement` method on Timeline that bypasses change detection.

### Phase 4 Verification

#### Manual
- [ ] Unity builds without errors
- [ ] Camera pan/tilt, then verify no keyframes created (stopwatch off by default)
- [ ] Turn on camera stopwatch (via code or debug), move camera, verify keyframe appears
- [ ] During playback, verify no keyframes created even with stopwatch on
- [ ] Element gizmo drag, verify no keyframes when stopwatch off
- [ ] C key creates camera keyframe, V key creates element keyframe

> **Pause here.**

---

## Phase 5: UI — Stopwatch Icons and Confirmation Dialog

### Overview
Add stopwatch toggle icons to TrackRow headers. Clicking toggles recording state. Turning off with existing keyframes shows a confirmation dialog.

### Task 5.1: Stopwatch icon on TrackRow

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TrackRow.cs`

Add a clickable stopwatch element in the track header, between the arrow and the name label:
- Small circle (12x12) that toggles between on (filled, red/recording color) and off (outline only)
- CSS classes: `.track-stopwatch`, `.track-stopwatch--on`, `.track-stopwatch--partial`
- `StopwatchClicked` event fires on click

### Task 5.2: USS styles for stopwatch

**Files:**
- Modify: `Unity/Fram3d/Assets/Resources/fram3d.uss`

```css
.track-stopwatch {
    width: 12px;
    height: 12px;
    border-radius: 6px;
    border-width: 1px;
    border-color: rgb(180, 60, 60);
    background-color: transparent;
    margin-right: 4px;
    cursor: link;
}

.track-stopwatch--on {
    background-color: rgb(200, 50, 50);
    border-color: rgb(220, 70, 70);
}

.track-stopwatch--partial {
    background-color: rgb(140, 50, 50);
    border-color: rgb(180, 60, 60);
}
```

### Task 5.3: Wire stopwatch click in TimelineSectionView

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

When stopwatch is clicked:
1. If turning ON: Set all properties to recording (`SetAll(true)`)
2. If turning OFF:
   a. Check if track has keyframes
   b. If yes: show confirmation dialog
   c. If confirmed: clear all keyframes, set stopwatch off
   d. If no keyframes: just set stopwatch off

### Task 5.4: Confirmation dialog

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/UI/Timeline/StopwatchConfirmDialog.cs`

A simple modal overlay with:
- Message: "Turning off the stopwatch will delete all keyframes on this track. Continue?"
- "Don't show again" checkbox
- Confirm and Cancel buttons
- Built with UI Toolkit VisualElements
- Memory-only "don't show again" flag (not persisted)

### Task 5.5: Sync stopwatch visual state

In `SyncTrackVisuals()`, update each track row's stopwatch visual based on the `StopwatchState`:
- Camera: `shot.CameraStopwatch.AnyRecording` / `AllRecording`
- Element: `track.Stopwatch.AnyRecording` / `AllRecording`

### Phase 5 Verification

#### Manual
- [ ] Stopwatch icon visible on camera track header (circle, off by default)
- [ ] Clicking stopwatch turns it on (filled red)
- [ ] Moving camera with stopwatch on creates keyframe diamond
- [ ] Clicking stopwatch to turn off shows confirmation dialog (if keyframes exist)
- [ ] Confirming deletes all keyframes
- [ ] Cancelling preserves keyframes and keeps stopwatch on
- [ ] Turning off with no keyframes: no dialog, immediate off
- [ ] "Don't show again" suppresses future dialogs
- [ ] Element track stopwatch works the same
- [ ] During playback, no keyframes created
- [ ] C key creates camera keyframe when not playing
- [ ] V key creates element keyframe when not playing
- [ ] C/V do nothing during playback

---

## Testing Strategy

### Unit Tests (xUnit — Core)
- StopwatchState: default off, set/toggle individual, set/toggle all, partial state queries
- KeyframeRecorder.RecordCamera: creates keyframe, skips when off, skips below threshold, merges within window, creates new outside window, records only changed properties, handles DollyZoom (position + focal), skips when property stopwatch off
- KeyframeRecorder.RecordElement: creates keyframe, skips when off, merges within window
- Timeline.RecordCameraManipulation: no-op during playback, delegates to recorder
- Timeline.RecordElementManipulation: no-op during playback, creates track on first record
- Shot.ClearAllCameraKeyframes: clears all 5 managers
- ElementTrack.ClearAllKeyframes: clears all 3 managers

### Manual Testing Steps
1. Open scene, verify stopwatch off (circle outline on camera track)
2. Move camera — no keyframes created
3. Click stopwatch on — red filled circle
4. Pan camera — yellow diamond appears at playhead time
5. Scrub to new time, dolly camera — second diamond appears
6. Scrub near first diamond (within 0.1s), tilt camera — first diamond updated (no new diamond)
7. Press play, watch camera animate — no new keyframes during playback
8. Stop playback, press C — camera keyframe created at stop position
9. Click stopwatch off — confirmation dialog
10. Confirm — all keyframes deleted, stopwatch off
11. Select element, click element stopwatch on, drag with gizmo — green diamond
12. Press V — element keyframe created

## Performance Considerations

- `CameraSnapshot.FromCamera` is 5 field reads — negligible
- Change detection for rotation uses `EulerAngles.FromQuaternion` — ~20 trig ops. Only called on manipulation end, not per-frame. Fine.
- Merge window search iterates keyframes linearly. Typical shot has <50 keyframes. Fine.

## References

- Spec: `docs/specs/milestone-3.2-keyframe-animation-spec.md` lines 246–395+
- Ticket: FRA-61
- Camera mutation sites: `CameraInputHandler.cs:95-130` (scroll), `CameraInputHandler.cs:254-282` (drag)
- Element mutation sites: `GizmoBehaviour.cs:45-49` (EndDrag), `DragSession.cs` (transform writes)
- Keyboard shortcuts: `KeyboardShortcutRouter.cs:37-50` (Route chain)
