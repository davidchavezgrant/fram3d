# FRA-48: Transform Gizmos Implementation Plan

> **For Claude:** Implement this plan task-by-task. Complete each task fully before moving to the next. Pause at phase boundaries for manual verification.

**Goal:** Custom runtime translate/rotate/scale gizmos that show on selected elements, with Q/W/E/R tool switching and an active tool badge overlay.

**Spec:** `docs/specs/milestone-2.1-scene-management-spec.md` §2.1.2

**Architecture:** Gizmo meshes are real GameObjects on a dedicated "Gizmo" layer, rendered by the main camera with a `ZTest Always` unlit shader. Handle detection uses `Physics.Raycast` on the Gizmo layer only. During drag, the gizmo writes directly to `Element.Position/Rotation/Scale` (no command dispatch, no evaluation trigger). `GizmoController` lives in `Engine.Integration`.

**Tech Stack:** Unity 6, URP, C# 9, UI Toolkit (programmatic, no UXML/USS)

---

## Current State Analysis

### Key Discoveries
- `Element` base class has `Position` (System.Numerics.Vector3), `Rotation` (System.Numerics.Quaternion), `Scale` (float) — `Unity/Fram3d/Assets/Scripts/Core/Common/Element.cs:14-17`
- `SelectionRaycaster` already excludes a "Gizmo" layer from element selection — `Unity/Fram3d/Assets/Scripts/Engine/Integration/SelectionRaycaster.cs:56-59`
- `SelectionHighlighter` owns the `Selection` instance; `SelectionInputHandler` reads it via serialized ref — established wiring pattern
- `CameraInputHandler` owns all modifier-key camera operations (Alt+drag=orbit, Cmd+drag=pan/tilt, middle=pan/tilt) — `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:74-95`
- `AspectRatio` uses sealed-class-with-private-constructor pattern for closed type sets — `Unity/Fram3d/Assets/Scripts/Core/Camera/AspectRatio.cs:9-28`
- Coordinate conversion: `ToUnity()` negates Z for positions, negates X/Y for quaternions — `Unity/Fram3d/Assets/Scripts/Engine/Conversion/VectorExtensions.cs`
- decisions.md specifies gizmo drag flow: direct writes during drag, command on release — `docs/reference/decisions.md:49`
- `GizmoController` is explicitly named in Engine.Integration namespace structure — `docs/reference/unity-naming-conventions.md:96`

### Alignment with Future Goals
- Multi-select (8.1): `GizmoController` receives a single element now; pivot computation is the extension point for multi-select later
- Grid snapping (8.1.2): snap layer inserts between raw drag delta and Element write — clean insertion point
- Lighting (5.1): lights use "the same gizmo system as other elements" — no special handling needed since `LightElement : Element`
- Undo (4.1): command infrastructure deferred; drag flow writes directly for now, commands added later

## What We're NOT Doing

- Command/undo infrastructure (milestone 4.1)
- Grid snapping (milestone 8.1.2)
- Multi-select gizmo pivot (milestone 8.1)
- Custom interpolation curves for rotation (milestone 8.1.3)
- Rotation gizmo screen-space interaction (world-space ring drag is sufficient for v1)

---

## Phase 1: Core Domain — ActiveTool

### Overview
Create the `ActiveTool` type in `Core.Scene` — a sealed class with static instances (Select, Translate, Rotate, Scale), following the `AspectRatio` pattern.

### Task 1.1: ActiveTool Type

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Scene/ActiveTool.cs`
- Test: `tests/Fram3d.Core.Tests/Scene/ActiveToolTests.cs`

**Implementation:**

`ActiveTool` — sealed class with private constructor. Each instance carries:
- `Name` (string) — "Select", "Translate", "Rotate", "Scale"
- `Shortcut` (char) — 'Q', 'W', 'E', 'R'

Static instances: `SELECT`, `TRANSLATE`, `ROTATE`, `SCALE`. Default is `SELECT`.

**Tests:**
- Each static instance has correct name and shortcut
- Equality works by reference (same sealed instances)
- All four instances are distinct

**Step 5: Commit**
```bash
git commit -m "add ActiveTool sealed class with Select/Translate/Rotate/Scale instances"
```

### Phase 1 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` passes

