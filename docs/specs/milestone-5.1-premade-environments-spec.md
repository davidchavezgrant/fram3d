# Milestone 5.1: Premade Environments

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 5.1 — Premade environments (HIGHER PRIORITY)
**Project**: 5 — Feature parity and extensions

---

- ### 5.1. Premade environments (Milestone)
	*Ship with a library of ready-to-use environments. Interior and exterior sets that establish context in one click.*

	- ##### 5.1.1. Environment library (Feature)
		*Premade sets covering common filming locations. Each includes walls, floors, props, lighting, and one character. Closed walls and ceiling. Real-world scale.*

		**Starter set (12–15 environments):**

		| Category | Environments |
		|----------|-------------|
		| Interior — Residential | Living room, bedroom, kitchen |
		| Interior — Commercial | Office, restaurant/bar, hospital room |
		| Interior — Institutional | Courtroom, classroom, interrogation room |
		| Exterior — Urban | City street, alley, parking lot |
		| Exterior — Nature | Park/forest clearing, rooftop |
		| Special | Warehouse/empty stage (flexible use) |

		**Construction:** Closed walls and ceiling for interiors. Real-world scale (a living room is ~5x4m). For residential environments, consider small + larger variants using the same furniture placement rules with different room dimensions.

		**Behavior:**
		- Environments are NOT locked prefabs — after placement, every object is independent and fully editable
		- Placing an environment adds its objects alongside existing scene objects (doesn't clear the scene)
		- If scene already has objects, prompt: "Add environment to current scene?" or "Replace current scene?"
		- Consistent visual style matching the "clearly previs" aesthetic (stylized/abstract, not realistic)
		- Pre-placed lighting appropriate to setting (window light for interior, sun for exterior)
		- Each environment includes one pre-placed character
		- User can save custom environments as reusable templates (custom sets)

		**Asset sourcing:** Mix of programmatically generated (procedural room assembly from modular pieces — wall panels, floor tiles, furniture prefabs) and marketplace/hand-built assets. Procedural system can also power the set builder (5.4).

		**Objects are independent after placement — fully editable.** User can move, delete, or add to anything. The environment is a starting point, not a constraint.
