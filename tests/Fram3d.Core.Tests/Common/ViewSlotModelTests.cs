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
		public void SetLayout__CreatesDirectorViewInSlot1__When__SwitchingToSideBySide()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);

			model.ActiveSlotCount.Should().Be(2);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetLayout__CreatesDirectorAndDesignerInSlots1And2__When__SwitchingToOnePlusTwo()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.ONE_PLUS_TWO);

			model.ActiveSlotCount.Should().Be(3);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(2).Should().Be(ViewMode.DESIGNER);
		}

		[Fact]
		public void SetLayout__PreservesSlot0Type__When__ShrinkingToSingle()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			model.SetLayout(ViewLayout.SINGLE);

			model.ActiveSlotCount.Should().Be(1);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetLayout__PreservesExistingSlotTypes__When__Expanding()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			model.SetSlotType(1, ViewMode.CAMERA);
			// Now slot 0 = Director (smart swap), slot 1 = Camera

			model.SetLayout(ViewLayout.ONE_PLUS_TWO);

			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(1).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(2).Should().Be(ViewMode.DESIGNER);
		}

		[Fact]
		public void SetLayout__MovesCameraViewToSlot0__When__CameraViewSlotExceedsNewCount()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.ONE_PLUS_TWO);
			model.SetSlotType(2, ViewMode.CAMERA);
			// Slot 0 = Camera's old type → stays as is, slot 2 = Camera

			model.SetLayout(ViewLayout.SINGLE);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetLayout__FiresChangedEvent__When__LayoutChanges()
		{
			var model = new ViewSlotModel();
			var fired = false;
			model.Changed += () => fired = true;

			model.SetLayout(ViewLayout.SIDE_BY_SIDE);

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

		// ── SetSlotType ────────────────────────────────────────────────

		[Fact]
		public void SetSlotType__ChangesType__When__NonCameraType()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			model.SetSlotType(1, ViewMode.DESIGNER);

			model.GetSlotType(1).Should().Be(ViewMode.DESIGNER);
		}

		[Fact]
		public void SetSlotType__SmartSwaps__When__SettingToCameraView()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			// Slot 0 = Camera, Slot 1 = Director

			model.SetSlotType(1, ViewMode.CAMERA);

			model.GetSlotType(0).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(1).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetSlotType__SmartSwapsInThreeView__When__SettingToCameraView()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.ONE_PLUS_TWO);
			// Slot 0 = Camera, Slot 1 = Director, Slot 2 = Designer

			model.SetSlotType(2, ViewMode.CAMERA);

			model.GetSlotType(0).Should().Be(ViewMode.DESIGNER);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(2).Should().Be(ViewMode.CAMERA);
		}

		[Fact]
		public void SetSlotType__FiresChangedEvent__When__TypeChanges()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			var fired = false;
			model.Changed += () => fired = true;

			model.SetSlotType(1, ViewMode.DESIGNER);

			fired.Should().BeTrue();
		}

		[Fact]
		public void SetSlotType__DoesNotFireChangedEvent__When__SameType()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
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
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			var fired = false;
			model.Changed += () => fired = true;

			model.SetSlotType(0, ViewMode.CAMERA);

			fired.Should().BeFalse();
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}

		// ── GetSlotType ────────────────────────────────────────────────

		[Fact]
		public void GetSlotType__Throws__When__IndexOutOfRange()
		{
			var model = new ViewSlotModel();
			Action act = () => model.GetSlotType(1);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		// ── CameraViewSlotIndex ────────────────────────────────────────

		[Fact]
		public void CameraViewSlotIndex__TracksSwap__When__SmartSwapOccurs()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			model.SetSlotType(1, ViewMode.CAMERA);

			model.CameraViewSlotIndex.Should().Be(1);
		}

		// ── GetSlotType bounds ──────────────────────────────────────────

		[Fact]
		public void GetSlotType__Throws__When__NegativeIndex()
		{
			var model = new ViewSlotModel();
			Action act = () => model.GetSlotType(-1);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		// ── SetSlotType bounds ─────────────────────────────────────────

		[Fact]
		public void SetSlotType__Throws__When__NegativeIndex()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			Action act = () => model.SetSlotType(-1, ViewMode.DIRECTOR);
			act.Should().Throw<ArgumentOutOfRangeException>();
		}

		// ── Layout expansion with duplicate resolution ─────────────────

		[Fact]
		public void SetLayout__ResolvesSlotDuplicate__When__ExistingSlotMatchesDefault()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			// Slot 0 = Camera, Slot 1 = Director
			model.SetSlotType(1, ViewMode.DESIGNER);
			// Now Slot 0 = Camera, Slot 1 = Designer

			// Expand to 3 views: slot 2 defaults to Designer,
			// but slot 1 already has Designer → slot 2 gets Director
			model.SetLayout(ViewLayout.ONE_PLUS_TWO);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DESIGNER);
			model.GetSlotType(2).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetLayout__DirectorDefaultNotDuplicated__When__ExpandFromSideBySide()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			// Slot 0 = Camera, Slot 1 = Director

			model.SetLayout(ViewLayout.ONE_PLUS_TWO);
			// Slot 1 already Director, Slot 2 should get Designer (not duplicate Director)

			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
			model.GetSlotType(2).Should().Be(ViewMode.DESIGNER);
		}

		// ── Camera View recovery ───────────────────────────────────────

		[Fact]
		public void SetLayout__RecoversCameraView__When__CameraViewInSlot1AndShrinkToSingle()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			model.SetSlotType(1, ViewMode.CAMERA);
			// Slot 0 = Director, Slot 1 = Camera

			model.SetLayout(ViewLayout.SINGLE);

			// Camera View must be in slot 0
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.CameraViewSlotIndex.Should().Be(0);
		}

		// ── ViewLayout properties ──────────────────────────────────────

		[Fact]
		public void ViewLayout__HasCorrectViewCount()
		{
			ViewLayout.SINGLE.ViewCount.Should().Be(1);
			ViewLayout.SIDE_BY_SIDE.ViewCount.Should().Be(2);
			ViewLayout.ONE_PLUS_TWO.ViewCount.Should().Be(3);
		}

		[Fact]
		public void ViewLayout__HasCorrectName()
		{
			ViewLayout.SINGLE.Name.Should().Be("Single");
			ViewLayout.SIDE_BY_SIDE.Name.Should().Be("Side by Side");
			ViewLayout.ONE_PLUS_TWO.Name.Should().Be("One + Two");
		}

		[Fact]
		public void ViewLayout__ToString__ReturnsName()
		{
			ViewLayout.SINGLE.ToString().Should().Be("Single");
		}

		// ── ViewMode properties ────────────────────────────────────────

		[Fact]
		public void ViewMode__Designer__Exists()
		{
			ViewMode.DESIGNER.Name.Should().Be("Designer View");
		}

		// ── Round-trip scenarios ────────────────────────────────────────

		[Fact]
		public void SetLayout__RoundTrip__When__SingleToSideBySideAndBack()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);
			model.SetLayout(ViewLayout.SINGLE);
			model.SetLayout(ViewLayout.SIDE_BY_SIDE);

			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
			model.GetSlotType(1).Should().Be(ViewMode.DIRECTOR);
		}

		[Fact]
		public void SetLayout__FullCycle__When__SingleToThreeViewAndBack()
		{
			var model = new ViewSlotModel();
			model.SetLayout(ViewLayout.ONE_PLUS_TWO);
			model.SetSlotType(2, ViewMode.CAMERA);
			// Slot 0 = Designer, Slot 1 = Director, Slot 2 = Camera

			model.SetLayout(ViewLayout.SINGLE);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);

			model.SetLayout(ViewLayout.ONE_PLUS_TWO);
			model.GetSlotType(0).Should().Be(ViewMode.CAMERA);
		}
	}
}
