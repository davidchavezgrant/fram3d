# Milestone 2.1: Scene Management

**Date**: 2026-03-10
**Status**: Draft
**Milestone**: 2.1 — Scene management
**Project**: Phase 2 — The Scene

---

- ### 2.1. Scene management (Milestone)
	*Click elements, move them around, see visual feedback. Single-selection with translate/rotate/scale gizmos. A ground plane provides spatial grounding so elements don't float in void. Elements exist in world space across all shots.*

	This milestone turns the view from a passive camera view into an interactive stage. A director needs to place furniture, reposition walls, and adjust props to block a scene. Without selection and manipulation, the camera has nothing to frame. Without a ground plane, there's no sense of floor, height, or distance.

	The scope is deliberately single-selection. Multi-select, grouping, and hierarchy come later (6.3, 6.2). This milestone establishes the core interaction loop: click a thing, move it, see the result through the camera.

	**Delete behavior:** Context-sensitive delete uses two distinct shortcuts to avoid ambiguity:
	- **Delete** — deletes keyframes only (selected keyframe on timeline). If no keyframe is selected, does nothing.
	- **Cmd+Delete (Ctrl+Delete on Windows)** — deletes the selected element entirely.
	This prevents the most common delete-ambiguity complaint from After Effects, Blender, and other tools where a single Delete key must guess whether you mean the keyframe or the element.

	*Blocked by: 1.1 (virtual camera — need a view and camera to interact with elements)*

	---

	- ##### 2.1.1. Scene elements (Feature)
		***Any element with a collider is interactive. Hover highlighting, selection highlighting, click-to-select, click-empty-to-deselect.***

		*Related:
		- 2.1.2 (gizmos appear on selected elements)
		- 2.1.4 (duplication creates new scene elements)*

		**Functional requirements:**
		- Every element that has a collider is a scene element — no manual tagging required
		- Compound elements (multi-mesh models) are treated as single selectable units. The entire compound element highlights and selects as one entity.
		- Hovering over a scene element shows a highlight on that element
			- Highlight must be visually distinct from the selection highlight
		- Clicking a scene element selects it
			- Only one element can be selected at a time
			- Selecting a new element deselects the previous one
		- Clicking empty space (no scene element under cursor) deselects the current selection
		- Selection persists across camera movements (dolly, pan, orbit, etc.)
		- Selected elements display a persistent selection highlight until deselected
		- Hover highlight disappears when the cursor leaves the element
		- Hover highlight does not appear on the currently selected element (selection highlight takes priority)

		**Design constraints:**
		- Selection raycasts must ignore gizmo geometry — gizmos are interaction tools, not scene elements
		- Selection raycasts must ignore UI elements (panels, overlays, buttons)

		**Note:** Characters (Milestone 6.1) may later allow partial child selection (e.g., selecting individual limbs).

		**Expected behavior:**
		``` python
			# Basic selection
			.if user moves cursor over a scene element >>
				<== element displays hover highlight
				||> .if user clicks the element >>
					<== element displays selection highlight
					<== hover highlight replaced by selection highlight

			# Deselection
			.if an element has selection >>
				||> .if user clicks empty space >>
					<== selection highlight removed
					<== no element selected

			# Selection transfer
			.if element A has selection >>
				||> .if user clicks element B >>
					<== element A loses selection highlight
					<== element B gains selection highlight

			# Hover does not override selection
			.if element A has selection >>
				||> .if user hovers over element A >>
					<== element A keeps selection highlight
					!== hover highlight shown on element A

			# Selection survives camera movement
			.if element A has selection >>
				||> .if user dollies the camera forward >>
					<== element A retains selection highlight

			# Compound element selection
			.if user clicks any mesh within a compound element >>
				<== entire compound element highlights as one unit
				<== entire compound element selected
				!== individual child mesh selected independently
		```

		**Error cases:**
		``` python
			# Click on UI panel covering a scene element
			.if a UI panel overlaps a scene element >>
				||> .if user clicks the overlapping area >>
					<== UI panel receives the click
					!== scene element selected
					!== selection state changes

			# Click during active camera drag
			.if user holds Ctrl and drags to pan/tilt >>
				!== scene elements selected during the drag
				<== selection only evaluated on clean single clicks
		```

	---

	- ##### 2.1.2. Transform gizmos (Feature)
		***Translate (arrows), rotate (rings), scale (uniform handle). Axis-colored (RGB = XYZ) for translate and rotate, always-on-top, constant screen size. The director's tool for moving furniture around on the virtual set.***

		*Blocked by:
		- 2.1.1 (scene elements — gizmos require a selected element)*

		*Related:
		- 2.2.2 (keyboard shortcuts — Q/W/E/R switch active tool)*

		**Functional requirements:**
		- Three active tools:
			- Translate — arrow handles along each axis
			- Rotate — ring handles around each axis
			- Scale — a single uniform scale handle. Dragging it scales the element proportionally on all axes. There are no per-axis scale handles.
		- **Active tool badge**: A non-interactive overlay in the bottom-left corner of the Camera View showing the current active tool name, icon, and keyboard shortcut. Updates immediately on tool switch. Visible whenever the Camera View is active. See `ui-layout-spec.md` §3.4 for visual design.
			- Select (Q): diamond icon
			- Translate (W): crosshair icon
			- Rotate (E): circle-arrow icon
			- Scale (R): hexagon icon
		- Axis color convention: red = X, green = Y, blue = Z (applies to translate and rotate gizmos)
		- Gizmos render on top of every scene element — never occluded by geometry
		- Gizmos maintain a constant size on screen regardless of camera distance
			- An element across the room has the same size gizmo as one right next to the camera
		- Gizmos appear at the selected element's position when an element is selected
		- Gizmos disappear when no element is selected
		- Only one active tool at a time
		- Dragging a translate or rotate handle constrains movement to that axis
		- Dragging the uniform scale handle scales proportionally on all axes
		- Dragging a gizmo handle does not move the camera
		- Releasing the mouse button completes the transform operation
		- The gizmo follows the element during the drag — it does not lag behind
		- Minimum scale clamped to a small positive value — elements cannot be scaled to zero or negative

		**Design constraints:**
		- Gizmo handles must take priority over scene element selection when they overlap
			- Clicking a gizmo handle starts a drag, not a selection change
		- Gizmo interaction must not conflict with camera modifier-key controls
			- Ctrl+drag, Alt+drag, and scroll remain camera operations even when a gizmo is visible

		**Expected behavior:**
		``` python
			# Gizmo appears on selection
			.if user selects a scene element >>
				<== translate gizmo appears at the element's position

			# Axis-constrained translation
			.if translate gizmo visible >>
				||> .if user drags the red (X) arrow handle >>
					<== element moves only along the X axis
					<== gizmo moves with the element
					||> .if user releases mouse >>
						<== element stays at new position

			# Active tool switch
			.if translate gizmo visible >>
				||> .if user presses E (rotate) >>
					<== translate gizmo replaced by rotation rings
					<== rings centered on the selected element

			# Rotation
			.if rotate gizmo visible >>
				||> .if user drags the green (Y) ring >>
					<== element rotates around its Y axis
					<== rotation amount corresponds to drag distance

			# Uniform scale
			.if scale gizmo visible >>
				||> .if user drags the uniform scale handle >>
					<== element scales proportionally on all axes
					<== element cannot scale below minimum positive value
					!== element scales on one axis only

			# Gizmo priority over scene elements
			.if a gizmo handle overlaps another scene element >>
				||> .if user clicks the overlapping area >>
					<== gizmo drag initiated
					!== overlapped scene element selected

			# Camera controls unaffected
			.if gizmo visible >>
				||> .if user Ctrl-drags to pan/tilt >>
					<== camera pans and tilts
					!== gizmo interaction triggered

			# Gizmo disappears on deselection
			.if user deselects the element (clicks empty space) >>
				<== gizmo removed from view
		```

		**Error cases:**
		``` python
			# Scale toward zero
			.if user drags the uniform scale handle to shrink the element >>
				||> .if scale reaches the minimum positive value >>
					<== element stops shrinking
					<== element remains visible
					!== element scales to zero
					!== element scales to negative

			# Gizmo on element behind camera
			.if selected element moves behind the camera >>
				<== gizmo not visible (element off-screen)
				<== element remains selected
				<== gizmo reappears when camera turns to face the element
		```

	---

	- ##### 2.1.3. Ground plane (Feature)
		***Infinite ground plane with visible grid for spatial reference. The stage floor — gives directors a sense of height, distance, and blocking positions.***

		**Functional requirements:**
		- A horizontal plane at Y=0 visible within the view
		- The ground plane is always visible. There is no toggle to hide it.
		- Grid lines drawn on the plane at regular intervals
			- Major grid lines at 1-meter intervals
			- Minor grid lines subdividing each meter (optional, fainter)
		- Grid extends to the horizon — no abrupt visible edge
		- Grid lines fade with distance from the camera to avoid visual noise
			- Near the camera: fully opaque
			- At distance: gradually transparent until invisible
		- Grid does not obscure elements — elements sitting on the plane remain clearly visible
		- Ground plane receives selection raycasts — clicking the ground plane counts as clicking empty space (deselects)
		- Ground plane provides spatial reference for camera height
			- When the camera is at default height (1.6m), the grid establishes eye-level perspective

		**Expected behavior:**
		``` python
			# Spatial grounding
			.if scene loads >>
				<== ground plane visible at Y=0
				<== grid lines visible near the camera
				<== grid lines fade toward the horizon
				!== hard visible edge where the grid ends

			# Camera height context
			.if camera at default position (1.6m height) >>
				<== grid visible below the camera
				<== perspective communicates approximate eye-level

			# Deselection via ground
			.if an element has selection >>
				||> .if user clicks the ground plane (no element under cursor) >>
					<== current selection cleared

			# Objects on the plane
			.if a scene element sits on the ground plane >>
				<== element clearly visible against the grid
				<== grid lines do not render on top of the element

			# Ground plane always present
			.if scene is open >>
				<== ground plane visible
				!== option to hide the ground plane
		```

	---

	- ##### 2.1.4. Element duplication (Feature)
		***Ctrl+D to duplicate the selected element. The copy appears near the original with a slight offset so both are visible.***

		*Blocked by:
		- 2.1.1 (scene elements — must have a selected element to duplicate)*

		*Related:
		- 2.1.2 (gizmo appears on the new duplicate after it becomes selected)*

		**Functional requirements:**
		- Ctrl+D (Cmd+D on macOS) duplicates the currently selected element
		- The duplicate appears at the original's position plus a visible offset
			- Offset direction: world-space positive X and positive Z (diagonal, away from the original). The offset is always world-relative, not camera-relative — duplicates always shift in the same world direction regardless of camera orientation.
			- Offset large enough that the duplicate does not fully overlap the original
		- The duplicate becomes the selected element immediately after creation
			- The original loses selection
			- Gizmo appears on the duplicate
		- Duplicated properties:
			- Geometry and visual appearance (identical copy)
			- Current scale
			- Current rotation
		- Not duplicated:
			- Animation keyframes (duplicate starts with no animation data)
			- Name: duplicated elements receive names with incrementing numbers: Chair_1, Chair_2, Chair_3, etc.
		- The duplicate is a fully independent element — modifying it does not affect the original
		- Duplication does nothing when no element is selected
		- Ctrl+D does nothing during an active gizmo drag operation
		- Multiple consecutive Ctrl+D presses each create a new duplicate, each offset from the previous duplicate's position

		**Expected behavior:**
		``` python
			# Basic duplication
			.if user selects element "Chair" >>
				||> .if user presses Ctrl+D >>
					<== new element "Chair_1" appears near the original
					<== "Chair_1" has selection
					<== "Chair" loses selection
					<== gizmo appears on "Chair_1"
					<== "Chair_1" has same rotation and scale

			# Sequential duplication
			.if "Chair_1" has selection >>
				||> .if user presses Ctrl+D >>
					<== new element "Chair_2" appears offset from "Chair_1"
					<== "Chair_2" has selection
					!== "Chair_2" stacked exactly on "Chair_1"

			# Duplicate independence
			.if "Chair_1" exists (duplicated from "Chair") >>
				||> .if user moves "Chair_1" using translate gizmo >>
					<== "Chair" remains at its original position
					!== "Chair" moves

			# Animation not copied
			.if "Chair" has keyframed animation >>
				||> .if user presses Ctrl+D >>
					<== "Chair_1" has no animation keyframes

			# Duplication blocked during gizmo drag
			.if user is actively dragging a gizmo handle >>
				||> .if user presses Ctrl+D >>
					<== nothing happens
					!== duplicate created
					!== gizmo drag interrupted
		```

		**Error cases:**
		``` python
			# No selection
			.if no element selected >>
				||> .if user presses Ctrl+D >>
					<== nothing happens
					!== empty duplicate created
					!== error message shown

			# Ctrl+D during active gizmo drag
			.if user is mid-drag on a translate gizmo handle >>
				||> .if user presses Ctrl+D >>
					<== gizmo drag continues uninterrupted
					<== no duplicate created
		```

	---

	- ##### 2.1.5. Director View (Feature)
		***A global utility camera decoupled from the shot timeline. Lets the director orbit freely to see the full scene from any angle without affecting shot camera keyframes. Element manipulation still works — if the element's stopwatch is on, keyframes are created as normal.***

		*Related:
		- 1.1 (virtual camera rig — Director View shows the shot camera as a frustum wireframe)
		- 3.2.3 (per-track stopwatch — element transforms in Director View still create keyframes when the stopwatch is on)
		- 2.1.2 (transform gizmos — work identically in Director View)*

		**Functional requirements:**
		- Two view types: "Camera View" and "Director View"
			- Camera View: the view looks through the virtual camera rig (existing behavior). Camera movements create keyframes.
			- Director View: the view looks through a free utility camera that never creates keyframes for itself.
		- The user can switch between Camera View and Director View at any time via keyboard shortcut (`D`) or UI toggle
		- Director View camera controls:
			- Orbit, pan, and zoom to see the scene from any angle
			- The camera movement controls (pan, tilt, dolly, truck, crane, orbit) work the same way as in Camera View — they move the director camera instead of the shot camera
		- In Director View, the virtual camera rig is visible in the scene as a frustum wireframe showing where the shot camera is pointing
			- The frustum wireframe updates in real time if the shot camera has animated keyframes during playback
		- In Director View, the user can still select and manipulate elements using gizmos
			- Element transform manipulations DO create keyframes if the element's stopwatch is on (recording still applies to element transforms)
		- In Director View, the user can select the shot camera rig itself via its frustum wireframe
			- Repositioning the shot camera via gizmo DOES update the shot camera's keyframes
		- Director View preserves its own position and rotation independently from the shot camera
			- Moving the director camera does not affect the shot camera's position, rotation, or keyframes
			- Moving the shot camera (via gizmo in Director View) does not affect the director camera's position
		- Switching back to Camera View returns to looking through the virtual camera rig at its current keyframed state
		- Director View is per-session state — it does not save with the project
			- On project load, the view always starts in Camera View
			- The director camera's position/rotation is not serialized

		**Design constraints:**
		- Director View must be clearly visually distinguished from Camera View so the user always knows which view they're in
		- **Director View badge**: When Director View is active, a prominent badge reading "DIRECTOR VIEW" appears at top-center of the Camera View frame. Colored in red/pink accent to signal "you are NOT looking through the shot camera." See `ui-layout-spec.md` §3.5 for visual design.
		- The frustum wireframe must not interfere with element selection — it should be selectable like any element but should not block clicks aimed at elements behind it unless the click hits the wireframe geometry directly

		**Expected behavior:**
		``` python
			# Switching to Director View
			.if user is in Camera View >>
				||> .if user activates Director View >>
					<== view switches to the director camera
					<== shot camera rig visible as a frustum wireframe in the scene
					<== director camera starts at the shot camera's current position and rotation
					!== shot camera keyframes modified

			# Director camera movement does not keyframe
			.if user is in Director View >>
				||> .if user orbits the director camera >>
					<== view orbits freely around the scene
					!== keyframes created for any camera
					!== shot camera position or rotation affected

			# Element manipulation in Director View still keyframes
			.if user is in Director View >>
				||> .if user selects an element and drags the translate gizmo >>
					<== element moves to new position
					<== keyframe created for the element's new position (if stopwatch is on)

			# Shot camera manipulation in Director View
			.if user is in Director View >>
				||> .if user clicks the shot camera frustum wireframe >>
					<== shot camera rig selected
					<== gizmo appears on the shot camera rig
					||> .if user drags the translate gizmo >>
						<== shot camera moves to new position
						<== keyframe created for the shot camera's new position
						<== frustum wireframe updates to reflect new position
						!== director camera position affected

			# Shot camera rotation in Director View
			.if user is in Director View >>
				||> .if shot camera rig selected >>
					||> .if user switches to rotate gizmo and drags >>
						<== shot camera rotates to new orientation
						<== keyframe created for the shot camera's new rotation
						<== frustum wireframe updates to reflect new orientation

			# Switching back to Camera View
			.if user is in Director View >>
				||> .if user activates Camera View >>
					<== view switches to looking through the shot camera rig
					<== frustum wireframe no longer visible (user is now looking through it)
					<== director camera position preserved in memory for next switch

			# Director View preserves position across switches
			.if user is in Director View at position A >>
				||> .if user switches to Camera View >>
					||> .if user moves the shot camera >>
						||> .if user switches back to Director View >>
							<== director camera returns to position A
							!== director camera at the shot camera's new position

			# Playback in Director View
			.if user is in Director View >>
				||> .if user plays back the timeline >>
					<== shot camera frustum wireframe animates according to camera keyframes
					<== scene elements animate according to their keyframes
					<== director camera remains stationary (user's chosen vantage point)

			# Camera controls work the same way
			.if user is in Director View >>
				||> .if user uses dolly control >>
					<== director camera dollies forward/backward
					!== shot camera affected
				||> .if user uses truck control >>
					<== director camera trucks left/right
					!== shot camera affected
				||> .if user uses crane control >>
					<== director camera cranes up/down
					!== shot camera affected
		```

		**Error cases:**
		``` python
			# Director View state not saved
			.if user saves the project while in Director View >>
				||> .if user reloads the project >>
					<== view opens in Camera View
					!== view opens in Director View
					!== director camera position restored

			# No double-keyframing from director camera
			.if user is in Director View >>
				||> .if user pans the director camera >>
					!== shot camera keyframe created
					!== any keyframe created
					<== only the Director View changes

			# Gizmo on shot camera does not move director camera
			.if user is in Director View >>
				||> .if user drags the shot camera via translate gizmo >>
					<== shot camera moves and keyframes
					<== director camera stays at its current position
					!== director camera moves with the shot camera
		```
