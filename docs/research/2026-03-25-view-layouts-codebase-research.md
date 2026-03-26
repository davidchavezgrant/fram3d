# Research: View Layouts (2.2.1) — Codebase State

**Date**: 2026-03-25
**Purpose**: Document existing codebase infrastructure to inform milestone 2.2.1 (View Layouts) implementation.

---

## Summary

The codebase currently operates as a **single-view system** with a Camera View / Director View toggle. There is one Unity Camera, one `CameraBehaviour`, and a `ViewMode` sealed class that switches between the shot camera and a director camera. All UI overlays (`AspectRatioMaskView`, `CompositionGuideView`, `DirectorViewBadge`) are full-screen UI Toolkit documents that position themselves absolutely and read `CameraBehaviour.RightInsetPixels` for panel inset. There are no UXML or USS layout documents defining multi-view structure — all UI is built programmatically in C#. The scene hierarchy has separate GameObjects for each UI overlay, each with its own `UIDocument` component. To implement 2.2.1, the major work is: (1) a Core-layer `ViewLayout` type and view slot model, (2) multiple Unity Cameras rendering to `RenderTexture` targets, (3) a UI Toolkit layout container that arranges `RawImage`-equivalent elements showing each camera's output, (4) per-view overlays scoped to their container rather than full-screen, and (5) a layout chooser UI.

---

## File Map

### Core Layer — `Unity/Fram3d/Assets/Scripts/Core/`

| File | Purpose | Relevance to 2.2.1 |
|------|---------|---------------------|
| `Scene/ViewMode.cs` | Sealed class: `CAMERA`, `DIRECTOR`. Tracks which camera the user looks through. | Needs extension: a view type per slot, not a single global toggle. Will need `DESIGNER` added for 2D Designer View. |
| `Camera/CameraElement.cs` | Domain camera with position, rotation, movement methods, lens/body/DOF. Inherits from `Element`. | Each view that isn't Camera View needs its own `CameraElement` for navigation. Director View already has one. |
| `Common/Element.cs` | Base class with Position, Rotation, Scale, GroundOffset, BoundingRadius. | Used by `ElementBehaviour` to wire scene elements. The frustum wireframe's `ElementBehaviour` wraps the shot `CameraElement`. |
| `Scene/Selection.cs` | Single-selection state: `HoveredId`, `SelectedId`. | Selection is global — shared across all views. Spec confirms this: "changes in one view are immediately reflected in all others." |
| `Scene/GizmoState.cs` | Active tool tracking. | Shared across views — gizmo appears in the active view. |
| `Viewport/AspectRatio.cs` | Sealed class with 8 ratios, cycling logic. | Only relevant in Camera View — other views render without aspect masks. |
| `Viewport/CompositionGuideSettings.cs` | Toggle state for thirds, center cross, safe zones. | Only relevant in Camera View. |

### Engine Layer — `Unity/Fram3d/Assets/Scripts/Engine/`

| File | Purpose | Relevance to 2.2.1 |
|------|---------|---------------------|
| `Integration/CameraBehaviour.cs` | Creates shot + director `CameraElement`s in `Awake()`. Single Unity `Camera` component. `Sync()` reads `ActiveCamera` and writes to Unity transform/properties. Manages DOF Volume, viewport rect, shake. Creates `FrustumWireframe` GO. `ToggleDirectorView()` swaps `_viewMode`. | **Central integration point.** Currently assumes a single Unity Camera. Multi-view requires multiple cameras or render-to-texture. The `Sync()` pipeline, DOF management, and viewport rect logic all need to become per-view or be split into a per-camera sync component. |
| `Integration/FrustumWireframe.cs` | Procedural wireframe mesh showing shot camera's frustum. Visible only in Director View. Rebuilds mesh every frame from `CameraElement.VerticalFov` and sensor aspect. Has a `BoxCollider` for selection raycasting. | In multi-view, the frustum should be visible in any view that isn't Camera View. Currently toggled by `CameraBehaviour.ToggleDirectorView()`. |
| `Integration/ElementBehaviour.cs` | MonoBehaviour wrapper for `Element`. Syncs Core transform to Unity transform in `LateUpdate()`. `Element` property has `internal set` for pre-wiring (used by frustum wireframe). | Shared across views — elements exist once in the scene and are rendered by all cameras. |
| `Integration/GizmoController.cs` | Manages gizmo handles, drag, tool switching. Uses `targetCamera` for raycasting. Layer 6 for isolation. | Currently uses a single `Camera` reference. Multi-view needs the gizmo to use the camera of the active view for raycasting. |
| `Integration/SelectionRaycaster.cs` | Raycasts from camera through screen position. `targetCamera` is serialized. | Needs to use the camera corresponding to whichever view the mouse is over. |
| `Integration/SelectionHighlighter.cs` | Visual feedback via `MaterialPropertyBlock`. Owns the `Selection` instance (created in `Awake()`). | View-independent — highlight is on the 3D object, visible from all cameras. |
| `Integration/ElementDuplicator.cs` | Static class, duplicates selected element. | View-independent. |
| `Integration/GroundPlane.cs` | Procedural ground mesh at Y=0. | Shared scene element, rendered by all cameras. |
| `Rendering/GizmoRenderFeature.cs` | URP `ScriptableRendererFeature`, enqueues `GizmoRenderPass`. | Currently attached to the single renderer. All cameras sharing the same renderer will render gizmos. If different cameras use different renderer assets, gizmo rendering needs to be configured per-renderer. |
| `Rendering/GizmoRenderPass.cs` | Draws layer 6 objects after transparents. | |
| `Conversion/VectorExtensions.cs` | System.Numerics <-> Unity coordinate conversion. | Shared utility, no change needed. |

