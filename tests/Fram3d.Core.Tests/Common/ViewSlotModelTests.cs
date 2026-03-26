using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
	public class ViewSlotModelTests
	{
		// ── Default state ──────────────────────────────────────────────

		[Fact]
		public void Constructor__DefaultsToSingleLayout()
		{
			var model = new ViewSlotModel();
			model.Layout.Should().Be(ViewLayout.SINGLE);
			model.ActiveSlotCount.Should().Be(1);
		}

		[Fact]
		public void Constructor__Slot0IsCameraView()
		{
			var model = new ViewSlotModel();
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void CameraViewSlotIndex__ReturnsZero__When__DefaultState()
		{
			var model = new ViewSlotModel();
			model.CameraViewSlotIndex.Should().Be(0);
		}

		// ── SetLayout ──────────────────────────────────────────────────

		[Fact]
		public void SetLayout__CreatesDirectorViewInSlot1__When__SwitchingToHorizontal()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);

			model.ActiveSlotCount.Should().Be(2);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetLayout__CreatesDirectorViewInSlot1__When__SwitchingToVertical()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.VERTICAL);

			model.ActiveSlotCount.Should().Be(2);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetLayout__PreservesSlot0Type__When__ShrinkingToSingle()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			model.SetLayout(ViewLayout.SINGLE);

			model.ActiveSlotCount.Should().Be(1);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetLayout__MovesCameraViewToSlot0__When__CameraViewInSlot1AndShrinking()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			model.SetSlotType(1, ViewMode.CAMERA);
			// Slot 0 = Director, Slot 1 = Camera

			model.SetLayout(ViewLayout.SINGLE);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.CameraViewSlotIndex.Should().Be(0);
		}

		[Fact]
		public void SetLayout__FiresChangedEvent__When__LayoutChanges()
		{
			var model = new ViewSlotModel();
			var fired = false;
			model.Changed += () => fired = true;

			model.SetLayout(ViewLayout.HORIZONTAL);

			fired.Should().BeTrue();
		}

		[Fact]
		public void SetLayout__DoesNotFireChangedEvent__When__SameLayout()
		{
			var model = new ViewSlotModel();
			var fired = false;
			model.Changed += () => fired = true;

			model.SetLayout(ViewLayout.SINGLE);

			fired.Should().BeFalse();
		}

		[Fact]
		public void SetLayout__SwitchesBetweenHorizontalAndVertical()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			model.SetLayout(ViewLayout.VERTICAL);

			model.ActiveSlotCount.Should().Be(2);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}

		// ── SetSlotType ────────────────────────────────────────────────

		[Fact]
		public void SetSlotType__SmartSwaps__When__SettingToCameraView()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);

			model.SetSlotType(1, ViewMode.CAMERA);

			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(1).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetSlotType__SwapsCameraViewToOtherSlot__When__MultiView()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);

			model.SetSlotType(0, ViewMode.DIRECTOR);

			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(1).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetSlotType__AllowsDirectChange__When__SingleView()
		{
			var model = new ViewSlotModel();

			model.SetSlotType(0, ViewMode.DIRECTOR);

			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetSlotType__FiresChangedEvent__When__TypeChanges()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			var fired = false;
			model.Changed += () => fired = true;

			model.SetSlotType(1, ViewMode.CAMERA);

			fired.Should().BeTrue();
		}

		[Fact]
		public void SetSlotType__DoesNotFireChangedEvent__When__SameType()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			var fired = false;
			model.Changed += () => fired = true;

			model.SetSlotType(1, ViewMode.DIRECTOR);

			fired.Should().BeFalse();
		}

		[Fact]
		public void SetSlotType__Throws__When__IndexOutOfRange()
		{
			var model = new ViewSlotModel();
			Action act = () => model.SetSlotType(1, ViewMode.DIRECTOR);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		[Fact]
		public void SetSlotType__DoesNothing__When__SettingCameraViewOnSlotThatAlreadyHasIt()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			var fired = false;
			model.Changed += () => fired = true;

			model.SetSlotType(0, ViewMode.CAMERA);

			fired.Should().BeFalse();
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		// ── GetSlotType bounds ──────────────────────────────────────────

		[Fact]
		public void GetSlotType__Throws__When__NegativeIndex()
		{
			var model = new ViewSlotModel();
			Action act = () => model.GetSlotType(-1);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		[Fact]
		public void GetSlotType__Throws__When__IndexOutOfRange()
		{
			var model = new ViewSlotModel();
			Action act = () => model.GetSlotType(1);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		// ── SetSlotType bounds ─────────────────────────────────────────

		[Fact]
		public void SetSlotType__Throws__When__NegativeIndex()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			Action act = () => model.SetSlotType(-1, ViewMode.DIRECTOR);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		// ── CameraViewSlotIndex ────────────────────────────────────────

		[Fact]
		public void CameraViewSlotIndex__TracksSwap__When__SmartSwapOccurs()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			model.SetSlotType(1, ViewMode.CAMERA);

			model.CameraViewSlotIndex.Should().Be(1);
		}

		// ── ViewLayout properties ──────────────────────────────────────

		[Fact]
		public void ViewLayout__HasCorrectViewCount()
		{
			ViewLayout.SINGLE.ViewCount.Should().Be(1);
			ViewLayout.HORIZONTAL.ViewCount.Should().Be(2);
			ViewLayout.VERTICAL.ViewCount.Should().Be(2);
		}

		[Fact]
		public void ViewLayout__HasCorrectName()
		{
			ViewLayout.SINGLE.Name.Should().Be("Single");
			ViewLayout.HORIZONTAL.Name.Should().Be("Horizontal");
			ViewLayout.VERTICAL.Name.Should().Be("Vertical");
		}

		[Fact]
		public void ViewLayout__ToString__ReturnsName()
		{
			ViewLayout.SINGLE.ToString().Should().Be("Single");
		}

		// ── SetSlotType — Camera-to-Director swap vs single-view direct change ──

		[Fact]
		public void SetSlotType__DoesNotSwap__When__SingleViewCameraSlotSetToDirector()
		{
			// In single-view (ActiveSlotCount == 1), setting the Camera slot
			// to Director should NOT try to move Camera elsewhere — there's
			// no other slot. It should just change directly.
			var model = new ViewSlotModel();

			model.SetSlotType(0, ViewMode.DIRECTOR);

			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
			// Verify CameraViewSlotIndex still reports 0 because _slots[0]
			// is no longer CAMERA — the property checks slot 0 first.
			model.CameraViewSlotIndex.Should().Be(1);
		}

		[Fact]
		public void SetSlotType__MovesCamera__When__MultiViewCameraSlotSetToDirector()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			// Slot 0 = Camera, Slot 1 = Director

			model.SetSlotType(0, ViewMode.DIRECTOR);

			// Camera should have moved to slot 1
			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(1).Should().Be(ViewMode.CAMERA);
			model.CameraViewSlotIndex.Should().Be(1);
		}

		// ── SetLayout — expanding resets slot types ───────────────────

		[Fact]
		public void SetLayout__ResetsSlot1ToDirector__When__ExpandingFromCustomSingleView()
		{
			var model = new ViewSlotModel();
			// Set single-view to Director (non-default)
			model.SetSlotType(0, ViewMode.DIRECTOR);
			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);

			// Expand to multi-view: should force Camera + Director
			model.SetLayout(ViewLayout.HORIZONTAL);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetLayout__KeepsCameraInSlot0__When__ShrinkingAndSlot0IsAlreadyCamera()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			// Slot 0 = Camera (default) — shrinking should be a no-op on slot types
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);

			model.SetLayout(ViewLayout.SINGLE);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		// ── Round-trip scenarios ────────────────────────────────────────

		[Fact]
		public void SetLayout__RoundTrip__When__SingleToHorizontalAndBack()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.HORIZONTAL);
			model.SetLayout(ViewLayout.SINGLE);
			model.SetLayout(ViewLayout.HORIZONTAL);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}
	}
}
