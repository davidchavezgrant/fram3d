# Assembly Simulation — Roadmap to Classes

**Date**: 2026-03-12
**Parent**: Design Session 1 — Shared Kernel & Assembly Graph
**Purpose**: Walk every roadmap feature through the assembly structure. Identify every class, trace dependencies, find spaghetti risks — both between AND within assemblies.

---

## Phase 1 — The Camera

### 1.1 Virtual Camera

**Core.Camera:**
- `CameraElement : Element` — the camera IS the rig. Properties (focal length, aperture, focus distance, DOF, shake, camera body, lens set) AND operations (`Dolly()`, `Pan()`, `Tilt()`, `Crane()`, `Truck()`, `Roll()`, `Orbit()`, `DollyZoom()`, `FocusOn()`, `Reset()`) live on one type. Follows the prior codebase lesson: unified facade, no caller reaches into sub-controllers.
- `FocalLength` — value object, 14–400mm
- `LensSystem` — FOV from focal length + sensor dimensions, anamorphic squeeze
- `CameraBody` — sensor width/height, resolution, frame rates, mount
- `LensSet` — focal lengths, prime vs zoom
- `CameraLensDatabase` — queries camera bodies and lens sets
- `DepthOfFieldSettings` — aperture, focus distance, enabled
- `ShakeGenerator` — Perlin noise rotation offset, amplitude/frequency (internal helper, CameraElement delegates to it)
- `OrbitPivot` — persistent last-focused element (Maya-style)
- `AspectRatio` — value (ratio float + display name)
- `AspectRatioSet` — ordered list, cycle forward/backward
- `ChangeFocalLengthCommand : ICommand`
- `MoveCameraCommand : ICommand`
- `ResetCameraCommand : ICommand`

**Core.Common:**
- `Element` — add `float BoundingRadius` (set by Engine on creation, used by CameraElement.FocusOn, AI camera positioning, marquee selection)

**Engine.Integration:**
- `CameraBehaviour : MonoBehaviour` — pushes CameraElement state to plain Unity Camera (physical mode) + URP post-processing Volume for DOF

**UI.Input:**
- `CameraInputHandler` — maps mouse + modifier keys to CameraElement methods

### 1.2 Camera Overlays

**Core.Common:**
- `OverlaySettings` — aspect ratio selection, guide visibility (thirds/center/safe), camera info visibility, camera path visibility. Saved with project.

**Core.Shot:**
- `Subtitle` — text, start time, end time, color, size, font
- `Shot` — add `List<Subtitle> Subtitles`

**UI.Views:**
- `AspectRatioMaskRenderer`, `CompositionGuideRenderer`, `CameraInfoOverlay`, `SubtitleRenderer`, `DirectorViewBadge`, `CameraPathBadge`, `ActiveToolBadge`

---

## Phase 2 — The Scene

### 2.1 Scene Management

**Core.Scene:**
- `Selection` — selected element(s), hover element
- `ActiveTool` — enum: Select, Translate, Rotate, Scale

**Core.Common:**
- `MoveElementCommand`, `RotateElementCommand`, `ScaleElementCommand`, `DuplicateElementCommand` — all `: ICommand`

**Core.Camera:**
- `DirectorCamera` — free camera position/rotation, not linked to any shot, not serialized

**Engine.Integration:**
- `SelectionRaycaster` — raycasts to find element under cursor, writes to Selection
- `GizmoController` — renders 3D translate/rotate/scale handles, handles drag, creates commands
- `GroundPlaneRenderer` — infinite grid, fade with distance
- `SelectionHighlighter` — renders outlines on selected/hovered elements
- `DirectorCameraBehaviour` — free camera controls, renders shot camera as frustum wireframe

### 2.2 View System

**Core.Common:**
- `ViewSettings` — current layout, per-panel view type. Saved with project.

**UI.Views:**
- `ViewLayoutManager`, `ViewPanel`, `ViewLayoutChooser`

**Engine.Integration:**
- `CameraViewRenderer`, `DirectorViewRenderer`, `DesignerViewRenderer` — each view type needs its own render camera/target

