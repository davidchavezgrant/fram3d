# Research: Cursor Management Code Path

## Summary

Cursor management in Fram3d flows through a single method — `SelectionInputHandler.UpdateCursor()` — which decides whether to show the pointer (link) cursor based on two hover sources: element hover via `Selection.HoveredId` and gizmo handle hover via `GizmoController.IsHoveringHandle`. The cursor is set/reset via the `NativeCursor` static facade, which delegates to a platform-specific `ICursorService`. A 100ms grace period prevents cursor flickering when the mouse passes between interactive regions. Existing tests cover selection and click behavior but **zero cursor tests exist today**.

## File Map

### Cursor decision logic
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/UI/Input/SelectionInputHandler.cs` — the only consumer of `NativeCursor.SetCursor`/`ResetCursor` in application code

### NativeCursor infrastructure
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/NativeCursor/Scripts/Core/NativeCursor.cs` — static facade
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/NativeCursor/Scripts/Core/ICursorService.cs` — interface + `NTCursors` enum
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/NativeCursor/Scripts/Editor/EditorCursorService.cs` — editor service (active during Play Mode tests)
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/NativeCursor/Scripts/Native/MacOS/MacOSCursorService.cs` — standalone macOS service

### Hover sources
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Scene/Selection.cs` — `HoveredId` property
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Engine/Integration/GizmoController.cs` — `IsHoveringHandle`, `UpdateHover()`, `ActiveTool`
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Scene/ActiveTool.cs` — sealed class enum (SELECT, TRANSLATE, ROTATE, SCALE)

### Existing tests
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Tests/PlayMode/UI/SelectionInputHandlerTests.cs` — 6 tests, none cover cursor
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Tests/PlayMode/Engine/GizmoControllerTests.cs` — 10 tests, none cover cursor or `IsHoveringHandle`

## How It Works

### The `UpdateCursor()` decision tree (SelectionInputHandler.cs:46-77)

Called once per frame from `Update()` (line 207), after both `UpdateHover()` and `UpdateGizmoHover()` have run.

```
overElement = Selection.HoveredId != null
overGizmo   = gizmoController != null
           && gizmoController.ActiveTool != ActiveTool.SELECT
           && gizmoController.IsHoveringHandle

hasInteractiveHover = overElement || overGizmo

if hasInteractiveHover:
    record _lastInteractiveHoverTime = Time.unscaledTime

withinResetGrace = _cursorIsPointer
                && (now - _lastInteractiveHoverTime) <= 0.1s

wantPointer = hasInteractiveHover || withinResetGrace

if wantPointer == _cursorIsPointer:
    return (no change)

if wantPointer:
    NativeCursor.SetCursor(NTCursors.Link)
    _cursorIsPointer = true
else:
    NativeCursor.ResetCursor()
    _cursorIsPointer = false
```

### Key behaviors:

1. **Element hover triggers pointer cursor.** When `Selection.HoveredId` is non-null (mouse is over a scene element), cursor becomes `Link` (pointing hand).

2. **Gizmo handle hover triggers pointer cursor** — but only when `ActiveTool != SELECT`. The `IsHoveringHandle` property (GizmoController.cs:84) is `this._hoveredRenderer != null`, set by `UpdateHover()` which raycasts on layer 6 (GIZMO_LAYER_INDEX).

3. **Grace period prevents flicker.** The 100ms `CURSOR_RESET_GRACE_SECONDS` constant (line 17) keeps the pointer cursor active briefly after hovering ends. The `withinResetGrace` condition (line 61) is only true when `_cursorIsPointer` is already true AND we're within the grace window. This prevents cursor bounce when moving between adjacent interactive regions.

4. **No-change early exit.** If `wantPointer` already matches `_cursorIsPointer`, the method returns immediately (line 64-67). This avoids redundant `NativeCursor` calls.

5. **OnDisable resets.** When the component is disabled (line 211-214), `ResetPointerCursor()` is called to restore the default cursor.

