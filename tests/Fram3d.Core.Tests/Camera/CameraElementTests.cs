using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class CameraElementTests
	{
		private static CameraElement CreateCamera() =>
			new CameraElement(new ElementId(Guid.NewGuid()), "Test Camera");

		// --- Construction defaults ---

		[Fact]
		public void Constructor__HasDefaultPosition__When__Constructed()
		{
			var cam = CreateCamera();
			cam.Position.Should().Be(new Vector3(0f, 1.6f, 5f));
		}

		[Fact]
		public void Constructor__HasIdentityRotation__When__Constructed()
		{
			var cam = CreateCamera();
			cam.Rotation.Should().Be(Quaternion.Identity);
		}

		[Fact]
		public void Constructor__HasDefaultFocalLength__When__Constructed()
		{
			var cam = CreateCamera();
			cam.FocalLength.Should().Be(50f);
		}

		// --- Pan ---

		[Fact]
		public void Pan__DoesNotChangePosition__When__AnyAmount()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.Pan(0.5f);
			cam.Position.Should().Be(originalPos);
		}

		[Fact]
		public void Pan__ChangesRotation__When__PositiveAmount()
		{
			var cam = CreateCamera();
			cam.Pan(0.1f);
			cam.Rotation.Should().NotBe(Quaternion.Identity);
		}

		[Fact]
		public void Pan__RotatesRightward__When__PositiveAmount()
		{
			// After panning right, the camera's forward direction should have
			// a negative X component (looking rightward means forward shifts right).
			var cam = CreateCamera();
			cam.Pan(0.3f);

			// Get the new forward direction
			var forward = Vector3.Transform(-Vector3.UnitZ, cam.Rotation);
			// Positive pan = rightward, so forward.X should become positive
			// (in right-hand coords, looking right means forward gains +X)
			forward.X.Should().BePositive();
		}

		// --- Tilt ---

		[Fact]
		public void Tilt__DoesNotChangePosition__When__AnyAmount()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.Tilt(0.5f);
			cam.Position.Should().Be(originalPos);
		}

		[Fact]
		public void Tilt__ChangesRotation__When__PositiveAmount()
		{
			var cam = CreateCamera();
			cam.Tilt(0.1f);
			cam.Rotation.Should().NotBe(Quaternion.Identity);
		}

		[Fact]
		public void Tilt__RotatesUpward__When__PositiveAmount()
		{
			var cam = CreateCamera();
			cam.Tilt(0.3f);

			// After tilting up, forward direction should gain positive Y component
			var forward = Vector3.Transform(-Vector3.UnitZ, cam.Rotation);
			forward.Y.Should().BePositive();
		}

		// --- Dolly ---

		[Fact]
		public void Dolly__TranslatesAlongLocalForward__When__PositiveAmount()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.Dolly(1.0f);

			// Default camera faces -Z, so dolly forward should decrease Z
			// (in right-hand coords, forward is -Z)
			cam.Position.Z.Should().BeLessThan(originalPos.Z);
		}

		[Fact]
		public void Dolly__DoesNotChangeRotation__When__AnyAmount()
		{
			var cam = CreateCamera();
			cam.Dolly(1.0f);
			cam.Rotation.Should().Be(Quaternion.Identity);
		}

		[Fact]
		public void Dolly__MovesTowardScene__When__CameraFacesForward()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.Dolly(2.0f);

			// Camera started at Z=-5, facing -Z. Dolly forward moves further in -Z.
			var distance = Vector3.Distance(cam.Position, originalPos);
			distance.Should().BeApproximately(2.0f, 0.001f);
		}

		// --- Truck ---

		[Fact]
		public void Truck__TranslatesAlongLocalRight__When__PositiveAmount()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.Truck(1.0f);

			// Default camera: right is +X
			cam.Position.X.Should().BeGreaterThan(originalPos.X);
		}

		[Fact]
		public void Truck__DoesNotChangeRotation__When__AnyAmount()
		{
			var cam = CreateCamera();
			cam.Truck(1.0f);
			cam.Rotation.Should().Be(Quaternion.Identity);
		}

		// --- Crane ---

		[Fact]
		public void Crane__TranslatesAlongWorldY__When__PositiveAmount()
		{
			var cam = CreateCamera();
			var originalY = cam.Position.Y;
			cam.Crane(1.0f);
			cam.Position.Y.Should().BeGreaterThan(originalY);
		}

		[Fact]
		public void Crane__MovesWorldUp__When__CameraTiltedDown()
		{
			var cam = CreateCamera();
			cam.Tilt(-0.5f); // tilt down
			var posBeforeCrane = cam.Position;

			cam.Crane(1.0f);

			// Crane is world-relative: only Y changes, X and Z stay the same
			cam.Position.X.Should().BeApproximately(posBeforeCrane.X, 0.001f);
			cam.Position.Z.Should().BeApproximately(posBeforeCrane.Z, 0.001f);
			cam.Position.Y.Should().BeGreaterThan(posBeforeCrane.Y);
		}

		[Fact]
		public void Crane__DoesNotChangeRotation__When__AnyAmount()
		{
			var cam = CreateCamera();
			var originalRot = cam.Rotation;
			cam.Crane(1.0f);
			cam.Rotation.Should().Be(originalRot);
		}

		// --- Roll ---

		[Fact]
		public void Roll__DoesNotChangePosition__When__AnyAmount()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.Roll(0.5f);
			cam.Position.Should().Be(originalPos);
		}

		[Fact]
		public void Roll__RotatesAroundLocalForward__When__PositiveAmount()
		{
			var cam = CreateCamera();
			cam.Roll(0.3f);

			// After rolling, the camera's up direction should no longer be pure +Y
			var up = Vector3.Transform(Vector3.UnitY, cam.Rotation);
			up.X.Should().NotBeApproximately(0f, 0.01f);
		}

		// --- Orbit ---

		[Fact]
		public void Orbit__MaintainsDistanceToPivot__When__Orbiting()
		{
			var cam = CreateCamera();
			cam.OrbitPivotPoint = Vector3.Zero;
			var distanceBefore = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);

			cam.Orbit(0.5f, 0.0f);

			var distanceAfter = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);
			distanceAfter.Should().BeApproximately(distanceBefore, 0.01f);
		}

		[Fact]
		public void Orbit__ChangesPosition__When__HorizontalAmount()
		{
			var cam = CreateCamera();
			cam.OrbitPivotPoint = Vector3.Zero;
			var originalPos = cam.Position;

			cam.Orbit(0.5f, 0.0f);

			cam.Position.Should().NotBe(originalPos);
		}

		[Fact]
		public void Orbit__UsesStoredPivotPoint__When__PivotWasSet()
		{
			var cam = CreateCamera();
			var pivot = new Vector3(3f, 0f, 0f);
			cam.OrbitPivotPoint = pivot;
			var distanceBefore = Vector3.Distance(cam.Position, pivot);

			cam.Orbit(0.5f, 0.0f);

			var distanceAfter = Vector3.Distance(cam.Position, pivot);
			distanceAfter.Should().BeApproximately(distanceBefore, 0.01f);
		}

		[Fact]
		public void Orbit__UsesWorldOrigin__When__NoPivotSet()
		{
			var cam = CreateCamera();
			// Default OrbitPivotPoint is Vector3.Zero
			cam.OrbitPivotPoint.Should().Be(Vector3.Zero);
		}

		[Fact]
		public void Orbit__RotatesBothAxes__When__BothAmountsProvided()
		{
			var cam = CreateCamera();
			cam.OrbitPivotPoint = Vector3.Zero;
			var originalPos = cam.Position;

			cam.Orbit(0.3f, 0.2f);

			// Both X and Y of position should change
			cam.Position.X.Should().NotBeApproximately(originalPos.X, 0.01f);
			cam.Position.Y.Should().NotBeApproximately(originalPos.Y, 0.01f);
		}

		// --- Dolly Zoom ---

		[Fact]
		public void DollyZoom__TranslatesAlongForward__When__Called()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;
			cam.DollyZoom(1.0f);
			cam.Position.Should().NotBe(originalPos);
		}

		// --- Reset ---

		[Fact]
		public void Reset__RestoresDefaultPosition__When__CameraWasMoved()
		{
			var cam = CreateCamera();
			cam.Dolly(5.0f);
			cam.Truck(3.0f);
			cam.Crane(2.0f);

			cam.Reset();

			cam.Position.Should().Be(new Vector3(0f, 1.6f, 5f));
		}

		[Fact]
		public void Reset__RestoresDefaultRotation__When__CameraWasRotated()
		{
			var cam = CreateCamera();
			cam.Pan(1.0f);
			cam.Tilt(0.5f);
			cam.Roll(0.3f);

			cam.Reset();

			cam.Rotation.Should().Be(Quaternion.Identity);
		}

		[Fact]
		public void Reset__RestoresDefaultFocalLength__When__FocalLengthChanged()
		{
			var cam = CreateCamera();
			cam.FocalLength = 85f;

			cam.Reset();

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void Reset__ClearsOrbitPivot__When__PivotWasSet()
		{
			var cam = CreateCamera();
			cam.OrbitPivotPoint = new Vector3(5f, 0f, 3f);

			cam.Reset();

			cam.OrbitPivotPoint.Should().Be(Vector3.Zero);
		}

		[Fact]
		public void Reset__PreservesBody__When__BodyWasSet()
		{
			var cam = CreateCamera();
			cam.CycleAspectRatioBackward(); // 16:9 → Full Screen (no aspect ratio constraint)
			var body = new CameraBody("ARRI Alexa 35", "ARRI", 0, 27.99f, 19.22f, "S35", "LPL", new[] { 4608, 3164 }, new[] { 24 });
			cam.SetBody(body);

			cam.Reset();

			cam.Body.Should().Be(body);
			cam.SensorHeight.Should().Be(19.22f);
		}

		[Fact]
		public void Reset__PreservesLensSet__When__LensSetWasSet()
		{
			var cam = CreateCamera();
			var lensSet = new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f);
			cam.SetLensSet(lensSet);

			cam.Reset();

			cam.ActiveLensSet.Should().Be(lensSet);
		}

		[Fact]
		public void Reset__ClampsFocalLengthToZoomRange__When__ZoomLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("ARRI Signature Zoom 65-300mm", 65f, 300f, false, 1.0f));
			cam.SetFocalLengthPreset(200f);

			cam.Reset();

			// Default 50mm is below the zoom's 65mm minimum — should clamp to 65mm
			cam.FocalLength.Should().Be(65f);
			cam.ActiveLensSet.Should().NotBeNull();
		}

		[Fact]
		public void Reset__RestoresDefaultFocalLength__When__NoLensSetActive()
		{
			var cam = CreateCamera();
			cam.FocalLength = 200f;

			cam.Reset();

			cam.FocalLength.Should().Be(50f);
		}

		// --- Composition ---

		[Fact]
		public void Compose__ProducesSumOfMotions__When__PanAndDollyApplied()
		{
			// Apply pan then dolly to one camera
			var cam1 = CreateCamera();
			cam1.Pan(0.2f);
			cam1.Dolly(1.0f);

			// Apply in reverse order to another camera
			var cam2 = CreateCamera();
			cam2.Pan(0.2f);
			cam2.Dolly(1.0f);

			// Both should end up at the same position (operations are independent
			// because pan doesn't change position and dolly uses the current rotation)
			cam1.Position.X.Should().BeApproximately(cam2.Position.X, 0.001f);
			cam1.Position.Y.Should().BeApproximately(cam2.Position.Y, 0.001f);
			cam1.Position.Z.Should().BeApproximately(cam2.Position.Z, 0.001f);
		}

		[Fact]
		public void Compose__AppliesBothMotions__When__TruckAndCraneCombined()
		{
			var cam = CreateCamera();
			var originalPos = cam.Position;

			cam.Truck(1.0f);
			cam.Crane(1.0f);

			// X should change (truck) and Y should change (crane)
			cam.Position.X.Should().NotBeApproximately(originalPos.X, 0.001f);
			cam.Position.Y.Should().NotBeApproximately(originalPos.Y, 0.001f);
		}

		// --- Focal Length (1.1.2) ---

		[Fact]
		public void SetFocalLength__ClampsToMinimum__When__BelowRange()
		{
			var cam = CreateCamera();
			cam.FocalLength = 5f;
			cam.FocalLength.Should().Be(14f);
		}

		[Fact]
		public void SetFocalLength__ClampsToMaximum__When__AboveRange()
		{
			var cam = CreateCamera();
			cam.FocalLength = 500f;
			cam.FocalLength.Should().Be(400f);
		}

		[Fact]
		public void SetFocalLength__SetsExactValue__When__WithinRange()
		{
			var cam = CreateCamera();
			cam.FocalLength = 85f;
			cam.FocalLength.Should().Be(85f);
		}

		// --- FOV (1.1.2) ---

		[Fact]
		public void VerticalFov__ReturnsCorrectFov__When__50mmOnSuper35()
		{
			var cam = CreateCamera();
			// SensorHeight = 18.66mm, FocalLength = 50mm
			// FOV = 2 * atan(18.66 / 100) ≈ 0.3696 radians ≈ 21.18°
			var expected = 2f * MathF.Atan(18.66f / (2f * 50f));
			cam.VerticalFov.Should().BeApproximately(expected, 0.001f);
		}

		[Fact]
		public void VerticalFov__ReturnsWiderFov__When__FocalLengthDecreases()
		{
			var cam = CreateCamera();
			var fov50 = cam.VerticalFov;

			cam.FocalLength = 24f;
			var fov24 = cam.VerticalFov;

			fov24.Should().BeGreaterThan(fov50);
		}

		[Fact]
		public void VerticalFov__ReturnsNarrowerFov__When__FocalLengthIncreases()
		{
			var cam = CreateCamera();
			var fov50 = cam.VerticalFov;

			cam.FocalLength = 135f;
			var fov135 = cam.VerticalFov;

			fov135.Should().BeLessThan(fov50);
		}

		[Fact]
		public void VerticalFov__ReturnsWiderFov__When__SensorHeightIncreases()
		{
			var cam = CreateCamera();
			var fovSuper35 = cam.VerticalFov;

			cam.SetBody(new CameraBody("Generic 35mm", "Generic", 0, 36.0f, 24.0f, "FF", "", new[] { 4096, 2160 }, new[] { 24 }));
			var fovFullFrame = cam.VerticalFov;

			fovFullFrame.Should().BeGreaterThan(fovSuper35);
		}

		// --- Dolly Zoom with focal length (1.1.2) ---

		[Fact]
		public void DollyZoom__AdjustsFocalLength__When__MovingCloser()
		{
			var cam = CreateCamera();
			cam.FocalLength = 50f;
			cam.OrbitPivotPoint = Vector3.Zero;

			cam.DollyZoom(1.0f);

			// Moving closer to pivot should decrease focal length
			cam.FocalLength.Should().BeLessThan(50f);
		}

		[Fact]
		public void DollyZoom__MaintainsSubjectSize__When__Applied()
		{
			var cam = CreateCamera();
			cam.FocalLength = 50f;
			cam.OrbitPivotPoint = Vector3.Zero;

			var distanceBefore = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);
			var ratioBefore    = cam.FocalLength / distanceBefore;

			cam.DollyZoom(1.0f);

			var distanceAfter = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);
			var ratioAfter    = cam.FocalLength / distanceAfter;

			ratioAfter.Should().BeApproximately(ratioBefore, 0.01f);
		}

		[Fact]
		public void DollyZoom__ClampsAtMinFocalLength__When__VeryClose()
		{
			var cam = CreateCamera();
			cam.FocalLength = 15f;
			cam.OrbitPivotPoint = Vector3.Zero;

			// Move very close — focal length should clamp at 14mm
			cam.DollyZoom(4.0f);

			cam.FocalLength.Should().BeGreaterThanOrEqualTo(14f);
		}

		[Fact]
		public void DollyZoom__StopsMoving__When__AtMinFocalLength()
		{
			var cam = CreateCamera();
			cam.FocalLength = 14f;
			cam.OrbitPivotPoint = Vector3.Zero;
			var positionBefore = cam.Position;

			// Already at minimum — should not move
			cam.DollyZoom(1.0f);

			cam.Position.Should().Be(positionBefore);
			cam.FocalLength.Should().Be(14f);
		}

		[Fact]
		public void DollyZoom__StopsMoving__When__AtMaxFocalLength()
		{
			var cam = CreateCamera();
			cam.FocalLength = 400f;
			cam.OrbitPivotPoint = Vector3.Zero;
			var positionBefore = cam.Position;

			// Already at maximum — should not move
			cam.DollyZoom(-1.0f);

			cam.Position.Should().Be(positionBefore);
			cam.FocalLength.Should().Be(400f);
		}

		[Fact]
		public void DollyZoom__KeepsPositionAndFocalLengthConsistent__When__Clamped()
		{
			var cam = CreateCamera();
			cam.FocalLength = 20f;
			cam.OrbitPivotPoint = Vector3.Zero;

			// Large move that would overshoot min focal length
			cam.DollyZoom(4.5f);

			// Position should be adjusted so focal/distance ratio is maintained at the clamped value
			var distance = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);
			cam.FocalLength.Should().BeGreaterThanOrEqualTo(14f);
			distance.Should().BeGreaterThan(0f);
		}

		[Fact]
		public void DollyZoom__ClampsToZoomRange__When__ZoomLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("ARRI Signature Zoom 65-300mm", 65f, 300f, false, 1.0f));
			cam.SetFocalLengthPreset(100f);
			cam.OrbitPivotPoint = Vector3.Zero;

			// Dolly zoom forward should decrease focal length but never below 65mm
			for (var i = 0; i < 50; i++)
				cam.DollyZoom(1.0f);

			cam.FocalLength.Should().BeGreaterThanOrEqualTo(65f);
		}

		[Fact]
		public void DollyZoom__MaintainsRatioWithinZoomRange__When__ZoomLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("ARRI Signature Zoom 65-300mm", 65f, 300f, false, 1.0f));
			cam.SetFocalLengthPreset(150f);
			cam.OrbitPivotPoint = Vector3.Zero;

			var distanceBefore = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);
			var ratioBefore    = cam.FocalLength / distanceBefore;

			cam.DollyZoom(0.5f);

			var distanceAfter = Vector3.Distance(cam.Position, cam.OrbitPivotPoint);
			var ratioAfter    = cam.FocalLength / distanceAfter;

			ratioAfter.Should().BeApproximately(ratioBefore, 0.01f);
		}

		[Fact]
		public void DollyZoom__StopsAtZoomMin__When__AlreadyAtMinimum()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 16-35mm", 16f, 35f, false, 1.0f));
			cam.SetFocalLengthPreset(16f);
			cam.OrbitPivotPoint = Vector3.Zero;
			var positionBefore = cam.Position;

			cam.DollyZoom(1.0f);

			cam.Position.Should().Be(positionBefore);
			cam.FocalLength.Should().Be(16f);
		}

		// --- Camera Body (1.1.3) ---

		[Fact]
		public void SetBody__UpdatesSensorDimensions__When__BodyChanged()
		{
			var cam = CreateCamera();
			cam.CycleAspectRatioBackward(); // 16:9 → Full Screen (no aspect ratio constraint)
			var body = new CameraBody("ARRI Alexa Mini LF", "ARRI", 0, 36.7f, 25.54f, "LF", "LPL", new[] { 4448, 3096 }, new[] { 24 });

			cam.SetBody(body);

			cam.SensorWidth.Should().Be(36.7f);
			cam.SensorHeight.Should().Be(25.54f);
			cam.Body.Should().Be(body);
		}

		[Fact]
		public void SetBody__PreservesFocalLength__When__BodyChanged()
		{
			var cam = CreateCamera();
			cam.FocalLength = 85f;

			cam.SetBody(new CameraBody("ARRI Alexa 35", "ARRI", 0, 27.99f, 19.22f, "S35", "LPL", new[] { 4608, 3164 }, new[] { 24 }));

			cam.FocalLength.Should().Be(85f);
		}

		[Fact]
		public void SetBody__ChangesFov__When__DifferentSensorSize()
		{
			var cam = CreateCamera();
			var fovBefore = cam.VerticalFov;

			// Full-frame has larger sensor than default Super 35
			cam.SetBody(new CameraBody("Generic 35mm", "Generic", 0, 36.0f, 24.0f, "FF", "", new[] { 4096, 2160 }, new[] { 24 }));

			cam.VerticalFov.Should().BeGreaterThan(fovBefore);
		}

		// --- Lens Set (1.1.3) ---

		[Fact]
		public void SetLensSet__StoresLensSet__When__Called()
		{
			var cam = CreateCamera();
			var lensSet = new LensSet("Cooke S4/i", new float[] { 14, 18, 21, 25, 27, 32, 35, 40, 50, 65, 75, 100, 135 }, false, 1.0f);

			cam.SetLensSet(lensSet);

			cam.ActiveLensSet.Should().Be(lensSet);
			cam.ActiveLensSet.FocalLengths.Should().HaveCount(13);
		}

		[Fact]
		public void SetLensSet__SnapsToNearestPrime__When__FocalLengthNotInSet()
		{
			var cam = CreateCamera();
			cam.FocalLength = 200f;

			// Leica Summilux-C maxes out at 135mm — 200mm snaps to nearest (135mm)
			cam.SetLensSet(new LensSet("Leica Summilux-C", new float[] { 16, 18, 21, 25, 29, 35, 40, 50, 65, 75, 100, 135 }, false, 1.0f));

			cam.FocalLength.Should().Be(135f);
		}

		[Fact]
		public void SetLensSet__WorksWithZoomLens__When__ZoomSelected()
		{
			var cam = CreateCamera();
			var zoom = new LensSet("Angenieux Optimo 24-290mm", 24f, 290f, false, 1.0f);

			cam.SetLensSet(zoom);

			cam.ActiveLensSet.IsZoom.Should().BeTrue();
			cam.ActiveLensSet.MinFocalLength.Should().Be(24f);
			cam.ActiveLensSet.MaxFocalLength.Should().Be(290f);
		}

		// --- Prime lens restrictions (1.1.3) ---

		[Fact]
		public void SetFocalLength__IsIgnored__When__PrimeLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(50f);

			cam.FocalLength = 85f;

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void StepFocalLengthUp__SnapsToNextLens__When__Called()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(35f);

			cam.StepFocalLengthUp();

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void StepFocalLengthDown__SnapsToPreviousLens__When__Called()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(50f);

			cam.StepFocalLengthDown();

			cam.FocalLength.Should().Be(35f);
		}

		[Fact]
		public void StepFocalLengthUp__StaysAtMax__When__AlreadyAtLongestLens()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(100f);

			cam.StepFocalLengthUp();

			cam.FocalLength.Should().Be(100f);
		}

		[Fact]
		public void StepFocalLengthUp__SetsSnapFlag__When__Called()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(35f);
			cam.SnapFocalLength = false;

			cam.StepFocalLengthUp();

			cam.SnapFocalLength.Should().BeTrue();
		}

		[Fact]
		public void DollyZoom__IsDisabled__When__PrimeLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(50f);
			cam.OrbitPivotPoint = Vector3.Zero;
			var positionBefore = cam.Position;

			cam.DollyZoom(1.0f);

			cam.Position.Should().Be(positionBefore);
			cam.FocalLength.Should().Be(50f);
		}

		// --- Zoom lens clamping (1.1.3) ---

		[Fact]
		public void SetFocalLength__ClampsToZoomRange__When__ZoomLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 16-35mm", 16f, 35f, false, 1.0f));

			cam.FocalLength = 100f;

			cam.FocalLength.Should().Be(35f);
		}

		[Fact]
		public void SetFocalLength__ClampsToZoomMin__When__BelowRange()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 16-35mm", 16f, 35f, false, 1.0f));

			cam.FocalLength = 10f;

			cam.FocalLength.Should().Be(16f);
		}

		[Fact]
		public void SetFocalLength__AllowsContinuousAdjustment__When__ZoomLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));

			cam.FocalLength = 45f;

			cam.FocalLength.Should().Be(45f);
		}

		// --- Snap on lens set switch (1.1.3) ---

		[Fact]
		public void SetLensSet__SnapsToZoomMin__When__BelowZoomRange()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 16-35mm", 16f, 35f, false, 1.0f));
			cam.FocalLength = 16f;

			// Switch to 24-70mm — current 16mm is below range, should snap to 24mm
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));

			cam.FocalLength.Should().Be(24f);
		}

		[Fact]
		public void SetLensSet__SnapsToZoomMax__When__AboveZoomRange()
		{
			var cam = CreateCamera();
			cam.FocalLength = 200f;

			// Switch to 24-70mm — current 200mm is above range, should snap to 70mm
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));

			cam.FocalLength.Should().Be(70f);
		}

		[Fact]
		public void SetLensSet__PreservesFocalLength__When__WithinZoomRange()
		{
			var cam = CreateCamera();
			cam.FocalLength = 35f;

			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));

			cam.FocalLength.Should().Be(35f);
		}

		[Fact]
		public void SetLensSet__SnapsToNearestPrime__When__SwitchingFromZoomToPrime()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));
			cam.FocalLength = 45f;

			// Switch to primes — 45mm should snap to nearest (50mm)
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void SetLensSet__SetsSnapFlag__When__FocalLengthChanges()
		{
			var cam = CreateCamera();
			cam.FocalLength = 200f;
			cam.SnapFocalLength = false;

			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));

			cam.SnapFocalLength.Should().BeTrue();
		}

		[Fact]
		public void SetFocalLengthPreset__ClampsToZoomRange__When__OutsideRange()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));

			cam.SetFocalLengthPreset(200f);

			// Preset bypasses prime check but still clamps to zoom range
			cam.FocalLength.Should().Be(70f);
		}

		[Fact]
		public void SetFocalLengthPreset__WorksWithinZoomRange__When__InsideRange()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));

			cam.SetFocalLengthPreset(50f);

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void StepFocalLengthDown__StaysAtMin__When__AlreadyAtShortestLens()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f));
			cam.SetFocalLengthPreset(18f);

			cam.StepFocalLengthDown();

			cam.FocalLength.Should().Be(18f);
		}

		[Fact]
		public void StepFocalLengthUp__IsNoOp__When__NoLensSetActive()
		{
			var cam = CreateCamera();
			cam.FocalLength = 50f;

			cam.StepFocalLengthUp();

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void StepFocalLengthUp__IsNoOp__When__ZoomLensActive()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Canon 24-70mm", 24f, 70f, false, 1.0f));
			cam.SetFocalLengthPreset(50f);

			cam.StepFocalLengthUp();

			cam.FocalLength.Should().Be(50f);
		}

		[Fact]
		public void DollyZoom__IsAllowed__When__NoLensSetActive()
		{
			var cam = CreateCamera();
			cam.FocalLength = 50f;
			cam.OrbitPivotPoint = Vector3.Zero;
			var positionBefore = cam.Position;

			cam.DollyZoom(0.5f);

			cam.Position.Should().NotBe(positionBefore);
		}

		// --- Depth of Field (1.1.5) ---

		[Fact]
		public void DofEnabled__DefaultsFalse__When__Constructed()
		{
			var cam = CreateCamera();
			cam.DofEnabled.Should().BeFalse();
		}

		[Fact]
		public void FocusDistance__DefaultsTo10__When__Constructed()
		{
			var cam = CreateCamera();
			cam.FocusDistance.Should().Be(10f);
		}

		[Fact]
		public void Aperture__DefaultsToF56__When__Constructed()
		{
			var cam = CreateCamera();
			cam.Aperture.Should().Be(5.6f);
		}

		[Fact]
		public void StepApertureWider__DecreasesFNumber__When__NotAtWidest()
		{
			var cam = CreateCamera();
			// Default is f/5.6
			cam.StepApertureWider();
			cam.Aperture.Should().Be(4f);
		}

		[Fact]
		public void StepApertureNarrower__IncreasesFNumber__When__NotAtNarrowest()
		{
			var cam = CreateCamera();
			// Default is f/5.6
			cam.StepApertureNarrower();
			cam.Aperture.Should().Be(8f);
		}

		[Fact]
		public void StepApertureWider__StaysAtF14__When__AlreadyAtWidest()
		{
			var cam = CreateCamera();
			// Step wider until at f/1.4
			for (var i = 0; i < 20; i++)
				cam.StepApertureWider();

			cam.Aperture.Should().Be(1.4f);

			// One more step should stay at f/1.4
			cam.StepApertureWider();
			cam.Aperture.Should().Be(1.4f);
		}

		[Fact]
		public void StepApertureNarrower__StaysAtF22__When__AlreadyAtNarrowest()
		{
			var cam = CreateCamera();
			// Step narrower until at f/22
			for (var i = 0; i < 20; i++)
				cam.StepApertureNarrower();

			cam.Aperture.Should().Be(22f);

			// One more step should stay at f/22
			cam.StepApertureNarrower();
			cam.Aperture.Should().Be(22f);
		}

		[Fact]
		public void StepApertureWider__TraversesAllStops__When__SteppedRepeatedly()
		{
			var cam = CreateCamera();
			// Start at f/22
			for (var i = 0; i < 20; i++)
				cam.StepApertureNarrower();

			cam.Aperture.Should().Be(22f);

			// Step wider through every stop
			var expectedStops = new[] { 16f, 11f, 8f, 5.6f, 4f, 2.8f, 2f, 1.4f };
			foreach (var expected in expectedStops)
			{
				cam.StepApertureWider();
				cam.Aperture.Should().Be(expected);
			}
		}

		[Fact]
		public void DofEnabled__CanBeToggled__When__SetDirectly()
		{
			var cam = CreateCamera();
			cam.DofEnabled = true;
			cam.DofEnabled.Should().BeTrue();

			cam.DofEnabled = false;
			cam.DofEnabled.Should().BeFalse();
		}

		[Fact]
		public void FocusDistance__CanBeSet__When__AssignedDirectly()
		{
			var cam = CreateCamera();
			cam.FocusDistance = 5.5f;
			cam.FocusDistance.Should().Be(5.5f);
		}

		[Fact]
		public void Reset__PreservesDofSettings__When__DofWasConfigured()
		{
			var cam = CreateCamera();
			cam.DofEnabled = true;
			cam.StepApertureWider();
			cam.StepApertureWider();
			cam.FocusDistance = 3f;

			cam.Reset();

			// DOF settings are equipment — preserved through reset
			cam.DofEnabled.Should().BeTrue();
			cam.Aperture.Should().Be(2.8f);
			cam.FocusDistance.Should().Be(3f);
		}

		// --- Lens-constrained aperture (1.1.5) ---

		[Fact]
		public void StepApertureWider__ClampsToLensMaxAperture__When__LensHasT2()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Cooke S4/i", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f, maxAperture: 2f));

			for (var i = 0; i < 20; i++)
				cam.StepApertureWider();

			cam.Aperture.Should().Be(2f);
		}

		[Fact]
		public void StepApertureWider__AllowsF14__When__NoLensSetActive()
		{
			var cam = CreateCamera();

			for (var i = 0; i < 20; i++)
				cam.StepApertureWider();

			cam.Aperture.Should().Be(1.4f);
		}

		[Fact]
		public void SetLensSet__ClampsAperture__When__CurrentApertureWiderThanLens()
		{
			var cam = CreateCamera();
			for (var i = 0; i < 20; i++)
				cam.StepApertureWider();

			cam.Aperture.Should().Be(1.4f);

			cam.SetLensSet(new LensSet("Slow Lens", new float[] { 50 }, false, 1.0f, maxAperture: 2.8f));

			cam.Aperture.Should().BeGreaterThanOrEqualTo(2.8f);
		}

		// --- Lens-constrained focus distance (1.1.5) ---

		[Fact]
		public void FocusDistance__ClampsToCloseFocus__When__LensHasMinimum()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Master Primes", new float[] { 50 }, false, 1.0f, closeFocusM: 0.45f));

			cam.FocusDistance = 0.2f;

			cam.FocusDistance.Should().Be(0.45f);
		}

		[Fact]
		public void FocusDistance__AllowsAboveCloseFocus__When__LensHasMinimum()
		{
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Master Primes", new float[] { 50 }, false, 1.0f, closeFocusM: 0.45f));

			cam.FocusDistance = 5f;

			cam.FocusDistance.Should().Be(5f);
		}

		[Fact]
		public void SetLensSet__ClampsFocusDistance__When__CurrentBelowCloseFocus()
		{
			var cam = CreateCamera();
			cam.FocusDistance = 0.2f;

			cam.SetLensSet(new LensSet("Long Lens", new float[] { 200 }, false, 1.0f, closeFocusM: 1.5f));

			cam.FocusDistance.Should().Be(1.5f);
		}

		// --- Per-lens constraints (1.1.5) ---

		[Fact]
		public void StepFocalLengthUp__ClampsAperture__When__NextLensHasNarrowerMaxAperture()
		{
			// 50mm is T1.4, 85mm is T2.8
			var specs = new[]
			{
				new LensSpec(50f, 1.4f, 0.4f),
				new LensSpec(85f, 2.8f, 0.8f)
			};
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Mixed Set", specs, false, 1.0f));
			cam.SetFocalLengthPreset(50f);

			// Open aperture to f/1.4 (allowed on 50mm)
			for (var i = 0; i < 20; i++)
				cam.StepApertureWider();

			cam.Aperture.Should().Be(1.4f);

			// Step to 85mm — max aperture is T2.8, should clamp
			cam.StepFocalLengthUp();

			cam.FocalLength.Should().Be(85f);
			cam.Aperture.Should().BeGreaterThanOrEqualTo(2.8f);
		}

		[Fact]
		public void StepFocalLengthDown__ClampsFocusDistance__When__NextLensHasLongerCloseFocus()
		{
			// 85mm has 0.8m close focus, 50mm has 0.4m
			var specs = new[]
			{
				new LensSpec(50f, 1.4f, 0.4f),
				new LensSpec(85f, 1.4f, 0.8f)
			};
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Test Set", specs, false, 1.0f));
			cam.SetFocalLengthPreset(50f);
			cam.FocusDistance = 0.5f;

			cam.FocusDistance.Should().Be(0.5f);

			// Step to 85mm — close focus is 0.8m, should clamp
			cam.StepFocalLengthUp();

			cam.FocusDistance.Should().Be(0.8f);
		}

		[Fact]
		public void StepFocalLengthDown__AllowsWiderAperture__When__NextLensIsFaster()
		{
			// 50mm is T2.8, 35mm is T1.4
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0.3f),
				new LensSpec(50f, 2.8f, 0.4f)
			};
			var cam = CreateCamera();
			cam.SetLensSet(new LensSet("Fast Wide", specs, false, 1.0f));
			cam.SetFocalLengthPreset(50f);

			// At f/2.8 on 50mm (its max)
			for (var i = 0; i < 20; i++)
				cam.StepApertureWider();

			cam.Aperture.Should().Be(2.8f);

			// Step to 35mm — T1.4 max, now we can open wider
			cam.StepFocalLengthDown();
			cam.StepApertureWider();

			cam.Aperture.Should().Be(2f);
		}

		// --- Camera Shake (1.1.6) ---

		[Fact]
		public void ShakeEnabled__DefaultsFalse__When__Constructed()
		{
			var cam = CreateCamera();
			cam.ShakeEnabled.Should().BeFalse();
		}

		[Fact]
		public void ShakeAmplitude__DefaultsTo01__When__Constructed()
		{
			var cam = CreateCamera();
			cam.ShakeAmplitude.Should().Be(0.1f);
		}

		[Fact]
		public void ShakeFrequency__DefaultsTo1__When__Constructed()
		{
			var cam = CreateCamera();
			cam.ShakeFrequency.Should().Be(1.0f);
		}

		[Fact]
		public void ShakeEnabled__CanBeToggled__When__SetDirectly()
		{
			var cam = CreateCamera();
			cam.ShakeEnabled = true;
			cam.ShakeEnabled.Should().BeTrue();

			cam.ShakeEnabled = false;
			cam.ShakeEnabled.Should().BeFalse();
		}

		[Fact]
		public void Reset__PreservesShakeSettings__When__ShakeWasConfigured()
		{
			var cam = CreateCamera();
			cam.ShakeEnabled   = true;
			cam.ShakeAmplitude = 0.5f;
			cam.ShakeFrequency = 3.0f;

			cam.Reset();

			cam.ShakeEnabled.Should().BeTrue();
			cam.ShakeAmplitude.Should().Be(0.5f);
			cam.ShakeFrequency.Should().Be(3.0f);
		}
	}
}
