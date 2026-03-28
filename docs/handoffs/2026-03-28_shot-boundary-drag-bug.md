# Shot Boundary Drag Bug — Handoff

## The Problem

Dragging shot boundaries in the shot track strip is unreliable. The resize cursor appears when hovering near a boundary, but clicking and dragging often does nothing — the cursor reverts and no resize occurs. Sometimes it works, sometimes it doesn't, with no obvious pattern from the user's perspective.

## Current Branch

`me/fra-59-321-timeline-editor` — PR #27

## Architecture

The shot track interaction is split across two layers:

1. **Core controller** (`Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs`) — Pure C# state machine. Receives pixel x coordinates, converts to time via `PixelToTime()`, detects edges via `FindEdgeAtTime()`, manages drag state. Returns `ShotTrackAction` enum values (NONE, POTENTIAL_CLICK, CLICK, NEAR_EDGE, BOUNDARY_DRAG, etc.).

2. **UI view** (`Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotTrackStrip.cs`) — VisualElement. Registers PointerDown/Move/Up callbacks on `_trackArea`. Forwards coordinates to the controller. Shows cursor changes, drop indicators, tooltips.

The track area (`_trackArea`) contains absolutely-positioned child `ShotBlock` elements that cover the entire strip width. The playhead, drop indicator, and out-of-range overlay are also children.

## Interaction Flow

1. **Hover**: `OnPointerMove` → `ShotTrackPointerMove(x)` → if x is within `EDGE_TOLERANCE_PX` (12px) of a shot boundary → returns `NEAR_EDGE` → cursor changes to resize
2. **Click**: `OnPointerDown` → `ShotTrackPointerDown(x)` → if x is within tolerance → returns `BOUNDARY_DRAG`, sets `_isBoundaryDragging = true`, stores `_boundaryDragIndex`
3. **Drag**: `OnPointerMove` → `ShotTrackPointerMove(x)` → if `_isBoundaryDragging` → calls `ResizeShotAtEdge()` → returns `BOUNDARY_DRAG`
4. **Release**: `OnPointerUp` → `ShotTrackPointerUp()` → returns `BOUNDARY_COMPLETE`, resets state

## Attempted Fixes (all failed)

### 1. Increased edge tolerance (6px → 12px)
`Timeline.cs` `EDGE_TOLERANCE_PX` changed from 6.0 to 12.0. Made the hover zone larger but didn't fix the click-miss problem.

### 2. Pointer capture on all left-button clicks
`ShotTrackStrip.OnPointerDown` now calls `_trackArea.CapturePointer()` for all left clicks, not just boundary drags. Intended to prevent losing tracking when pointer drifts vertically. Didn't fix the core issue.

### 3. Hovered edge fallback
Added `_lastHoveredEdgeIndex` to Timeline — stored the edge index from `NEAR_EDGE` hover detection, used as fallback in `ShotTrackPointerDown` when `FindEdgeAtTime` missed. Removed after finding what seemed like the real cause.

### 4. WorldToLocal coordinate conversion (current state)
**Hypothesis**: `evt.localPosition` in UI Toolkit is relative to the event **target** (the ShotBlock child that received the event), not the element where the callback is registered (`_trackArea`). Since ShotBlocks cover the entire strip, clicks near boundaries would give coordinates within the ShotBlock (e.g., 295px from its left edge), not within the track area.

**Fix**: Added `TrackLocalX()` helper that uses `_trackArea.WorldToLocal(evt.position).x` to convert panel coordinates to track-area-local coordinates.

**Result**: Still not working. The hypothesis about `localPosition` coordinate space may be wrong, or there's an additional issue.

## Things to Investigate

1. **Verify the coordinate space theory empirically.** Add `Debug.Log` statements in `OnPointerDown` comparing `evt.localPosition.x`, `evt.position.x`, and `_trackArea.WorldToLocal(evt.position).x`. Click on boundaries and see which values are correct.

2. **Check if `WorldToLocal` works correctly.** `evt.position` might be in screen space, not panel space. UI Toolkit's `WorldToLocal` expects panel-space input. If `evt.position` is screen-space, the conversion would be wrong.

3. **Check if pointer capture changes coordinate spaces.** After `CapturePointer`, subsequent move events might report `localPosition` relative to the capturing element. If so, the coordinate fix might break *during* a drag even if it fixes the initial click.

4. **Try registering callbacks in TrickleDown phase.** `RegisterCallback<PointerDownEvent>(handler, TrickleDown.TrickleDown)` fires before the event reaches children. In trickle-down, `localPosition` might be relative to `_trackArea` since it hasn't reached any child yet.

5. **Try setting ShotBlock pickingMode to Ignore near edges.** If the ShotBlocks don't pick near their edges (say, within 15px of either side), the pointer events would target `_trackArea` directly and `localPosition` would be correct.

6. **Consider a completely different approach**: Instead of relying on pointer events bubbling from children, overlay an invisible VisualElement (with `pickingMode = Position`) on top of the track area just for boundary hit zones. Each boundary gets its own invisible hit zone element. This bypasses all coordinate space issues.

## Key Files

- `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs` — state machine, `ShotTrackPointerDown/Move/Up`, `EDGE_TOLERANCE_PX`
- `Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotTrackStrip.cs` — `OnPointerDown/Move/Up`, `TrackLocalX()`, `Bind()`
- `Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotBlock.cs` — the child elements covering the strip
- `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs` — controller-level tests (these all pass; the bug is in the UI coordinate layer)