---

## Phase 3 — Time

### 3.1 Shot Structure

**Core.Shot:**
- `Shot` — name, duration, speed factor, time range, camera animations per angle, subtitles
- `ShotId` — identity wrapper
- `ShotRegistry` — ordered collection, queries, `IObservable` streams
- `AddShotCommand`, `DeleteShotCommand`, `ReorderShotCommand`, `ChangeShotDurationCommand`, `RenameShotCommand`

**Core.Timeline:**
- `GlobalTimeline` — maps ShotIds + durations to time ranges, computes global time from shot + local time + speed factor

**UI.Timeline:**
- `ShotTrackView` — shot blocks, drag-and-drop, boundary dragging with ripple/shift

**Engine.Rendering:**
- `ShotThumbnailRenderer` — captures frame at t=0

### 3.2 Keyframe Animation

**Core.Timeline:**
- `Keyframe<T>`, `KeyframeManager<T>`, `InterpolationCurve`
- `Track`, `SubTrack`
- `Playhead` — current time, playback state
- `StopwatchState` — per-track recording toggle
- `AddKeyframeCommand`, `DeleteKeyframeCommand`, `MoveKeyframeCommand`

**Core.Common:**
- `CompoundCommand` — used by Engine/UI to wrap "move element + create keyframe" into one undo step. CompoundCommand itself is generic — it holds `ICommand[]`, no cross-namespace imports needed. The code that CREATES the compound lives in Engine or UI (which reference all of Core).

**Engine.Evaluation:**
- `SceneEvaluator` — per-frame: reads Playhead → evaluates tracks → pushes results to elements + Unity transforms

**Engine.Integration:**
- `CameraPathRenderer` — 3D spline with frustum indicators at keyframe positions

**UI.Timeline:**
- `TimelineEditorView`, `TimeRulerView`, `TransportBarView`, `TrackView`, `SubTrackView`, `KeyframeMarkerView`

---

## Phase 4 — Persistence & I/O

### 4.1 Undo/Redo

**Core.Common:**
- `ICommand`, `CommandStack`, `CompoundCommand` — already designed. Commands distributed across Core namespaces.

### 4.2 Save/Load

**Core.Common:**
- `Project` — root aggregate: name, save path, frame rate, default camera body name, default lens set name, `List<SceneId>` scene identifiers, `List<ElementId>` project-level character definition IDs. Stores IDs and primitives only — does NOT import Camera or Character namespaces.
- `DirtyStateTracker` — integrates with CommandStack, tracks save point

**Services.Serialization:**
- `ProjectSerializer`, `SceneSerializer`, `VersionMigrator`, `AssetBundler`

### 4.3 Asset Import

**Core.Assets:** *(new namespace)*
- `AssetEntry` — name, source, file size, thumbnail reference
- `AssetLibrary` — collection with search, category filter, `IObservable` streams
- `AssetCategory` — enum

**Engine.Assets:**
- `RuntimeModelImporter`, `ThumbnailRenderer`, `AssetPlacementController`

**UI.Panels:**
- `AssetsPanelView`

### 4.4 Export

**Services.Export:**
- `EDLWriter`, `StoryboardLayout`, `CameraMetadataWriter`
- `ShakeBaker`, `FollowBaker`, `SnorricamBaker` — read Core types, produce keyframes

**Engine.Rendering:**
- `OfflineFrameRenderer`, `VideoEncoder`, `StoryboardRenderer`, `ImageExporter`

---

## Phase 5 — Scene Dressing

### 5.1 Lighting

**Core.Scene:**
- `LightElement : Element` — light type, intensity, color, range, cone angles
- `ColorTemperature` — value object: Kelvin ↔ RGB
- `AddLightCommand`, `ChangeLightPropertyCommand`, `ConvertLightTypeCommand`

**Engine.Integration:**
- `LightBehaviour`, `LightIconRenderer`, `ConeWireframeRenderer`

### 5.2–5.3 Assets & Environments

**Core.Assets:**
- `BuiltInAssetLibrary`, `MarketplaceAssetEntry`, `UserAssetCollection`, `AssetSearchEngine`
- `EnvironmentTemplate`, `EnvironmentLibrary`

