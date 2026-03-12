# Milestone 6.3: Element Linking & Grouping

**Date**: 2026-03-10
**Status**: Draft
**Milestone**: 6.3 — Element linking & grouping
**Phase**: 6 — Characters

---

- ### 6.3. Element linking & grouping (Milestone)
	*Temporal element-to-character and element-to-element linking. Group elements for simultaneous transforms. The director puts a prop in a character's hand, passengers in a car, and plates on a table — and they move together.*

	A director thinks in terms of "this goes with that." A character holds a prop. Passengers ride in a car. Plates sit on a table. When the character walks, the prop comes along. When the car drives, the passengers follow.

	This milestone adds two systems:
	- **Linking** — temporal relationships where one element follows another. Links are events on the global timeline with start and end times. A sword can be linked to a character's hand at 0:05 and unlinked at 0:10. During the linked period, the sword follows the hand. Outside it, the sword is independent. Two types: element-to-character (follows a body region/bone) and element-to-element (follows the parent's transform — vehicles, moving platforms).
	- **Grouping** — persistent groups with their own transform. Single-click selects the group; double-click enters it to edit individual members. Non-group elements dim to 30% opacity when inside a group. No temporal behavior and no "following." Grouping is a convenience for simultaneous manipulation.

	**Implementation: positional attachment.** The scene graph stays flat — every element is a root-level node. During linked periods, the system overrides the child's world transform with `parent.bone.worldTransform * storedOffset` (for character links) or `parent.worldTransform * storedOffset` (for element links). No Unity `SetParent()`. No hierarchy restructuring. Linking and unlinking are data changes (add/remove timeline events), not structural changes. For chain links (A follows B follows C), evaluation is in dependency order (parents before children). Max chain depth: 4.

	*Blocked by: 2.1 (scene management — linking extends the scene element model), 3.2 (keyframe animation — link/unlink events live on the global timeline)*

	---

	- ##### 6.3.1. Element Linking (Feature)
		***Drag an element onto a character (or another element) to create a temporal link. The linked element follows the parent for the duration of the link. Unlink via right-click. Links are events on the global element timeline — a prop can be linked to one character, unlinked, then linked to another.***

		*Related:
		- 6.3.2 (element grouping — persistent multiselect, separate from temporal linking)
		- 6.3.3 (Elements panel — link tool for linking, shows link state)
		- 3.2.2 (animation tracks — link/unlink events appear on the global timeline)
		- 6.1.3 (custom posing — props can be linked to character body regions)*

		**Linking types:**
		- **Element-to-character**: The child follows a character's body region (hand, head, torso, etc.). The child's position is determined by the bone's world transform + the child's anchor offset. Use cases: holding props (sword in hand), wearing items (hat on head), carrying elements.
		- **Element-to-element**: The child follows the parent element's world transform + the child's anchor offset. Use cases: passengers in a moving car, props on a moving table, elements on a boat or skateboard.

		**Anchor point:**
		- Every linkable element has an **anchor point** — an XYZ offset that defines "where on this element does the attachment happen"
		- The anchor point is a persistent property of the element, displayed as three number fields in the element's properties sidebar (not a view gizmo)
		- Default anchor point: the element's center (bounding box center), equivalent to (0, 0, 0)
		- The anchor point fields are hidden (or disabled) until the element has been linked at least once. Before any link, there is nothing to anchor to
		- When an element is linked, the parent's attachment point (bone position for characters, transform origin for elements) aligns with the child's anchor point
		- The user can adjust the anchor point at any time, including while the element is linked — the linked position updates immediately
		- The anchor point persists across all links. Set it once on the sword (handle), and every time it links to any character's hand, the handle goes to the hand

		**View interaction (link):**
		- User selects an element and click-drags it toward a character or another element
		- When the dragged element is close enough to a valid target, the target highlights (drop zone feedback — glow, outline change, or body region highlight for characters)
		- Releasing on a highlighted target creates a link at the current playhead time on the global timeline
		- The element snaps to the target using the anchor point offset
		- Releasing without hitting a valid target does nothing (no link created, no repositioning — element movement uses gizmos/keyboard shortcuts, not click-drag)
		- Click-drag on an element in the view always means "initiate link." There is no ambiguity with repositioning because repositioning uses gizmos or keyboard shortcuts

		**View interaction (unlink):**
		- User right-clicks a linked element
		- Context menu shows "Unlink from [parent name]"
		- Clicking the menu item creates an unlink event at the current playhead time

		**Panel interaction (link tool):**
		- In the Elements panel (6.3.3), the user can drag a link tool connector from a child to a parent to create a link at the current playhead time
		- Right-click on a linked element in the panel → "Unlink" creates an unlink event at the current playhead time
		- Link/unlink boundary keyframes are visible on the timeline and can be dragged to adjust timing

		**Unlinking options:**
		1. Right-click → "Unlink from [parent]" (in view or panel) — creates unlink event at current playhead time
		2. Drag the link boundary keyframe on the timeline — adjusts when the link starts or ends
		3. Delete the boundary keyframe — removes the link entirely

		**Timeline behavior:**
		- Link and unlink events live on the **global element timeline** (not per-shot)
		- On the child's timeline track, the linked period is visually greyed out with boundary keyframes marking the link/unlink points
		- While linked (greyed-out region), the child follows the parent. The child cannot be independently keyframed or moved during this period
		- Outside the linked region, the child is independent and can be keyframed normally
		- Example: sword is independent at 0:00, linked to Character_A's hand at 0:05, unlinked at 0:10, linked to Character_B's hand at 0:15. The regions 0:05–0:10 and 0:15+ are greyed out on the sword's timeline

		**Link behavior:**
		- On link: the child snaps to the parent's attachment point (bone for characters, transform origin for elements), offset by the child's anchor point
		- For character links: snapping is available because the target is a body region with a well-defined position
		- For element links: the child maintains its relative offset to the parent at the time of linking (stored as the offset)
		- While linked, the child's world transform is overridden each frame: `parent_attachment_world_transform * stored_offset`
		- On unlink: the child stays at its current world-space position (no teleport). The child becomes independent from that point forward

		**Gizmo behavior for linked elements:**
		- A linked element shows **no transform gizmo** while it is in a linked period. The element can be selected (selection highlight appears) but no manipulator is shown
		- A tooltip appears when the user hovers or selects a linked element: "Linked to [parent name]. Unlink to transform independently."
		- The user must unlink the element before they can move it with gizmos
		- This prevents the misleading state of showing a gizmo on an element whose position is controlled by another element

		**Functional requirements:**
		- An element can be linked to at most one parent at any given time
		- An element can be a parent to any number of children simultaneously
		- Maximum chain depth: 4 levels (A→B→C→D). Attempting to create a deeper chain is rejected
		- Self-linking is rejected
		- Circular links are rejected (if B is linked to A, A cannot link to B)
		- Deleting a linked child removes the child and its link events. The parent is unaffected
		- Deleting a parent that has linked children: the children are unlinked first (become independent at their current positions), then the parent is deleted
		- Reparenting (linking to a new parent while already linked): the old link ends and the new link begins at the current playhead time. This is how a baton pass works — drag the baton from one character to the other

		**Expected behavior:**
		``` python
			# Linking a prop to a character's hand
			.if user click-drags "Sword" onto "Character_A" in the view >>
			.if "Character_A" hand region highlights >>
			.if user releases on the highlighted region >>
				<== "Sword" links to "Character_A" hand at current playhead time
				<== "Sword" snaps to hand position, offset by anchor point
				<== "Sword" timeline shows greyed-out region starting at current time
				<== Elements panel shows "Sword" linked to "Character_A"

			# Linked element follows parent
			.if "Sword" is linked to "Character_A" hand >>
				||> .if "Character_A" moves or is animated >>
					<== "Sword" follows "Character_A" hand position
					<== "Sword" maintains its anchor offset relative to the hand
					!== "Sword" stays at its original position

			# Element-to-element linking — vehicle
			.if user click-drags "Passenger" onto "Car" in the view >>
			.if "Car" highlights as a valid target >>
			.if user releases on "Car" >>
				<== "Passenger" links to "Car" at current playhead time
				<== "Passenger" maintains its current relative offset to "Car"
				<== when "Car" is animated (driving), "Passenger" follows

			# Unlinking via right-click
			.if "Sword" is linked to "Character_A" >>
				||> .if user right-clicks "Sword" in the view >>
					<== context menu shows "Unlink from Character_A"
				||> .if user clicks "Unlink from Character_A" >>
					<== "Sword" is unlinked at current playhead time
					<== "Sword" remains at its current world position
					<== "Sword" timeline greyed-out region ends at current time
					<== "Sword" can now be independently keyframed

			# Linking via link tool in the panel
			.if user drags link tool from "Cup" to "Character_B" in the Elements panel >>
				<== "Cup" links to "Character_B" at current playhead time
				<== same behavior as view linking

			# Reparenting — baton pass
			.if "Baton" is linked to "Runner_A" >>
				||> .if user click-drags "Baton" onto "Runner_B" in the view >>
					<== "Baton" unlinks from "Runner_A" at current playhead time
					<== "Baton" links to "Runner_B" at current playhead time
					<== the handoff is a single action

			# Chain linking
			.if "Prop" is linked to "Character" >>
			.if "Character" is linked to "Vehicle" >>
				||> .if "Vehicle" moves >>
					<== "Character" follows "Vehicle"
					<== "Prop" follows "Character" (which follows "Vehicle")
					<== evaluation order: Vehicle first, then Character, then Prop

			# Unlink preserves world position
			.if "Cup" is linked to "Character" >>
			.if "Character" is at position (5, 0, 3) >>
			.if "Cup" world position is (5.2, 1.1, 3) >>
				||> .if user unlinks "Cup" >>
					<== "Cup" remains at world position (5.2, 1.1, 3)
					<== "Cup" is now independent
					!== "Cup" snaps to any other position

			# Anchor point adjustment
			.if "Sword" has anchor point at (0, 0, 0) (center) >>
			.if "Sword" is linked to "Character_A" hand >>
				||> .if user changes "Sword" anchor point to (0, -0.4, 0) (handle) >>
					<== "Sword" shifts so the handle aligns with the hand
					<== the adjustment is immediate and visual

			# Gizmo suppression on linked element
			.if "Sword" is linked to "Character_A" hand >>
				||> .if user clicks "Sword" in the view >>
					<== "Sword" shows selection highlight
					!== transform gizmo appears on "Sword"
					<== tooltip: "Linked to Character_A. Unlink to transform independently."

			# Gizmo returns after unlinking
			.if "Sword" is linked to "Character_A" hand >>
				||> .if user unlinks "Sword" >>
					<== "Sword" is still selected
					<== transform gizmo now appears on "Sword"
		```

		**Timeline linking across time:**
		``` python
			# Prop linked and unlinked at different times
			.if "Sword" is independent >>
				||> .if user links "Sword" to "Character" at global time 5.0 >>
					<== link event created at t=5.0 on the global timeline
					<== sword timeline greyed out from t=5.0 onward
				||> .if user moves playhead to t=10.0 and unlinks "Sword" >>
					<== unlink event created at t=10.0
					<== sword timeline greyed out from t=5.0 to t=10.0
					<== sword is independent before t=5.0 and after t=10.0

			# Multiple link periods
			.if "Baton" is independent >>
			.if user links "Baton" to "Runner_A" at t=2.0 >>
			.if user unlinks "Baton" at t=5.0 >>
			.if user links "Baton" to "Runner_B" at t=5.0 >>
				<== t=0–2: "Baton" is independent
				<== t=2–5: "Baton" follows "Runner_A" (greyed out on timeline)
				<== t=5+: "Baton" follows "Runner_B" (greyed out on timeline)
		```

		**Error cases:**
		``` python
			# Self-linking
			.if user attempts to link "Table" to itself >>
				<== operation rejected
				<== no link created

			# Circular link
			.if "B" is linked to "A" >>
				||> .if user attempts to link "A" to "B" >>
					<== operation rejected
					<== no change

			# Deep circular link
			.if "C" is linked to "B", "B" is linked to "A" >>
				||> .if user attempts to link "A" to "C" >>
					<== operation rejected

			# Exceeding max chain depth
			.if a chain of links is already 4 levels deep >>
				||> .if user attempts to link another element to the deepest element >>
					<== operation rejected
					<== visual feedback indicating the depth limit

			# Deleting a linked child
			.if "Sword" is linked to "Character" >>
				||> .if user deletes "Sword" >>
					<== "Sword" is deleted along with its link events
					<== "Character" is unaffected

			# Deleting a parent with linked children
			.if "Plate" and "Glass" are linked to "Table" >>
				||> .if user deletes "Table" >>
					<== "Plate" and "Glass" are unlinked (become independent at current positions)
					<== "Table" is deleted
					<== "Plate" and "Glass" remain in the scene
					!== "Plate" and "Glass" are deleted with "Table"
		```

	---

	- ##### 6.3.2. Element Grouping (Feature)
		***Select multiple elements and group them. Groups have their own transform. Single-click selects the group; double-click enters it to edit individual members. Non-group elements dim to 30% opacity when inside a group. No temporal behavior — grouping is a convenience for simultaneous manipulation.***

		*Related:
		- 6.3.1 (linking — temporal following, separate from grouping)
		- 2.1.1 (scene elements — grouped elements are still independent scene elements)
		- 8.1.1 (multi-select — ad hoc multi-select is separate from persistent groups)*

		**Group transform ownership:**
		- Groups have their own transform (position, rotation, scale)
		- Member elements have transforms relative to the group
		- Moving the group moves all members. Rotating the group rotates all members around the group's center. Scaling the group scales all members relative to the group's center
		- The group's gizmo appears at the **bounding box center** of all members

		**Functional requirements:**
		- User selects multiple elements (multiselect), then right-click → "Group" (or keyboard shortcut)
		- Grouped elements form a named group
		- An element can belong to at most one group
		- Groups do not nest (a group cannot contain another group)
		- Groups have no timeline behavior — they do not appear on the timeline, they do not keyframe
		- Deleting a member removes it from the group. The remaining members stay grouped
		- Deleting the last member dissolves the group
		- Groups persist across the project (not per-shot)

		**Enter/exit pattern:**
		- **Single-click** any group member in the view → selects the entire group. Gizmo appears at the group's bounding box center. Transforms affect the whole group
		- **Double-click** a group member → "enters" the group. Individual members become selectable and transformable. The gizmo appears on the clicked member
		- **Click outside** the group or press **Escape** → exits the group. Returns to group-level selection
		- **Cmd/Ctrl+Click** on a member → deep select: directly select an individual member without entering the group (power user shortcut)

		**Visual feedback when inside a group:**
		- Non-group elements in the scene reduce to **30% opacity** — the group's contents are visually isolated
		- A label in the view header shows "Editing: [Group Name]"
		- Selected members within the group get the normal selection highlight
		- Exiting the group restores all elements to full opacity

		**Grouping vs linking:**
		- **Linking** says "follow this thing wherever it goes" — temporal, appears on the timeline, one element dynamically follows another
		- **Grouping** says "perform this operation on all of us simultaneously" — no dynamic following, just coordinated transforms

		**Expected behavior:**
		``` python
			# Creating a group
			.if user selects "Plate", "Glass", and "Fork" >>
				||> .if user right-clicks and selects "Group" >>
					<== a group is created containing "Plate", "Glass", "Fork"
					<== the group is given a default name (e.g., "Group 1")
					<== gizmo appears at bounding box center of the three elements

			# Selecting a group (single-click)
			.if "Plate", "Glass", "Fork" are in a group >>
				||> .if user clicks "Plate" in the view >>
					<== the entire group is selected
					<== all three show selection highlights
					<== gizmo appears at bounding box center of the group
					!== only "Plate" is selected

			# Transforming a group
			.if "Plate", "Glass", "Fork" are in a group >>
				||> .if user translates the group by (2, 0, 0) >>
					<== "Plate", "Glass", and "Fork" all move by (2, 0, 0)
					<== their relative positions are preserved

			# Entering a group (double-click)
			.if "Plate", "Glass", "Fork" are in a group >>
				||> .if user double-clicks "Plate" in the view >>
					<== group is "entered" — individual members now selectable
					<== "Plate" is selected individually
					<== gizmo appears on "Plate"
					<== non-group elements reduce to 30% opacity
					<== view header shows "Editing: [Group Name]"

			# Transforming inside a group
			.if user is inside a group and "Plate" is selected >>
				||> .if user translates "Plate" by (1, 0, 0) >>
					<== only "Plate" moves
					!== "Glass" or "Fork" move

			# Exiting a group
			.if user is inside a group >>
				||> .if user clicks outside the group or presses Escape >>
					<== group editing mode exits
					<== all elements return to full opacity
					<== clicking a group member selects the whole group again

			# Deep select (Cmd+Click)
			.if "Plate", "Glass", "Fork" are in a group >>
				||> .if user Cmd+Clicks "Glass" >>
					<== "Glass" is selected individually without entering the group
					<== gizmo appears on "Glass"
					!== group editing mode activated
					!== other group members selected

			# Ungrouping
			.if "Plate", "Glass", "Fork" are in a group >>
				||> .if user right-clicks and selects "Ungroup" >>
					<== all three become independent elements
					<== selecting "Plate" no longer selects the others

			# Deleting a group member
			.if "Plate", "Glass", "Fork" are in a group >>
				||> .if user deletes "Fork" >>
					<== "Fork" is deleted
					<== "Plate" and "Glass" remain grouped
					<== selecting "Plate" still selects "Glass"

			# Linked element in a group
			.if "Cup" is linked to "Character" hand >>
			.if "Cup" is also in a group with "Saucer" >>
				<== "Cup" follows "Character" hand (linking takes precedence for position)
				<== selecting "Cup" still selects "Saucer" (grouping still affects selection)
				<== no gizmo appears on "Cup" while linked (6.3.1 linking rules override)
		```

		**Error cases:**
		``` python
			# Grouping a single element
			.if user selects one element and attempts to group >>
				<== operation rejected — groups require at least 2 elements

			# Grouping an element already in a group
			.if "Plate" is in Group_1 >>
				||> .if user selects "Plate" and "Lamp" and attempts to group >>
					<== "Plate" is removed from Group_1 and added to the new group with "Lamp"
					<== Group_1 continues with remaining members (or dissolves if "Plate" was the last)
		```

	---

	- ##### 6.3.3. Elements Panel (Feature)
		***A list of every scene element showing link relationships and group membership. Click to select, link tool to link. The director's view of "what goes with what."***

		*Blocked by:
		- 6.3.1 (linking — the panel visualizes link relationships)
		- 6.3.2 (grouping — the panel shows group membership)*

		*Related:
		- 2.1.1 (scene elements — panel lists all scene elements)
		- 2.1.2 (transform gizmos — selecting an element in the panel activates its gizmo)*

		**Functional requirements:**
		- The Elements panel is a collapsible sidebar panel, toggled via right gutter tab or keyboard shortcut (`O`)
		- Dockable to left or right side of the view (user-configurable). Default: right side, 220px width, resizable via drag edge (min 150px, max 500px)
		- Mutually exclusive with Assets panel — opening Elements closes Assets and vice versa (they share the same dock space). See 2.2.1 for gutter tab behavior.
		- Panel header: "ELEMENTS" in uppercase
		- The panel displays every scene element as a row in a flat list (scene graph is flat — no nesting)
		- Separate "Lights" section — lights are displayed below a section header divider labeled "LIGHTS", not mixed with other elements
		- **Row layout** (see `ui-layout-spec.md` §6.1 for visual details):
			- **Icon** (16px): Varies by element type — diamond for camera/element, circle for character, filled square for environment, star for light
			- **Name**: Element name, truncated with ellipsis if needed
			- **Link indicator** (conditional): When the element is linked at the current playhead time, shows "→ Parent" in a subtle accent color (steel blue). Updates as playhead moves.
			- **Type tag** (trailing): Uppercase 8px label showing the element type (cam, character, prop, environment, light, mesh, rig)
		- Child elements (sub-components like Body, Face_Rig) are indented with dimmer icons
		- The currently selected element is highlighted in the panel
		- Selecting an element in the view highlights it in the panel, and vice versa (always in sync)
		- **Link indicators**: linked elements show their parent name next to their row (e.g., "Sword → Character_A hand"). The indicator reflects the link state at the current playhead time
		- **Group indicators**: grouped elements share a visual marker (color bar, bracket, or icon) identifying their group
		- **Link tool**: each row has a link tool handle. Dragging from one row's link tool handle to another row creates a link at the current playhead time

		**Interaction:**
		- Clicking a row selects that element (and all group members if grouped)
		- Clicking empty space deselects
		- Link tool drag from child to parent → link at current playhead time
		- Right-click on a linked element → "Unlink from [parent]" → unlink at current playhead time
		- Right-click on selected elements → "Group" / "Ungroup"
		- The panel updates immediately when elements are added, deleted, linked, unlinked, grouped, or ungrouped

		**Expected behavior:**
		``` python
			# Viewing elements
			.if scene contains "Table", "Plate", "Character_A", "Sword" >>
				<== panel shows all four as rows in a flat list
				<== no nesting (scene graph is flat)

			# Viewing link state
			.if "Sword" is linked to "Character_A" hand at current playhead time >>
				<== "Sword" row shows "→ Character_A (hand)"
			.if playhead moves to a time before the link >>
				<== "Sword" row shows no link indicator (independent at this time)

			# Viewing group membership
			.if "Plate", "Glass", "Fork" are in "Group 1" >>
				<== all three rows share a visual group indicator
				<== the group name is visible

			# Separate lights section
			.if scene contains "Table", "Chair", "Key Light", "Fill Light" >>
				<== "Table" and "Chair" appear in the elements section
				<== "Key Light" and "Fill Light" appear in a separate "Lights" section
				!== lights mixed in with elements

			# Docking
			.if user configures panel to dock left >>
				<== panel appears on the left side of the view
				<== view adjusts to fill remaining space

			# Selecting via panel
			.if user clicks "Plate" row in the panel >>
				<== "Plate" is selected in the view
				<== gizmo appears on "Plate"
				<== if "Plate" is in a group, all group members are also selected

			# Selection sync from view
			.if user clicks "Glass" in the 3D view >>
				<== "Glass" row is highlighted in the panel
				<== panel scrolls to reveal "Glass" if necessary

			# Linking via link tool
			.if user drags link tool from "Sword" row to "Character_A" row >>
				<== "Sword" links to "Character_A" at current playhead time
				<== "Sword" row shows link indicator "→ Character_A"

			# Unlinking via panel
			.if "Sword" is linked to "Character_A" >>
				||> .if user right-clicks "Sword" row >>
					<== context menu shows "Unlink from Character_A"
				||> .if user clicks "Unlink" >>
					<== link ends at current playhead time

			# Element added to scene
			.if user imports or duplicates an element >>
				<== new element appears in the panel immediately

			# Element deleted from scene
			.if user deletes "Sword" >>
				<== "Sword" row removed from panel
				<== any link indicators referencing "Sword" are removed
		```

		**Error cases:**
		``` python
			# Empty scene
			.if no scene elements exist >>
				<== panel shows empty state (no rows)
				!== panel crashes or shows stale data

			# Invalid link tool drop
			.if user drags link tool from "A" and drops on "A" (self-link) >>
				<== operation rejected
				<== no link created
		```
