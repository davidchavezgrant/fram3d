# Milestone 8.1: Selection and Manipulation Refinements

**Date**: 2026-03-10
**Status**: Draft
**Milestone**: 8.1 — Selection and manipulation refinements
**Phase**: 8 — Polish & Views

---

- ### 8.1. Selection and manipulation refinements (Milestone)
	*Quality-of-life improvements to element interaction as scenes get more complex.*

	A director blocking a dinner scene has a table, four chairs, plates, glasses, and centerpiece. Moving all of that across the room means nine individual drag operations. Lining up furniture against a wall means eyeballing it. And every camera move interpolates the same way -- mechanical, uniform, lifeless. These three features address the friction that accumulates as scenes grow from a chair and a camera into actual sets.

	None of these features are deep. Multi-select extends an existing interaction (single-select) to handle groups. Grid snapping adds precision to an existing operation (translation). Interpolation curves add expression to an existing system (keyframe animation). They are refinements, not new systems -- and the spec is proportional.

	*Blocked by: 2.1 (scene management -- selection and gizmos must exist), 3.2 (keyframe animation -- curves apply to keyframes)*

	---

	- ##### 8.1.1. Multi-select (Feature)
		***Select multiple elements and manipulate them as a group. Shift-click to build a selection incrementally, or drag a marquee to grab everything in an area. Gizmos operate on the whole group.***

		*Related:
		- 2.1.1 (scene elements -- extends single-select)
		- 2.1.2 (transform gizmos -- gizmos now operate on groups)
		- 6.2 (scene hierarchy -- parent/child groups are different from multi-select groups)*

		**Functional requirements:**
		- Clicking an unselected element while holding Shift adds it to the current selection without deselecting anything
		- Clicking a selected element while holding Shift removes it from the current selection
		- Clicking an element without Shift replaces the entire selection with that single element (existing 2.1.1 behavior, preserved)
		- Clicking empty space without Shift deselects everything (existing 2.1.1 behavior, preserved)
		- Clicking empty space with Shift held does nothing -- the current selection is preserved
		- Marquee selection: clicking and dragging on empty space draws a rectangular selection region on screen
			- Drag-select (marquee) uses partial intersection / crossing selection -- an element only needs to be partially within the marquee rectangle to be selected, not fully enclosed
			- Without Shift: replaces the current selection
			- With Shift: adds to the current selection (does not remove elements already selected that fall outside the marquee)
			- The marquee rectangle is drawn as a visible dashed outline during the drag
			- Releasing the mouse button completes the marquee and evaluates the selection
		- When multiple elements are selected, the active gizmo appears at the center of the selection's bounding box
			- Multi-select gizmo pivot is at the center of the selection bounding box (no option to pivot at first-selected element)
			- "Center" means the midpoint of the axis-aligned bounding box enclosing all selected elements
		- Dragging a gizmo handle applies the transform to every selected element
			- Translation: all elements move by the same offset
			- Rotation: all elements rotate around the gizmo's position (the selection center), not around their individual origins
			- Scale: all elements scale relative to the gizmo's position (the selection center)
		- Ctrl+D (Cmd+D on macOS) duplicates all selected elements
			- The duplicates become the new selection
			- Each duplicate is offset from its original by the same offset vector
		- Ctrl+A (Cmd+A on macOS) selects all scene elements
		- The selection count is visible in the UI (e.g., "3 elements selected")

		**Design constraints:**
		- Marquee drag must not conflict with camera controls. Camera modifier-key drags (Ctrl-drag for pan/tilt, Alt-drag for orbit) take priority over marquee initiation
		- Marquee drag must not conflict with gizmo interaction. If a gizmo handle is under the cursor, gizmo drag takes priority
		- Selection highlighting applies independently to each selected element -- every selected element shows the selection highlight from 2.1.1

		**Expected behavior:**
		``` python
			# Shift-click to build selection
			.if user clicks element A (no modifier) >>
				<== element A selected
			||> .if user Shift-clicks element B >>
				<== element A and element B both selected
				<== gizmo appears at center between A and B
			||> .if user Shift-clicks element A >>
				<== element A removed from selection
				<== element B remains selected
				<== gizmo moves to element B's position

			# Marquee selection (crossing / partial intersection)
			.if user clicks and drags on empty space (no modifier keys) >>
				<== dashed rectangle drawn from drag start to current cursor position
				||> .if user releases mouse >>
					<== all elements whose bounds partially intersect the rectangle become selected
					<== elements fully outside the rectangle are not selected
					<== gizmo appears at center of the group
					!== only fully enclosed elements are selected

			# Marquee with Shift
			.if elements A and B are selected >>
				||> .if user Shift-drags a marquee that contains elements C and D >>
					<== A, B, C, and D are all selected
					!== A and B deselected

			# Gizmo pivot at bounding box center (no first-selected option)
			.if elements A (at x=0) and B (at x=4) are selected >>
				<== gizmo appears at x=2 (bounding box center)
				!== gizmo appears at A's position (first-selected)
				||> .if user Shift-clicks element C (at x=6) >>
					<== gizmo moves to x=3 (new bounding box center of A, B, C)

			# Group translation
			.if elements A, B, and C are selected >>
				||> .if user drags the translate gizmo along X by 2 units >>
					<== A moves +2 on X from its original position
					<== B moves +2 on X from its original position
					<== C moves +2 on X from its original position
					<== gizmo moves +2 on X

			# Group rotation
			.if elements A and B are selected, gizmo at center point P >>
				||> .if user drags the Y rotation ring >>
					<== A orbits around point P along the Y axis
					<== B orbits around point P along the Y axis
					<== each element's own rotation also updates
					!== elements rotate only around their own origins

			# Group duplication
			.if elements A and B are selected >>
				||> .if user presses Ctrl+D >>
					<== "A (1)" and "B (1)" appear offset from originals
					<== "A (1)" and "B (1)" become the new selection
					<== spatial relationship between A (1) and B (1) matches A and B

			# Select all
			.if scene contains elements A, B, C, D >>
				||> .if user presses Ctrl+A >>
					<== all four elements selected
					<== gizmo appears at center of all four

			# Click to reset to single selection
			.if elements A, B, and C are selected >>
				||> .if user clicks element B (no modifier) >>
					<== only element B selected
					<== A and C deselected
		```

		**Error cases:**
		``` python
			# Marquee with no elements inside
			.if user drags a marquee in an empty area of the view >>
				<== no elements selected after release
				<== previous selection cleared (no Shift held)

			# Shift-click empty space
			.if elements A and B are selected >>
				||> .if user Shift-clicks empty space >>
					<== selection unchanged (A and B remain selected)
					!== selection cleared

			# Gizmo priority over marquee
			.if an element is selected and gizmo is visible >>
				||> .if user clicks and drags starting on a gizmo handle >>
					<== gizmo drag initiated
					!== marquee selection initiated
		```

	---

	- ##### 8.1.2. Grid snapping (Feature)
		***Snap elements to a configurable spatial grid for precise alignment. A visual grid overlay shows the snap points. Hold a modifier key to temporarily move freely without snapping.***

		*Related:
		- 2.1.2 (transform gizmos -- snapping applies during gizmo drags)
		- 2.1.3 (ground plane -- the visual grid overlay extends the ground plane grid)*

		**Functional requirements:**
		- When snapping is enabled, element translation via gizmo drag snaps to the nearest grid increment
			- Snapping applies per-axis: the element position rounds to the nearest grid value on each axis independently
			- Snapping occurs during the drag, not just on release -- the element visibly jumps between grid positions as the user drags
		- Rotation snapping: when enabled, rotation via gizmo drag snaps to the nearest angle increment
			- Default rotation increment: 15 degrees, user-configurable
			- Configurable: 5, 10, 15, 30, 45, 90 degrees
		- Scale snapping: when enabled, scale via gizmo drag snaps to the nearest scale increment
			- Default scale increment: 0.25
			- Configurable: 0.1, 0.25, 0.5, 1.0
		- Grid snap sizes are fixed presets: 0.1m, 0.25m, 0.5m, 1.0m, 2.0m. No custom values.
			- Default: 0.5 meters
			- Grid size affects translation snapping only -- rotation and scale have their own increments
		- Snapping is toggled globally via a toolbar button or keyboard shortcut
		- Holding a modifier key (Alt) temporarily inverts the snap state during a drag
			- If snapping is on, holding Alt allows free movement for this drag
			- If snapping is off, holding Alt enables snapping for this drag
		- Visual grid overlay:
			- When snapping is enabled, a grid overlay appears on the ground plane matching the configured grid size
			- Grid lines are subtle and visually distinct from the ground plane's default reference grid (2.1.3)
			- The snap grid replaces the ground plane reference grid when active -- the user sees one grid, not two overlapping grids
			- Grid overlay is visible only when snapping is enabled
		- Snapping applies equally to multi-select operations (8.1.1) -- the gizmo position snaps, and all selected elements move by the snapped offset

		**Design constraints:**
		- The Alt modifier for snap override must not conflict with Alt-drag for orbit. Snap override applies only while actively dragging a gizmo handle; Alt-drag starting on empty space or the view remains orbit
		- Grid configuration UI should be minimal -- a dropdown or small panel, not a full settings dialog

		**Expected behavior:**
		``` python
			# Translation snapping
			.if snapping enabled with 0.5m grid >>
				||> .if user drags an element along the X axis >>
					<== element X position jumps in 0.5m increments
					<== element appears to slide between grid positions
					!== element moves smoothly between grid points

			# Rotation snapping (user-configurable default)
			.if snapping enabled with 15-degree rotation increment >>
				||> .if user drags the Y rotation ring >>
					<== element rotation jumps in 15-degree increments
					<== rotation values are always multiples of 15

			# User changes rotation snap increment
			.if user changes rotation snap from 15 to 45 degrees >>
				||> .if user drags the Y rotation ring >>
					<== element rotation jumps in 45-degree increments
					<== rotation values are always multiples of 45

			# Override with modifier
			.if snapping enabled >>
				||> .if user holds Alt and drags the translate gizmo >>
					<== element moves freely, no snapping
					||> .if user releases Alt while still dragging >>
						<== element snaps to nearest grid position
						<== snapped movement resumes

			# Visual grid matches snap grid
			.if user changes grid size from 0.5m to 1.0m >>
				<== grid overlay updates to show 1.0m spacing
				<== translation snapping now uses 1.0m increments

			# Fixed preset grid sizes only
			.if user opens grid size selector >>
				<== options shown: 0.1m, 0.25m, 0.5m, 1.0m, 2.0m
				!== free-form text input for custom grid size

			# Snapping off with Alt override
			.if snapping disabled >>
				||> .if user holds Alt and drags the translate gizmo >>
					<== element snaps to grid during this drag
					||> .if user releases Alt >>
						<== element resumes free movement

			# Grid visibility
			.if user enables snapping >>
				<== snap grid overlay appears on ground plane
				<== ground plane reference grid is replaced by snap grid
			.if user disables snapping >>
				<== snap grid overlay disappears
				<== ground plane reference grid returns

			# Snapping with multi-select
			.if snapping enabled with 0.5m grid >>
				.if three elements are selected >>
					||> .if user drags translate gizmo along X >>
						<== gizmo position snaps to 0.5m grid
						<== all three elements move by the same snapped offset
						<== relative positions between elements preserved exactly
		```

		**Error cases:**
		``` python
			# Alt-drag on empty space (orbit, not snap override)
			.if snapping enabled >>
				||> .if user Alt-drags starting on empty view space >>
					<== camera orbits (standard behavior)
					!== snap override activated
					!== grid behavior changes

			# Snap with multi-select
			.if snapping enabled with 0.5m grid >>
				.if three objects are selected >>
					||> .if user drags translate gizmo along X >>
						<== gizmo position snaps to 0.5m grid
						<== all three objects move by the same snapped offset
						<== relative positions between objects preserved exactly
		```

	---

	- ##### 8.1.3. Custom interpolation curves (Feature)
		***Control how the camera and objects ease between keyframes. Per-property easing lets each animated property -- position, rotation, focal length -- have its own interpolation curve independently. The difference between a camera that mechanically slides from A to B and one that gently accelerates, glides, then settles into position -- the way a dolly grip actually pushes a dolly.***

		*Blocked by:
		- 3.2.5 (interpolation and playback -- the current system uses a single interpolation mode)*

		*Related:
		- 3.2.2 (tracks and keyframes -- curves are set per-property on each keyframe)
		- 3.2.4 (keyframe interaction -- curve type is a property of a selected keyframe, configurable per animated property)*

		**Functional requirements:**
		- Each animated property on a keyframe has its own interpolation curve that controls how that property transitions from this keyframe to the next
			- Properties are independent: position, rotation, and focal length can each have different easing curves on the same keyframe
			- The curve defines the easing behavior for the outgoing segment (from this keyframe to the next)
			- The last keyframe in a track has no outgoing curve (there is no next keyframe to transition to)
		- Five curve presets:
			- **Linear**: constant speed from keyframe to keyframe. No acceleration, no deceleration. This is the default for new keyframes.
			- **Ease in**: starts slow, accelerates to full speed. The camera gathers momentum. Like a dolly beginning its push.
			- **Ease out**: starts at full speed, decelerates to a stop. The camera settles into position. Like a dolly arriving at its mark.
			- **Ease in-out**: starts slow, reaches full speed in the middle, decelerates at the end. The most natural-feeling motion for most camera moves. This is how a skilled dolly grip operates.
			- **Bezier**: user-defined curve with two control handles. Full control over acceleration and deceleration profile.
		- Default interpolation for new keyframes: linear
			- All properties on a new keyframe default to linear
			- Users can switch to other easing types via a preference setting that changes the default for newly created keyframes
		- Selecting a keyframe in the timeline shows the current curve type for each property
			- For presets (linear, ease in, ease out, ease in-out): a dropdown or button group per property to switch between them
			- For bezier: opens the curve editor for that specific property
		- The curve type can be changed on multiple selected keyframes at once
			- Selecting several keyframes and choosing "Ease out" applies ease out to the selected property across all of them
			- Bezier cannot be bulk-applied (each bezier curve is unique to its keyframe/property)

		**Curve editor:**
		- The curve editor displays a graph showing the interpolation curve for the selected property on the selected keyframe
			- X axis: time (normalized 0 to 1, representing the span from this keyframe to the next)
			- Y axis: value (normalized 0 to 1, representing the interpolation factor from this keyframe's value to the next keyframe's value)
		- For presets, the curve editor shows the curve shape as read-only -- the user sees what linear, ease in, etc. look like but cannot drag the curve. They switch presets via the preset controls.
		- For bezier mode, two control handles appear on the curve
			- Each handle is draggable
			- The handles define a cubic bezier curve
			- Default bezier handle positions match ease in-out (the user starts with a natural curve and adjusts)
			- Handles are constrained: the X value of each handle stays within the 0-1 range (no backward-in-time curves)
			- Y values are constrained to 0-1 by default, with a checkbox to allow overshoot (values outside 0-1, enabling effects like the camera overshooting its target and settling back)
		- The curve editor appears inline in the timeline area, below the track containing the selected keyframe
			- It does not open as a separate window or modal
			- It collapses when no keyframe is selected or when the user clicks away
		- The curve editor height is fixed and compact -- tall enough to manipulate handles, short enough not to dominate the timeline

		**Design constraints:**
		- Per-property easing: each animated property (position, rotation, focal length) can have its own easing curve independently. This allows natural combinations like a camera that eases into position but rotates at constant speed.
		- The curve editor must be usable without understanding bezier math. The preset buttons handle 90% of use cases. Bezier is for users who want fine control.

		**Expected behavior:**
		``` python
			# Default behavior -- linear
			.if user creates a new keyframe (any track) >>
				<== all properties on the keyframe default to linear interpolation
				!== properties default to ease in-out

			# User changes default easing preference
			.if user sets default easing preference to "Ease In-Out" >>
				||> .if user creates a new keyframe >>
					<== all properties on the keyframe default to ease in-out
					<== existing keyframes are not affected

			# Per-property easing
			.if user selects a camera keyframe >>
				<== curve type shown for each property: position, rotation, focal length
				||> .if user sets position to "Ease In-Out" and rotation to "Linear" >>
					<== position eases in-out between this keyframe and the next
					<== rotation interpolates at constant speed between this keyframe and the next
					<== focal length retains its own independent curve setting

			# Per-property independence across keyframes
			.if camera keyframe A has position=ease-in, rotation=linear >>
				.if camera keyframe B has position=ease-out, rotation=ease-in-out >>
					||> .if playback runs A to B >>
						<== camera position eases in (starts slow, accelerates)
						<== camera rotation is constant speed
					||> .if playback runs B to C >>
						<== camera position eases out (starts fast, decelerates)
						<== camera rotation eases in-out

			# Preset selection
			.if user selects a keyframe in the timeline >>
				<== curve type indicator shows current type per property
				||> .if user switches position to "Linear" >>
					<== position playback between this keyframe and the next is constant speed
					<== rotation and focal length curves unchanged

			# Ease in behavior
			.if keyframe A at t=0 has position ease-in curve, keyframe B at t=2 >>
				||> .if playback runs from A to B >>
					<== position movement starts slowly near keyframe A
					<== position movement accelerates and reaches full speed arriving at keyframe B
					!== movement is constant speed throughout

			# Ease out behavior
			.if keyframe A at t=0 has position ease-out curve, keyframe B at t=2 >>
				||> .if playback runs from A to B >>
					<== position movement starts at full speed leaving keyframe A
					<== position movement decelerates and arrives gently at keyframe B

			# Ease in-out behavior
			.if keyframe A at t=0 has position ease-in-out curve, keyframe B at t=2 >>
				||> .if playback runs from A to B >>
					<== position movement starts slowly near keyframe A
					<== position movement reaches full speed in the middle
					<== position movement decelerates approaching keyframe B

			# Bezier curve editing
			.if user selects a keyframe and switches position to "Bezier" >>
				<== curve editor appears inline below the track for the position property
				<== two draggable control handles visible on the curve
				<== default handle positions produce ease in-out shape
				||> .if user drags a control handle >>
					<== curve shape updates in real time
					<== position playback behavior updates to match the new curve

			# Bezier overshoot checkbox
			.if user is editing a bezier curve for position >>
				<== Y values constrained to 0-1 by default
				||> .if user checks "Allow overshoot" >>
					<== control handle Y values can now exceed 1.0 or go below 0.0
					<== enables camera to overshoot its target position and settle back
				||> .if user unchecks "Allow overshoot" >>
					<== control handle Y values clamped back to 0-1
					<== any handles outside 0-1 are snapped to the nearest boundary

			# Curve editor for presets (read-only)
			.if user selects a keyframe with position set to "Ease In" preset >>
				<== curve editor shows the ease-in curve shape for position
				!== control handles are draggable
				<== switching to Bezier makes handles draggable

			# Bulk curve assignment
			.if user selects keyframes A, B, and C >>
				||> .if user chooses "Ease Out" for the rotation property >>
					<== rotation on all three keyframes switches to ease out
					<== position and focal length curves on all three keyframes unchanged
					<== rotation playback between each pair uses ease out

			# Per-segment independence
			.if track has keyframes A (position=ease in), B (position=linear), C (position=ease out) >>
				||> .if playback runs A to B to C >>
					<== A-to-B segment position eases in
					<== B-to-C segment position is linear
					!== all segments use the same curve

			# Last keyframe has no outgoing curve
			.if user selects the last keyframe in a track >>
				<== curve type controls are disabled or hidden for all properties
				<== no curve editor shown (no outgoing segment)
		```

		**Error cases:**
		``` python
			# Bezier handle X constrained to valid range
			.if user drags a bezier handle's X value beyond 1.0 >>
				<== handle clamps at X = 1.0
				!== handle moves beyond the time range
				!== curve loops backward in time

			.if user drags a bezier handle's X value below 0.0 >>
				<== handle clamps at X = 0.0

			# Bezier handle Y constrained by default
			.if "Allow overshoot" is unchecked >>
				.if user drags a bezier handle's Y value beyond 1.0 >>
					<== handle clamps at Y = 1.0
				.if user drags a bezier handle's Y value below 0.0 >>
					<== handle clamps at Y = 0.0

			# Bezier handle Y unconstrained with overshoot
			.if "Allow overshoot" is checked >>
				.if user drags a bezier handle's Y value to 1.3 >>
					<== handle stays at Y = 1.3
					<== property will overshoot its target value during playback

			# Single keyframe in track
			.if a track has only one keyframe >>
				||> .if user selects it >>
					<== curve type controls disabled for all properties (no next keyframe to transition to)

			# Bezier bulk-apply blocked
			.if user selects multiple keyframes >>
				||> .if user attempts to switch any property to Bezier >>
					<== bezier option is unavailable for bulk selection
					<== preset options remain available
		```

