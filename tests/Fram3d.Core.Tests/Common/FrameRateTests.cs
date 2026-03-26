using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
    public class FrameRateTests
    {
        [Fact]
        public void Constructor__ThrowsArgumentOutOfRange__When__FpsIsZero()
        {
            Action act = () => new FrameRate(0.0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Constructor__ThrowsArgumentOutOfRange__When__FpsIsNegative()
        {
            Action act = () => new FrameRate(-24.0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Constructor__Succeeds__When__FpsIsPositive()
        {
            var fr = new FrameRate(24.0);
            fr.Fps.Should().Be(24.0);
        }

        [Fact]
        public void FrameDuration__ReturnsCorrectDuration__When__At24Fps()
        {
            FrameRate.FPS_24.FrameDuration.Should().BeApproximately(1.0 / 24.0, 1e-9);
        }

        [Fact]
        public void FrameDuration__ReturnsCorrectDuration__When__At30Fps()
        {
            FrameRate.FPS_30.FrameDuration.Should().BeApproximately(1.0 / 30.0, 1e-9);
        }

        [Fact]
        public void SnapToFrame__ReturnsExactTime__When__AlreadyOnBoundary()
        {
            var time = new TimePosition(1.0);
            var snapped = FrameRate.FPS_24.SnapToFrame(time);
            snapped.Seconds.Should().BeApproximately(1.0, 1e-9);
        }

        [Fact]
        public void SnapToFrame__SnapsToNearestFrame__When__BetweenBoundaries()
        {
            var time = new TimePosition(0.03);
            var snapped = FrameRate.FPS_24.SnapToFrame(time);
            // 0.03s * 24 = 0.72, rounds to 1, so 1/24 ≈ 0.04167
            snapped.Seconds.Should().BeApproximately(1.0 / 24.0, 1e-9);
        }

        [Fact]
        public void SnapToFrame__ReturnsZero__When__AtTimeZero()
        {
            var snapped = FrameRate.FPS_24.SnapToFrame(TimePosition.ZERO);
            snapped.Seconds.Should().Be(0.0);
        }

        [Fact]
        public void Equals__ReturnsTrue__When__SameFps()
        {
            var a = new FrameRate(24.0);
            var b = new FrameRate(24.0);
            a.Should().Be(b);
            (a == b).Should().BeTrue();
        }

        [Fact]
        public void Equals__ReturnsFalse__When__DifferentFps()
        {
            (FrameRate.FPS_24 != FrameRate.FPS_30).Should().BeTrue();
        }

        [Fact]
        public void Equals__ReturnsFalse__When__ComparedToNull()
        {
            FrameRate.FPS_24.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode__IsSame__When__SameFps()
        {
            var a = new FrameRate(24.0);
            var b = new FrameRate(24.0);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void ToString__ShowsFps__When__Called()
        {
            FrameRate.FPS_24.ToString().Should().Be("24fps");
        }

        [Fact]
        public void Presets__ExistForCommonRates__When__Accessed()
        {
            FrameRate.FPS_24.Fps.Should().Be(24.0);
            FrameRate.FPS_25.Fps.Should().Be(25.0);
            FrameRate.FPS_29_97.Fps.Should().Be(29.97);
            FrameRate.FPS_30.Fps.Should().Be(30.0);
            FrameRate.FPS_48.Fps.Should().Be(48.0);
            FrameRate.FPS_59_94.Fps.Should().Be(59.94);
            FrameRate.FPS_60.Fps.Should().Be(60.0);
        }
    }
}