---

## Phase 2: Engine — Gizmo Rendering Infrastructure

### Overview
Create the "Gizmo" layer, the `ZTest Always` unlit shader, and the mesh-building utilities for arrow handles, rotation rings, and the scale cube.

### Task 2.1: ZTest Always Shader

**Files:**
- Create: `Unity/Fram3d/Assets/Shaders/GizmoHandle.shader`

**Implementation:**

Minimal URP-compatible unlit shader:
- `ZTest Always` — renders on top of everything
- `ZWrite Off` — doesn't interfere with scene depth
- Single `_Color` property for per-handle axis coloring
- No lighting, no shadows — pure flat color
- Queue = `Overlay+1`

### Task 2.2: Gizmo Mesh Builder

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoMeshBuilder.cs`

**Implementation:**

Static utility class that creates meshes procedurally at runtime:
- `CreateArrow()` → Mesh — cylinder shaft + cone tip, oriented along +Y. Used for translate handles (rotated to X/Y/Z axes). Include a `CapsuleCollider` or `MeshCollider` for raycasting.
- `CreateRing()` → Mesh — torus around Y axis. Used for rotate handles (rotated to X/Y/Z axes).
- `CreateCube()` → Mesh — small cube. Used for uniform scale handle.

Each mesh is simple (low poly count — gizmos are visual tools, not models). Arrows: ~12-segment cylinder + cone. Rings: ~32-segment torus. Cube: Unity primitive.

### Task 2.3: Gizmo Layer Setup

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs`

**Implementation:**

Add a `SetupGizmoLayer()` call that ensures the "Gizmo" layer exists (Unity layers are project-level, set via `TagManager`). If adding layers programmatically is fragile, document that the layer must be added manually via Edit > Project Settings > Tags and Layers.

**Step 5: Commit**
```bash
git commit -m "add gizmo rendering infrastructure: ZTest Always shader, mesh builder, Gizmo layer"
```

### Phase 2 Verification

#### Automated
- [ ] Project compiles in Unity

#### Manual
- [ ] Gizmo layer visible in Edit > Project Settings > Tags and Layers

---

## Phase 3: Engine — GizmoController (Display)

### Overview
`GizmoController` MonoBehaviour that watches the `Selection` state and shows/hides gizmo handles at the selected element's position. Handles constant screen-size scaling. No interaction yet — display only.

### Task 3.1: GizmoController — Show/Hide and Tool Switching

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs`

**Implementation:**

MonoBehaviour on the Main Camera GameObject. Serialized refs:
- `SelectionHighlighter selectionHighlighter` — to read `Selection.SelectedId`
- `Camera targetCamera` — for screen-size scaling

State:
- `ActiveTool _activeTool = ActiveTool.SELECT`
- `GameObject _gizmoRoot` — parent of all handle GameObjects, created in `Awake`
- `GameObject _translateGroup`, `_rotateGroup`, `_scaleGroup` — one per tool, children of `_gizmoRoot`

Behavior in `LateUpdate`:
1. Read `Selection.SelectedId` → find `ElementBehaviour` (same pattern as `SelectionHighlighter.FindBehaviour`)
2. If null → hide `_gizmoRoot`, return
3. Position `_gizmoRoot` at the selected element's Unity world position
4. Show the active tool's group, hide the others
5. Scale `_gizmoRoot` for constant screen size: `scale = distance_to_camera * scaleFactor`

Each handle child has:
- `MeshFilter` + `MeshRenderer` with the `GizmoHandle` material (axis color set per-handle)
- A `Collider` for raycast detection (capsule for arrows, mesh for rings, box for cube)
- All on the "Gizmo" layer

Public API:
- `ActiveTool ActiveTool { get; }` — read by the badge overlay
- `void SetActiveTool(ActiveTool tool)` — called by input handler on Q/W/E/R

### Task 3.2: Handle Construction Helpers

**Files:**
- Part of `GizmoController.cs`

**Implementation:**

Private methods that build the three tool groups in `Awake`:

`BuildTranslateHandles()`:
- 3 arrows (X=red, Y=green, Z=blue), rotated to point along their respective axes
- Each arrow is a child GameObject named "TranslateX", "TranslateY", "TranslateZ"

`BuildRotateHandles()`:
- 3 rings (X=red, Y=green, Z=blue), rotated to encircle their respective axes
- Each ring named "RotateX", "RotateY", "RotateZ"

`BuildScaleHandle()`:
- 1 cube (white/grey), centered at origin
- Named "ScaleUniform"

### Task 3.3: Wire GizmoController in SceneBootstrap

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs`

