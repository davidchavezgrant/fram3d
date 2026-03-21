# Implementation Plan: 1.1.2 Lens System

**Date**: 2026-03-20
**Milestone**: 1.1.2 Lens System
**Spec**: `docs/specs/milestone-1.1-virtual-camera-spec.md` §1.1.2
**Linear**: FRA-37
**Status**: Draft

---

## Summary

Focal length from 14mm to 400mm with physically accurate FOV from sensor dimensions. Smooth transitions on scroll and preset selection. Completes the dolly zoom stub from 1.1.1.

## Scope

**In scope:**
- Focal length clamping (14–400mm) on `CameraElement`
- Sensor height on `CameraElement` (default Super 35 = 18.66mm until 1.1.3 provides camera bodies)
- `ComputeVerticalFov()` — `2 * atan(sensorHeight / (2 * focalLength))`
- Smooth visual transitions in `CameraBehaviour` (lerp toward target focal length)
- Focal length presets as a static array
- Unmodified Scroll Y = adjust focal length
- Number keys 1–9 = preset selection
- Complete dolly zoom: adjust focal length to compensate for distance change
- xUnit tests for clamping, FOV computation, dolly zoom focal length adjustment

**Out of scope (deferred to 1.1.3):**
- Camera body selection / sensor dimension switching
- Lens set selection (which presets are available)
- Anamorphic squeeze factors
- Prime vs zoom distinction

## Design

**Domain vs presentation split for smooth transitions:**
- `CameraElement.FocalLength` is the target/intended value — always the "truth." This is what keyframes will record (3.2).
- `CameraBehaviour` lerps a `_displayedFocalLength` toward `CameraElement.FocalLength` each frame. The Unity camera gets the smoothed value.
- When the domain value changes (scroll, preset, dolly zoom), the visual catches up smoothly. No time or frame concepts in Core.

**Dolly zoom completion:**
- `DollyZoom(float amount)` currently only translates. Now it also adjusts `FocalLength` to maintain apparent subject size.
- Formula: as the camera moves closer by `delta`, the focal length must decrease proportionally to keep the subject the same angular size. `newFocal = focal * (distance - delta) / distance` where `distance` is the current distance to the reference point (world origin for now, specific element later in 2.1.1).

---

## Phases

### Phase 1: Core — focal length clamping, FOV, and presets

#### Task 1.1 — Focal length clamping

Add `SetFocalLength(float mm)` to `CameraElement` that clamps to 14–400:

```
public void SetFocalLength(float mm)
{
    this.FocalLength = Math.Clamp(mm, MIN_FOCAL_LENGTH, MAX_FOCAL_LENGTH);
}
```

Constants: `MIN_FOCAL_LENGTH = 14f`, `MAX_FOCAL_LENGTH = 400f`.

Update `Reset()` to use `SetFocalLength()`.

**Files:** `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs`

**Tests:**
- `SetFocalLength__ClampsToMinimum__When__BelowRange`
- `SetFocalLength__ClampsToMaximum__When__AboveRange`
- `SetFocalLength__SetsExactValue__When__WithinRange`

#### Task 1.2 — Sensor height and FOV computation

Add `SensorHeight` property (default 18.66mm — Super 35 vertical dimension).

Add `ComputeVerticalFov()`:
```
public float ComputeVerticalFov()
{
    return 2f * MathF.Atan(this.SensorHeight / (2f * this.FocalLength));
}
```

Returns radians. Unity's `Camera.fieldOfView` expects degrees, so `CameraBehaviour` converts.

**Files:** `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs`

**Tests:**
- `ComputeVerticalFov__ReturnsCorrectFov__When__50mmOnSuper35` — verify against known value
- `ComputeVerticalFov__ReturnsWiderFov__When__FocalLengthDecreases`
- `ComputeVerticalFov__ReturnsNarrowerFov__When__FocalLengthIncreases`
- `ComputeVerticalFov__ReturnsWiderFov__When__SensorHeightIncreases`

#### Task 1.3 — Focal length presets

Static array on `CameraElement` or a separate static class:

```
public static class FocalLengthPresets
{
    public static readonly float[] ALL = { 14, 18, 21, 24, 28, 35, 50, 65, 75, 85, 100, 135, 150, 200, 300, 400 };

    // Keys 1–9 map to the most cinematically useful subset
    public static readonly float[] QUICK = { 14, 18, 24, 35, 50, 85, 100, 135, 200 };
}
```

**Files:** `Unity/Fram3d/Assets/Scripts/Core/Camera/FocalLengthPresets.cs`

**Tests:**
- `QUICK__HasNineEntries__When__Accessed`
- `ALL__ContainsAllSpecPresets__When__Accessed`

#### Task 1.4 — Complete dolly zoom

Update `DollyZoom(float amount)` to adjust focal length proportionally:

```
public void DollyZoom(float amount)
{
    var forward = this.ComputeLookDirection();
    var referencePoint = this.OrbitPivotPoint; // world origin or last-focused element
    var distance = Vector3.Distance(this.Position, referencePoint);

    if (distance < 0.01f)
        return;

    this.Position += forward * amount;
    var newDistance = Vector3.Distance(this.Position, referencePoint);
    this.SetFocalLength(this.FocalLength * newDistance / distance);
}
```

**Files:** `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs`

**Tests:**
- `DollyZoom__AdjustsFocalLength__When__MovingCloser`
- `DollyZoom__MaintainsSubjectSize__When__Applied` — verify `focalLength / distance` ratio stays constant
- `DollyZoom__ClampsAtMinFocalLength__When__VeryClose`

### Phase 2: Engine — smooth transitions and FOV sync

#### Task 2.1 — Smooth focal length lerp in CameraBehaviour

Add `_displayedFocalLength` field. In `Sync()`, lerp toward target:

```
private float _displayedFocalLength;

private void Sync()
{
    this._displayedFocalLength = Mathf.Lerp(
        this._displayedFocalLength,
        this._cameraElement.FocalLength,
        Time.deltaTime * LENS_LERP_SPEED);

    this._unityCamera.focalLength = this._displayedFocalLength;
    // ... existing position/rotation sync
}
```

`LENS_LERP_SPEED = 10f` (from tuned-constants.md).

**Files:** `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs`

#### Task 2.2 — FOV sync

Set Unity camera's `fieldOfView` from the domain FOV computation, converted to degrees:

```
this._unityCamera.fieldOfView = this._displayedFov * Mathf.Rad2Deg;
```

Where `_displayedFov` is computed from `_displayedFocalLength` and `SensorHeight`.

Actually — since `usePhysicalProperties = true`, Unity computes FOV from `focalLength` and `sensorSize` automatically. We just need to sync `focalLength` and `sensorSize`. The domain `ComputeVerticalFov()` exists for other code that needs the FOV value (camera info overlay in 1.2.3, focus distance calculation in 1.1.4) without going through Unity.

So this task simplifies to: ensure `sensorSize` is synced from `CameraElement.SensorHeight` (and a default width).

**Files:** `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs`

### Phase 3: UI — scroll and preset input

#### Task 3.1 — Scroll Y = focal length

Wire unmodified Scroll Y to adjust focal length:

```
// Unmodified Scroll Y = focal length
if (Mathf.Abs(scrollY) > SCROLL_DEADZONE)
{
    this._camera.SetFocalLength(this._camera.FocalLength + scrollY * FOCAL_LENGTH_SCROLL_SPEED);
}
```

`FOCAL_LENGTH_SCROLL_SPEED` — add to `MovementSpeeds`: `0.5f` per scroll unit (from tuned-constants.md "Focal length adjustment multiplier: 0.5 per scroll unit"). Scroll values are ~120 per notch, so one notch ≈ 60mm change. May need tuning.

**Files:** `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`, `Unity/Fram3d/Assets/Scripts/Core/Camera/MovementSpeeds.cs`

#### Task 3.2 — Number keys 1–9 = presets

In `HandleKeyboardInput`, check number keys and snap to corresponding preset:

```
for (var i = 0; i < FocalLengthPresets.QUICK.Length; i++)
{
    if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
    {
        this._camera.SetFocalLength(FocalLengthPresets.QUICK[i]);
        break;
    }
}
```

**Files:** `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`

---

## File Inventory

### New files
| File | Type | Phase |
|------|------|-------|
| `Unity/Fram3d/Assets/Scripts/Core/Camera/FocalLengthPresets.cs` | Static class | 1 |

### Modified files
| File | Change | Phase |
|------|--------|-------|
| `Unity/Fram3d/Assets/Scripts/Core/Camera/CameraElement.cs` | Clamping, SensorHeight, ComputeVerticalFov, DollyZoom update | 1 |
| `Unity/Fram3d/Assets/Scripts/Core/Camera/MovementSpeeds.cs` | Add FOCAL_LENGTH_SCROLL | 1 |
| `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs` | Smooth lerp, sensor sync | 2 |
| `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs` | Scroll Y + number keys | 3 |
| `tests/Fram3d.Core.Tests/Camera/CameraElementTests.cs` | New tests | 1 |

### New test files
| File | Phase |
|------|-------|
| `tests/Fram3d.Core.Tests/Camera/FocalLengthPresetsTests.cs` | 1 |

---

## Exit Criteria

- [ ] Scroll Y adjusts focal length smoothly (14–400mm range, clamped)
- [ ] Number keys 1–9 snap to preset focal lengths with smooth transition
- [ ] FOV changes visually as focal length changes
- [ ] Dolly zoom adjusts both position and focal length to maintain subject size
- [ ] xUnit tests pass for clamping, FOV computation, presets, and dolly zoom
