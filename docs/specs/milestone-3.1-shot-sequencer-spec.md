# Milestone 3.1: Shot Track

**Date**: 2026-03-10
**Status**: Draft
**Blocked by**: 1.1 (Virtual camera), 2.1 (Scene management)

---

Each shot is an independent camera animation over shared world-space elements. A director thinks within shots. "Shot 1: wide establishing. Shot 2: over-the-shoulder. Shot 3: close-up reaction." The shot track is their shot list made tangible -- a horizontal strip of thumbnails representing the structure of the scene.

---

## 3.1.1 Shot Model

***Elements exist in world space and animate on a single global timeline. Shots are windows into this timeline — each shot captures camera animation for its time segment. Element keyframes live on the global timeline, camera keyframes are per-shot.***

### Shot Definition

- Every shot contains: name (string), duration (seconds), camera animation, and a time range on the global timeline
- Camera animation stores: position, rotation -- each as keyframed curves (per-shot)
- Element animation lives on the global element timeline (not per-shot) — see 3.1.3
- Default duration 5 seconds, max 300 seconds (5 minutes)

### Naming

``` python
  .if >> shot created via "Add Shot" button
      name <== "Shot_NN" where NN is a zero-padded sequential number (01, 02, 03...)
      numbering follows insertion order, not position within sequence
      <== name is unique within the sequence at time of creation

  .if >> shot name matches existing name after drag-and-drop reorder
      <== names are unchanged -- ordering does not trigger renaming

  .if >> user double-clicks shot name
      <== name becomes editable inline text field
      <== accepts any non-empty string up to 32 characters
      !== empty string accepted as a name

  .if >> user clears name field and confirms (Enter or blur)
      <== name reverts to previous value
```

### Duration

- Every shot has a duration expressed within seconds
- Default duration: 5.0 seconds
- Minimum duration: 0.1 seconds
- Maximum duration: 300.0 seconds (5 minutes)
- Duration resolution: 0.1 seconds (tenths)

``` python
  .if >> user enters a duration below 0.1
      <== duration clamps to 0.1

  .if >> user enters a duration above 300.0
      <== duration clamps to 300.0

  .if >> user enters a non-numeric value for duration
      <== duration reverts to previous value

  .if >> duration is shortened below existing keyframe times
      <== keyframes beyond the new duration are preserved but unreachable during playback
      !== keyframes deleted when duration is shortened

  .if >> duration is later extended again
      <== previously unreachable keyframes become reachable again
```

### Initial State

``` python
  .if >> new shot is created
      <== camera animation begins using a single keyframe at t=0 capturing the current camera position and rotation
      <== the shot maps to the next time range on the global element timeline (immediately after the previous shot)
      <== elements are at whatever position the global timeline defines for this time
      !== new copies of elements created for the shot
      !== camera state reset to defaults
```

### Minimum Viable Shot

- A shot must contain at minimum: a name, a duration, and one camera keyframe at t=0
- The shot maps to a time range on the global element timeline
- All world-space elements are always present; the shot only stores camera animation

---

## 3.1.2 Shot Track UI

***A scrollable horizontal thumbnail strip at the bottom of the view. Each thumbnail represents one shot. The director sees their shot list at a glance and restructures it by dragging.***

### Layout

- The shot track is a horizontal bar integrated into the timeline section, positioned between the ruler and the active angle track/track area
- Left 140px: label column reading "SHOTS" (consistent with all timeline label columns — see `ui-layout-spec.md` §4.1 for the full timeline component stack)
- Right side: horizontally scrollable strip of shot blocks, each rendered as a colored rectangle sized proportionally to the shot's duration within the visible time range
- Contains: shot blocks (one row per camera in multi-camera shots), an "Add Shot" button (in the label column), aggregate duration display, shot boundary drag handles
- Shot bar height: 20px per camera row, auto-adjusts to the maximum camera count across all shots (e.g., 22px for single-camera, 42px for two-camera). See also 9.1.3 for multi-camera shot bar states.

> **Note:** The roadmap originally described shot thumbnails (120x100px cards with preview images). The mockup evolved to a timeline-style horizontal bar where shots are colored blocks proportional to duration. The timeline bar approach provides better spatial context for shot timing and integrates naturally with the ruler, tracks, and zoom bar. The thumbnail preview approach (for a shot list/overview context) may be revisited as a separate panel.

### Aggregate Duration

- The shot track displays the total running time of all shots combined
- Format: "Total: Xs" where X is the sum of all shot durations

