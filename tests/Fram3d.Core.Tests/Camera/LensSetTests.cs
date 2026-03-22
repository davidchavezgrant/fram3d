using FluentAssertions;
using Fram3d.Core.Camera;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class LensSetTests
	{
		// --- Prime set computed properties ---

		[Fact]
		public void CloseFocusM__ReturnsMinAcrossSpecs__When__PrimeSetWithVariedCloseFocus()
		{
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0.3f),
				new LensSpec(50f, 1.4f, 0.45f),
				new LensSpec(85f, 1.4f, 0.8f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			set.CloseFocusM.Should().Be(0.3f);
		}

		[Fact]
		public void CloseFocusM__IgnoresZeroValues__When__SomeSpecsHaveNoCloseFocus()
		{
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0f),
				new LensSpec(50f, 1.4f, 0.45f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			set.CloseFocusM.Should().Be(0.45f);
		}

		[Fact]
		public void CloseFocusM__ReturnsZero__When__NoSpecsHaveCloseFocus()
		{
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0f),
				new LensSpec(50f, 1.4f, 0f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			set.CloseFocusM.Should().Be(0f);
		}

		[Fact]
		public void MaxAperture__ReturnsMinTStopAcrossSpecs__When__PrimeSetWithVariedAperture()
		{
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0.3f),
				new LensSpec(50f, 2.0f, 0.4f),
				new LensSpec(85f, 2.8f, 0.8f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			// Min T-stop = widest aperture across the set = T1.4
			set.MaxAperture.Should().Be(1.4f);
		}

		[Fact]
		public void MaxAperture__IgnoresZeroValues__When__SomeSpecsHaveNoAperture()
		{
			var specs = new[]
			{
				new LensSpec(35f, 0f, 0.3f),
				new LensSpec(50f, 2.0f, 0.4f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			set.MaxAperture.Should().Be(2.0f);
		}

		[Fact]
		public void MaxFocalLength__ReturnsLastFocalLength__When__PrimeSet()
		{
			var set = new LensSet("Test", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f);

			set.MaxFocalLength.Should().Be(100f);
		}

		[Fact]
		public void MinFocalLength__ReturnsFirstFocalLength__When__PrimeSet()
		{
			var set = new LensSet("Test", new float[] { 18, 25, 35, 50, 75, 100 }, false, 1.0f);

			set.MinFocalLength.Should().Be(18f);
		}

		[Fact]
		public void MaxFocalLength__ReturnsZero__When__EmptyFocalLengths()
		{
			var set = new LensSet("Test", new LensSpec[0], false, 1.0f);

			set.MaxFocalLength.Should().Be(0f);
			set.MinFocalLength.Should().Be(0f);
		}

		[Fact]
		public void CloseFocusM__ReturnsZero__When__EmptySpecs()
		{
			var set = new LensSet("Test", new LensSpec[0], false, 1.0f);

			set.CloseFocusM.Should().Be(0f);
		}

		[Fact]
		public void MaxAperture__ReturnsZero__When__EmptySpecs()
		{
			var set = new LensSet("Test", new LensSpec[0], false, 1.0f);

			set.MaxAperture.Should().Be(0f);
		}

		// --- Zoom lens properties ---

		[Fact]
		public void IsZoom__ReturnsTrue__When__ZoomConstructor()
		{
			var set = new LensSet("Zoom", 24f, 70f, false, 1.0f);

			set.IsZoom.Should().BeTrue();
			set.MinFocalLength.Should().Be(24f);
			set.MaxFocalLength.Should().Be(70f);
		}

		[Fact]
		public void IsZoom__ReturnsFalse__When__PrimeConstructor()
		{
			var set = new LensSet("Prime", new float[] { 50 }, false, 1.0f);

			set.IsZoom.Should().BeFalse();
		}

		[Fact]
		public void ZoomConstructor__StoresApertureAndCloseFocus__When__Provided()
		{
			var set = new LensSet("Zoom", 24f, 70f, false, 1.0f, maxAperture: 2.8f, closeFocusM: 0.38f);

			set.MaxAperture.Should().Be(2.8f);
			set.CloseFocusM.Should().Be(0.38f);
		}

		// --- Boundary: specs.Length > 0 ternary ---

		[Fact]
		public void CloseFocusM__UsesMinNotMax__When__MultipleDifferentValues()
		{
			// If Min() were mutated to Max(), we'd get 0.8 instead of 0.3
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0.3f),
				new LensSpec(85f, 1.4f, 0.8f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			set.CloseFocusM.Should().Be(0.3f);
			set.CloseFocusM.Should().NotBe(0.8f);
		}

		[Fact]
		public void MaxAperture__UsesMinNotMax__When__MultipleDifferentValues()
		{
			// Min T-stop across set = widest common aperture. If Min() → Max(), wrong.
			var specs = new[]
			{
				new LensSpec(35f, 1.4f, 0.3f),
				new LensSpec(85f, 4.0f, 0.8f)
			};
			var set = new LensSet("Test", specs, false, 1.0f);

			set.MaxAperture.Should().Be(1.4f);
			set.MaxAperture.Should().NotBe(4.0f);
		}

		[Fact]
		public void CloseFocusM__DefaultsToZero__When__SpecsLengthIsZero()
		{
			// The ternary `specs.Length > 0 ? ... : 0f` — if mutated to true, would try to query empty array
			var set = new LensSet("Empty", new LensSpec[0], false, 1.0f);

			set.CloseFocusM.Should().Be(0f);
		}

		[Fact]
		public void MaxAperture__DefaultsToZero__When__SpecsLengthIsZero()
		{
			var set = new LensSet("Empty", new LensSpec[0], false, 1.0f);

			set.MaxAperture.Should().Be(0f);
		}

		// --- Single lens edge case ---

		[Fact]
		public void MinAndMax__AreSame__When__SingleLens()
		{
			var set = new LensSet("Single", new float[] { 50 }, false, 1.0f);

			set.MinFocalLength.Should().Be(50f);
			set.MaxFocalLength.Should().Be(50f);
		}
	}
}
