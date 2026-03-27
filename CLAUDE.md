# Fram3d — Project Instructions

Fram3d is a 3D previsualization tool for filmmakers. Unity project (Unity 6, URP, C#).

## Rules

- **Never overwrite or replace existing work.** When building new mockups, features, or files that relate to existing ones, incorporate or reference the existing work — don't start from scratch. If you need to create something new in the same space, create a separate file. Never nuke what we've already iterated on.
- **Use the domain language.** Read `docs/reference/domain-language.md` before writing specs, code, or UI text. Terms are chosen deliberately — don't invent synonyms.
- **All work requires a Linear ticket.** Before creating a new issue, **thoroughly search** for an existing one — search by title keywords, browse the relevant project/milestone, and check backlog. Only create a new issue if no match exists. Reference the ticket (e.g., FRA-36) in commits and PRs.
- **Never overwrite Linear issue content.** When updating an issue, append implementation notes below the existing description. Never replace the original spec text or user-written content.
- **No emojis in UI.** Never use emoji characters for buttons, labels, or indicators. Unicode symbols and special glyphs are fine.
- **Question scope decisions.** If asked to create a separate issue/PR for a bug found during feature work, push back — bugs found during implementation usually belong in the feature's PR. Only create separate issues for bugs that are genuinely independent or discovered after the feature is merged.

## Code Style (C#)

The `.editorconfig` at the project root is the source of truth for formatting. These rules cover what editorconfig can't express.

### Type patterns
- **Sealed class over enum for closed value sets.** Use a sealed class with a private constructor and `static readonly` instances (see `AspectRatio`, `ActiveTool`, `ShotTrackAction`). Each instance carries typed data (display name, shortcut key). No switch statements needed. The private constructor guarantees the set is closed at compile time. Never use C# `enum` for domain concepts.
- **`IObservable<T>` over delegates and events.** Use `Subject<T>` (Core.Common) for all event streams. Expose as `IObservable<T>` properties, never as `event` delegates. Subscribers use `source.Subscribe(action)` via `ObservableExtensions`. This keeps a single eventing pattern across the codebase — no mixing of `event Action<T>` with `IObservable<T>`.
- **One class per file.** Every public or internal class gets its own `.cs` file. No nesting multiple types in one file. File name matches class name.
- **Plural namespaces.** Namespace directories use plural names: `Cameras`, `Scenes`, `Shots`, `Timelines`, `Viewports`. This avoids namespace-class name collisions (e.g., `Timeline` class in `Timelines` namespace).
- **Push logic into Core.** UI and Engine layers should be thin. Domain logic, interaction state machines, computed properties, and formatting all belong in Core. Views forward pointer events and read state — they make zero decisions. If a method doesn't need `using UnityEngine`, it belongs in Core.
- **Small classes.** Target a few hundred lines maximum. Decompose large classes into private sub-components owned by the parent. Expose a unified API via delegation — consumers shouldn't know about internal decomposition.

### Language restrictions
- **C# 9 maximum.** Unity 6 supports C# 9. Do not use C# 10+ features (`record struct`, `global using`, file-scoped namespaces, `required`, etc.). Do not use `record` or `init` — Unity's runtime lacks `IsExternalInit` and polyfilling it is a hack. Use plain classes or structs instead.
- **No ternary expressions.** Use `if`/`else` instead.
- **Target-typed `new`.** Use `new()` instead of `new ClassName()` when the type is evident from context.

### Naming
- **Private fields:** `_camelCase` (underscore prefix). E.g., `private float _focalLength;`
- **`[SerializeField]` fields:** `camelCase` (no underscore). E.g., `[SerializeField] private CameraBehaviour cameraBehaviour;`
- **Private methods:** PascalCase. E.g., `private void ApplyRotation()`
- **Constants and static readonly:** `SCREAMING_SNAKE_CASE`.

### Key rules from editorconfig (for quick reference)
- **`this.` qualifier on all instance members** — fields, properties, events, methods.
- **`var` everywhere** — including built-in types.
- **4 spaces** for indentation, not tabs.
- **`sealed`** on classes unless designed for inheritance.
- **Block-scoped namespaces** (not file-scoped).
- **Expression bodies** for methods, properties, accessors (single-expression only).
- **No space before `:` in inheritance/constraints.**
- **Column-align** fields, properties, variables, assignments.
- **Braces always required on conditionals** (`if`, `else if`, `else`). No exceptions, even for single-line bodies. `using` blocks never require braces. `for`, `foreach`, `while`, `do-while`, `lock` require braces for multiline only.
- **No extra blank lines** — formatter strips them (keep_blank_lines = 0).
- **Early return** over else blocks — if an `if` branch returns/continues/breaks and the else would be the rest of the method, drop the else and early return.
- **No single-line conditional bodies.** The body of an `if`/`else` must be on its own line inside braces. Never `if (x) return;` on one line.
- **Local variables for readability** — when passing `this._thing.Property` chains into method calls, introduce a short local variable to keep lines readable.
- **Member ordering** — enforced by Rider, alphabetized within each group: constants/static fields → instance fields → constructors → properties → methods. Alphabetical order within each group. **Common mistakes:** placing properties above constructors, and placing methods out of alphabetical order (e.g., `StepFocalLengthUp` before `StepFocalLengthDown`). Double-check ordering before committing.
- **Column alignment** — when adding or changing members, recalculate alignment for the entire group. Don't leave stale padding from previous column widths.
- **Computed properties over methods** — if it takes no parameters and computes from current state, make it a property (e.g., `VerticalFov`, `LookDirection`, `CanDollyZoom`), not a `Compute*()` or `Get*()` method.
- **Break up large methods** — extract private methods when a method has distinct logical sections. Each method should do one thing. Name the extracted method after what it does, not when it's called.

## Project Layout

### Application code

All source code lives inside the Unity project under `Unity/Fram3d/Assets/Scripts/`, organized by assembly:

```
Unity/Fram3d/Assets/Scripts/
  Core/        ← Fram3d.Core (pure C#, System.Numerics, no Unity imports)
  Engine/      ← Fram3d.Engine (Unity, references Core)
  UI/          ← Fram3d.UI (Unity, references Core + Engine)
```

Each directory has a `.asmdef` file defining the assembly and its references. See `docs/reference/domain-model.md` and `docs/reference/decisions.md` for what belongs in each layer.

### Tests

Two test suites: xUnit for Core (pure C#, fast, runs outside Unity) and NUnit Play Mode tests for Engine/UI (runs inside Unity).

```
tests/Fram3d.Core/
  Fram3d.Core.csproj              ← class library compiling Core sources
tests/Fram3d.Core.Tests/
  Fram3d.Core.Tests.csproj        ← xUnit + FluentAssertions, references Fram3d.Core
  stryker-config.json             ← Stryker.NET mutation testing config
Unity/Fram3d/Assets/Tests/
  PlayMode/
    Fram3d.PlayMode.Tests.asmdef  ← NUnit, references Core + Engine + UI
    Engine/                       ← CameraBehaviour, CameraDatabaseLoader,
                                    ViewCameraManager
    UI/                           ← AspectRatioMaskView, CameraInputHandler,
                                    CursorBehaviour, PropertiesPanelView,
                                    SearchableDropdown, SelectionInputHandler
```

Run Core tests: `dotnet test tests/Fram3d.Core.Tests`
Run mutation tests: `cd tests/Fram3d.Core.Tests && dotnet stryker`
Run Play Mode tests: Unity Test Runner → PlayMode tab → Run All

**After writing or modifying tests, always run Stryker** to verify the mutation score hasn't regressed. Current baseline: ~80%. Thresholds: green ≥80%, yellow ≥70%, break <55%.

### When to write which tests

- **Core logic** (domain math, computed properties, state machines) → xUnit tests in `Fram3d.Core.Tests`. These are fast, deterministic, and support mutation testing.
- **Unity wiring** (does CameraBehaviour sync Core state to Unity Camera?) → Play Mode tests. Test that property changes propagate through the Engine layer.
- **UI structure and behavior** (do bars exist, does search filter correctly, do keyboard shortcuts work?) → Play Mode tests. Test behavior, not visual styling.
- **Don't duplicate** — if Core tests already cover the logic, Play Mode tests should only verify the integration wiring, not re-test the math.

### Play Mode test gotchas

- **`DestroyImmediate`, not `Destroy`.** `Object.Destroy` is deferred to end of frame. When tests run back-to-back, the previous test's objects still exist during the next SetUp. `FindAnyObjectByType` finds stale instances, `InputSystem.onEvent` dispatches to stale handlers. Always use `Object.DestroyImmediate` in TearDown.
- **`FindObjectOfType` is deprecated.** Use `FindAnyObjectByType` (Unity 6).
- **Input simulation: queue events, don't manually call `InputSystem.Update()`.** In Play Mode, the Input System updates automatically each frame. Manually calling `InputSystem.Update()` double-processes events and consumes `wasPressedThisFrame` before `MonoBehaviour.Update()` runs. Instead: `InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A))` then `yield return null`.
- **Modifier + key: use `KeyboardState` with multiple keys.** `new KeyboardState(Key.LeftShift, Key.A)` sets both keys in a single state event.
- **Click simulation spans 3 frames, not 2.** `wasPressedThisFrame` fires on frame 1. Frame 2 is the "held" state (`isPressed` true, `wasPressedThisFrame` false) where drag threshold is checked. `wasReleasedThisFrame` fires on frame 3 when buttons go from 1→0. Tests must yield 3 times: `queue press → yield → yield (held) → queue release → yield`. Skipping the held frame means the release never sees `_mouseDownValid` as true.
- **`[SerializeField]` wiring in tests.** Use reflection to set private serialized fields: `typeof(T).GetField("fieldName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value)`.
- **UI Toolkit `resolvedStyle` vs `style`.** `resolvedStyle` depends on the layout engine resolving element sizes, which requires a rendering surface. For absolutely positioned elements, prefer reading `style` values (what code SET) over `resolvedStyle` (what layout computed). Use `s.keyword == StyleKeyword.Undefined ? s.value.value : 0f` to extract the float from a `StyleLength`.
- **UI Toolkit layout needs a panel.** When testing UI elements that read `resolvedStyle` (like `AspectRatioMaskView.UpdateBars`), render to a `RenderTexture` with known dimensions so layout resolves deterministically: `panelSettings.targetTexture = new RenderTexture(1920, 1080, 0)`.
- **Coordinate conversion in assertions.** System.Numerics is right-handed (-Z forward), Unity is left-handed (+Z forward). `ToUnity()` negates Z and negates quaternion X/Y. Position assertions: `Assert.AreEqual(-expected.Z, actual.z)`. Rotation assertions: `Assert.AreEqual(-coreRot.X, unityRot.x)`.
- **Focal length lerp is frame-rate dependent.** Don't assert convergence after a fixed frame count. Poll until convergence or timeout: `for (var i = 0; i < 600; i++) { yield return null; if (converged) yield break; }`.
- **`Assert.AreNotEqual` has no tolerance overload.** Use `Assert.That(Mathf.Abs(a - b), Is.GreaterThan(tolerance))` instead.

- **Don't call `Tick()` explicitly when `Update()` delegates to it.** `CameraInputHandler.Update()` calls `this.Tick(Keyboard.current, Mouse.current)`. If a test also calls `handler.Tick(keyboard, mouse)` after `yield return null`, the keyboard shortcut fires twice in one frame. SET operations (Q/W/E/R tool switching, focal length presets) survive because they're idempotent. TOGGLE operations (`DofEnabled = !DofEnabled`, `ToggleAll()`, etc.) flip twice and return to the original value — the test sees no change and fails. Rule: if `Update()` delegates to a public test entry point, don't call both. Either disable the component during the input frame or let `Update()` handle it.
- **`[Test]` vs `[UnityTest]`.** Use `[Test]` (synchronous) when no frames are needed. `Awake` runs synchronously on `AddComponent` — tests that only verify Awake state don't need `yield return null`. Use `[UnityTest]` only when testing `Start`, `LateUpdate`, `Update`, physics (`WaitForFixedUpdate`), or multi-frame behavior. `[Test]` is faster (no coroutine overhead).
- **Track dynamically created objects for TearDown.** Objects created mid-test (`GameObject.CreatePrimitive`, `new GameObject`) must be destroyed in TearDown, not at the bottom of the test body. If an assertion fails before the cleanup line, the object leaks into subsequent tests. Pattern: maintain a `List<GameObject> _extras` field, add to it when creating objects, destroy all in TearDown.
- **`GameObject.Find` cannot find inactive objects.** After `SetActive(false)`, `GameObject.Find("name")` returns null. Use `FindObjectsByType<T>(FindObjectsInactive.Include, ...)` for inactive objects, or keep a direct reference. Prefer testing observable behavior via public properties (e.g., `controller.IsVisible`) over finding internal GameObjects.
- **Don't test implementation details.** Avoid reflection to read private fields for assertions. If a test needs to verify something, the component should expose it as a public computed property (e.g., `IsVisible`, `IsHoveringHandle`). This makes tests compile-safe and documents the component's observable API.
- **Child GameObjects vs scene roots.** Runtime-created child objects (like gizmo handles) should be parented to the owning MonoBehaviour's transform, not left as scene roots. Child objects auto-destroy with their parent — no manual cleanup needed. Scene root orphans require complex TearDown and leak if cleanup is missed.
- **Input state machines must handle simultaneous transitions.** `wasPressedThisFrame` and `wasReleasedThisFrame` can both be true when a frame hitch (GC, domain reload) exceeds the duration of the physical click. Any state machine with `if (pressed) { return; }` before `if (released)` will silently discard the click. Always check for `pressed && released` first and handle it as an instant action.
- **Multi-camera test isolation.** When testing `ViewCameraManager` or multi-view layouts, each test must destroy all cameras and GameObjects created. Camera.allCameras persists across tests — a stale camera from a previous test will corrupt viewport rect assertions. Use the `_extras` list pattern and `DestroyImmediate` in TearDown.
- **`FindAnyObjectByType` poisoning from scene views.** `CameraInputHandler.Start()` and `SelectionInputHandler.Start()` call `FindAnyObjectByType` to locate `PropertiesPanelView`, `ViewLayoutView`, and `TimelineSectionView`. If any of these exist in the test scene (from the game scene or leaked from another test), `IsPointerOverBlockingUI()` returns true — silently blocking all scroll, drag, click, and duplicate processing while keyboard shortcuts still work (they're evaluated before the blocking UI check). Fix: destroy all instances of these types in `SetUp` before creating the handler component.
- **Do NOT extend `InputTestFixture`.** `InputTestFixture.Setup()` replaces `NativeInputRuntime.instance.onUpdate` with a lambda that routes to an isolated InputManager, but `TearDown()` never restores it. After the fixture runs, `InputActionState` monitors have stale indices. Any real mouse event then triggers `Map index out of range in ProcessControlStateChange` — which fails every subsequent test in the session via unhandled log assertions. Instead: call `InputSystem.AddDevice<Keyboard/Mouse>()` in SetUp (they become `.current`) and `InputSystem.RemoveDevice()` in TearDown, matching the pattern in `CursorBehaviourTests` and `SelectionInputHandlerTests`.

## Unity API Gotchas

**`GenericDropdownMenu.DropDown` overload ambiguity.** `DropDown(Rect, VisualElement)` and `DropDown(Rect, VisualElement, bool)` both exist. When passing just two args, the compiler may select the wrong overload depending on context. Always pass all 4 args explicitly to disambiguate: `menu.DropDown(rect, element, false)`.

## Unity Rendering

**`mesh.triangles` returns a copy.** `Mesh.triangles` (and `vertices`, `normals`, `uv`) returns a COPY of the internal array. `Array.Copy(src, mesh.triangles, n)` copies into a throwaway — the mesh never receives the data. Always create the final array first, then assign: `mesh.triangles = finalArray`.

**`MaterialPropertyBlock` for per-renderer visual variation.** Use `SetPropertyBlock(block)` to overlay properties without creating material instances. `SetPropertyBlock(null)` removes the overlay entirely. Never use `renderer.material` (creates instances that are hard to clean up) — use `renderer.sharedMaterial` for reads and `MaterialPropertyBlock` for writes. The `_EMISSION` keyword cannot be toggled via PropertyBlock (it's a shader compile variant, not a property).

**`ZTest Always` for always-on-top rendering.** Gizmos use a shader with `ZTest Always` and `ZWrite Off` on a dedicated layer. The main camera renders them — no separate overlay camera needed. Objects on the Gizmo layer are excluded from `ElementPicker` via layer mask.

**Shader stripping in builds.** Unity strips shaders not referenced by materials in the build. If a shader is only used at runtime (e.g., `Unlit/Color` for frustum wireframes created via `new Material(Shader.Find(...))`), it will be stripped. Fix: add a `[SerializeField] private Shader` field referencing the shader, or add it to `Project Settings → Graphics → Always Included Shaders`. A `[SerializeField]` reference is preferred — it's explicit and discoverable.

**`Camera.rect` for multi-viewport layout.** Use `Camera.rect` (normalized 0–1) to split the screen into viewports instead of RenderTextures. Camera.rect is cheaper (no extra render targets), composites naturally, and Unity handles input coordinate mapping. When combined with UI Toolkit overlays, convert screen pixels to CSS pixels via `screenPixels / (Screen.width / root.resolvedStyle.width)` because PanelSettings may scale the UI independently of screen resolution.

**`rootVisualElement` may be null in `Start()`.** UI Toolkit documents can fail to initialize by `Start()` if the UIDocument's panel hasn't been created yet. Guard with a null check and retry next frame: `if (uiDocument.rootVisualElement == null) { StartCoroutine(RetryInit()); return; }`. This is especially common when UIDocuments are created at runtime via `AddComponent`.

## Input Handling

**Do not poll `Mouse.scroll.ReadValue()` with modifier checks in `Update()`.** This architecture causes scroll bleed on macOS — scroll events and modifier key-up events can land in the same frame, and polling sees the final state (modifier released) while scroll accumulated while the modifier was still held. Seven fix attempts failed before this was identified as an architectural problem, not a timing/guard problem.

**`CameraInputHandler` uses event-level interception via `InputSystem.onEvent`.** Each scroll event is paired with the modifier state at the time the raw event arrived, preserving temporal ordering. Scroll samples are queued and processed in `Update()`. Do not rewrite this to poll `ReadValue()` — the scroll bleed bug will return.

**Trackpad momentum requires a gap-based guard.** macOS trackpads deliver momentum scroll events for 1-2 seconds after finger lift, with intermittent gaps where scroll drops to zero then resurfaces. The guard in `HandleScrollSample` blocks unmodified scroll within 150ms of the last modifier-associated scroll (`SCROLL_BLEED_COOLDOWN`). The timer resets on each blocked momentum event, so the guard stays active through the entire momentum period. Intentional new scroll after a 150ms+ gap passes through immediately. See `docs/reference/prior-codebase-lessons.md` for the full history.

**Resync event-tracked state on focus change.** `InputSystem.onEvent` doesn't fire while the app is unfocused. Any state tracked incrementally from events (modifier booleans, accumulated deltas) can get stuck if the user releases a key while another app has focus. `OnApplicationFocus(true)` must resync from polled values (`keyboard.leftCtrlKey.isPressed` etc.) and clear stale event queues. Without this, modifier state permanently inverts after Cmd-tab (FRA-127).

**Clamp mouse delta to reject focus-change spikes.** `mouse.delta.ReadValue()` can return a huge accumulated value (hundreds of pixels) when the app regains focus. Apply a `MAX_DELTA_SQR_MAGNITUDE` guard in drag handlers — anything above ~200px/frame is a focus artifact, not real user input (FRA-126).

**Raycast hover instability near collider edges.** When a ground plane (or other large collider) is in the scene, `Physics.Raycast` can intermittently hit the ground instead of a small element — especially at oblique angles near element edges. This causes single-frame hover loss and cursor flicker. Fix: keep the previous hover if the raycast misses but the mouse hasn't moved far from the last hit position (`HOVER_KEEP_DISTANCE_SQ = 400f`, ~20px). Hover only clears when the mouse moves away. Don't try to fix this by adjusting collider sizes or layer masks — the raycast miss is a geometric reality at certain angles.

## Multi-View System

**`ViewCameraManager` owns all camera lifecycle.** Creates, positions, and destroys cameras for each view slot. In single-view mode, one camera fills the screen. In multi-view, each slot gets a camera with a `Camera.rect` defining its viewport region. The Camera View slot always uses the main `CameraBehaviour` camera; Director View creates a separate camera. When switching layouts, existing cameras are recycled — don't destroy and recreate if the slot type hasn't changed.

**Stale slot state when switching view modes.** When switching from single Director view to a split layout, the Director camera's state (position, rotation) can leak into the Camera View slot if the slot model isn't reset. Always reset slot state on layout change, not just camera properties.

**Screen pixels vs CSS pixels for UI overlays.** `PanelSettings.scaleMode` may cause UI Toolkit to use different coordinates than `Screen.width/height`. The ratio `Screen.width / root.resolvedStyle.width` gives the conversion factor. All viewport inset calculations (properties panel width, overlay positioning) must use CSS pixels, not screen pixels. This was the root cause of 3 separate bugs during FRA-52.

**Extract shared viewport scoping into `ViewportScope`.** `AspectRatioMaskView` and `CompositionGuideView` both need to position themselves within the Camera View's viewport (accounting for properties panel inset in single-view, or slot rect in multi-view). This logic was duplicated until extracted into `ViewportScope.Apply()`. When adding new overlays that scope to the camera viewport, use `ViewportScope` — don't reimplement the inset/rect logic.

## Cursor Management

**Software cursor overlay via `OnGUI`, not native plugins.** All platform-specific native cursor code was deleted (MacOSCursorService, EditorCursorService, LinuxCursorService, WindowsCursorPatch, CursorWrapper.dylib, NativeCursorsWebGL.jslib). The replacement is `SoftwareCursorOverlay` — a `MonoBehaviour` that renders cursor textures via `GUI.DrawTexture` in `OnGUI`. When a custom cursor is active, `Cursor.visible = false` hides the OS cursor and the overlay draws the texture at the mouse position. When reset, `Cursor.visible = true` and the overlay stops drawing. This approach:
- Works identically in Editor and standalone
- Has zero platform-specific code
- Cannot flicker (it's rendered by Unity, not fighting the OS cursor system)
- Trails by one frame (acceptable for hover/drag cursors, not for the primary pointer)

**Why native cursor management failed (8 approaches).** macOS resets the cursor on every `NSMouseMoved` event via its cursor rect system. Any approach that calls `[NSCursor set]` from outside AppKit's view hierarchy fights this reset. Approaches tried and abandoned: (1) `Cursor.SetCursor` — overridden by Editor IMGUI. (2) Native dylib with `[NSCursor pointingHandCursor] set` — flickered between set and reset. (3) Transparent overlay NSView with `cursorUpdate:` — worked in standalone but Unity Editor inserts subviews above it during repaint. (4) `[NSWindow disableCursorRects]` — Unity re-enables during repaint. (5) Per-frame re-application in `EditorApplication.update` — still one-frame flashes. (6) Persistent overlay with grace-based reset. (7) Raising the overlay NSView above Unity's subviews each frame. (8) Combined belt-and-suspenders of all above. The lesson: **don't fight the platform's cursor system from outside it**. The software overlay sidesteps the entire problem.

**Cursor textures from PNG assets, not programmatic pixels.** Early attempts drew cursor shapes via explicit `Color32` arrays (~100 lines for a 19x24 hand). This is fragile, looks bad at Retina resolutions, and is unmaintainable. Use actual PNG files loaded from `Resources/Cursors/` with correct import settings: `isReadable=true`, `alphaIsTransparency=true`, `enableMipMap=false`, `textureType=Cursor`, `filterMode=Point`, no compression. `Cursor.SetCursor` requires readable textures — calling `texture.Apply(false, true)` makes them non-readable and triggers "Invalid texture used for cursor" errors.

## Implementation Workflow

When asked to implement a feature or milestone, follow this sequence exactly.

### 1. Find the work

1. Read `docs/reference/build-order.md` to check the dependency map. If the feature depends on unbuilt infrastructure, flag it before starting — it may need to be reordered or deferred.
2. Read `docs/reference/roadmap.md` to understand the feature and its dependencies.
3. Read the relevant spec in `docs/specs/`.
4. Read all reference docs listed in the "Read before writing code" table below. If the feature involves UI, also read the "Read before writing UI" docs.

### 2. Find the Linear ticket

1. Search Linear thoroughly — by title keywords, by project/milestone, and by browsing backlog.
2. If an existing ticket matches, use it. Update its status to In Progress.
3. Only create a new ticket if no match exists after thorough search.

### 3. Plan

1. Check `docs/plans/` for an existing implementation plan.
2. If none exists, write one. Save to `docs/plans/YYYY-MM-DD-<feature>.md`.
3. All file paths in the plan must use full paths from the repo root (e.g., `Unity/Fram3d/Assets/Scripts/Core/...`).
4. Plans must cover all layers end-to-end (Core → Engine → UI), not just domain.

### 4. Branch and implement

1. Create a feature branch from the Linear ticket's suggested branch name.
2. Implement across all layers. Every new type needs tests.
3. Run `dotnet test tests/Fram3d.Core.Tests` after every significant change — do not accumulate untested code.
4. Before each commit, verify:
   - Tests pass.
   - No C# 10+ features (check for `record`, `init`, inferred delegate types, file-scoped namespaces).
   - All instance members use `this.` qualifier.
   - Naming follows conventions (`_camelCase` private fields, `camelCase` serialized fields, `SCREAMING_SNAKE_CASE` constants).
   - No YAGNI — every member, parameter, and type is used by code in this PR. Nothing "for later."

### 5. Commit

- Clean, logical commits — separate scaffolding, config, and feature code.
- Reference the Linear ticket ID (e.g., FRA-36) in the feature commit message.
- **Never use `git add -A`.** The Unity project may contain large untracked directories (asset packages, cursor themes, test artifacts). Always stage specific files by name.

### 6. PR and Linear

1. Push branch, create PR with summary and test plan.
2. **PRs always target `main`.** Never set the base branch to another feature branch.
3. PR title format: `FRA-XX: short description`.
4. Link the PR to the Linear issue (as an attachment).
5. Update Linear status — do NOT overwrite the existing description. Append implementation notes below a `---` separator.

## Documentation

### Must-read before any work

| Doc | What it is |
|-----|-----------|
| `docs/reference/domain-language.md` | **Read this first.** Canonical terminology: element, shot, angle, track, view, etc. Part 1 is definitions. Part 2 explains why. Part 3 lists retired terms. |
| `docs/reference/roadmap.md` | Product roadmap — milestones, features, phases. |
| `docs/reference/decisions.md` | Architectural decisions (confirmed and pending). |

### Read before writing code

| Doc | What it is |
|-----|-----------|
| `docs/reference/domain-model.md` | DDD approach — bounded contexts, aggregates, value objects, the split model. |
| `docs/reference/unity-naming-conventions.md` | How domain terms map to C# types. The `*Element` suffix convention, namespace structure, aliasing rules. |
| `docs/reference/bounded-context-map.md` | Assembly definitions and bounded context boundaries. |
| `docs/reference/build-order.md` | Implementation sequencing. |
| `docs/reference/prior-codebase-lessons.md` | Patterns to reuse and anti-patterns to avoid from the prior codebase. |

### Read before writing UI

| Doc | What it is |
|-----|-----------|
| `docs/reference/interaction-patterns.md` | Input mappings, keyframe rules, shot bar behavior, panel system, timecodes. |
| `docs/reference/tuned-constants.md` | Empirically tuned values — camera speeds, thresholds, sensitivities. |

### Other reference

| Doc | What it is |
|-----|-----------|
| `docs/reference/tech-stack.md` | Engine, packages, UI framework. |

### Specs

All feature specs live in `docs/specs/`. Named `milestone-X.Y-feature-name-spec.md`. Read the relevant spec before implementing any milestone.

### Research

Codebase research reports live in `docs/research/`. Check for recent reports before dispatching research agents — reuse reports less than 24 hours old.
