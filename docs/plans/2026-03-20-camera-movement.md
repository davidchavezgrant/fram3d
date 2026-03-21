# Implementation Plan: 1.1.1 Camera Movement

**Date**: 2026-03-20
**Milestone**: 1.1.1 Camera Movement
**Spec**: `docs/specs/milestone-1.1-virtual-camera-spec.md` §1.1.1
**Status**: Draft

---

## Summary

Pan, tilt, dolly, truck, crane, roll, orbit, dolly zoom, and reset — the full vocabulary of physical camera movement, mapped to mouse and modifier keys. This is the first feature implemented. Since the project is greenfield (no application code exists), this plan also bootstraps the foundational types and assembly structure that all future features build on.

## Scope

**In scope:**
- Assembly structure scaffolding (Core, Engine, UI — not Services yet)
- Foundation types: `Element`, `ElementId`, `CameraElement`
- All 8 movement operations + reset on `CameraElement` (pure C#, `System.Numerics`)
- Per-movement configurable speed multipliers
- Framerate-independent movement
- Composable movements (multiple inputs in one frame sum naturally)
- No collision detection (camera passes through everything)
- `CameraBehaviour` MonoBehaviour bridging domain → Unity camera
- `CameraInputHandler` routing mouse/keyboard to domain operations
- Default position at (0, 1.6, -5) facing forward
- Orbit pivot using persistent last-focus model (stub — actual focus comes in 1.1.4)
- Dolly zoom with optional element lock (stub — element selection comes in 2.1.1)
- xUnit tests for all movement operations on `CameraElement`

**Out of scope:**
- Lens system (1.1.2), camera body database (1.1.3), focus (1.1.4), DOF (1.1.5), shake (1.1.6)
- Overlays, timeline, shots, keyframes
- Command pattern / undo (scaffolded later per decisions.md)

## Architecture

All application code lives inside the Unity project at `Unity/Fram3d/Assets/Scripts/`, organized by assembly. Tests live outside Unity as a standalone .NET project at `tests/Fram3d.Core.Tests/` that compiles Core's source files directly via `<Compile Include>`.

```
Unity/Fram3d/Assets/Scripts/
  Core/                          ← Fram3d.Core assembly (pure C#, System.Numerics)
    Fram3d.Core.asmdef
    Common/   Element, ElementId
    Camera/   CameraElement, MovementSpeeds

  Engine/                        ← Fram3d.Engine assembly (Unity, references Core)
    Fram3d.Engine.asmdef
    Integration/  CameraBehaviour
    Evaluation/   SceneEvaluator (minimal — just camera sync for now)
    Conversion/   VectorExtensions (System.Numerics ↔ UnityEngine)

  UI/                            ← Fram3d.UI assembly (Unity, references Core + Engine)
    Fram3d.UI.asmdef
    Input/  CameraInputHandler

tests/Fram3d.Core.Tests/         ← xUnit + FluentAssertions, standalone .NET project
  Fram3d.Core.Tests.csproj        (includes Core source files, no Unity dependency)
  Camera/  CameraElementTests
```

### Key Design Decisions

- **CameraElement IS the rig** (per decisions.md). No separate CameraRig class. Movement operations are methods on CameraElement. Internally delegates to private helpers for organization.
- **System.Numerics for domain math.** `Vector3` and `Quaternion` from `System.Numerics`, not Unity. Conversion at the Engine boundary via extension methods.
- **Orbit pivot as stored state.** CameraElement stores `OrbitPivotPoint` (Vector3). Updated when focus completes (1.1.4). For now, defaults to world origin.
- **Dolly zoom stores reference distance.** `DollyZoomReferenceDistance` (float) and optional `DollyZoomTargetId` (ElementId?). Without lens system, dolly zoom translates the camera but the focal length change is a no-op stub.
- **No `deltaTime` inside domain.** Movement methods accept a `float amount` (already scaled by deltaTime and speed). The Engine/UI layer applies `Time.deltaTime * speed * rawInput`.

---

## Phases

### Phase 1: Assembly scaffolding + foundation types

**Goal:** Project compiles with the 4-assembly structure and test project. Types exist, nothing moves yet.

#### Task 1.1 — Assembly definitions

Create Unity assembly definition files establishing the dependency graph:

| Assembly | References |
|----------|-----------|
| `Fram3d.Core.asmdef` | None (pure C#) |
| `Fram3d.Engine.asmdef` | Fram3d.Core |
| `Fram3d.UI.asmdef` | Fram3d.Core, Fram3d.Engine |

**Files:**
- `Unity/Fram3d/Assets/Scripts/Core/Fram3d.Core.asmdef`
- `Unity/Fram3d/Assets/Scripts/Engine/Fram3d.Engine.asmdef`
- `Unity/Fram3d/Assets/Scripts/UI/Fram3d.UI.asmdef`

Assembly definitions must:
- Set `Fram3d.Core` to NOT auto-reference Unity assemblies and override references to include only `System.Numerics`
- Set `Fram3d.Engine` to reference `Fram3d.Core` + Unity assemblies + `Unity.InputSystem`
- Set `Fram3d.UI` to reference `Fram3d.Core`, `Fram3d.Engine` + Unity assemblies + `Unity.InputSystem`

> **Note:** Unity asmdef files don't directly control `System.Numerics` — Core stays Unity-free by convention. The asmdef prevents Core from accidentally referencing Engine/UI.

#### Task 1.2 — xUnit test project setup

The test project is an external .NET project that compiles Core's source files directly — no Unity dependency. This lets us run `dotnet test` from the command line against pure C# domain code.

Set up `tests/Fram3d.Core.Tests/Fram3d.Core.Tests.csproj`:
- `<Compile Include="../../Unity/Fram3d/Assets/Scripts/Core/**/*.cs" />` to include all Core source files
- `<PackageReference>` for xUnit, FluentAssertions, xunit.runner.visualstudio
- Target `net9.0`
- A single placeholder test to verify the pipeline works: `dotnet test` passes

This approach keeps one copy of the source files (inside the Unity project at `Unity/Fram3d/Assets/Scripts/Core/`) while compiling them both as a Unity asmdef assembly and as a standalone .NET assembly for testing.

#### Task 1.3 — Element base class + ElementId

`Unity/Fram3d/Assets/Scripts/Core/Common/ElementId.cs`:
```csharp
// Value object wrapping Guid. Rejects Guid.Empty. Full equality.
// readonly struct, not record struct (C# 9 — record struct is C# 10).
public readonly struct ElementId : IEquatable<ElementId>
{
    public Guid Value { get; }

    public ElementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ElementId cannot be empty");
        Value = value;
    }

    public static ElementId New() => new(Guid.NewGuid());
    public bool Equals(ElementId other) => Value.Equals(other.Value);
    public override bool Equals(object obj) => obj is ElementId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(ElementId left, ElementId right) => left.Equals(right);
    public static bool operator !=(ElementId left, ElementId right) => !left.Equals(right);
}
```

`Unity/Fram3d/Assets/Scripts/Core/Common/Element.cs`:
```csharp
// Base class for all scene elements.
public class Element
{
    public ElementId Id { get; }
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public float Scale { get; set; } = 1f;
    public float BoundingRadius { get; set; }
}
```

**Tests:**
- `ElementId__ThrowsArgumentException__When__GuidIsEmpty`
- `ElementId__EqualsAnother__When__SameGuid`
- `ElementId__IsUnique__When__CreatedWithNew`

#### Task 1.4 — CameraElement shell

`Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs`:
- Inherits `Element`
- Properties: `FocalLength` (float, default 50), `OrbitPivotPoint` (Vector3, default origin)
- Stub movement methods (just signatures, implementations in Phase 2)
- Constructor sets default position `(0, 1.6f, -5)` and identity rotation

**Tests:**
- `CameraElement__HasDefaultPosition__When__Constructed`
- `CameraElement__HasDefaultFocalLength__When__Constructed`

#### Task 1.5 — Vector conversion extensions

`Unity/Fram3d/Assets/Scripts/Engine/Conversion/VectorExtensions.cs`:
```csharp
public static class VectorExtensions
{
    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v) => ...;
    public static System.Numerics.Vector3 ToSystem(this UnityEngine.Vector3 v) => ...;
    public static UnityEngine.Quaternion ToUnity(this System.Numerics.Quaternion q) => ...;
    public static System.Numerics.Quaternion ToSystem(this UnityEngine.Quaternion q) => ...;
}
```

**Verification:** `dotnet test` passes. Unity project compiles. Assembly references are correct (Core has no Unity imports).

---

### Phase 2: Camera movement operations (domain)

**Goal:** All 8 movement types + reset implemented as pure C# methods on `CameraElement`, fully tested.

All movement methods accept a `float amount` — the pre-scaled input value. The caller (Engine/UI) is responsible for applying `deltaTime * speedMultiplier * rawInput`. This keeps the domain framerate-agnostic and trivially testable.

#### Task 2.1 — MovementSpeeds value object

`Unity/Fram3d/Assets/Scripts/Core/Camera/MovementSpeeds.cs`:
```csharp
public record MovementSpeeds
{
    public float Dolly { get; init; } = 0.01f;
    public float PanTilt { get; init; } = 0.2f;
    public float Roll { get; init; } = 0.03f;
    public float Crane { get; init; } = 0.02f;
    public float Truck { get; init; } = 0.02f;
    public float DollyZoom { get; init; } = 0.5f;
}
```

Add `MovementSpeeds Speeds { get; set; }` to `CameraElement` with defaults from tuned-constants.md.

#### Task 2.2 — Pan and Tilt

**Pan:** Rotate around world Y axis through camera position.
```
Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, amount) * Rotation
```

**Tilt:** Rotate around camera's local right axis.
```
var right = Vector3.Transform(Vector3.UnitX, Rotation)
Rotation = Quaternion.CreateFromAxisAngle(right, amount) * Rotation
```

**Tests:**
- `Pan__RotatesAroundWorldY__When__PositiveAmount` — verify position unchanged, yaw increased
- `Pan__RotatesRightward__When__PositiveAmount`
- `Tilt__RotatesAroundLocalRight__When__PositiveAmount` — verify position unchanged, pitch changed
- `Tilt__RotatesUpward__When__PositiveAmount`
- `Pan__DoesNotChangePosition__When__AnyAmount`
- `Tilt__DoesNotChangePosition__When__AnyAmount`

#### Task 2.3 — Dolly and Truck

**Dolly:** Translate along camera's local forward (Z) axis.
```
var forward = Vector3.Transform(-Vector3.UnitZ, Rotation)  // Unity convention: forward is -Z
Position += forward * amount
```

> **Note:** System.Numerics uses right-hand coordinates where forward is -Z. Need to verify Unity convention mapping here. The conversion extensions handle the coordinate system difference.

**Truck:** Translate along camera's local right (X) axis.
```
var right = Vector3.Transform(Vector3.UnitX, Rotation)
Position += right * amount
```

**Tests:**
- `Dolly__TranslatesAlongLocalForward__When__PositiveAmount`
- `Dolly__DoesNotChangeRotation__When__AnyAmount`
- `Truck__TranslatesAlongLocalRight__When__PositiveAmount`
- `Truck__DoesNotChangeRotation__When__AnyAmount`
- `Dolly__MovesTowardScene__When__CameraFacesForward` (spec example)

#### Task 2.4 — Crane

**Crane:** Translate along world Y axis (not camera-relative).
```
Position += Vector3.UnitY * amount
```

**Tests:**
- `Crane__TranslatesAlongWorldY__When__PositiveAmount`
- `Crane__MovesWorldUp__When__CameraTiltedDown` (spec: crane is world-relative, not camera-relative)
- `Crane__DoesNotChangeRotation__When__AnyAmount`

#### Task 2.5 — Roll

**Roll:** Rotate around camera's local forward axis.
```
var forward = Vector3.Transform(-Vector3.UnitZ, Rotation)
Rotation = Quaternion.CreateFromAxisAngle(forward, amount) * Rotation
```

**Tests:**
- `Roll__RotatesAroundLocalForward__When__PositiveAmount`
- `Roll__DoesNotChangePosition__When__AnyAmount`
- `Roll__TiltsFrameClockwise__When__PositiveAmount`

#### Task 2.6 — Orbit

**Orbit:** Rotate camera around `OrbitPivotPoint` while keeping the pivot centered.
```
// Calculate current offset from pivot
var offset = Position - OrbitPivotPoint
// Rotate offset by horizontal (world Y) and vertical (camera right) amounts
var horizontalRot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, horizontalAmount)
var right = Vector3.Transform(Vector3.UnitX, Rotation)
var verticalRot = Quaternion.CreateFromAxisAngle(right, verticalAmount)
var combinedRot = horizontalRot * verticalRot
// Apply rotated offset
Position = OrbitPivotPoint + Vector3.Transform(offset, combinedRot)
// Rotate camera to keep looking at pivot
Rotation = combinedRot * Rotation
```

Method signature: `Orbit(float horizontalAmount, float verticalAmount)`

**Tests:**
- `Orbit__MaintainsPivotInFrame__When__RotatingHorizontally`
- `Orbit__MaintainsDistanceToPivot__When__Orbiting`
- `Orbit__UsesStoredPivotPoint__When__PivotWasSet`
- `Orbit__UsesWorldOrigin__When__NoPivotSet`
- `Orbit__RotatesBothAxes__When__BothAmountsProvided`

#### Task 2.7 — Dolly zoom (stub)

Dolly zoom requires focal length changes (1.1.2) and element positions (2.1.1). For now, implement the translation half — the focal length half is a stub that does nothing.

```
DollyZoom(float amount):
    // Translate along forward axis to maintain subject size
    // (full implementation in 1.1.2 will also adjust FocalLength)
    var forward = Vector3.Transform(-Vector3.UnitZ, Rotation)
    Position += forward * amount
    // TODO 1.1.2: Adjust FocalLength to compensate
```

Store `DollyZoomReferenceDistance` (float) for use when the lens system arrives.

**Tests:**
- `DollyZoom__TranslatesAlongForward__When__Called`
- `DollyZoom__StoresReferenceDistance__When__Initialized`
- `DollyZoom__UsesFixedDistance__When__NoTargetElement` (spec error case)

#### Task 2.8 — Reset

```
Reset():
    Position = DefaultPosition   // (0, 1.6, -5)
    Rotation = Quaternion.Identity
    FocalLength = 50f
    OrbitPivotPoint = Vector3.Zero
```

**Tests:**
- `Reset__RestoresDefaultPosition__When__CameraWasMoved`
- `Reset__RestoresDefaultRotation__When__CameraWasRotated`
- `Reset__RestoresDefaultFocalLength__When__FocalLengthChanged`

#### Task 2.9 — Movement composition test

Verify that applying multiple movements in sequence produces the sum of both:
- `Compose__ProducesSumOfMotions__When__PanAndDollyApplied` — apply pan then dolly, verify result equals independent application

**Verification:** All `dotnet test` pass. Every spec behavior from §1.1.1 has a corresponding test.

---

### Phase 3: Engine integration

**Goal:** CameraElement drives a real Unity camera. You can see the default view in the Game window.

#### Task 3.1 — CameraBehaviour

`Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs`:
- MonoBehaviour on a GameObject with `UnityEngine.Camera`
- Holds a reference to `CameraElement` (domain object)
- `LateUpdate()`: syncs `CameraElement.Position` → `transform.position`, `CameraElement.Rotation` → `transform.rotation` using conversion extensions
- Sets `camera.usePhysicalProperties = true`
- Sets `camera.focalLength` from `CameraElement.FocalLength`
- Sets `camera.sensorSize` to default Super 35 for now (sensor from body comes in 1.1.3)

#### Task 3.2 — SceneEvaluator (minimal)

`Unity/Fram3d/Assets/Scripts/Engine/Evaluation/SceneEvaluator.cs`:
- MonoBehaviour that runs each frame
- For now: just ensures CameraBehaviour syncs. This is the hook point for the full evaluation pipeline later.
- `Update()`: calls `CameraBehaviour.Sync()` (or CameraBehaviour does it in its own `LateUpdate`)

> For Phase 1, CameraBehaviour self-syncing in LateUpdate is sufficient. SceneEvaluator becomes meaningful when we have multiple elements, keyframe evaluation, and event-driven triggers (Phase 3 / keyframes).

#### Task 3.3 — Scene setup

Create a minimal Unity scene:
- Camera GameObject with `CameraBehaviour`
- A ground plane (simple quad or plane at Y=0, scaled large) for spatial reference
- A few primitive cubes/spheres at various positions so there's something to look at
- Directional light

**Verification:** Press Play in Unity. Camera starts at (0, 1.6, -5) facing forward. You see the ground plane and reference objects. Nothing moves yet (input comes next).

---

### Phase 4: Input routing

**Goal:** Mouse and keyboard input drives camera movement. You can fly around the scene.

#### Task 4.1 — CameraInputHandler

`Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`:
- MonoBehaviour (or static input poller) reading from `Mouse.current` and `Keyboard.current`
- References `CameraBehaviour` to get the `CameraElement`
- Each `Update()`:
  1. Read raw input values (mouse delta, scroll, modifier keys)
  2. Apply speed multiplier and `Time.deltaTime`
  3. Call the appropriate `CameraElement` method

Input mapping (from interaction-patterns.md):

| Input | Modifier | Operation |
|-------|----------|-----------|
| Scroll Y | (none) | Focal length (stub — no lens system yet) |
| Scroll Y | Alt | Dolly |
| Scroll Y | Shift | Crane |
| Scroll Y | Ctrl | Roll |
| Scroll Y | Cmd+Alt | Dolly zoom |
| Scroll X | Cmd | Truck |
| Ctrl+Drag | (none) | Pan (X) + Tilt (Y) |
| Alt+Drag | (none) | Orbit |
| Ctrl+R | (none) | Reset |

**Implementation notes:**
- Use `Mouse.current.scroll.ReadValue()` for scroll
- Use `Mouse.current.delta.ReadValue()` for drag
- Check modifier state via `Keyboard.current.ctrlKey.isPressed`, etc.
- On macOS, "Cmd" maps to `Keyboard.current.leftCommandKey` or `rightCommandKey`
- Scroll deadzone: 0.01 (from tuned-constants.md)

#### Task 4.2 — Framerate independence verification

Manual test:
1. Lock editor to 30fps (via `Application.targetFrameRate = 30`)
2. Dolly forward a fixed amount of scroll
3. Lock editor to 120fps
4. Dolly forward the same scroll amount
5. Camera should end up at approximately the same position

This validates that `deltaTime` scaling works correctly.

#### Task 4.3 — Composition verification

Manual test:
1. Hold Ctrl and drag (pan/tilt) while simultaneously scrolling with Alt held (dolly)
2. Camera should pan AND dolly in the same frame
3. Result should look smooth, not jittery

**Verification:** You can fly around the scene using all input mappings. Ctrl-drag pans/tilts. Alt-drag orbits. Scroll+Alt dollies. Scroll+Shift cranes. Scroll+Ctrl rolls. Scroll+Cmd trucks. Ctrl+R resets. Movements compose when multiple inputs happen simultaneously.

---

## File Inventory

### Core (pure C#, no Unity) — `Unity/Fram3d/Assets/Scripts/Core/`
| File | Type | Phase |
|------|------|-------|
| `Core/Fram3d.Core.asmdef` | Assembly definition | 1 |
| `Core/Common/ElementId.cs` | Value object | 1 |
| `Core/Common/Element.cs` | Base class | 1 |
| `Core/Camera/CameraElement.cs` | Domain entity | 1–2 |
| `Core/Camera/MovementSpeeds.cs` | Value object | 2 |

### Engine (Unity) — `Unity/Fram3d/Assets/Scripts/Engine/`
| File | Type | Phase |
|------|------|-------|
| `Engine/Fram3d.Engine.asmdef` | Assembly definition | 1 |
| `Engine/Conversion/VectorExtensions.cs` | Static helpers | 1 |
| `Engine/Integration/CameraBehaviour.cs` | MonoBehaviour | 3 |

### UI (Unity) — `Unity/Fram3d/Assets/Scripts/UI/`
| File | Type | Phase |
|------|------|-------|
| `UI/Fram3d.UI.asmdef` | Assembly definition | 1 |
| `UI/Input/CameraInputHandler.cs` | MonoBehaviour | 4 |

### Tests (xUnit, external .NET project) — `tests/Fram3d.Core.Tests/`
| File | Phase |
|------|-------|
| `Fram3d.Core.Tests.csproj` | 1 |
| `Common/ElementIdTests.cs` | 1 |
| `Camera/CameraElementTests.cs` | 1–2 |

---

## Risks & Open Questions

1. **System.Numerics coordinate handedness.** System.Numerics uses right-hand coordinates. Unity uses left-hand. The "forward" direction differs (-Z vs +Z). The conversion extensions must handle this. Need to verify early in Phase 2 whether `Quaternion.CreateFromAxisAngle` + `Vector3.Transform` produce the expected results when converted to Unity space. If the math gets messy, consider using a thin `CameraVector3` / `CameraQuaternion` wrapper in Core that abstracts the convention, but prefer keeping it simple with well-tested conversion.

2. **Unity asmdef + standalone .csproj for Core.** Core source lives in Unity's Assets folder and is compiled both by Unity (via asmdef) and by an external .NET project (via `<Compile Include>` in `tests/Fram3d.Core.Tests.csproj`). Unity's auto-generated `.csproj` files live in the Unity project root — they shouldn't conflict with the test project, but verify early that `dotnet test` and Unity compilation both succeed without issues.

3. **Orbit pivot persistence.** The spec says "persistent last-focus model (Maya/Unity style)." Without focus (1.1.4), the orbit pivot defaults to world origin. When focus arrives, it updates `OrbitPivotPoint`. This is clean but means orbit is less useful until 1.1.4 — it always orbits around (0,0,0). Acceptable for the milestone.

4. **Dolly zoom is a stub.** Without focal length adjustment (1.1.2), dolly zoom is just a dolly. The architecture is in place — when 1.1.2 arrives, the method body fills out. No API changes needed.

---

## Exit Criteria

- [ ] All 8 movement types work interactively in the Unity editor via mouse + modifier keys
- [ ] Reset (Ctrl+R) restores default camera state
- [ ] Movements compose (simultaneous inputs produce combined motion)
- [ ] Movement is framerate-independent
- [ ] No collision detection — camera passes through everything
- [ ] Default camera position is (0, 1.6, -5) facing forward
- [ ] xUnit tests pass for all domain movement operations
- [ ] Assembly boundaries enforced: Core has zero Unity imports
- [ ] Code compiles with zero warnings