``` python
  .if >> a shot's duration changes
      <== aggregate duration updates immediately

  .if >> a shot is added or removed
      <== aggregate duration updates immediately

  .if >> no shots exist
      <== aggregate duration displays "Total: 0.0s"
```

### Shot Thumbnails

- Each shot renders as a thumbnail within the strip
- Thumbnail dimensions: 120px wide, 100px tall
- Thumbnail contents from top to bottom: preview image, shot name, editable duration field
- Preview image shows a render of the camera's view at t=0 of that shot (the first frame)

``` python
  .if >> user modifies the camera position at t=0 of a shot
      <== that shot's thumbnail updates to reflect the new framing
      !== thumbnail updates for keyframes at times other than t=0

  .if >> shot is currently selected
      <== shot displays a visible highlight border distinguishing it from unselected shots

  .if >> user clicks a shot on the shot track
      <== that shot becomes the current shot
      <== view jumps to t=0 of that shot
      <== camera and all elements evaluate to their t=0 state for that shot
      <== keyframe editor (3.2) loads that shot's tracks, .if >> keyframe editor exists
```

### Scrolling

``` python
  .if >> total thumbnail width exceeds available horizontal space
      <== horizontal scrollbar appears below the thumbnails
      <== user scrolls via scrollbar drag, mouse wheel over the shot track area, or trackpad horizontal scroll

  .if >> total thumbnail width fits within available space
      !== scrollbar shown

  .if >> new shot is added while scrolled
      <== shot track auto-scrolls to reveal the new shot
```

### Add Shot

- An "Add Shot" button appears at the right end of the thumbnail strip, after the last shot
- The button is always visible (scrolls along using the strip)

``` python
  .if >> user clicks "Add Shot"
      <== new shot is appended to the end of the sequence
      <== new shot becomes the current shot
      <== new shot maps to the next time range on the global timeline (after the previous shot's end)
      <== elements are at their global timeline positions for this time range
      <== shot track scrolls to show the new shot

  .if >> no shots exist and user clicks "Add Shot"
      <== first shot is created with camera capturing current view
      <== shot maps to t=0 on the global timeline
      <== that shot becomes current
```

### Delete Shot

- Each shot has a delete control (button or icon)

``` python
  .if >> user clicks delete on a shot
      <== confirmation dialog appears: "Delete [shot name]? This cannot be undone."
      <== dialog contains a "Don't show this again" checkbox
      !== shot deleted without confirmation (unless user has previously checked "Don't show this again")

  .if >> user checks "Don't show this again" and confirms deletion
      <== all future delete actions skip the confirmation dialog
      <== a menu item exists (Edit > Reset Delete Confirmation) to re-enable the confirmation dialog

  .if >> user clicks the "Reset Delete Confirmation" menu item
      <== the confirmation dialog is re-enabled for future deletions

  .if >> user confirms deletion on a shot that is not the only shot
      <== shot is removed from the sequence
      <== if the deleted shot was the current shot, the next shot becomes current; if it was the last shot, the previous shot becomes current
      <== view and keyframe editor update to the new current shot

  .if >> user confirms deletion on the only remaining shot
      <== shot is removed
      <== sequence is now empty
      <== view shows the scene within its default state (no active shot)
      <== keyframe editor clears (no tracks, no playhead)
      !== application crash or error state

  .if >> user cancels the confirmation dialog
      <== shot is not deleted
      <== no state changes

  .if >> sequence is empty
      <== "Add Shot" button remains visible and functional
      !== playback controls functional (nothing to play)
```

### Shot Duplication

``` python
  .if >> user presses Ctrl+D while a shot is selected
      !== shot duplicated
      <== no action is taken
```

### Default State

``` python
  .if >> project is opened for the first time (fresh scene)
      <== one shot exists: "Shot_01", duration 5.0s, capturing default camera and scene state
      !== empty shot track on first launch
```

### Drag-and-Drop Reordering

``` python
  .if >> user presses and holds on a shot
      <== after a brief hold threshold (not an instant click), shot enters drag mode
      <== shot visually lifts from the strip (slight scale increase or opacity change)
      <== a drop indicator (vertical line or gap) appears between shots showing where the shot would land

  .if >> user drags a shot over the shot track strip
      <== drop indicator tracks the cursor position, snapping to valid insertion points
      <== valid insertion points: before the first shot, between any two shots, after the last shot
      !== drop indicator shown outside the shot track strip

  .if >> user releases the shot at a new position
      <== shot moves to the indicated position
      <== remaining shots shift to accommodate
      <== shot names are unchanged (Shot_01 dragged to position 3 is still "Shot_01")
      <== the moved shot remains the current shot
      <== thumbnail order reflects the new sequence

  .if >> user releases the shot at its original position
      <== no change to sequence order
      !== shot duplicated or deleted

  .if >> user drags the shot outside the shot track strip and releases
      <== drag cancels, shot returns to original position
      !== shot deleted by dragging out

  .if >> user reorders shots while playback is active
      <== playback stops immediately
      <== the dragged shot becomes current at t=0
      !== playback continues using the old sequence order
```