### Update order within a frame (SelectionInputHandler.Update, lines 181-209)

```
1. Guard: bail if _selection == null || raycaster == null || Mouse.current == null
2. If _isGizmoDragging: UpdateGizmoDrag() then return (skip all hover/cursor/selection)
3. UpdateHover(mousePosition)       — raycasts for elements, sets Selection.HoveredId
4. UpdateGizmoHover(mousePosition)  — raycasts layer 6 for gizmo handles, sets _hoveredRenderer
5. UpdateCursor()                   — reads both hover states, decides cursor
6. UpdateSelection(mouse, kb, pos)  — handles click/drag/select/deselect
```

### GizmoController.UpdateHover (GizmoController.cs:225-253)

- If `ActiveTool == SELECT` or gizmo root is inactive: clears hover highlight and returns.
- Raycasts on layer 6 (gizmo handles only).
- On hit: stores the hit renderer as `_hoveredRenderer` (making `IsHoveringHandle` return true) and applies yellow highlight color.
- On miss: clears `_hoveredRenderer` to null.

### NativeCursor static facade (NativeCursor.cs)

- `SetCursor(NTCursors)` — delegates to `_instance.SetCursor()`. Returns false if no service registered.
- `ResetCursor()` — delegates to `_instance?.ResetCursor()`.
- `SetService(ICursorService)` — replaces the active service; resets previous, sets new to Default.
- `_instance` is static with no thread safety. In tests, it retains whatever service was set during editor initialization.

### ICursorService (ICursorService.cs)

Simple interface:
- `bool SetCursor(NTCursors ntCursor)`
- `void ResetCursor()`

### Test environment cursor service

In the Unity Editor (where Play Mode tests run), `EditorCursorService` auto-registers via `[RuntimeInitializeOnLoadMethod]` (EditorCursorService.cs:118-127). This means `NativeCursor.SetCursor()` and `NativeCursor.ResetCursor()` will actually execute during tests, calling into macOS P/Invoke on macOS editors. Tests that want to verify cursor behavior without side effects need to either:
- Inject a mock `ICursorService` via `NativeCursor.SetService()` before the test and restore the original after
- Or test the decision logic indirectly by verifying the `_cursorIsPointer` private field

## Existing Patterns

### Test infrastructure pattern (from SelectionInputHandlerTests.cs)

**SetUp:**
```csharp
// Create camera + components on single GameObject
this._cameraGo = new GameObject("TestCamera");
var camera = this._cameraGo.AddComponent<Camera>();

// Wire SerializeField references via reflection
var raycaster = this._cameraGo.AddComponent<SelectionRaycaster>();
SetField(raycaster, "targetCamera", camera);
this._highlighter = this._cameraGo.AddComponent<SelectionHighlighter>();
this._handler = this._cameraGo.AddComponent<SelectionInputHandler>();
SetField(this._handler, "selectionHighlighter", this._highlighter);
SetField(this._handler, "raycaster", raycaster);

// Create scene element
this._cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
this._cube.transform.position = new Vector3(0f, 0f, 5f);
this._cube.AddComponent<ElementBehaviour>();

// Add input devices
this._keyboard = InputSystem.AddDevice<Keyboard>();
this._mouse = InputSystem.AddDevice<Mouse>();
```

**TearDown:**
```csharp
// Reset input state BEFORE removing devices
InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
InputSystem.QueueStateEvent(this._mouse, new MouseState());
InputSystem.RemoveDevice(this._keyboard);
InputSystem.RemoveDevice(this._mouse);
Object.DestroyImmediate(this._cube);
Object.DestroyImmediate(this._cameraGo);
```

**SetField helper (reflection for SerializeField):**
```csharp
private static void SetField(object target, string fieldName, object value)
{
    var field = target.GetType().GetField(fieldName,
        BindingFlags.NonPublic | BindingFlags.Instance);
    field.SetValue(target, value);
}
```