---

## Acceptance Criteria

### Multi-select

1. User clicks object A, then Shift-clicks objects B and C.
   ``` python
   <== three objects selected, gizmo at their bounding box center
   <== all three show selection highlight
   ```

2. User drags a marquee that partially intersects five objects in a furniture cluster.
   ``` python
   <== all five selected in one operation (crossing/partial intersection)
   !== only fully enclosed objects selected
   ```

3. User translates the multi-selection via gizmo.
   ``` python
   <== all selected objects move uniformly
   <== relative positions between objects preserved
   ```

4. User rotates a multi-selection around the selection center.
   ``` python
   <== objects orbit the center point, not their individual origins
   ```

5. User Ctrl+D duplicates a multi-selection.
   ``` python
   <== duplicates appear offset, preserving the group's spatial arrangement
   <== duplicates become the new selection
   ```

### Grid snapping

6. User enables snapping with 0.5m grid and translates an object.
   ``` python
   <== object position jumps in 0.5m increments during drag
   ```

7. User holds Alt while dragging with snapping enabled.
   ``` python
   <== object moves freely, ignoring the grid
   ```

8. User changes grid size from 0.5m to 1.0m.
   ``` python
   <== grid overlay updates to 1.0m spacing
   <== next translation snaps at 1.0m increments
   <== grid size selector shows only fixed presets (0.1m, 0.25m, 0.5m, 1.0m, 2.0m)
   ```

