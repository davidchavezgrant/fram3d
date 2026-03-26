# Research: Existing Codebase State for Shot Model (Milestone 3.1.1)

**Date**: 2026-03-26
**Purpose**: Audit all existing Core types, assembly structure, and test infrastructure to understand what exists before implementing the Shot model.

---

## Summary

The Core assembly has **5 namespaces** implemented (`Common`, `Camera`, `Scene`, `Input`, `Viewport`) with **32 source files**. None of the types needed for the Shot model exist yet — no `Core.Timeline` namespace, no `Core.Shot` namespace, no `ShotId`, `KeyframeId`, `TimePosition`, `FrameRate`, `ElementRegistry`, `ICommand`, `CommandStack`, `Keyframe<T>`, `KeyframeManager<T>`, `Track`, `GlobalTimeline`, `Playhead`, or `Shot` types. These must all be built from scratch. The existing `Element`, `ElementId`, and `CameraElement` types provide the foundation to build on.

---

## File Map

### Core Assembly (`Unity/Fram3d/Assets/Scripts/Core/`)

```
Core/
  Fram3d.Core.asmdef                    ← Pure C#, noEngineReferences: true
  Common/
    Element.cs                          ← Base class for all scene elements
    ElementId.cs                        ← GUID-based identity value object
    ElementNaming.cs                    ← Duplicate name generation logic
    GizmoScaling.cs                     ← Screen-size-invariant gizmo scaling
    StringFilter.cs                     ← Multi-word substring filter
    TransformOperations.cs              ← Pure transform math for gizmo drags
    ViewLayout.cs                       ← Sealed class: SINGLE, HORIZONTAL, VERTICAL
    ViewSlotModel.cs                    ← View slot type management with smart swap
  Camera/
    BodyController.cs                   ← Manages camera body, sensor, aspect ratio
    CameraBody.cs                       ← Camera body data (sensor, resolution, modes)
    CameraDatabase.cs                   ← Registry of camera bodies and lens sets
    CameraDatabaseParser.cs             ← JSON -> Core type parser (+ raw DTOs)
    CameraElement.cs                    ← CameraElement : Element — full camera state
    FocalLengthPresets.cs               ← Static focal length arrays
    LensController.cs                   ← Focal length, aperture, focus management
    LensSet.cs                          ← Prime/zoom lens set with per-lens specs
    LensSpec.cs                         ← Per-lens physical properties (struct)
    MovementSpeeds.cs                   ← Camera movement speed constants
    SensorMode.cs                       ← Recording mode (resolution, sensor area)
  Input/
    DragRouter.cs                       ← Routes mouse drags to camera actions
    ScrollAction.cs                     ← Scroll action result type
    ScrollRouter.cs                     ← Routes scroll to camera ops with cooldown
  Scene/
    ActiveTool.cs                       ← Sealed class: SELECT, TRANSLATE, ROTATE, SCALE
    ClickDetector.cs                    ← Click vs drag discrimination
    DragSession.cs                      ← Gizmo drag state + transform updates
    GizmoAxis.cs                        ← Sealed class: X, Y, Z, UNIFORM
    GizmoState.cs                       ← Active tool + selection reset logic
    Selection.cs                        ← Hover/selected element tracking
    ViewMode.cs                         ← Sealed class: CAMERA, DIRECTOR
  Viewport/
    AspectRatio.cs                      ← Sealed class: 10 aspect ratios + compute
    CompositionGuideSettings.cs         ← Guide visibility toggle state
    UnmaskedRect.cs                     ← Viewport rectangle (struct)
    ViewportRect.cs                     ← Normalized viewport rect computation
```

### Namespaces That Do NOT Exist Yet

```
Core/Timeline/  ← DOES NOT EXIST. Needed for Keyframe<T>, KeyframeManager<T>, Track,
                   GlobalTimeline, Playhead, TimePosition, FrameRate
Core/Shot/      ← DOES NOT EXIST. Needed for Shot, ShotId, Angle, ActiveAngleTrack, Subtitle
Core/Assets/    ← DOES NOT EXIST. Future milestone.
Core/Character/ ← DOES NOT EXIST. Future milestone.
```

---

## Core.Common Types — Full API

### Element (`Core/Common/Element.cs`)