**Implementation:**

Add `GizmoController` to the Main Camera in `SetupSelection()` (or a new `SetupGizmos()` method). Wire serialized refs via `SerializedObject`.

**Step 5: Commit**
```bash
git commit -m "FRA-48: add GizmoController — display gizmo at selected element with constant screen size"
```

### Phase 3 Verification

#### Automated
- [ ] Project compiles in Unity

#### Manual
- [ ] Select an element → translate arrows appear at its position
- [ ] Deselect → arrows disappear
- [ ] Move camera closer/farther → gizmo stays same screen size
- [ ] Gizmo renders on top of scene geometry (ZTest Always)

> **Pause here.** Confirm gizmo display works before adding interaction.

---

## Phase 4: Engine — Drag Interaction

### Overview
Handle detection via Gizmo-layer raycast. Axis-constrained dragging for translate, rotate, and scale. Direct writes to `Element.Position/Rotation/Scale` during drag — no commands.

### Task 4.1: Handle Detection

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs`

**Implementation:**

Add to `GizmoController`:

`GizmoHandle RaycastHandle(Vector2 screenPos)`:
- `Physics.Raycast` with layer mask = Gizmo layer only
- Hit collider → walk up to find handle identity (which axis, which tool)
- Return a `GizmoHandle` value identifying the handle, or null

`GizmoHandle` — small struct or class identifying what was hit:
- `Axis` (X, Y, Z, or Uniform for scale)
- `Tool` (Translate, Rotate, Scale)

### Task 4.2: Translate Drag

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs`

**Implementation:**

When a translate handle is grabbed:
1. Record `_dragStartElementPosition` (System.Numerics.Vector3)
2. Compute the drag plane: the axis direction + camera's view direction define a plane through the element's position
3. Each frame during drag: raycast from mouse onto the drag plane, project onto the constrained axis, compute delta from drag start
4. Write `element.Position = _dragStartElementPosition + axisDelta`
5. The `ElementBehaviour` sync path (or `CameraBehaviour.Sync` pattern) propagates to Unity Transform

Key method: `Vector3 ProjectMouseOntoAxis(Vector2 screenPos, Vector3 axisWorld, Vector3 origin)`:
- Cast ray from camera through screen position
- Find closest point between the ray and the axis line
- Return the projected point on the axis

### Task 4.3: Rotate Drag

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs`

**Implementation:**

When a rotate handle is grabbed:
1. Record `_dragStartRotation` (System.Numerics.Quaternion) and `_dragStartMouseAngle`
2. The rotation plane is perpendicular to the axis (e.g., Y rotation → XZ plane)
3. Each frame: project mouse position onto the rotation plane, compute angle delta from drag start
4. Write `element.Rotation = Quaternion.CreateFromAxisAngle(axis, angleDelta) * _dragStartRotation`

### Task 4.4: Scale Drag

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs`

**Implementation:**

When the scale handle is grabbed:
1. Record `_dragStartScale` (float) and `_dragStartMouseY` (screen Y)
2. Each frame: compute scale factor from vertical mouse delta
3. Write `element.Scale = Math.Max(MIN_SCALE, _dragStartScale * scaleFactor)`
4. `MIN_SCALE = 0.01f` — elements cannot scale to zero or negative