### UI Layer — `Unity/Fram3d/Assets/Scripts/UI/`

| File | Purpose | Relevance to 2.2.1 |
|------|---------|---------------------|
| `Input/CameraInputHandler.cs` | Processes all camera keyboard/scroll/drag input. `_camera` reference swaps on view toggle (`HandleViewToggle`). Serialized refs to `cameraBehaviour`, `compositionGuides`, `gizmoController`, `propertiesPanel`. | Needs to route camera movement to the `CameraElement` of whichever view is active/focused. Currently uses `this.cameraBehaviour.ActiveCamera` which returns the right element based on `_viewMode`. In multi-view, input should target the view the mouse is hovering over. |
| `Input/SelectionInputHandler.cs` | Mouse click/hover/drag for selection and gizmo interaction. Serialized ref to `raycaster`. | Raycaster needs to use the camera of the view the mouse is over. |
| `Views/AspectRatioMaskView.cs` | Full-screen overlay, four absolutely positioned bars. Hides in Director View (`IsDirectorView` check in `Update()`). Reads `RightInsetPixels` for positioning. | In multi-view, this should only render in the Camera View's container, not full-screen. |
| `Views/CompositionGuideView.cs` | Full-screen overlay for thirds, center cross, safe zones. Hides in Director View. | Same — Camera View container only. |
| `Views/DirectorViewBadge.cs` | Full-screen overlay, "DIRECTOR VIEW" pill. Shows/hides based on `IsDirectorView`. | In multi-view, each view that shows Director View should have its own badge. |
| `Panels/PropertiesPanelView.cs` | Side panel with camera info, body/lens/sensor pickers. 440px wide. `Toggle()` method. Sets `CameraBehaviour.SetRightInset()`. | Stays as a global side panel (spec has it in the right gutter system). But the right inset logic needs to affect only the Camera View's container width, not all views. |
| `Panels/StyleSheetLoader.cs` | Loads `Resources/fram3d.uss` and caches it. Applies to root `VisualElement`. | Reusable, no change needed. |

### Shaders

| File | Purpose |
|------|---------|
| `Shaders/GizmoHandle.shader` | `ZTest Always`, `ZWrite Off`, flat `_Color`. Used by gizmo handles. |
| `Shaders/InfiniteGrid.shader` | Analytical grid for ground plane. |

### Stylesheet

| File | Purpose |
|------|---------|
| `Resources/fram3d.uss` | All UI Toolkit styles. Contains: properties panel, dropdowns, info rows, aspect mask, composition guides, director badge. |

### Assembly Definitions

