using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timeline;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timeline
{
    public sealed class ShotTrackTests
    {
        private static ShotTrack CreateTrack(out TimelineState state, int shotCount = 3)
        {
            var track = new ShotTrack(FrameRate.FPS_24);

            for (var i = 0; i < shotCount; i++)
            {
                track.AddShot(Vector3.Zero, Quaternion.Identity);
            }

            state = new TimelineState(track.TotalDuration, 1000);
            return track;
        }

        // ── Edge detection ─────────────────────────────────────────────

        [Fact]
        public void FindEdgeAtTime__ReturnsIndex__When__NearShotEdge()
        {
            var track = CreateTrack(out _);

            var index = track.FindEdgeAtTime(5.0, 0.1);

            index.Should().Be(0);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsLastIndex__When__NearLastShotEdge()
        {
            var track = CreateTrack(out _);

            var index = track.FindEdgeAtTime(15.0, 0.1);

            index.Should().Be(2);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsNegativeOne__When__NotNearEdge()
        {
            var track = CreateTrack(out _);

            var index = track.FindEdgeAtTime(7.5, 0.1);

            index.Should().Be(-1);
        }

        // ── Insertion index ────────────────────────────────────────────

        [Fact]
        public void FindInsertionIndex__ReturnsZero__When__BeforeFirstShot()
        {
            var track = CreateTrack(out _);

            track.FindInsertionIndex(1.0).Should().Be(0);
        }

        [Fact]
        public void FindInsertionIndex__ReturnsMidIndex__When__BetweenShots()
        {
            var track = CreateTrack(out _);

            track.FindInsertionIndex(6.0).Should().Be(1);
        }

        [Fact]
        public void FindInsertionIndex__ReturnsCount__When__AfterLastShot()
        {
            var track = CreateTrack(out _);

            track.FindInsertionIndex(20.0).Should().Be(track.Count);
        }

        // ── Resize ─────────────────────────────────────────────────────

        [Fact]
        public void ResizeShotAtEdge__SnapsToFrame__When__Called()
        {
            var track = CreateTrack(out _);

            var newDuration = track.ResizeShotAtEdge(0, 3.03);

            var expectedFrame = System.Math.Round(3.03 * 24) / 24;
            newDuration.Should().BeApproximately(expectedFrame, 1e-9);
            track.Shots[0].Duration.Should().BeApproximately(expectedFrame, 1e-9);
        }

        [Fact]
        public void ResizeShotAtEdge__ClampsToMinDuration__When__TooSmall()
        {
            var track = CreateTrack(out _);

            track.ResizeShotAtEdge(0, 0.01);

            track.Shots[0].Duration.Should().BeGreaterOrEqualTo(0.1);
        }

        // ── Fit to shot ────────────────────────────────────────────────

        [Fact]
        public void FitToShot__FitsAll__When__SingleShot()
        {
            var track = CreateTrack(out var state, 1);

            track.FitToShot(track.Shots[0].Id, state);

            state.ViewStart.Should().Be(0);
            state.ViewEnd.Should().Be(track.TotalDuration);
        }

        [Fact]
        public void FitToShot__NoLeftPadding__When__FirstShot()
        {
            var track = CreateTrack(out var state);

            track.FitToShot(track.Shots[0].Id, state);

            state.ViewStart.Should().Be(0);
            state.ViewEnd.Should().BeGreaterThan(5.0);
        }

        [Fact]
        public void FitToShot__NoRightPadding__When__LastShot()
        {
            var track = CreateTrack(out var state);

            track.FitToShot(track.Shots[2].Id, state);

            state.ViewEnd.Should().Be(track.TotalDuration);
            state.ViewStart.Should().BeLessThan(10.0);
        }

        [Fact]
        public void FitToShot__PaddsBothSides__When__MiddleShot()
        {
            var track = CreateTrack(out var state);

            track.FitToShot(track.Shots[1].Id, state);

            state.ViewStart.Should().BeLessThan(5.0);
            state.ViewEnd.Should().BeGreaterThan(10.0);
        }

        // ── Tooltip formatting ─────────────────────────────────────────

        [Fact]
        public void FormatShotTooltip__ContainsNameAndDuration__When__Called()
        {
            var track = CreateTrack(out _);

            var text = track.FormatShotTooltip(track.Shots[0]);

            text.Should().Contain("Shot_01");
            text.Should().Contain("5.0s");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsRipple__When__ShiftNotHeld()
        {
            var track = CreateTrack(out _);

            var text = track.FormatResizeTooltip(0, false);

            text.Should().Contain("[ripple]");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsShotsOnly__When__ShiftHeld()
        {
            var track = CreateTrack(out _);

            var text = track.FormatResizeTooltip(0, true);

            text.Should().Contain("[shots only]");
        }
    }
}
