# Research: Camera Infrastructure for Director View (2.1.5)

**Date**: 2026-03-25
**Purpose**: Document existing camera infrastructure to inform Director View implementation planning.

---

## Summary

The camera infrastructure follows the split model cleanly: `CameraElement` (Core, pure C#) owns all camera state and movement logic; `CameraBehaviour` (Engine, MonoBehaviour) syncs that state to the Unity Camera each frame via `LateUpdate`. `CameraInputHandler` (UI) routes all keyboard/mouse/scroll input to `CameraElement` methods. There is currently **no view system** — no ViewMode, ActiveView, DirectorView, or any view-related types exist in the codebase. The D key currently toggles DOF (needs to move to Shift+D). There is **no frustum wireframe rendering code** anywhere. The GizmoHandle shader (`ZTest Always`, `ZWrite Off`, flat color) and GizmoMeshBuilder (runtime procedural mesh creation) provide a reusable pattern for the frustum wireframe. The active tool badge mentioned in the 2.1.2 spec has **not been implemented yet** — no badge UI exists in the codebase.

---

## File Map

### Core Layer (`Unity/Fram3d/Assets/Scripts/Core/`)

| File | Purpose |
|------|---------|
| `Camera/CameraElement.cs` | Domain camera: position, rotation, movement methods (Pan, Tilt, Dolly, Truck, Crane, Orbit, Roll, DollyZoom), lens delegation, shake, DOF, reset |
| `Camera/LensController.cs` | Focal length, aperture, focus distance, lens set management |
| `Camera/BodyController.cs` | Camera body, sensor mode, aspect ratio, effective sensor dimensions |
| `Camera/MovementSpeeds.cs` | Tuned speed constants for all camera operations |
| `Common/Element.cs` | Base class: Position (Y-clamped), Rotation, Scale, GroundOffset, BoundingRadius |
| `Common/GizmoScaling.cs` | FOV-aware constant-screen-size scaling formula |
| `Input/DragRouter.cs` | Routes mouse drag + modifiers to Orbit or Pan/Tilt actions |
| `Input/ScrollRouter.cs` | Routes scroll + modifiers to camera operations (with bleed cooldown) |
| `Scene/ActiveTool.cs` | Sealed class pattern: SELECT, TRANSLATE, ROTATE, SCALE |
| `Scene/Selection.cs` | Single-selection state: HoveredId, SelectedId |
| `Scene/GizmoState.cs` | Active tool tracking, selection-change reset, tool-property reset |
| `Scene/DragSession.cs` | Active gizmo drag state: translation/rotation/scale computation |

### Engine Layer (`Unity/Fram3d/Assets/Scripts/Engine/`)

| File | Purpose |
|------|---------|
| `Integration/CameraBehaviour.cs` | Syncs CameraElement state to Unity Camera in LateUpdate. Manages DOF Volume, viewport rect, shake, focal length lerp |
| `Integration/ElementBehaviour.cs` | Syncs Element state to Unity Transform in LateUpdate |
| `Integration/GizmoController.cs` | Gizmo lifecycle: tool switching, drag begin/update/end, hover, constant-screen-size scaling |
| `Integration/GizmoHandleFactory.cs` | Procedural mesh creation for translate/rotate/scale handles, collider setup |
| `Integration/GizmoMeshBuilder.cs` | Creates arrow, ring, and diamond meshes at runtime |
| `Integration/GizmoHighlighter.cs` | Hover/drag color overlay via material property |
| `Integration/SelectionHighlighter.cs` | Selection/hover visual feedback via MaterialPropertyBlock |
| `Integration/SelectionRaycaster.cs` | Resolves screen position to domain Element via physics raycast |
| `Integration/GroundPlane.cs` | Procedural ground mesh + collider at Y=0 |
| `Rendering/GizmoRenderFeature.cs` | URP ScriptableRendererFeature for gizmo layer |
| `Rendering/GizmoRenderPass.cs` | Draws gizmo layer objects after transparents |
| `Conversion/VectorExtensions.cs` | System.Numerics <-> Unity coordinate conversion (Z negation) |

### UI Layer (`Unity/Fram3d/Assets/Scripts/UI/`)

| File | Purpose |
|------|---------|
| `Input/CameraInputHandler.cs` | All camera keyboard/scroll/drag input processing |
| `Input/SelectionInputHandler.cs` | Mouse click/hover/drag for selection and gizmo interaction |
| `Views/AspectRatioMaskView.cs` | UI Toolkit overlay: letterbox/pillarbox bars |
| `Views/CompositionGuideView.cs` | UI Toolkit overlay: thirds, center cross, safe zones |
| `Panels/PropertiesPanelView.cs` | Side panel with camera info, body/lens/sensor pickers |

### Shaders

| File | Purpose |
|------|---------|
| `Shaders/GizmoHandle.shader` | Flat color, ZTest Always, ZWrite Off, Cull Back |
| `Shaders/InfiniteGrid.shader` | Analytical grid for ground plane |

### Stylesheet

| File | Purpose |
|------|---------|
| `Resources/fram3d.uss` | All UI Toolkit styles. No badge styles exist yet. |

---

## How It Works

### 1. CameraBehaviour — Camera Sync Pipeline

**File**: `Engine/Integration/CameraBehaviour.cs`

`CameraBehaviour` is a `[RequireComponent(typeof(Camera))]` MonoBehaviour attached to the scene's main camera. It creates a `CameraElement` in `Awake()` and syncs its state to the Unity Camera every frame.

**Awake flow** (lines 141-168):
1. Gets the Unity `Camera` component
2. Creates a new `CameraElement` with a fresh GUID and name "Main Camera"
3. Loads the camera database via `CameraDatabaseLoader.Load()`
4. Enables physical camera properties (`usePhysicalProperties = true`, `gateFit = Overscan`)
5. Enables URP post-processing
6. Sets default camera body and lens set from the database
7. Creates a DOF Volume (priority 100, Bokeh mode)
8. Calls `Sync()` for initial state

**LateUpdate** (line 170-173): Calls `Sync()` every frame. Nothing else.

**Sync flow** (lines 78-90):
1. `transform.position = cam.Position.ToUnity()` — Core right-handed to Unity left-handed
2. `transform.rotation = cam.Rotation.ToUnity()` — X/Y negation for handedness
3. `SyncFocalLength` — lerps `_displayedFocalLength` toward target (speed 10), or snaps instantly if `SnapFocalLength` is true
4. `SyncDof` — maps DofEnabled/FocusDistance/Aperture to URP DepthOfField volume
5. Sets `_unityCamera.focalLength` and `sensorSize` from Core state
6. `SyncViewportRect` — shrinks camera rect to account for panel inset
7. `ApplyShake` — Perlin noise rotation overlay if shake enabled

**Public API**:
- `CameraElement` (read) — exposes the domain camera
- `Database` (read) — camera/lens database
- `ActiveAspectRatio`, `ActiveSensorMode` (read) — delegated from CameraElement
- `RightInsetPixels` (read) — panel reservation
- `SetRightInset(float)` — called by PropertiesPanelView
- `SetSensorMode(SensorMode)` — forwarded to CameraElement
- `CycleAspectRatioForward/Backward()` — forwarded to CameraElement

**Key observation for Director View**: `CameraBehaviour` currently creates and owns the single `CameraElement`. For Director View, we need a second camera (director camera) with independent position/rotation but no DOF/shake/lens. The director camera's state should NOT flow through `CameraBehaviour.Sync()` — it needs its own sync path or a second `CameraBehaviour`-like component.

### 2. CameraInputHandler — Input Routing

**File**: `UI/Input/CameraInputHandler.cs`

Processes all camera-related input. Has `[SerializeField]` references to:
- `cameraBehaviour` — for aspect ratio cycling
- `compositionGuides` — for guide toggle shortcuts
- `gizmoController` — for tool switching (Q/W/E/R)
- `propertiesPanel` — for panel toggle (I key) and input suppression when panel has focus

**Update flow** (line 490): `Update() => Tick(Keyboard.current, Mouse.current)`

**Tick flow** (lines 49-79):
1. Null checks on `_camera`, keyboard, mouse
2. If text field focused in panel, clear scroll queue and return
3. `HandleKeyboardInput()` — processes keyboard shortcuts
4. If pointer over panel UI, clear scroll queue and return
5. `ProcessQueuedScroll()` — processes event-intercepted scroll samples
6. `HandleDragInput()` — processes mouse drag for orbit/pan-tilt

**Keyboard routing** (lines 295-309) — order matters, first match returns:
1. `HandleToolSwitching` — Q/W/E/R (no modifiers), sets active tool via GizmoController
2. `HandlePanelToggle` — I key (no modifiers), toggles properties panel
3. `HandleAspectRatio` — A key (Shift+A backward), cycles aspect ratio
4. `HandleReset` — Ctrl+R, tries gizmo reset then camera reset
5. `HandleToggles` — **D key toggles DOF** (line 360), `[`/`]` for aperture, S for shake
6. `HandleGuideShortcuts` — G (all), Shift+G (thirds), Ctrl+G (center), Alt+G (safe zones)
7. `HandleFocalLengthPresets` — number keys 1-9

**D key DOF toggle** (lines 359-364):
```csharp
if (keyboard.dKey.wasPressedThisFrame && !keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed && !keyboard.shiftKey.isPressed)
{
    this._camera.DofEnabled = !this._camera.DofEnabled;
    return true;
}
```
This needs to change to `Shift+D` for DOF, freeing bare `D` for Director View toggle.

**Scroll routing** uses event-level interception (`InputSystem.onEvent`) to pair scroll events with per-event modifier state, preventing scroll bleed on macOS trackpads. Scroll samples are queued and processed in Update via `ScrollRouter`.

**Drag routing** (lines 184-212): Polls `mouse.delta.ReadValue()` and `keyboard` modifier state, passes through `DragRouter.Route()`. Alt+LeftButton = Orbit, Cmd+LeftButton or MiddleButton = Pan/Tilt.

**Key observation for Director View**: `CameraInputHandler` stores a single `_camera` reference (line 27) set in `Start()` from `cameraBehaviour.CameraElement`. For Director View, all the movement methods (dolly, truck, crane, pan, tilt, orbit, roll) need to target the director camera instead of the shot camera. The `_camera` reference needs to switch based on view state, OR the input handler needs to be view-aware and route to different CameraElement instances.

### 3. View System — Nothing Exists

A search for `ViewMode`, `DirectorView`, `ActiveView`, `ViewType` across all scripts found **zero matches**. The codebase has no view abstraction layer. The current architecture assumes a single camera (the shot camera) renders everything.

The UI layout spec (Section 2.4) has a placeholder for Director View: "film clapper icon, 'Director View' label, hint text." The spec for the view system (milestone 2.2) describes multi-view layouts but is a separate milestone that depends on 2.1.5.

**What needs to be built from scratch**:
- A view state concept (Core layer) — which view is active, Camera View vs Director View
- A director camera CameraElement (Core) — independent position/rotation, no DOF/shake
- Director View camera sync (Engine) — maps director CameraElement to a Unity Camera
- View toggle input handling (UI) — D key switches views
- Shot camera frustum wireframe (Engine) — visible in Director View

### 4. Frustum Wireframe — Nothing Exists

A search for `frustum` and `wireframe` across all scripts found **zero matches**. No camera frustum visualization code exists anywhere in the codebase.

**Reusable patterns from gizmos**:

The gizmo system provides a proven pattern for always-on-top wireframe rendering:

- **Shader**: `Shaders/GizmoHandle.shader` — `ZTest Always`, `ZWrite Off`, flat `_Color` property. This shader (or a similar one) can be reused for frustum wireframe lines.

- **Mesh building**: `GizmoMeshBuilder` creates procedural meshes at runtime (arrows, rings, diamonds). A similar `FrustumMeshBuilder` could create the wireframe lines from FOV and near/far planes.

- **Layer isolation**: Gizmo objects use layer index 6 (`GizmoController.GIZMO_LAYER_INDEX`). The `GizmoRenderFeature`/`GizmoRenderPass` renders this layer after transparents. Frustum wireframe objects could share this layer or use a separate one.

- **Constant screen size**: `GizmoScaling.CalculateZoomScale()` computes FOV-aware scale factors. The frustum wireframe does NOT need this — it should render at actual world-space size based on the shot camera's FOV and a chosen near/far distance.

- **SelectionRaycaster exclusion**: Already excludes gizmo layer (line 64-71 of `SelectionRaycaster.cs`). If frustum uses the same layer, it's automatically excluded from element selection raycasts.

### 5. Active Tool Badge — Not Implemented

The 2.1.2 spec describes an "active tool badge" in the bottom-left corner showing icon, tool name, and keyboard shortcut. The ui-layout-spec Section 3.4 specifies:
- Bottom-left corner
- Blue icon (`#4a9eff`)
- Tool name in uppercase
- Keyboard shortcut in a subtle pill

**But no badge UI has been implemented.** There are no classes or USS rules for any badge. The `ActiveTool` class (Core) carries `Name` and `Shortcut` properties that would feed the badge data.

The Director View badge (ui-layout-spec Section 3.5) should be:
- Top-center of the frame
- Pink-red pill (`#ff6688` on `rgba(255,68,102,0.25)` background)
- Bold uppercase "DIRECTOR VIEW" with 2px letter spacing
- Visible only when Director View is active

Both badges would be UI Toolkit `VisualElement` overlays following the same pattern as `AspectRatioMaskView` and `CompositionGuideView`: a MonoBehaviour with a UIDocument, building elements in `Start()`, updating in `Update()`, using `PickingMode.Ignore` so clicks pass through.

### 6. Overlay Pattern (for badges)

All existing overlays follow the same structure:

1. MonoBehaviour attached to a GameObject with a `UIDocument` component
2. In `Start()`: find `CameraBehaviour`, get `rootVisualElement`, call `StyleSheetLoader.Apply()`, build overlay elements
3. Position elements absolutely within a full-screen container
4. Set `pickingMode = PickingMode.Ignore` on all elements
5. Account for `CameraBehaviour.RightInsetPixels` when positioning (properties panel reservation)
6. Update positions in `Update()` each frame
7. USS styles in `Resources/fram3d.uss`

### 7. GizmoController Architecture (reference for frustum selectability)

The spec says the shot camera frustum wireframe should be selectable like any element — clicking it selects the shot camera rig, and gizmos appear for repositioning.

The existing `ElementBehaviour` + `SelectionRaycaster` pattern handles this: any GameObject with a collider and an `ElementBehaviour` component is selectable. The frustum wireframe would need:
- An `ElementBehaviour` (or a new `CameraFrustumBehaviour`) pointing at the shot camera's `CameraElement`
- Colliders on the frustum geometry for raycasting
- Layer setup to be selectable by `SelectionRaycaster` but not treated as a gizmo

The key question is how dragging the shot camera frustum via gizmo writes back to the shot camera's `CameraElement.Position`/`Rotation`. Currently `DragSession` writes to `Element.Position` directly. If the frustum's `ElementBehaviour` points to the shot camera's `CameraElement` (which inherits from `Element`), gizmo drag would write directly to the camera's domain state, and `CameraBehaviour.Sync()` would pick it up next frame.

---

## Existing Patterns

### Sealed Class Pattern (for view type)
`ActiveTool.cs` — sealed class with private constructor and `static readonly` instances. Each carries typed data (Name, Shortcut). Use this same pattern for a `ViewType` or `ActiveView` class: `CAMERA_VIEW`, `DIRECTOR_VIEW` (and later `DESIGNER_VIEW`).

### Split Model Sync Pattern (for director camera)
`CameraBehaviour.Sync()` — reads Core state, writes to Unity Transform. The director camera needs a similar sync, but simpler (no DOF volume, no shake, no focal length lerp, no viewport rect adjustment).

### Procedural Mesh Pattern (for frustum wireframe)
`GizmoMeshBuilder` — creates meshes from vertex/triangle arrays at runtime. A frustum wireframe mesh would compute vertices from the camera's FOV, aspect ratio, and chosen near/far distances.

### UI Toolkit Overlay Pattern (for badges)
`AspectRatioMaskView` / `CompositionGuideView` — MonoBehaviour + UIDocument, absolute positioning, `PickingMode.Ignore`, accounts for panel inset, USS styles in shared stylesheet.

---

## Key References

- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:78-90` — Sync pipeline
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:141-168` — Awake initialization
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:359-364` — D key DOF toggle (must move to Shift+D)
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:49-79` — Tick flow
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:184-212` — Drag routing to camera
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:489` — `_camera` reference set in Start
- `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs:7-8` — Inherits from Element
- `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs:88-105` — Movement methods (Crane, Dolly)
- `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs:143-154` — Orbit method
- `Unity/Fram3d/Assets/Scripts/Core/Scene/ActiveTool.cs:1-24` — Sealed class pattern
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs:16` — Layer index 6
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoMeshBuilder.cs` — Procedural mesh creation
- `Unity/Fram3d/Assets/Scripts/Engine/Rendering/GizmoRenderFeature.cs` — URP render feature
- `Unity/Fram3d/Assets/Scripts/Engine/Rendering/GizmoRenderPass.cs` — Layer-filtered render pass
- `Unity/Fram3d/Assets/Shaders/GizmoHandle.shader:19-20` — ZTest Always, ZWrite Off
- `Unity/Fram3d/Assets/Scripts/UI/Views/AspectRatioMaskView.cs` — Overlay pattern reference
- `Unity/Fram3d/Assets/Scripts/UI/Views/CompositionGuideView.cs` — Overlay pattern reference
- `Unity/Fram3d/Assets/Resources/fram3d.uss` — Shared stylesheet (no badge styles yet)
- `docs/specs/milestone-2.1-scene-management-spec.md:351-485` — Director View spec
- `docs/specs/milestone-2.2-view-system-spec.md` — Future multi-view system spec
- `docs/specs/ui-layout-spec.md:162-177` — Active tool badge visual design
- `docs/specs/ui-layout-spec.md:175-177` — Director View badge visual design

---

## Open Questions

1. **Director camera as CameraElement or plain Element?** The director camera needs position/rotation and movement methods (pan, tilt, dolly, etc.) but NOT lens/DOF/shake/body. CameraElement inherits from Element and adds lens/body. Options: (a) use CameraElement but ignore lens features, (b) create a new lightweight class, (c) use the movement methods directly on Element by extracting them. CameraElement's movement methods all operate on Position/Rotation inherited from Element, plus LookDirection (computed from Rotation). The director camera likely SHOULD be a CameraElement since all movement methods are on CameraElement, not Element.

2. **Second Unity Camera for Director View?** When Director View is active, the director camera needs to render the scene. Options: (a) reuse the existing Unity Camera by switching which CameraElement drives it, (b) create a second Unity Camera. For 2.1.5 (single view toggle), option (a) is simpler — swap the CameraElement that `CameraBehaviour.Sync()` reads from. For 2.2 (multi-view), option (b) will be needed. Design for (b) but implement (a) first.

3. **Frustum wireframe mesh update frequency?** The frustum needs to update when the shot camera's FOV or position changes. During playback, the shot camera animates. The mesh vertices depend on FOV (which depends on focal length), so the mesh may need rebuilding every frame during playback, or the wireframe could use a fixed representative near/far distance and only rebuild when focal length changes significantly.

4. **Input routing architecture?** The simplest approach: `CameraInputHandler` holds a reference to the "active" CameraElement (either shot camera or director camera). When view toggles, swap the reference. Movement methods on CameraElement work identically for both. Only difference: the director camera's movements never create keyframes (relevant for future milestone 3.2, not now).