| File | References | `noEngineReferences` |
|------|-----------|---------------------|
| `Core/Fram3d.Core.asmdef` | None | `true` (pure C#) |
| `Engine/Fram3d.Engine.asmdef` | `Fram3d.Core`, `Unity.InputSystem`, `Unity.RenderPipelines.Core.Runtime`, `Unity.RenderPipelines.Universal.Runtime` | `false` |
| `UI/Fram3d.UI.asmdef` | `Fram3d.Core`, `Fram3d.Engine`, `Unity.InputSystem` | `false` |
| `Tests/PlayMode/Fram3d.PlayMode.Tests.asmdef` | All three + test frameworks | `false` |

---

## How It Works Today

### Scene Hierarchy (SampleScene.unity)

```
Main Camera           ← Camera, CameraBehaviour, CameraInputHandler,
                        SelectionInputHandler, SelectionHighlighter,
                        SelectionRaycaster, GizmoController
Ground Plane          ← GroundPlane (procedural mesh)
Ref Cube A            ← ElementBehaviour
Ref Cube B            ← ElementBehaviour
Ref Sphere            ← ElementBehaviour
Properties Panel      ← UIDocument, PropertiesPanelView
Aspect Ratio Mask     ← UIDocument, AspectRatioMaskView
Composition Guides    ← UIDocument, CompositionGuideView
```

**Not in scene**: `DirectorViewBadge` — the `.cs` file exists on the current branch but has not been added to the scene yet. The `FrustumWireframe` is created at runtime by `CameraBehaviour.CreateFrustumWireframe()` as a child-less scene root named "Shot Camera Frustum".

### Single Camera Architecture

1. **One Unity Camera** (`Main Camera`). It has `CameraBehaviour` which creates TWO `CameraElement` instances in `Awake()`: `_cameraElement` (shot camera) and `_directorCamera` (utility camera).

2. **View toggle** (D key): `CameraInputHandler.HandleViewToggle()` calls `CameraBehaviour.ToggleDirectorView()`, which swaps `_viewMode` between `ViewMode.CAMERA` and `ViewMode.DIRECTOR`. The `ActiveCamera` property returns the appropriate `CameraElement`.

3. **Camera sync**: `CameraBehaviour.Sync()` runs in `LateUpdate()`. It reads from `ActiveCamera` (which might be shot or director) and writes position/rotation to the Unity transform, focal length to `_unityCamera.focalLength`, sensor size to `_unityCamera.sensorSize`. In Director View, DOF is forced off and shake is skipped.

4. **Input routing**: `CameraInputHandler._camera` starts as `cameraBehaviour.CameraElement` (shot camera) in `Start()`. On view toggle, `_camera = this.cameraBehaviour.ActiveCamera`. All movement methods (dolly, truck, crane, pan, tilt, orbit, scroll actions) operate on `_camera`, so they target whichever camera is active.

5. **Overlay suppression**: `AspectRatioMaskView.Update()` and `CompositionGuideView.Update()` check `_cameraBehaviour.IsDirectorView` — if true, they set their container to `DisplayStyle.None`. Camera overlays are Camera View only.

6. **Frustum wireframe**: Created in `CameraBehaviour.Awake()` as a new `GameObject("Shot Camera Frustum")` with `ElementBehaviour` (pointing at the shot camera's `CameraElement`) and `FrustumWireframe`. Starts as `SetActive(false)`. Activated/deactivated by `ToggleDirectorView()`.

### UI Overlay Pattern

All overlays follow the same structure:
- A separate `GameObject` in the scene with a `UIDocument` component
- A `MonoBehaviour` that builds UI elements in `Start()`, updates in `Update()`
- Elements are absolutely positioned within the UIDocument's root
- `PickingMode.Ignore` on all overlay elements
- USS classes from `Resources/fram3d.uss`, loaded via `StyleSheetLoader.Apply()`
- `CameraBehaviour.RightInsetPixels` used to offset containers when the properties panel is open

Each overlay's `UIDocument` creates its own independent visual tree rooted at the screen edges. This means overlays are **not** children of a shared layout container — they're independent full-screen layers.

### Key Architectural Constraint

The decisions doc states: **"All UI built programmatically in C# — no UXML, no USS files."** However, there IS a USS file (`Resources/fram3d.uss`) used for styling. The "no UXML" part holds — all layout is done via C# element construction. The correction to the decisions doc would be: "All UI structure built programmatically in C# — no UXML. Styles defined in USS."

---

## What 2.2.1 Needs to Change

### 1. Core Layer Changes

**New type: `ViewLayout`** (sealed class pattern, like `ViewMode` and `ActiveTool`):
- `SINGLE` — one view fills the workspace
- `SIDE_BY_SIDE` — two views, equal width
- `ONE_PLUS_TWO` — one large + two small

**Extend `ViewMode`**: Add `DESIGNER` for 2D Designer View (even if 8.2 Designer View is not implemented yet, the type needs to exist so the view selector dropdown can show it).

**New concept: view slots.** The current `ViewMode` is a global toggle. Multi-view needs a data structure that tracks which `ViewMode` is assigned to each slot. Something like `ViewMode[] _viewSlots` with 1-3 entries depending on `ViewLayout`. The Core layer owns the model (which layout, which view type per slot, which slot is active), the Engine layer owns the cameras, and the UI layer owns the visual containers.

### 2. Engine Layer Changes

**Multiple cameras.** Each view slot needs its own Unity Camera rendering to a `RenderTexture`. The Camera View camera has DOF, shake, viewport rect, sensor sizing. Director View / Designer View cameras are simpler (no DOF, no shake). `CameraBehaviour` currently assumes it IS the camera — this needs to become either:
- (a) Multiple `CameraBehaviour`-like components, one per view
- (b) A single coordinator that manages multiple Unity Cameras

Option (b) is probably cleaner — a `ViewManager` MonoBehaviour that owns N cameras and N `CameraElement`s, syncing each pair independently.

**Raycasting.** `SelectionRaycaster` and `GizmoController` both raycast from a single camera. In multi-view, they need to raycast from the camera of the view the mouse is hovering over.

**Frustum wireframe.** Currently toggled globally by `ToggleDirectorView()`. In multi-view, it should be visible in all non-Camera-View cameras and hidden from the Camera View camera (via layer exclusion or per-camera culling).

### 3. UI Layer Changes

**View container layout.** A root `VisualElement` that arranges 1-3 view containers based on the active `ViewLayout`. Each container holds:
- A top bar (22px) with a view type dropdown
- A rendering surface showing the camera output (likely a `VisualElement` displaying a `RenderTexture` via `Background`)
- View-specific overlays (aspect masks, guides, badges) scoped to that container

**Layout chooser.** Three buttons in the bottom-right corner of the view area. Selecting a layout triggers a restructure of the view containers.

**View type dropdown.** Each view's top bar has a dropdown with Camera View, Director View, and 2D Designer. Selecting Camera View in one view performs a "smart swap" with the view that currently holds Camera View.

**Overlay scoping.** Currently, overlays are full-screen `UIDocument`s. In multi-view, they need to be children of their view's container element, not full-screen. This means the overlay pattern changes from "independent UIDocument per overlay" to "overlay elements within a view container's visual tree."

**Input routing.** `CameraInputHandler` needs to know which view the mouse is over and route camera movements to that view's `CameraElement`. The simplest approach: track which view container the mouse is over, and set `_camera` to that view's `CameraElement`.

### 4. Scene Hierarchy Changes

The current scene has separate GameObjects for each UI overlay (Properties Panel, Aspect Ratio Mask, Composition Guides). With multi-view, the overlays would ideally be part of a single view management system rather than independent GameObjects. However, each `UIDocument` in Unity needs its own `GameObject` and `MonoBehaviour`. The approach could be:
- One `ViewManager` GameObject with a `UIDocument` that owns the layout container
- Overlay elements created as children within each view container (no separate overlay GameObjects)
- The Properties Panel remains a separate `UIDocument` (it's outside the view area)

---

## Existing Patterns to Follow

### Sealed Class Pattern (ViewLayout, ViewMode)
`ActiveTool.cs` (lines 1-24) and `ViewMode.cs` (lines 1-18) — sealed class with private constructor and `static readonly` instances. Each instance carries typed data. Use this for `ViewLayout`:
```csharp
public sealed class ViewLayout
{
    public static readonly ViewLayout SINGLE       = new("Single", 1);
    public static readonly ViewLayout SIDE_BY_SIDE = new("Side by Side", 2);
    public static readonly ViewLayout ONE_PLUS_TWO = new("One + Two", 3);
    private ViewLayout(string name, int viewCount) { ... }
    public string Name      { get; }
    public int    ViewCount { get; }
}
```

### Split Model Sync Pattern (per-view camera sync)
`CameraBehaviour.Sync()` — reads Core state, writes to Unity. A per-view sync would be simpler for non-Camera-View cameras: just position + rotation, no DOF/shake/viewport/sensor.

### UI Overlay Pattern (view-scoped overlays)
`AspectRatioMaskView` / `CompositionGuideView` — build elements, position absolutely, `PickingMode.Ignore`, USS classes. The same approach applies within a view container rather than full-screen.

### ElementBehaviour Pattern (frustum as selectable element)
The frustum wireframe uses `ElementBehaviour` to make it selectable. This pattern works for any view — clicking the frustum in any view selects the shot camera.

---

## Test Structure

### Core Tests (xUnit, `tests/Fram3d.Core.Tests/`)
- One test class per type: `CameraElementTests`, `SelectionTests`, `ActiveToolTests`, etc.
- Test naming: `MethodName__ExpectedBehavior__When__Condition`
- FluentAssertions
- No Unity dependencies

### Play Mode Tests (`Unity/Fram3d/Assets/Tests/PlayMode/`)
- NUnit, `[UnityTest]` for multi-frame, `[Test]` for synchronous
- SetUp creates GameObjects, TearDown destroys with `DestroyImmediate`
- CameraBehaviourTests: 35+ tests covering sync pipeline, DOF, shake, viewport rect, Director View toggle, frustum visibility
- Test naming follows same convention
- Key pattern: `SetUp` creates the minimum GameObjects, tests verify observable behavior
- CameraBehaviourTests.TearDown explicitly destroys the runtime-created "Shot Camera Frustum" GO

---

## Key References

- `Unity/Fram3d/Assets/Scripts/Core/Scene/ViewMode.cs:1-18` — Current view mode (CAMERA, DIRECTOR)
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:17-26` — Fields: single Unity Camera, two CameraElements, ViewMode
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:32-34` — `ActiveCamera` property (switches by view mode)
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:72-94` — `ToggleDirectorView()` (swap logic, frustum toggle)
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:130-151` — `Sync()` (per-frame, reads ActiveCamera, branches for Director View)
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:202-242` — `Awake()` (creates cameras, database, DOF, frustum)
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:234-242` — `CreateFrustumWireframe()` (runtime GO creation)
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/FrustumWireframe.cs:12-98` — Procedural mesh, per-frame rebuild, BoxCollider
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:389-404` — `HandleViewToggle()` (D key, swaps _camera ref)
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:49-79` — `Tick()` (input processing pipeline)
- `Unity/Fram3d/Assets/Scripts/UI/Views/AspectRatioMaskView.cs:97-134` — `Start()`/`Update()` (Director View suppression)
- `Unity/Fram3d/Assets/Scripts/UI/Views/CompositionGuideView.cs:201-237` — `Start()`/`Update()` (Director View suppression)
- `Unity/Fram3d/Assets/Scripts/UI/Views/DirectorViewBadge.cs:1-50` — Badge (not yet in scene)
- `Unity/Fram3d/Assets/Scripts/UI/Panels/PropertiesPanelView.cs:53-58` — `Toggle()` and right inset
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs:16` — `GIZMO_LAYER_INDEX = 6`
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/SelectionRaycaster.cs:29-54` — `Raycast()` uses single `targetCamera`
- `Unity/Fram3d/Assets/Resources/fram3d.uss:1-269` — All current styles
- `docs/specs/milestone-2.2-view-system-spec.md` — Full spec for view layouts and view types
- `docs/specs/ui-layout-spec.md:59-103` — View area layout design (single/side-by-side/one+two, layout chooser, overlays)
- `docs/reference/decisions.md:93` — "All UI built programmatically in C# — no UXML, no USS files" (note: USS does exist)

---

## Open Questions

1. **RenderTexture vs Camera stacking?** Unity 6 URP supports camera stacking (base + overlay cameras). An alternative to RenderTexture is using camera stacking with different viewports (`Camera.rect`). Camera stacking avoids RenderTexture overhead but limits layout flexibility (cameras render directly to screen, positioned by rect). RenderTexture allows UI Toolkit to display camera output as a `Background.renderTexture` on any `VisualElement`, enabling flexible layouts. For the three layout modes in the spec, either approach could work.

2. **How many UIDocuments?** Currently there are 3 overlay UIDocuments + 1 panel UIDocument. Multi-view options:
   - (a) One UIDocument for the entire view layout, with overlays as children of each view container
   - (b) Keep separate UIDocuments per overlay but scope them with CSS to their view container
   Option (a) is cleaner for multi-view — all view containers and their overlays share a single visual tree.

3. **Gizmo rendering per view.** The `GizmoRenderFeature` is a URP feature on the renderer asset. If all cameras share the same renderer, gizmos render in all views. This is correct for translate/rotate/scale handles (they should appear wherever the selected element is visible). But the frustum wireframe should only render in non-Camera-View cameras. This could be handled by:
   - Layer-based culling mask per camera
   - Separate renderer assets for Camera View vs other views
   - Toggling the frustum GO's renderer based on which camera is currently rendering

4. **Designer View stub.** The spec says Designer View should be available even before 8.2 is fully implemented. The simplest stub: an orthographic camera looking straight down with a fixed height, no DOF, no overlays. The actual 8.2 features (element icons, frustum visualization, labeled elements) come later.

5. **Input focus model.** In multi-view, which view receives camera movement input? The spec doesn't say explicitly. The most natural model: the view the mouse is currently hovering over receives camera input. Mouse click on an empty area within a view makes it the "active" view for keyboard shortcuts.

6. **Camera View "smart swap" implementation.** The spec says: "When a view is reassigned to Camera View, the view that previously held Camera View receives the reassigning view's old view type." This is a Core-layer operation on the view slot model — swap the ViewModes of two slots.
