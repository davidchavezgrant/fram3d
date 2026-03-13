# Design Session 1: Shared Kernel & Assembly Graph

**Date**: 2026-03-12
**Status**: Decided
**Feeds into**: `architecture.md`, `bounded-context-map.md`, `unity-naming-conventions.md`

---

## The Problem

The original bounded context map had 9 separate assemblies (Camera, Sequencing, Scene, Viewport, Characters, Persistence, Assets, Export, AI). In practice they have deep cross-dependencies — camera follow needs character positions, linking needs bone transforms, undo wraps operations from every context, animation tracks control properties across all element types.

---

## Decisions

### Four assemblies, layered

```
Fram3d.Core      — pure C#, no dependencies
Fram3d.Services  — pure C#  → Core
Fram3d.Engine    — Unity    → Services, Core
Fram3d.UI        — Unity    → Engine, Services, Core
```

Each layer references only downward. Three compile-time boundaries:
1. **Core** can't touch Unity — testability with xUnit
2. **Services** can't touch Unity — orchestration logic stays pure
3. **Engine** can't reference UI — integration code is independent of presentation

### Core: one assembly, namespace-organized

All domain logic in one assembly. Camera code can call `character.GetBoneWorldPosition("head")` directly — no indirection, no pipeline ferry. Namespaces provide organization; the compiler doesn't enforce internal boundaries. Discipline enforced via access modifiers (Character internals like `Skeleton`, `Joint`, `IKSolver` are `internal`).

**Intra-Core namespace dependency graph (DAG, no cycles):**

```
Common ← (no deps — identity types, Element, commands, registries, value objects, settings)
  │
  ├── Timeline ← Common
  │
  ├── Assets ← Common
  │
  ├── Camera ← Common, Timeline
  │
  ├── Character ← Common, Timeline
  │
  ├── Shot ← Common, Timeline, Camera
  │
  └── Scene ← Common, Timeline, Character
```

- Common is foundational — referenced by everything, references nothing within Core
- Timeline provides generic keyframe evaluation — no domain knowledge
- Camera and Character are siblings — both reference Common + Timeline, not each other
- Shot → Camera (Angle references CameraElement) but Camera does NOT reference Shot
- Scene → Character (LinkChainEvaluator reads bone positions) but Character does NOT reference Scene

### Element specialization — inheritance, derived types in domain namespaces

```
Core.Common:    Element (base — identity, name, transform, bounding radius)
Core.Camera:    CameraElement : Element (operations + properties — the camera IS the rig)
Core.Character: CharacterElement : Element (skeleton, pose, expression — public bone API)
Core.Scene:     LightElement : Element (intensity, color, range, cone)
```

UI typeswitches on element types for icons and property panels. Engine typeswitches for MonoBehaviour wrapper creation. Both reference all Core namespaces.

### CameraElement IS the rig

No separate CameraRig class. CameraElement has properties (focal length, aperture, DOF, shake, body, lens set) AND operations (`Dolly()`, `Pan()`, `Tilt()`, `Crane()`, `Truck()`, `Roll()`, `Orbit()`, `DollyZoom()`, `FocusOn()`, `Follow()`, `Watch()`, `MountSnorricam()`). Internally delegates to private helpers (ShakeGenerator, LensSystem) for organization. Follows prior codebase lesson: unified facade, callers see one type.

### Transforms on Element using System.Numerics

```csharp
using System.Numerics;

public class Element
{
	public ElementId Id { get; }
	public string Name { get; set; }
	public Vector3 Position { get; set; }
	public Quaternion Rotation { get; set; }
	public float Scale { get; set; }
	public float BoundingRadius { get; set; }  // set by Engine on creation
}
```

Core stays Unity-free. Conversion at the integration boundary via extension methods.

### Registries for shared data, plain collections for private data

