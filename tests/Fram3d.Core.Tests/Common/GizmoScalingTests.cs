using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;
namespace Fram3d.Core.Tests.Common
{
    public sealed class GizmoScalingTests
    {
        [Fact]
        public void CalculateZoomScale__MatchesLegacy__When__ReferenceConditions()
        {
            // At 1080p / 65° FOV, should match the old distance * 0.15 formula
            var result = GizmoScaling.CalculateZoomScale(10f, 65f, 1080f);

            result.Should().BeApproximately(10f * 0.15f, 0.02f);
        }

        [Fact]
        public void CalculateZoomScale__ScalesWithDistance__When__DistanceDoubles()
        {
            var near = GizmoScaling.CalculateZoomScale(5f,  65f, 1080f);
            var far  = GizmoScaling.CalculateZoomScale(10f, 65f, 1080f);

            far.Should().BeApproximately(near * 2f, 0.01f);
        }

        [Fact]
        public void CalculateZoomScale__CompensatesForFov__When__FovDoubles()
        {
            var narrow = GizmoScaling.CalculateZoomScale(10f, 32.5f, 1080f);
            var wide   = GizmoScaling.CalculateZoomScale(10f, 65f,   1080f);

            // At half the FOV, objects look 2x bigger on screen, so scale
            // should be half to compensate — keeping screen size constant
            narrow.Should().BeApproximately(wide / 2f, 0.01f);
        }

        [Fact]
        public void CalculateZoomScale__CompensatesForResolution__When__ResolutionDoubles()
        {
            var hd = GizmoScaling.CalculateZoomScale(10f, 65f, 1080f);
            var k4 = GizmoScaling.CalculateZoomScale(10f, 65f, 2160f);

            // At 4K the same world-space size covers half the screen fraction,
            // so scale should be half to keep the pixel footprint constant
            k4.Should().BeApproximately(hd / 2f, 0.01f);
        }

        [Fact]
        public void CalculateZoomScale__ReturnsZero__When__DistanceIsZero()
        {
            var result = GizmoScaling.CalculateZoomScale(0f, 65f, 1080f);

            result.Should().Be(0f);
        }

        [Fact]
        public void CalculateZoomScale__UsesFallback__When__PixelHeightInvalid()
        {
            var result = GizmoScaling.CalculateZoomScale(10f, 65f, 0f);

            result.Should().BeApproximately(10f * 0.15f, 0.001f);
        }

        [Fact]
        public void CalculateZoomScale__UsesFallback__When__FovInvalid()
        {
            var result = GizmoScaling.CalculateZoomScale(10f, 0f, 1080f);

            result.Should().BeApproximately(10f * 0.15f, 0.001f);
        }

        [Fact]
        public void CalculateZoomScale__UsesFallback__When__NegativeFov()
        {
            var result = GizmoScaling.CalculateZoomScale(10f, -30f, 1080f);

            result.Should().BeApproximately(10f * 0.15f, 0.001f);
        }
    }
}
