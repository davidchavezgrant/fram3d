# FRA-52: View Layouts — Implementation Plan

**Date**: 2026-03-25
**Ticket**: FRA-52
**Spec**: `docs/specs/milestone-2.2-view-system-spec.md` §2.2.1
**Scope**: Layouts + view type selectors + layout chooser. Designer View is a placeholder.

---

## Architecture

**RenderTexture approach**: Each view gets a Unity Camera rendering to a RenderTexture. UI Toolkit displays the textures in VisualElements. Overlays are children of the Camera View's VisualElement. Input routes to the camera under the mouse.

Why RenderTexture over Camera.rect:
- UI Toolkit flexbox handles layout naturally (gaps, resizing)
- Overlays scoped to their view panel by parenting
- No pixel-position math to match viewport rects
- Clean separation: rendering (cameras) vs layout (UI Toolkit)

---

## Phases

### Phase 1: Core Types

**1.1 ViewLayout** — `Core/Common/ViewLayout.cs`
Sealed class: SINGLE (1 view), SIDE_BY_SIDE (2), ONE_PLUS_TWO (3). Properties: Name, ViewCount.

**1.2 ViewMode.DESIGNER** — `Core/Scene/ViewMode.cs`
Add `DESIGNER` static readonly instance.

**1.3 ViewSlotModel** — `Core/Common/ViewSlotModel.cs`
Pure C# class managing which ViewMode is in each slot.
- `Layout` property (get/set — changing preserves types per spec defaults)
- `GetSlotType(int)` / `SetSlotType(int, ViewMode)` (smart swap for Camera View)
- `CameraViewSlotIndex` — which slot has Camera View
- Invariant: exactly one slot has Camera View at all times

**1.4 xUnit tests** — `tests/Fram3d.Core.Tests/Common/ViewSlotModelTests.cs`

### Phase 2: Engine — Multi-Camera

**2.1 ViewCamera** — `Engine/Integration/ViewCamera.cs`
Manages one Unity Camera + RenderTexture pair. Creates/destroys the RT. Resizes RT on demand. Configures camera for each view type (Camera View = physical properties + DOF volume; Director View = perspective, no DOF; Designer = orthographic placeholder).

**2.2 Refactor CameraBehaviour** — `Engine/Integration/CameraBehaviour.cs`
- Add `SetTargetTexture(RenderTexture)` to render to RT instead of screen
- Extract `SyncCameraView(Camera)` / `SyncDirectorView(Camera)` so ViewCameraManager can call them
- Keep existing CameraElement management (shot camera, director camera, database)

**2.3 ViewCameraManager** — `Engine/Integration/ViewCameraManager.cs`
MonoBehaviour managing per-slot ViewCameras.
- Creates/destroys ViewCameras as ViewSlotModel changes
- Camera View slot's camera: full sync via CameraBehaviour (DOF, shake, sensor, focal length lerp)
- Director View slot's camera: position/rotation sync from director CameraElement
- Designer View slot's camera: disabled (placeholder)
- Exposes `GetCamera(int slot)` for raycasting
- Exposes `GetRenderTexture(int slot)` for UI
- Handles frustum wireframe visibility per camera (visible from Director, hidden from Camera View)

### Phase 3: UI — View Layout

**3.1 ViewLayoutView** — `UI/Views/ViewLayoutView.cs`
MonoBehaviour + UIDocument. Root VisualElement with:
- Flex container holding 1-3 ViewPanels
- Each ViewPanel: 22px header bar (view type dropdown) + content area (RenderTexture background)
- Layout chooser (bottom-right, absolutely positioned)
- Responds to layout/type changes by rebuilding flex structure and updating RT backgrounds

**3.2 View type dropdown** — built into ViewPanel header
Simple dropdown: Camera View, Director View, Designer View. Selection delegates to ViewSlotModel.SetSlotType().

**3.3 Layout chooser** — built into ViewLayoutView
Three buttons (icons or text) for SINGLE, SIDE_BY_SIDE, ONE_PLUS_TWO. Active layout highlighted.

**3.4 Designer View placeholder**
When a slot has Designer View, the ViewPanel shows a centered label "Designer View" with subtitle "Top-down scene layout (coming soon)" instead of a RenderTexture.

### Phase 4: Overlay Migration

**4.1 Consolidate overlays into Camera View panel**
- AspectRatioMaskView, CompositionGuideView, DirectorViewBadge build their elements into the Camera View's ViewPanel VisualElement instead of separate UIDocuments
- Remove old overlay GameObjects from scene (Aspect Ratio Mask, Composition Guides)
- Overlays position relative to the view panel, not full screen
- Director View badge shows on Director View panels (not just when CameraBehaviour.IsDirectorView)

**4.2 Remove RightInsetPixels from overlay positioning**
With RenderTexture, the camera renders to the full RT. The view panel's layout position already accounts for the Properties Panel. No need for manual inset on overlays.

### Phase 5: Input Routing

**5.1 Active view detection**
ViewLayoutView tracks which ViewPanel the mouse is over (PointerEnterEvent/PointerLeaveEvent on each panel).

**5.2 Camera input routing**
CameraInputHandler receives the active view's camera (CameraElement) and routes movement there. ViewLayoutView updates the reference when the active view changes.

**5.3 Selection/gizmo input routing**
SelectionRaycaster and GizmoController receive the active view's Unity Camera for raycasting. ViewLayoutView updates references when active view changes.

### Phase 6: Tests

**6.1 Core xUnit tests** (Phase 1.4)
ViewSlotModel: layout transitions, smart swap, Camera View invariant.

**6.2 Play Mode tests**
- ViewCameraManager creates correct number of cameras per layout
- Layout switching preserves view types
- Smart swap works through ViewLayoutView

---

## Out of Scope

- Designer View rendering (8.2 — placeholder only)
- Gutter tabs (no Elements/Assets panels yet)
- O/T/Tab shortcuts (no timeline/overview yet)
- Side panel drag resizing (Properties Panel width is fixed)
- Persisting layout across sessions (needs 4.2 Save/Load)

## Key Files Modified

| File | Change |
|------|--------|
| `Core/Scene/ViewMode.cs` | Add DESIGNER |
| `Core/Common/ViewLayout.cs` | New |
| `Core/Common/ViewSlotModel.cs` | New |
| `Engine/Integration/CameraBehaviour.cs` | Refactor for RT support, extract sync methods |
| `Engine/Integration/ViewCamera.cs` | New |
| `Engine/Integration/ViewCameraManager.cs` | New |
| `UI/Views/ViewLayoutView.cs` | New |
| `UI/Views/AspectRatioMaskView.cs` | Receive parent VisualElement instead of own UIDocument |
| `UI/Views/CompositionGuideView.cs` | Same |
| `UI/Views/DirectorViewBadge.cs` | Same |
| `UI/Input/CameraInputHandler.cs` | Accept active camera from ViewLayoutView |
| `Engine/Integration/SelectionRaycaster.cs` | Accept active camera dynamically |
| `Engine/Integration/GizmoController.cs` | Accept active camera dynamically |
| `Resources/fram3d.uss` | View layout styles |