`ElementRegistry` and `ShotRegistry` in Core.Common with typed queries and `IObservable` change streams. Context-private lookups (e.g., Camera's `ElementId → CameraElement` map) use plain collections internally.

### Command infrastructure in Core

`ICommand`, `CommandStack`, `CompoundCommand` in Core.Common. Each namespace defines its own commands. CompoundCommand is generic (`ICommand[]`) — the code that assembles compounds (move-while-recording) lives in Engine or UI, which reference all of Core.

### Cross-context communication via IObservable on the source

No central event bus. Registries expose `IObservable<T>` streams directly. Synchronous `OnNext()` delivery. No sibling context needs to react to another sibling's events — all cross-context events originate from Core registries. Context-specific events are consumed by UI and Engine, which already reference those namespaces.

### Timeline is data structures, not a controller

Timeline provides `Keyframe<T>`, `KeyframeManager<T>`, `Track`, `InterpolationCurve`, `Playhead`. It answers "given keyframes and time T, what's the interpolated value?" It doesn't know what T represents. The evaluation pipeline in `Engine.Evaluation.SceneEvaluator` orchestrates: reads Playhead → evaluates tracks → writes results to elements → pushes to Unity. Domain types implement `IInterpolatable<T>` for type-specific lerp (Pose interpolation in Character, etc.). Timeline stays generic.

### Follow/watch/shake state lives on CameraElement

Not on Angle or Shot. Prevents Camera → Shot dependency. Per-shot behavior handled by Engine: when switching shots, Engine updates CameraElement's state from the shot's stored configuration.

### Identity types in Common

`ElementId`, `ShotId`, `KeyframeId` all in Core.Common alongside each other. Prevents GlobalTimeline (in Timeline) from needing to reference Shot for ShotId.

### Sequencing context dissolved

Timeline data in Core. Timeline UI in UI. Feature map entries (3.1, 3.2, 8.4) map to Core (data) + UI (presentation).

---

## Cross-Assembly Dependency Graph

```
Fram3d.Core (pure C#, no deps)
  │
  ├── Fram3d.Services → Core
  │
  ├── Fram3d.Engine → Services, Core
  │
  └── Fram3d.UI → Engine, Services, Core
```

All arrows point downward. No cycles. See `01b-assembly-inventory.md` for detailed contents of each assembly. See `01c-class-simulation.md` for full class trace across all 12 roadmap phases, intra-Core dependency analysis, and spaghetti risk assessment.

---

## Spaghetti Risks Identified and Mitigated

| Risk | Mitigation |
|------|-----------|
| CameraElement becomes a god class | Unified public API, internal delegation to private helpers (ShakeGenerator, LensSystem, etc.) |
| Camera/Scene reaching into Character internals (Skeleton, Joint) | CharacterElement exposes clean public API (`GetBoneWorldPosition`). Skeleton/Joint/IKSolver are `internal`. |
| Timeline accumulating domain-specific interpolation logic | Domain types implement `IInterpolatable<T>`. Timeline calls `T.Lerp()`, never knows what T is. |
| Common becoming a dumping ground | Clear rule: identity types, Element base, commands, registries, value objects, project/settings only. Domain logic goes in domain namespaces. |
| CompoundCommand creation crossing namespaces | CompoundCommand is generic `ICommand[]`. Assembly code (Engine/UI) creates compounds — they reference all of Core. |

---

## What Changes in Existing Docs

| Doc | Change |
|-----|--------|
| `domain-model.md` | Replace 9-context table with 4-assembly model. Remove Sequencing. Add Core namespace structure. Acknowledge registries. |
| `bounded-context-map.md` | Replace with assembly + namespace map. Reassign all features to Core namespaces + Services/Engine/UI. |
| `unity-naming-conventions.md` | Update namespace structure. Element in Core.Common, derived types in domain namespaces. Add `internal` guidance for Character internals. |
| `decisions.md` | Record: 4 assemblies, Element inheritance, System.Numerics, registries, commands in Core, IObservable, CameraElement=rig, Timeline is data not controller. |
| `architecture.md` | Populate with assembly graph, namespace DAG, split model, evaluation pipeline ownership. |
| `tech-stack.md` | Already updated (UI Toolkit). |
