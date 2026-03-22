# Fram3d — Project Instructions

Fram3d is a 3D previsualization tool for filmmakers. Unity project (Unity 6, URP, C#).

## Rules

- **Never overwrite or replace existing work.** When building new mockups, features, or files that relate to existing ones, incorporate or reference the existing work — don't start from scratch. If you need to create something new in the same space, create a separate file. Never nuke what we've already iterated on.
- **Use the domain language.** Read `docs/reference/domain-language.md` before writing specs, code, or UI text. Terms are chosen deliberately — don't invent synonyms.
- **All work requires a Linear ticket.** Before creating a new issue, **thoroughly search** for an existing one — search by title keywords, browse the relevant project/milestone, and check backlog. Only create a new issue if no match exists. Reference the ticket (e.g., FRA-36) in commits and PRs.
- **Never overwrite Linear issue content.** When updating an issue, append implementation notes below the existing description. Never replace the original spec text or user-written content.
- **Question scope decisions.** If asked to create a separate issue/PR for a bug found during feature work, push back — bugs found during implementation usually belong in the feature's PR. Only create separate issues for bugs that are genuinely independent or discovered after the feature is merged.

## Code Style (C#)

The `.editorconfig` at the project root is the source of truth for formatting. These rules cover what editorconfig can't express.

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
- **Braces required for multiline only.** `using` blocks never require braces.
- **No extra blank lines** — formatter strips them (keep_blank_lines = 0).
- **Early return** over else blocks — if an `if` branch returns/continues/breaks and the else would be the rest of the method, drop the else and early return.
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
    Engine/                       ← CameraBehaviour, CameraDatabaseLoader
    UI/                           ← AspectRatioMaskView, CameraInputHandler,
                                    PropertiesPanelView, SearchableDropdown
```

Run Core tests: `dotnet test tests/Fram3d.Core.Tests`
Run mutation tests: `cd tests/Fram3d.Core.Tests && dotnet stryker`
Run Play Mode tests: Unity Test Runner → PlayMode tab → Run All

**After writing or modifying tests, always run Stryker** to verify the mutation score hasn't regressed. Current baseline: ~85%. Thresholds: green ≥85%, yellow ≥75%, break <60%.

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
- **`[SerializeField]` wiring in tests.** Use reflection to set private serialized fields: `typeof(T).GetField("fieldName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value)`.
- **UI Toolkit `resolvedStyle` vs `style`.** `resolvedStyle` depends on the layout engine resolving element sizes, which requires a rendering surface. For absolutely positioned elements, prefer reading `style` values (what code SET) over `resolvedStyle` (what layout computed). Use `s.keyword == StyleKeyword.Undefined ? s.value.value : 0f` to extract the float from a `StyleLength`.
- **UI Toolkit layout needs a panel.** When testing UI elements that read `resolvedStyle` (like `AspectRatioMaskView.UpdateBars`), render to a `RenderTexture` with known dimensions so layout resolves deterministically: `panelSettings.targetTexture = new RenderTexture(1920, 1080, 0)`.
- **Coordinate conversion in assertions.** System.Numerics is right-handed (-Z forward), Unity is left-handed (+Z forward). `ToUnity()` negates Z and negates quaternion X/Y. Position assertions: `Assert.AreEqual(-expected.Z, actual.z)`. Rotation assertions: `Assert.AreEqual(-coreRot.X, unityRot.x)`.
- **Focal length lerp is frame-rate dependent.** Don't assert convergence after a fixed frame count. Poll until convergence or timeout: `for (var i = 0; i < 600; i++) { yield return null; if (converged) yield break; }`.
- **`Assert.AreNotEqual` has no tolerance overload.** Use `Assert.That(Mathf.Abs(a - b), Is.GreaterThan(tolerance))` instead.

## Input Handling

**Do not poll `Mouse.scroll.ReadValue()` with modifier checks in `Update()`.** This architecture causes scroll bleed on macOS — scroll events and modifier key-up events can land in the same frame, and polling sees the final state (modifier released) while scroll accumulated while the modifier was still held. Seven fix attempts failed before this was identified as an architectural problem, not a timing/guard problem.

**`CameraInputHandler` uses event-level interception via `InputSystem.onEvent`.** Each scroll event is paired with the modifier state at the time the raw event arrived, preserving temporal ordering. Scroll samples are queued and processed in `Update()`. Do not rewrite this to poll `ReadValue()` — the scroll bleed bug will return.

**Trackpad momentum requires a gap-based guard.** macOS trackpads deliver momentum scroll events for 1-2 seconds after finger lift, with intermittent gaps where scroll drops to zero then resurfaces. The guard in `HandleScrollSample` blocks unmodified scroll within 150ms of the last modifier-associated scroll (`SCROLL_BLEED_COOLDOWN`). The timer resets on each blocked momentum event, so the guard stays active through the entire momentum period. Intentional new scroll after a 150ms+ gap passes through immediately. See `docs/reference/prior-codebase-lessons.md` for the full history.

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
