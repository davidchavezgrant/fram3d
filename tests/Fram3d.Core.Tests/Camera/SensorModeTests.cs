using FluentAssertions;
using Fram3d.Core.Cameras;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class SensorModeTests
	{
		[Fact]
		public void AspectRatio__ReturnsWidthOverHeight__When__NormalResolution()
		{
			var mode = new SensorMode("C4K", 4096, 2160, 24.89f, 13.12f, 60);

			mode.AspectRatio.Should().BeApproximately(4096f / 2160f, 0.001f);
		}

		[Fact]
		public void AspectRatio__ReturnsZero__When__ResolutionHeightIsZero()
		{
			var mode = new SensorMode("Bad", 4096, 0, 24.89f, 13.12f, 60);

			mode.AspectRatio.Should().Be(0f);
		}
	}
}
