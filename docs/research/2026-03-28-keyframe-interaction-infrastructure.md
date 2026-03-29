# Research: Keyframe Interaction Infrastructure for 3.2.4

**Date**: 2026-03-28
**Purpose**: Map what exists and what's missing before implementing spec 3.2.4 (Keyframe interaction)

---

## Summary

The codebase has solid infrastructure for keyframe storage, single-keyframe selection, and diamond rendering. However, spec 3.2.4's core behaviors — drag-to-reposition, delete, track-area scrub, V key for elements, sub-track diamond syncing, and parent/child keyframe rules — are **completely absent**. Selection is single-keyframe-only and works. The C key for manual camera keyframe creation is already wired. Everything else in 3.2.4 needs to be built.

---

## 1. Selection Model

### `KeyframeSelection` — `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeSelection.cs`

Single-selection model. Stores one selected keyframe at a time across all tracks.

**Full API:**
- `KeyframeId KeyframeId { get; }` — the selected keyframe's ID (null if none)
- `TrackId TrackId { get; }` — which track the selection is on (Camera or Element)
- `TimePosition Time { get; }` — the time of the selected keyframe
- `bool HasSelection` — true when `KeyframeId != null`
- `IObservable<bool> Changed` — fires on select/clear
- `void Select(TrackId trackId, KeyframeId keyframeId, TimePosition time)` — sets the selection
- `void Clear()` — clears all three fields, fires `Changed(false)`
- `bool IsSelected(KeyframeId id)` — equality check against current selection

**Owned by**: `Timeline.Selection` property (`Timeline.cs:81`)

**Subscribed to by**: `TimelineSectionView.Start()` subscribes to `Selection.Changed` to call `SyncTrackVisuals()` (`TimelineSectionView.cs:143`)

**Note**: This is single-selection only. No multi-select. No range selection. The spec for 3.2.4 only requires single selection, so this is sufficient.

---

## 2. KeyframeDiamond Click Handling

### `KeyframeDiamond` — `/Unity/Fram3d/Assets/Scripts/UI/Timeline/KeyframeDiamond.cs`

Minimal visual element. Just a `VisualElement` with:
- `.keyframe-diamond` CSS class
- `pickingMode = PickingMode.Position` (receives pointer events)
- `SetColor(bool isCamera)` — toggles `.keyframe-diamond--element` class
- `SetSelected(bool selected)` — toggles `.keyframe-diamond--selected` class

**No click handler on `KeyframeDiamond` itself.** Click events are registered by the *parent* (`TrackRow` or `SubTrackRow`) when diamonds are created.

### Click flow — Main diamonds on `TrackRow`

`TrackRow.UpdateMainDiamonds()` (`TrackRow.cs:115-159`):
1. Pools diamonds to match the number of keyframe times
2. Registers `ClickEvent` on each diamond that fires `DiamondClicked?.Invoke(null, times[idx])` — note: **keyframeId is null** for main diamonds
3. Positions diamonds via `style.left = px - 5f`
4. Updates selection state via `SetSelected()`

`TrackRow.DiamondClicked` is `event Action<KeyframeId, TimePosition>`.

### Click flow — Sub-track diamonds on `SubTrackRow`

`SubTrackRow.UpdateDiamondPositions()` (`SubTrackRow.cs:52-90`):
1. Same pooling pattern
2. Registers `ClickEvent` that fires `DiamondClicked?.Invoke(ids[idx], times[idx])` — **keyframeId IS provided** here
3. `SubTrackRow.DiamondClicked` bubbles up through `TrackRow.AddSubTrack()` (`TrackRow.cs:85`)

### Event routing in `TimelineSectionView`

`OnDiamondClicked(KeyframeId keyframeId, TimePosition time)` (`TimelineSectionView.cs:561-611`):
- If `keyframeId` is not null (sub-track click): calls `this._controller.SelectKeyframe(TrackId.Camera, keyframeId, time)`
- If `keyframeId` is null (main diamond click): searches position and rotation keyframes at that time to find a "representative" ID, then calls `SelectKeyframe`

`Timeline.SelectKeyframe()` (`Timeline.cs:319-324`):
- Calls `Selection.Select(trackId, keyframeId, time)`
- Scrubs the playhead to that keyframe's time
- Evaluates the camera at that time

**Limitation**: `OnDiamondClicked` always passes `TrackId.Camera` — element track diamond clicks don't correctly pass the element's TrackId. This is a bug or incomplete wiring.

---

## 3. KeyframeId

### `/Unity/Fram3d/Assets/Scripts/Core/Common/KeyframeId.cs`

Simple typed ID wrapper around `System.Guid`:
- `KeyframeId(Guid value)` — rejects `Guid.Empty`
- `Guid Value { get; }`
- Implements `IEquatable<KeyframeId>`, `==`, `!=`, `GetHashCode`, `ToString`

Created via `new KeyframeId(Guid.NewGuid())` wherever keyframes are recorded.

