# Engine Test Suite Audit

**Date:** 2026-03-26
**Scope:** All Play Mode tests under `Unity/Fram3d/Assets/Tests/PlayMode/Engine/` and all Engine source files under `Unity/Fram3d/Assets/Scripts/Engine/`

## Summary

The Engine layer has 24 source files across 3 subdirectories (Conversion, Cursor, Integration, Rendering). Of these 24 files, 10 have corresponding test files — a 42% file coverage rate. The tested files are concentrated in the Integration subdirectory. The Cursor subsystem (5 files) and Rendering subsystem (2 files) have zero test coverage. Several tested classes have significant untested methods and behaviors.

## Engine Source File Inventory

### Conversion/ (1 file)
| File | Has Tests | Test File |
|------|-----------|-----------|
| `VectorExtensions.cs` | YES | `VectorExtensionsTests.cs` |

### Cursor/ (5 files)
| File | Has Tests | Notes |
|------|-----------|-------|
| `CursorService.cs` | NO | Static facade delegating to ICursorService |
| `CursorTextures.cs` | NO | Static lazy-loading from Resources/ |
| `ICursorService.cs` | NO | Interface + CursorType enum (no logic) |
| `SoftwareCursorOverlay.cs` | NO | MonoBehaviour rendering via OnGUI |
| `UnityCursorService.cs` | NO | ICursorService impl with software overlay |

### Integration/ (16 files)
| File | Has Tests | Test File |
|------|-----------|-----------|
| `CameraBehaviour.cs` | YES | `CameraBehaviourTests.cs` |
| `CameraDatabaseLoader.cs` | YES | `CameraDatabaseLoaderTests.cs` |
| `ElementBehaviour.cs` | YES | `ElementBehaviourTests.cs` |
| `ElementDuplicator.cs` | YES | `ElementDuplicatorTests.cs` |
| `ElementPicker.cs` | YES | `ElementPickerTests.cs` |
| `FrustumWireframe.cs` | NO | Mesh rebuild each frame, box collider sizing |
| `GizmoBehaviour.cs` | YES | `GizmoBehaviourTests.cs` |
| `GizmoHandleFactory.cs` | NO | Builds gizmo GO tree (internal) |
| `GizmoHandles.cs` | NO | Data class for gizmo GO references (internal) |
| `GizmoHighlighter.cs` | NO | Hover/drag color management (internal) |
| `GizmoMeshBuilder.cs` | YES | `GizmoMeshBuilderTests.cs` |
| `GroundPlane.cs` | NO | Creates mesh, collider, material in Awake |
| `SelectionDisplay.cs` | YES | `SelectionDisplayTests.cs` |
| `ShotEvaluator.cs` | NO | Timeline bridge, camera evaluation routing |
| `ViewCamera.cs` | NO | Director camera wrapper (simple) |
| `ViewCameraManager.cs` | YES | `ViewCameraManagerTests.cs` |

### Rendering/ (2 files)
| File | Has Tests | Notes |
|------|-----------|-------|
| `GizmoRenderFeature.cs` | NO | ScriptableRendererFeature (URP integration) |
| `GizmoRenderPass.cs` | NO | RenderGraph + legacy Execute paths |

## Tested Classes: Method/Behavior Coverage Detail

### VectorExtensionsTests.cs (7 tests)
**Fully tested.** All 4 conversion methods (ToSystem/ToUnity for Vector3 and Quaternion) plus roundtrip tests. This is complete coverage for the file's public API.

