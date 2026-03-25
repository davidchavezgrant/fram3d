# FRA-51: Director View — Implementation Plan

**Date**: 2026-03-25
**Ticket**: FRA-51
**Spec**: `docs/specs/milestone-2.1-scene-management-spec.md` §2.1.5

---

## Architecture

Single Unity Camera, swap which `CameraElement` drives it. When Director View is active, `CameraBehaviour.Sync()` reads from a director `CameraElement` instead of the shot `CameraElement`. `CameraInputHandler._camera` swaps to target the active camera. The shot camera's frustum wireframe appears as a selectable element.

Director camera is a full `CameraElement` — gets all movement methods for free. DOF and shake are suppressed in Director View (utility view, not a shot).

**Deferred (needs Phase 3 — Timeline):**
- Keyframe creation from element/camera manipulation
- Frustum wireframe animation during playback
- Shot camera keyframe updates via gizmo drag

**Key shortcut change:**
- D (no modifiers) → toggle Director View (was: DOF toggle)
- Shift+D → DOF toggle (moved from bare D)

---

## Phase 1: Core — ViewMode

**File:** `Unity/Fram3d/Assets/Scripts/Core/Scene/ViewMode.cs`

Sealed class with private constructor (project convention for closed value sets):
- `CAMERA` — looking through the shot camera
- `DIRECTOR` — free utility camera

No tests needed — trivial sealed class.

---

## Phase 2: Engine — View Switching

### CameraBehaviour changes
**File:** `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs`

- Add `_directorCamera` field (CameraElement, created in Awake)
- Add `_viewMode` field (ViewMode, defaults to CAMERA)
- Add `ActiveCamera` property → returns director or shot camera based on mode
- Add `IsDirectorView` property
- Add `ToggleDirectorView()` method:
  - If switching TO Director: copy shot camera's current position/rotation to director camera (first entry); preserve director position on subsequent entries
  - If switching FROM Director: no position copy (director position preserved)
  - Toggle frustum wireframe visibility
- Modify `Sync()`:
  - Read from `ActiveCamera` instead of `_cameraElement`
  - Skip DOF sync when in Director View (force DOF off)
  - Skip shake when in Director View
  - Always sync focal length, sensor, viewport (zoom still useful)

### ElementBehaviour change
**File:** `Unity/Fram3d/Assets/Scripts/Engine/Integration/ElementBehaviour.cs`

- Change `Element` property setter from `private set` to `internal set`
- Modify `Awake()` to skip auto-creation if Element is already set

This lets the frustum wireframe's ElementBehaviour wrap the shot CameraElement directly. When the user selects and drags the frustum, the gizmo writes to the shot CameraElement's Position/Rotation.

---

## Phase 3: Engine — Frustum Wireframe

**File:** `Unity/Fram3d/Assets/Scripts/Engine/Integration/FrustumWireframe.cs`

MonoBehaviour that creates and updates a wireframe mesh showing the shot camera's frustum.

- Created by CameraBehaviour when Director View is first activated
- Parented to a root GO with ElementBehaviour (Element = shot CameraElement)
- Mesh: `MeshTopology.Lines` — 12 edges of a truncated pyramid (4 near, 4 far, 4 connecting)
- Material: unlit, wireframe color (white or light grey), no ZTest override
- Collider: single BoxCollider covering the frustum volume for selection
- Updated each `LateUpdate` from CameraElement's VerticalFov, SensorWidth/Height, plus fixed near (0.3m) and far (3m) distances
- Show/hide via `SetActive()` based on view mode

The frustum GO's ElementBehaviour.Element IS the shot CameraElement. So:
- SelectionRaycaster finds it → selection works
- GizmoController attaches gizmo → drag writes to CameraElement.Position/Rotation
- ElementBehaviour.LateUpdate syncs GO transform from CameraElement → frustum follows shot camera

---

## Phase 4: UI — Keyboard Shortcuts

**File:** `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`

- Move DOF toggle from `D` to `Shift+D` in `HandleToggles()`
- Add `HandleViewToggle()` in `HandleKeyboardInput()`:
  - D (no modifiers) → `cameraBehaviour.ToggleDirectorView()`
  - After toggle: `this._camera = this.cameraBehaviour.ActiveCamera`

**File:** `docs/reference/interaction-patterns.md`
- Update keyboard shortcuts section

---

## Phase 5: UI — Director View Badge

**File:** `Unity/Fram3d/Assets/Scripts/UI/Views/DirectorViewBadge.cs`

MonoBehaviour + UIDocument overlay (same pattern as CompositionGuideView):
- "DIRECTOR VIEW" label, top-center, red/pink background
- Show/hide based on `CameraBehaviour.IsDirectorView`
- Absolute positioning, `PickingMode.Ignore`
- Styled in `Resources/fram3d.uss`

---

## Phase 6: Tests

### Core (xUnit)
- ViewMode sealed class basic checks (if warranted)

### Play Mode
**ElementDuplicator** — already covered

**CameraBehaviour / Director View:**
- `ToggleDirectorView__SwitchesToDirector__When__InCameraView`
- `ToggleDirectorView__SwitchesBackToCamera__When__InDirectorView`
- `ToggleDirectorView__PreservesDirectorPosition__When__SwitchingBackAndForth`
- `ActiveCamera__ReturnsShotCamera__When__InCameraView`
- `ActiveCamera__ReturnsDirectorCamera__When__InDirectorView`
- `DirectorCamera__InitializesAtShotCameraPosition__When__FirstToggle`

**CameraInputHandler:**
- `DKey__TogglesDirectorView__When__Pressed` (update existing DOF test)
- `ShiftD__TogglesDof__When__Pressed` (new)

**FrustumWireframe:**
- `FrustumWireframe__IsVisible__When__DirectorViewActive`
- `FrustumWireframe__IsHidden__When__CameraViewActive`
- `FrustumWireframe__IsSelectable__When__Clicked`
