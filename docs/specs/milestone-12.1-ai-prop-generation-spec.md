# Milestone 12.1: AI Prop Generation

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 12.1 — AI prop generation
**Phase**: 12 — AI Generation
**Competitor reference**: Previs Pro PropGen (v2.6.1)

---

- ### 12.1. AI prop generation (Milestone)
	*Generate 3D props from text descriptions. Fills gaps in the asset library when a specific prop isn't available.*

	- ##### 12.1.1. Text-to-prop (Feature)
		*Text prompt generates a 3D model suitable for previs. Low-poly, consistent visual style. Auto-generates collider. Saved to asset library for reuse.*

		**Workflow:**
		1. User opens Assets panel or presses shortcut
		2. Types a description: "wooden baseball bat," "vintage rotary phone," "hospital gurney"
		3. System calls a 3D generation API (Meshy, Tripo, or best available at implementation time)
		4. Generated model appears in the scene at camera look-point
		5. Auto-generates collider for selection/gizmo interaction
		6. Saved to user's asset library for reuse

		**Constraints:**
		- Requires internet connection
		- Generation may take 5–30 seconds depending on API
		- Visual style should be consistent with bundled assets (previs-quality, not photorealistic)
		- Generated props are single-object (no multi-mesh complexity)
		- Generated props are treated as imported (not vertex-editable)
		- BYOK (bring your own API key) is always an option regardless of pricing model

		**API/service choice:** Implementation decision at build time — depends on quality, cost, and speed of available services. Research required.