### `Keyframe<T>` — `/Unity/Fram3d/Assets/Scripts/Core/Timelines/Keyframe.cs`

Generic immutable keyframe:
- `KeyframeId Id`
- `TimePosition Time`
- `T Value`
- `CompareTo` sorts by time
- `WithTime(TimePosition)` — creates new Keyframe with same ID, different time
- `WithValue(T)` — creates new Keyframe with same ID, different value

**Important for 3.2.4**: `WithTime()` is the mechanism for drag-to-reposition. Create a new immutable keyframe with the same ID at a new time, then call `KeyframeManager.Update()` or `SetOrMerge()`.

---

## 4. Track Area Click Handling

### Current state: **Partial — selection clear only, no scrub**

`TimelineSectionView.BuildTrackArea()` (`TimelineSectionView.cs:294-300`):
```csharp
this._trackContainer.RegisterCallback<ClickEvent>(evt =>
{
    if (evt.target == this._trackContainer)
    {
        this._controller.Selection.Clear();
    }
});
```

This **only** clears selection when clicking the track container itself (empty space). It does **NOT**:
- Scrub the playhead to the clicked time
- Convert click position to time
- Handle anything beyond selection clearing

### Ruler scrub

Scrubbing only exists on the Ruler (`Ruler.cs:59-90`). The ruler has PointerDown/Move/Up handlers that fire `ScrubRequested` with the local pixel X. `TimelineSectionView.OnScrub()` forwards to `Timeline.BeginScrub()` + `Timeline.ScrubToPixel()`.

**What's missing for 3.2.4**: Clicking empty space in the track area should scrub the playhead to that time position (spec line: "Clicking empty space within the track area (not on a keyframe) scrubs the playhead to that time").

---

## 5. Keyboard Shortcuts

### `KeyboardShortcutRouter` — `/Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs`

