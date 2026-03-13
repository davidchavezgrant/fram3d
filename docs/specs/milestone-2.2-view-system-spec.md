# Milestone 2.2: View System — Specification

**Date**: 2026-03-12
**Milestone**: 2.2
**Status**: Draft
**Project**: Phase 2 — The Scene

---

- ### 2.2. View system (Milestone)

  Configurable view layouts. The workspace can display one, two, or three views simultaneously, each showing an independent view of the scene. This replaces the single-view model and enables workflows like Camera View alongside a Director View, or a Designer View next to the Camera View.

  The design follows the mockup: a layout chooser in the bottom-right of the workspace, and a per-view dropdown selector for the view type. The multi-view approach was chosen over a single-view toggle model (like Blender's Numpad 0) because it avoids the #1 complaint across every 3D tool — accidentally moving the shot camera while navigating. With separate views, each view is independent and cannot accidentally affect another.

  *Blocked by: 2.1 (scene management — Director View requires elements and gizmos), 3.2 (keyframe animation — timeline context needed)*

  ---

  - ##### 2.2.1. Panel layouts (Feature)
    ***Three layout options: single view, side-by-side, and one-large-two-small. Each view independently assignable to any view type.***

    **Functional requirements:**
    - Three layout configurations, selected via layout chooser buttons in the bottom-right corner of the workspace:
      - **Single** (default): One view fills the workspace
      - **Side-by-side**: Two views of equal width, arranged horizontally
      - **Three-view**: One large view on top, two smaller views below (or one large left, two stacked right — follow mockup)
    - Each view has a dropdown selector in its top bar for choosing the view type
    - Layout choice persists across session (saved with project)
    - Side panels (Elements, Assets) have horizontal drag edges for resizing. Min 150px, max 500px.
    - Switching layouts preserves each view's type where possible (e.g., switching from 2-view to 1-view keeps the first view's type)

    **Gutter-based panel toggles:**
    - JetBrains-style vertical label strips on left and right workspace edges
    - Left gutter: Overview toggle
    - Right gutter: Elements panel, Assets panel toggles
    - Clicking a gutter tab toggles the associated panel open/closed
    - **Mutual exclusion**: opening Elements closes Assets and vice versa (they occupy the same space)
    - Keyboard shortcuts: O = Elements panel, T = timeline, Tab = toggle all panels simultaneously

    **Expected behavior:**
    ``` python
      # default state
      .if the application starts >>
          <== single-view layout active
          <== view 0 shows Camera View
          <== layout chooser shows single-view button highlighted

      # switching to side-by-side
      .if user clicks the side-by-side layout button >>
          <== two views appear, each with its own view type selector
          <== view 0 retains its previous view type
          <== view 1 defaults to Director View

      # switching to three-view
      .if user clicks the three-view layout button >>
          <== three views appear
          <== view 0 (large) retains its previous view type
          <== view 1 defaults to Director View
          <== view 2 defaults to 2D Designer

      # switching back to single
      .if user clicks the single-view layout button >>
          <== only view 0 remains visible
          <== view 0 retains its view type
    ```

  ---

  - ##### 2.2.2. View types (Feature)
    ***Three view types assignable to any view: Camera View, 2D Designer, Director View.***

    **Functional requirements:**
    - **Camera View**: Looks through the virtual camera rig (shot camera). Shows the scene as framed by the current shot's camera position. This is the primary working view. Camera movements in this view create keyframes when the stopwatch is on.
      - The Camera View frame maintains a 16:9 aspect ratio inside its container, centered with black letterbox surround.
      - **Camera View overlays** (see `ui-layout-spec.md` §2.3 for z-ordering and positions): aspect ratio masks, composition guides (1.2), shot label (top-left), sequence timecode (bottom-center), camera info (top-right, 1.2.3), active tool badge (bottom-left, 2.1.2), Director View badge (top-center, 2.1.5), camera path badge (bottom-right, 3.2.6), subtitles (bottom-center, 1.2.4).
    - **2D Designer**: Top-down orthographic view (see 8.2). Elements shown as icons/silhouettes, cameras as frustums with FOV cones, lights as standard symbols. Fully interactive — drag elements to reposition. Available as a view type even before the full 8.2 spec is implemented (can start as a simple orthographic camera).
    - **Director View**: Free utility camera decoupled from the shot timeline (see 2.1.5). Navigating this view never creates camera keyframes. The shot camera is visible as a frustum wireframe. Element manipulation still creates keyframes when stopwatches are on.
    - Any view type can be assigned to any view via the dropdown selector in the view's top bar
    - **Camera View is a single movable instance** — only one view can show Camera View at a time
    - When a view is reassigned to Camera View, the view that previously held Camera View receives the reassigning view's old view type (**smart swap**)
    - Camera View must always exist in exactly one view — it cannot be removed
    - View type selection is per-view and persists until changed
    - All views share the same scene data and timeline state — changes in one view are immediately reflected in all others

    **Expected behavior:**
    ``` python
      # changing a view's type
      .if view 1 is showing Director View >>
        ||> .if user selects "2D Designer" from view 1's dropdown >>
            <== view 1 switches to the 2D Designer
            <== other views are unaffected

      # views share scene state
      .if view 0 shows Camera View and view 1 shows Director View >>
        ||> .if user moves an element via gizmo in view 1 (Director View) >>
            <== the element moves in both views simultaneously
            <== view 0 (Camera View) reflects the new position through the shot camera

      # timeline interaction across views
      .if view 0 shows Camera View and view 1 shows Director View >>
        ||> .if user scrubs the timeline >>
            <== both views update to show the scene state at the new time
            <== the shot camera frustum in Director View animates to its keyframed position

      # Camera View smart swap
      .if view 0 shows Camera View and view 1 shows Director View >>
        ||> .if user selects "Camera View" from view 1's dropdown >>
            <== view 1 now shows Camera View
            <== view 0 receives Director View (the view type that view 1 had)
            !== Camera View exists in two views simultaneously

      # gutter panel mutual exclusion
      .if Elements panel is open >>
        ||> .if user clicks Assets gutter tab >>
            <== Assets panel opens
            <== Elements panel closes
            !== both panels open simultaneously

      # Tab toggles all panels
      .if Elements panel and timeline are visible >>
        ||> .if user presses Tab >>
            <== all panels close (Elements, Assets, timeline, overview)
        ||> .if user presses Tab again >>
            <== timeline and overview reopen
    ```
