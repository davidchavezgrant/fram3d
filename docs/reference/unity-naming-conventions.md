# Unity Naming Conventions

How Fram3d domain terms coexist with Unity's overlapping vocabulary in code.

**Rule: Fram3d terms are always the default.** Unqualified names in code refer to Fram3d concepts. When you need a Unity type that collides, qualify it explicitly.

---

## The Element Suffix Convention

Unity already uses `Camera`, `Light`, `Object`, and `Scene` as type names. Instead of aliasing Unity types everywhere, Fram3d scene types use an `Element` suffix:

| Domain term | C# type | Namespace | Why the suffix |
|------------|---------|-----------|---------------|
| element (a chair, bush, car) | `Element` | `Core.Common` | Base class. No Unity collision. |
| character | `CharacterElement` | `Core.Character` | Avoids ambiguity with other `Character` types. |
| light | `LightElement` | `Core.Scene` | `UnityEngine.Light` exists. |
| camera | `CameraElement` | `Core.Camera` | `UnityEngine.Camera` exists. |

The suffix is a code convention only. In specs, UI, and conversation, use the short domain terms: element, character, light, camera.

### Element inheritance

`Element` (base class) lives in `Core.Common`. Derived types live in their domain namespaces. All stored as `Element` references in `ElementRegistry`. UI and Engine typeswitch for specialized behavior:

```csharp
switch (element)
{
	case CameraElement cam   => ...,
	case CharacterElement ch => ...,
	case LightElement light  => ...,
	_                        => ...,
}
```

### CameraElement IS the rig

No separate `CameraRig` class. `CameraElement` has properties (focal length, aperture, DOF, shake, body, lens set) AND operations (`Dolly()`, `Pan()`, `Tilt()`, `Crane()`, `Truck()`, `Roll()`, `Orbit()`, `DollyZoom()`, `FocusOn()`, `Follow()`, `Watch()`, `MountSnorricam()`). Internally delegates to private helpers for organization.

### Access modifiers for aggregate boundaries

Character internals (`Skeleton`, `Joint`, `IKSolver`, `IKChain`) are `internal`. Camera and Scene access bones through `CharacterElement`'s public API only:

```csharp
// Public — Camera and Scene use these
public Vector3 GetBoneWorldPosition(string boneName) { ... }
public Quaternion GetBoneWorldRotation(string boneName) { ... }
public Vector3 RootMotionDelta { get; }

// Internal — only Core.Character code can access
internal Skeleton Skeleton { get; }
internal void SolveIK(string chain, Vector3 target) { ... }
```

---

## Other Collisions

| Fram3d term | Unity collision | Strategy |
|------------|----------------|----------|
| Scene | `UnityEngine.SceneManagement.Scene` | Fram3d's `Scene` is a namespace (`Core.Scene`). Unity's scene is infrastructure — alias it when needed: `using UnityScene = UnityEngine.SceneManagement.Scene;` |
| Transform | `UnityEngine.Transform` | Domain types use `System.Numerics.Vector3` and `Quaternion`. Unity's Transform exists only in `Fram3d.Engine`. Conversion via extension methods at the integration boundary. |
| Animation | `UnityEngine.Animation`, `Animator` | Fram3d has its own keyframe system in `Core.Timeline`. Don't reference Unity's Animation/Animator types in domain code. Using Unity's Humanoid/Mecanim for character import rig mapping is fine — the "never use" rule is about type collisions, not the mechanism. |
| Timeline | `Unity.Timeline` package | Don't use Unity's Timeline package. Fram3d's timeline is custom-built in `Core.Timeline`. |
| Asset | Unity calls Project folder contents "assets" | No collision in type names. Fram3d's asset types live in `Core.Assets`. |

---

## Namespace Structure

Four assemblies, Core organized by namespace:

```
Fram3d.Core
  ├── Common    — Element, ElementId, ShotId, KeyframeId, ICommand, CommandStack,
  │               ElementRegistry, TimePosition, FrameRate, Project, OverlaySettings
  ├── Timeline  — Keyframe<T>, KeyframeManager<T>, Track, InterpolationCurve,
  │               GlobalTimeline, Playhead, StopwatchState
  ├── Assets    — AssetEntry, AssetLibrary, EnvironmentTemplate, EnvironmentLibrary
  ├── Camera    — CameraElement, FocalLength, LensSystem, CameraBody, LensSet,
  │               AspectRatio, DepthOfFieldSettings, DirectorCamera
  ├── Character — CharacterElement, Pose, PoseLibrary, Expression, WalkCycle,
  │               Costume, BodyType, EyeDirection
  ├── Shot      — Shot, ShotRegistry, Angle, ActiveAngleTrack, Subtitle
  └── Scene     — LightElement, Selection, LinkChain, Group, SnapSettings,
                  Wall, GroundPlane, ColorTemperature

Fram3d.Services
  ├── AI            — ShotFromText, SceneFromText, CoverageSuggester
  ├── Serialization — ProjectSerializer, SceneSerializer, VersionMigrator
  ├── Export        — EDLWriter, StoryboardLayout, ShakeBaker
  └── ScriptImport  — FountainParser, FinalDraftParser, ScriptToSceneMapper

Fram3d.Engine
  ├── Integration — ElementBehaviour, CameraBehaviour, CharacterBehaviour,
  │                 LightBehaviour, GizmoController, SelectionRaycaster
  ├── Evaluation  — SceneEvaluator
  ├── Assets      — RuntimeModelImporter, ThumbnailRenderer, EnvironmentLoader
  └── Rendering   — OfflineFrameRenderer, VideoEncoder, ImageExporter

Fram3d.UI
  ├── Timeline   — TimelineEditorView, ShotTrackView, TrackView, CurveEditorView
  ├── Panels     — ElementsPanelView, PropertiesPanelView, AssetsPanelView
  ├── Views      — ViewLayoutManager, AspectRatioMaskRenderer, CameraInfoOverlay
  ├── Input      — KeyboardShortcutHandler, CameraInputHandler, MarqueeSelectionView
  └── SetBuilder — SetBuilderPageView, WallDrawingToolView
```

---

## Aliasing Convention

When a file in `Fram3d.Engine` or `Fram3d.UI` needs both Fram3d and Unity types, alias the Unity type. Never alias the Fram3d type.

```csharp
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnityVector3 = UnityEngine.Vector3;
```

Most of the time you won't need aliases at all — the `Element` suffix handles the biggest collisions, and `Core`/`Services` never import Unity types.

If you find yourself aliasing Fram3d types instead of Unity types, the dependency is going the wrong direction.

---

## Architecture Boundary Recap

Fram3d uses a split model with pure C# domain types and thin MonoBehaviour wrappers.

- **`Fram3d.Core` + `Fram3d.Services`** (pure C#): No Unity dependencies. Use `System.Numerics` for math. Testable with xUnit.
- **`Fram3d.Engine`** (Unity): Thin MonoBehaviour wrappers + evaluation pipeline. Bridges domain types to Unity's scene graph.
- **`Fram3d.UI`** (Unity): UI Toolkit presentation. Reads domain state, routes input to commands.

If a `Core` or `Services` type needs `using UnityEngine;`, something is wrong.
