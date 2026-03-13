# Domain Model

DDD-informed, not DDD-orthodox. The cinema domain is rich enough to warrant modeling discipline; Unity is opinionated enough that full layered architecture would fight the engine.

This doc describes the modeling **concepts** we draw from and how they apply to Fram3d. It is not a prescription for specific class hierarchies — those emerge during implementation.

---

## Ubiquitous Language

Cinema terminology is consistent from roadmap to spec to class name to method name. Dolly, crane, truck, angle, blocking, stopwatch — these terms mean the same thing everywhere. This is the single highest-value DDD concept. See `domain-language.md` for the full glossary.

---

## Assembly Structure

Four assemblies, layered. Each references only downward. See `architecture.md` for the full graph and design-sessions/01 for the design rationale.

| Assembly | Layer | Description |
|----------|-------|-------------|
| `Fram3d.Core` | Domain | Pure C#. The 3D world and everything it needs — camera, characters, scene, shots, timeline, assets. Testable with xUnit. |
| `Fram3d.Services` | Orchestration | Pure C#. Cross-domain features that touch the whole domain — AI, serialization, export logic, script import. |
| `Fram3d.Engine` | Integration | Unity. MonoBehaviour wrappers, evaluation pipeline, asset loading, rendering. Can't reference UI. |
| `Fram3d.UI` | Presentation | Unity. UI Toolkit panels, overlays, input handling. Top of the stack. |

### Core Namespaces

`Fram3d.Core` is organized by namespace. All namespaces are in one assembly — natural domain couplings (camera reads character positions, linking reads bone transforms) are allowed. Discipline enforced via `internal` access modifiers on type internals.

| Namespace | What it owns | References within Core |
|-----------|-------------|----------------------|
| `Core.Common` | Element base class, identity types, commands, registries, value objects, project/settings | Nothing |
| `Core.Timeline` | Keyframe<T>, tracks, interpolation, GlobalTimeline, Playhead | Common |
| `Core.Assets` | AssetEntry, AssetLibrary, EnvironmentTemplate | Common |
| `Core.Camera` | CameraElement, lens, focus, DOF, shake, follow, watch, snorricam | Common, Timeline |
| `Core.Character` | CharacterElement, skeleton, pose, IK, expression, walk cycles | Common, Timeline |
| `Core.Shot` | Shot, Angle, ActiveAngleTrack, Subtitle | Common, Timeline, Camera |
| `Core.Scene` | LightElement, selection, linking, grouping, walls, snap | Common, Timeline, Character |

---

## Modeling Concepts

### Aggregates

An aggregate is a cluster of domain objects with a single root that controls access. External code only touches the root — never reaches inside to manipulate children directly. This prevents tangled state and makes ownership clear.

*Example*: A shot owns its camera animations. Code that wants to add a keyframe goes through the shot, not directly to the animation object. The shot enforces rules like "at least one camera keyframe must exist."

*Example*: `CharacterElement` is the aggregate root for character data. Camera and Scene code calls `CharacterElement.GetBoneWorldPosition("head")` — they never reference `Skeleton`, `Joint`, or `IKSolver` directly. Those types are `internal` to the Character namespace.

Aggregate boundaries should emerge during implementation. The prior codebase's `TimelineState` was a cautionary tale — it owned everything and enforced nothing.

### Value Objects

Immutable types that carry meaning without identity. Two value objects with the same data are equal. They prevent primitive obsession — passing raw floats for focal lengths, raw strings for IDs.

*Examples*: A focal length isn't just a float — it has a valid range (14–400mm). A keyframe ID isn't just a GUID — it rejects `Guid.Empty`. A time position isn't just a double — it can't be negative. Wrapping these in value objects catches bugs at construction time instead of at runtime.

*From the prior codebase*: `KeyframeId` (wraps GUID, rejects empty) and `TimePosition` (rejects negative, provides `Add()`/`Subtract()` with clamping) worked well and should be carried forward.

### Registries

Typed collections of domain objects with query methods and `IObservable` change streams. `ElementRegistry` and `ShotRegistry` in `Core.Common` for shared data. Context-private data uses plain collections internally.

Registries are the cross-context communication mechanism — subscribers react to `Added`/`Removed`/`Reordered` events via `IObservable<T>`. No central event bus.

### Command Pattern

`ICommand` with `Execute()` / `Undo()` / `Redo()` for all user-initiated state changes. `ICommand`, `CommandStack`, and `CompoundCommand` live in `Core.Common`. Each namespace defines its own command types. Commands are exclusively for user actions — internal operations (scrubbing, evaluating, refreshing) are plain method calls, not commands. See `prior-codebase-lessons.md` for the anti-pattern this prevents.

---

## What We Skip

**Full layered architecture** (domain → application → infrastructure → presentation). The four-assembly structure provides the meaningful boundaries (pure C# vs Unity, domain vs orchestration vs integration vs presentation) without the ceremony. Unity blurs traditional layers by design.

---

## The Split Model

The key architectural pattern: **pure C# for domain logic, thin MonoBehaviour wrappers for scene graph integration.**

- **`Fram3d.Core` + `Fram3d.Services`**: Pure C# domain and orchestration layers. No Unity dependencies. Testable with standard xUnit. Use `System.Numerics` for math types.
- **`Fram3d.Engine`**: Thin MonoBehaviour wrappers that bridge domain types to Unity's scene graph. Contains the evaluation pipeline (`SceneEvaluator`) that runs each frame. Minimal logic — calls into Core for all computation.
- **`Fram3d.UI`**: UI Toolkit presentation layer. Reads domain state, renders UI, routes user input to domain commands.

If a Core or Services type needs `using UnityEngine;`, something is wrong.