**Input simulation (3-frame click lifecycle):**
```csharp
// Frame 1: press
InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center, buttons = 1 });
yield return null;
// Frame 2: held
yield return null;
// Frame 3: release
InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center, buttons = 0 });
yield return null;
```

### GizmoController test pattern (from GizmoControllerTests.cs)

- Uses `List<GameObject> _extras` for tracking dynamically created objects
- `CreateExtra()` helper adds to the list and returns the new GO
- TearDown iterates `_extras` with null check before `DestroyImmediate`
- GizmoController requires both `selectionHighlighter` and `targetCamera` fields wired
- Tool lifecycle: select element + yield (LateUpdate resets to TRANSLATE), THEN set desired tool, THEN call method under test without yielding

## Testable Behaviors in the Cursor Code Path

### 1. Element hover shows pointer cursor
- **Condition:** `Selection.HoveredId != null`
- **Expected:** `NativeCursor.SetCursor(NTCursors.Link)` called, `_cursorIsPointer` becomes true
- **Test approach:** Set up scene with element, move mouse over it, verify cursor state after Update

### 2. Gizmo handle hover shows pointer cursor (when tool != SELECT)
- **Condition:** `gizmoController.ActiveTool != SELECT` AND `gizmoController.IsHoveringHandle`
- **Expected:** pointer cursor activated
- **Test approach:** Select element, set tool to TRANSLATE, position mouse over gizmo handle

### 3. Gizmo handle hover does NOT show pointer when tool == SELECT
- **Condition:** `ActiveTool == SELECT`
- **Expected:** cursor stays default even if handle geometry is under mouse
- **Note:** Actually `UpdateHover` in GizmoController already clears `_hoveredRenderer` when tool is SELECT (line 227-229), so `IsHoveringHandle` would be false. Double-guarded by SelectionInputHandler.cs:51.

### 4. Cursor resets when leaving interactive region
- **Condition:** was hovering (pointer active), then hover clears
- **Expected:** after grace period expires, `NativeCursor.ResetCursor()` called

### 5. Grace period prevents flicker
- **Condition:** `_cursorIsPointer` is true, hover clears, but within 100ms
- **Expected:** cursor stays as pointer during grace window

### 6. Grace period expires and cursor resets
- **Condition:** `_cursorIsPointer` is true, hover clears, and 100ms+ passes
- **Expected:** cursor resets to default

### 7. No redundant cursor calls when state unchanged
- **Condition:** already hovering element (pointer active), next frame still hovering
- **Expected:** no `SetCursor` or `ResetCursor` call (early return at line 64-67)

### 8. OnDisable resets cursor
- **Condition:** component disabled while pointer cursor is active
- **Expected:** `NativeCursor.ResetCursor()` called, `_cursorIsPointer` reset to false

### 9. During gizmo drag, cursor logic is skipped entirely
- **Condition:** `_isGizmoDragging` is true
- **Expected:** `UpdateCursor()` never called (Update returns early at line 200-203)

### 10. Null gizmoController is safe
- **Condition:** `gizmoController` field is null
- **Expected:** `overGizmo` evaluates to false (null check at line 50), no NRE

## Testing Strategy

### Mock ICursorService approach

The cleanest way to test cursor behavior is to inject a recording `ICursorService` via `NativeCursor.SetService()`:

```csharp
private sealed class RecordingCursorService : ICursorService
{
    public NTCursors? LastSetCursor { get; private set; }
    public int SetCursorCallCount { get; private set; }
    public int ResetCallCount { get; private set; }

    public bool SetCursor(NTCursors ntCursor)
    {
        this.LastSetCursor = ntCursor;
        this.SetCursorCallCount++;
        return true;
    }

    public void ResetCursor()
    {
        this.LastSetCursor = null;
        this.ResetCallCount++;
    }

    public void Reset()
    {
        this.LastSetCursor = null;
        this.SetCursorCallCount = 0;
        this.ResetCallCount = 0;
    }
}
```