### Task 4.5: ElementBehaviour Sync

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/ElementBehaviour.cs`

**Implementation:**

Currently `ElementBehaviour` only creates the `Element` in `Awake`. It needs a sync path to propagate Core transform state to the Unity Transform each frame (like `CameraBehaviour.Sync`):

Add `LateUpdate`:
```csharp
private void LateUpdate()
{
    this.transform.position   = this.Element.Position.ToUnity();
    this.transform.rotation   = this.Element.Rotation.ToUnity();
    this.transform.localScale = UnityEngine.Vector3.one * this.Element.Scale;
}
```

This ensures gizmo drag writes to `Element.Position` are reflected in the Unity scene immediately.

**Step 5: Commit**
```bash
git commit -m "FRA-48: add gizmo drag interaction — translate, rotate, scale with axis constraints"
```

### Phase 4 Verification

#### Automated
- [ ] Project compiles in Unity

#### Manual
- [ ] Drag X arrow → element moves only along X
- [ ] Drag Y arrow → element moves only along Y
- [ ] Drag Z arrow → element moves only along Z
- [ ] Gizmo follows the element during drag (no lag)
- [ ] Switch to rotate (E key) → drag Y ring → element rotates around Y
- [ ] Switch to scale (R key) → drag handle → element scales uniformly
- [ ] Scale cannot go below minimum (element stays visible)
- [ ] Releasing mouse completes the operation — element stays at new position
- [ ] Camera controls (Alt+drag orbit, Cmd+drag pan/tilt, scroll) still work while gizmo is visible

> **Pause here.** Confirm all three gizmo tools work before adding input integration.

---

## Phase 5: UI — Input Integration

### Overview
Q/W/E/R tool switching, gizmo drag priority over selection clicks, modifier key guards. Integrate `GizmoController` with `SelectionInputHandler` so gizmo handles take precedence over element selection.

### Task 5.1: Tool Switching in CameraInputHandler

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`

**Implementation:**

Add Q/W/E/R key handling in `HandleKeyboardInput`:
- Q → `gizmoController.SetActiveTool(ActiveTool.SELECT)`
- W → `gizmoController.SetActiveTool(ActiveTool.TRANSLATE)`
- E → `gizmoController.SetActiveTool(ActiveTool.ROTATE)`
- R → `gizmoController.SetActiveTool(ActiveTool.SCALE)`

Add `[SerializeField] private GizmoController gizmoController` and wire in SceneBootstrap.

### Task 5.2: Gizmo Drag Priority over Selection

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Input/SelectionInputHandler.cs`

**Implementation:**

Before evaluating selection on mouse-down, check if a gizmo handle is under the cursor. If so, start a gizmo drag instead of a selection operation.

Add `[SerializeField] private GizmoController gizmoController`.

In `UpdateSelection`, on `wasPressedThisFrame`:
1. If modifier held → skip (camera operation)
2. If `gizmoController.RaycastHandle(mousePosition)` hits a handle → start gizmo drag, set `_mouseDownValid = false`
3. Otherwise → proceed with normal selection logic

The gizmo drag lifecycle:
- `mouseDown` on handle → `gizmoController.BeginDrag(handle, mousePosition)`
- Each frame while held → `gizmoController.UpdateDrag(mousePosition)`
- `mouseUp` → `gizmoController.EndDrag()`

### Task 5.3: Wire GizmoController References

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs`

**Implementation:**

Wire `gizmoController` serialized ref on `CameraInputHandler` and `SelectionInputHandler`.

**Step 5: Commit**
```bash
git commit -m "FRA-48: integrate gizmo input — Q/W/E/R switching, drag priority over selection"
```

### Phase 5 Verification

#### Manual
- [ ] Q/W/E/R switches the visible gizmo tool
- [ ] Clicking a gizmo handle starts a drag, not a selection change
- [ ] Clicking empty space (not on handle) still deselects
- [ ] Alt+drag still orbits the camera even when gizmo is visible
- [ ] Cmd+drag still pans/tilts
- [ ] Scroll still adjusts focal length

