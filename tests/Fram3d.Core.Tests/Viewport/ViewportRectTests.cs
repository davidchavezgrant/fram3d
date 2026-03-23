using FluentAssertions;
using Fram3d.Core.Camera;
using Fram3d.Core.Viewport;
using Xunit;

namespace Fram3d.Core.Tests.Viewport
{
	public class ViewportRectTests
	{
		// --- Full viewport (no constraint needed) ---

		[Fact]
		public void Compute__ReturnsFullViewport__When__AspectsMatch()
		{
			var vp = ViewportRect.Compute(1.778f, 1.778f);

			vp.X.Should().Be(0f);
			vp.Y.Should().Be(0f);
			vp.Width.Should().Be(1f);
			vp.Height.Should().Be(1f);
		}

		[Fact]
		public void Compute__ReturnsFullViewport__When__AspectsWithinEpsilon()
		{
			// 0.001 epsilon — aspects that differ by less than this are treated as equal
			var vp = ViewportRect.Compute(1.778f, 1.7785f);

			vp.X.Should().Be(0f);
			vp.Y.Should().Be(0f);
			vp.Width.Should().Be(1f);
			vp.Height.Should().Be(1f);
		}

		[Fact]
		public void Compute__ReturnsFullViewport__When__ScreenAspectIsZero()
		{
			var vp = ViewportRect.Compute(1.778f, 0f);

			vp.Width.Should().Be(1f);
			vp.Height.Should().Be(1f);
		}

		[Fact]
		public void Compute__ReturnsFullViewport__When__SensorAspectIsZero()
		{
			var vp = ViewportRect.Compute(0f, 1.778f);

			vp.Width.Should().Be(1f);
			vp.Height.Should().Be(1f);
		}

		[Fact]
		public void Compute__ReturnsFullViewport__When__NegativeAspect()
		{
			var vp = ViewportRect.Compute(-1f, 1.778f);

			vp.Width.Should().Be(1f);
			vp.Height.Should().Be(1f);
		}

		// --- Letterbox (sensor wider than screen) ---

		[Fact]
		public void Compute__Letterboxes__When__SensorWiderThanScreen()
		{
			// 2.39:1 sensor on 16:9 screen
			var vp = ViewportRect.Compute(2.39f, 16f / 9f);

			vp.X.Should().Be(0f);
			vp.Width.Should().Be(1f);
			vp.Height.Should().BeLessThan(1f);
			vp.Y.Should().BeGreaterThan(0f);
		}

		[Fact]
		public void Compute__LetterboxIsCentered__When__SensorWiderThanScreen()
		{
			var vp = ViewportRect.Compute(2.39f, 16f / 9f);

			// Y offset + height + Y offset = 1
			var topBar    = vp.Y;
			var bottomBar = 1f - vp.Y - vp.Height;
			topBar.Should().BeApproximately(bottomBar, 0.001f);
		}

		[Fact]
		public void Compute__LetterboxHeightIsCorrect__When__SensorWiderThanScreen()
		{
			// height = screenAspect / sensorAspect
			var screenAspect = 16f / 9f;
			var sensorAspect = 2.39f;
			var vp           = ViewportRect.Compute(sensorAspect, screenAspect);

			vp.Height.Should().BeApproximately(screenAspect / sensorAspect, 0.001f);
		}

		// --- Pillarbox (sensor narrower than screen) ---

		[Fact]
		public void Compute__Pillarboxes__When__SensorNarrowerThanScreen()
		{
			// 4:3 sensor on 16:9 screen
			var vp = ViewportRect.Compute(4f / 3f, 16f / 9f);

			vp.Y.Should().Be(0f);
			vp.Height.Should().Be(1f);
			vp.Width.Should().BeLessThan(1f);
			vp.X.Should().BeGreaterThan(0f);
		}

		[Fact]
		public void Compute__PillarboxIsCentered__When__SensorNarrowerThanScreen()
		{
			var vp = ViewportRect.Compute(4f / 3f, 16f / 9f);

			var leftBar  = vp.X;
			var rightBar = 1f - vp.X - vp.Width;
			leftBar.Should().BeApproximately(rightBar, 0.001f);
		}

		[Fact]
		public void Compute__PillarboxWidthIsCorrect__When__SensorNarrowerThanScreen()
		{
			// width = sensorAspect / screenAspect
			var screenAspect = 16f / 9f;
			var sensorAspect = 4f / 3f;
			var vp           = ViewportRect.Compute(sensorAspect, screenAspect);

			vp.Width.Should().BeApproximately(sensorAspect / screenAspect, 0.001f);
		}

		// --- Edge cases ---

		[Fact]
		public void Compute__FullWidth__When__Letterbox()
		{
			var vp = ViewportRect.Compute(2.39f, 16f / 9f);

			vp.X.Should().Be(0f);
			vp.Width.Should().Be(1f);
		}

		[Fact]
		public void Compute__FullHeight__When__Pillarbox()
		{
			var vp = ViewportRect.Compute(4f / 3f, 16f / 9f);

			vp.Y.Should().Be(0f);
			vp.Height.Should().Be(1f);
		}

		[Fact]
		public void Compute__SquareSensorOn16x9__When__ExtremePillarbox()
		{
			// 1:1 sensor on 16:9 screen — significant pillarbox
			var vp = ViewportRect.Compute(1f, 16f / 9f);

			vp.Width.Should().BeApproximately(9f / 16f, 0.001f);
			vp.Height.Should().Be(1f);
		}
	}
}