**SetUp** injects the mock, **TearDown** restores the original service. Since `NativeCursor._instance` is static, save and restore it via `NativeCursor.SetService()`.

### Reading private state

For assertions on `_cursorIsPointer`, use the same reflection pattern the existing tests use:

```csharp
private static T GetField<T>(object target, string fieldName)
{
    var field = target.GetType().GetField(fieldName,
        BindingFlags.NonPublic | BindingFlags.Instance);
    return (T)field.GetValue(target);
}
```

However, per CLAUDE.md project guidelines: "Don't test implementation details. Avoid reflection to read private fields for assertions." The preferred approach is to observe behavior through the mock `ICursorService`, which records `SetCursor`/`ResetCursor` calls.

### What's already tested (no cursor coverage)

The existing `SelectionInputHandlerTests` (6 tests) cover:
- Click-to-select
- Click-to-deselect on empty space
- Alt+click modifier guard
- Cmd+click modifier guard
- Small mouse movement still selects (under threshold)
- Drag past threshold does not deselect

The existing `GizmoControllerTests` (10 tests) cover:
- Tool switching
- Show/hide on selection
- Tool reset on new selection
- TryResetActiveTool for all tools
- TryBeginDrag guard conditions (SELECT tool, nothing selected)

**None of the 16 existing tests verify any cursor behavior.**

## Key References

- `SelectionInputHandler.cs:16-17` — `CLICK_THRESHOLD` (5px) and `CURSOR_RESET_GRACE_SECONDS` (0.1s) constants
- `SelectionInputHandler.cs:18` — `_cursorIsPointer` boolean tracking cursor state
- `SelectionInputHandler.cs:46-77` — `UpdateCursor()` full decision logic
- `SelectionInputHandler.cs:211-214` — `OnDisable()` cursor cleanup
- `SelectionInputHandler.cs:199-203` — gizmo drag skips cursor logic
- `GizmoController.cs:82` — `ActiveTool` property (defaults to TRANSLATE)
- `GizmoController.cs:84` — `IsHoveringHandle` computed property
- `GizmoController.cs:225-253` — `UpdateHover()` raycast on layer 6
- `NativeCursor.cs:15-23` — `SetService()` allows injecting mock services
- `NativeCursor.cs:25-28` — `SetCursor()` delegates to instance
- `NativeCursor.cs:30-33` — `ResetCursor()` delegates to instance
- `ICursorService.cs:22-27` — interface definition
- `Selection.cs:7` — `HoveredId` property

## Open Questions

1. **Grace period timing in tests.** Testing the 100ms grace period requires `Time.unscaledTime` to advance. In Play Mode tests, each `yield return null` advances by one frame (~16ms at 60fps). Roughly 7 frames of yielding would be needed to exceed the grace period. An alternative is to test that the grace period IS active (within the window) vs. that it expires, avoiding timing sensitivity.

2. **NativeCursor static state leaks between tests.** Since `NativeCursor._instance` is static, tests that inject a mock service must carefully restore the original in TearDown. If a test fails mid-run, TearDown still executes, but the order matters. The `EditorCursorService` registered via `[RuntimeInitializeOnLoadMethod]` is the "natural" state to restore to — but getting a reference to it requires either saving it before replacing, or using `NativeCursor.SetService()` with `null` (which would leave no service).

3. **GizmoController hover in tests.** Testing gizmo handle hover (behavior 2) requires Physics.Raycast to hit gizmo handle colliders on layer 6. This requires the gizmo to be visible (element selected), properly positioned, and the camera to point at it. The gizmo builds its mesh handles in `Awake()` using `GizmoMeshBuilder` and `Shader.Find("Fram3d/GizmoHandle")`. If the shader isn't available in the test environment, the gizmo handles may not render or collide properly.
