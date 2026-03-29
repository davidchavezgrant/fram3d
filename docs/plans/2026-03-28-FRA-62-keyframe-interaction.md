# FRA-62: Keyframe Interaction — Implementation Plan

**Spec**: 3.2.4 Keyframe interaction
**Branch**: `me/fra-62-324-keyframe-interaction`
**Date**: 2026-03-28

---

## Overview

Click to select, drag to reposition, delete selected, merge on drag, minimum 1 camera keyframe enforced, manual keyframe creation (C/V keys), track area click-to-scrub.

## Phases

### Phase 1: Core — Shot keyframe operations

New methods on `Shot` for batch keyframe manipulation:

**File**: `Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs`

- `MoveAllCameraKeyframesAtTime(TimePosition from, TimePosition to)` — for main keyframe drag. Finds all property keyframes at `from`, moves each to `to` using `SetOrMerge`. Snapping is the caller's responsibility.
- `DeleteAllCameraKeyframesAtTime(TimePosition time)` — for main keyframe delete. Removes all property keyframes at that time across all 5 managers.
- `CanDeleteCameraKeyframesAtTime(TimePosition time)` — returns false if this is the last unique keyframe time (minimum 1 enforcement). Checks `GetAllCameraKeyframeTimes().Count <= 1`.
- `CountUniqueCameraKeyframeTimes` — computed property, returns count of unique times.

**Tests**: `tests/Fram3d.Core.Tests/Shots/ShotTests.cs` — add tests for move, delete, minimum enforcement.

### Phase 2: Core — ElementTrack keyframe operations

Same batch operations on `ElementTrack`:

**File**: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs`

- `MoveAllKeyframesAtTime(TimePosition from, TimePosition to)`
- `DeleteAllKeyframesAtTime(TimePosition time)`
- No minimum enforcement for element tracks (spec only requires it for camera).

**Tests**: `tests/Fram3d.Core.Tests/Timelines/ElementTrackTests.cs`

### Phase 3: Core — Timeline orchestration

Add keyframe interaction methods to `Timeline`:

**File**: `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs`

- `DeleteSelectedKeyframe()` — reads `Selection`, delegates to `Shot` or `ElementTrack`, clears selection, re-evaluates camera.
- `MoveSelectedKeyframe(TimePosition newTime)` — reads `Selection`, snaps to 0.1s grid, clamps to [0, duration], delegates to `Shot.MoveAllCameraKeyframesAtTime` or `ElementTrack.MoveAllKeyframesAtTime`, updates selection time, re-evaluates camera.
- `AddElementKeyframeAtPlayhead(ElementId elementId)` — creates keyframe for all properties at current playhead. No-op during playback.
- `ScrubTrackArea(double px)` — converts pixel to time, scrubs playhead, clears selection.

Snap helper: `SnapToGrid(double seconds, double grid)` — static utility, returns `Math.Round(seconds / grid) * grid`.

**Tests**: `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs`

### Phase 4: UI — Keyboard shortcuts (Delete, V)

**File**: `Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs`

- Delete key → calls `Timeline.DeleteSelectedKeyframe()`
- V key → calls `Timeline.AddElementKeyframeAtPlayhead(selectedElementId)` (needs selected element from scene selection)

### Phase 5: UI — Track area click-to-scrub

**File**: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

- Modify the track container click handler (currently only clears selection) to also scrub the playhead to the clicked position.
- Need to convert click position to global time using `PixelToTime`.

### Phase 6: UI — Keyframe diamond drag

**File**: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

New drag state machine for keyframe diamonds:
- `PointerDownEvent` on diamond → record drag start position and keyframe info
- `PointerMoveEvent` → compute new time from mouse X, snap to 0.1s, clamp to [0, duration], update diamond position visually
- `PointerUpEvent` → commit the move via `Timeline.MoveSelectedKeyframe(newTime)`
- Capture pointer on drag start, release on drop

This interacts with the existing diamond click handling — need to distinguish click from drag (use a minimum pixel threshold).

### Phase 7: UI — Fix element track diamond click bug

**File**: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

`OnDiamondClicked` always passes `TrackId.Camera`. Fix to pass the correct `TrackId` from the `TrackRow` that raised the event.

### Phase 8: UI — Sub-track diamond rendering

**File**: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

Wire `SubTrackRow.UpdateDiamondPositions()` calls into `SyncTrackVisuals()` when tracks are expanded. Currently sub-track diamonds exist but are never populated.

---

## Deferred (not in 3.2.4 scope)

- **Revolutions field** — spec mentions it but it's about interpolation (3.2.5), not interaction
- **Property keyframe detach on drag** — moving a single property keyframe from one main time to another (parent/child rules). This requires expanded sub-tracks with draggable property diamonds. Build in a follow-up.
- **Multi-select** — spec 8.1.1, not needed here

## File Change Summary

| File | Changes |
|------|---------|
| `Core/Shots/Shot.cs` | +MoveAllCameraKeyframesAtTime, +DeleteAllCameraKeyframesAtTime, +CanDeleteCameraKeyframesAtTime, +CountUniqueCameraKeyframeTimes |
| `Core/Timelines/ElementTrack.cs` | +MoveAllKeyframesAtTime, +DeleteAllKeyframesAtTime |
| `Core/Timelines/Timeline.cs` | +DeleteSelectedKeyframe, +MoveSelectedKeyframe, +AddElementKeyframeAtPlayhead, +ScrubTrackArea |
| `UI/Input/KeyboardShortcutRouter.cs` | +Delete key, +V key |
| `UI/Timeline/TimelineSectionView.cs` | Track area scrub, drag state machine, fix element diamond click, sub-track diamond sync |
| `tests/.../ShotTests.cs` | New tests |
| `tests/.../ElementTrackTests.cs` | New tests |
| `tests/.../TimelineTests.cs` | New tests |
