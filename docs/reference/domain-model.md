# Domain Model

DDD-informed, not DDD-orthodox. The cinema domain is rich enough to warrant modeling discipline; Unity is opinionated enough that full layered architecture would fight the engine.

---

## What We Use

**Ubiquitous language.** Cinema terminology is consistent from roadmap to spec to class name to method name. Dolly, crane, truck, angle, blocking, stopwatch — these terms mean the same thing everywhere. This is the single highest-value DDD concept.

**Bounded contexts via Assembly Definitions.** Each context is a separate `.asmdef`, enforcing boundaries at compile time:

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

**Aggregates.** Domain objects with clear ownership boundaries:

| Aggregate Root | Owns |
|---------------|------|
| `ShotSetup` | `CameraAnimation`, `ShotObjectManager`, per-object `ObjectAnimation` instances |
| `Character` | Pose state, expression state, skeleton mapping, customization |
| `Scene` | Elements, shots, lighting, timelines |
| `Project` | Scenes, character definitions, settings |

**Value objects.** Immutable types that carry meaning without identity:

- `ShotId`, `KeyframeId` — GUID-based identity
- `CameraKeyframeState`, `ObjectState` — state snapshots
- `FocalLength`, `AspectRatio`, `SensorDimensions` — typed domain values
- `Pose`, `Expression` — character state snapshots

**Domain events.** Cross-context communication without coupling:

- Shot-level: `DurationChanged`, `KeyframeAdded`, `KeyframeRemoved`, `KeyframeMoved`
- Selection: `ElementSelected`, `SelectionCleared`
- Scene-level: events for element add/remove, lighting changes

**Command pattern.** `ICommand` with `Execute()` / `Undo()` / `Redo()` for all user actions. Enables undo stack and action replay.

---

## What We Skip

**Repositories.** Unity manages object lifecycles through `MonoBehaviour`, `Instantiate`, `Destroy`, and scene serialization. Wrapping that in repositories duplicates what the engine already does.

**Application services / use case classes.** The `ApplicationController.Update()` frame loop is inherently imperative. Routing every frame tick through application service abstractions adds indirection in a hot path.

**Full layered architecture** (domain → application → infrastructure → presentation). Unity blurs these layers by design. `MonoBehaviour` is simultaneously presentation, infrastructure, and sometimes domain logic.

---

## The Split Model

The key architectural pattern: **pure C# for domain logic, thin MonoBehaviour wrappers for scene graph integration.**

- **Pure C# domain layer**: `ShotSetup`, `CameraAnimation`, `KeyframeManager<T>`, `Character`, `Pose`, value objects. No Unity dependencies. Testable with standard xUnit without Unity's test runner.
- **MonoBehaviour integration layer**: `SceneElement`, `VirtualCameraRig`, `ApplicationController`. Thin wrappers that delegate to the domain layer. These need Unity to run but contain minimal logic.

This gives testability without the ceremony of formal DDD layering.
