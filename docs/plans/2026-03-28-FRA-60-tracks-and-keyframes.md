# FRA-60: Tracks and Keyframes — Implementation Plan

> **For Claude:** Implement this plan task-by-task. Complete each task fully before moving to the next. Pause at phase boundaries for manual verification.

**Goal:** Display camera and element tracks in the timeline with collapsible sub-tracks, keyframe diamond markers, live interpolated values, and single-keyframe selection.

**Spec:** `docs/specs/milestone-3.2-keyframe-animation-spec.md` lines 100–244

**Architecture:** Extend the Core data model with missing keyframe properties (focal length, aperture, focus distance, scale), add track view state (expansion/selection) to Timeline, extend the Engine evaluator, then build TrackRow/SubTrackRow VisualElements in the UI that render keyframe diamonds and live values. The "main keyframe" concept is virtual — a union of distinct times across all property KeyframeManagers.

**Tech Stack:** C# 9, System.Numerics, Unity 6 UI Toolkit, xUnit + FluentAssertions

---

## Current State Analysis

### What exists
- `Shot` (`Core/Shots/Shot.cs`): Stores camera animation via `CameraPositionKeyframes` (Vector3) and `CameraRotationKeyframes` (Quaternion). Evaluates with `Vector3.Lerp` and `Quaternion.Slerp`. Mandatory initial keyframes at t=0.
- `ElementTrack` (`Core/Timelines/ElementTrack.cs`): Stores per-element position (Vector3) and rotation (Quaternion) on the global timeline.
- `KeyframeManager<T>`: Generic sorted list + dictionary. Supports Add/Update/SetOrMerge/RemoveById/Evaluate with caller-supplied lerp.
- `Timeline` (`Core/Timelines/Timeline.cs`, 430 lines): Orchestrator. Owns ShotTrack, Playhead, ElementTimeline, ViewRange. Fires `CameraEvaluationRequested` and `ElementEvaluationRequested` observables.
- `TimelineSectionView` (`UI/Timeline/TimelineSectionView.cs`): Top-level MonoBehaviour. `BuildTrackArea()` creates a skeleton row (label column + content + playhead + out-of-range). **No actual track rows exist.**
- `ShotEvaluator` (`Engine/Integration/ShotEvaluator.cs`): Bridges Timeline → Unity. Only evaluates camera position + rotation, and element position + rotation.

### What's missing
- Focal length, aperture, focus distance KeyframeManagers on Shot
- Scale KeyframeManager on ElementTrack
- "All keyframe times" queries for main keyframe display
- Quaternion → pan/tilt/roll euler decomposition
- Track expansion/collapse state
- Keyframe selection state
- All track/sub-track UI

### Key Discoveries
- `CameraElement.CanDollyZoom` (`CameraElement.cs:27`) returns true when no lens set or zoom lens — matches spec condition for Focal Length sub-track visibility
- Pan/Tilt/Roll are applied as incremental quaternion multiplications: Pan around world Y (`CameraElement.cs:160-163`), Tilt around local right (`CameraElement.cs:213-217`), Roll around local forward (`CameraElement.cs:170-174`). The YXZ euler decomposition matches this application order.
- The test project (`tests/Fram3d.Core/Fram3d.Core.csproj:10`) uses `<Compile Include="../../Unity/Fram3d/Assets/Scripts/Core/**/*.cs" />` — any new `.cs` file in Core is auto-included.
- Element tracks use global time, not per-shot time. Element tracks appear whenever they have any keyframes, regardless of which shot is selected.

## What We're NOT Doing

- **Stopwatch / recording** — that's 3.2.3
- **Keyframe drag repositioning** — that's 3.2.4 (we DO implement click-to-select)
- **Interpolation curves** — that's 3.2.5/8.1.3
- **Per-axis keyframe detachment** — future feature; sub-tracks currently mirror parent KeyframeManager times
- **Path visualization** — that's 3.2.6

---

## Phase 1: Core — Data Model Extensions

### Overview
Add missing KeyframeManagers to Shot and ElementTrack. Add a quaternion-to-euler decomposition utility. Add "all keyframe times" queries for the main keyframe concept.

### Task 1.1: Add focal length, aperture, and focus distance keyframes to Shot

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs`
- Test: `tests/Fram3d.Core.Tests/Shots/ShotTests.cs`

**Step 1: Write the failing tests**

Add to `ShotTests.cs`:

```csharp
// --- Focal Length Keyframes ---

[Fact]
public void CameraFocalLengthKeyframes__IsEmpty__When__Created()
{
    var shot = MakeShot();
    shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
}

[Fact]
public void EvaluateCameraFocalLength__ReturnsDefault__When__NoKeyframes()
{
    var shot = MakeShot();
    shot.EvaluateCameraFocalLength(TimePosition.ZERO).Should().Be(0f);
}

[Fact]
public void EvaluateCameraFocalLength__ReturnsValue__When__SingleKeyframe()
{
    var shot = MakeShot();
    shot.CameraFocalLengthKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 50f));
    shot.EvaluateCameraFocalLength(TimePosition.ZERO).Should().Be(50f);
}

[Fact]
public void EvaluateCameraFocalLength__Interpolates__When__BetweenKeyframes()
{
    var shot = MakeShot();
    shot.CameraFocalLengthKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 24f));
    shot.CameraFocalLengthKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 100f));
    shot.EvaluateCameraFocalLength(new TimePosition(1.0)).Should().BeApproximately(62f, 0.1f);
}

// --- Aperture Keyframes ---

[Fact]
public void CameraApertureKeyframes__IsEmpty__When__Created()
{
    var shot = MakeShot();
    shot.CameraApertureKeyframes.Count.Should().Be(0);
}

[Fact]
public void EvaluateCameraAperture__Interpolates__When__BetweenKeyframes()
{
    var shot = MakeShot();
    shot.CameraApertureKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 2.8f));
    shot.CameraApertureKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 5.6f));
    shot.EvaluateCameraAperture(new TimePosition(1.0)).Should().BeApproximately(4.2f, 0.1f);
}

// --- Focus Distance Keyframes ---

[Fact]
public void CameraFocusDistanceKeyframes__IsEmpty__When__Created()
{
    var shot = MakeShot();
    shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
}

[Fact]
public void EvaluateCameraFocusDistance__Interpolates__When__BetweenKeyframes()
{
    var shot = MakeShot();
    shot.CameraFocusDistanceKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1.5f));
    shot.CameraFocusDistanceKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 10f));
    shot.EvaluateCameraFocusDistance(new TimePosition(1.0)).Should().BeApproximately(5.75f, 0.1f);
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~ShotTests"`
Expected: FAIL — `CameraFocalLengthKeyframes`, `CameraApertureKeyframes`, `CameraFocusDistanceKeyframes` don't exist.

**Step 3: Implement**

Add to `Shot.cs` — new fields after `CameraRotationKeyframes`:

```csharp
public KeyframeManager<float> CameraApertureKeyframes      { get; } = new();
public KeyframeManager<float> CameraFocalLengthKeyframes   { get; } = new();
public KeyframeManager<float> CameraFocusDistanceKeyframes { get; } = new();
```

Add evaluate methods after `EvaluateCameraRotation`:

```csharp
public float EvaluateCameraAperture(TimePosition localTime) =>
    this.CameraApertureKeyframes.Evaluate(localTime, Lerp);

public float EvaluateCameraFocalLength(TimePosition localTime) =>
    this.CameraFocalLengthKeyframes.Evaluate(localTime, Lerp);

public float EvaluateCameraFocusDistance(TimePosition localTime) =>
    this.CameraFocusDistanceKeyframes.Evaluate(localTime, Lerp);

private static float Lerp(float a, float b, float t) => a + (b - a) * t;
```

