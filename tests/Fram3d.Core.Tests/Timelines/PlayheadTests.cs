using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timelines
{
    public sealed class PlayheadTests
    {
        private static readonly FrameRate FPS_24 = FrameRate.FPS_24;

        [Fact]
        public void Scrub__SnapsToFrameBoundary__When__TimeBetweenFrames()
        {
            var playhead = new Playhead(FPS_24);
            playhead.Scrub(1.03, 10.0);

            playhead.CurrentTime.Should().BeApproximately(25.0 / 24.0, 1e-9);
        }

        [Fact]
        public void Scrub__ClampsToZero__When__NegativeTime()
        {
            var playhead = new Playhead(FPS_24);
            playhead.Scrub(-5.0, 10.0);

            playhead.CurrentTime.Should().Be(0);
        }

        [Fact]
        public void Scrub__ClampsToTotalDuration__When__PastEnd()
        {
            var playhead = new Playhead(FPS_24);
            playhead.Scrub(15.0, 10.0);

            playhead.CurrentTime.Should().Be(10.0);
        }

        [Fact]
        public void Advance__MovesTimeForward__When__Playing()
        {
            var playhead = new Playhead(FPS_24);
            playhead.TogglePlayback(10.0);

            playhead.Advance(0.5, 10.0);

            playhead.CurrentTime.Should().BeApproximately(0.5, 1e-9);
        }

        [Fact]
        public void Advance__StopsAtEnd__When__ReachesTotalDuration()
        {
            var playhead = new Playhead(FPS_24);
            playhead.TogglePlayback(5.0);

            playhead.Advance(6.0, 5.0).Should().BeFalse();
            playhead.IsPlaying.Should().BeFalse();
        }

        [Fact]
        public void TogglePlayback__ResetsToZero__When__AtEnd()
        {
            var playhead = new Playhead(FPS_24);
            playhead.Scrub(10.0, 10.0);

            playhead.TogglePlayback(10.0);

            playhead.CurrentTime.Should().Be(0);
            playhead.IsPlaying.Should().BeTrue();
        }

        [Fact]
        public void Reset__SetsTimeToZeroAndStops__When__Called()
        {
            var playhead = new Playhead(FPS_24);
            playhead.TogglePlayback(10.0);
            playhead.Advance(3.0, 10.0);

            playhead.Reset();

            playhead.CurrentTime.Should().Be(0);
            playhead.IsPlaying.Should().BeFalse();
        }
    }
}
