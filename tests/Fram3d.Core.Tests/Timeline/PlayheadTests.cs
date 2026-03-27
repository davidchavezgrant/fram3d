using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timeline;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timeline
{
    public sealed class PlayheadTests
    {
        private static readonly FrameRate FPS_24 = FrameRate.FPS_24;

        // ── Scrub ──────────────────────────────────────────────────────

        [Fact]
        public void Scrub__SnapsToFrameBoundary__When__TimeBetweenFrames()
        {
            var playhead = new Playhead(FPS_24);
            playhead.Scrub(1.03, 10.0);

            // 1.03 * 24 = 24.72, rounds to 25 => 25/24 = 1.04166...
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
        public void Scrub__EmitsTimeChanged__When__TimeChanges()
        {
            var playhead = new Playhead(FPS_24);
            double emitted = -1;
            playhead.TimeChanged.Subscribe(t => emitted = t);

            playhead.Scrub(2.0, 10.0);

            emitted.Should().BeApproximately(2.0, 1e-9);
        }

        // ── Playback ───────────────────────────────────────────────────

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

            var result = playhead.Advance(6.0, 5.0);

            result.Should().BeFalse();
            playhead.IsPlaying.Should().BeFalse();
            playhead.CurrentTime.Should().Be(5.0);
        }

        [Fact]
        public void Advance__DoesNothing__When__NotPlaying()
        {
            var playhead = new Playhead(FPS_24);
            playhead.Scrub(1.0, 10.0);

            playhead.Advance(0.5, 10.0);

            playhead.CurrentTime.Should().BeApproximately(1.0, 1e-9);
        }

        // ── Toggle playback ────────────────────────────────────────────

        [Fact]
        public void TogglePlayback__StartsPlaying__When__Stopped()
        {
            var playhead = new Playhead(FPS_24);

            var result = playhead.TogglePlayback(10.0);

            result.Should().BeTrue();
            playhead.IsPlaying.Should().BeTrue();
        }

        [Fact]
        public void TogglePlayback__StopsPlaying__When__Playing()
        {
            var playhead = new Playhead(FPS_24);
            playhead.TogglePlayback(10.0);

            var result = playhead.TogglePlayback(10.0);

            result.Should().BeFalse();
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

        // ── Shot resolution ────────────────────────────────────────────

        [Fact]
        public void ResolveShot__ReturnsCorrectShot__When__InMiddleOfSecondShot()
        {
            var registry = new ShotRegistry();
            registry.AddShot(Vector3.Zero, Quaternion.Identity); // Shot_01, 5s
            registry.AddShot(Vector3.Zero, Quaternion.Identity); // Shot_02, 5s

            var playhead = new Playhead(FPS_24);
            playhead.Scrub(7.0, 10.0);

            var result = playhead.ResolveShot(registry);

            result.Should().NotBeNull();
            result.Value.shot.Name.Should().Be("Shot_02");
            result.Value.localTime.Seconds.Should().BeApproximately(2.0, 1e-9);
        }

        [Fact]
        public void GoToShot__SetsTimeToShotStart__When__Called()
        {
            var registry = new ShotRegistry();
            var shot1 = registry.AddShot(Vector3.Zero, Quaternion.Identity);
            var shot2 = registry.AddShot(Vector3.Zero, Quaternion.Identity);

            var playhead = new Playhead(FPS_24);
            playhead.GoToShot(registry, shot2.Id);

            playhead.CurrentTime.Should().Be(5.0);
        }

        // ── Reset ──────────────────────────────────────────────────────

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