9. User enables rotation snapping at 15 degrees (default, user-configurable) and rotates an object.
   ``` python
   <== rotation jumps in 15-degree increments
   ```

### Custom interpolation curves

10. User creates three keyframes. Default interpolation is linear.
    ``` python
    <== playback between keyframes is constant speed
    !== playback uses ease in-out by default
    ```

11. User selects a keyframe and sets position to ease-in-out, rotation to linear.
    ``` python
    <== position playback between that keyframe and the next shows acceleration and deceleration
    <== rotation playback between that keyframe and the next is constant speed
    ```

12. User selects a keyframe and switches position to bezier, then adjusts handles.
    ``` python
    <== curve editor appears inline for the position property
    <== dragging handles changes the curve shape
    <== playback reflects the custom curve
    <== Y values constrained to 0-1 by default
    ```

13. User checks "Allow overshoot" on a bezier curve.
    ``` python
    <== control handles can be dragged outside 0-1 Y range
    <== property overshoots its target value and settles back during playback
    ```

14. User selects three keyframes and applies ease out to the rotation property.
    ``` python
    <== rotation on all three keyframes switches to ease out in one operation
    <== position and focal length curves unchanged
    ```

15. User selects the last keyframe in a track.
    ``` python
    <== no curve type controls shown for any property (no outgoing segment)
    ```
