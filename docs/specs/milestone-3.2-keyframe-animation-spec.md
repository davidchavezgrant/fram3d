# Milestone 3.2: Keyframe Animation — Specification

**Date**: 2026-03-10
**Milestone**: 3.2
**Status**: Draft

---

- ### 3.2. Keyframe animation (Milestone)

  Multi-track timeline editor for camera and element animation within each shot. This is the heartbeat of previsualization — the system that turns a static camera position into a moving shot.

  Directors think about time linearly: this happens, then that happens. The timeline must communicate that mental model without requiring animation expertise. Dots on a line, left to right, each one a moment where something changes. Move the camera and a dot appears. Drag the dot to change when it happens. Press play and watch the shot unfold. Everything else is implementation — the user sees cause and effect.

  The track system follows an After Effects / Premiere model: each track is collapsible, revealing per-property sub-tracks underneath. When collapsed, keyframes appear as unified "main" dots — one dot per moment in time, containing all property values. When expanded, each property gets its own sub-track row with independent keyframes. This parent/child relationship is the architectural core of the system. Main keyframes own their children. Moving a main keyframe moves all its children. But a child can break free — dragging a property keyframe to a new time detaches it from its parent and creates a new main keyframe at the destination.

  The per-track stopwatch model (After Effects / Premiere) controls when keyframes are created. Each track has a stopwatch that toggles between not recording (off — manipulations change values without keyframing) and recording (on — manipulations create/update keyframes). All stopwatches default to off. This gives the director explicit control: set up the scene while not recording, then turn on the stopwatch when ready to animate. The first manipulation with the stopwatch on creates the initial keyframe.

  Camera rotation is expressed in cinematic terms — pan, tilt, and roll in degrees — not quaternions. A director says "pan left 45 degrees" or "tilt up 10." The UI displays and accepts pan/tilt/roll in degrees, but **internally all rotation is stored and interpolated as quaternions**. This is an implementation detail that prevents gimbal lock without any user-visible limitation. At the 90-degree tilt edge case (looking straight up or down), pan and roll become mathematically equivalent — quaternion interpolation handles this transparently, producing smooth motion through the singularity. The user never encounters gimbal lock artifacts.

  *Blocked by: 3.1 (shot track — keyframes exist within shots)*

  - ##### 3.2.1. Timeline editor (Feature)

    ***A three-region panel that communicates time, control, and context — transport bar for playback, track area for tracks and keyframes, status bar for contextual keyboard hints.***

    **Functional requirements:**
    - The timeline editor sits directly above the shot track
    - The editor is divided into three horizontal regions: transport bar (top), track area (middle), status bar (bottom)
    - The transport bar contains: play/pause button, current time display (seconds.tenths), shot duration display, and a shot name label
    - The track area contains: a horizontal time ruler, vertical track lanes, keyframe markers, and a playhead
    - The time ruler shows second markers and subdivisions, scaled to the shot duration
    - The time ruler rescales dynamically when the shot duration changes
    - The user can manually zoom/scale the time ruler (scroll wheel or pinch) to focus on a region of interest
    - A Premiere-style zoom scroll bar sits below the track area. It contains a draggable thumb representing the visible time range. Dragging the thumb edges resizes the visible range (zoom). Dragging the thumb body pans. A playhead position indicator within the zoom bar shows the current time relative to the full shot duration. Clicking the zoom bar (not on the thumb) navigates to that position.
    - Timecode format: non-drop-frame (`:` separator) for 24/25/30fps projects, drop-frame (`;` separator) for 29.97/59.94fps projects. Format determined by the project frame rate setting.
    - **Dual timecode display**: the transport bar shows shot-local elapsed / duration. A separate overlay shows sequence-global timecode. Format: semicolon-separated `HH;MM;SS;FF`.
    - **Camera View overlays** (see `ui-layout-spec.md` §2.3 for full layout):
      - **Shot label** (top-left): Shows shot number, camera letter, name, and duration. Example: "Shot 3A: OTS DET→WIT (3.5s)". Updates on shot/camera changes.
      - **Sequence timecode** (bottom-center): Shows the global timecode position across all shots. Updates every frame during playback and scrubbing.
    - The playhead is a vertical red line spanning every track, indicating the current time
    - The status bar displays contextual keyboard shortcut hints based on current state and selection
    - The timeline editor is always visible when a shot is selected
    - The timeline editor updates its contents when the user switches between shots
    - **Timeline resize**: a vertical drag handle (5px) between the view area and the timeline allows the user to resize the timeline height. Default 320px. Min 80px, max 80vh.
    - **Timeline layout constants** (see `ui-layout-spec.md` for full visual details):
      - All timeline rows share a 140px left label column for track names, consistent across ruler, shot track, active angle track, track area, and zoom bar
      - Main track rows: 28px height
      - Sub-track rows (expanded properties): 22px height
      - Ruler: 22px height
      - Active angle track: 24px height
      - Zoom bar: 18px height
      - Scene tabs: 26px height
      - Transport bar: 28px height
    - **Timeline input mappings**: scroll wheel zooms at cursor position. Shift+scroll pans horizontally. Middle-click drag pans the track area and shot track. `\` key zooms to fit the full timeline. Home/End keys jump to start/end of timeline.

    **Expected behavior:**
    ``` python
      # initial state
      .if a shot is selected
      .if the timeline editor is visible >>
          <== the transport bar shows shot name, current time 0.0, and shot duration
          <== the track area shows the camera track with at least one main keyframe
          <== the playhead is positioned at time 0.0
          <== the status bar shows relevant keyboard hints

      # switching shots
      .if the user selects a different shot >>
          <== the timeline editor displays the new shot's tracks and keyframes
          <== the playhead resets to time 0.0
          <== the transport bar updates to show the new shot's name and duration

      # time ruler rescales with shot duration
      .if the shot duration is 3 seconds >>
          <== the time ruler spans 0.0 to 3.0
      ||> .if the shot duration is changed to 8 seconds >>
          <== the time ruler dynamically rescales to span 0.0 to 8.0
          <== existing keyframes reposition proportionally along the wider ruler

      # manual ruler zoom
      .if the time ruler shows 0.0 to 5.0 >>
          <== the full shot duration is visible
      ||> .if the user zooms in on the ruler >>
          <== the ruler shows a narrower time range (e.g., 1.0 to 3.0)
          <== keyframes within the visible range are spaced further apart for finer control
          <== the user can scroll/pan the ruler to see other time regions
      ||> .if the user zooms out >>
          <== the ruler shows a wider time range, up to the full shot duration
          !== the ruler zooms beyond the shot duration

      # status bar reflects context
      .if no keyframe is selected >>
          <== the status bar shows general shortcuts (e.g., "Space: Play/Pause", "C: Add Camera Keyframe")
      ||> .if a main keyframe is selected >>
          <== the status bar shows keyframe shortcuts (e.g., "Delete: Remove Keyframe", "Drag: Move in Time")
      ||> .if a property keyframe is selected (track expanded) >>
          <== the status bar shows property keyframe shortcuts (e.g., "Delete: Remove Property Keyframe", "Drag: Detach to New Time")
    ```

  - ##### 3.2.2. Tracks and keyframes (Feature)

    ***Camera track (yellow) always present, element tracks (green) per animated element. Each track is collapsible, revealing per-property sub-tracks. Camera properties: position XYZ, pan, tilt, roll, focal length (zoom lenses only). Element properties: position XYZ, scale, rotation XYZ. Collapsed view shows unified main keyframes; expanded view shows individual property keyframes.***

    **Functional requirements:**
    - Every shot has exactly one camera track, displayed using yellow keyframe markers
    - Element tracks are displayed using green keyframe markers, one track per animated element
    - Element tracks appear only when an element has at least one keyframe within the current shot
    - Each track displays its name: "Camera" for the camera track, the element's name for element tracks
    - Each track has a collapsible dropdown arrow that toggles between collapsed and expanded views
    - Tracks are collapsed by default

    **Camera property sub-tracks (visible when camera track is expanded):**
    - Position X — current value displayed
    - Position Y — current value displayed
    - Position Z — current value displayed
    - Pan — single value in degrees (can be negative, e.g., -45 means pan left 45 degrees)
    - Tilt — single value in degrees (can be negative, e.g., -10 means tilt down 10 degrees)
    - Roll — single value in degrees (can be negative, e.g., -15 means roll counterclockwise)
    - Focal Length — only visible when the selected lens preset is a zoom lens; hidden when a prime lens set is selected
    - Focus Distance — distance to focus target in meters. Enables rack focus (keyframing focus transitions between subjects). Updates automatically when focus-on-object is used (1.1.4).
    - Aperture — f-stop value (f/1.4, f/2, f/2.8, etc.). Controls depth of field when DOF preview is active (1.1.5).

    **Element property sub-tracks (visible when element track is expanded):**
    - Position X — current value displayed
    - Position Y — current value displayed
    - Position Z — current value displayed
    - Scale — single positive value (uniform scale only, e.g., 1.0 = default, 2.0 = double size)
    - Rotation X — value in degrees
    - Rotation Y — value in degrees
    - Rotation Z — value in degrees

    **Parent/child keyframe model:**
    - A **main keyframe** is a unified marker on the collapsed track view at a specific time. It contains values for all properties at that time.
    - A **property keyframe** (child) is a marker on an individual property sub-track. Each main keyframe has one child per property.
    - When collapsed: only main keyframe markers are visible — one dot per time position
    - When expanded: each property sub-track row shows its own keyframe markers independently
    - When the track is recording, manipulations create main keyframes containing all changed property values at that moment. Only changed properties get keyframed.
    - A selected keyframe is highlighted using a distinct color (cyan)
    - Selecting a keyframe on one track deselects any keyframe on every other track — only one keyframe is selected at a time
    - The view reflects the state at the selected keyframe's time
    - Each property sub-track row displays the **live interpolated value** of that property at the playhead position (e.g., `Position (1.2, 0.9, -1.5)`). Values update in real time during scrub and playback.

    **Expected behavior:**
    ``` python
      # camera track is always present
      .if a new shot is created >>
          <== the camera track is visible in collapsed view
          <== the camera track has at least one main keyframe at time 0.0
          <== the camera track label reads "Camera"
          <== the camera keyframe marker is yellow

      # expanding a track reveals property sub-tracks
      .if the user clicks the camera track's dropdown arrow >>
          <== the camera track expands to show property sub-track rows
          <== sub-tracks are labeled: Position X, Position Y, Position Z, Pan, Tilt, Roll
          <== each sub-track row shows keyframe markers at the same times as the main keyframes
          <== each sub-track row displays its current value at the playhead position
      ||> .if the selected lens preset is a zoom lens >>
          <== a Focal Length sub-track row is also visible
      ||> .if the selected lens preset is a prime lens set >>
          !== a Focal Length sub-track row is visible

      # collapsing a track hides property sub-tracks
      .if the camera track is expanded
      .if the user clicks the dropdown arrow again >>
          <== the property sub-track rows are hidden
          <== only the main keyframe markers are visible on the camera track

      # element track appears on first animation
      .if no element tracks exist
      .if the user moves an element while the track is recording >>
          <== an element track appears labeled with the element's name
          <== the element track has a green main keyframe marker at the current time

      # expanding an element track
      .if the user clicks an element track's dropdown arrow >>
          <== the element track expands to show: Position X, Position Y, Position Z, Scale, Rotation X, Rotation Y, Rotation Z
          <== each sub-track row shows keyframe markers at the times corresponding to main keyframes

      # element track removal
      .if an element track has keyframes
      .if the user deletes every keyframe on that track >>
          <== the element track is removed from the timeline

      # single selection across tracks
      .if a camera keyframe is selected
      .if the user clicks an element keyframe >>
          <== the element keyframe becomes selected (cyan)
          <== the camera keyframe is deselected (returns to yellow)
          <== only one keyframe is selected across every track

      # selecting a main keyframe shows its state
      .if the user clicks a camera main keyframe at time 2.0 >>
          <== the view shows the camera at the position, pan, tilt, roll, and focal length stored within that keyframe
          <== the playhead moves to time 2.0

      # selecting a property keyframe
      .if the camera track is expanded
      .if the user clicks the Pan property keyframe at time 2.0 >>
          <== the property keyframe is highlighted cyan
          <== the playhead moves to time 2.0
          <== the view shows the full scene state at time 2.0 (not just the pan value)

      # main keyframe data fidelity
      .if the camera is at position (1, 2, 3) with pan 45, tilt -10, roll 0, and focal length 50mm
      .if a main keyframe is created >>
          <== the main keyframe stores all values: position (1, 2, 3), pan 45, tilt -10, roll 0, focal length 50
          <== each property sub-track child stores its individual value
      ||> .if the user selects that keyframe later >>
          <== the camera restores to position (1, 2, 3), pan 45, tilt -10, roll 0, focal length 50mm

      # property sub-track values update with playhead
      .if the camera track is expanded
      .if the playhead moves to time 1.5
      .if position X interpolates to 3.7 at time 1.5 >>
          <== the Position X sub-track row displays "3.7" as its current value
          <== all other property sub-track rows display their interpolated values at time 1.5

      # focal length visibility changes with lens preset
      .if the camera track is expanded
      .if the user switches from a prime lens set to a zoom lens >>
          <== the Focal Length sub-track row appears
      ||> .if the user switches from a zoom lens to a prime lens set >>
          <== the Focal Length sub-track row disappears
          <== any focal length keyframe data is preserved internally (not deleted)
    ```

    **Edge cases:**
    ``` python
      # many properties at one time — collapsed view clarity
      .if a main keyframe exists at time 1.0 with all property values set
      .if the track is collapsed >>
          <== exactly one main keyframe dot is visible at time 1.0
          !== multiple overlapping dots appear

      # main keyframe with some children removed (after property keyframe detachment)
      .if a main keyframe at time 1.0 originally had all children
      .if the Pan child was moved to time 2.0 (detached)
      .if the track is collapsed >>
          <== a main keyframe dot is still visible at time 1.0 (remaining children exist)
      ||> .if the track is expanded >>
          <== the Pan sub-track row has no keyframe at time 1.0
          <== all other property sub-track rows still have keyframes at time 1.0
    ```

  - ##### 3.2.3. Per-track stopwatch (Feature)

    ***Each track has a stopwatch toggle controlling whether manipulations create keyframes (recording) or just change values (not recording). After Effects / Premiere model. Defaults to off. This replaces "always-on" recording with explicit per-track control — the director decides when to start recording animation.***

    **Functional requirements:**
    - Each track (camera and every element track) has a stopwatch icon in the track header
    - The stopwatch toggles between two states:
      - **Off (not recording)**: Manipulations change property values without creating or modifying keyframes. The director is positioning things, not animating.
      - **On (recording)**: Manipulations create or update keyframes at the current playhead time. The director is recording animation.
    - All stopwatches default to off when the application starts and when a new shot is created
    - Clicking the top-level track stopwatch enables/disables the stopwatch for ALL child property sub-tracks on that track
    - Individual property sub-track stopwatches can be toggled independently when the track is expanded
    - If any child property stopwatch is on, the parent track stopwatch shows as on (partial state indicator if not all children are on)

    **Recording — keyframe creation behavior (stopwatch on):**
    - The first manipulation after turning the stopwatch on creates an initial keyframe at the playhead position, capturing all current property values
    - Subsequent manipulations create main keyframes containing only the changed property values
    - If the current time is within 0.1 seconds of an existing keyframe on the same track, that keyframe is updated with the new values instead of creating a new keyframe
    - If the current time is not within 0.1 seconds of any existing keyframe on that track, a new main keyframe is created
    - "Manipulation" means the user is directly interacting via mouse/keyboard input — not the result of animation playback, timeline scrubbing, or keyframe selection
    - Keyframes are NOT created during playback, even if the stopwatch is on. The C and V manual keyframe shortcuts are also disabled during playback.
    - Keyframes are NOT created during timeline scrubbing, keyframe selection, shot switching, or animation evaluation
    - Change detection thresholds prevent micro-movements from triggering keyframes:
      - Position: movement greater than 0.001 units (per axis)
      - Pan/Tilt/Roll: rotation greater than 0.01 degrees
      - Focal length: change greater than 0.01mm
      - Scale: change greater than 0.001 units
      - Element rotation: change greater than 0.01 degrees (per axis)
    - When recording creates the first keyframe on an element track, the track appears in the timeline

    **Not recording — no keyframes (stopwatch off):**
    - Manipulations change the live property values in the view
    - No keyframes are created or modified
    - Existing keyframes on the track are unaffected — the director can scrub and see them, but moving the camera or objects doesn't record anything
    - Not recording is for positioning, framing, and experimentation before committing to animation

    **Turning the stopwatch off (disabling recording):**
    - If the track has existing keyframes, a confirmation dialog warns: "Turning off the stopwatch will delete all keyframes on this track. Continue?"
    - If confirmed, ALL keyframes on that track are deleted
    - If cancelled, the stopwatch remains on
    - The "don't show again" option is available on the confirmation dialog

    **Expected behavior:**
    ``` python
      # default state — stopwatch off
      .if a new shot is created >>
          <== the camera track stopwatch is off (not recording)
          <== manipulating the camera changes the view without creating keyframes

      # turning stopwatch on — first manipulation creates initial keyframe
      .if the camera stopwatch is off
      .if the user turns the camera stopwatch on
      .if the user dollies the camera forward >>
          <== an initial camera main keyframe is created at the current playhead time
          <== the keyframe captures all current camera properties
          <== a yellow marker appears on the camera track

      # subsequent manipulations with stopwatch on
      .if the camera stopwatch is on
      .if the playhead is at time 2.0
      .if no camera keyframe exists near time 2.0
      .if the user pans the camera >>
          <== a new camera main keyframe is created at time 2.0
          <== only the changed properties are keyframed
          <== the keyframe marker appears at time 2.0

      # updating an existing nearby keyframe
      .if the camera stopwatch is on
      .if a camera keyframe exists at time 2.0
      .if the playhead is at time 2.05 (within 0.1s)
      .if the user pans the camera >>
          <== the existing keyframe at time 2.0 is updated with the new pan value
          <== all other property values in that keyframe are preserved
          !== a new keyframe is created at time 2.05

      # stopwatch off — no keyframes created
      .if the camera stopwatch is off
      .if the user dollies, pans, and tilts the camera >>
          <== the view updates to reflect the camera movement
          !== any keyframes are created or modified
          !== any markers appear on the timeline

      # element track stopwatch
      .if an element's stopwatch is off
      .if the user moves the element >>
          <== the element moves in the view
          !== a keyframe is created
      ||> .if the user turns the element's stopwatch on
          .if the user moves the element >>
          <== an element main keyframe is created at the current time
          <== a green marker appears on the element's track

      # turning stopwatch off — warning and deletion
      .if the camera stopwatch is on
      .if the camera track has 5 keyframes
      .if the user clicks the stopwatch to turn it off >>
          <== a confirmation dialog appears: "Turning off the stopwatch will delete all keyframes on this track. Continue?"
      ||> .if the user confirms >>
          <== all 5 camera keyframes are deleted
          <== the stopwatch is now off (not recording)
      ||> .if the user cancels >>
          <== the stopwatch remains on
          <== all keyframes are preserved

      # turning stopwatch off on track with no keyframes — no warning
      .if the camera stopwatch is on
      .if no keyframes have been created yet
      .if the user clicks the stopwatch to turn it off >>
          <== the stopwatch turns off immediately
          !== a confirmation dialog appears

      # top-level stopwatch enables all children
      .if the camera track is expanded
      .if the user clicks the top-level camera stopwatch on >>
          <== all property sub-track stopwatches (Position X, Y, Z, Pan, Tilt, Roll, etc.) turn on
      ||> .if the user clicks the top-level camera stopwatch off >>
          <== all property sub-track stopwatches turn off
          <== the keyframe deletion warning applies to all tracks

      # individual property stopwatch override
      .if the camera track is expanded
      .if the top-level stopwatch is on (all children on)
      .if the user turns off just the Pan sub-track stopwatch >>
          <== the Pan sub-track stops recording (pan changes don't create keyframes)
          <== all other sub-tracks remain recording
          <== the top-level stopwatch shows a partial state indicator

      # first element keyframe creates track
      .if no element track exists for "Chair"
      .if the user turns on "Chair"'s stopwatch and moves it >>
          <== an element track labeled "Chair" appears in the timeline
          <== a green main keyframe marker appears at the current time
    ```

    **Critical: no keyframe creation during playback (regardless of stopwatch state):**
    ``` python
      # animation playback — stopwatch on, no keyframes
      .if the camera stopwatch is on
      .if playback is active
      .if the camera moves due to animation evaluation >>
          !== a new keyframe is created
          !== any existing keyframe is modified

      # C and V keys disabled during playback
      .if playback is active
      .if the user presses C >>
          !== a camera keyframe is created
      .if the user presses V >>
          !== an element keyframe is created
      <== playback continues without interruption

      # all keyframe creation suppressed during playback
      .if playback is active >>
          <== no keyframe creation occurs regardless of stopwatch state or input
    ```

    **Critical: distinguishing manual input from system evaluation:**
    ``` python
      # scrubbing does not create keyframes
      .if the camera stopwatch is on
      .if the user clicks the timeline to scrub to time 3.0
      .if the camera and objects update to reflect time 3.0 >>
          !== a new keyframe is created
          !== any existing keyframe is modified

      # selecting a keyframe does not create keyframes
      .if the camera stopwatch is on
      .if the user clicks a keyframe at time 1.0
      .if the view updates to show the state at time 1.0 >>
          !== a new keyframe is created at time 1.0
          !== the clicked keyframe's values are overwritten

      # evaluating after keyframe drag does not create keyframes
      .if the user drags a keyframe from time 1.0 to time 2.0
      .if the system re-evaluates the animation >>
          !== additional keyframes are created as a side effect

      # switching shots does not create keyframes
      .if the user switches from Shot_01 to Shot_02
      .if the view updates to show Shot_02's initial state >>
          !== keyframes are created in either shot
    ```

    **Critical: threshold and noise filtering:**
    ``` python
      # micro-movements below threshold are ignored
      .if the camera stopwatch is on
      .if the camera position changes by 0.0005 units on one axis (below 0.001 threshold) >>
          !== a keyframe is created or updated

      # rotation below threshold is ignored
      .if the camera stopwatch is on
      .if the camera pan changes by 0.005 degrees (below 0.01 threshold) >>
          !== a keyframe is created or updated

      # accumulated small movements that cross threshold do trigger
      .if the camera stopwatch is on
      .if the camera has moved a total of 0.002 units from its last keyframed position (above 0.001 threshold) >>
          <== a keyframe is created or updated

      # mouse hover or accidental brush does not trigger
      .if the user's mouse moves across the view without holding any modifier key >>
          !== a keyframe is created
          !== the camera position changes
    ```

    **Critical: interaction with playback state transitions:**
    ``` python
      # stopping playback does not trigger keyframe creation
      .if the camera stopwatch is on
      .if playback is active
      .if the user presses Space to stop playback
      .if the camera is at an interpolated position >>
          !== a keyframe is created at the stop position
          <== the camera remains at the interpolated position until the user manually moves it
          <== the playhead stays at the time where playback stopped

      # resuming after pause — movement creates keyframe if stopwatch is on
      .if the camera stopwatch is on
      .if playback was paused at time 2.0
      .if the user manually moves the camera >>
          <== a keyframe is created or updated at time 2.0
      ||> .if the user presses play >>
          <== playback resumes with the new keyframe incorporated
    ```

    **Edge cases:**
    ``` python
      # rapid sequential movements — same keyframe updated
      .if the camera stopwatch is on
      .if the user moves the camera, pauses briefly, then moves again
      .if both movements occur while the playhead is within 0.1s of the same keyframe >>
          <== the same keyframe is updated both times
          !== two separate keyframes are created

      # element deselected mid-movement — keyframe still committed
      .if an element's stopwatch is on
      .if the user is dragging the element
      .if the user clicks empty space (deselecting) before releasing the drag >>
          <== the keyframe for the drag-in-progress is still committed
          !== the partial movement is lost

      # keyframe at time 0.0 on new shot
      .if a new shot is created with an initial camera keyframe at time 0.0
      .if the camera stopwatch is on
      .if the user immediately moves the camera (playhead still at 0.0) >>
          <== the existing keyframe at time 0.0 is updated (within 0.1s threshold)
          !== a second keyframe is created at time 0.0

      # keyframe near shot boundaries
      .if the camera stopwatch is on
      .if the shot duration is 5.0
      .if the playhead is at time 4.95
      .if a keyframe exists at time 5.0
      .if the user moves the camera >>
          <== the keyframe at time 5.0 is updated (within 0.1s threshold)

      # stopwatch on with track expanded — main keyframe still created
      .if the camera stopwatch is on
      .if the camera track is expanded showing property sub-tracks
      .if the user pans the camera >>
          <== a main keyframe is created with all changed properties
          <== relevant property sub-tracks show a new keyframe marker at the current time

      # stopwatch state persists across shot switches
      .if the camera stopwatch is on for Shot_01
      .if the user switches to Shot_02 >>
          <== the camera stopwatch state on Shot_01 is preserved
          <== Shot_02's camera stopwatch has its own independent state (default off for new shots)
    ```

    **Error cases:**
    ``` python
      # turning off stopwatch on the only remaining camera keyframe
      .if the camera track has exactly one keyframe
      .if the user turns off the stopwatch >>
          <== the confirmation dialog warns about deletion
      ||> .if the user confirms >>
          <== the keyframe is deleted
          <== the camera track returns to its no-keyframe state (uses current live values)

      # undo interaction with stopwatch toggle
      .if the user turns off the stopwatch (deleting keyframes)
      .if the user presses Cmd+Z >>
          <== the keyframe deletion is undone
          <== the stopwatch returns to on
          <== all keyframes are restored
    ```

  - ##### 3.2.4. Keyframe interaction (Feature)

    ***Click to select, drag to reposition in time, click the timeline to scrub. Parent/child keyframe rules govern how main and property keyframes move, merge, and delete. Minimum one camera keyframe enforced.***

    **Functional requirements:**
    - Clicking a keyframe marker selects it
    - Clicking empty space within the track area (not on a keyframe) scrubs the playhead to that time
    - Dragging a selected keyframe repositions it in time along its track
    - Dragged keyframes snap to 0.1-second intervals
    - A keyframe cannot be dragged before time 0.0 or beyond the shot duration
    - Rotation keyframes include a **revolutions field** (After Effects model) to distinguish 0° from 360° and specify multi-revolution animation. The field stores the number of full revolutions (0, 1, 2, etc.) plus the remaining degrees. This enables smooth 360° spins and long-path rotation without requiring intermediate keyframes.
    - The selected keyframe can be deleted using the Delete key
    - The last remaining camera keyframe cannot be deleted — every shot must have at least one camera keyframe
    - The user can manually add a camera keyframe at the current playhead time using a keyboard shortcut (C key)
    - The user can manually add an element keyframe for the selected element at the current playhead time using a keyboard shortcut (V key)
    - When a keyframe is dragged, the timeline updates the keyframe's time position in real time during the drag

    **Parent/child keyframe interaction rules:**
    - Moving a **main keyframe** (collapsed view) moves ALL child property keyframes together, preserving their relative time alignment
    - Moving a **property keyframe** (expanded view) to a new time **detaches** it from its parent main keyframe:
      - A new main keyframe is created at the target time containing only that one property value
      - The original main keyframe loses that child — the property slot becomes empty
      - If the target time already has a main keyframe, the property value merges into it (see merge rule below)
    - If ALL children leave a main keyframe (all property slots empty), the main keyframe is automatically deleted
    - Deleting a **main keyframe** deletes ALL its child property keyframes
    - Deleting ALL children of a main keyframe deletes the main keyframe
    - Dragging any keyframe onto an existing keyframe at the same time performs a **silent merge**: the arriving values overwrite the existing values for those properties, no confirmation dialog

    **Expected behavior:**
    ``` python
      # click to select
      .if the user clicks a main keyframe marker >>
          <== the keyframe becomes selected (highlighted cyan)
          <== the playhead moves to that keyframe's time

      # click to scrub
      .if the user clicks empty space within the track area at the 1.5 second mark >>
          <== the playhead moves to time 1.5
          <== the view shows the interpolated state at time 1.5
          !== any keyframe becomes selected

      # drag main keyframe — children follow
      .if a main keyframe is selected at time 1.0
      .if the main keyframe has children: Position X, Position Y, Position Z, Pan, Tilt, Roll
      .if the user drags the main keyframe to time 2.5 >>
          <== the main keyframe moves to time 2.5
          <== ALL child property keyframes move to time 2.5
          <== the interpolation updates to reflect the new keyframe position
          <== the playhead follows the keyframe during the drag

      # drag property keyframe — detach from parent
      .if the camera track is expanded
      .if a Pan property keyframe exists at time 1.0 (child of the main keyframe at 1.0)
      .if the user drags the Pan property keyframe to time 2.0 >>
          <== the Pan property keyframe moves to time 2.0
          <== a new main keyframe is created at time 2.0 containing only the Pan value
          <== the original main keyframe at time 1.0 no longer has a Pan child
          <== the main keyframe at time 1.0 still exists (other children remain)

      # detaching last child deletes parent main keyframe
      .if a main keyframe at time 1.0 has only one remaining child (Position X)
      .if the user drags Position X to time 3.0 >>
          <== Position X moves to time 3.0
          <== a new main keyframe is created at time 3.0 (or merged into existing)
          <== the main keyframe at time 1.0 is automatically deleted (no children remain)

      # property keyframe dragged to time with existing main keyframe — silent merge
      .if a main keyframe exists at time 2.0 with Pan = 30
      .if the user drags a Pan property keyframe from time 1.0 to time 2.0 >>
          <== the Pan value at time 2.0 is overwritten with the arriving value
          !== a confirmation dialog appears
          !== two Pan values coexist at time 2.0

      # main keyframe dragged onto existing main keyframe — silent merge
      .if a main keyframe exists at time 1.0 with all properties set
      .if a main keyframe exists at time 2.0 with all properties set
      .if the user drags the main keyframe from 1.0 to 2.0 >>
          <== all property values from the arriving keyframe overwrite the existing values at time 2.0
          <== the main keyframe at time 1.0 no longer exists
          !== a confirmation dialog appears

      # snap to grid
      .if the user drags a keyframe >>
          <== the keyframe time snaps to the nearest 0.1-second interval
          !== the keyframe rests at a time like 1.37 or 2.84

      # drag boundaries
      .if the user drags a keyframe toward time -0.5 >>
          <== the keyframe stops at time 0.0
      .if the user drags a keyframe beyond the shot duration >>
          <== the keyframe stops at the shot duration

      # delete main keyframe — deletes all children
      .if a main keyframe is selected at time 2.0
      .if the main keyframe has children: Position X, Position Y, Position Z, Pan, Tilt, Roll
      .if the user presses Delete >>
          <== the main keyframe is removed from the track
          <== ALL child property keyframes at time 2.0 are removed
          <== the interpolation recalculates without those keyframes

      # delete property keyframe
      .if the camera track is expanded
      .if a Pan property keyframe is selected at time 2.0
      .if the user presses Delete >>
          <== the Pan property keyframe is removed
          <== the main keyframe at time 2.0 loses its Pan child
      ||> .if other children still exist at time 2.0 >>
          <== the main keyframe at time 2.0 persists
      ||> .if no children remain at time 2.0 >>
          <== the main keyframe at time 2.0 is automatically deleted

      # minimum camera keyframe enforcement
      .if the camera track has exactly one main keyframe
      .if the user attempts to delete it >>
          <== the keyframe is not deleted
          !== the camera track becomes empty

      # minimum camera keyframe — cannot delete last via property deletion
      .if the camera track has exactly one main keyframe at time 0.0
      .if the track is expanded
      .if the user tries to delete all property keyframes at time 0.0 one by one >>
          <== the last property keyframe cannot be deleted
          !== the main keyframe is destroyed by losing all children

      # manual camera keyframe creation
      .if the playhead is at time 1.0
      .if the user presses the C key >>
          <== a new camera main keyframe appears at time 1.0
          <== the keyframe stores all current camera property values (position, pan, tilt, roll, focal length if zoom lens)
          <== child property keyframes are created for all properties

      # manual element keyframe creation
      .if an element is selected within the scene
      .if the user presses the V key >>
          <== a new element main keyframe appears at the current time on that element's track
          <== the keyframe stores all current property values (position, scale, rotation)
          <== .if no track exists for that element >> a new element track is created

      # C and V keys during playback — no effect
      .if playback is active
      .if the user presses C or V >>
          !== a keyframe is created
          <== playback continues uninterrupted
    ```

    **Error cases:**
    ``` python
      # no element selected for element keyframe
      .if no element is selected within the scene
      .if the user presses V >>
          !== a keyframe is created
          !== the application produces an error

      # overlapping keyframe times via drag
      .if a main keyframe already exists at time 1.0
      .if the user drags another main keyframe to time 1.0 >>
          <== the arriving keyframe merges silently into the existing one, updating its values
          !== two main keyframes occupy the same time on the same track
          !== a confirmation dialog appears
    ```

  - ##### 3.2.5. Interpolation and playback (Feature)

    ***Smooth interpolation between keyframes with cinematic rotation handling. Pan wrap-around through 360/0 boundary. Real-time playback evaluating every track each frame. Playhead stays at shot end when playback completes.***

    **Functional requirements:**
    - Position values (X, Y, Z) interpolate smoothly between keyframes (not linearly snapping)
    - Pan interpolation uses shortest-path through the 360/0 degree boundary — a pan from 350 to 10 degrees travels 20 degrees forward (through 360/0), not 340 degrees backward
    - Tilt interpolation is linear in degrees (tilt does not wrap — range is typically -90 to +90)
    - Roll interpolation uses shortest-path through the 360/0 degree boundary, same as pan
    - **360-degree rotation / revolutions**: rotation keyframes include an explicit "revolutions" field (After Effects approach). When the start and end rotation values are identical, the revolutions field specifies full turns (e.g., "1 revolution clockwise"). This allows the user to animate a full 360-degree spin — or multiple full spins — without ambiguity. Without the revolutions field, interpolation would take the shortest path (zero movement for identical start/end values).
    - Focal length interpolates smoothly between keyframe values (zoom lenses only)
    - Scale interpolates smoothly between keyframe values (element tracks, always positive)
    - Element rotation (X, Y, Z in degrees) interpolates using shortest-path through 360/0 per axis
    - When only one keyframe exists on a track, the value is held constant (no interpolation needed)
    - When the playhead is before the first keyframe, the first keyframe's values are held
    - When the playhead is after the last keyframe, the last keyframe's values are held
    - Per-property interpolation: each property sub-track interpolates independently between its own keyframes. If Pan has keyframes at times 0.0 and 2.0, but Tilt has keyframes at times 0.0 and 3.0, each interpolates along its own timeline.
    - The project frame rate is user-configurable, defaulting to 24fps. Common options: 24, 25, 30, 48, 60fps. The frame rate applies project-wide (all shots share the same frame rate).
    - Playback advances time at real-time speed (1 second of playback = 1 second of shot time)
    - During playback, every track (camera and every element track) is evaluated each frame at the project frame rate
    - Playback stops when the playhead reaches the shot duration
    - The playhead stays at the shot duration when playback completes — it does NOT return to time 0.0
    - Playback can be started and stopped using the Space key or the transport bar play button
    - During playback, the user cannot select or drag keyframes
    - The current time display within the transport bar updates continuously during playback
    - **Playback auto-scroll**: if the playhead exits the visible range during playback, the view shifts to follow while maintaining the current zoom level

    **Keyframe interpolation shapes (AE-style):**
    - Three keyframe shapes indicating interpolation type: **diamond** (linear), **circle** (smooth), **square** (hold)
    - Alt+click a keyframe marker to cycle through shapes
    - Default shape heuristic: camera keyframes default to smooth, single-keyframe elements default to hold, multi-keyframe elements default to linear
    - On expanded sub-tracks, between-keyframe **curve indicators** show the interpolation curve type: `─` linear, `⌒` ease-in, `⌓` ease-out, `~` ease-in-out, `∿` bezier
    - Curve indicators are only shown when there is enough horizontal space between keyframes (>30px)

    > **Design note — slow motion:** Variable playback speed (e.g., 0.5x, 0.25x for slow motion review) is a desired future capability. Not in scope for this milestone, but the playback system should not assume a fixed 1:1 time ratio in its architecture.

    **Expected behavior:**
    ``` python
      # smooth position interpolation
      .if camera position X keyframe A is 0.0 at time 0.0
      .if camera position X keyframe B is 10.0 at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera position X is near 5.0
          <== the motion between A and B appears smooth, not jerky

      # pan shortest-path through 360/0 boundary
      .if camera pan keyframe A is 350 degrees at time 0.0
      .if camera pan keyframe B is 10 degrees at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera pan is near 0 degrees (traveled 20 degrees forward through 360/0)
          !== the camera pans 340 degrees the long way backward

      # pan shortest-path — no wrap needed
      .if camera pan keyframe A is 30 degrees at time 0.0
      .if camera pan keyframe B is 90 degrees at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera pan is near 60 degrees (traveled 60 degrees directly)

      # tilt interpolation — no wrap-around
      .if camera tilt keyframe A is -30 degrees at time 0.0
      .if camera tilt keyframe B is 45 degrees at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera tilt is near 7.5 degrees
          <== the interpolation is linear between -30 and 45

      # roll shortest-path through 360/0 boundary
      .if camera roll keyframe A is 340 degrees at time 0.0
      .if camera roll keyframe B is 20 degrees at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera roll is near 0 degrees (traveled 40 degrees through 360/0)
          !== the camera rolls 320 degrees the long way

      # focal length interpolation (zoom lens)
      .if the selected lens is a zoom lens
      .if camera focal length keyframe A is 24mm at time 0.0
      .if camera focal length keyframe B is 85mm at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera focal length is between 24mm and 85mm
          <== the view's field of view changes smoothly

      # per-property independent interpolation
      .if Pan has keyframes at time 0.0 (pan=0) and time 2.0 (pan=90)
      .if Tilt has keyframes at time 0.0 (tilt=0) and time 4.0 (tilt=45)
      .if the playhead is at time 2.0 >>
          <== pan interpolation is complete: pan is at 90 degrees
          <== tilt interpolation is halfway: tilt is near 22.5 degrees
          <== each property follows its own keyframe timing independently

      # hold values beyond keyframe range
      .if the first keyframe is at time 1.0
      .if the playhead is at time 0.5 >>
          <== the track holds the first keyframe's values

      .if the last keyframe is at time 3.0
      .if the playhead is at time 4.0 >>
          <== the track holds the last keyframe's values

      # single keyframe hold
      .if a track has exactly one keyframe >>
          <== the track's values are constant regardless of playhead position

      # real-time playback
      .if the user presses play >>
          <== the playhead advances at real-time speed
          <== the view updates smoothly showing the animated result
          <== every camera and element track are evaluated simultaneously
          <== the transport bar time display updates continuously

      # playback stops at shot end — playhead stays
      .if the shot duration is 5.0 seconds
      .if the user presses play >>
          <== playback stops when the playhead reaches 5.0
          <== the playhead remains at time 5.0 after playback ends
          !== the playhead returns to time 0.0
          !== the playhead advances beyond the shot duration

      # playback locks interaction
      .if playback is active
      .if the user clicks a keyframe >>
          !== the keyframe becomes selected
          !== the playhead jumps to the keyframe's time

      # play/pause toggle
      .if the user presses Space during playback >>
          <== playback pauses
          <== the playhead remains at the current time
      ||> .if the user presses Space again >>
          <== playback resumes from the paused time

      # multi-track evaluation
      .if the camera track has keyframes
      .if two element tracks have keyframes
      .if the user presses play >>
          <== the camera animates along its keyframed path
          <== both objects animate along their keyframed paths
          <== all animations play simultaneously
    ```

    **Edge cases:**
    ``` python
      # frame rate under heavy scenes — frame dropping
      .if the scene contains many objects
      .if playback cannot maintain real-time speed >>
          <== playback drops frames to maintain correct timing (this is the default Unity behavior)
          <== the shot plays for the correct wall-clock duration
          !== the shot takes longer than its duration to play
          <== animation evaluation skips to the correct time, not the next sequential frame

      # zero-duration gap between keyframes (merged)
      .if two keyframes exist at time 1.0 (merged) >>
          <== the merged keyframe's values take precedence
          <== no interpolation occurs

      # property keyframe exists without siblings at same time
      .if Pan has a keyframe at time 1.5 (detached from original main keyframe)
      .if no other property has a keyframe at time 1.5
      .if the playhead is at time 1.5 >>
          <== pan value comes from the keyframe at time 1.5
          <== all other properties interpolate from their own nearest keyframes
          <== the combined result is a valid camera state

      # pan wrap-around edge case: 180 degree ambiguity
      .if camera pan keyframe A is 0 degrees at time 0.0
      .if camera pan keyframe B is 180 degrees at time 2.0
      .if the playhead is at time 1.0 >>
          <== the camera pan is near 90 degrees (either direction is equal; implementation picks one consistently)
    ```

  - ##### 3.2.6. Camera path visualization (Feature)

    ***Render the camera's dolly track as a smooth spline in 3D space, showing keyframe positions as nodes with frustum indicators displaying camera look direction. Toggleable visibility.***

    **Functional requirements:**
    - The camera path is rendered as a smooth curve (spline) connecting keyframe positions in 3D space
    - Keyframe positions are shown as visible nodes (dots or spheres) along the path
    - Each node displays a **frustum indicator** showing the camera's look direction at that keyframe — a small wireframe frustum or cone extending from the node in the direction the camera faces (based on pan, tilt, roll at that keyframe)
    - The frustum indicator size is proportional to the focal length at that keyframe (wider frustum for wide-angle, narrower for telephoto) — or a fixed small size if the lens is a prime
    - The path shows the direction of camera travel (e.g., arrow indicators or graduated opacity)
    - The path is visible within the 3D view, rendered in world space (not a UI overlay)
    - The path updates in real time as keyframes are added, moved, or deleted
    - The path updates in real time as recording creates or modifies keyframes
    - Frustum indicators update in real time when camera rotation or focal length changes at a keyframe
    - Path visibility is toggled by the user (keyboard shortcut `P` or UI button)
    - The path is hidden by default
    - **Camera path badge**: When the camera path is visible, a badge reading "PATH" appears in the bottom-right corner of the Camera View frame in amber/gold color. This provides a persistent visual cue that path visualization is active. See `ui-layout-spec.md` §3.6.
    - When visible, the path does not obscure critical scene elements (rendered using semi-transparency or a thin line)
    - The path represents the camera's position trajectory — frustum indicators additionally communicate rotation and field of view at each keyframe
    - When only one camera keyframe exists, no path is drawn (a single node with frustum indicator is shown)
    - The camera path corresponds to the current shot only — switching shots updates the path

    **Expected behavior:**
    ``` python
      # toggling visibility
      .if the user enables camera path visualization >>
          <== a smooth curve appears within the 3D view connecting camera keyframe positions
          <== nodes appear at each keyframe position along the curve
          <== each node shows a frustum indicator displaying the camera's look direction at that keyframe
      ||> .if the user disables it >>
          <== the path, nodes, and frustum indicators are no longer visible

      # frustum indicators show camera direction
      .if three camera keyframes exist
      .if keyframe A has pan=0, tilt=0 (looking forward)
      .if keyframe B has pan=90, tilt=0 (looking right)
      .if keyframe C has pan=0, tilt=-45 (looking down at 45 degrees) >>
          <== node A's frustum points forward
          <== node B's frustum points to the right
          <== node C's frustum points downward at 45 degrees
          <== the frustum directions clearly communicate where the camera looks at each keyframe

      # frustum size reflects focal length (zoom lens)
      .if the selected lens is a zoom lens
      .if keyframe A has focal length 24mm
      .if keyframe B has focal length 85mm >>
          <== node A's frustum is wider (wide-angle field of view)
          <== node B's frustum is narrower (telephoto field of view)

      # path reflects keyframes
      .if three camera keyframes exist at positions A, B, and C >>
          <== the path curves smoothly from A through B to C
          <== three nodes with frustum indicators are visible at positions A, B, and C

      # real-time update on keyframe change
      .if the path is visible
      .if the user creates a new camera keyframe (stopwatch on, recording) >>
          <== the path immediately updates to include the new keyframe position
          <== a new node with frustum indicator appears at the keyframe position

      # real-time update on rotation change
      .if the path is visible
      .if the user pans the camera at a keyframe position >>
          <== the frustum indicator at that node rotates to reflect the new pan value
          <== the update is immediate

      # real-time update on keyframe drag
      .if the path is visible
      .if the user drags a keyframe to a different time (changing interpolation) >>
          <== the path shape updates to reflect the new timing

      # real-time update on camera movement
      .if the path is visible
      .if the user moves the camera (stopwatch on, updating a keyframe) >>
          <== the node at that keyframe moves to the new position
          <== the frustum indicator follows the node
          <== the path curve updates smoothly

      # single keyframe — no path, but frustum shown
      .if only one camera keyframe exists
      .if path visualization is enabled >>
          <== a single node is visible at the keyframe position
          <== the node shows a frustum indicator displaying the camera's look direction
          !== a path or curve is drawn

      # direction indication
      .if the path is visible
      .if multiple keyframes exist >>
          <== the path indicates the direction of camera travel
          <== the user can distinguish the start from the end of the path

      # shot switching
      .if the path is visible for Shot_01
      .if the user switches to Shot_02 >>
          <== the path updates to show Shot_02's camera keyframe positions and frustum indicators
          !== the path from Shot_01 remains visible

      # hidden by default
      .if the application starts >>
          <== the camera path is not visible
    ```

    **Edge cases:**
    ``` python
      # two keyframes at same position but different rotation
      .if two camera keyframes have the same position but different pan/tilt/roll >>
          <== the path shows overlapping nodes at that position
          <== each node's frustum indicator points in its own direction
          !== the path displays erratic behavior

      # keyframes very close together in space
      .if two keyframes are separated by less than 0.01 units >>
          <== the path does not produce visual artifacts
          <== the curve remains smooth
          <== frustum indicators are still distinguishable (or gracefully overlap)

      # frustum indicator when roll is applied
      .if a keyframe has roll = 45 degrees >>
          <== the frustum indicator is visibly rotated 45 degrees around its look axis
          <== the rotation is clearly distinguishable from a non-rolled frustum
    ```
