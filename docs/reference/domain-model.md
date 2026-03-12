# Domain Model

DDD-informed, not DDD-orthodox. The cinema domain is rich enough to warrant modeling discipline; Unity is opinionated enough that full layered architecture would fight the engine.

This doc describes the modeling **concepts** we draw from and how they apply to Fram3d. It is not a prescription for specific class hierarchies — those emerge during implementation.

---

## Ubiquitous Language

Cinema terminology is consistent from roadmap to spec to class name to method name. Dolly, crane, truck, angle, blocking, stopwatch — these terms mean the same thing everywhere. This is the single highest-value DDD concept. See `domain-language.md` for the full glossary.

---

## Bounded Contexts via Assembly Definitions

Each context is a separate `.asmdef`, enforcing boundaries at compile time:

| Context | Core Concepts | Assembly | Milestones |
|---------|--------------|----------|------------|
| Camera | Rig, lens, focus, shake, DOF | `Fram3d.Camera` | 1.1, 1.2, 6.2, 7.2, 9.1 |
| Sequencing | Shot, keyframe, track, playback, stopwatch | `Fram3d.Sequencing` | 3.1, 3.2, 8.4 |
| Scene | Element, selection, gizmo, ground plane | `Fram3d.Scene` | 2.1, 5.1, 6.3, 8.1, 10.1 |
| Viewport | Panel system, layout, views | `Fram3d.Viewport` | 2.2, 8.2 |
| Characters | Mannequin, pose, expression, skeleton | `Fram3d.Characters` | 6.1, 7.1, 12.2 |
| Persistence | Project, scene file, asset bundle | `Fram3d.Persistence` | 4.1, 4.2, 8.3 |
| Assets | Import, library, environments | `Fram3d.Assets` | 4.3, 5.2, 5.3, 12.3 |
| Export | Renderer, storyboard, EDL | `Fram3d.Export` | 4.4 |
| AI | Shot description, blocking, suggestions | `Fram3d.AI` | 11.1, 11.2, 11.3, 12.1 |

If Characters can't reference Camera internals, you physically can't create the wrong coupling.

---

## Modeling Concepts

### Aggregates

An aggregate is a cluster of domain objects with a single root that controls access. External code only touches the root — never reaches inside to manipulate children directly. This prevents tangled state and makes ownership clear.

*Example*: A shot owns its camera animations. Code that wants to add a keyframe goes through the shot, not directly to the animation object. The shot enforces rules like "at least one camera keyframe must exist."

Aggregate boundaries should emerge during implementation. The prior codebase's `TimelineState` was a cautionary tale — it owned everything and enforced nothing.

### Value Objects

Immutable types that carry meaning without identity. Two value objects with the same data are equal. They prevent primitive obsession — passing raw floats for focal lengths, raw strings for IDs.

*Examples*: A focal length isn't just a float — it has a valid range (14–400mm). A keyframe ID isn't just a GUID — it rejects `Guid.Empty`. A time position isn't just a double — it can't be negative. Wrapping these in value objects catches bugs at construction time instead of at runtime.

*From the prior codebase*: `KeyframeId` (wraps GUID, rejects empty) and `TimePosition` (rejects negative, provides `Add()`/`Subtract()` with clamping) worked well and should be carried forward.

### Domain Events

Cross-context communication without coupling. When something happens in one context that another context cares about, fire an event rather than creating a direct dependency.

*Example*: When a shot's duration changes, the timeline UI needs to update, the overview needs to redraw, and the export system's cached frame count is stale. The shot doesn't know about any of these — it fires `DurationChanged` and each subscriber handles its own update.

### Command Pattern

`ICommand` with `Execute()` / `Undo()` / `Redo()` for all user-initiated state changes. Enables the undo stack and potential action replay. Commands are exclusively for user actions — internal operations (scrubbing, evaluating, refreshing) are plain method calls, not commands. See `prior-codebase-lessons.md` for the anti-pattern this prevents.

---

## What We Skip

**Repositories.** Unity manages object lifecycles through `MonoBehaviour`, `Instantiate`, `Destroy`, and scene serialization. Wrapping that in repositories duplicates what the engine already does.

**Application services / use case classes.** The `ApplicationController.Update()` frame loop is inherently imperative. Routing every frame tick through application service abstractions adds indirection in a hot path.

**Full layered architecture** (domain → application → infrastructure → presentation). Unity blurs these layers by design. `MonoBehaviour` is simultaneously presentation, infrastructure, and sometimes domain logic.

---

## The Split Model

The key architectural pattern: **pure C# for domain logic, thin MonoBehaviour wrappers for scene graph integration.**

- **Pure C# domain layer**: Domain types with no Unity dependencies. Testable with standard xUnit without Unity's test runner.
- **MonoBehaviour integration layer**: Thin wrappers that bridge domain types to Unity's scene graph. These need Unity to run but contain minimal logic.

This gives testability without the ceremony of formal DDD layering.