### Duration Editing

- Duration is displayed as a text field on each shot
- Format: "Xs" where X is the duration value (e.g., "5.0s")

``` python
  .if >> user clicks the duration field
      <== field becomes editable, text is selected for easy replacement
      <== field accepts numeric input only

  .if >> user presses Enter or clicks away from the duration field
      <== value is validated and applied per Duration rules above
      <== thumbnail does not change (duration change does not affect the t=0 frame)
```

### Shot Boundary Dragging (Ripple Editing)

- The boundary between two adjacent shots is a draggable handle
- Dragging a boundary adjusts the end time of the left shot and shifts all downstream content:
  - All subsequent shots (start and end times)
  - All camera keyframes in subsequent shots
  - All angle segments in subsequent shots
  - All element keyframes at or after the boundary on the global timeline
  - All linked periods at or after the boundary
- Hold **Shift** to resize shots only — element keyframes on the global timeline stay in place (useful for tightening or loosening a shot without disrupting blocking)
- Boundary position snaps to frame boundaries (nearest `1/fps` increment)
- During drag, a resize tooltip follows the cursor showing: shot name, new duration (seconds + frames), and ripple mode indicator (`[ripple]` or `[shots only]`)
- Minimum shot duration of 0.1s is enforced during drag — the boundary cannot be dragged past the previous shot's start + 0.1s

``` python
  .if >> user drags the boundary between Shot_01 and Shot_02 rightward by 1 second
      <== Shot_01 duration increases by 1 second
      <== Shot_02 start shifts right by 1 second
      <== Shot_02 end shifts right by 1 second
      <== all subsequent shots shift right by 1 second
      <== all element keyframes at or after the original boundary shift right by 1 second
      <== total project duration increases by 1 second

  .if >> user holds Shift and drags the boundary rightward by 1 second
      <== Shot_01 duration increases by 1 second
      <== Shot_02 and all subsequent shots shift right by 1 second
      !== element keyframes shift — they stay at their original global times
      <== this allows "loosening" a shot (giving the camera more time) without disturbing character blocking

  .if >> user drags a boundary and the resize tooltip is visible
      <== tooltip shows: "WIDE ESTABLISHING: 4.0s (96f) [ripple]"
      <== tooltip updates in real time during drag
```

### Shot Hover Tooltip

- Hovering over a shot in the shot bar displays a tooltip with metadata
- Tooltip shows: shot name, camera name, duration (seconds + frame count), and keyframe count
- Tooltip follows the cursor position
- Tooltip disappears on mouse leave

``` python
  .if >> user hovers over a shot bar camera row
      <== tooltip appears showing "WIDE ESTABLISHING\nCam A · 3.0s (72f) · 2 kf"
      <== tooltip follows the cursor
  .if >> user moves the cursor off the shot
      <== tooltip disappears
```

### Shot Boundary Playback

``` python
  .if >> playback crosses from one shot to the next
      <== the cut is instantaneous with no visual transition indicator
      !== crossfade, wipe, or other transition effect applied
      !== visual marker or flash shown at the boundary
```

---

## 3.1.5 Timeline Overview *(Deferred to Phase 8)*

***A collapsible bird's-eye view of the full timeline showing all shots, all tracks, and the current view window. Provides at-a-glance context for where you are in the sequence.***

> **Note**: This feature is deferred to Phase 8 per the roadmap. The UI specification and mockup prototype include it for layout reference. See `ui-layout-spec.md` §5 for full details.

### Layout

- 48px-tall strip positioned above the view area (below the top gutter, above the Camera View)
- Left 140px: label column reading "OVERVIEW" (consistent with all timeline label columns)
- Right side split into two vertical regions:
  - **Top 16px**: Shot blocks — one small colored rectangle per shot, labeled with shot name
  - **Bottom**: Track rows with miniature keyframe dots and linked-period region indicators
- Collapsible via the left gutter tab or keyboard shortcut

### Visual Elements

- **Shot blocks**: Colored to match shot color. Current shot at full opacity; other shots at 0.6 opacity.
- **Track rows**: One row per element track. Height adapts to fit within the available space.
- **Keyframe dots**: 3px diamond markers matching track color.
- **Link regions**: Semi-transparent grey bars for linked periods.
- **View window**: White-bordered rectangle showing the currently visible time range `[viewStart, viewEnd]`. Hidden when the entire timeline is visible.
- **Playhead**: 1px red line at the current playback position.

