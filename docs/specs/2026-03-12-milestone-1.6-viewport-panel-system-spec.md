# Milestone 1.6: Viewport Panel System — Specification

**Date**: 2026-03-12
**Milestone**: 1.6
**Status**: Draft
**Project**: 1 — Core previsualization tool

---

- ### 1.6. Viewport panel system (Milestone)

  Multi-panel viewport with configurable layouts. The viewport area can display one, two, or three panels simultaneously, each showing an independent view of the scene. This replaces the single-viewport model and enables workflows like 3D camera view alongside a Director Mode overview, or a 2D Designer next to the camera view.

  The design follows the mockup: a layout chooser in the bottom-right of the viewport area, and a per-panel dropdown selector for the view mode. The panel-based approach was chosen over a single-viewport toggle model (like Blender's Numpad 0) because it avoids the #1 complaint across every 3D tool — accidentally moving the shot camera while navigating. With separate panels, each view is independent and cannot accidentally affect another.

  *Blocked by: 1.3 (scene management — Director Mode requires scene elements and gizmos), 1.5 (keyframe animation — timeline context needed)*

  ---

  - ##### 1.6.1. Panel layouts (Feature)
    ***Three layout options: single panel, side-by-side, and one-large-two-small. Each panel independently assignable to any view mode.***

    **Functional requirements:**
    - Three layout configurations, selected via layout chooser buttons in the bottom-right corner of the viewport area:
      - **Single** (default): One panel fills the viewport area
      - **Side-by-side**: Two panels of equal width, arranged horizontally
      - **Three-panel**: One large panel on top, two smaller panels below (or one large left, two stacked right — follow mockup)
    - Each panel has a dropdown selector in its top bar for choosing the view mode
    - Layout choice persists across session (saved with project)
    - Panel sizes are fixed ratios per layout — no user-resizable splits in v1
    - Switching layouts preserves each panel's view mode where possible (e.g., switching from 2-panel to 1-panel keeps the first panel's view mode)

    **Expected behavior:**
    ``` python
      # default state
      .if the application starts >>
          <== single-panel layout active
          <== panel 0 shows 3D Viewport
          <== layout chooser shows single-panel button highlighted

      # switching to side-by-side
      .if user clicks the side-by-side layout button >>
          <== two panels appear, each with its own view selector
          <== panel 0 retains its previous view mode
          <== panel 1 defaults to Director Mode

      # switching to three-panel
      .if user clicks the three-panel layout button >>
          <== three panels appear
          <== panel 0 (large) retains its previous view mode
          <== panel 1 defaults to Director Mode
          <== panel 2 defaults to 2D Designer

      # switching back to single
      .if user clicks the single-panel layout button >>
          <== only panel 0 remains visible
          <== panel 0 retains its view mode
    ```

  ---

  - ##### 1.6.2. View modes (Feature)
    ***Three view modes assignable to any panel: 3D Viewport, 2D Designer, Director Mode.***

    **Functional requirements:**
    - **3D Viewport**: Looks through the virtual camera rig (shot camera). Shows the scene as framed by the current shot's camera position. This is the primary working view — aspect ratio masks, frame guides, HUD, and subtitles render here. Camera movements in this view create keyframes when the stopwatch is on.
    - **2D Designer**: Top-down orthographic view (see 5.3). Objects shown as icons/silhouettes, cameras as frustums with FOV cones, lights as standard symbols. Fully interactive — drag objects to reposition. Available as a panel mode even before the full 5.3 spec is implemented (can start as a simple orthographic camera).
    - **Director Mode**: Free utility camera decoupled from the shot timeline (see 1.3.5). Navigating this view never creates camera keyframes. The shot camera is visible as a frustum wireframe. Object manipulation still creates keyframes when stopwatches are on.
    - Any view mode can be assigned to any panel via the dropdown selector in the panel's top bar
    - Multiple panels can show the same view mode (e.g., two 3D Viewports at different zoom levels — though this is unusual)
    - View mode selection is per-panel and persists until changed
    - All panels share the same scene data and timeline state — changes in one panel are immediately reflected in all others

    **Expected behavior:**
    ``` python
      # changing a panel's view mode
      .if panel 1 is showing Director Mode >>
        ||> .if user selects "2D Designer" from panel 1's dropdown >>
            <== panel 1 switches to the 2D Designer view
            <== other panels are unaffected

      # panels share scene state
      .if panel 0 shows 3D Viewport and panel 1 shows Director Mode >>
        ||> .if user moves an object via gizmo in panel 1 (Director Mode) >>
            <== the object moves in both panels simultaneously
            <== panel 0 (3D Viewport) reflects the new position through the shot camera

      # timeline interaction across panels
      .if panel 0 shows 3D Viewport and panel 1 shows Director Mode >>
        ||> .if user scrubs the timeline >>
            <== both panels update to show the scene state at the new time
            <== the shot camera frustum in Director Mode animates to its keyframed position
    ```
