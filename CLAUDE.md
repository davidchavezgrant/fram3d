# Fram3d — Project Instructions

Fram3d is a 3D previsualization tool for filmmakers. Unity project (Unity 6, Cinemachine 3.x, C#).

## Rules

- **Never overwrite or replace existing work.** When building new mockups, features, or files that relate to existing ones, incorporate or reference the existing work — don't start from scratch. If you need to create something new in the same space, create a separate file. Never nuke what we've already iterated on.
- **Use the domain language.** Read `docs/reference/domain-language.md` before writing specs, code, or UI text. Terms are chosen deliberately — don't invent synonyms.

## Documentation

### Must-read before any work

| Doc | What it is |
|-----|-----------|
| `docs/reference/domain-language.md` | **Read this first.** Canonical terminology: element, shot, angle, track, view, etc. Part 1 is the definitions. Part 2 explains why. Part 3 lists retired terms (don't use these). |
| `docs/reference/roadmap.md` | Product roadmap — milestones, features, build order. |
| `docs/reference/architecture.md` | Technical architecture — patterns, anti-patterns, constants, domain modeling. |

### Read before writing code

| Doc | What it is |
|-----|-----------|
| `docs/reference/unity-naming-conventions.md` | How domain terms map to C# types. The `*Element` suffix convention, namespace structure, aliasing rules. |
| `docs/reference/bounded-context-map.md` | Assembly definitions and bounded context boundaries. |
| `docs/reference/build-order.md` | Implementation sequencing. |

### Specs

All feature specs live in `docs/specs/`. Named `milestone-X.Y-feature-name-spec.md`. Read the relevant spec before implementing any milestone.

### Research

Codebase research reports live in `docs/research/`. Check for recent reports before dispatching research agents — reuse reports less than 24 hours old.
