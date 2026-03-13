# Fram3d Architecture

---

## Assembly Graph

```
Fram3d.Core      — pure C#, no dependencies
Fram3d.Services  — pure C#  → Core
Fram3d.Engine    — Unity    → Services, Core
Fram3d.UI        — Unity    → Engine, Services, Core
```

Four assemblies. Each references only downward. No cycles.

| Assembly | What it does | Can't do |
|----------|-------------|----------|
| **Core** | Domain logic, timeline, commands, registries, value objects. All pure C#, testable with xUnit. | Touch Unity |
| **Services** | Cross-domain orchestration: AI, serialization, export logic, script import. Pure C#. | Touch Unity |
| **Engine** | MonoBehaviour wrappers, evaluation pipeline, asset loading, rendering, 3D scene interaction (gizmos, raycasting, selection highlighting). | Reference UI |
| **UI** | UI Toolkit: panels, overlays, timeline editor, keyboard shortcuts, 2D input. | — (top of stack) |

---

## Core Namespace Structure

One assembly, organized by namespace. Natural domain couplings are allowed (camera reads character bone positions directly). Namespace boundaries are organizational, not compile-time.

```
Core.Common ← (no deps within Core)
  │
  ├── Core.Timeline ← Common
  │
  ├── Core.Assets ← Common
  │
  ├── Core.Camera ← Common, Timeline
  │
  ├── Core.Character ← Common, Timeline
  │
  ├── Core.Shot ← Common, Timeline, Camera
  │
  └── Core.Scene ← Common, Timeline, Character
```

**DAG. No cycles.**

- **Common** is foundational — identity types, Element base, commands, registries, value objects, project settings
- **Timeline** is generic animation infrastructure — `Keyframe<T>`, tracks, interpolation. Doesn't know what T represents.
- **Camera** and **Character** are siblings — both reference Common + Timeline, not each other
- **Shot** → Camera (Angle references CameraElement) but Camera does NOT reference Shot
- **Scene** → Character (LinkChainEvaluator reads bone positions) but Character does NOT reference Scene

---

## The Split Model

**Pure C# for domain logic, thin wrappers for Unity integration.**

| Layer | Math types | Testing | Unity access |
|-------|-----------|---------|-------------|
| Core + Services | `System.Numerics` (`Vector3`, `Quaternion`) | xUnit | None |
| Engine + UI | `UnityEngine` (`Vector3`, `Quaternion`, `Transform`) | Unity Test Runner | Full |

Conversion between `System.Numerics` and `UnityEngine` types happens at the integration boundary via extension methods in `Fram3d.Engine`.

---

## Evaluation Pipeline

The per-frame update that connects domain state to Unity's scene graph. Orchestrated by `SceneEvaluator` in `Fram3d.Engine.Evaluation`. Runs in `LateUpdate`. See `design-sessions/02-evaluation-pipeline.md` for full design.

### Evaluation steps

```
1. Read current time from Playhead (Core.Timeline)
2. Evaluate element keyframes at global time → write to Element.Position/Rotation/Scale (Core.Common)
3. Evaluate link chains in dependency order — parents before children (Core.Scene)
4. Evaluate character poses at global time (Core.Character)
5. Evaluate camera keyframes at local time — per-shot (Core.Camera)
6. Apply follow/watch — read target Element.Position (Core.Common)
7. Apply shake — additive rotation offset (Core.Camera)
8. Sync all results to Unity (Engine.Integration)
```

All computation in Core (pure C#, steps 1–7). Only step 8 touches Unity. SceneEvaluator directly calls domain types — no interfaces, no abstraction.

### Triggers

SceneEvaluator subscribes to events — no dirty flags in domain code:
- `CommandStack.Executed/Undone/Redone` → evaluate once
- `Playhead.Scrubbed` → evaluate once
- `Playhead.IsPlaying` → evaluate every frame

`Sync()` runs every LateUpdate regardless — covers gizmo drags where evaluation is skipped but the dragged position still needs to reach Unity.

### Gizmo drag flow

```
During drag: User → Element.Position (direct write) → Sync → Unity
On release:  User → Command → CommandStack → Element.Position → Evaluate → Sync → Unity
Playback:    Playhead → Evaluate → Element.Position (from keyframes) → Sync → Unity
```

### Export

Procedural effects (shake, follow, snorricam) baked into keyframes first. Then `SceneEvaluator.EvaluateAtTime(t)` steps through frames at 1/fps — same evaluation logic, explicit time instead of Playhead.

### Unity sync

Each `*Behaviour` MonoBehaviour holds a reference to its domain object and implements `Sync()`. `CameraBehaviour` syncs to a plain `UnityEngine.Camera` with `usePhysicalProperties = true`. DOF via URP post-processing Volume (Bokeh mode). No Cinemachine.

---

## Cross-Context Communication

No central event bus. Registries and domain objects expose `IObservable<T>` streams. Synchronous `OnNext()` delivery.

```csharp
// ElementRegistry publishes
_elementRegistry.Added.Subscribe(id => RefreshElementsPanel());

// CommandStack publishes — SceneEvaluator subscribes for re-evaluation
_commandStack.Executed.Subscribe(_ => _needsEvaluation = true);
```

All cross-context events originate from Core types (registries, CommandStack, Playhead). Context-specific events are consumed by UI and Engine, which already reference those namespaces.

---

## Key Design Patterns

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Element inheritance** | Base in Core.Common, derived in domain namespaces | Typeswitch in UI/Engine for specialized behavior |
| **Aggregate roots** | CameraElement, CharacterElement, Shot | Clean public API, `internal` internals |
| **IInterpolatable\<T\>** | Core.Common (interface), domain types implement | Timeline stays generic, domain types own their own lerp |
| **ICommand + CommandStack** | Core.Common (infrastructure), commands in each namespace | Every user action reversible, one global stack, IObservable streams |
| **CompoundCommand** | Core.Common | Wraps multi-namespace operations (move-while-recording) into one undo step |
| **Registry + IObservable** | Core.Common (ElementRegistry, ShotRegistry) | Typed queries + change notification without a central bus |

---

## Further Reading

- `design-sessions/01-shared-kernel-and-assembly-graph.md` — assembly structure decisions
- `design-sessions/01b-assembly-inventory.md` — detailed class listing per assembly/namespace
- `design-sessions/01c-class-simulation.md` — every roadmap feature traced through the structure, spaghetti risk analysis
- `design-sessions/02-evaluation-pipeline.md` — evaluation pipeline design, time model, recording, export, sync
- `domain-model.md` — modeling concepts (aggregates, value objects, registries, commands)
- `bounded-context-map.md` — every feature mapped to assembly + namespace
- `decisions.md` — all confirmed architectural decisions