```csharp
public class Element
{
    // Constructor
    Element(ElementId id, string name)

    // Properties
    float      BoundingRadius { get; set; }
    float      GroundOffset   { get; set; }  // prevents geometry clipping Y=0
    ElementId  Id             { get; }
    string     Name           { get; set; }
    Vector3    Position       { get; set; }  // Y clamped to max(GroundOffset, value.Y)
    Quaternion Rotation       { get; set; }  // default Identity
    float      Scale          { get; set; }  // default 1f
}
```

- Uses `System.Numerics` (Vector3, Quaternion)
- Not sealed — `CameraElement` inherits from it
- Position setter enforces Y >= GroundOffset

### ElementId (`Core/Common/ElementId.cs`)

```csharp
public sealed class ElementId : IEquatable<ElementId>
{
    ElementId(Guid value)           // rejects Guid.Empty
    Guid   Value     { get; }
    bool   Equals(ElementId other)
    int    GetHashCode()
    string ToString()
    static bool operator ==(ElementId, ElementId)
    static bool operator !=(ElementId, ElementId)
}
```

- **Pattern to follow for ShotId and KeyframeId**: sealed class wrapping Guid, rejecting empty, implementing IEquatable<T>, equality operators.

### Types That Do NOT Exist Yet in Core.Common

- `ElementRegistry` — not implemented. No registry pattern in Common yet.
- `ICommand` — not implemented. Only a comment in `GizmoController.cs` references it: `"// Future: create ICommand with before/after state here (milestone 4.1)"`.
- `CommandStack` — not implemented.
- `CompoundCommand` — not implemented.
- `TimePosition` — not implemented.
- `FrameRate` — not implemented.

---

## Core.Camera Types — Full API

### CameraElement (`Core/Camera/CameraElement.cs`)

```csharp
public class CameraElement : Element
{
    // Constants
    private const float MINIMUM_HEIGHT = 0.1f;
    private static readonly Vector3 DEFAULT_POSITION = new(0f, 1.6f, 5f);

    // Constructor
    CameraElement(ElementId id, string name)

    // Properties (read-only computed)
    AspectRatio ActiveAspectRatio
    LensSet     ActiveLensSet
    SensorMode  ActiveSensorMode
    float       Aperture
    CameraBody  Body
    bool        CanDollyZoom
    float       HorizontalFov      // radians, from sensor width + focal length
    float       SensorHeight       // mm
    float       SensorWidth        // mm
    float       VerticalFov        // radians, from sensor height + focal length

    // Properties (read-write)
    bool        DofEnabled         { get; set; }
    float       FocalLength        { get; set; }  // delegates to LensController
    bool        FocusAtInfinity    // computed
    float       FocusDistance      { get; set; }
    Vector3     OrbitPivotPoint    { get; set; }  // default Zero
    float       ShakeAmplitude     { get; set; }  // default 0.1f
    bool        ShakeEnabled       { get; set; }
    float       ShakeFrequency     { get; set; }  // default 1.0f
    bool        SnapFocalLength    { get; set; }

    // Camera movement methods
    void Crane(float amount)
    void Dolly(float amount)
    void DollyZoom(float amount)
    void Orbit(float horizontalAmount, float verticalAmount)
    void Pan(float amount)
    void Roll(float amount)
    void Truck(float amount)

    // Equipment methods
    void CycleAspectRatioBackward()
    void CycleAspectRatioForward()
    void SetBody(CameraBody body)
    void SetFocalLengthPreset(float mm)
    void SetLensSet(LensSet lensSet)
    void SetSensorMode(SensorMode mode)
    void StepApertureNarrower()
    void StepApertureWider()
    void StepFocalLengthDown()
    void StepFocalLengthUp()

    // Reset
    void Reset()
}
```

**Key for Shot model**: The Shot needs to capture camera position + rotation as keyframed curves. The existing CameraElement stores all camera state — Position (Vector3) and Rotation (Quaternion) are the properties that will be keyframed per-shot. All other camera state (focal length, aperture, DOF, lens set, body, etc.) is not keyframed in 3.1.1.

### Supporting Types

- **CameraBody**: Immutable data object — name, manufacturer, year, sensor dimensions, format, mount, native resolution, supported frame rates, sensor modes.
- **LensController**: Manages focal length (14-400mm), aperture (f/1.4-f/22 discrete stops), focus distance (0.1-100m), lens set selection. Internal state machine for prime vs zoom behavior.
- **BodyController**: Manages camera body, sensor mode, aspect ratio. Computes effective sensor dimensions.
- **LensSet**: Prime (discrete focal lengths array + per-lens LensSpec) or Zoom (continuous range). Carries anamorphic/squeeze data.
- **LensSpec**: readonly struct — FocalLength, MaxAperture, CloseFocusM.
- **SensorMode**: Recording mode — resolution, optional physical sensor area, max FPS.
- **CameraDatabase**: Registry of CameraBody + LensSet instances. Has generic defaults. Uses `AddBody()`/`AddLensSet()`/`FindBody()`/`FindLensSet()` pattern.
- **FocalLengthPresets**: Static arrays of focal lengths.
- **MovementSpeeds**: Static floats for camera movement sensitivities.