**Engine.Assets:**
- `MarketplaceClient`, `AssetDownloadManager`, `AssetFormatConverter`, `EnvironmentLoader`

---

## Phase 6 — Characters

### 6.1 Characters

**Core.Character:**
- `CharacterElement : Element` — skeleton, current pose, body type, tint, expression, eye direction. Public API for bone access: `GetBoneWorldPosition(string boneName)`, `GetBoneWorldRotation(string boneName)`
- `BodyType` — gender, height, build sliders
- `Skeleton` — bone hierarchy (16 joints)
- `Joint` — rotation, limits, parent reference
- `Pose`, `PoseLibrary`, `PoseCategory`
- `IKSolver`, `IKChain`
- `Expression`, `BlendShape`, `ExpressionPreset`
- `EyeDirection`
- `WalkCycle`, `GaitPreset`
- `Costume`, `CostumeLibrary`
- Commands

**Engine.Integration:**
- `CharacterBehaviour`, `CharacterMeshGenerator`, `BlendShapeController`

### 6.2 Camera Follow/Watch

**Core.Camera:**
- `CameraElement` — add follow/watch methods directly:
  - `Follow(ElementId target, float distance, float height)`
  - `StopFollowing()`
  - `Watch(ElementId target)`
  - `StopWatching()`
  - Follow/watch evaluation reads `Element.Position` (Common) or `CharacterElement.GetBoneWorldPosition("head")` (Character). Same assembly. ✅
- `ActivateFollowCommand`, `ActivateWatchCommand`, `ChangeFollowParameterCommand`

### 6.3 Element Linking & Grouping

**Core.Scene:**
- `LinkEvent`, `LinkChain`, `LinkChainEvaluator`, `AnchorPoint`
- `Group`, `GroupManager`
- `CreateLinkCommand`, `RemoveLinkCommand`, `CreateGroupCommand`, `UngroupCommand`

> LinkChainEvaluator calls `CharacterElement.GetBoneWorldPosition(boneName)` for character links. Core.Scene → Core.Character. Same assembly. ✅

---

## Phases 7–12

### 7.1 Expressions — Core.Character types already listed. Expression track uses Core.Timeline generics.
### 7.2 Snorricam — CameraElement adds `MountSnorricam()`, `DismountSnorricam()`. Reads CharacterElement root motion. Same assembly. ✅
### 8.1 Multi-select — Core.Scene extends Selection to multi. Engine adds MarqueeRaycaster.
### 8.1 Grid snap — Core.Scene: SnapSettings, SnapController. Engine: SnapGridRenderer.
### 8.1 Curves — Core.Timeline: ChangeInterpolationCommand. UI: CurveEditorView.
### 8.2 Designer View — Engine: DesignerViewRenderer. UI: DesignerViewOverlay.
### 8.3 Script import — Services.ScriptImport: parsers + mapper. UI: ScriptImportDialogView.
### 8.4 Slow-motion — Core.Shot: Shot.SpeedFactor, ChangeSpeedFactorCommand.
### 9.1 Multi-camera — Core.Shot: Angle, ActiveAngleTrack, AngleSegment + commands. UI: ActiveAngleTrackView, CameraPreviewView.
### 10.1 Set builder — Core.Scene: Wall, WallChain, cutouts, RoomPreset. Engine: WallMeshGenerator. UI: SetBuilderPageView, WallDrawingToolView.
### 11.x AI — Services.AI: already listed.
### 12.x Stretch — Services.AI + Engine.Assets + Core.Character/Scene.

---

## Intra-Core Namespace Dependency Graph

```
Common ← (no deps within Core — truly foundational)
  │
  ├── Timeline ← Common
  │     │
  │     ├── Shot ← Common, Timeline, Camera
  │     │
  │     ├── Camera ← Common, Timeline
  │     │
  │     ├── Character ← Common, Timeline
  │     │     │
  │     │     └── Scene ← Common, Timeline, Character
  │     │
  │     └── Assets ← Common
```