Update `TotalCameraKeyframeCount`:

```csharp
public int TotalCameraKeyframeCount =>
    this.CameraPositionKeyframes.Count
    + this.CameraRotationKeyframes.Count
    + this.CameraFocalLengthKeyframes.Count
    + this.CameraApertureKeyframes.Count
    + this.CameraFocusDistanceKeyframes.Count;
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~ShotTests"`
Expected: PASS

### Task 1.2: Add scale keyframes to ElementTrack

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/ElementTrackTests.cs`

**Step 1: Write the failing tests**

Add to `ElementTrackTests.cs`:

```csharp
[Fact]
public void ScaleKeyframes__IsEmpty__When__Created()
{
    var track = new ElementTrack(new ElementId(Guid.NewGuid()));
    track.ScaleKeyframes.Count.Should().Be(0);
}

[Fact]
public void EvaluateScale__ReturnsDefault__When__NoKeyframes()
{
    var track = new ElementTrack(new ElementId(Guid.NewGuid()));
    track.EvaluateScale(TimePosition.ZERO).Should().Be(0f);
}

[Fact]
public void EvaluateScale__Interpolates__When__BetweenKeyframes()
{
    var track = new ElementTrack(new ElementId(Guid.NewGuid()));
    track.ScaleKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1f));
    track.ScaleKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 3f));
    track.EvaluateScale(new TimePosition(1.0)).Should().BeApproximately(2f, 0.01f);
}

[Fact]
public void KeyframeCount__IncludesScale__When__ScaleKeyframesExist()
{
    var track = new ElementTrack(new ElementId(Guid.NewGuid()));
    track.ScaleKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1f));
    track.KeyframeCount.Should().Be(1);
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~ElementTrackTests"`
Expected: FAIL — `ScaleKeyframes` doesn't exist.

**Step 3: Implement**

Modify `ElementTrack.cs`:

```csharp
public sealed class ElementTrack
{
    public ElementTrack(ElementId elementId)
    {
        this.ElementId = elementId ?? throw new ArgumentNullException(nameof(elementId));
    }

    public ElementId                   ElementId         { get; }
    public bool                        HasKeyframes      => this.KeyframeCount > 0;
    public int                         KeyframeCount     => this.PositionKeyframes.Count + this.RotationKeyframes.Count + this.ScaleKeyframes.Count;
    public KeyframeManager<Vector3>    PositionKeyframes { get; } = new();
    public KeyframeManager<Quaternion> RotationKeyframes { get; } = new();
    public KeyframeManager<float>      ScaleKeyframes    { get; } = new();

    public Vector3    EvaluatePosition(TimePosition globalTime) => this.PositionKeyframes.Evaluate(globalTime, Vector3.Lerp);
    public Quaternion EvaluateRotation(TimePosition globalTime) => this.RotationKeyframes.Evaluate(globalTime, Quaternion.Slerp);
    public float      EvaluateScale(TimePosition globalTime)    => this.ScaleKeyframes.Evaluate(globalTime, Lerp);

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~ElementTrackTests"`
Expected: PASS