---

## Core.Timeline — DOES NOT EXIST

Per the domain model spec, `Core.Timeline` should contain:
- `Keyframe<T>` — generic keyframe storing a value at a time position
- `KeyframeManager<T>` — manages an ordered list of keyframes with interpolation
- `Track` — named animation track owning a KeyframeManager
- `GlobalTimeline` — the single global timeline spanning all shots
- `Playhead` — current playback position
- `TimePosition` — value object wrapping time (rejects negative)
- `FrameRate` — value object for frame rate

None of these exist. The namespace directory does not exist.

---

## Core.Shot — DOES NOT EXIST

Per the domain model spec, `Core.Shot` should contain:
- `Shot` — the aggregate root. Owns name, duration, camera animation, time range on global timeline.
- `ShotId` — identity value object (like ElementId)
- `Angle` — camera angle within a shot (future multi-camera, 9.1)
- `ActiveAngleTrack` — which angle is active
- `Subtitle` — shot subtitle text

None of these exist. The namespace directory does not exist.

---

## Assembly Definitions

### Fram3d.Core (`Fram3d.Core.asmdef`)
- `noEngineReferences: true` — pure C#, no Unity dependencies
- No assembly references — standalone
- Root namespace: `Fram3d.Core`

### Fram3d.Engine (`Fram3d.Engine.asmdef`)
- References: `Fram3d.Core`, `Unity.InputSystem`, `Unity.RenderPipelines.Core.Runtime`, `Unity.RenderPipelines.Universal.Runtime`
- `noEngineReferences: false` — uses Unity engine

### Fram3d.UI (`Fram3d.UI.asmdef`)
- References: `Fram3d.Core`, `Fram3d.Engine`, `Unity.InputSystem`
- Top of the stack

### Fram3d.Editor (`Fram3d.Editor.asmdef`)
- References: `Fram3d.Core`, `Fram3d.Engine`, `Fram3d.UI`
- Editor-only platform

---

## Test Infrastructure

### xUnit Tests (`tests/Fram3d.Core.Tests/`)

**Project file**: `Fram3d.Core.Tests.csproj`
- Target: `net9.0`, LangVersion: `9.0`
- References: `Fram3d.Core.csproj` (which compiles Core sources via glob)
- Packages: `xunit 2.*`, `FluentAssertions 7.*`, `Microsoft.NET.Test.Sdk 17.*`
- Run: `dotnet test tests/Fram3d.Core.Tests`

**Compilation project**: `tests/Fram3d.Core/Fram3d.Core.csproj`
- Compiles all Core sources via: `<Compile Include="../../Unity/Fram3d/Assets/Scripts/Core/**/*.cs" LinkBase="Core" />`
- Any new `.cs` files added to `Core/` subdirectories are automatically included.

**Existing test files** (27 test files):
```
Camera/
  CameraBodyTests.cs
  CameraDatabaseParserTests.cs
  CameraDatabaseTests.cs
  CameraElementTests.cs
  FocalLengthPresetsTests.cs
  LensSetTests.cs
  SensorModeTests.cs
Common/
  ElementIdTests.cs
  ElementNamingTests.cs
  ElementTests.cs
  GizmoScalingTests.cs
  StringFilterTests.cs
  TransformOperationsTests.cs
  ViewSlotModelTests.cs
Input/
  DragRouterTests.cs
  ScrollRouterTests.cs
Scene/
  ActiveToolTests.cs
  ClickDetectorTests.cs
  DragSessionTests.cs
  GizmoAxisTests.cs
  GizmoStateTests.cs
  SelectionTests.cs
Viewport/
  AspectRatioTests.cs
  CompositionGuideSettingsTests.cs
  ViewportRectTests.cs
```

**Stryker mutation testing**: Configured at `tests/Fram3d.Core.Tests/stryker-config.json`
- Mutation level: Complete
- Thresholds: high=85%, low=75%, break=60%
- Mutates: `../../Unity/Fram3d/Assets/Scripts/Core/**/*.cs`
- Ignores: string mutations