**This is a DAG. No cycles.**

- Common is referenced by everyone, references nothing within Core
- Timeline references only Common
- Camera and Character are siblings — they both reference Common + Timeline, not each other
- Shot references Camera (Angle has CameraElement) but Camera does NOT reference Shot
- Scene references Character (LinkChainEvaluator reads bone positions) but Character does NOT reference Scene
- Assets references only Common

---

## Intra-Core Spaghetti Risk Analysis

### Risk 1: CameraElement becomes a god class

CameraElement now has: properties (focal length, aperture, DOF, shake, body, lens set), movement operations (8 methods), follow (4 methods/properties), watch (3 methods/properties), snorricam (3 methods/properties), focus. That's a lot on one type.

**Mitigation:** CameraElement is the public API — one type, one concept, "the virtual camera." Internally, it delegates to private helper classes: `ShakeGenerator`, `LensSystem`, `FollowEvaluator`, `WatchEvaluator`, `SnorricamEvaluator`. Callers see one type. Implementation is organized. This matches the prior codebase lesson: `VirtualCameraRig` was a unified facade with four sub-controllers behind it. Same pattern, no separate public types.

**Verdict:** Watch for it, but it's a code organization problem, not a structural one.

### Risk 2: Camera/Scene reaching into Character internals

Camera needs bone positions (follow targeting head, snorricam reading root motion). Scene needs bone positions (linking props to hands). The risk is Camera and Scene code reaching past CharacterElement's public API into Skeleton/Joint internals.

**Mitigation:** `CharacterElement` exposes a clean public API:
```csharp
public Vector3 GetBoneWorldPosition(string boneName) { ... }
public Quaternion GetBoneWorldRotation(string boneName) { ... }
public Vector3 RootMotionDelta { get; }
```
Camera and Scene call these methods. They never reference `Skeleton`, `Joint`, `Pose`, or `IKSolver` directly. Those are `internal` or nested private types within the Character namespace.

**Verdict:** Enforce with access modifiers. `Skeleton`, `Joint`, `IKSolver` should be `internal` to prevent accidental coupling.

### Risk 3: Timeline accumulating domain-specific interpolation

Timeline provides generic `Keyframe<T>` and evaluation. The risk is Timeline learning how to interpolate Poses, Expressions, or camera-specific types — pulling domain knowledge into infrastructure.

**Mitigation:** Domain types implement an interpolation interface:
```csharp
// Core.Common
public interface IInterpolatable<T>
{
    T Lerp(T other, float t);
}
```
`Pose : IInterpolatable<Pose>` lives in Core.Character. `Expression : IInterpolatable<Expression>` lives in Core.Character. Position lerp uses `System.Numerics.Vector3.Lerp`. Timeline calls `T.Lerp()` — it never knows what T is.

**Verdict:** Clean if the interface is established early. Timeline stays generic.

### Risk 4: Common becoming a dumping ground

Common has: Element, ElementId, registries, commands, value objects, Project, settings. As the project grows, things that "don't fit anywhere else" get tossed into Common.

**Mitigation:** Clear rule: Common contains ONLY identity types, Element base class, command infrastructure, registries, value objects, and project/settings types. If it has domain logic (camera math, posing algorithms, linking rules), it goes in the domain namespace. Review Common periodically — if a type has a natural home in Camera/Character/Scene/Shot/Timeline, move it.

**Verdict:** Discipline, not structure. No structural fix possible — Common IS the shared foundation.

### Risk 5: CompoundCommand creation crossing namespaces

Move-while-recording needs MoveElementCommand (Scene/Common) + AddKeyframeCommand (Timeline). Who creates the compound?

**Mitigation:** CompoundCommand is generic (`ICommand[]`). It lives in Common with zero domain knowledge. The code that ASSEMBLES the compound lives in Engine (GizmoController) or UI (input handler) — both reference all of Core, so they can import any command type. No namespace within Core needs to import a sibling to create compounds.

**Verdict:** No risk. The assembly structure handles this naturally.

### Risk 6: Shot ↔ Camera coupling direction

