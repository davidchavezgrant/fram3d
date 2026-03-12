# Milestone 6.2: Camera Follow and Look-At

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 6.2 — Camera follow and look-at
**Phase**: 6 — Characters
**Blocked by**: 6.1 (Characters — follow/look-at requires animated targets to be meaningful)

---

- ### 6.2. Camera follow and look-at (Milestone)
	*Camera maintains spatial relationship with a target. Timeline relationship model — not a mode to enter/exit. Implemented after characters (6.1), before snorricam.*

	- ##### 6.2.1. Camera follow (Feature)
		*Persistent relationship (like prop locking). Creates non-keyframeable segment on timeline. Follow distance, height offset, lateral offset are separately keyframeable.*

		**Setup:**
		- Select a character or object
		- Right-click → "Follow" (or keybind, e.g. Shift+F)
		- A non-keyframeable segment appears on the camera's timeline (like greyed-out linking segments for props)
		- Segment extends to end of shot by default, adjustable by user

		**Follow parameters:**

		| Parameter | Description | Default | Keyframeable? |
		|-----------|-------------|---------|---------------|
		| Follow distance | Distance from camera to target | Current distance at activation | Yes |
		| Height offset | Camera height relative to target | Current height at activation | Yes |
		| Lateral offset | Left/right offset from target's facing direction | 0 (centered) | Yes |
		| Response | How tightly the camera tracks the target. Slider labeled "Rigid ←→ Loose". Internal damping value 0–1. | 0.3 | Yes |
		| Lead/trail | Camera ahead of or behind target's movement direction | Trail | No (enum) |

		**Response (damping) reference:**
		- **0 (Rigid):** Instant response. Robotic dolly on rails.
		- **0.1–0.3 (Steadicam):** Slight delay, smooth catch-up. Professional, barely noticeable lag.
		- **0.5–0.7 (Handheld):** Noticeable lag, more reactive. Documentary feel.
		- **0.8–1.0 (Loose):** Very delayed. Dreamy, disoriented feel.

		**Target switching:**
		- User can switch follow targets mid-shot
		- Smooth blend (~0.5s default, configurable) so the switch isn't noticeable
		- Timeline shows: [Follow Target A] → [blend] → [Follow Target B]

		**Multi-target follow:**
		- Group multiple characters/objects, then activate follow on the group
		- Camera follows the midpoint of the group
		- Reuses existing group system — no special multi-target feature

		**Manual offset adjustment:**
		- During a follow segment, user can adjust camera rotation in the viewport
		- This adjusts the lateral/vertical offset parameters
		- Offsets are keyframeable — allows framing the subject off-center

		**Follow path:**
		- Camera's computed follow path visualized as a 3D spline (extends path visualization from 3.2.6)

		**Interaction with existing systems:**
		- Camera movement (1.1.1): Manual controls disabled during follow (position derived from target). Exception: if rotation is manually adjustable per offset feature.
		- Per-track stopwatch (3.2.3): Follow parameter changes (distance, height) create keyframes when stopwatch is on.
		- Focus (1.1.4): Optional auto-focus on target (DOF focus distance tracks followed subject). Toggle: "Auto-focus on target" (default on). Focus distance keyframes generated when active.
		- Camera shake (1.1.6): Stacks with follow. Follow provides position, shake adds rotational noise.
		- Snorricam (7.2): Mutually exclusive. Snorricam is rigidly attached; follow maintains dynamic spatial relationship.
		- Multi-camera (9.1): A follow cam can be one camera in a multi-cam setup.
		- Shot sequencer (3.1): Follow is per-shot.
		- Export (4.4): Follow motion baked to keyframes during export (like camera shake).

	- ##### 6.2.2. Look-at tracking (Feature)
		*Camera stays in position, rotates to track target. Like a camera operator panning/tilting on a tripod or crane to follow a moving subject.*

		**Setup:**
		- Right-click object or character → "Look At"
		- Camera position is locked (tripod, crane arm at fixed extension)
		- Camera rotation automatically adjusts to keep target centered
		- Creates a timeline segment (not a mode to enter/exit)

		**Separate from follow:**
		- Follow = camera moves to maintain spatial relationship
		- Look-at = camera stays put, rotates to track
		- Both available independently

		**Deferred (flagged for later):**
		- Follow + look-at on different targets simultaneously (e.g., camera follows Character A's position but looks at Character B — POV-style shot). Not v1.

	**What this is NOT:**
	- Not a mode to enter and exit (it's a timeline relationship)
	- Not a separate animation system (follow segments are visible on the timeline alongside regular keyframes)
	- Not Snorricam (7.2) (which is rigidly attached to the character's body)
