# Unity Naming Conventions

How Fram3d domain terms coexist with Unity's overlapping vocabulary in code.

**Rule: Fram3d terms are always the default.** Unqualified names in code refer to Fram3d concepts. When you need a Unity type that collides, qualify it explicitly.

---

## The Element Suffix Convention

Unity already uses `Camera`, `Light`, `Object`, and `Scene` as type names. Instead of aliasing Unity types everywhere, Fram3d scene types use an `Element` suffix:

| Domain term | C# type | Why the suffix |
|------------|---------|---------------|
| element (a chair, bush, car) | `Element` | Base class. No Unity collision. |
| character | `CharacterElement` | Avoids ambiguity with other `Character` types. |
| light | `LightElement` | `UnityEngine.Light` exists. |
| camera | `CameraElement` | `UnityEngine.Camera` exists. |

The suffix is a code convention only. In specs, UI, and conversation, use the short domain terms: element, character, light, camera.

---

## Other Collisions

| Fram3d term | Unity collision | Strategy |
|------------|----------------|----------|
| Scene | `UnityEngine.SceneManagement.Scene` | Fram3d's `Scene` is a domain type. Unity's scene is infrastructure — alias it when needed: `using UnityScene = UnityEngine.SceneManagement.Scene;` |
| Transform | `UnityEngine.Transform` | Don't wrap it. Use Unity's Transform directly. The engine concept and the domain concept are the same thing. |
| Animation | `UnityEngine.Animation`, `Animator` | Fram3d has its own keyframe system. Never use Unity's Animation/Animator. No collision in practice — Fram3d types have distinct names (`CameraAnimation`, `ElementAnimation`). |
| Timeline | `Unity.Timeline` package | Don't use Unity's Timeline package. Fram3d's timeline is custom-built. |
| Asset | Unity calls Project folder contents "assets" | No collision in type names. Fram3d's asset types live in `Fram3d.Assets`. |

---

## Namespace Structure

Assembly definitions per bounded context. Within each, Fram3d types own the unqualified names:

```
Fram3d.Camera       — ICameraState, CameraBody, LensSet, FocalLength
Fram3d.Sequencing   — Shot, Angle, Track, Keyframe, Playhead
Fram3d.Scene        — Element, CharacterElement, LightElement, CameraElement, Selection
Fram3d.Viewport     — View, Layout, CameraView, DirectorView, DesignerView
Fram3d.Characters   — Pose, Expression, Skeleton, BodyRegion
Fram3d.Persistence  — Project, SceneFile
Fram3d.Assets       — AssetLibrary, AssetEntry
Fram3d.Export        — Exporter, BurnIn
Fram3d.AI           — ShotDescription, BlockingSuggestion
```

---

## Aliasing Convention

When a file needs both Fram3d and Unity types, alias the Unity type. Never alias the Fram3d type.

```csharp
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace Fram3d.Persistence
{
	public class SceneSerializer
	{
		// "Scene" here is Fram3d's Scene — the default
		public void Save(Scene scene, UnityScene unityScene)
		{
			// ...
		}
	}
}
```

Most of the time you won't need aliases at all — the `Element` suffix handles the biggest collisions. Aliasing is mainly needed for `Scene` in the persistence layer.

If you find yourself aliasing Fram3d types instead of Unity types, the dependency is going the wrong direction.

---

## Architecture Boundary Recap

From the architecture doc: Fram3d uses a split model with pure C# domain types and thin MonoBehaviour wrappers.

- **Pure C# domain layer** (`Shot`, `CameraAnimation`, `Keyframe`, `CharacterElement`, `Pose`, value objects): No Unity dependencies. These types use Fram3d names exclusively and never reference `UnityEngine.*`.
- **MonoBehaviour integration layer**: Thin wrappers that bridge domain types to Unity. This is the ONLY place where Unity types and Fram3d types coexist.

If a domain type needs `using UnityEngine;`, something is wrong.
