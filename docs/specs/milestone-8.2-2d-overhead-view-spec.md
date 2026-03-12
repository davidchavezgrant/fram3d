# Milestone 8.2: 2D Designer

**Date**: 2026-03-11
**Status**: Resolved
**Milestone**: 8.2 — 2D Designer
**Phase**: 8 — Polish & Views
**Competitor reference**: Shot Designer, FrameForge, Previs Pro

---

- ### 8.2. 2D Designer (Milestone)
	*Bird's-eye 2D view of the scene showing element positions, camera frustums, and light positions. Not a separate tool — a view panel mode on the same scene data (see 2.2 View Panel System).*

	- ##### 8.2.1. 2D Designer view (Feature)
		*Top-down orthographic view. Elements as labeled icons/silhouettes. Camera as frustum with FOV cone. Fully interactive — not just a display.*

		**Visual design:**
		- **Elements:** Labeled circles or silhouettes from above. Characters get head-shaped icons. Props get simple shape outlines. Size proportional to real size.
		- **Cameras:** Triangular frustums with field-of-view cones. Active camera highlighted. In multi-camera setups, all cameras visible with color codes (A/B/C/D).
		- **Lights:** Standard lighting diagram symbols (circle with rays for point, arrow for directional, cone for spot).
		- **Labels:** Element names next to icons. Camera names (Shot 1 Cam A, etc.) displayed.
		- **Movement paths:** Animated elements/cameras show path as dotted line with direction arrow. Keyframe positions shown as dots along the path.
		- **Scale reference:** Grid visible with distance markings.
		- **Camera preview:** Small camera preview in corner of the 2D view.

		**Interaction:**
		- Click elements to select (same selection model as 3D view)
		- Drag elements to reposition (updates position in 3D — same data)
		- Drag cameras to reposition
		- Creating keyframes respects the recording state (per-track stopwatch from 3.2.3)
		- Scroll to zoom in/out
		- Right-click for context menus (same as 3D view)
		- Timeline and playhead work normally — scrubbing shows animated positions

		**Access:**
		- Toggle between 3D and 2D view via keyboard shortcut or toolbar button
		- Purely orthographic (no perspective, no isometric tilt)
		- 2D view is NOT a separate mode — it's a different camera angle on the same scene

		**What this is NOT:**
		- Not a separate diagramming tool (Shot Designer is that)
		- Not a floor plan editor (rooms come from premade environments, set builder, or imported models)
		- Not exportable as standalone diagram (v1 — could add PDF export later)
