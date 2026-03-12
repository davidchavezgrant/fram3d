# Milestone 1.1: Virtual Camera

**Date**: 2026-03-10
**Parent**: Phase 1 — The Camera
**Companion to**: `fram3d-roadmap.md`

---

- ### 1.1. Virtual camera (Milestone)

  The virtual camera is the product. Every creative decision in previsualization flows through how the camera sees the scene -- what lens is on it, where it sits, what it's focused on, how it moves. This milestone delivers a physically-grounded camera rig that a DP or director can operate using cinematic language they already know: focal lengths in millimeters, dolly and crane moves, focus pulls, handheld shake. No 3D jargon, no abstract parameters.

  The camera must feel like a real tool. When a user racks focus to a new subject, the transition should feel like pulling a follow focus ring -- not teleporting. When they switch from a 24mm to an 85mm, the field of view should change exactly as it would on a real set with their chosen camera body and sensor. When they add handheld shake, it should look like a human holding the camera, not a random vibration. The camera info should show the same data a 1st AC would read off a monitor.

  - ##### 1.1.1. Camera movement (Feature)
    ***Pan, tilt, dolly, truck, crane, roll, orbit, dolly zoom, and reset -- the full vocabulary of physical camera movement, mapped to mouse and modifier keys.***

    *Related: 2.2.1 Mouse controls, 2.2.2 Keyboard shortcuts*

    **Functional requirements:**
    - Pan rotates the camera horizontally around a vertical axis through the camera's position. Positive pan rotates rightward.
    - Tilt rotates the camera vertically around a horizontal axis through the camera's position. Positive tilt rotates upward.
    - Dolly translates the camera forward or backward along its local forward axis. Dolly in moves toward whatever the camera is looking at; dolly out moves away.
    - Truck translates the camera laterally along its local right axis. Positive trucks rightward.
    - Crane translates the camera vertically along the world Y axis. Positive cranes upward.
    - Roll rotates the camera around its local forward axis. Positive roll tilts the top of the frame rightward (clockwise from the camera's perspective).
    - Orbit simultaneously rotates the camera around a target point while keeping that point centered in frame. Orbit respects both horizontal and vertical axes. Orbit pivot uses a persistent last-focus model (Maya/Unity style) — the pivot point is the last element the camera focused on, and persists until the user focuses on a new element.
    - Dolly zoom simultaneously changes focal length and translates the camera forward/backward to maintain the apparent size of a subject at a fixed distance while the perspective distortion changes. The background compresses or expands around the subject.
    - Dolly zoom can be locked to a specific element. When locked, the reference distance tracks that element's position rather than using a fixed distance. This allows dolly zoom to follow a moving subject.
    - Reset restores the camera to a known default position, rotation, and focal length.
    - Every movement type has a configurable speed multiplier that scales the magnitude of the movement per unit of input.
    - All movements are framerate-independent. The same input produces the same visual result at 30fps and at 120fps.
    - Movements compose naturally. A user can pan while dollying in (via separate inputs) and the result is the sum of both motions applied in the same frame.
    - There is no collision detection. The camera can pass through elements and below the ground plane. This is previs, not a game -- the camera goes wherever the director wants it.

    **Design constraints:**
    - Default camera position places the camera at approximate eye height (1.6 units above the ground plane) facing forward.
    - Pan and tilt operate around an anchor point (e.g., the camera's own position or a scene anchor), not around a fixed world origin.

    **Expected behavior:**
    ``` python
      # dolly moves along camera's forward axis
      .if camera faces forward
      ||> .if user dollies forward >>
          <== camera translates along its local Z axis toward the scene
          <== camera rotation does not change

      # crane is world-relative, not camera-relative
      .if camera is tilted 45 degrees downward
      ||> .if user cranes up >>
          <== camera moves straight up within world space
          !== camera moves along its tilted local Y axis

      # truck is camera-relative
      .if camera is rotated 90 degrees to the right
      ||> .if user trucks right >>
          <== camera moves along its local right axis (which is now world-forward)
          <== camera rotation does not change

      # dolly zoom preserves subject size
      .if a subject is visible at center frame using 50mm focal length
      ||> .if user activates dolly zoom to widen toward 24mm >>
          <== subject remains approximately the same size on screen
          <== background appears to stretch away from subject
          <== focal length decreases while camera moves closer

      # dolly zoom locked to an element
      .if dolly zoom is locked to a character in the scene
      ||> .if the character moves farther from the camera during dolly zoom >>
          <== the reference distance updates to track the character's current position
          <== the character remains approximately the same size on screen despite moving
          <== focal length and camera position adjust to compensate for the changing reference distance

      # movements compose
      .if user provides simultaneous pan-right input and dolly-forward input >>
          <== camera pans right AND translates forward within the same frame
          <== result is identical to applying each movement independently and summing

      # reset returns to defaults
      .if camera has been moved, rotated, and has a non-default focal length
      ||> .if user triggers reset >>
          <== camera returns to default position (eye height, facing forward)
          <== camera rotation returns to identity
          <== focal length returns to default value
    ```

    **Error cases:**
    ``` python
      # no target for dolly zoom
      .if no elements exist within the scene
      ||> .if user activates dolly zoom >>
          <== dolly zoom uses the camera's current focus distance as the reference distance
          <== movement still functions, using that fixed distance

      # extreme speed multiplier
      .if user sets movement speed to its maximum value
      ||> .if user performs a dolly move >>
          <== movement is fast but remains finite and framerate-independent
          !== camera teleports to infinity or produces NaN positions
    ```

  - ##### 1.1.2. Lens system (Feature)
    ***Focal length from 14mm to 400mm, with physically accurate field of view derived from real sensor dimensions. Lens changes feel smooth -- like turning a barrel, not flipping a switch.***

    *Blocked by: 1.1.3 Camera body and lens presets (sensor dimensions come from the selected body)*

    **Functional requirements:**
    - Focal length is adjustable from 14mm to 400mm.
    - Field of view is calculated from focal length and the vertical sensor dimension of the currently selected camera body: `FOV = 2 * atan(sensorHeight / (2 * focalLength))`.
    - Focal length changes smoothly via lerp when adjusted through scroll or preset selection. The transition is fast but perceptible -- the user should see the field of view shift, not jump.
    - Switching camera body (and therefore sensor size) while keeping the same focal length results in a different FOV. A 50mm on a Super 35mm body looks different from a 50mm on a full-frame body, just as it would on set.
    - Common focal length presets are available for quick selection: 14, 18, 21, 24, 28, 35, 50, 65, 75, 85, 100, 135, 150, 200, 300, 400mm.
    - The current focal length is displayed in millimeters on the camera info and updates in real time during transitions.
    - Focal length is a keyframeable property. It is recorded when the track is recording (see 3.2.3) and interpolated during playback.
    - **Anamorphic lenses**: Squeeze factors (1.33x, 1.5x, 1.8x, 2x) are supported. FOV is computed from the squeeze factor and sensor dimensions. Aspect ratio is auto-locked to the computed delivery format when an anamorphic lens is active (see 1.2.1). Oval bokeh is deferred to future polish.

    **Design constraints:**
    - The default focal length is 50mm (a neutral starting point most DPs are comfortable with).
    - At 14mm the ultra-wide distortion should be extreme but not produce visual artifacts. At 400mm the FOV should be extremely narrow but still render a usable image.

    **Expected behavior:**
    ``` python
      # focal length controls FOV through sensor physics
      .if camera body is ARRI Alexa 35 (Super 35 sensor)
      ||> .if focal length is set to 50mm >>
          <== vertical FOV equals 2 * atan(sensorHeight / 100) using the Alexa 35 sensor height

      # smooth focal length transition
      .if current focal length is 35mm
      ||> .if user selects 85mm preset >>
          <== focal length transitions smoothly from 35mm to 85mm over several frames
          <== FOV narrows progressively during transition
          !== FOV jumps instantly to the 85mm value

      # same focal length, different body = different FOV
      .if focal length is 50mm using Generic 35mm body
      ||> .if user switches body to ARRI Alexa Mini LF (large format sensor) >>
          <== FOV becomes wider because sensor is physically larger
          <== focal length remains 50mm
          <== camera info still reads 50mm

      # preset snapping
      .if user selects 24mm preset >>
          <== focal length smoothly transitions to exactly 24.0mm
          <== camera info displays "24mm" once transition completes

      # extremes of range
      .if user scrolls focal length upward repeatedly >>
          <== focal length increases until reaching 400mm
          <== further scroll input produces no change
          <== FOV at 400mm is very narrow but scene remains visible

      .if user scrolls focal length downward repeatedly >>
          <== focal length decreases until reaching 14mm
          <== further scroll input produces no change
          <== FOV at 14mm is very wide, significant perspective distortion visible
    ```

  - ##### 1.1.3. Camera body and lens database (Feature)
    ***Full database of real-world camera bodies and lens sets. 200+ cameras across 17 manufacturers. 100+ lens sets including spherical primes, spherical zooms, anamorphic primes/zooms, and stills lenses adapted for cinema. See `reference data/camera-lens-database.json` for the complete database.***

    **Functional requirements:**
    - Camera body presets, each defining sensor width, height, native resolution, supported frame rates, and mount compatibility. Representative bodies include:
      - ARRI Alexa Mini LF (large format)
      - ARRI Alexa 35
      - RED V-Raptor
      - Canon C300 Mark III
      - Canon C70
      - Sony FX6
      - Sony FX3
      - Vision Research Phantom Flex4K, VEO 640, v2512, and others
      - IMAX MSM 9802, IMAX MKIV
      - Panavision Millennium DXL2, Genesis
      - Generic 35mm, Super 35mm, 16mm, Super 16mm, 8mm
    - Lens set presets, each defining a list of available focal lengths. Presets distinguish prime vs. zoom lenses. Prime lenses have a single fixed focal length. Zoom lenses cover a continuous range (e.g., 24-70mm). Focal length keyframing is only available for zoom lenses -- a prime lens is always at its fixed focal length. The database includes:
      - Spherical primes (Zeiss Master Prime, Cooke S4/i, Leica Summilux-C, Sigma Cine FF, etc.)
      - Spherical zooms (Angenieux Optimo, Fujinon Cabrio, Canon CN-E, etc.)
      - Anamorphic primes (Panavision C/G Series, Cooke Anamorphic/i, Atlas Orion, etc.)
      - Anamorphic zooms (Angenieux Optimo Anamorphic, Panavision AWZ2.3, etc.)
      - Stills lenses adapted for cinema
      - Generic Prime: 14, 18, 21, 24, 28, 35, 50, 65, 75, 85, 100, 135, 150, 200, 300, 400mm
    - Selecting a camera body immediately recalculates the FOV using the new sensor dimensions and the current focal length. The image "breathes" -- it gets wider or narrower as the sensor size changes.
    - Selecting a lens set updates which focal lengths are available for preset snapping. It does not change the current focal length. If the current focal length is not in the new lens set, it remains at its current value -- the user just cannot snap to it.
    - Both the selected camera body and lens set are displayed on the camera info.
    - The default configuration is Generic 35mm body with Generic Prime lens set.

    **Design constraints:**
    - Sensor dimensions must match published specifications for each real camera body. These are constants, not approximations.
    - The available focal lengths for each lens set must match real lenses available in that product line.

    **Expected behavior:**
    ``` python
      # body selection changes FOV
      .if current body is Super 35mm using focal length 50mm
      ||> .if user selects ARRI Alexa Mini LF >>
          <== FOV recalculates using larger sensor dimensions
          <== image appears wider (more of the scene visible)
          <== focal length remains 50mm
          <== camera info updates to show "ARRI Alexa Mini LF"

      # lens set changes available snapping points
      .if current lens set is Generic Prime
      ||> .if user selects Leica Summilux-C >>
          <== preset snap targets change to 16, 18, 21, 25, 29, 35, 40, 50, 65, 75, 100, 135mm
          <== current focal length does not change
          <== camera info updates to show "Leica Summilux-C"

      # body and lens are independent choices
      .if user selects ARRI Alexa 35 body AND Cooke S4/i lens set >>
          <== sensor dimensions from Alexa 35 are used for FOV
          <== snap points from Cooke S4/i are used for preset selection
          <== this combination works regardless of whether it is "realistic"

      # prime lens has fixed focal length
      .if user selects Zeiss Master Prime set and snaps to 50mm >>
          <== focal length is fixed at 50mm
          <== focal length cannot be keyframed or animated
          <== user must snap to a different prime to change focal length

      # defaults on fresh start
      .if application launches for the first time >>
          <== body is Generic 35mm
          <== lens set is Generic Prime
          <== focal length is 50mm
    ```

    **Error cases:**
    ``` python
      # current focal length not in new lens set
      .if current focal length is 200mm
      ||> .if user selects Leica Summilux-C (max 135mm) >>
          <== current focal length remains at 200mm
          <== lens preset snapping only offers values up to 135mm
          <== user can still manually adjust focal length to any value within 14-400mm
    ```

  - ##### 1.1.4. Focus (Feature)
    ***Click an element, and the camera smoothly pulls focus to it -- calculating the right distance to frame the subject with breathing room, like a 1st AC executing a rack focus.***

    *Related: 2.1.1 Scene elements (requires selectable elements)*

    **Functional requirements:**
    - The user triggers focus on a target element. The camera smoothly transitions to frame the subject.
    - "Framing" means: the camera looks at the center of the element's bounding volume and positions itself at a distance where the element fills a comfortable portion of the frame -- not cropped tight, not lost in empty space. There should be breathing room around the subject.
    - The focus distance is calculated from the element's bounding volume size and the camera's current FOV. Larger elements or narrower FOVs result in greater focus distance.
    - A minimum distance multiplier (e.g., 1.5x the bounding radius) ensures the camera never ends up inside or clipping through the element.
    - The transition uses smooth interpolation -- both the camera's position and its rotation change gradually, not instantly.
    - Focus respects the current focal length. Focusing on the same element at 24mm versus 85mm produces different camera positions (the 85mm must be farther away to fit the subject).
    - When focusing on an element, the depth of field focus distance also automatically moves to that element. The DOF focal plane tracks the focused subject so that it appears sharp in DOF preview without requiring a separate adjustment.
    - **Focus distance is a keyframeable timeline track**, just like camera position and rotation. When the user focuses on an element outside of playback, a focus keyframe is added at the current playhead time. This enables rack focus — animating the focus distance between two subjects across time by placing focus keyframes at different points on the timeline.
    - After focus completes, the user can immediately continue moving the camera. Focus does not lock the camera.
    - A minimum focus distance prevents the camera from ending up unreasonably close (e.g., inside a very small element).

    **Expected behavior:**
    ``` python
      # focus on an element
      .if a chair exists within the scene
      ||> .if user triggers focus on the chair >>
          <== camera smoothly rotates to face the chair's center
          <== camera smoothly moves to a distance where the chair is comfortably framed
          <== DOF focus distance moves to the chair's distance from the camera
          <== transition takes a perceptible but brief duration (not instant, not sluggish)
          <== after transition completes, camera is stationary and ready for further input

      # focus updates DOF focus distance
      .if DOF preview is active and user focuses on an element at 5 meters >>
          <== DOF focus distance is set to 5 meters
          <== the focused element appears sharp within DOF preview
          <== elements closer or farther than 5 meters appear blurred according to DOF settings

      # focal length affects focus distance
      .if current focal length is 24mm
      ||> .if user focuses on a person >>
          <== camera positions itself relatively close (wide lens, wide FOV)
      .if current focal length is then changed to 135mm
      ||> .if user focuses on the same person >>
          <== camera positions itself much farther away (long lens, narrow FOV)
          <== person appears approximately the same size on screen at both focal lengths

      # focus on a large element vs small element
      .if user focuses on a building >>
          <== camera pulls back far enough to show the full building using generous framing
      .if user focuses on a coffee cup >>
          <== camera moves much closer, using the cup's smaller bounding volume

      # user input during focus transition
      .if focus transition is animating
      ||> .if user begins a pan input >>
          <== focus transition is interrupted
          <== user's pan input takes immediate effect
          !== camera ignores user input until focus completes
    ```

    **Error cases:**
    ``` python
      # focus on an extremely small element
      .if user focuses on an element whose bounding volume is nearly zero >>
          <== camera moves to the minimum focus distance
          <== camera does not clip through or land inside the element

      # focus when no element is targeted
      .if user triggers focus without a valid target element >>
          <== nothing happens
          <== camera remains at its current position and orientation
    ```

  - ##### 1.1.5. Depth of field preview (Feature)
    ***See what's sharp and what's soft before you get to set. Adjust aperture and see the DOF change in real time -- not photorealistic bokeh, but enough to make decisions about what the audience's eye will be drawn to.***

    *Blocked by: 1.1.2 Lens system (focal length), 1.1.4 Focus (focus distance)*

    **Functional requirements:**
    - Depth of field preview is a toggleable visual mode. When off, everything renders sharp (the default). When on, elements outside the plane of focus appear progressively blurred.
    - DOF is controlled by three parameters: focal length (from the lens system), aperture (f-stop), and focus distance. Changing any parameter updates the DOF preview in real time.
    - Aperture stops match real cinema lenses: f/1.4, f/2, f/2.8, f/4, f/5.6, f/8, f/11, f/16, f/22. Aperture snaps between these discrete stops rather than sliding continuously.
    - Wider apertures (lower f-numbers) produce shallower DOF. Narrower apertures (higher f-numbers) produce deeper DOF. At f/22, nearly everything should appear in focus.
    - Longer focal lengths produce shallower DOF at the same aperture and focus distance. A 135mm at f/2 has dramatically less depth of field than a 24mm at f/2.
    - The preview does not need to be photorealistic. The goal is creative decision-making: the user should clearly see the boundary between sharp and soft, and understand how changing aperture or focal length shifts that boundary. Smooth blur falloff is preferred over hard cutoffs.
    - Bokeh quality (shape, highlights, chromatic effects) is not a requirement. A simple Gaussian-style blur that increases with distance from the focal plane is sufficient. The user needs to see "this is sharp, this is soft" -- they do not need to evaluate bokeh character.
    - The aperture value is displayed on the camera info when DOF preview is active.

    **Design constraints:**
    - DOF preview must maintain interactive frame rates. If the preview causes significant performance degradation, it is worse than no preview at all -- a DP will turn it off and never use it.
    - Default aperture is f/5.6 (a common practical stop that produces visible but moderate DOF).

    **Expected behavior:**
    ``` python
      # DOF preview toggle
      .if DOF preview is off
      ||> .if user enables DOF preview >>
          <== elements at the focus distance remain sharp
          <== elements closer and farther than focus distance appear blurred
          <== blur intensity increases using distance from focus plane

      # aperture affects DOF via discrete stops
      .if DOF preview is on using f/5.6
      ||> .if user steps aperture wider >>
          <== aperture snaps to f/4
          <== DOF becomes shallower
      ||> .if user steps aperture wider again >>
          <== aperture snaps to f/2.8
          !== aperture slides to an intermediate value like f/3.2

      .if DOF preview is on using f/1.4
      ||> .if user steps aperture narrower repeatedly >>
          <== aperture snaps through f/2, f/2.8, f/4, f/5.6, f/8, f/11, f/16, f/22
          <== DOF becomes progressively deeper at each stop
          <== at f/22, nearly every element within the scene appears sharp

      # focal length interaction
      .if aperture is f/2.8 using focal length 24mm >>
          <== DOF is moderately deep (wide lenses have inherently deeper DOF)
      .if aperture is f/2.8 using focal length 135mm >>
          <== DOF is much shallower (long lenses compress DOF)

      # real-time parameter changes
      .if user steps through aperture stops while viewing the scene >>
          <== blur amount updates immediately on each stop change
          !== blur waits until user finishes adjusting to update

      # turning DOF preview off
      .if DOF preview is on
      ||> .if user disables DOF preview >>
          <== all elements render sharp regardless of aperture setting
          <== aperture value is hidden from camera info
          <== aperture setting is preserved (re-enabling restores the last value)
    ```

  - ##### 1.1.6. Camera shake (Feature)
    ***Add procedural handheld shake to the camera. Configurable intensity and speed -- from a barely perceptible documentary tremor to a chaotic run-and-gun feel.***

    **Functional requirements:**
    - Camera shake is a toggleable effect. When enabled, it applies continuous procedural rotation noise to the camera on the X (tilt) and Y (pan) axes only. No roll. No positional shake.
    - Shake is driven by noise with two user-facing parameters: amplitude (how far the camera drifts from center) and frequency (how fast it oscillates).
    - Low amplitude + low frequency produces a subtle steadicam drift. High amplitude + high frequency produces an aggressive handheld feel.
    - Shake is additive. It applies on top of the camera's actual position and rotation without altering the underlying values. If the user disables shake, the camera returns to exactly where it was.
    - Shake is cosmetic only -- it is not recorded as keyframes (even when the camera track's stopwatch is on). It is baked to keyframes only during export. During interactive use and playback, shake is a preview effect layered on top of the camera animation data.
    - Shake has per-shot enable/disable. Each shot in the timeline independently tracks whether shake is on or off, along with its own amplitude and frequency settings.
    - Shake operates during both interactive mode and playback. During playback, it layers on top of the keyframed camera animation.
    - When shake is disabled, its removal is instant -- no fade-out, no settling.

    **Expected behavior:**
    ``` python
      # enabling shake
      .if camera is stationary
      ||> .if user enables shake >>
          <== camera begins continuously oscillating within pan and tilt
          <== oscillation appears organic and non-repeating over short time spans
          !== camera shakes within roll, yaw, or positional axes

      # amplitude and frequency
      .if shake is enabled using low amplitude and low frequency >>
          <== camera drifts subtly, resembling a steadicam operator breathing
      .if shake is enabled using high amplitude and high frequency >>
          <== camera jitters aggressively, resembling handheld run-and-gun footage

      # shake does not affect underlying camera state
      .if shake is enabled and camera is at position P using rotation R
      ||> .if user disables shake >>
          <== camera returns to exactly position P using rotation R
          !== camera ends up at a slightly offset position

      # shake during playback
      .if a keyframed camera animation is playing back
      ||> .if user enables shake >>
          <== shake layers on top of the animated camera path
          <== the underlying animation plays identically to when shake is off

      # shake is not recorded
      .if shake is enabled
      ||> .if the stopwatch is on and a camera keyframe is created >>
          <== the recorded position and rotation reflect the true camera state, not the shaken state

      # per-shot shake state
      .if shot A has shake enabled using high amplitude
      ||> .if user switches to shot B which has shake disabled >>
          <== shake stops immediately on shot B
          <== switching back to shot A restores shake with its amplitude setting
      .if shot C has shake enabled using low amplitude >>
          <== shot C uses its own shake settings independently of shot A

      # shake baked on export
      .if shake is enabled on a shot
      ||> .if user exports the project >>
          <== exported keyframe data includes shake baked into camera position and rotation
          <== the exported animation matches what was seen during playback with shake enabled
      .if shake is disabled on a shot
      ||> .if user exports the project >>
          <== exported keyframe data contains only the clean camera animation
          <== no shake is present in the exported data
    ```

  - ##### 1.1.7. Camera info HUD (Feature) — *Relocated to 1.2.3*
    > **Note**: The camera info HUD has been moved to milestone 1.2 (Camera Overlays) as feature 1.2.3, since it is an overlay. The specification below is retained for reference but the canonical location is the Camera Overlays spec. The camera info now also displays squeeze factor when an anamorphic lens is active.

    ***An overlay showing the data a DP would read off an on-set monitor: focal length, camera height, angle of view, aspect ratio, body, and lens set. Toggleable via shortcut, glanceable, not intrusive.***

    *Blocked by: 1.1.2 Lens system, 1.1.3 Camera body and lens presets, 1.2.1 Aspect ratio masks*

    **Functional requirements:**
    - The camera info is a non-interactive overlay positioned at the top-right of the Camera View. See `ui-layout-spec.md` §3.3 for layout details.
    - The camera info is toggleable via a keyboard shortcut. It is not always visible -- the user can show or hide it as needed.
    - The camera info counts as an overlay for export purposes. When visible during export, it renders into the exported output. When hidden, it does not.
    - The camera info displays the following information, updated every frame:
      - **Focal length**: current value in millimeters, rounded to the nearest integer (e.g., "50mm"). During animated transitions, this updates in real time.
      - **Camera height**: the camera's Y position in meters above the ground plane (e.g., "1.6m"). Negative values displayed when below ground. This tells the director if they're at eye level, low angle, or bird's eye.
      - **Angle of view**: the horizontal angle of view in degrees, derived from focal length and sensor width (e.g., "39.6deg"). This is horizontal AOV because that's what DPs reference when discussing framing width.
      - **Aspect ratio**: the currently selected aspect ratio from the overlay system (e.g., "2.39:1"). Displays the ratio name, not raw numbers.
      - **Camera body**: the name of the selected camera body preset (e.g., "ARRI Alexa 35").
      - **Lens set**: the name of the selected lens set preset (e.g., "Cooke S4/i").
    - When depth of field preview is active, the camera info additionally displays:
      - **Aperture**: the current f-stop (e.g., "f/2.8").
    - The camera info renders above all scene content and overlays. It is always legible regardless of the scene behind it when visible.

    **Design constraints:**
    - The camera info must not interfere with framing composition. It sits outside or at the edge of the primary composition area.
    - Text must be legible at a glance -- sufficient size and contrast against any background. A subtle background panel behind the text ensures readability against bright scenes.

    **Expected behavior:**
    ``` python
      # camera info toggle
      .if camera info is hidden
      ||> .if user presses camera info toggle shortcut >>
          <== camera info becomes visible at the top-right of the Camera View
          <== all camera info fields are displayed and updating
      .if camera info is visible
      ||> .if user presses camera info toggle shortcut >>
          <== camera info is hidden
          <== no camera info is displayed on screen

      # camera info updates in real time
      .if camera info is visible
      ||> .if user adjusts focal length from 35mm to 85mm smoothly >>
          <== camera info focal length display updates continuously during transition
          <== angle of view display updates continuously during transition

      # camera height reflects vertical position
      .if camera info is visible
      ||> .if user cranes camera upward from 1.6m to 4.0m >>
          <== camera info height display changes from "1.6m" to "4.0m" progressively
      .if user cranes camera below the ground plane >>
          <== camera info height displays a negative value (e.g., "-0.5m")

      # body and lens set update on selection
      .if camera info is visible
      ||> .if user selects RED V-Raptor body and Zeiss Master Prime lens set >>
          <== camera info displays "RED V-Raptor" for body
          <== camera info displays "Zeiss Master Prime" for lens set
          <== angle of view recalculates for V-Raptor sensor dimensions

      # aperture shown conditionally
      .if camera info is visible and DOF preview is off >>
          <== camera info does not display aperture
      .if camera info is visible and DOF preview is turned on >>
          <== camera info begins displaying current aperture value (e.g., "f/5.6")

      # camera info visible over any scene content
      .if camera info is visible and scene behind camera info area is very bright (white wall, sky) >>
          <== camera info text remains legible due to background panel
      .if camera info is visible and scene behind camera info area is very dark >>
          <== camera info text remains legible

      # camera info as export overlay
      .if camera info is visible during export >>
          <== camera info renders into the exported frames
      .if camera info is hidden during export >>
          <== exported frames contain no camera info overlay
    ```
