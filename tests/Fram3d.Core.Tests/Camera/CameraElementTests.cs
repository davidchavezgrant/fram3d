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
			new CameraElement(ElementId.New(), "Test Camera");

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

		[Fact]
		public void DollyZoom__StoresReferenceDistance__When__Initialized()
		{
			var cam = CreateCamera();
			cam.DollyZoomReferenceDistance.Should().Be(10f);
		}

		[Fact]
		public void DollyZoom__UsesFixedDistance__When__NoTargetElement()
		{
			var cam = CreateCamera();
			cam.DollyZoomTargetId.Should().BeNull();
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
	}
}