### Play Mode Tests (`Unity/Fram3d/Assets/Tests/PlayMode/`)

**Assembly**: `Fram3d.PlayMode.Tests.asmdef`
- References: `Fram3d.Core`, `Fram3d.Engine`, `Fram3d.UI`, Input System, URP, Test Runner
- NUnit framework, `UNITY_INCLUDE_TESTS` define constraint

**Existing test files** (17 test files):
```
Engine/
  CameraBehaviourTests.cs
  CameraDatabaseLoaderTests.cs
  ElementBehaviourTests.cs
  ElementDuplicatorTests.cs
  GizmoControllerTests.cs
  GizmoMeshBuilderTests.cs
  SelectionHighlighterTests.cs
  SelectionRaycasterTests.cs
  VectorExtensionsTests.cs
  ViewCameraManagerTests.cs
UI/
  AspectRatioMaskViewTests.cs
  CameraInputHandlerTests.cs
  CompositionGuideViewTests.cs
  CursorBehaviourTests.cs
  PropertiesPanelViewTests.cs
  SearchableDropdownTests.cs
  SelectionInputHandlerTests.cs
```

---

## Patterns to Follow

### Identity Value Objects
`ElementId` is the template: sealed class, wraps Guid, rejects Empty in constructor, implements `IEquatable<T>`, equality operators, ToString. Copy this pattern for `ShotId` and `KeyframeId`.

### Sealed Class Enum Pattern
Used extensively: `ActiveTool`, `ViewMode`, `ViewLayout`, `AspectRatio`, `GizmoAxis`, `ClickResultKind`, `DragActionKind`, `ScrollActionKind`. Private constructor + static readonly instances. Used where C# enums would normally go but each instance carries typed data.

### Domain Modeling
- `System.Numerics` for all math types (not Unity math)
- Value objects for domain concepts (not raw primitives)
- Internal controllers for complex state (`LensController`, `BodyController`) behind an aggregate root facade (`CameraElement`)
- No Unity dependencies anywhere in Core

### Test Conventions
- Test file naming: `{TypeName}Tests.cs`
- Test method naming: `MethodName__ExpectedBehavior__When__Condition`
- xUnit + FluentAssertions for Core tests
- NUnit for Play Mode tests
- Mirror directory structure: tests mirror the namespace structure of the source

---

## What Needs to Be Built for 3.1.1

Based on the spec and domain model, the Shot model implementation needs these new types:

### Core.Common (new types in existing namespace)
- `TimePosition` — value object wrapping double seconds, rejects negative
- `FrameRate` — value object (e.g., 24fps)

### Core.Timeline (new namespace)
- `Keyframe<T>` — generic keyframe: KeyframeId + TimePosition + T value
- `KeyframeManager<T>` — ordered list of keyframes, evaluation at a time position

### Core.Shot (new namespace)
- `ShotId` — identity value object (follow ElementId pattern)
- `Shot` — aggregate root: ShotId, name, duration, camera position/rotation keyframes, time range on global timeline

### Tests
- New test files in `tests/Fram3d.Core.Tests/Common/` for TimePosition, FrameRate
- New test directory `tests/Fram3d.Core.Tests/Timeline/` for keyframe types
- New test directory `tests/Fram3d.Core.Tests/Shot/` for Shot types
- All automatically included via the glob pattern in `Fram3d.Core.csproj`

---

## Key References

- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Common/Element.cs` — base class Shot's camera inherits from
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Common/ElementId.cs` — pattern for ShotId/KeyframeId
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs` — full camera state; Position + Rotation are the keyframed properties
- `/Users/davidgrant/Code/projects/fram3d/Unity/Fram3d/Assets/Scripts/Core/Fram3d.Core.asmdef` — assembly def (noEngineReferences: true)
- `/Users/davidgrant/Code/projects/fram3d/tests/Fram3d.Core/Fram3d.Core.csproj` — glob-includes all Core/**/*.cs
- `/Users/davidgrant/Code/projects/fram3d/tests/Fram3d.Core.Tests/Fram3d.Core.Tests.csproj` — xunit + FluentAssertions 7.*
- `/Users/davidgrant/Code/projects/fram3d/docs/specs/milestone-3.1-shot-sequencer-spec.md` — full spec
- `/Users/davidgrant/Code/projects/fram3d/docs/reference/domain-model.md` — namespace ownership and aggregate rules