Shot contains Angles, each referencing a CameraElement. Shot → Camera. Does Camera ever need to reference Shot?

Camera follow is "per-shot" (spec says so). Shake is "per-shot." If these states are stored on CameraElement, Camera doesn't need Shot. If they're stored on Angle (which is in Shot), Camera would need to reference Shot for evaluation.

**Mitigation:** Follow/watch/shake state lives on CameraElement. Per-shot behavior is handled by the evaluation pipeline: when switching shots, Engine updates CameraElement's state from the shot's stored configuration. Camera never imports Shot.

**Verdict:** Clean IF we keep follow/watch/shake state on CameraElement, not on Angle/Shot.

---

## Intra-Services Spaghetti Risk Analysis

### Do Services namespaces reference each other?

| From | To | Needed? |
|------|-----|---------|
| AI → Serialization | No | AI doesn't save files |
| AI → Export | No | AI doesn't produce export output |
| AI → ScriptImport | No | AI parses plain text, not .fdx/.fountain files |
| Serialization → AI | No | Serialization doesn't invoke AI |
| Serialization → Export | No | Serialization saves project files, not exports |
| Export → AI | No | Export doesn't use AI |
| Export → Serialization | No | Export produces its own file formats |
| ScriptImport → AI | No | Script import is parsing, not AI generation |
| ScriptImport → Serialization | No | Script import creates domain objects, doesn't serialize them |

**All four Services namespaces are independent.** They each reference Core and nothing else within Services. Zero intra-Services coupling risk.

---

## Cross-Assembly Dependency Summary

```
                    Core (pure C#, no deps)
                   / | \  \
                  /  |  \  \
            Services |   \  \
            (→ Core) |    \  \
                  \  |     \  \
                 Engine     \  \
            (→ Svc, Core)    \  \
                    \         |  |
                     UI       |  |
              (→ Eng, Svc, Core) |
```

All arrows point downward. No cycles. ✅

---

## Findings to Fix in Assembly Inventory

1. **CameraRig folded into CameraElement.** CameraElement IS the rig — properties AND operations on one type. Internal delegation to private helpers (ShakeGenerator, LensSystem, etc.) for organization.
2. **Gizmos belong in Engine.** They're 3D scene objects needing raycasting. Move from UI.Input to Engine.Integration.
3. **Input splits:** Keyboard shortcuts + timeline/panel interaction → UI.Input. 3D scene interaction (raycasting, gizmo drags) → Engine.Integration. Marquee rectangle overlay → UI (2D screen-space).
4. **Add Core.Assets namespace.**
5. **Add `Element.BoundingRadius`.**
6. **Add `Core.Common.OverlaySettings`, `ViewSettings`, `Project`, `DirtyStateTracker`.**
7. **`IInterpolatable<T>` in Core.Common** — domain types implement their own lerp. Timeline stays generic.
8. **Character internals (`Skeleton`, `Joint`, `IKSolver`) should be `internal`** — prevent Camera/Scene from bypassing CharacterElement's public API.
9. **Follow/watch/shake state lives on CameraElement**, not on Angle/Shot — prevents Camera → Shot dependency.
10. **ShotId in Core.Common** (alongside ElementId) — prevents Common → Shot dependency. GlobalTimeline references ShotId, and GlobalTimeline should be in Common or Timeline, not in Shot.

### Wait — ShotId location revisited

GlobalTimeline maps ShotIds to time ranges. GlobalTimeline is in Core.Timeline. ShotId is currently in Core.Shot. That means Timeline → Shot, which creates a dependency we don't want (Timeline should be foundational, referenced by Shot, not the other way around).

**Fix:** Move ShotId to Core.Common alongside ElementId and KeyframeId. All identity wrappers in one place. GlobalTimeline stays in Timeline, references Common for ShotId. Shot references Timeline (for keyframe types) and Common (for ShotId). No cycles.

Updated namespace dependency graph:
```
Common ← (no deps — ElementId, ShotId, KeyframeId, Element, commands, registries, value objects)
  │
  ├── Timeline ← Common (Keyframe<T>, Track, GlobalTimeline, Playhead)
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