### CameraBehaviourTests.cs (39 tests)
**Heavily tested. Key tested areas:**
- Awake initialization: database loading, default body/lens/focal length, physical camera mode, gate fit, sensor size (6 tests)
- Sync position/rotation: coordinate conversion, crane world-Y, pan/tilt (5 tests)
- Sync focal length: lerp convergence, snap flag, preset snap, lens set switch (4 tests)
- Sync sensor size: body change, sensor mode change (3 tests)
- Sync viewport rect: panel inset, full height for any aspect ratio, no sensor aspect constraint (5 tests)
- Aspect ratio cycling: forward and backward (2 tests)
- DOF wiring: Bokeh/Off modes, focus distance, aperture, focal length tracks displayed value during lerp (5 tests)
- Shake: applies rotation offset, no base rotation drift (2 tests)
- DollyZoom: focal length match, snap flag consumed, boundary no-op (3 tests)
- Reset: restores default focal length, preserves sensor size (2 tests)
- Director View: toggle, active camera routing, position copy, position preservation, frustum show/hide, DOF suppression, position sync, shot camera isolation (10 tests)
- Sensor mode: SetSensorMode updates (1 test)

**Untested behaviors:**
- `BottomInsetPixels` property (reads from ShotEvaluator)
- `SetRightInset` + `SyncViewportRect` with both right and bottom insets simultaneously
- `CreateFrustumWireframe` fallback when wireframeShader is null (Shader.Find path)
- `EnsureDirectorInitialized` (called externally by ViewCameraManager)
- LateUpdate calling Sync every frame (implicit — tested via yield return null)

### CameraDatabaseLoaderTests.cs (6 tests)
**Tested areas:**
- Load returns non-null database
- Parses bodies (count > 5 generics)
- Parses lens sets (count > 1 generic)
- All bodies have valid sensor size
- Parses zoom lenses
- Parses sensor modes

**Untested behaviors:**
- File-not-found fallback (returns generic defaults only)
- Null cameras/lenses arrays in JSON (MapCameras/MapLenses null guards)
- Null sensor_modes in camera JSON

### ElementBehaviourTests.cs (9 tests)
**Tested areas:**
- Awake: creates Element, unique IDs, name matches GO, captures position at non-origin, captures rotation
- LateUpdate: syncs position, rotation, scale from Core to Unity
- Roundtrip: position doesn't drift after Awake + 2 frames

**Untested behaviors:**
- `ComputeGroundOffset` (renderer bounds calculation)
- Awake early-return when `Element` is already set (pre-assigned for FrustumWireframe)
- `Element` internal setter path

