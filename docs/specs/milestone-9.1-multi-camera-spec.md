# Milestone 9.1: Multi-camera

**Date**: 2026-03-10
**Status**: Draft
**Blocked by**: 3.2 (Keyframe animation), 3.1 (Shot track), 6.3 (Element linking — cameras are scene entities)

---

- ### 9.1. Multi-camera (Milestone)

  Up to four cameras per shot with independent keyframe timelines, shared element animation, and an active angle track for editing camera cuts within a setup.

  This milestone is the capstone of production features. It requires mature keyframe, shot track, and scene management foundations. The design reflects how real multi-camera productions work: multiple cameras roll simultaneously on the same action, and the editor decides which angle to use moment by moment.

  The single-camera model from Milestone 1.1 remains the default. A shot with one camera works exactly as it always did. Multi-camera features surface only when the user adds additional cameras to a shot.

  ---

  - ##### 9.1.1. Per-shot camera addition (Feature)

    ***A shot starts with one camera. The user adds up to three more -- each with its own label, color, and preview. The cameras live within the shot, not the scene. Camera A is the master, Camera B is the OTS, Camera C is the CU, Camera D is the reaction.***

    *Related:
    - 1.1.1 (camera movement -- each camera has the same movement capabilities)
    - 1.1.2 (lens system -- each camera has its own focal length)
    - 3.2 (keyframe animation -- each camera has independent animation tracks)*

    **Functional requirements:**
    - Every shot begins with exactly one camera: Camera A
    - The user can add cameras to the current shot, up to a maximum of 4 per shot: A, B, C, D
    - Cameras are added via a keybind (configurable) or a UI element positioned near the frame preview area
    - Each camera is labeled with its letter (A, B, C, D) by default and assigned a color
    - Default colors: A = blue, B = red, C = green, D = yellow. Colors are user-configurable per camera
    - Cameras are renameable: right-click a camera preview element -> rename. Default names are A, B, C, D — the application does not prompt for a name on creation
    - Cameras are reorderable: the user can drag camera preview elements to rearrange their order
    - Keyboard shortcuts (Shift+1/2/3/4) correspond to camera position (first, second, third, fourth), not the camera's name. Keybindings are user-configurable, but defaults should not require configuration
    - When a shot has more than one camera, camera preview elements appear above the current shot in the shot track area
    - Each camera preview element shows a static snapshot thumbnail of that camera's current view (not a live-updating render). These previews are exact UI duplicates of shot preview thumbnails — same rendering approach, same update behavior
    - Camera preview elements are labeled with the camera letter and bordered with the camera's color
    - Left-clicking a camera preview element switches to that camera's keyframe timeline in the timeline editor
    - The currently-selected camera's preview element has a visually distinct highlight (thicker border, brighter color)
    - Adding a camera does not affect other shots -- camera count is per-shot, not project-wide
    - When a user creates a new camera, a **modal dialog** prompts for camera body and lens choice, with an option to copy settings from the active camera or any other camera in the shot. This same prompt also applies at initial project creation — the user picks a camera body and lens when creating a new project. The dialog has sensible defaults so the user can dismiss it quickly.
    - A newly added camera is positioned at a slight world-space offset from the active camera (so they are not stacked), and inherits the current view rotation and focal length as its initial state (unless overridden by the creation prompt)
    - The user can remove any camera except Camera A. Camera A cannot be removed -- at least one camera must always exist per shot
    - Removing a camera that is the active camera (9.1.3) causes Camera A to become active
    - Removing a camera that has active angle segments assigned (9.1.4) deletes those segments and follows the standard segment deletion procedure: the neighboring segment absorbs the deleted segment's duration (left neighbor absorbs; if the deleted segment is the first segment, the right neighbor absorbs). If this results in a single remaining segment, the active angle track remains. If all segments are removed, the active angle track is removed entirely.

    **Expected behavior:**
    ``` python
      # initial state -- single camera
      .if a shot is selected >>
          <== one camera exists: Camera A
          <== no camera preview elements visible in the shot track area
          <== timeline editor shows Camera A's keyframe timeline

      # adding a second camera
      .if a shot has Camera A only
      ||> .if user adds a camera >>
          <== Camera B is created
          <== Camera B's initial position, rotation, and focal length match the current view state
          <== two camera preview elements appear above the shot in the shot track area
          <== Camera A preview shows Camera A's view, Camera B preview shows Camera B's view
          <== each preview is labeled (A, B) and color-coded
          <== timeline editor switches to Camera B's keyframe timeline

      # adding cameras up to the maximum
      .if a shot has Camera A and Camera B
      ||> .if user adds a camera >>
          <== Camera C is created
          <== three camera preview elements visible
      ||> .if user adds a camera >>
          <== Camera D is created
          <== four camera preview elements visible
      ||> .if user attempts to add another camera >>
          <== addition is rejected
          !== fifth camera created
          <== status bar or tooltip indicates the maximum of 4 cameras has been reached

      # switching camera timeline via preview click
      .if a shot has Camera A, B, C
      .if timeline editor shows Camera A's keyframe timeline
      ||> .if user left-clicks Camera B's preview element >>
          <== timeline editor switches to Camera B's keyframe timeline
          <== Camera B's preview element is highlighted as selected
          <== Camera A's preview element loses its selected highlight
          <== view switches to show Camera B's perspective

      # camera colors
      .if a shot has Camera A (blue) and Camera B (red) >>
          <== Camera A's preview element has a blue border
          <== Camera B's preview element has a red border
          <== Camera A's keyframe markers in the timeline use blue
          <== Camera B's keyframe markers in the timeline use red

      # changing camera colors
      .if user changes Camera B's color from red to orange >>
          <== Camera B's preview element border updates to orange
          <== Camera B's keyframe markers update to orange
          <== Camera B's active angle segments (if any) update to orange

      # per-shot independence
      .if Shot_01 has Camera A and Camera B
      .if Shot_02 has Camera A only >>
          <== selecting Shot_01 shows two camera preview elements
          <== selecting Shot_02 shows no camera preview elements (single camera, no previews needed)
          <== adding Camera B to Shot_01 does not add Camera B to Shot_02

      # removing a camera
      .if a shot has Camera A, Camera B, Camera C
      ||> .if user removes Camera B >>
          <== Camera B is deleted from this shot
          <== Camera B's preview element disappears
          <== Camera B's keyframe timeline data is discarded
          <== two camera preview elements remain (A, C)
          <== Camera A and Camera C are unaffected

      # removing the active camera
      .if a shot has Camera A and Camera B
      .if Camera B is the active camera (9.1.3)
      ||> .if user removes Camera B >>
          <== Camera A becomes the active camera
          <== timeline editor switches to Camera A's keyframe timeline
          <== view switches to Camera A's perspective

      # cannot remove Camera A
      .if a shot has only Camera A
      ||> .if user attempts to remove Camera A >>
          <== removal is prevented
          !== shot left with zero cameras

      .if a shot has Camera A and Camera B
      ||> .if user attempts to remove Camera A >>
          <== removal is prevented
          !== Camera A removed while other cameras exist
    ```

    **Error cases:**
    ``` python
      # adding a camera with no shot selected
      .if no shot is selected >>
          <== camera addition is unavailable (greyed out or hidden)
          !== camera created without a shot context

      # rapid sequential addition
      .if user rapidly adds cameras A, B, C, D in quick succession >>
          <== all four cameras are created correctly
          <== four preview elements visible
          !== duplicate cameras or skipped letters
    ```

  ---

  - ##### 9.1.2. Multi-camera timelines (Feature)

    ***Each camera has its own independent keyframe timeline for camera motion. Element keyframe timelines are shared across all cameras in the same shot -- the characters do the same thing regardless of which camera is rolling.***

    *Related:
    - 3.2.2 (tracks and keyframes -- camera and element track model)
    - 3.2.3 (per-track stopwatch -- keyframes created when stopwatch is on)*

    **Functional requirements:**
    - Each camera within a shot has its own independent camera keyframe timeline
    - Camera keyframe timelines store: position, rotation, and focal length -- the same data as single-camera mode
    - Camera A can have a dolly move while Camera B is locked off while Camera C has a pan -- each timeline is fully independent
    - Element keyframe timelines are SHARED across all cameras in the same shot
    - There is exactly one set of element keyframes per shot, regardless of how many cameras exist
    - When the user views any camera's timeline, the element tracks display the same keyframes
    - Editing an element keyframe while viewing Camera B's timeline affects the same element keyframe data that Camera A, C, and D also reference
    - This shared model reflects real production: multiple cameras cover the same action simultaneously -- actors perform the same blocking regardless of which camera is rolling
    - When the user moves the camera (pan, tilt, dolly, etc.) with the camera stopwatch recording, the keyframe is created on the CURRENTLY SELECTED camera's timeline only
    - When the user moves an element with the element's stopwatch on (recording), the keyframe is created on the shared element timeline (visible from all camera views)
    - The timeline editor visually distinguishes which camera's keyframe track is displayed (using the camera's color)
    - When switching between camera timelines, the camera track row updates to show the selected camera's keyframes; the element track rows remain unchanged

    **Expected behavior:**
    ``` python
      # independent camera keyframes
      .if a shot has Camera A and Camera B
      .if user selects Camera A's timeline
      ||> .if user creates a dolly move with three keyframes on Camera A >>
          <== Camera A's camera track shows three keyframes
      ||> .if user clicks Camera B's preview element >>
          <== timeline editor switches to Camera B's camera track
          <== Camera B's camera track shows only the default keyframe at t=0 (no dolly move)
          <== Camera A's three keyframes are unaffected

      # shared element keyframes
      .if a shot has Camera A and Camera B
      .if a character "CharacterA" is present in the scene
      .if user selects Camera A's timeline
      ||> .if user moves CharacterA and creates a keyframe at t=1.0 >>
          <== CharacterA's element track shows a keyframe at t=1.0
      ||> .if user clicks Camera B's preview element >>
          <== Camera B's camera track is now displayed
          <== CharacterA's element track STILL shows the keyframe at t=1.0
          <== the element keyframe is the same data, not a copy

      # editing shared element keyframe from any camera view
      .if a shot has Camera A and Camera B
      .if CharacterA has a keyframe at t=1.0 (position X=5)
      .if user is viewing Camera B's timeline
      ||> .if user selects CharacterA's keyframe at t=1.0 and changes position to X=10 >>
          <== CharacterA's keyframe at t=1.0 now stores X=10
      ||> .if user switches to Camera A's timeline >>
          <== CharacterA's keyframe at t=1.0 shows X=10
          <== the edit made from Camera B's view is reflected everywhere

      # Recording targets the correct timeline
      .if a shot has Camera A and Camera B
      .if the camera stopwatch is recording
      .if Camera B is selected
      ||> .if user moves the camera (dolly forward) >>
          <== a keyframe is created on Camera B's camera track
          !== keyframe created on Camera A's camera track
      ||> .if user moves an element >>
          <== a keyframe is created on the shared element track
          <== this keyframe is visible from Camera A's view and Camera B's view

      # visual distinction between camera timelines
      .if Camera A is blue and Camera B is red
      .if user is viewing Camera A's timeline >>
          <== camera track header shows "Camera A" in blue
          <== camera keyframe markers are blue
      ||> .if user switches to Camera B's timeline >>
          <== camera track header shows "Camera B" in red
          <== camera keyframe markers are red
          <== element track headers and keyframe markers remain unchanged (green)

      # playback uses the selected camera's motion
      .if a shot has Camera A (dolly move) and Camera B (locked off)
      .if Camera A is selected
      ||> .if user plays the shot >>
          <== view shows Camera A's dolly move
      ||> .if user stops, selects Camera B, and plays again >>
          <== view shows Camera B's static framing
          <== elements animate identically in both cases (shared element keyframes)

      # four cameras, all independent
      .if a shot has Camera A, B, C, D
      .if Camera A has a dolly, Camera B is locked, Camera C has a pan, Camera D has a crane move >>
          <== each camera's timeline shows its own distinct keyframe pattern
          <== switching between them shows completely different camera track data
          <== element tracks are identical across all four views
    ```

    **Error cases:**
    ``` python
      # removing a camera with keyframes
      .if Camera B has 10 camera keyframes
      ||> .if user removes Camera B >>
          <== Camera B's camera keyframe data is discarded
          <== element keyframe data is unaffected (it is shared, not owned by Camera B)
          !== element keyframes deleted when a camera is removed

      # single camera -- degenerate case
      .if a shot has only Camera A >>
          <== camera track shows Camera A's keyframes
          <== element tracks show shared element keyframes
          <== behavior is identical to pre-multi-camera single-camera mode
          <== no change in workflow or data model from the user's perspective
    ```

  ---

  - ##### 9.1.3. Active camera and switching (Feature)

    ***One camera per shot is "active" -- the camera used during sequence playback. The director can switch the active camera at any time. Switching during playback auto-creates an active angle split.***

    *Related:
    - 9.1.1 (per-shot camera addition -- active camera must be one of the shot's cameras)
    - 9.1.4 (active angle splitting -- switching during playback creates active angle splits)*

    **Functional requirements:**
    - Each shot designates exactly one camera as "active"
    - The active camera is the camera used during sequence playback (when playing through the full sequence, the shot renders from its active camera)
    - By default, Camera A is the active camera when a shot is created
    - The user can change the active camera via right-click on a camera preview element -> "Set to active" context menu option
    - Keyboard shortcuts: Shift+1 (Camera A), Shift+2 (Camera B), Shift+3 (Camera C), Shift+4 (Camera D)
    - The active camera's preview element displays a visual indicator (e.g., a small icon or badge distinguishing it from the "selected" highlight)
    - "Active" and "selected" are independent states: the selected camera determines which timeline is shown in the editor; the active camera determines which camera renders during sequence playback
    - If an active angle track exists (9.1.4), the active angle track overrides the active camera designation -- the active angle track determines which camera renders at each moment during playback
    - If no active angle track exists, the active camera renders for the entire shot duration during playback
    - Switching the active camera during playback triggers two simultaneous effects: (1) the view switches to the new camera, and (2) an active angle split is automatically created at the current playhead position (see 9.1.4 for active angle split behavior)
    - If the shot has only one camera, the active camera is always Camera A and cannot be changed

    **Shot bar multi-camera interaction:**
    - The shot bar displays one row per camera within each shot (20px per camera row). Shot bar height auto-adjusts based on the maximum camera count across all shots.
    - **Single-click** a camera row = preview that camera (non-destructive — does not change the active camera)
    - **Double-click** a camera row (within 350ms) = activate that camera AND zoom the timeline to that shot (with 8% padding on each side)
    - Preview mode is transient — navigating to a different shot clears the preview

    **Shot bar visual states** (see `ui-layout-spec.md` §4.5 for exact colors):
    - Each shot × camera block is a colored rectangle labeled with the camera letter and shot name (e.g., "A: WIDE ESTABLISHING")
    - Four visual states communicate camera status at a glance:

    | State | Condition | Appearance |
    |-------|-----------|------------|
    | **Active camera** | This is the active camera in the current shot | Bright border, high brightness, high text opacity |
    | **Previewed** | Single-clicked for comparison, not yet activated | White text, slightly dimmed |
    | **Dimmed** | Other cameras in the same (current) shot | Reduced brightness and opacity |
    | **Inactive** | Cameras in non-current shots | Very low brightness and opacity |

    **Expected behavior:**
    ``` python
      # default active camera
      .if a shot is created >>
          <== Camera A is the active camera
          <== Camera A's preview element shows the active indicator

      # changing active camera via context menu
      .if a shot has Camera A (active) and Camera B
      ||> .if user right-clicks Camera B's preview element
      ||> .if user selects "Set to active" >>
          <== Camera B becomes the active camera
          <== Camera B's preview element shows the active indicator
          <== Camera A's preview element loses the active indicator
          <== the selected camera (for timeline editing) is unchanged

      # changing active camera via keyboard
      .if a shot has Camera A (active), Camera B, Camera C
      ||> .if user presses Shift+2 >>
          <== Camera B becomes the active camera
      ||> .if user presses Shift+3 >>
          <== Camera C becomes the active camera

      # keyboard shortcut for nonexistent camera
      .if a shot has Camera A and Camera B only
      ||> .if user presses Shift+3 >>
          <== nothing happens
          !== error or crash
          <== active camera remains unchanged

      # active vs selected distinction
      .if a shot has Camera A (active) and Camera B
      ||> .if user left-clicks Camera B's preview element >>
          <== Camera B is now SELECTED -- timeline editor shows Camera B's keyframes
          <== Camera A is still ACTIVE -- sequence playback will use Camera A
          <== Camera B's preview shows selected highlight
          <== Camera A's preview shows active indicator

      # shot bar single-click preview
      .if a shot has Camera A (active) and Camera B
      ||> .if user single-clicks Camera B's row in the shot bar >>
          <== Camera B is shown in preview mode (dimmed display)
          <== Camera B is NOT activated — sequence playback still uses Camera A
          <== the timeline editor shows Camera B's keyframes for quick inspection
      ||> .if user navigates to a different shot >>
          <== preview mode clears

      # shot bar double-click activate + zoom
      .if a shot has Camera A (active) and Camera B
      ||> .if user double-clicks Camera B's row in the shot bar >>
          <== Camera B becomes the active camera
          <== the timeline zooms to show the shot's time range (with 8% padding)
          <== preview mode clears

      # sequence playback uses active camera
      .if Shot_01 has Camera A (active) and Camera B
      .if Shot_02 has Camera A only
      ||> .if user plays the sequence from Shot_01 >>
          <== Shot_01 renders from Camera A (the active camera)
          !== Shot_01 renders from Camera B (even if Camera B is selected for editing)
          <== Shot_02 renders from Camera A

      # switching active camera during playback
      .if a shot has Camera A (active) and Camera B
      .if shot is playing back
      .if playhead is at t=2.0
      ||> .if user presses Shift+2 (switch to Camera B) >>
          <== view immediately switches to Camera B's perspective
          <== Camera B becomes the active camera
          <== an active angle track is automatically created (if not already present)
          <== the active angle track shows: Camera A from t=0 to t=2.0, Camera B from t=2.0 to end
          <== playback continues uninterrupted from Camera B's perspective

      # switching active during playback, active angle track already exists
      .if a shot has Camera A and Camera B
      .if an active angle track exists showing Camera A for the full duration
      .if shot is playing back at t=3.0
      ||> .if user presses Shift+2 >>
          <== the active angle track splits at t=3.0
          <== Camera A covers t=0 to t=3.0, Camera B covers t=3.0 to end
          <== playback continues from Camera B

      # single camera shot -- switching not applicable
      .if a shot has only Camera A
      ||> .if user presses Shift+2 >>
          <== nothing happens (Camera B does not exist)
          <== Camera A remains active
    ```

  ---

  - ##### 9.1.4. Active angle splitting (Feature)

    ***A timeline track that represents which camera is "on air" at each moment. The director splits the shot into segments, each assigned to a camera, creating the editing effect of cutting between angles within a single shot.***

    *Blocked by:
    - 9.1.1 (per-shot camera addition -- active angle track requires multiple cameras)
    - 9.1.3 (active camera -- active angle track overrides the active camera during playback)*

    *Related:
    - 3.2.1 (timeline editor -- active angle track lives in the timeline)*

    **Functional requirements:**
    - The user can toggle an "active angle" track row in the timeline editor
    - The active angle track is available only when the shot has more than one camera
    - When enabled, the active angle track appears as a horizontal bar spanning the full shot duration
    - By default, the active angle track shows a single segment colored and labeled with the active camera's color and letter, spanning the full shot duration
    - Two ways to split:
      - **Right-click → "Split angle"**: Creates a split point at the click position (snapped to the nearest frame boundary). The user is prompted to select which camera to assign to the new right-side segment. The left-side segment retains its current camera assignment.
      - **Alt+click** (quick split): Creates a split point at the click position (snapped to frame). The new right-side segment automatically cycles to the next camera: `(currentCamera + 1) % cameraCount`. No prompt — optimized for rapid cutting. Minimum 0.2s per segment after split.
    - Each segment displays its assigned camera's color and letter label
    - Cut points (split boundaries) between segments are draggable -- the user clicks and drags horizontally to adjust where the cut happens
    - Dragging a cut point snaps to frame boundaries (1/24th of a second increments at 24fps)
    - Minimum segment duration: 1 frame (1/24th of a second at 24fps)
    - A cut point cannot be dragged past an adjacent cut point -- segments cannot overlap or have zero duration below 1 frame
    - The user can delete a segment by right-clicking it -> "Delete segment"
    - When a segment is deleted, the neighboring segment to the LEFT absorbs the deleted segment's duration
    - Exception: if the FIRST (leftmost) segment is deleted, the SECOND segment absorbs its duration (expands leftward to t=0)
    - If all segments are deleted (or only one remains and is deleted), the active angle track is removed and playback reverts to using the active camera designation (9.1.3)
    - The user can reassign a segment's camera: right-click segment -> "Assign camera" -> select from available cameras
    - During playback, the active angle track determines which camera renders at each moment
    - At a cut point during playback, the view performs an instantaneous cut to the next segment's camera -- no blending, no transition
    - The active angle track is per-shot data. Each shot can have its own independent active angle track

    **Expected behavior:**
    ``` python
      # enabling the active angle track
      .if a shot has Camera A and Camera B
      ||> .if user toggles the active angle track on >>
          <== an active angle track row appears in the timeline editor
          <== the active angle track shows a single segment spanning the full shot duration
          <== the segment is colored with Camera A's color (the active camera) and labeled "A"

      # splitting active angle
      .if active angle track shows a single segment (Camera A, full duration)
      .if shot duration is 5.0 seconds
      ||> .if user right-clicks the active angle track at t=2.5 and selects "Split angle" >>
          <== two segments now exist
          <== left segment: Camera A, t=0 to t=2.5
          <== right segment: user selects a camera (e.g., Camera B)
          <== right segment: Camera B, t=2.5 to t=5.0
          <== a visible cut point appears at t=2.5

      # multiple splits
      .if active angle track has two segments (A: 0-2.5, B: 2.5-5.0)
      ||> .if user right-clicks at t=4.0 within Camera B's segment and selects "Split angle" >>
          <== three segments: A (0-2.5), B (2.5-4.0), user-assigned camera (4.0-5.0)
          <== two cut points visible: t=2.5 and t=4.0

      # dragging a cut point
      .if active angle track has segments A (0-2.5) and B (2.5-5.0)
      ||> .if user clicks and drags the cut point from t=2.5 to t=3.0 >>
          <== segment A now spans t=0 to t=3.0
          <== segment B now spans t=3.0 to t=5.0
          <== cut point snaps to frame boundaries during drag

      # dragging constrained by minimum segment duration
      .if active angle track has segments A (0-2.5), B (2.5-4.0), C (4.0-5.0)
      ||> .if user drags the first cut point rightward toward t=4.0 >>
          <== the cut point stops at t=3.958... (1 frame before the second cut point at t=4.0)
          <== segment B cannot be reduced below 1 frame duration
          !== cut point passes through or overlaps the second cut point

      # deleting a middle segment
      .if active angle track has segments A (0-2.0), B (2.0-3.5), C (3.5-5.0)
      ||> .if user right-clicks segment B and selects "Delete segment" >>
          <== segment B is removed
          <== segment A absorbs B's duration: A now spans t=0 to t=3.5
          <== segment C remains t=3.5 to t=5.0
          <== one cut point remains at t=3.5

      # deleting the first segment
      .if active angle track has segments A (0-2.0), B (2.0-3.5), C (3.5-5.0)
      ||> .if user right-clicks segment A (the leftmost) and selects "Delete segment" >>
          <== segment A is removed
          <== segment B absorbs A's duration: B now spans t=0 to t=3.5
          <== segment C remains t=3.5 to t=5.0

      # deleting the last remaining segment
      .if active angle track has a single segment (Camera A, full duration)
      ||> .if user right-clicks the segment and selects "Delete segment" >>
          <== the active angle track is removed entirely
          <== playback reverts to using the active camera (9.1.3)

      # deleting down to one segment
      .if active angle track has segments A (0-2.5) and B (2.5-5.0)
      ||> .if user deletes segment B >>
          <== segment A absorbs B's duration: A now spans t=0 to t=5.0
          <== only one segment remains, active angle track still visible
      ||> .if user deletes segment A >>
          <== active angle track is removed entirely

      # reassigning a segment's camera
      .if active angle track has segments A (0-2.5) and B (2.5-5.0)
      ||> .if user right-clicks segment A and selects "Assign camera" -> "Camera C" >>
          <== segment A is now assigned to Camera C
          <== segment A's color and label update to Camera C's color and "C"

      # playback with active angle track
      .if active angle track has segments A (0-2.0), B (2.0-3.5), A (3.5-5.0)
      ||> .if user plays the shot from the beginning >>
          <== t=0 to t=2.0: view renders from Camera A
          <== at t=2.0: cut, view switches to Camera B
          <== t=2.0 to t=3.5: view renders from Camera B
          <== at t=3.5: cut, view switches back to Camera A
          <== t=3.5 to t=5.0: view renders from Camera A
          !== any blending, crossfade, or transition between cameras

      # active angle track unavailable for single camera
      .if a shot has only Camera A >>
          <== active angle track toggle is disabled or hidden
          !== active angle track shown for a single-camera shot

      # scrubbing through active angle track
      .if active angle track has segments A (0-2.0) and B (2.0-5.0)
      ||> .if user drags the playhead from t=1.0 to t=3.0 >>
          <== view switches from Camera A to Camera B as the playhead crosses t=2.0
          <== the switch is immediate at the cut point
    ```

    **Error cases:**
    ``` python
      # removing a camera that has active angle segments
      .if active angle track has segments A (0-2.0), B (2.0-3.5), A (3.5-5.0)
      ||> .if user removes Camera B from the shot (9.1.1) >>
          <== Camera B's segment (2.0-3.5) is deleted
          <== the left neighbor (segment A, 0-2.0) absorbs the deleted segment's duration
          <== active angle track now shows: A (0-3.5), A (3.5-5.0)
          <== adjacent segments with the same camera MAY be automatically merged: A (0-5.0)

      # split at exact start or end of shot
      .if user right-clicks the active angle track at t=0.0 and selects "Split angle" >>
          <== split is rejected (cannot create a zero-duration segment at the start)
          !== crash or invalid state

      .if user right-clicks the active angle track at the last frame and selects "Split angle" >>
          <== split is rejected (cannot create a zero-duration segment at the end)
    ```

  ---

  - ##### 9.1.5. Multi-split (Feature)

    ***Rapidly divide the active angle track into evenly-spaced segments. "Split every 12 frames" creates a rhythmic cutting pattern that the director can then assign cameras to.***

    *Blocked by:
    - 9.1.4 (active angle splitting -- multi-split creates active angle splits)*

    **Functional requirements:**
    - Right-click the active angle track -> "Multi-split" opens a prompt
    - The prompt asks: "Split every {n} frames" where the user enters a positive integer
    - Multi-split creates evenly-spaced cut points every n frames across the full shot duration
    - If the shot duration is not evenly divisible by n, the final segment is shorter than n frames (the remainder)
    - By default, all segments created by multi-split are assigned to the SAME camera as the active camera -- no visual change until the user assigns different cameras to individual segments
    - Multi-split replaces any existing active angle segments -- it is a destructive operation on the active angle track
    - The user is warned before multi-split if existing active angle segments will be overwritten: "This will replace your current active angle layout. Continue?"
    - Minimum value for n: 1 (every frame is a cut -- extreme but valid)
    - Maximum value for n: total shot duration in frames (results in a single segment, effectively no splits)
    - If the user enters 0 or a negative number, the input is rejected
    - After multi-split, the user can drag cut points, delete segments, and reassign cameras using the standard active angle editing tools (9.1.4)

    **Expected behavior:**
    ``` python
      # basic multi-split
      .if a shot is 5.0 seconds (120 frames at 24fps)
      .if active angle track is enabled
      ||> .if user right-clicks active angle track -> "Multi-split" -> enters 24 >>
          <== 5 segments are created, each 24 frames (1 second) long
          <== all segments are assigned to the active camera (e.g., Camera A)
          <== 4 cut points visible at t=1.0, t=2.0, t=3.0, t=4.0

      # multi-split with remainder
      .if a shot is 100 frames
      ||> .if user multi-splits every 30 frames >>
          <== 4 segments: 30 frames, 30 frames, 30 frames, 10 frames
          <== 3 cut points visible
          <== the final segment is 10 frames (the remainder)

      # multi-split replaces existing active angle layout
      .if active angle track has segments A (0-2.0) and B (2.0-5.0) with manual edits
      ||> .if user triggers multi-split >>
          <== warning: "This will replace your current active angle layout. Continue?"
      ||> .if user confirms >>
          <== existing segments are replaced with the new evenly-spaced segments
          !== old segments preserved or merged

      .if user cancels the warning >>
          <== multi-split is aborted
          <== existing active angle segments are unchanged

      # assigning cameras after multi-split
      .if multi-split created 5 segments, all Camera A
      ||> .if user right-clicks segment 2 -> "Assign camera" -> Camera B
      ||> .if user right-clicks segment 4 -> "Assign camera" -> Camera B >>
          <== pattern is now: A, B, A, B, A
          <== playback cuts between Camera A and Camera B every second

      # multi-split with n = 1
      .if a shot is 120 frames
      ||> .if user multi-splits every 1 frame >>
          <== 120 segments, each 1 frame long
          <== 119 cut points
          <== all segments assigned to the active camera
          <== this is extreme but valid -- useful for experimentation

      # multi-split with n = total duration
      .if a shot is 120 frames
      ||> .if user multi-splits every 120 frames >>
          <== 1 segment spanning the full duration
          <== no cut points
          <== effectively resets active angle track to single-segment default

      # invalid input
      .if user enters 0 in the multi-split prompt >>
          <== input is rejected
          <== prompt remains open or shows an error message
          !== active angle track modified

      .if user enters -5 in the multi-split prompt >>
          <== input is rejected

      .if user enters a non-integer (e.g., "abc") >>
          <== input is rejected
    ```

---

## Acceptance Criteria

### Setting Up a Two-Camera Shot

1. User selects Shot_01 (single camera by default)
   ``` python
     .if user selects Shot_01 (single camera by default) >>
         <== Camera A exists, no camera preview elements visible
   ```

2. User adds a camera
   ``` python
     .if user adds a camera >>
         <== Camera B is created
         <== two camera preview elements appear above Shot_01 in the shot track area
         <== Camera A preview shows its view, Camera B preview shows its view
         <== previews are labeled A and B with distinct colors
   ```

### Setting Up a Four-Camera Shot

3. Starting from a two-camera shot, user adds two more cameras
   ``` python
     .if user adds two more cameras from a two-camera shot >>
         <== Camera C and Camera D are created
         <== four camera preview elements visible, labeled A, B, C, D
         <== each preview shows its camera's live view with its assigned color
   ```

4. User attempts to add a fifth camera
   ``` python
     .if user attempts to add a fifth camera >>
         <== addition is rejected
         <== maximum of 4 cameras enforced
   ```

### Switching Between Camera Timelines

5. User left-clicks Camera B's preview element
   ``` python
     .if user left-clicks Camera B's preview element >>
         <== timeline editor switches to Camera B's keyframe timeline
         <== Camera B's preview is highlighted as selected
         <== view shows Camera B's perspective
   ```

6. User left-clicks Camera D's preview element
   ``` python
     .if user left-clicks Camera D's preview element >>
         <== timeline editor switches to Camera D's keyframe timeline
         <== view shows Camera D's perspective
   ```

### Element Keyframes Shared, Camera Keyframes Independent

7. User selects Camera A's timeline and creates a dolly move with 4 keyframes
   ``` python
     .if user selects Camera A's timeline and creates a dolly move with 4 keyframes >>
         <== Camera A's camera track shows 4 keyframes
   ```

8. User switches to Camera B's timeline
   ``` python
     .if user switches to Camera B's timeline >>
         <== Camera B's camera track shows only the default keyframe (no dolly)
         <== Camera A's dolly keyframes are not visible on Camera B's camera track
   ```

9. User moves an element (CharacterA) while viewing Camera B's timeline
   ``` python
     .if user moves an element (CharacterA) while viewing Camera B's timeline >>
         <== element keyframe appears on CharacterA's track
   ```

10. User switches back to Camera A's timeline
    ``` python
      .if user switches back to Camera A's timeline >>
          <== CharacterA's element track shows the same keyframe created in step 9
          <== Camera A's dolly keyframes are still present on the camera track
    ```

### Active Camera During Playback

11. User sets Camera B as active (right-click -> "Set to active") and plays the sequence
    ``` python
      .if user sets Camera B as active and plays the sequence >>
          <== Shot renders from Camera B's perspective during playback
          <== Camera A's perspective is not used for playback even if Camera A is selected for editing
    ```

12. User presses Shift+1 while playback is stopped
    ``` python
      .if user presses Shift+1 while playback is stopped >>
          <== Camera A becomes the active camera
          <== next playback uses Camera A
    ```

### Active Angle Splitting Workflow

13. User enables the active angle track on a two-camera shot
    ``` python
      .if user enables the active angle track on a two-camera shot >>
          <== active angle track appears in the timeline
          <== single segment spanning full duration, colored with the active camera's color
    ```

14. User right-clicks active angle track at t=2.0 and selects "Split angle", assigns Camera B to the right segment
    ``` python
      .if user right-clicks active angle track at t=2.0 and selects "Split angle", assigns Camera B to the right segment >>
          <== two segments: Camera A (0-2.0), Camera B (2.0-5.0)
          <== cut point visible at t=2.0
    ```

15. User drags the cut point from t=2.0 to t=3.0
    ``` python
      .if user drags the cut point from t=2.0 to t=3.0 >>
          <== segment A expands to 0-3.0, segment B contracts to 3.0-5.0
    ```

16. User creates another split at t=4.0 within segment B, assigns Camera A
    ``` python
      .if user creates another split at t=4.0 within segment B, assigns Camera A >>
          <== three segments: A (0-3.0), B (3.0-4.0), A (4.0-5.0)
    ```

17. User deletes the middle segment (Camera B, 3.0-4.0)
    ``` python
      .if user deletes the middle segment (Camera B, 3.0-4.0) >>
          <== first segment A absorbs: A (0-4.0), A (4.0-5.0)
    ```

18. User plays the shot with active angle track active
    ``` python
      .if user plays the shot with active angle track active >>
          <== view renders from the camera assigned to each active angle segment at each moment
          <== cuts at cut points, no transitions
    ```

### Multi-Split Workflow

19. User right-clicks active angle track -> "Multi-split" -> enters 12
    ``` python
      .if user right-clicks active angle track -> "Multi-split" -> enters 12 >>
          <== evenly-spaced segments every 12 frames (0.5 seconds at 24fps)
          <== all segments assigned to the active camera
    ```

20. User assigns alternating cameras: A, B, A, B, A, B...
    ``` python
      .if user assigns alternating cameras: A, B, A, B, A, B... >>
          <== active angle track shows alternating colors
          <== playback cuts between Camera A and Camera B every 0.5 seconds
    ```

### Switching Cameras During Playback Auto-Creating Active Angle Split

21. User plays a shot with Camera A active and no active angle track
    ``` python
      .if user plays a shot with Camera A active and no active angle track >>
          <== shot renders from Camera A
    ```

22. User presses Shift+2 at t=2.5 during playback
    ``` python
      .if user presses Shift+2 at t=2.5 during playback >>
          <== view switches to Camera B immediately
          <== active angle track is automatically created
          <== active angle track shows: Camera A (0-2.5), Camera B (2.5-end)
          <== playback continues from Camera B
    ```

23. User presses Shift+1 at t=4.0 during continued playback
    ``` python
      .if user presses Shift+1 at t=4.0 during continued playback >>
          <== view switches back to Camera A
          <== active angle track adds another split: A (0-2.5), B (2.5-4.0), A (4.0-end)
    ```

### Edge Cases

24. User removes Camera B from a shot that has active angle segments assigned to Camera B
    ``` python
      .if user removes Camera B from a shot that has active angle segments assigned to Camera B >>
          <== Camera B's active angle segments are deleted; neighbor segments absorb the duration (standard delete procedure)
          <== adjacent segments with the same camera may be merged automatically
    ```

25. User removes Camera B from a shot where Camera B is the active camera
    ``` python
      .if user removes Camera B from a shot where Camera B is the active camera >>
          <== Camera A becomes the active camera
          <== Camera B's keyframe data is discarded
          <== element keyframe data is unaffected
    ```

26. Single-camera shot: active angle track toggle is disabled
    ``` python
      .if single-camera shot: active angle track toggle is disabled >>
          <== user cannot enable active angle track with only one camera
          <== timeline editor shows Camera A's keyframes exactly as in pre-multi-camera behavior
    ```

27. Single-camera shot: Shift+2, Shift+3, Shift+4 do nothing
    ``` python
      .if single-camera shot: user presses Shift+2, Shift+3, or Shift+4 >>
          <== no error, no crash, active camera remains Camera A
    ```

28. User creates a shot with 4 cameras, animates all 4 camera timelines independently, sets up complex active angle splits, then removes Camera C
    ``` python
      .if user creates a shot with 4 cameras, animates all 4 camera timelines independently, sets up complex active angle splits, then removes Camera C >>
          <== Camera C's camera keyframes are discarded
          <== Camera C's active angle segments are reassigned to Camera A
          <== Camera A, B, and D's camera keyframes are unaffected
          <== shared element keyframes are unaffected
          <== active angle track remains functional with remaining cameras
    ```