### Interactions

- **Click**: Move playhead to the clicked position and recenter the view window
- **Drag**: Continuously update playhead position (same as scrubbing)

---

## 3.1.3 Element Continuity — Global Element Timeline

***Elements animate on a single global timeline that spans all shots. Shots are windows into this timeline — they define which time range the camera is recording, not where elements are. Element keyframes live on the global timeline. Camera keyframes are per-shot. There is no per-shot "initial state" for elements and no continuity propagation — the global timeline IS the continuity.***

### Global Timeline Model

Elements have one continuous animation timeline. When the user creates shots, each shot maps to a time range on this global timeline. Element positions at any point in time are determined by the element's global keyframes, not by per-shot state.

- The global timeline is continuous — there are no gaps or boundaries between shots for element animation
- Element keyframes are placed on the global timeline at absolute times
- When the user views Shot 2, the view shows elements at their global timeline positions for Shot 2's time range
- Editing an element's keyframe at any point on the global timeline affects every shot that includes that time

This model matches how directors think: block the scene first (animate elements on a continuous timeline), then build camera setups around the established sequence of actions.

``` python
  .if >> user places a table at position (0, 0, 0) and creates Shot_01 (time range 0–5s)
      <== table is at (0, 0, 0) at the start of Shot_01

  .if >> user keyframes the table to (5, 0, 0) at t=5s on the global timeline
  ||> user creates Shot_02 (time range 5–10s)
      <== Shot_02 shows the table starting at (5, 0, 0) — the global timeline position at t=5s
      <== no explicit "inheritance" needed — the global timeline is the single source of truth

  .if >> user changes the table keyframe at t=5s to (3, 0, 0)
      <== Shot_02 now shows the table starting at (3, 0, 0)
      <== this is automatic — there is no propagation, just one timeline
```

### Camera vs Element Timeline

``` python
  .if >> user is viewing Shot_02 and adds a camera keyframe
      <== the camera keyframe is stored on Shot_02's camera track (per-shot)
      <== the camera keyframe does not affect any other shot's camera

  .if >> user is viewing Shot_02 and moves a table
      <== the table keyframe is stored on the global element timeline
      <== the table's position is affected in any shot whose time range includes this keyframe
```

### Shot Navigation

``` python
  .if >> user clicks Shot_01 (navigating to it)
      <== elements evaluate their global timeline at Shot_01's start time (t=0)
      <== camera loads Shot_01's camera animation at t=0
      <== the view shows the correct state for the start of Shot_01

  .if >> user clicks Shot_03 (navigating forward)
      <== elements evaluate their global timeline at Shot_03's start time
      <== camera loads Shot_03's camera animation at t=0
```

### Elements Added Mid-Sequence

``` python
  .if >> a new element is added to the scene while any shot is current
      <== the element exists on the global timeline at its placement position
      <== the element is visible in every shot (it exists in world space)
      <== the element can be keyframed at any point on the global timeline
```

### Elements Deleted

``` python
  .if >> user deletes an element from the scene
      <== element is removed from world space
      <== all keyframes for that element are removed from the global timeline
      !== element persists in earlier shots as a "ghost"
      !== application errors when navigating to shots that previously contained the element
```

### Sequence Playback Across Shots

``` python
  .if >> playback reaches the end of a shot and advances to the next shot
      <== elements continue their global timeline animation seamlessly (no reset, no jump)
      <== camera cuts to the next shot's camera animation at t=0
      <== the camera cut is instantaneous — no blending between shots
      <== element animation is continuous across the shot boundary
```

---

## 3.1.4 Slow-motion

***Per-shot speed factor. Slow-mo is a playback presentation layer — no keyframes are modified. The global element timeline plays at a different rate.***

> **Note**: Multi-scene project structure (scenes containing shots) has moved to milestone 4.2.5. See the Save / Load spec for the scene model, scene switching, scene creation, and scene deletion.

### Speed Model

- Each shot has a speed factor (default 1.0 = normal speed)
- Playback-time transform: `globalTime = shotStart + (localTime * speedFactor)`
- No keyframes are modified — slow-mo is purely a playback transformation
- A speed factor of 0.5 means the shot plays at half speed (and lasts twice as long in wall-clock time)
- A speed factor of 0.25 means quarter speed (four times as long)

### UI

- Speed percentage display on the shot or inspector (e.g., "50%" for speedFactor 0.5)
- Playback duration readout shows the wall-clock duration (shot duration / speed factor)