### ElementDuplicatorTests.cs (10 tests)
**Fully tested.** All `TryDuplicate` paths covered:
- Returns false when nothing selected, when null
- Creates clone, applies offset, copies scale/rotation
- Selects duplicate, assigns incremented name, sequential naming
- Creates independent element (moving duplicate doesn't affect original)

### ElementPickerTests.cs (5 tests)
**Tested areas:**
- Raycast hits element with collider
- Raycast returns null in empty space
- Raycast returns null for objects without ElementBehaviour
- Raycast resolves parent for compound elements (child collider)
- Raycast ignores Gizmo layer

**Untested behaviors:**
- `SetCamera` method
- Null targetCamera guard in Raycast
- `pixelRect.Contains` guard (raycast outside camera viewport)

### GizmoBehaviourTests.cs (14 tests)
**Tested areas:**
- SetActiveTool: defaults to Translate, changes tool
- LateUpdate: shows gizmo on selection, hides on deselection, resets to Translate on new selection
- TryBeginDrag: returns false when nothing selected, returns false when Select tool active
- UpdateHover: not hovering when no gizmo visible, not hovering when Select tool active
- TryResetActiveTool: resets position (Translate), rotation (Rotate), scale (Scale), returns false when nothing selected, returns false when Select tool active

**Untested behaviors:**
- `TryBeginDrag` success path (raycast hits a gizmo handle, creates DragSession)
- `UpdateDrag` — all three tool paths (translate projection, rotation, scale)
- `EndDrag` — clearing drag state
- `UpdateHover` success path (raycast hits handle, sets hover)
- `ClearHover` public method
- `SetCamera` method
- `ScaleForConstantScreenSize` (constant screen-size gizmo scaling)
- `IsWithinTargetViewport` viewport bounds check
- `IsDragging` property
- LateUpdate positioning gizmo at element position with identity rotation

### GizmoMeshBuilderTests.cs (9 tests)
**Tested areas:**
- CreateArrow: has vertices, has triangles, triangle indices in range
- CreateDiamond: has vertices (6), has triangles (24), indices in range
- CreateRing: has vertices, has triangles, indices in range

**Untested behaviors:**
- Mesh geometry correctness (specific vertex positions, normals)
- Mesh names assigned correctly

### SelectionDisplayTests.cs (8 tests)
**Tested areas:**
- Apply hover color (yellow)
- Apply select color (cyan)
- Remove color on deselect (PropertyBlock cleared)
- Remove hover on cursor leave
- Select overrides hover
- Original shared material preserved after deselect
- Compound element: both children get selection color
- Rapid selection transfer between elements

**Untested behaviors:**
- `_Color` (UNLIT_COLOR) property also being set (both BaseColor and Color are set but only BaseColor is asserted)

### ViewCameraManagerTests.cs (18 tests)
**Tested areas:**
- Awake: creates ViewSlotModel, defaults to single view
- Single view: ActiveCameraElement returns shot camera, GetUnityCamera returns main camera, GetUnityCameraAtPosition returns main camera, CameraViewRect returns full rect
- Entering multi-view: creates Director camera, main camera serves slot 0, forces out of Director View
- Returning to single: destroys Director cameras, restores full viewport
- Viewport rects: horizontal split (side by side), vertical split (top/bottom)
- ActiveCameraElement routing: shot camera for Camera slot, director camera for Director slot
- ActivateSlotAtPosition: no-op in single view
- GetUnityCamera bounds: null for negative index, null for out-of-range
- GetViewportRect: returns full for out-of-range
- Stale state: FRA-52 regression (director single to split clears state)
- Frustum visibility in multi-view
- Layout switching: horizontal to vertical preserves camera count

**Untested behaviors:**
- `ActivateSlotAtPosition` actually changing the active slot in multi-view
- `GetUnityCameraAtPosition` in multi-view (slot selection by position)
- `SyncDirectorCameras` (director camera position sync each frame)
- `ApplyViewportRects` with panel insets (right and bottom)
- `ComputeViewportRects` with non-zero insets
- `OnDestroy` cleanup
- `Start` fallback `FindAnyObjectByType<CameraBehaviour>()` path
- `SyncSingleViewDirectorToggle` when returning to single view from Director-in-slot-0

## Untested Engine Classes

### High Value (contain logic worth testing)
1. **`ShotEvaluator.cs`** — Creates Timeline, subscribes to shot changes and camera evaluation events, routes camera position/rotation on shot change. Contains `Awake`, `Start`, `OnDestroy` lifecycle and event subscription logic.
2. **`FrustumWireframe.cs`** — Rebuilds mesh every frame from camera FOV and sensor aspect. Contains frustum geometry math (near/far plane corners, collider sizing). Could be tested for mesh correctness given known camera parameters.
3. **`GizmoHighlighter.cs`** — Manages hover/drag color state on renderers. Has specific state machine behavior (drag clears hover, restore axis color). Internal but testable.

### Medium Value (mostly wiring)
4. **`GizmoHandleFactory.cs`** — Builds gizmo GO tree. Internal. Could verify handle counts and naming.
5. **`ViewCamera.cs`** — Simple wrapper. Constructor creates GameObject + Camera. `SyncDirectorView` sets position/rotation. `Destroy` destroys the GO.

### Low Value (hard to test or trivial)
6. **`CursorService.cs`** — Static facade with global mutable state.
7. **`CursorTextures.cs`** — Lazy loads from Resources. Requires actual texture files.
8. **`UnityCursorService.cs`** — Integrates Cursor.visible, SoftwareCursorOverlay. Hard to assert cursor state in tests.
9. **`SoftwareCursorOverlay.cs`** — OnGUI rendering. Not unit-testable.
10. **`ICursorService.cs`** — Interface + enum. No logic.
11. **`GizmoHandles.cs`** — Pure data class. No logic.
12. **`GroundPlane.cs`** — Creates mesh/collider/material. Mostly Unity API calls.
13. **`GizmoRenderFeature.cs`** — URP pipeline integration. Not unit-testable.
14. **`GizmoRenderPass.cs`** — URP RenderGraph pass. Not unit-testable.

## Test Patterns and Infrastructure

### Setup/TearDown Pattern
Every test class uses `[SetUp]`/`[TearDown]` with `DestroyImmediate` in TearDown. No shared base class or helper utilities exist — each test class is fully self-contained.

**Common setup structure:**
1. Create GameObjects and add components
2. Wire `[SerializeField]` fields via reflection (`typeof(T).GetField(..., BindingFlags.NonPublic | BindingFlags.Instance).SetValue(...)`)
3. Track dynamically created objects in `List<GameObject> _extras` for cleanup

**Common teardown structure:**
1. Iterate `_extras` and `DestroyImmediate` each
2. `DestroyImmediate` primary test objects
3. Find and destroy scene-root objects created by Awake (e.g., "Shot Camera Frustum")

### Reflection for SerializeField Wiring
Used in 3 test classes:
- `ElementPickerTests`: sets `targetCamera` on ElementPicker
- `GizmoBehaviourTests`: sets `selectionDisplay` and `targetCamera` on GizmoBehaviour
- `ViewCameraManagerTests`: sets `cameraBehaviour` and `_activeSlot` on ViewCameraManager

Pattern: `SetField(target, "fieldName", value)` static helper in each class that needs it (duplicated, not shared).

### [Test] vs [UnityTest] Usage
Tests correctly use `[Test]` (synchronous) when only verifying Awake-time state (no frames needed) and `[UnityTest]` (IEnumerator) when testing LateUpdate sync, multi-frame behavior, or physics.

### Mocking
No mocking frameworks are used. All tests use real Unity objects with reflection-based wiring. Domain interfaces (e.g., `ICursorService`) exist but are not mocked in Engine tests.

### Physics Testing
`ElementPickerTests` uses `yield return new WaitForFixedUpdate()` after setup to ensure physics colliders are registered before raycasting. Other tests that don't involve physics only use `yield return null`.

## Test Count Summary

| Test File | Test Count | Sync [Test] | Async [UnityTest] |
|-----------|-----------|-------------|-------------------|
| CameraBehaviourTests | 39 | 6 | 33 |
| CameraDatabaseLoaderTests | 6 | 0 | 6 |
| ElementBehaviourTests | 9 | 4 | 5 |
| ElementDuplicatorTests | 10 | 2 | 8 |
| ElementPickerTests | 5 | 0 | 5 |
| GizmoBehaviourTests | 14 | 2 | 12 |
| GizmoMeshBuilderTests | 9 | 9 | 0 |
| SelectionDisplayTests | 8 | 0 | 8 |
| VectorExtensionsTests | 7 | 7 | 0 |
| ViewCameraManagerTests | 18 | 6 | 12 |
| **Total** | **125** | **36** | **89** |

## Key References

- Test asmdef: `Unity/Fram3d/Assets/Tests/PlayMode/Fram3d.PlayMode.Tests.asmdef` — references Core, Engine, UI, InputSystem, URP
- Engine asmdef: `Unity/Fram3d/Assets/Scripts/Engine/Fram3d.Engine.asmdef` (not read, but referenced by tests)
- All 10 Engine test files: `Unity/Fram3d/Assets/Tests/PlayMode/Engine/*.cs`
- All 24 Engine source files: `Unity/Fram3d/Assets/Scripts/Engine/**/*.cs`
