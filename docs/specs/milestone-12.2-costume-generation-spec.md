# Milestone 12.2: Costume Generation

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 12.2 — Costume generation
**Phase**: 12 — AI Generation
**Blocked by**: 6.1 (Characters — costumes apply to mannequin mesh)
**Related**: 12.1 (AI prop generation — similar AI pipeline)

---

- ### 12.2. Costume generation (Milestone)
	*Generate 3D clothing meshes on existing mannequin. Clothing-on-mannequin approach — skeleton stays fixed, auto-binding to known skeleton is tractable. Supplements non-AI mannequin customization (6.1.1).*

	- ##### 12.2.1. AI costume generation (Feature)
		*Text description → 3D clothing meshes auto-bound to mannequin skeleton. Costume library for cross-project reuse.*

		**Approach: 3D clothing mesh generation (not texture-only)**

		Generating 3D clothing meshes that layer over the mannequin, rather than flat textures or full AI characters. Clothing has volume and silhouette — a trench coat billows, armor has thickness.

		**Why not generate full characters?**
		1. **AI 3D models don't come rigged.** Generation outputs geometry only. Auto-rigging works for standard proportions but breaks on stylized characters.
		2. **Even when the rig maps, skin weights are often wrong.** Elbows deform badly, shoulders clip, fingers bend incorrectly. Every generated character needs QA.
		3. **Failure modes differ.** Bad rig on full character = broken posing, unusable. Bad weights on clothing = jacket wrinkles weirdly, acceptable for previs.

		Clothing-on-mannequin keeps all systems working (posing, body regions, walk cycles, expressions, snorricam, follow cam) because the skeleton never changes.

		**Pipeline:**
		1. User selects a character, opens "Appearance" panel in inspector
		2. Types a costume description: "detective trench coat," "scrubs," "medieval armor"
		3. AI generates clothing mesh geometry (jacket mesh, pants mesh, hat mesh)
		4. Meshes auto-bound to mannequin's known skeleton (automatic skinning weights — tractable because we control the target skeleton)
		5. Clothing deforms with character during animation

		**Auto-binding approach:**
		- Known skeleton = known bone positions → use automatic skinning weights or pre-computed weight templates for common clothing regions:
			- Torso clothing → Spine/Chest bones
			- Pants → Hips/UpperLeg bones
			- Hat → Head bone
		- For accessories (hats, glasses, weapons), use existing prop attachment system (link to bone)

		**Clipping:** Clothing meshes will clip through body mesh during extreme poses. For previs, minor clipping is acceptable. Options for severe cases: slightly inflate body mesh inward where clothing covers it, or accept it.

		**Body type changes:** Costume stretches when body type changes (no regeneration required).

		**Costume library (hard requirement):**
		- Generated costumes saveable for reuse
		- Cross-project, cross-character
		- Saved locally — no cost for reuse

		**Cost considerations:**
		- 3D clothing generation is more expensive than 2D texture gen
		- BYOK option required
		- Generated meshes saved locally for free reuse

		**Relationship to non-AI customization (6.1.1):**
		- 6.1.1 provides extensive non-AI mannequin customization (clothing presets, hair, accessories, skin tone — see Characters spec)
		- Non-AI covers ~80% of use cases
		- AI costume generation fills the gap for specific/unusual outfits ("1920s flapper dress," "astronaut EVA suit") not worth building as presets

	**What this is NOT:**
	- Not full AI character generation (skeleton stays fixed, only appearance changes)
	- Not texture-only (3D clothing with volume, not painted surfaces)
	- Not reference-image-based (text input only)
