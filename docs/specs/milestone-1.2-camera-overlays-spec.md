# Milestone 1.2: Camera Overlays — Specification

**Date**: 2026-03-10
**Milestone**: 1.2
**Status**: Draft

---

- ### 1.2. Camera overlays (Milestone)

  Composition aids that sit on top of the 3D view. These are the director's framing tools — the black bars that define the frame, the guide lines that help place subjects within it, and subtitle text for dialogue and description.

  Aspect ratio masks are not decorative. They define the deliverable image. Everything outside the mask is invisible to the audience, and the director needs to see that boundary at all times while composing. Composition guides provide the geometric scaffolding that cinematographers use instinctively — thirds lines, center marks, and safe zones for broadcast compliance. Subtitles let the director place dialogue and descriptive text directly into the frame for storyboard review and export. All overlay types must feel like they are part of the camera, not part of the UI.

  **Overlay z-index ordering** (see `ui-layout-spec.md` §2.3 for full details):
  1. Aspect ratio masks (above scene content)
  2. Composition guides (above masks)
  3. Shot label, timecode, camera info, active tool badge, Director View badge, camera path badge (above guides)

  All overlays render inside the Camera View frame. Masks and guides are compositional; the others are informational and positioned at the edges/corners of the frame to avoid interfering with framing.

  *Blocked by: 1.1 (virtual camera — overlays need a view to overlay on)*

  - ##### 1.2.1. Aspect ratio masks (Feature)

    ***Letterbox and pillarbox bars that crop the view to the selected delivery format, showing exactly what the audience will see.***

    **Functional requirements:**
    - The system provides eight aspect ratio options: Full Screen (matches window), 16:9, 16:10, 1.85:1, 2.35:1, 2.39:1, 2:1, 4:3
    - The default aspect ratio is 16:9
    - When the selected ratio is wider than the view, horizontal bars appear on top and bottom (letterbox)
    - When the selected ratio is narrower than the view, vertical bars appear on left and right (pillarbox)
    - When "Full Screen" is selected, no bars are displayed regardless of window dimensions
    - Mask bars are opaque black — they completely obscure the 3D scene behind them
    - The user can cycle through aspect ratios or select one directly
    - `A` key cycles aspect ratios forward through the list. `Shift+A` cycles backward.
    - The selected aspect ratio persists until the user changes it
    - The active aspect ratio is displayed within the camera info HUD (1.2.3)
    - When an anamorphic lens is active, the aspect ratio is auto-locked to the computed delivery format based on the squeeze factor. The user cannot manually change the aspect ratio while an anamorphic lens is selected.

    **Expected behavior:**
    ``` python
      # default state on launch
      .if the application starts >>
          <== aspect ratio is set to 16:9
          <== mask bars are visible
          <== the unmasked area is centered within the view

      # switching aspect ratio
      .if the user selects 2.39:1 >>
          <== horizontal bars appear on top and bottom
          <== the unmasked area has a 2.39:1 ratio
          <== the unmasked area is centered vertically

      # narrower ratio than window creates pillarbox
      .if the view is landscape
      .if the user selects 4:3 >>
          <== vertical bars appear on left and right
          <== the unmasked area has a 4:3 ratio
          <== the unmasked area is centered horizontally

      # full screen removes bars
      .if the user selects Full Screen >>
          <== no mask bars are visible
          <== the entire view is unmasked

      # cycling forward through aspect ratios
      .if aspect ratio is set to 16:9
      .if the user presses A >>
          <== aspect ratio changes to 16:10
          <== mask bars update immediately

      # cycling backward through aspect ratios
      .if aspect ratio is set to 16:9
      .if the user presses Shift+A >>
          <== aspect ratio changes to Full Screen
          <== mask bars update immediately

      # forward cycling wraps around at end of list
      .if aspect ratio is set to 4:3
      .if the user presses A >>
          <== aspect ratio wraps to Full Screen

      # backward cycling wraps around at beginning of list
      .if aspect ratio is set to Full Screen
      .if the user presses Shift+A >>
          <== aspect ratio wraps to 4:3

      # window resize preserves ratio
      .if aspect ratio is set to 2.35:1
      .if the user resizes the window >>
          <== the unmasked area still has a 2.35:1 ratio
          <== mask bars resize to fill the remaining space
          <== the unmasked area remains centered

      # resize can flip between letterbox and pillarbox
      .if aspect ratio is set to 16:9
      .if the user resizes the window to be taller than it is wide >>
          <== pillarbox bars appear on left and right
      ||> .if the user resizes the window to be wider than it is tall >>
          <== letterbox bars appear on top and bottom

      # masks render above the 3D scene
      .if elements exist near the edges of the frame >>
          <== mask bars draw over any 3D content behind them
          !== 3D content bleeds through the mask bars
    ```

    **Error cases:**
    ``` python
      # degenerate window size
      .if the window is resized to an extremely small size >>
          <== mask bars still render correctly
          !== the application crashes or produces rendering artifacts
    ```

  - ##### 1.2.2. Composition guides (Feature)

    ***Rule of thirds grid, center cross, and broadcast safe zone rectangles — toggled independently, drawn within the unmasked area.***

    **Functional requirements:**
    - Three guide types are available: rule of thirds, center cross, and safe zones
    - Every guide type starts hidden by default
    - Each guide type is toggled independently (on/off). `G` key toggles all composition guides on/off as a group.
    - Guide visibility state is global, not per-shot — changing guides in one shot changes them everywhere
    - Guides are drawn only within the unmasked area defined by the current aspect ratio mask
    - Rule of thirds draws two horizontal and two vertical lines, dividing the frame into a 3x3 grid
    - Center cross draws a small crosshair at the exact center of the frame
    - Center cross has fixed pixel size (not proportional to frame)
    - Safe zones draws two concentric rectangles within the frame:
      - Title safe: 90% of the frame dimensions, centered
      - Action safe: 93% of the frame dimensions, centered
    - Safe zone percentages are configurable with defaults (title safe 90%, action safe 93%)
    - Guide lines are semi-transparent so they do not obscure the scene
    - Guides render above the 3D scene but do not extend into the mask bars
    - Safe zone percentages are relative to the unmasked area, not the full view

    **Expected behavior:**
    ``` python
      # default state
      .if the application starts >>
          <== rule of thirds is hidden
          <== center cross is hidden
          <== safe zones are hidden

      # toggling guides independently
      .if the user enables rule of thirds >>
          <== rule of thirds grid is visible
          <== center cross remains hidden
          <== safe zones remain hidden

      # multiple guides can be active simultaneously
      .if the user enables rule of thirds
      .if the user enables center cross
      .if the user enables safe zones >>
          <== every three guide types are visible simultaneously
          <== guides do not interfere using each other visually

      # guides respect the aspect ratio mask
      .if aspect ratio is set to 2.39:1
      .if rule of thirds is enabled >>
          <== the thirds grid is drawn within the 2.39:1 unmasked area
          !== guide lines extend into the letterbox bars

      # guides update when aspect ratio changes
      .if rule of thirds is enabled
      .if the user changes aspect ratio from 16:9 to 4:3 >>
          <== the thirds grid resizes to fit the new unmasked area
          <== guide positions recalculate immediately

      # guides update when window resizes
      .if center cross is enabled
      .if the user resizes the window >>
          <== the center cross remains at the center of the unmasked area
          !== the center cross drifts from center

      # center cross has fixed pixel size
      .if center cross is enabled
      .if the user resizes the window to be much larger >>
          <== the center cross remains the same pixel size
          !== the center cross scales proportionally with the frame

      # safe zones are relative to unmasked area
      .if safe zones are enabled
      .if aspect ratio is set to 2.35:1 >>
          <== title safe rectangle is 90% of the 2.35:1 unmasked area
          <== action safe rectangle is 93% of the 2.35:1 unmasked area
          <== both rectangles are centered within the unmasked area
          !== safe zone percentages are calculated from the full view

      # configurable safe zone percentages
      .if the user changes title safe to 85%
      .if safe zones are enabled >>
          <== title safe rectangle is 85% of the unmasked area
          <== action safe rectangle remains at 93% of the unmasked area

      # guide visibility is global across shots
      .if rule of thirds is enabled on shot 1
      .if the user navigates to shot 2 >>
          <== rule of thirds is visible on shot 2
      ||> .if the user disables rule of thirds on shot 2
          .if the user navigates back to shot 1 >>
          <== rule of thirds is hidden on shot 1

      # full screen aspect ratio
      .if aspect ratio is set to Full Screen
      .if rule of thirds is enabled >>
          <== the thirds grid spans the entire view
          <== guide positions update if the window is resized

      # toggling off
      .if rule of thirds is enabled
      .if the user disables rule of thirds >>
          <== rule of thirds grid is no longer visible
          <== other enabled guides remain visible
    ```

  - ##### 1.2.3. Camera info HUD (Feature)

    ***An overlay showing the data a DP would read off an on-set monitor: focal length, camera height, angle of view, aspect ratio, body, lens preset, and squeeze factor (if anamorphic). Toggleable via shortcut, glanceable, not intrusive. Counts as an overlay for export purposes.***

    > **Note**: Full specification is in the Virtual Camera spec (1.1.7, relocated here). This section establishes the camera info's identity as a camera overlay. See the 1.1 spec for complete functional requirements, expected behavior, and error cases.

    **Key requirements:**
    - Non-interactive overlay positioned at top-right of Camera View (see `ui-layout-spec.md` §3.3)
    - Toggleable via keyboard shortcut (`H`)
    - Displays: focal length (mm), camera height (m), horizontal angle of view (degrees), aspect ratio name, camera body name, lens set name
    - When DOF preview is active, additionally displays aperture (f-stop)
    - When an anamorphic lens is active, additionally displays squeeze factor
    - Counts as an overlay for export purposes — renders into exported output when visible
    - Always legible regardless of scene behind it (subtle background panel)
    - A global overlay toggle controls visibility of all view overlay elements (light icons, camera frustum wireframes, active tool indicators) — but NOT camera overlays (aspect ratio masks, composition guides, subtitle overlay). This toggle is separate from the camera info toggle.

  - ##### 1.2.4. Subtitle overlay (Feature)

    ***Text layers overlaid on the frame for dialogue, description, or any director's text — timeable within each shot and included in exports.***

    **Functional requirements:**
    - Subtitles are text layers rendered on top of the 3D scene in a fixed position (bottom center of the unmasked area)
    - Subtitles are per-shot data — each shot has its own list of subtitle layers
    - Multiple subtitle layers can exist per shot (stacked vertically or arranged sequentially in time)
    - Each subtitle layer has an adjustable start time and end time within the shot duration
    - Text appears at the start time and disappears at the end time — no animations or transitions
    - When a subtitle layer is selected, the user can change: text content, color, font size, and font family
    - Subtitles render above the 3D scene but below aspect ratio masks
    - Subtitles are included when exporting frames or video
    - Subtitle position is fixed at bottom center — not user-draggable
    - When multiple subtitles overlap in time, they stack vertically (most recently created on bottom)

    **Expected behavior:**
    ``` python
      # adding a subtitle
      .if the user adds a subtitle layer to a shot >>
          <== a new subtitle layer appears in the shot's subtitle list
          <== the subtitle has default text ("Subtitle")
          <== the subtitle start time defaults to 0
          <== the subtitle end time defaults to the shot duration
          <== the subtitle is positioned at bottom center of the unmasked area

      # subtitle visibility during playback
      .if a subtitle has start time 1.0s and end time 3.0s
      .if the playhead is at 0.5s >>
          <== the subtitle is not visible
      ||> .if the playhead is at 1.0s >>
          <== the subtitle is visible
      ||> .if the playhead is at 2.0s >>
          <== the subtitle is visible
      ||> .if the playhead is at 3.0s >>
          <== the subtitle is not visible

      # editing subtitle properties
      .if a subtitle layer is selected
      .if the user changes the font size to 24 >>
          <== the subtitle renders at font size 24
          <== the change is visible immediately in the view
      ||> .if the user changes the color to yellow >>
          <== the subtitle renders in yellow
      ||> .if the user changes the font family to Courier >>
          <== the subtitle renders in Courier

      # multiple subtitles in sequence
      .if subtitle A has start 0.0s end 2.0s
      .if subtitle B has start 2.0s end 4.0s
      .if the playhead is at 1.0s >>
          <== only subtitle A is visible
      ||> .if the playhead is at 3.0s >>
          <== only subtitle B is visible

      # multiple subtitles overlapping in time
      .if subtitle A has start 0.0s end 3.0s
      .if subtitle B has start 1.0s end 4.0s
      .if the playhead is at 2.0s >>
          <== both subtitles are visible
          <== subtitles are stacked vertically at bottom center

      # subtitles respect aspect ratio mask
      .if aspect ratio is set to 2.39:1
      .if a subtitle is visible >>
          <== the subtitle is positioned within the unmasked area
          !== the subtitle extends into the letterbox bars

      # subtitles render below masks
      .if aspect ratio is set to 2.39:1
      .if a subtitle is positioned near the bottom of the unmasked area >>
          <== the mask bar draws over any subtitle text that would fall outside the unmasked area
          <== the subtitle does not draw on top of the mask

      # subtitles are per-shot
      .if shot 1 has two subtitle layers
      .if the user navigates to shot 2 >>
          <== shot 2 has no subtitles (unless separately added)
          !== shot 1's subtitles appear on shot 2
      ||> .if the user navigates back to shot 1 >>
          <== both subtitle layers are still present

      # subtitle timing clamped to shot duration
      .if a shot is 5.0s long
      .if the user sets a subtitle end time to 5.0s >>
          <== the subtitle end time is 5.0s
      ||> .if the user sets a subtitle end time to 7.0s >>
          <== the subtitle end time is clamped to 5.0s

      # subtitle in export
      .if a subtitle is visible at a given frame
      .if the user exports that frame >>
          <== the exported image includes the subtitle text
          <== the subtitle appears at the same position and style as in the view

      # deleting a subtitle
      .if a subtitle layer is selected
      .if the user deletes the subtitle >>
          <== the subtitle is removed from the shot
          <== the subtitle is no longer visible at any playhead position
          <== other subtitle layers in the shot are unaffected

      # aspect ratio change repositions subtitles
      .if a subtitle is visible
      .if the user changes aspect ratio from 16:9 to 2.39:1 >>
          <== the subtitle repositions to bottom center of the new unmasked area
          <== the subtitle remains fully within the unmasked area
    ```

    **Error cases:**
    ``` python
      # start time after end time
      .if the user sets a subtitle start time to 3.0s and end time to 1.0s >>
          <== the system prevents the invalid range
          !== the subtitle has a negative duration

      # empty subtitle text
      .if the user clears the subtitle text to an empty string >>
          <== the subtitle layer still exists in the list
          <== nothing is rendered in the view for that subtitle
          !== the application crashes or errors

      # subtitle on zero-duration shot
      .if a shot has zero duration
      .if the user adds a subtitle >>
          <== the subtitle is created with start 0 and end 0
          <== the subtitle is never visible during playback

      # very long subtitle text
      .if the user enters subtitle text that exceeds the frame width >>
          <== the text wraps to a second line
          !== the text is clipped or extends beyond the unmasked area horizontally
    ```
