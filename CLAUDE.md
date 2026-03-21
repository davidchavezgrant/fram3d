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
- **Member ordering** — enforced by Rider, alphabetized within each group: constants/static fields → instance fields → constructors → properties → methods. Alphabetical order within each group.

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

Tests live outside Unity as a standalone .NET project:

```
tests/Fram3d.Core.Tests/
  Fram3d.Core.Tests.csproj    ← xUnit + FluentAssertions
```

The test project compiles Core's source files directly via `<Compile Include="../../Unity/Fram3d/Assets/Scripts/Core/**/*.cs" />` — one copy of source, two compilation targets (Unity asmdef + .NET for `dotnet test`).

## Implementation Workflow

When asked to implement a feature or milestone, follow this sequence exactly.

### 1. Find the work

1. Read `docs/reference/roadmap.md` to understand the feature and its dependencies.
2. Read the relevant spec in `docs/specs/`.
3. Read all reference docs listed in the "Read before writing code" table below. If the feature involves UI, also read the "Read before writing UI" docs.

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
2. PR title format: `FRA-XX: short description`.
3. Link the PR to the Linear issue (as an attachment).
4. Update Linear status — do NOT overwrite the existing description. Append implementation notes below a `---` separator.

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