> **Pause here.** Confirm input integration is solid before adding the badge overlay.

---

## Phase 6: UI — Active Tool Badge

### Overview
Non-interactive UI Toolkit overlay in the bottom-left corner showing the current active tool name, icon character, and keyboard shortcut.

### Task 6.1: ActiveToolBadgeView

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/UI/Views/ActiveToolBadgeView.cs`

**Implementation:**

MonoBehaviour with UIDocument. Builds a small badge programmatically:
- Container: fixed position bottom-left, semi-transparent dark background, rounded corners
- Icon label: Unicode character or simple text (◇ Select, ✛ Translate, ↻ Rotate, ⬡ Scale)
- Tool name label: "SELECT", "TRANSLATE", "ROTATE", "SCALE"
- Shortcut label: dim text showing "Q", "W", "E", "R"

Reads `GizmoController.ActiveTool` each frame in `Update` and updates labels.

### Task 6.2: Wire Badge in SceneBootstrap

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs`

**Implementation:**

Create a "Tool Badge" UIDocument GameObject, add `ActiveToolBadgeView`, wire `GizmoController` reference.

### Task 6.3: Play Mode Tests

**Files:**
- Create: `Unity/Fram3d/Assets/Tests/PlayMode/Engine/GizmoControllerTests.cs`

**Implementation:**

Tests for `GizmoController` integration:
- `SetActiveTool__ChangesTool__When__Called`
- `LateUpdate__ShowsGizmo__When__ElementSelected`
- `LateUpdate__HidesGizmo__When__NothingSelected`
- `LateUpdate__PositionsAtElement__When__ElementSelected`
- `LateUpdate__ScalesForConstantScreenSize__When__CameraDistanceChanges`

**Step 5: Commit**
```bash
git commit -m "FRA-48: add active tool badge overlay and Play Mode tests"
```

### Phase 6 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` passes
- [ ] Play Mode tests pass in Unity Test Runner

#### Manual
- [ ] Badge visible in bottom-left corner
- [ ] Badge updates immediately on Q/W/E/R press
- [ ] Badge shows correct icon, name, and shortcut for each tool
- [ ] Full workflow: select element → translate with gizmo → switch to rotate → rotate → switch to scale → scale → deselect → gizmo disappears

---

## Testing Strategy

### Unit Tests (xUnit)
- `ActiveTool` instances have correct properties
- `ActiveTool` instances are distinct

### Play Mode Tests (NUnit)
- `GizmoController` shows/hides based on selection state
- `GizmoController` positions at selected element
- `GizmoController` tool switching
- `GizmoController` constant screen-size scaling
- `ElementBehaviour` sync propagates position/rotation/scale to Transform

### Manual Testing
1. Select cube → translate arrows appear
2. Drag X arrow → cube moves along X only
3. Press E → rotation rings replace arrows
4. Drag Y ring → cube rotates around Y
5. Press R → scale handle appears
6. Drag handle up → cube grows, drag down → shrinks to minimum
7. Press Q → Select tool, no gizmo handles
8. Click empty space → deselect, gizmo disappears
9. Alt+drag while gizmo visible → camera orbits (no gizmo interference)
10. Badge in bottom-left updates with each tool switch

## Performance Considerations

- Gizmo meshes are low-poly (~100 verts total across all handles)
- `FindObjectsByType<ElementBehaviour>` runs once per frame in LateUpdate — acceptable for previs scene element counts
- No allocations during drag (reuse mouse ray, axis projection vectors)

## References

- Spec: `docs/specs/milestone-2.1-scene-management-spec.md` §2.1.2
- Ticket: FRA-48
- Gizmo drag flow decision: `docs/reference/decisions.md:49`
- Naming convention: `docs/reference/unity-naming-conventions.md:96`
- Pattern to follow: `AspectRatio` sealed class — `Unity/Fram3d/Assets/Scripts/Core/Camera/AspectRatio.cs`
- Coordinate conversion: `Unity/Fram3d/Assets/Scripts/Engine/Conversion/VectorExtensions.cs`
