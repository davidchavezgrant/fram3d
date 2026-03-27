# Research: Play Mode Test Isolation Issues

**Date:** 2026-03-26
**Scope:** AspectRatioMaskViewTests, CameraInputHandlerTests, CursorBehaviourTests, SelectionInputHandlerTests

## Summary

Four Play Mode test files pass individually but fail when run together, indicating state leaks between test classes. Analysis reveals multiple categories of isolation risk: **static singletons** (CursorService, SoftwareCursorOverlay, SearchableDropdown, StyleSheetLoader), **InputSystem.onEvent handler leaks**, **untracked mid-test GameObjects**, **FindAnyObjectByType cross-contamination**, and **stale FrustumWireframe/GizmoRoot objects**. The highest-risk issues are the `InputSystem.onEvent` subscription in CameraInputHandler (which survives if `OnDisable` doesn't fire before TearDown) and the `CursorService._service` static field (which UnityCursorService sets via `[RuntimeInitializeOnLoadMethod]` and CursorBehaviourTests replaces without guaranteed restoration on test failure).

---

## Per-File Analysis

### 1. AspectRatioMaskViewTests

**File:** `Unity/Fram3d/Assets/Tests/PlayMode/UI/AspectRatioMaskViewTests.cs`

#### SetUp creates (lines 28-54):
| Object | Type | Field |
|--------|------|-------|
| `_cameraGo` | GameObject("TestCamera") | `_cameraGo` |
| CameraBehaviour | Component on `_cameraGo` | `_behaviour` |
| `_uiGo` | GameObject("TestUI") | `_uiGo` |
| UIDocument | Component on `_uiGo` | `_uiDocument` |
| PanelSettings | Instantiated from asset | `_panelSettings` |
| RenderTexture | 1920x1080 | `_renderTexture` |
| AspectRatioMaskView | Component on `_uiGo` | `_maskView` |

#### TearDown destroys (lines 104-128):
- `_uiGo` (DestroyImmediate)
- All FrustumWireframe objects found via FindObjectsByType (DestroyImmediate)
- `_cameraGo` (DestroyImmediate)
- `_renderTexture` (Release + DestroyImmediate)
- `_panelSettings` (DestroyImmediate)

#### Implicit objects created during SetUp (not directly tracked):
- **CameraBehaviour.Awake()** (line 247 of CameraBehaviour.cs) creates:
  - A `CameraDatabase` via `CameraDatabaseLoader.Load()` — **logs "Camera database loaded"** every time (line 30 of CameraDatabaseLoader.cs). This is a new instance each test, not cached, so no state leak here.
  - A `FrustumWireframe` GameObject named "Shot Camera Frustum" (line 298 of CameraBehaviour.cs) — created as a scene root, NOT parented to `_cameraGo`. **TearDown handles this** by searching for all FrustumWireframe objects.
  - An `ElementBehaviour` on the frustum GO (line 299).
  - A `Volume` component with a DOF override (line 157).

#### Risks:
1. **FrustumWireframe is a scene root.** Created at CameraBehaviour.Awake:298 via `new GameObject("Shot Camera Frustum")` — not parented to `_cameraGo`. If TearDown's `FindObjectsByType<FrustumWireframe>` misses it (e.g., if `_cameraGo` was already destroyed and the component was nulled), it leaks.
2. **AspectRatioMaskView.Start() calls `FindAnyObjectByType<CameraBehaviour>()`** (line 103 of AspectRatioMaskView.cs). If a stale CameraBehaviour from another test class exists, it finds the wrong one.
3. **StyleSheetLoader.\_cached** is a static field (line 11 of StyleSheetLoader.cs). Once loaded, it persists across tests. Not harmful per se, but worth noting.

---

### 2. CameraInputHandlerTests

**File:** `Unity/Fram3d/Assets/Tests/PlayMode/UI/CameraInputHandlerTests.cs`

**Extends `InputTestFixture`** — this is critical. InputTestFixture isolates the Input System by removing all real devices and creating a sandbox. `base.Setup()` and `base.TearDown()` handle this.

#### SetUp creates (lines 742-778):
| Object | Type | Field |
|--------|------|-------|
| `_keyboard` | Keyboard (InputSystem device) | `_keyboard` |
| `_mouse` | Mouse (InputSystem device) | `_mouse` |
| `_go` | GameObject("TestCamera") | `_go` |
| CameraBehaviour | Component on `_go` | `_behaviour` |
| CameraInputHandler | Component on `_go` | `_handler` |
| `_guideGo` | GameObject("TestGuides") | `_guideGo` |
| UIDocument | Component on `_guideGo` | — |
| CompositionGuideView | Component on `_guideGo` | `_guideView` |
| SelectionDisplay | Component on `_go` | `highlighter` |
| GizmoBehaviour | Component on `_go` | `_gizmoController` |

#### TearDown destroys (lines 843-863):
- GizmoRoot via `GameObject.Find("GizmoRoot")` (DestroyImmediate)
- All FrustumWireframe objects (DestroyImmediate)
- `_guideGo` (DestroyImmediate)
- `_go` (DestroyImmediate)
- `base.TearDown()` — restores InputSystem state

#### Critical static state — InputSystem.onEvent:
- **CameraInputHandler.OnEnable()** (line 392) registers `InputSystem.onEvent += this.HandleInputEvent`
- **CameraInputHandler.OnDisable()** (line 385) unregisters `InputSystem.onEvent -= this.HandleInputEvent`
- Since `InputTestFixture.TearDown()` is called via `base.TearDown()` AFTER the GameObjects are destroyed, the handler should be unregistered when the component is destroyed. However, the order matters:
  - TearDown destroys `_go` first (which triggers OnDisable, unregistering the event)
  - Then calls `base.TearDown()`
  - **This is correct** — the handler is unregistered before InputSystem teardown.

#### Implicit objects not directly tracked:
- **GizmoBehaviour.Awake()** (line 241 of GizmoBehaviour.cs) creates a `GizmoRoot` GameObject parented to the GizmoBehaviour's transform. Since GizmoBehaviour is on `_go`, the GizmoRoot is a child of `_go` and would be destroyed with it. **But TearDown also explicitly finds and destroys GizmoRoot** via `GameObject.Find("GizmoRoot")` — this is redundant but safe.
- **CameraBehaviour.Awake()** creates FrustumWireframe as a **scene root** (not parented to `_go`). TearDown handles this via FindObjectsByType.
- **CameraInputHandler.Start()** (line 400-402) calls `FindAnyObjectByType<PropertiesPanelView>()`, `FindAnyObjectByType<ViewLayoutView>()`, and `FindAnyObjectByType<TimelineSectionView>()`. If these exist from another test, they could be found.

#### Mid-test objects:
- **HasFocusedTextField test** (line 382) creates `panelGo = new GameObject("TestPanel")` with UIDocument and PropertiesPanelView. It calls `DestroyImmediate(panelGo)` at line 451, **but only on the success path**. If an assertion fails before line 451 or if the early-exit paths at lines 413/424 run, `panelGo` is destroyed at those points. However, if an unexpected exception occurs after line 399 but before any explicit destroy, `panelGo` leaks. **This is a risk.**

#### Risks:
1. **GizmoRoot is parented to `_go` transform** (GizmoHandleFactory.cs line 48-49: `root.transform.SetParent(parent, false)`), but TearDown tries to find it via `GameObject.Find("GizmoRoot")`. `GameObject.Find` cannot find inactive objects. If GizmoRoot was deactivated (line 250: `this._gizmoRoot.SetActive(false)`), Find returns null — but this is OK because it's a child of `_go` and will be destroyed with its parent.
2. **`panelGo` leak in HasFocusedTextField test** if an exception occurs mid-test.
3. **FrustumWireframe** created as scene root — handled by TearDown but relies on `FindObjectsByType`.

---

### 3. CursorBehaviourTests

**File:** `Unity/Fram3d/Assets/Tests/PlayMode/UI/CursorBehaviourTests.cs`

**Does NOT extend InputTestFixture.** Uses raw `InputSystem.AddDevice`/`RemoveDevice`.

#### SetUp creates (lines 86-108):
| Object | Type | Field |
|--------|------|-------|
| `_cameraGo` | GameObject("TestCamera") | `_cameraGo` |
| Camera | Component on `_cameraGo` | local `camera` |
| ElementPicker | Component on `_cameraGo` | `raycaster` |
| SelectionDisplay | Component on `_cameraGo` | `_highlighter` |
| SelectionInputHandler | Component on `_cameraGo` | `_handler` |
| `_cube` | CreatePrimitive(Cube) | `_cube` |
| ElementBehaviour | Component on `_cube` | — |
| `_mouse` | Mouse (InputSystem device) | `_mouse` |
| RecordingCursorService | Test double | `_cursorService` |

#### TearDown destroys (lines 111-124):
- Restores original cursor service via `CursorService.SetService(this._originalService)` — **but only if `_originalService != null`**
- Queues empty mouse state, removes mouse device
- `_cube` (DestroyImmediate)
- `_cameraGo` (DestroyImmediate)

#### Critical static state — CursorService._service:
- **`UnityCursorService.Setup()`** (line 20 of UnityCursorService.cs) runs via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` — this sets `CursorService._service` to a `UnityCursorService` instance at domain load time.
- **SetUp saves `_originalService`** (line 105) via reflection, then replaces it with RecordingCursorService.
- **TearDown restores** — but **only if `_originalService != null`**. Since `GetStaticField` reads the field via reflection, this should always work. However, if SetUp fails partway through (e.g., Shader.Find returns null for "Fram3d/GizmoHandle"), `_originalService` might not be set and TearDown would leave the RecordingCursorService in place.
- **More critically:** If TearDown itself fails (assertion in a [UnityTest] throws before TearDown runs — not possible, TearDown always runs), or if tests run in an unexpected order, the RecordingCursorService could leak into other test classes.

#### Mid-test objects:
- **UpdateCursor__SetsClosedHand__When__GizmoDragActive** (line 285) creates:
  - `GizmoBehaviour` component on `_cameraGo` — will be destroyed with `_cameraGo`.
  - **But GizmoBehaviour.Awake() creates a "GizmoRoot" child** (GizmoHandleFactory line 48). This is parented to `_cameraGo` and will be destroyed with it.
  - **However**, `Shader.Find("Fram3d/GizmoHandle")` at GizmoBehaviour.cs:243 may return null in test context, which would cause `new Material(null)` to throw. If this happens, the test would fail and the GizmoBehaviour would be in a broken state.

#### Risks:
1. **CursorService._service is static** — the test replaces it and restores in TearDown, but any test ordering issue or TearDown failure leaves the RecordingCursorService in place for subsequent tests.
2. **No InputTestFixture** — `InputSystem.AddDevice<Mouse>()` in SetUp adds a device to the REAL Input System, not a sandboxed one. If RemoveDevice fails or is skipped, the extra mouse persists.
3. **`SetCursor__ReturnsFalse__When__NoServiceSet`** (line 72) calls `CursorService.SetService(null)` then immediately restores. If the test fails between these calls, `_service` stays null for subsequent tests.
4. **SelectionInputHandler.Start()** calls `FindAnyObjectByType<PropertiesPanelView>()` and `FindAnyObjectByType<ViewLayoutView>()`. If these exist from another test class, they get wired in.
5. **No keyboard device created** — `_keyboard` field doesn't exist, but `SelectionInputHandler.Update()` reads `Keyboard.current`. If `CameraInputHandlerTests` left a keyboard device registered, it would be found.

---

### 4. SelectionInputHandlerTests

**File:** `Unity/Fram3d/Assets/Tests/PlayMode/UI/SelectionInputHandlerTests.cs`

**Does NOT extend InputTestFixture.** Uses raw `InputSystem.AddDevice`/`RemoveDevice`.

#### SetUp creates (lines 454-473):
| Object | Type | Field |
|--------|------|-------|
| `_cameraGo` | GameObject("TestCamera") | `_cameraGo` |
| Camera | Component on `_cameraGo` | local `camera` |
| ElementPicker | Component on `_cameraGo` | `raycaster` |
| SelectionDisplay | Component on `_cameraGo` | `_highlighter` |
| SelectionInputHandler | Component on `_cameraGo` | `_handler` |
| `_cube` | CreatePrimitive(Cube) | `_cube` |
| ElementBehaviour | Component on `_cube` | — |
| `_keyboard` | Keyboard (InputSystem device) | `_keyboard` |
| `_mouse` | Mouse (InputSystem device) | `_mouse` |

#### TearDown destroys (lines 475-484):
- Queues empty keyboard and mouse states
- Removes keyboard and mouse devices
- `_cube` (DestroyImmediate)
- `_cameraGo` (DestroyImmediate)

#### Mid-test objects:
- **CtrlD__DuplicatesElement__When__ElementSelected** (line 205) duplicates an element via `ElementDuplicator.TryDuplicate()`. This creates a clone GameObject. The test manually destroys the duplicate at lines 232-238 — **but only on the success path**. If the assertion at line 225 fails, the forEach cleanup at 232 doesn't run, and the duplicate leaks.

#### Risks:
1. **Duplicate element leak** in CtrlD test if assertions fail before cleanup.
2. **No InputTestFixture** — devices added to real Input System. If CameraInputHandlerTests runs before this and its InputTestFixture teardown removes all devices, the Keyboard.current/Mouse.current in this test's `SelectionInputHandler.Update()` would be the test-created devices. But if CameraInputHandlerTests runs after and its SetUp calls `base.Setup()` which removes all devices, the devices from SelectionInputHandlerTests would vanish mid-run.
3. **SelectionInputHandler.Update()** (line 328) calls `this.Tick(Mouse.current, Keyboard.current)`. In tests that also call `this._handler.Tick(this._mouse, this._keyboard)` explicitly, the shortcut fires twice per frame — the CLAUDE.md documents this as a known gotcha for TOGGLE operations. Tests in this file DO call Tick explicitly (lines 93, 96, 107, 131, 136, 148, 171, 183, 195, 435, 438, 444) alongside `Update()` running automatically. For idempotent SET operations this is fine, but for CtrlD (duplicate), this could cause double-duplication.
4. **FindAnyObjectByType contamination** from `SelectionInputHandler.Start()` finding PropertiesPanelView or ViewLayoutView from other tests.

---

## Cross-Cutting Issues

### 1. CursorService._service (static singleton)

**Location:** `Engine/Cursor/CursorService.cs:5`

`UnityCursorService.Setup()` sets this via `[RuntimeInitializeOnLoadMethod]` at domain load. CursorBehaviourTests replaces it in SetUp and restores in TearDown. If tests from another class interact with CursorService between CursorBehaviourTests' SetUp/TearDown, they get the RecordingCursorService. SelectionInputHandler.ResetCustomCursor() and SetPointerCursor() call CursorService methods.

**Affected tests:** CursorBehaviourTests (writes), SelectionInputHandlerTests (reads indirectly via SelectionInputHandler.Update).

### 2. SoftwareCursorOverlay._instance (static singleton + DontDestroyOnLoad)

**Location:** `Engine/Cursor/SoftwareCursorOverlay.cs:8`

Created once via `EnsureCreated()` with `DontDestroyOnLoad`. Persists across all tests. Not cleaned up by any TearDown. The `_instance` static field and the actual GameObject both persist. This is probably intentional (it's meant to survive scene loads) but means it accumulates across test runs.

### 3. InputSystem.onEvent handler lifecycle

**Location:** `UI/Input/CameraInputHandler.cs:385,392`

CameraInputHandler subscribes to `InputSystem.onEvent` in `OnEnable` and unsubscribes in `OnDisable`. The subscription captures `this` (the handler instance). If the handler is destroyed without `OnDisable` firing (e.g., via `DestroyImmediate` from outside), the delegate remains registered and will throw NullReferenceException on subsequent input events.

**In CameraInputHandlerTests:** The tests extend InputTestFixture, which sandboxes the Input System. TearDown destroys `_go` (triggering OnDisable → unsubscribe) before calling `base.TearDown()`. This sequence is correct.

**Risk:** If another test class creates a CameraInputHandler (e.g., via CameraBehaviour-adjacent testing) without InputTestFixture sandboxing, the handler would subscribe to the real InputSystem.onEvent and persist.

### 4. FindAnyObjectByType cross-contamination

Multiple Start() methods use `FindAnyObjectByType` to locate scene services:
- `CameraInputHandler.Start()` → PropertiesPanelView, ViewLayoutView, TimelineSectionView (lines 400-402)
- `SelectionInputHandler.Start()` → PropertiesPanelView, ViewLayoutView (lines 306-307)
- `AspectRatioMaskView.Start()` → CameraBehaviour, ViewCameraManager (lines 103-104)
- `CameraBehaviour.Awake()` → ShotEvaluator (line 249)

If test classes run concurrently or don't fully clean up, these `Find*` calls could discover objects from other tests.

### 5. FrustumWireframe as scene root

**Location:** `CameraBehaviour.cs:280-304`

`CreateFrustumWireframe()` creates `new GameObject("Shot Camera Frustum")` as a scene root (not parented to the CameraBehaviour's transform). Every test class that creates a CameraBehaviour (AspectRatioMaskViewTests, CameraInputHandlerTests) creates one of these. Both TearDown methods search for FrustumWireframe objects and destroy them.

**Risk:** If destruction order matters — e.g., if `_cameraGo` is destroyed first, the CameraBehaviour's `_frustumWireframe` reference becomes null, but the FrustumWireframe GameObject still exists. The TearDown sweep via `FindObjectsByType<FrustumWireframe>` should catch it.

### 6. GizmoRoot lifecycle

**Location:** `GizmoHandleFactory.cs:47-51`

GizmoRoot is created parented to the GizmoBehaviour's transform. When GizmoBehaviour is on `_go`, destroying `_go` destroys GizmoRoot as a child. CameraInputHandlerTests also explicitly searches for GizmoRoot via `GameObject.Find("GizmoRoot")` — but `GameObject.Find` cannot find inactive GameObjects, and GizmoRoot starts inactive (GizmoBehaviour.cs:250). The explicit Find is a no-op when GizmoRoot is inactive, but this is harmless because the child is destroyed with its parent.

### 7. SearchableDropdown._currentlyOpen (static)

**Location:** `UI/Panels/SearchableDropdown.cs:15`

Static field tracking which dropdown is open. If the HasFocusedTextField test opens a dropdown and the test fails before closing it, `_currentlyOpen` remains set, potentially blocking other tests that check dropdown state.

### 8. CursorTextures static texture cache

**Location:** `Engine/Cursor/CursorTextures.cs:17-20`

Static Texture2D fields loaded lazily from Resources. These persist across tests but are read-only after loading — no state leak risk, just memory.

---

## Risk Summary Table

| Risk | Severity | Affected Tests | Root Cause |
|------|----------|----------------|------------|
| CursorService._service replacement not restored on failure | **High** | CursorBehaviourTests → all others | Static singleton, restored only in TearDown success path |
| InputSystem devices not sandboxed | **High** | CursorBehaviourTests, SelectionInputHandlerTests | Don't extend InputTestFixture |
| Duplicate element leak in CtrlD test | **Medium** | SelectionInputHandlerTests | Cleanup on success path only, not tracked in field |
| panelGo leak in HasFocusedTextField test | **Medium** | CameraInputHandlerTests | Cleanup on success path only, not tracked in field |
| FrustumWireframe scene root orphan | **Medium** | AspectRatioMaskViewTests, CameraInputHandlerTests | Created as scene root, not parented |
| FindAnyObjectByType cross-contamination | **Medium** | All four | Start() methods discover objects from other tests |
| SoftwareCursorOverlay DontDestroyOnLoad | **Low** | All | Singleton persists indefinitely |
| Double Tick() invocation for toggle ops | **Low** | SelectionInputHandlerTests | Update() + explicit Tick() in same frame |
| SearchableDropdown._currentlyOpen | **Low** | CameraInputHandlerTests | Static field not reset in TearDown |
| StyleSheetLoader._cached | **None** | — | Read-only after load |

---

## Key References

- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:385-395` — InputSystem.onEvent subscribe/unsubscribe in OnEnable/OnDisable
- `Unity/Fram3d/Assets/Scripts/Engine/Cursor/CursorService.cs:5` — static `_service` field
- `Unity/Fram3d/Assets/Scripts/Engine/Cursor/UnityCursorService.cs:19-24` — RuntimeInitializeOnLoadMethod sets the service
- `Unity/Fram3d/Assets/Scripts/Engine/Cursor/SoftwareCursorOverlay.cs:8,29` — static `_instance` + DontDestroyOnLoad
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs:280-304` — FrustumWireframe created as scene root
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoBehaviour.cs:241-251` — GizmoRoot created parented to transform
- `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraDatabaseLoader.cs:30` — Debug.Log on every load
- `Unity/Fram3d/Assets/Tests/PlayMode/UI/CursorBehaviourTests.cs:85-108` — SetUp replaces CursorService
- `Unity/Fram3d/Assets/Tests/PlayMode/UI/SelectionInputHandlerTests.cs:205-239` — CtrlD test with success-path-only cleanup
- `Unity/Fram3d/Assets/Tests/PlayMode/UI/CameraInputHandlerTests.cs:382-452` — HasFocusedTextField with mid-test panelGo