**Currently wired:**
| Key | Action | Status |
|-----|--------|--------|
| C | `ForceRecordCamera` (manual camera keyframe) | DONE (`HandleKeyframeShortcuts`, line 197) |
| Q/W/E/R | Tool switching | DONE |
| A / Shift+A | Cycle aspect ratio | DONE |
| Ctrl+R | Reset camera/gizmo | DONE |
| Shift+D | Toggle DOF | DONE |
| `[` / `]` | Step aperture | DONE |
| S | Toggle shake | DONE |
| T | Toggle timeline visibility | DONE |
| Space | Toggle playback | DONE |
| `+` / `-` | Zoom in/out | DONE |
| `\` | Fit all | DONE |
| Home / End | Jump to start/end | DONE |
| 1-9 | Focal length presets | DONE |

**NOT wired (needed for 3.2.4):**
| Key | Action | Status |
|-----|--------|--------|
| V | Manual element keyframe creation | MISSING |
| Delete | Delete selected keyframe | MISSING |

---

## 6. KeyframeManager API

### `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeManager.cs`

Generic `KeyframeManager<T>` — dual storage (sorted list + dictionary by ID).

**Full mutation API:**

| Method | What it does | Use case |
|--------|-------------|----------|
| `Add(Keyframe<T>)` | Adds new. Throws if ID exists or time occupied. | Initial recording |
| `SetOrMerge(Keyframe<T>)` | Removes existing by same ID, removes existing at same time, then inserts. | Drag-merge, overwrite |
| `Update(Keyframe<T>)` | Replaces keyframe by ID (must exist). Removes old, re-inserts at new position. | Drag-reposition |
| `RemoveById(KeyframeId)` | Removes by ID. Returns bool. | Delete keyframe |
| `Clear()` | Removes all. | Stopwatch off |
| `GetById(KeyframeId)` | Lookup by ID, returns null if missing. | |
| `GetInRange(TimePosition, TimePosition)` | Range query (inclusive). | |
| `Evaluate(TimePosition, Func<T,T,float,T>)` | Linear interpolation. | Playback |

**Can you move/reposition?** Yes — `Update()` accepts a keyframe with the same ID but different time. Or use `Keyframe.WithTime()` + `Update()`.

**Can you delete?** Yes — `RemoveById(KeyframeId)`.

**Is there a merge operation?** Yes — `SetOrMerge()` handles the case where you drag a keyframe onto an existing time. It removes the old occupant at that time, then inserts the arriving keyframe.

**What's missing**: No batch move (move all keyframes at a time). No "move main keyframe" that moves children across all property managers atomically. This orchestration needs to be built in Core (probably on `Shot` or a new helper).

---

## 7. Sub-Track Diamond Syncing

`SubTrackRow.UpdateDiamondPositions()` exists and is fully implemented (`SubTrackRow.cs:52-90`), but it is **never called** from `TimelineSectionView.SyncTrackVisuals()`. The current `SyncTrackVisuals()` method:
1. Updates main diamonds on `_cameraTrackRow` via `UpdateMainDiamonds()` (**working**)
2. Updates sub-track live interpolated values via `SyncCameraSubTrackValues()` (**working**)
3. Does **NOT** call `UpdateDiamondPositions()` on any sub-track

This means: when the camera track is expanded, sub-track rows show property names and live values but **no diamonds**. The diamond rendering infrastructure exists in `SubTrackRow` but is unused.

---

## 8. What's NOT Implemented (3.2.4 Gap Analysis)

### Completely missing:

1. **Keyframe drag-to-reposition** — No drag handling on `KeyframeDiamond` or `TrackRow`/`SubTrackRow`. No PointerDown/Move/Up state machine for diamond drag. No snap-to-0.1s logic. No boundary clamping (0 to shot duration).

2. **Track area click-to-scrub** — Clicking empty track area only clears selection. Does not scrub playhead. Needs pixel-to-time conversion + scrub call.

3. **Delete key handling** — No `deleteKey` check anywhere in the codebase. No `HandleDelete` method. No minimum-keyframe enforcement logic.

4. **V key for element keyframes** — `vKey` not referenced anywhere. No `ForceRecordElement` wired to keyboard.

5. **Parent/child keyframe rules** — The spec describes a parent/child model where main keyframes "own" children across property sub-tracks. No such model exists in code. Currently, each `KeyframeManager<T>` is independent — there's no link between a position keyframe at t=1.0 and a rotation keyframe at t=1.0. The "main keyframe" concept only exists visually (via `GetAllCameraKeyframeTimes()` which unions all times). Rules needed:
   - Moving a main keyframe moves ALL property keyframes at that time
   - Moving a sub-track keyframe detaches it from its parent
   - Deleting a main keyframe deletes all property keyframes at that time
   - If all children leave/are deleted, the main keyframe auto-deletes
   - Minimum one camera keyframe enforcement

6. **Sub-track diamond rendering** — `SubTrackRow.UpdateDiamondPositions()` is defined but never called. Need to wire it in `SyncTrackVisuals()` for each sub-track with its specific keyframe manager's data.

7. **Keyframe merge on drag** — `SetOrMerge()` exists on `KeyframeManager` but no UI code invokes it during drag. The drag workflow (detect drag, show preview, commit position change, handle merge) is entirely unbuilt.

8. **Snap-to-grid (0.1s intervals)** — No snapping logic anywhere.

9. **Revolutions field on rotation keyframes** — Spec mentions it but the current `Keyframe<Quaternion>` has no revolutions data. This might be deferred.

### Partially built:

1. **C key for camera keyframes** — Fully wired and working. `HandleKeyframeShortcuts` calls `ForceRecordCamera`.

2. **Click-to-select keyframe** — Working for main diamonds (with representative ID lookup) and sub-track diamonds (via `DiamondClicked` event chain). However, element track clicks always pass `TrackId.Camera` which is a bug.

3. **Selection clear on empty click** — Working, but should be extended to also scrub.

4. **Selection visual feedback** — `SetSelected()` on `KeyframeDiamond` toggles `.keyframe-diamond--selected` class. Working for main diamonds and sub-track diamonds (if they were rendered).

---

## Key References

- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeSelection.cs` — Single-selection model
- `/Unity/Fram3d/Assets/Scripts/Core/Common/KeyframeId.cs` — Guid-based typed ID
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/Keyframe.cs` — Immutable generic keyframe with `WithTime()` / `WithValue()`
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeManager.cs` — Full CRUD with `Add`, `Update`, `SetOrMerge`, `RemoveById`
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/TrackId.cs` — Camera vs Element track identity
- `/Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs` — Owns 5 `KeyframeManager<T>` instances for camera properties
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs` — Owns 3 `KeyframeManager<T>` for position/rotation/scale
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs:81` — `Selection` property
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs:319-324` — `SelectKeyframe()` method
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs:294-300` — Track area click handler (clear only)
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs:561-611` — `OnDiamondClicked` routing
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/TrackRow.cs:115-159` — `UpdateMainDiamonds` with diamond pooling
- `/Unity/Fram3d/Assets/Scripts/UI/Timeline/SubTrackRow.cs:52-90` — `UpdateDiamondPositions` (defined, never called)
- `/Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs:188-205` — `HandleKeyframeShortcuts` (C key only)
- `/Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeRecorder.cs` — Stateless recording utility

---

## Open Questions

1. **Parent/child model**: Should this be a formal data structure (e.g., `MainKeyframe` class that owns child IDs), or should it remain implicit (keyframes at the same time are "grouped")? The current architecture groups by time coincidence, not by explicit parent references.

2. **Element track diamond click bug**: `OnDiamondClicked` always routes to `TrackId.Camera`. Element track diamond clicks need the correct `TrackId`. Should this be fixed as part of 3.2.4 or separately?

3. **Revolutions field**: The spec mentions it for rotation keyframes. Current `Keyframe<Quaternion>` has no slot for this. Is it in scope for 3.2.4 or deferred?