### Task 1.3: Quaternion → pan/tilt/roll euler decomposition

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Common/EulerAngles.cs`
- Test: `tests/Fram3d.Core.Tests/Common/EulerAnglesTests.cs`

**Step 1: Write the failing tests**

Create `tests/Fram3d.Core.Tests/Common/EulerAnglesTests.cs`:

```csharp
using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
    public class EulerAnglesTests
    {
        private const float TOLERANCE = 0.01f;

        [Fact]
        public void FromQuaternion__ReturnsZeros__When__Identity()
        {
            var euler = EulerAngles.FromQuaternion(Quaternion.Identity);
            euler.Pan.Should().BeApproximately(0f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(0f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsPan__When__PureYawApplied()
        {
            // Pan 45 degrees right = rotation around world Y by -45 degrees
            // (CameraElement.Pan negates the amount before CreateFromAxisAngle)
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(45f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(0f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsNegativePan__When__PanLeft()
        {
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 30f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(-30f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsTilt__When__PurePitchApplied()
        {
            // Tilt 20 degrees up = rotation around local right (X when identity)
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 20f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(0f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(20f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsRoll__When__PureRollApplied()
        {
            // Roll = rotation around local forward (-Z when identity)
            var q = Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, 15f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(0f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(0f, TOLERANCE);
            euler.Roll.Should().BeApproximately(15f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__RoundTrips__When__CombinedPanTilt()
        {
            // Apply pan then tilt (same order as CameraElement)
            var pan = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -30f * MathF.PI / 180f);
            var right = Vector3.Transform(Vector3.UnitX, pan);
            var tilt = Quaternion.CreateFromAxisAngle(right, 15f * MathF.PI / 180f);
            var combined = Quaternion.Normalize(tilt * pan);
            var euler = EulerAngles.FromQuaternion(combined);
            euler.Pan.Should().BeApproximately(30f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(15f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__HandlesGimbalEdge__When__TiltAt90()
        {
            // Looking straight up — pan and roll become ambiguous
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Tilt.Should().BeApproximately(90f, 0.5f);
            // Pan + Roll sum should be ~0 (they're coupled at gimbal lock)
        }
    }
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~EulerAnglesTests"`
Expected: FAIL — `EulerAngles` doesn't exist.

**Step 3: Implement**

Create `Unity/Fram3d/Assets/Scripts/Core/Common/EulerAngles.cs`:

```csharp
using System;
using System.Numerics;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Decomposes a quaternion into pan (yaw), tilt (pitch), and roll in degrees.
    /// Uses YXZ decomposition order matching CameraElement's application order:
    /// Pan around world Y, then Tilt around local X, then Roll around local Z.
    ///
    /// Convention: Pan positive = rightward, Tilt positive = upward,
    /// Roll positive = clockwise from camera POV.
    /// </summary>
    public sealed class EulerAngles
    {
        private const float DEG = 180f / MathF.PI;

        public EulerAngles(float pan, float tilt, float roll)
        {
            this.Pan  = pan;
            this.Tilt = tilt;
            this.Roll = roll;
        }

        public float Pan  { get; }
        public float Roll { get; }
        public float Tilt { get; }

        public static EulerAngles FromQuaternion(Quaternion q)
        {
            // Extract rotation matrix elements from quaternion
            var xx = q.X * q.X;
            var yy = q.Y * q.Y;
            var zz = q.Z * q.Z;
            var xy = q.X * q.Y;
            var xz = q.X * q.Z;
            var yz = q.Y * q.Z;
            var wx = q.W * q.X;
            var wy = q.W * q.Y;
            var wz = q.W * q.Z;

            // Rotation matrix (row-major)
            // R00 R01 R02     1-2(yy+zz)  2(xy-wz)    2(xz+wy)
            // R10 R11 R12  =  2(xy+wz)    1-2(xx+zz)  2(yz-wx)
            // R20 R21 R22     2(xz-wy)    2(yz+wx)    1-2(xx+yy)

            // YXZ decomposition:
            // tilt (X rotation) from R21
            var sinTilt = 2f * (yz + wx);
            sinTilt = Math.Clamp(sinTilt, -1f, 1f);

            float pan, tilt, roll;

            if (MathF.Abs(sinTilt) > 0.9999f)
            {
                // Gimbal lock — tilt at ±90°
                tilt = MathF.Asin(sinTilt);
                pan  = MathF.Atan2(2f * (xy + wz), 1f - 2f * (xx + zz));
                roll = 0f;
            }
            else
            {
                tilt = MathF.Asin(sinTilt);
                // pan (Y rotation) from R20 and R22
                pan = MathF.Atan2(-(2f * (xz - wy)), 1f - 2f * (xx + yy));
                // roll (Z rotation) from R01 and R11
                roll = MathF.Atan2(-(2f * (xy - wz)), 1f - 2f * (xx + zz));
            }

            // Negate pan because CameraElement.Pan negates the angle
            return new EulerAngles(-pan * DEG, tilt * DEG, roll * DEG);
        }
    }
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~EulerAnglesTests"`
Expected: PASS

> **Note:** The exact sign conventions may need tuning once wired to real camera state. The tests verify round-trip consistency with CameraElement's Pan/Tilt/Roll application.

### Task 1.4: "All keyframe times" queries

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Shots/Shot.cs`
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs`
- Test: `tests/Fram3d.Core.Tests/Shots/ShotTests.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/ElementTrackTests.cs`

**Step 1: Write the failing tests**

Add to `ShotTests.cs`:

```csharp
// --- GetAllCameraKeyframeTimes ---

[Fact]
public void GetAllCameraKeyframeTimes__ReturnsInitialTime__When__OnlyDefaultKeyframes()
{
    var shot = MakeShot();
    var times = shot.GetAllCameraKeyframeTimes();
    times.Should().HaveCount(1);
    times[0].Should().Be(TimePosition.ZERO);
}

[Fact]
public void GetAllCameraKeyframeTimes__MergesDistinctTimes__When__MultipleManagers()
{
    var shot = MakeShot();
    // Position has t=0 (default). Add rotation at t=1 and focal length at t=2.
    shot.CameraRotationKeyframes.Add(
        new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), Quaternion.Identity));
    shot.CameraFocalLengthKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 50f));
    var times = shot.GetAllCameraKeyframeTimes();
    times.Should().HaveCount(3);
    times[0].Seconds.Should().Be(0.0);
    times[1].Seconds.Should().Be(1.0);
    times[2].Seconds.Should().Be(2.0);
}

[Fact]
public void GetAllCameraKeyframeTimes__DeduplicatesSharedTimes__When__MultipleManagersSameTime()
{
    var shot = MakeShot();
    // Both position and rotation have t=0 by default.
    var times = shot.GetAllCameraKeyframeTimes();
    times.Should().HaveCount(1);
}
```

Add to `ElementTrackTests.cs`:

```csharp
// --- GetAllKeyframeTimes ---

[Fact]
public void GetAllKeyframeTimes__ReturnsEmpty__When__NoKeyframes()
{
    var track = new ElementTrack(new ElementId(Guid.NewGuid()));
    track.GetAllKeyframeTimes().Should().BeEmpty();
}

[Fact]
public void GetAllKeyframeTimes__MergesTimes__When__MultipleManagers()
{
    var track = new ElementTrack(new ElementId(Guid.NewGuid()));
    track.PositionKeyframes.Add(
        new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.Zero));
    track.ScaleKeyframes.Add(
        new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), 2f));
    var times = track.GetAllKeyframeTimes();
    times.Should().HaveCount(2);
    times[0].Seconds.Should().Be(0.0);
    times[1].Seconds.Should().Be(1.0);
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "GetAllCameraKeyframeTimes|GetAllKeyframeTimes"`
Expected: FAIL — methods don't exist.

**Step 3: Implement**

Add to `Shot.cs`:

```csharp
public IReadOnlyList<TimePosition> GetAllCameraKeyframeTimes()
{
    var times = new SortedSet<double>();

    foreach (var kf in this.CameraPositionKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    foreach (var kf in this.CameraRotationKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    foreach (var kf in this.CameraFocalLengthKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    foreach (var kf in this.CameraApertureKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    foreach (var kf in this.CameraFocusDistanceKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    var result = new List<TimePosition>(times.Count);

    foreach (var t in times)
    {
        result.Add(new TimePosition(t));
    }

    return result;
}
```

Add the `using System.Collections.Generic;` import to `Shot.cs` if not present.

Add to `ElementTrack.cs`:

```csharp
public IReadOnlyList<TimePosition> GetAllKeyframeTimes()
{
    var times = new SortedSet<double>();

    foreach (var kf in this.PositionKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    foreach (var kf in this.RotationKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    foreach (var kf in this.ScaleKeyframes.Keyframes)
    {
        times.Add(kf.Time.Seconds);
    }

    var result = new List<TimePosition>(times.Count);

    foreach (var t in times)
    {
        result.Add(new TimePosition(t));
    }

    return result;
}
```

Add `using System.Collections.Generic;` to `ElementTrack.cs`.

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "GetAllCameraKeyframeTimes|GetAllKeyframeTimes"`
Expected: PASS

**Step 5: Run all tests**

Run: `dotnet test tests/Fram3d.Core.Tests`
Expected: All PASS

### Phase 1 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` — all pass
- [ ] No C# 10+ features used

> **Pause here.** Confirm all tests pass before proceeding to Phase 2.

---

## Phase 2: Core — Track View State

### Overview
Add track expansion/collapse state and single-keyframe selection to Timeline. Selection moves the playhead and triggers camera evaluation.

### Task 2.1: TrackId type

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/TrackId.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/TrackIdTests.cs`

**Step 1: Write the failing tests**

Create `tests/Fram3d.Core.Tests/Timelines/TrackIdTests.cs`:

```csharp
using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class TrackIdTests
    {
        [Fact]
        public void Camera__IsSingleton__When__Compared()
        {
            TrackId.Camera.Should().Be(TrackId.Camera);
        }

        [Fact]
        public void ForElement__CreatesDistinct__When__DifferentElements()
        {
            var a = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            var b = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            a.Should().NotBe(b);
        }

        [Fact]
        public void ForElement__AreEqual__When__SameElement()
        {
            var id = new ElementId(Guid.NewGuid());
            TrackId.ForElement(id).Should().Be(TrackId.ForElement(id));
        }

        [Fact]
        public void Camera__IsNotEqual__When__ComparedToElement()
        {
            var element = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            TrackId.Camera.Should().NotBe(element);
        }

        [Fact]
        public void ForElement__ThrowsArgumentNull__When__NullId()
        {
            Action act = () => TrackId.ForElement(null);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~TrackIdTests"`
Expected: FAIL — `TrackId` doesn't exist.

**Step 3: Implement**

Create `Unity/Fram3d/Assets/Scripts/Core/Timelines/TrackId.cs`:

```csharp
using System;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Identifies a track in the timeline. Either the single camera track
    /// or an element track keyed by ElementId.
    /// </summary>
    public sealed class TrackId : IEquatable<TrackId>
    {
        public static readonly TrackId Camera = new(null);

        private readonly ElementId _elementId;

        private TrackId(ElementId elementId)
        {
            this._elementId = elementId;
        }

        public bool      IsCamera   => this._elementId == null;
        public bool      IsElement  => this._elementId != null;
        public ElementId ElementId  => this._elementId;

        public static TrackId ForElement(ElementId id) =>
            new(id ?? throw new ArgumentNullException(nameof(id)));

        public bool Equals(TrackId other) =>
            other != null && Equals(this._elementId, other._elementId);

        public override bool Equals(object obj) => obj is TrackId other && this.Equals(other);

        public override int GetHashCode() =>
            this._elementId?.GetHashCode() ?? 0;
    }
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~TrackIdTests"`
Expected: PASS

### Task 2.2: KeyframeSelection type

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeSelection.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/KeyframeSelectionTests.cs`

**Step 1: Write the failing tests**

Create `tests/Fram3d.Core.Tests/Timelines/KeyframeSelectionTests.cs`:

```csharp
using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class KeyframeSelectionTests
    {
        [Fact]
        public void HasSelection__ReturnsFalse__When__NothingSelected()
        {
            var sel = new KeyframeSelection();
            sel.HasSelection.Should().BeFalse();
        }

        [Fact]
        public void Select__SetsSelection__When__Called()
        {
            var sel = new KeyframeSelection();
            var kfId = new KeyframeId(Guid.NewGuid());
            var time = new TimePosition(1.5);
            sel.Select(TrackId.Camera, kfId, time);
            sel.HasSelection.Should().BeTrue();
            sel.TrackId.Should().Be(TrackId.Camera);
            sel.KeyframeId.Should().Be(kfId);
            sel.Time.Should().Be(time);
        }

        [Fact]
        public void Select__ReplacesExisting__When__CalledAgain()
        {
            var sel = new KeyframeSelection();
            var first = new KeyframeId(Guid.NewGuid());
            var second = new KeyframeId(Guid.NewGuid());
            sel.Select(TrackId.Camera, first, new TimePosition(1.0));
            sel.Select(TrackId.Camera, second, new TimePosition(2.0));
            sel.KeyframeId.Should().Be(second);
            sel.Time.Seconds.Should().Be(2.0);
        }

        [Fact]
        public void Clear__RemovesSelection__When__Called()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            sel.Clear();
            sel.HasSelection.Should().BeFalse();
        }

        [Fact]
        public void IsSelected__ReturnsTrue__When__MatchingKeyframe()
        {
            var sel = new KeyframeSelection();
            var kfId = new KeyframeId(Guid.NewGuid());
            sel.Select(TrackId.Camera, kfId, new TimePosition(1.0));
            sel.IsSelected(kfId).Should().BeTrue();
        }

        [Fact]
        public void IsSelected__ReturnsFalse__When__DifferentKeyframe()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            sel.IsSelected(new KeyframeId(Guid.NewGuid())).Should().BeFalse();
        }

        [Fact]
        public void Changed__Fires__When__SelectionChanges()
        {
            var sel = new KeyframeSelection();
            var fired = false;
            sel.Changed.Subscribe(_ => fired = true);
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            fired.Should().BeTrue();
        }

        [Fact]
        public void Changed__Fires__When__Cleared()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            var fired = false;
            sel.Changed.Subscribe(_ => fired = true);
            sel.Clear();
            fired.Should().BeTrue();
        }
    }
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~KeyframeSelectionTests"`
Expected: FAIL — `KeyframeSelection` doesn't exist.

**Step 3: Implement**

Create `Unity/Fram3d/Assets/Scripts/Core/Timelines/KeyframeSelection.cs`:

```csharp
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Tracks the single selected keyframe across all tracks.
    /// Selecting a keyframe on one track deselects on every other track.
    /// </summary>
    public sealed class KeyframeSelection
    {
        private readonly Subject<bool> _changed = new();

        public IObservable<bool> Changed      => this._changed;
        public bool              HasSelection => this.KeyframeId != null;
        public KeyframeId        KeyframeId   { get; private set; }
        public TimePosition      Time         { get; private set; }
        public TrackId           TrackId      { get; private set; }

        public void Clear()
        {
            this.KeyframeId = null;
            this.Time       = null;
            this.TrackId    = null;
            this._changed.OnNext(false);
        }

        public bool IsSelected(KeyframeId id) =>
            this.KeyframeId != null && this.KeyframeId.Equals(id);

        public void Select(TrackId trackId, KeyframeId keyframeId, TimePosition time)
        {
            this.TrackId    = trackId;
            this.KeyframeId = keyframeId;
            this.Time       = time;
            this._changed.OnNext(true);
        }
    }
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~KeyframeSelectionTests"`
Expected: PASS

### Task 2.3: Track expansion state and wire into Timeline

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/Core/Timelines/TrackExpansion.cs`
- Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/TrackExpansionTests.cs`
- Test: `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs`

**Step 1: Write the failing tests**

Create `tests/Fram3d.Core.Tests/Timelines/TrackExpansionTests.cs`:

```csharp
using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class TrackExpansionTests
    {
        [Fact]
        public void IsExpanded__ReturnsFalse__When__Default()
        {
            var exp = new TrackExpansion();
            exp.IsExpanded(TrackId.Camera).Should().BeFalse();
        }

        [Fact]
        public void Toggle__Expands__When__Collapsed()
        {
            var exp = new TrackExpansion();
            exp.Toggle(TrackId.Camera);
            exp.IsExpanded(TrackId.Camera).Should().BeTrue();
        }

        [Fact]
        public void Toggle__Collapses__When__Expanded()
        {
            var exp = new TrackExpansion();
            exp.Toggle(TrackId.Camera);
            exp.Toggle(TrackId.Camera);
            exp.IsExpanded(TrackId.Camera).Should().BeFalse();
        }

        [Fact]
        public void Toggle__IsPerTrack__When__DifferentTracks()
        {
            var exp = new TrackExpansion();
            var elemId = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            exp.Toggle(TrackId.Camera);
            exp.IsExpanded(TrackId.Camera).Should().BeTrue();
            exp.IsExpanded(elemId).Should().BeFalse();
        }
    }
}
```

Add to `TimelineTests.cs`:

```csharp
[Fact]
public void Selection__IsExposed__When__Accessed()
{
    var tl = new Timeline(FrameRate.FPS_24);
    tl.Selection.Should().NotBeNull();
    tl.Selection.HasSelection.Should().BeFalse();
}

[Fact]
public void Expansion__IsExposed__When__Accessed()
{
    var tl = new Timeline(FrameRate.FPS_24);
    tl.Expansion.Should().NotBeNull();
    tl.Expansion.IsExpanded(TrackId.Camera).Should().BeFalse();
}

[Fact]
public void SelectKeyframe__MovesPlayhead__When__Called()
{
    var tl = new Timeline(FrameRate.FPS_24);
    tl.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
    var kfId = tl.CurrentShot.CameraPositionKeyframes.Keyframes[0].Id;
    tl.SelectKeyframe(TrackId.Camera, kfId, new TimePosition(2.0));
    tl.Playhead.CurrentTime.Should().BeApproximately(2.0, 0.05);
    tl.Selection.IsSelected(kfId).Should().BeTrue();
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~TrackExpansionTests|Selection__|Expansion__"`
Expected: FAIL

**Step 3: Implement**

Create `Unity/Fram3d/Assets/Scripts/Core/Timelines/TrackExpansion.cs`:

```csharp
using System.Collections.Generic;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Tracks which timeline tracks are expanded (showing sub-tracks).
    /// All tracks default to collapsed.
    /// </summary>
    public sealed class TrackExpansion
    {
        private readonly HashSet<TrackId> _expanded = new();

        public bool IsExpanded(TrackId trackId) => this._expanded.Contains(trackId);

        public void Toggle(TrackId trackId)
        {
            if (!this._expanded.Remove(trackId))
            {
                this._expanded.Add(trackId);
            }
        }
    }
}
```

Modify `Timeline.cs` — add fields and properties:

```csharp
public KeyframeSelection Selection { get; } = new();
public TrackExpansion     Expansion { get; } = new();
```

Add the `SelectKeyframe` method:

```csharp
public void SelectKeyframe(TrackId trackId, KeyframeId keyframeId, TimePosition time)
{
    this.Selection.Select(trackId, keyframeId, time);
    this.Playhead.Scrub(time.Seconds, this.TotalDuration);
    this.EvaluateCamera();
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Fram3d.Core.Tests --filter "ClassName~TrackExpansionTests|Selection__|Expansion__|SelectKeyframe"`
Expected: PASS

**Step 5: Run all tests**

Run: `dotnet test tests/Fram3d.Core.Tests`
Expected: All PASS

### Phase 2 Verification

#### Automated
- [ ] `dotnet test tests/Fram3d.Core.Tests` — all pass

> **Pause here.** Confirm all tests pass before proceeding to Phase 3.

---

## Phase 3: Engine — Evaluate New Properties

### Overview
Extend ShotEvaluator to apply focal length, aperture, focus distance, and scale from keyframes during playback and scrubbing.

### Task 3.1: Extend ShotEvaluator for camera properties

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotEvaluator.cs`

**Step 1: Modify OnCameraEvaluationRequested**

Add focal length, aperture, and focus distance evaluation after the existing position/rotation application:

```csharp
private void OnCameraEvaluationRequested(CameraEvaluation eval)
{
    if (this._cameraBehaviour == null)
    {
        return;
    }

    var position = eval.Shot.EvaluateCameraPosition(eval.LocalTime);
    var rotation = eval.Shot.EvaluateCameraRotation(eval.LocalTime);
    this._cameraBehaviour.ShotCamera.Position = position;
    this._cameraBehaviour.ShotCamera.Rotation = rotation;

    if (eval.Shot.CameraFocalLengthKeyframes.Count > 0)
    {
        this._cameraBehaviour.ShotCamera.FocalLength = eval.Shot.EvaluateCameraFocalLength(eval.LocalTime);
        this._cameraBehaviour.ShotCamera.SnapFocalLength = true;
    }

    if (eval.Shot.CameraApertureKeyframes.Count > 0)
    {
        this._cameraBehaviour.ShotCamera.Aperture = eval.Shot.EvaluateCameraAperture(eval.LocalTime);
    }

    if (eval.Shot.CameraFocusDistanceKeyframes.Count > 0)
    {
        this._cameraBehaviour.ShotCamera.FocusDistance = eval.Shot.EvaluateCameraFocusDistance(eval.LocalTime);
    }
}
```

Also update `OnCurrentShotChanged` to apply the new properties at t=0 if keyframes exist:

```csharp
private void OnCurrentShotChanged(Shot shot)
{
    if (shot == null || this._cameraBehaviour == null)
    {
        return;
    }

    var position = shot.EvaluateCameraPosition(TimePosition.ZERO);
    var rotation = shot.EvaluateCameraRotation(TimePosition.ZERO);
    this._cameraBehaviour.ShotCamera.Position = position;
    this._cameraBehaviour.ShotCamera.Rotation = rotation;

    if (shot.CameraFocalLengthKeyframes.Count > 0)
    {
        this._cameraBehaviour.ShotCamera.FocalLength = shot.EvaluateCameraFocalLength(TimePosition.ZERO);
        this._cameraBehaviour.ShotCamera.SnapFocalLength = true;
    }

    if (shot.CameraApertureKeyframes.Count > 0)
    {
        this._cameraBehaviour.ShotCamera.Aperture = shot.EvaluateCameraAperture(TimePosition.ZERO);
    }

    if (shot.CameraFocusDistanceKeyframes.Count > 0)
    {
        this._cameraBehaviour.ShotCamera.FocusDistance = shot.EvaluateCameraFocusDistance(TimePosition.ZERO);
    }
}
```

### Task 3.2: Extend ShotEvaluator for element scale

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotEvaluator.cs`

**Step 1: Modify OnElementEvaluationRequested**

Add scale evaluation after position/rotation:

```csharp
private void OnElementEvaluationRequested(ElementEvaluation eval)
{
    var elements = FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

    foreach (var elementBehaviour in elements)
    {
        var element = elementBehaviour.Element;

        if (element == null)
        {
            continue;
        }

        var track = this.Controller.Elements.GetTrack(element.Id);

        if (track == null || !track.HasKeyframes)
        {
            continue;
        }

        element.Position = track.EvaluatePosition(eval.GlobalTime);
        element.Rotation = track.EvaluateRotation(eval.GlobalTime);

        if (track.ScaleKeyframes.Count > 0)
        {
            element.Scale = track.EvaluateScale(eval.GlobalTime);
        }
    }
}
```

**Step 2: Verify `CameraElement.Aperture` has a public setter**

Check if `Aperture` is settable. If it's read-only (delegated to LensController), we'll need to add a setter. The ShotEvaluator needs to write the keyframed value.

> **Implementation note:** If `Aperture` is read-only on `CameraElement`, add a public setter that delegates to `_lens.Aperture = value`. Same pattern as `FocalLength` setter.

### Phase 3 Verification

#### Manual
- [ ] Build the Unity project — verify no compile errors
- [ ] Play the scene — verify existing playback still works (no regressions from new empty KeyframeManagers)

> **Pause here.** Confirm Unity builds before proceeding to Phase 4.

---

## Phase 4: UI — Track Rendering

### Overview
Build TrackRow, SubTrackRow, and KeyframeDiamond VisualElements. Add USS styles. Wire into TimelineSectionView to replace the skeleton track area.

### Task 4.1: KeyframeDiamond VisualElement

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/UI/Timeline/KeyframeDiamond.cs`

A small 10×10 square rotated 45° to form a diamond shape. Colored by track type, with cyan override when selected.

```csharp
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A diamond-shaped keyframe marker on the timeline.
    /// Rendered as a rotated square via USS transform.
    /// </summary>
    public sealed class KeyframeDiamond : VisualElement
    {
        public KeyframeDiamond()
        {
            this.AddToClassList("keyframe-diamond");
            this.pickingMode = PickingMode.Position;
        }

        public void SetColor(bool isCamera) =>
            this.EnableInClassList("keyframe-diamond--element", !isCamera);

        public void SetSelected(bool selected) =>
            this.EnableInClassList("keyframe-diamond--selected", selected);
    }
}
```

### Task 4.2: SubTrackRow VisualElement

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/UI/Timeline/SubTrackRow.cs`

A 22px row with property name label, live value label, and keyframe diamond content area.

```csharp
using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A property sub-track row within an expanded track.
    /// Shows property name, live interpolated value, and keyframe diamonds.
    /// </summary>
    public sealed class SubTrackRow : VisualElement
    {
        private readonly VisualElement         _content;
        private readonly List<KeyframeDiamond> _diamonds = new();
        private readonly bool                  _isCamera;
        private readonly Label                 _nameLabel;
        private readonly Label                 _valueLabel;

        public SubTrackRow(string propertyName, bool isCamera)
        {
            this._isCamera = isCamera;
            this.AddToClassList("sub-track-row");

            var labels = new VisualElement();
            labels.AddToClassList("sub-track-label-column");

            this._nameLabel = new Label(propertyName);
            this._nameLabel.AddToClassList("sub-track-name");
            labels.Add(this._nameLabel);

            this._valueLabel = new Label("—");
            this._valueLabel.AddToClassList("sub-track-value");
            labels.Add(this._valueLabel);

            this.Add(labels);

            this._content = new VisualElement();
            this._content.AddToClassList("sub-track-content");
            this.Add(this._content);
        }

        public event Action<KeyframeId, TimePosition> DiamondClicked;

        public void SetValue(string formattedValue) =>
            this._valueLabel.text = formattedValue;

        public void UpdateDiamonds(
            IReadOnlyList<Keyframe<object>> keyframes,
            Func<double, double> timeToPixel,
            KeyframeSelection selection)
        {
            // Remove excess diamonds
            while (this._diamonds.Count > keyframes.Count)
            {
                var last = this._diamonds[this._diamonds.Count - 1];
                this._content.Remove(last);
                this._diamonds.RemoveAt(this._diamonds.Count - 1);
            }

            // Add missing diamonds
            while (this._diamonds.Count < keyframes.Count)
            {
                var diamond = new KeyframeDiamond();
                diamond.SetColor(this._isCamera);
                this._diamonds.Add(diamond);
                this._content.Add(diamond);
            }

            // Position and update selection state
            for (var i = 0; i < keyframes.Count; i++)
            {
                var kf = keyframes[i];
                var px = (float)timeToPixel(kf.Time.Seconds);
                this._diamonds[i].style.left = px - 5f; // center the 10px diamond
                this._diamonds[i].SetSelected(selection != null && selection.IsSelected(kf.Id));
            }
        }

        /// <summary>
        /// Simplified diamond update from TimePositions (when we don't have typed keyframes).
        /// Used for sub-tracks that share a parent KeyframeManager's times.
        /// </summary>
        public void UpdateDiamondPositions(
            IReadOnlyList<TimePosition> times,
            IReadOnlyList<KeyframeId> ids,
            Func<double, double> timeToPixel,
            KeyframeSelection selection)
        {
            while (this._diamonds.Count > times.Count)
            {
                var last = this._diamonds[this._diamonds.Count - 1];
                this._content.Remove(last);
                this._diamonds.RemoveAt(this._diamonds.Count - 1);
            }

            while (this._diamonds.Count < times.Count)
            {
                var diamond = new KeyframeDiamond();
                diamond.SetColor(this._isCamera);
                var idx = this._diamonds.Count;
                diamond.RegisterCallback<ClickEvent>(_ =>
                {
                    if (idx < ids.Count)
                    {
                        this.DiamondClicked?.Invoke(ids[idx], times[idx]);
                    }
                });
                this._diamonds.Add(diamond);
                this._content.Add(diamond);
            }

            for (var i = 0; i < times.Count; i++)
            {
                var px = (float)timeToPixel(times[i].Seconds);
                this._diamonds[i].style.left = px - 5f;
                this._diamonds[i].SetSelected(selection != null && i < ids.Count && selection.IsSelected(ids[i]));
            }
        }
    }
}
```

### Task 4.3: TrackRow VisualElement

**Files:**
- Create: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TrackRow.cs`

A 28px row with collapse arrow, track name, keyframe diamond area (for main keyframes when collapsed), and a container for sub-track rows when expanded.

```csharp
using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A track row in the timeline. Shows the track header (collapse arrow, name)
    /// and main keyframe diamonds when collapsed. When expanded, shows SubTrackRows.
    /// </summary>
    public sealed class TrackRow : VisualElement
    {
        private readonly VisualElement         _arrow;
        private readonly VisualElement         _content;
        private readonly List<KeyframeDiamond> _diamonds     = new();
        private readonly bool                  _isCamera;
        private readonly VisualElement         _subContainer;
        private readonly List<SubTrackRow>     _subTracks    = new();
        private readonly TrackId               _trackId;

        public TrackRow(TrackId trackId, string name, bool isCamera)
        {
            this._trackId  = trackId;
            this._isCamera = isCamera;
            this.AddToClassList("track-row");

            if (isCamera)
            {
                this.AddToClassList("track-row--camera");
            }
            else
            {
                this.AddToClassList("track-row--element");
            }

            // Header row
            var header = new VisualElement();
            header.AddToClassList("track-header");

            var labels = new VisualElement();
            labels.AddToClassList("track-label-column");

            this._arrow = new VisualElement();
            this._arrow.AddToClassList("track-arrow");
            this._arrow.AddToClassList("track-arrow--collapsed");
            this._arrow.RegisterCallback<ClickEvent>(_ => this.ArrowClicked?.Invoke(this._trackId));
            labels.Add(this._arrow);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("track-name");
            labels.Add(nameLabel);

            header.Add(labels);

            this._content = new VisualElement();
            this._content.AddToClassList("track-content");
            header.Add(this._content);

            this.Add(header);

            // Sub-track container (hidden by default)
            this._subContainer = new VisualElement();
            this._subContainer.AddToClassList("sub-track-container");
            this._subContainer.style.display = DisplayStyle.None;
            this.Add(this._subContainer);
        }

        public event Action<TrackId>                   ArrowClicked;
        public event Action<KeyframeId, TimePosition>  DiamondClicked;
        public IReadOnlyList<SubTrackRow>               SubTracks => this._subTracks;

        public SubTrackRow AddSubTrack(string propertyName)
        {
            var row = new SubTrackRow(propertyName, this._isCamera);
            row.DiamondClicked += (id, time) => this.DiamondClicked?.Invoke(id, time);
            this._subTracks.Add(row);
            this._subContainer.Add(row);
            return row;
        }

        public void SetExpanded(bool expanded)
        {
            this._subContainer.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            this._arrow.EnableInClassList("track-arrow--collapsed", !expanded);
            this._arrow.EnableInClassList("track-arrow--expanded", expanded);
        }

        /// <summary>
        /// Update the main keyframe diamonds on the collapsed header row.
        /// </summary>
        public void UpdateMainDiamonds(
            IReadOnlyList<TimePosition> times,
            Func<double, double> timeToPixel,
            KeyframeSelection selection)
        {
            while (this._diamonds.Count > times.Count)
            {
                var last = this._diamonds[this._diamonds.Count - 1];
                this._content.Remove(last);
                this._diamonds.RemoveAt(this._diamonds.Count - 1);
            }

            while (this._diamonds.Count < times.Count)
            {
                var diamond = new KeyframeDiamond();
                diamond.SetColor(this._isCamera);
                var idx = this._diamonds.Count;
                diamond.RegisterCallback<ClickEvent>(_ =>
                {
                    if (idx < times.Count)
                    {
                        this.DiamondClicked?.Invoke(null, times[idx]);
                    }
                });
                this._diamonds.Add(diamond);
                this._content.Add(diamond);
            }

            for (var i = 0; i < times.Count; i++)
            {
                var px = (float)timeToPixel(times[i].Seconds);
                this._diamonds[i].style.left = px - 5f;
                // Main diamonds are selected if any keyframe at that time is selected
                var isSelected = selection != null
                    && selection.HasSelection
                    && selection.TrackId != null
                    && selection.TrackId.Equals(this._trackId)
                    && selection.Time != null
                    && selection.Time.Equals(times[i]);
                this._diamonds[i].SetSelected(isSelected);
            }
        }
    }
}
```

### Task 4.4: USS styles for tracks, sub-tracks, and diamonds

**Files:**
- Modify: `Unity/Fram3d/Assets/Resources/fram3d.uss`

Append after the existing track area section (after line ~587):

```css
/* ── Track rows ─────────────────────────────────────────────────── */

.track-row {
    flex-shrink: 0;
    border-bottom-width: 1px;
    border-bottom-color: rgb(50, 50, 50);
}

.track-header {
    flex-direction: row;
    height: 28px;
    align-items: center;
}

.track-label-column {
    width: 140px;
    flex-shrink: 0;
    flex-direction: row;
    align-items: center;
    padding-left: 4px;
    overflow: hidden;
}

.track-arrow {
    width: 14px;
    height: 14px;
    margin-right: 4px;
    background-color: rgba(180, 180, 180, 0.6);
    -unity-slice-left: 0;
    -unity-slice-right: 0;
    cursor: link;
}

.track-arrow--collapsed {
    rotate: 0deg;
    border-left-width: 5px;
    border-right-width: 5px;
    border-top-width: 5px;
    border-bottom-width: 5px;
    border-left-color: transparent;
    border-right-color: transparent;
    border-top-color: rgba(180, 180, 180, 0.8);
    border-bottom-color: transparent;
    background-color: transparent;
    width: 0;
    height: 0;
}

.track-arrow--expanded {
    rotate: 0deg;
    border-left-width: 5px;
    border-right-width: 5px;
    border-top-width: 5px;
    border-bottom-width: 5px;
    border-left-color: transparent;
    border-right-color: transparent;
    border-top-color: transparent;
    border-bottom-color: rgba(180, 180, 180, 0.8);
    background-color: transparent;
    width: 0;
    height: 0;
}

.track-name {
    font-size: 11px;
    color: rgb(200, 200, 200);
    -unity-text-align: middle-left;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.track-content {
    flex-grow: 1;
    position: relative;
    height: 28px;
    overflow: hidden;
}

.track-row--camera .track-content {
    background-color: rgba(200, 180, 50, 0.06);
}

.track-row--element .track-content {
    background-color: rgba(80, 200, 80, 0.06);
}

/* ── Sub-track rows ─────────────────────────────────────────────── */

.sub-track-container {
    flex-direction: column;
}

.sub-track-row {
    flex-direction: row;
    height: 22px;
    align-items: center;
    border-bottom-width: 1px;
    border-bottom-color: rgb(45, 45, 45);
}

.sub-track-label-column {
    width: 140px;
    flex-shrink: 0;
    flex-direction: row;
    align-items: center;
    padding-left: 22px;
    overflow: hidden;
}

.sub-track-name {
    font-size: 10px;
    color: rgb(160, 160, 160);
    -unity-text-align: middle-left;
    flex-shrink: 0;
    margin-right: 6px;
}

.sub-track-value {
    font-size: 10px;
    color: rgb(120, 120, 120);
    -unity-text-align: middle-left;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.sub-track-content {
    flex-grow: 1;
    position: relative;
    height: 22px;
    overflow: hidden;
}

/* ── Keyframe diamonds ──────────────────────────────────────────── */

.keyframe-diamond {
    position: absolute;
    width: 10px;
    height: 10px;
    top: 50%;
    margin-top: -5px;
    rotate: 45deg;
    background-color: rgb(220, 200, 60);
    cursor: link;
}

.keyframe-diamond--element {
    background-color: rgb(80, 200, 80);
}

.keyframe-diamond--selected {
    background-color: rgb(0, 220, 220);
}
```

### Task 4.5: Wire tracks into TimelineSectionView

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

This is the main integration task. Replace the skeleton `BuildTrackArea()` with actual track generation.

**Key changes:**

1. Add fields for track management:
```csharp
private readonly List<TrackRow> _trackRows = new();
private TrackRow                _cameraTrackRow;
```

2. Replace `BuildTrackArea()`:
```csharp
private void BuildTrackArea()
{
    this._trackContainer = new VisualElement();
    this._trackContainer.AddToClassList("timeline-track-area");

    var trackScroll = new VisualElement();
    trackScroll.AddToClassList("timeline-track-row");

    var labels = new VisualElement();
    labels.AddToClassList("timeline-label-column");
    trackScroll.Add(labels);

    this._trackContent = new VisualElement();
    this._trackContent.AddToClassList("timeline-track-content");
    trackScroll.Add(this._trackContent);

    this._trackPlayhead = new VisualElement();
    this._trackPlayhead.AddToClassList("timeline-playhead");
    this._trackPlayhead.style.display = DisplayStyle.None;

    this._trackOutOfRange = new VisualElement();
    this._trackOutOfRange.AddToClassList("timeline-out-of-range");
    this._trackOutOfRange.pickingMode = PickingMode.Ignore;

    this._trackContainer.Add(trackScroll);
    this._trackContent.RegisterCallback<WheelEvent>(this.OnWheel);
    this._section.Add(this._trackContainer);

    this.RebuildTracks();
}
```

3. Add `RebuildTracks()` method:
```csharp
private void RebuildTracks()
{
    // Clear existing track rows from the track container
    foreach (var row in this._trackRows)
    {
        row.RemoveFromHierarchy();
    }
    this._trackRows.Clear();

    // Camera track (always present)
    this._cameraTrackRow = new TrackRow(TrackId.Camera, "Camera", true);
    this._cameraTrackRow.ArrowClicked += this.OnTrackArrowClicked;
    this._cameraTrackRow.DiamondClicked += this.OnDiamondClicked;
    this.BuildCameraSubTracks(this._cameraTrackRow);
    this._trackRows.Add(this._cameraTrackRow);
    this._trackContainer.Insert(0, this._cameraTrackRow);

    // Element tracks
    foreach (var track in this._controller.Elements.Tracks)
    {
        if (!track.HasKeyframes)
        {
            continue;
        }

        var elemRow = new TrackRow(
            TrackId.ForElement(track.ElementId),
            this.GetElementName(track.ElementId),
            false);
        elemRow.ArrowClicked += this.OnTrackArrowClicked;
        elemRow.DiamondClicked += this.OnDiamondClicked;
        this.BuildElementSubTracks(elemRow);
        this._trackRows.Add(elemRow);
        this._trackContainer.Insert(this._trackRows.Count - 1, elemRow);
    }

    // Re-add playhead and out-of-range on top
    this._trackContent.Add(this._trackPlayhead);
    this._trackContent.Add(this._trackOutOfRange);
}
```

4. Add `BuildCameraSubTracks()`:
```csharp
private void BuildCameraSubTracks(TrackRow row)
{
    row.AddSubTrack("Position X");
    row.AddSubTrack("Position Y");
    row.AddSubTrack("Position Z");
    row.AddSubTrack("Pan");
    row.AddSubTrack("Tilt");
    row.AddSubTrack("Roll");

    var cam = this.GetShotCamera();

    if (cam != null && cam.CanDollyZoom)
    {
        row.AddSubTrack("Focal Length");
    }

    row.AddSubTrack("Focus Distance");
    row.AddSubTrack("Aperture");
    row.SetExpanded(this._controller.Expansion.IsExpanded(TrackId.Camera));
}
```

5. Add `BuildElementSubTracks()`:
```csharp
private void BuildElementSubTracks(TrackRow row)
{
    row.AddSubTrack("Position X");
    row.AddSubTrack("Position Y");
    row.AddSubTrack("Position Z");
    row.AddSubTrack("Scale");
    row.AddSubTrack("Rotation X");
    row.AddSubTrack("Rotation Y");
    row.AddSubTrack("Rotation Z");
}
```

6. Update `SyncVisuals()` to update track diamonds and live values:
```csharp
// In SyncVisuals(), after existing code:
this.SyncTrackVisuals();
```

7. Add `SyncTrackVisuals()`:
```csharp
private void SyncTrackVisuals()
{
    if (this._controller.CurrentShot == null)
    {
        return;
    }

    var shot = this._controller.CurrentShot;
    Func<double, double> timeToPixel = this._controller.TimeToPixel;

    // Camera track main diamonds
    var cameraTimes = shot.GetAllCameraKeyframeTimes();
    this._cameraTrackRow.UpdateMainDiamonds(cameraTimes, timeToPixel, this._controller.Selection);
    this._cameraTrackRow.SetExpanded(this._controller.Expansion.IsExpanded(TrackId.Camera));

    // Camera sub-track live values
    if (this._controller.Expansion.IsExpanded(TrackId.Camera))
    {
        this.SyncCameraSubTrackValues(shot);
    }

    // Element tracks
    for (var i = 1; i < this._trackRows.Count; i++)
    {
        // Element track diamond sync similar to camera
    }
}
```

8. Add `SyncCameraSubTrackValues()` — reads interpolated values at playhead and formats them:
```csharp
private void SyncCameraSubTrackValues(Shot shot)
{
    var localTime = this.GetLocalPlayheadTime();

    if (localTime == null || this._cameraTrackRow.SubTracks.Count == 0)
    {
        return;
    }

    var pos = shot.EvaluateCameraPosition(localTime);
    var rot = shot.EvaluateCameraRotation(localTime);
    var euler = EulerAngles.FromQuaternion(rot);

    var subs = this._cameraTrackRow.SubTracks;
    var idx = 0;
    subs[idx++].SetValue(pos.X.ToString("F2"));
    subs[idx++].SetValue(pos.Y.ToString("F2"));
    subs[idx++].SetValue(pos.Z.ToString("F2"));
    subs[idx++].SetValue(euler.Pan.ToString("F1") + "°");
    subs[idx++].SetValue(euler.Tilt.ToString("F1") + "°");
    subs[idx++].SetValue(euler.Roll.ToString("F1") + "°");

    var cam = this.GetShotCamera();

    if (cam != null && cam.CanDollyZoom && idx < subs.Count)
    {
        var fl = shot.CameraFocalLengthKeyframes.Count > 0
            ? shot.EvaluateCameraFocalLength(localTime)
            : cam.FocalLength;
        subs[idx++].SetValue(fl.ToString("F0") + "mm");
    }

    if (idx < subs.Count)
    {
        var fd = shot.CameraFocusDistanceKeyframes.Count > 0
            ? shot.EvaluateCameraFocusDistance(localTime)
            : (cam?.FocusDistance ?? 0f);
        subs[idx++].SetValue(fd.ToString("F1") + "m");
    }

    if (idx < subs.Count)
    {
        var ap = shot.CameraApertureKeyframes.Count > 0
            ? shot.EvaluateCameraAperture(localTime)
            : (cam?.Aperture ?? 0f);
        subs[idx++].SetValue("f/" + ap.ToString("F1"));
    }
}
```

9. Add helper methods:
```csharp
private CameraElement GetShotCamera()
{
    var cam = FindAnyObjectByType<CameraBehaviour>();
    return cam?.ShotCamera;
}

private string GetElementName(ElementId id)
{
    var elements = FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

    foreach (var eb in elements)
    {
        if (eb.Element?.Id != null && eb.Element.Id.Equals(id))
        {
            return eb.Element.Name;
        }
    }

    return "Element";
}

private TimePosition GetLocalPlayheadTime()
{
    var result = this._controller.ResolveShot();

    if (!result.HasValue)
    {
        return null;
    }

    return result.Value.localTime;
}
```

10. Subscribe to shot changes and element track changes for rebuild:
```csharp
// In Start(), add:
this._controller.CurrentShotChanged.Subscribe(_ => this.RebuildTracks());
this._controller.Selection.Changed.Subscribe(_ => this.SyncTrackVisuals());
```

### Task 4.6: Add `_trackContainer` field and clean up references

Ensure `_trackContainer` is declared as a field and that the track rows are added to it rather than to `_trackContent`. The playhead and out-of-range overlays remain children of `_trackContent` so they render on top.

Add field:
```csharp
private VisualElement _trackContainer;
```

### Phase 4 Verification

#### Manual
- [ ] Unity builds without errors
- [ ] Camera track appears with "Camera" label when a shot exists
- [ ] Collapse arrow toggles sub-track visibility
- [ ] Yellow diamonds appear at keyframe times on the camera track
- [ ] Sub-tracks show Position X/Y/Z, Pan, Tilt, Roll, Focus Distance, Aperture
- [ ] Focal Length sub-track appears when no lens set is selected (free range)
- [ ] Focal Length sub-track hidden when a prime lens set is selected
- [ ] Live values update when scrubbing the playhead
- [ ] Element tracks appear when elements have keyframes

> **Pause here.** Confirm visual rendering before proceeding to Phase 5.

---

## Phase 5: UI — Keyframe Selection Interaction

### Overview
Wire click events on keyframe diamonds to select, move playhead, highlight cyan, and enforce single selection.

### Task 5.1: Wire diamond click events

**Files:**
- Modify: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

Add the `OnDiamondClicked` handler:

```csharp
private void OnDiamondClicked(KeyframeId keyframeId, TimePosition time)
{
    if (time == null)
    {
        return;
    }

    if (keyframeId != null)
    {
        // Property keyframe clicked
        this._controller.SelectKeyframe(TrackId.Camera, keyframeId, time);
    }
    else
    {
        // Main keyframe clicked — select by time, pick first keyframe at that time
        var shot = this._controller.CurrentShot;

        if (shot == null)
        {
            return;
        }

        // Find a representative keyframe at this time
        KeyframeId representative = null;

        foreach (var kf in shot.CameraPositionKeyframes.Keyframes)
        {
            if (kf.Time.Equals(time))
            {
                representative = kf.Id;

                break;
            }
        }

        if (representative == null)
        {
            foreach (var kf in shot.CameraRotationKeyframes.Keyframes)
            {
                if (kf.Time.Equals(time))
                {
                    representative = kf.Id;

                    break;
                }
            }
        }

        if (representative != null)
        {
            this._controller.SelectKeyframe(TrackId.Camera, representative, time);
        }
    }
}
```

### Task 5.2: Wire track arrow click events

```csharp
private void OnTrackArrowClicked(TrackId trackId)
{
    this._controller.Expansion.Toggle(trackId);

    foreach (var row in this._trackRows)
    {
        // Each row knows its own TrackId; find the matching one and update
    }

    this.SyncTrackVisuals();
}
```

> **Implementation note:** The `TrackRow` needs to expose its `TrackId` so the arrow handler can match. Add a `public TrackId TrackId => this._trackId;` property to `TrackRow`.

### Task 5.3: Click empty area to deselect

Register a click handler on the track content area that clears selection when clicking empty space (not on a diamond):

```csharp
// In BuildTrackArea():
this._trackContent.RegisterCallback<ClickEvent>(evt =>
{
    if (evt.target == this._trackContent)
    {
        this._controller.Selection.Clear();
    }
});
```

### Phase 5 Verification

#### Manual
- [ ] Clicking a keyframe diamond highlights it cyan
- [ ] Playhead moves to the clicked keyframe's time
- [ ] 3D view updates to show the camera state at that time
- [ ] Clicking a different diamond deselects the previous one
- [ ] Only one diamond is cyan across all tracks at any time
- [ ] Clicking empty track area deselects
- [ ] Collapse arrow works correctly (toggles sub-tracks)

> **Pause here.** Full manual verification.

---

## Testing Strategy

### Unit Tests (xUnit — Core)
- Shot: focal length / aperture / focus distance KeyframeManagers and evaluation
- Shot: `GetAllCameraKeyframeTimes()` — deduplication, sorting, multi-manager merge
- ElementTrack: Scale keyframes and evaluation
- ElementTrack: `GetAllKeyframeTimes()` — multi-manager merge
- EulerAngles: identity, pure pan, pure tilt, pure roll, combined, gimbal edge
- TrackId: equality, camera singleton, element keying
- KeyframeSelection: select, clear, single selection, Changed observable
- TrackExpansion: toggle, per-track independence
- Timeline: SelectKeyframe moves playhead and sets selection

### Manual Testing Steps
1. Open a scene with a shot — camera track appears with yellow diamond at t=0
2. Expand camera track — 9 sub-tracks appear (6 base + focal length if zoom + focus + aperture)
3. Scrub playhead — live values update on all sub-tracks
4. Click a diamond — it turns cyan, playhead moves, 3D view updates
5. Click another diamond — previous deselects, new one selects
6. Collapse camera track — sub-tracks hide, main diamonds still visible
7. Switch to a prime lens set — Focal Length sub-track disappears
8. Switch back to zoom or no lens set — Focal Length sub-track reappears

## Performance Considerations

- Diamond VisualElements are pooled per track row (grow/shrink, never recreate from scratch)
- `SyncTrackVisuals()` runs every frame in `Update()` — keep it cheap. Only update diamond positions and values, don't rebuild DOM.
- `GetAllCameraKeyframeTimes()` allocates a SortedSet + List each call. For typical shot keyframe counts (<100), this is negligible. If profiling shows issues, cache and invalidate on keyframe add/remove.

## References

- Spec: `docs/specs/milestone-3.2-keyframe-animation-spec.md` lines 100–244
- Ticket: FRA-60
- Similar UI pattern: `ShotTrackStrip` (`UI/Timeline/ShotTrackStrip.cs`) — VisualElement with child blocks positioned by time
- Similar state pattern: `Selection` (`Core/Common/Selection.cs`) — single-element selection with observable