``` python
  .if >> shot has duration 5.0s and speed factor 0.5
      <== playback takes 10 seconds of wall-clock time
      <== the global timeline advances 5 seconds of animation time
      <== keyframe positions are unchanged — interpolation is evaluated at the transformed time

  .if >> shot has duration 5.0s and speed factor 1.0 (normal)
      <== playback takes 5 seconds
      <== no transformation applied
```

### Interaction with other systems

- **Global element timeline (3.1.3)**: Elements animate at the slowed rate. The global timeline position is computed from the speed-adjusted time.
- **Camera keyframes**: Camera animation plays at the slowed rate. No keyframe modification needed.
- **Export (4.4)**: Rendered at the slowed rate — more frames are produced for the same animation content.
- **Camera shake (1.1.6)**: Shake plays at the slowed rate (cosmetic overlay on top of the slowed playback).

---

## Acceptance Criteria

### Shot Lifecycle

1. User clicks "Add Shot" three times
   ``` python
   <== three shots exist: Shot_01, Shot_02, Shot_03
   <== Shot_03 is the current shot
   <== all world-space elements are present in every shot -- no duplication
   ```

2. User renames Shot_02 to "Over the Shoulder"
   ``` python
   <== shot thumbnail shows "Over the Shoulder"
   <== other shot names unchanged
   ```

3. User sets Shot_01 duration to 3.0s
   ``` python
   <== duration field shows "3.0s"
   <== aggregate duration updates to reflect new total
   <== keyframe editor (when present) shows 3.0s timeline length
   ```

4. User deletes Shot_02
   ``` python
   <== confirmation dialog appears
   <== after confirmation, two shots remain: Shot_01 and Shot_03
   <== Shot_03 (or Shot_01 if Shot_02 was current) is now current
   ```

### Drag-and-Drop

5. User drags Shot_03 before Shot_01
   ``` python
   <== sequence order is now: Shot_03, Shot_01
   <== thumbnails reflect the new order
   <== shot names are unchanged
   ```

6. User drags a shot while playback is active
   ``` python
   <== playback stops
   <== dragged shot is selected at t=0
   ```

### Global Element Timeline

7. Scene has a table at position (0, 0, 0). User creates Shot_01 (0–5s on global timeline).
   ``` python
   <== table is at (0, 0, 0) at the start of Shot_01
   ```

8. User keyframes table to (5, 0, 0) at t=5s on the global timeline. User creates Shot_02 (5–10s).
   ``` python
   <== Shot_02 shows table starting at (5, 0, 0) — the global timeline position at t=5s
   ```

9. User navigates back to Shot_01.
   ``` python
   <== table appears at (0, 0, 0) — the global timeline position at t=0
   ```

10. User changes the table keyframe at t=5s to (3, 0, 0).
    ``` python
    <== Shot_02 now shows table starting at (3, 0, 0) — automatic, one timeline
    ```

11. User adds a lamp to the scene while within Shot_02.
    ``` python
    <== lamp is visible in every shot including Shot_01
    <== navigating to Shot_01 shows the lamp at its placement position
    ```

12. User presses Ctrl+D on a shot.
    ``` python
    <== nothing happens -- shot duplication is not supported
    ```

### Continuous Element Animation

13. User keyframes a box at (0,0,0) at t=0, (4,0,0) at t=5, and (10,0,0) at t=10. Shot_01 covers 0–5s, Shot_02 covers 5–10s.
    - User changes the keyframe at t=5 to (2,0,0).
    ``` python
    <== Shot_01 ends with box at (2,0,0) — the keyframe changed
    <== Shot_02 starts with box at (2,0,0) — same keyframe, one timeline
    <== Shot_02 still ends at (10,0,0) — that keyframe was not changed
    ```

14. Playback crosses from Shot_01 to Shot_02.
    ``` python
    <== element animation is continuous across the shot boundary (no jump, no reset)
    <== camera cuts to Shot_02's camera animation (instantaneous cut)
    <== the element moves smoothly while the camera angle changes
    ```

### Edge Cases

15. User deletes every shot within the sequence.
    ``` python
    <== shot track shows only "Add Shot" button
    <== view shows scene within default state
    <== clicking "Add Shot" creates Shot_01 normally
    ```

16. User enters "0" for duration.
    ``` python
    <== duration clamps to 0.1s
    ```

17. User enters "999" for duration.
    ``` python
    <== duration clamps to 300.0s
    ```

18. User enters "abc" for duration.
    ``` python
    <== duration reverts to its previous value
    ```
