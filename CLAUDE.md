# Fram3d — Project Instructions

Fram3d is a 3D previsualization tool for filmmakers. Unity project (Unity 6, Cinemachine 3.x, C#).

## Rules

- **Never overwrite or replace existing work.** When building new mockups, features, or files that relate to existing ones, incorporate or reference the existing work — don't start from scratch. If you need to create something new in the same space, create a separate file. Never nuke what we've already iterated on.
- **Use the domain language.** Read `docs/reference/domain-language.md` before writing specs, code, or UI text. Terms are chosen deliberately — don't invent synonyms.

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
