# Milestone 3.5: Snorricam

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 3.5 — Snorricam
**Project**: 3 — Production features
**Blocked by**: 3.2 (Characters), 3.4 (Camera follow — snorricam builds on follow concepts)

---

- ### 3.5. Snorricam (Milestone)
	*Body-mounted camera rig. Camera locks to character's root motion. Front and back mount options.*

	- ##### 3.5.1. Snorricam (Feature)
		*Front mount (camera faces actor's face) and back mount (camera faces away). Camera locks to character's root motion.*

		**Front mount behavior:**
		- Camera attaches to a point offset from the character's chest (configurable distance, default ~0.5m)
		- Camera looks at the character's face (head position)
		- As the character moves, the camera moves with them
		- Character stays perfectly centered and at constant size in frame
		- Background moves relative to the character's motion
		- Only root motion transfers to camera (translation + Y-axis rotation). No full skeletal motion — avoids nausea.

		**Back mount behavior:**
		- Camera attaches behind the character's head (configurable distance, default ~0.3m)
		- Camera looks forward along the character's facing direction
		- Shows the character's POV with their body motion baked in
		- Rigidly attached, no smoothing

		**Mount offset controls:**

		| Property | Description | Keyframeable? |
		|----------|-------------|---------------|
		| Height offset | How high on the body the rig sits (chest, shoulder, head level) | No |
		| Distance offset | How far from the body the camera extends | No |

		Adjustable via inspector panel while Snorricam is active. Default offsets produce a reasonable result without adjustment.

		**Activation:**
		- Right-click character → "Snorricam" → "Front Mount" / "Back Mount"
		- Or keybind (toggle)

		**Exiting:**
		- Right-click → "Dismount Snorricam" or press same keybind again (toggle)
		- Camera stays at its current world position and rotation — does NOT snap back
		- Normal camera controls resume

		**Interaction with existing systems:**
		- Per-track stopwatch (1.5.3): Camera position/rotation keyframes not created during Snorricam — position is derived from character transform + offset.
		- Character animation (3.2.4): Snorricam depends on character animation. Without walk cycles or pose keyframes, Snorricam produces a static shot.
		- Shot sequencer (1.4): Snorricam is per-shot. Shot A can have Snorricam, Shot B can have a normal camera.
		- Camera movement (1.1.1): All manual camera movements disabled while Snorricam is active.
		- Lens system (1.1.2): Focal length is still user-adjustable during Snorricam.
		- Camera shake (1.1.6): Disabled during Snorricam. Snorricam already produces motion from character movement — stacking shake would be too much.
		- Export (2.4): Snorricam camera motion baked to keyframes during export (like camera shake).
		- Multi-camera (3.9): A Snorricam can be one camera in a multi-cam setup. Camera B is Snorricam on Character A while Camera A is a static wide shot.
		- Camera follow (3.4): Mutually exclusive. Snorricam is rigid attachment; follow maintains dynamic spatial relationship.

	**What this is NOT:**
	- Not a mode with configurable damping (that's camera follow)
	- Not keyframeable for offset distance during the shot
	- Not affected by camera shake (shake is disabled)
	- Not dependent on breathing simulation or full skeletal motion
