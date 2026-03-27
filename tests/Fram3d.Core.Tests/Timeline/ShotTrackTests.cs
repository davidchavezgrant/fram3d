using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timeline;
using Fram3d.Core.Shots;
using Fram3d.Core.Timeline;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timeline
{
    public sealed class ShotTrackTests
    {
        private static ShotTrack CreateTrack(out ShotRegistry registry, out TimelineState state, int shotCount = 3)
        {
            registry = new ShotRegistry();

            for (var i = 0; i < shotCount; i++)
            {
                registry.AddShot(Vector3.Zero, Quaternion.Identity);
            }

            state = new TimelineState(registry.TotalDuration, 1000);
            return new ShotTrack(registry, state, FrameRate.FPS_24);
        }

        // ── Edge detection ─────────────────────────────────────────────

        [Fact]
        public void FindEdgeAtTime__ReturnsIndex__When__NearShotEdge()
        {
            var track = CreateTrack(out var registry, out _);

            // Shot_01 ends at 5.0s
            var index = track.FindEdgeAtTime(5.0, 0.1);

            index.Should().Be(0);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsLastIndex__When__NearLastShotEdge()
        {
            var track = CreateTrack(out var registry, out _);

            // Shot_03 ends at 15.0s
            var index = track.FindEdgeAtTime(15.0, 0.1);

            index.Should().Be(2);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsNegativeOne__When__NotNearEdge()
        {
            var track = CreateTrack(out _, out _);

            var index = track.FindEdgeAtTime(7.5, 0.1);

            index.Should().Be(-1);
        }

        // ── Insertion index ────────────────────────────────────────────

        [Fact]
        public void FindInsertionIndex__ReturnsZero__When__BeforeFirstShot()
        {
            var track = CreateTrack(out _, out _);

            track.FindInsertionIndex(1.0).Should().Be(0);
        }

        [Fact]
        public void FindInsertionIndex__ReturnsMidIndex__When__BetweenShots()
        {
            var track = CreateTrack(out _, out _);

            // Between Shot_01 (0-5) and Shot_02 (5-10), midpoint of Shot_02 = 7.5
            track.FindInsertionIndex(6.0).Should().Be(1);
        }

        [Fact]
        public void FindInsertionIndex__ReturnsCount__When__AfterLastShot()
        {
            var track = CreateTrack(out var registry, out _);

            track.FindInsertionIndex(20.0).Should().Be(registry.Count);
        }

        // ── Resize ─────────────────────────────────────────────────────

        [Fact]
        public void ResizeShotAtEdge__SnapsToFrame__When__Called()
        {
            var track = CreateTrack(out var registry, out _);

            var newDuration = track.ResizeShotAtEdge(0, 3.03);

            // 3.03s from start 0 → duration 3.03 → snaps to nearest frame
            var expectedFrame = System.Math.Round(3.03 * 24) / 24;
            newDuration.Should().BeApproximately(expectedFrame, 1e-9);
            registry.Shots[0].Duration.Should().BeApproximately(expectedFrame, 1e-9);
        }

        [Fact]
        public void ResizeShotAtEdge__ClampsToMinDuration__When__TooSmall()
        {
            var track = CreateTrack(out var registry, out _);

            track.ResizeShotAtEdge(0, 0.01);

            registry.Shots[0].Duration.Should().BeGreaterOrEqualTo(0.1);
        }

        // ── Fit to shot ────────────────────────────────────────────────

        [Fact]
        public void FitToShot__FitsAll__When__SingleShot()
        {
            var track = CreateTrack(out var registry, out var state, 1);

            track.FitToShot(registry.Shots[0].Id);

            state.ViewStart.Should().Be(0);
            state.ViewEnd.Should().Be(registry.TotalDuration);
        }

        [Fact]
        public void FitToShot__NoLeftPadding__When__FirstShot()
        {
            var track = CreateTrack(out var registry, out var state);

            track.FitToShot(registry.Shots[0].Id);

            state.ViewStart.Should().Be(0);
            state.ViewEnd.Should().BeGreaterThan(5.0);
        }

        [Fact]
        public void FitToShot__NoRightPadding__When__LastShot()
        {
            var track = CreateTrack(out var registry, out var state);

            track.FitToShot(registry.Shots[2].Id);

            state.ViewEnd.Should().Be(registry.TotalDuration);
            state.ViewStart.Should().BeLessThan(10.0);
        }

        [Fact]
        public void FitToShot__PaddsBothSides__When__MiddleShot()
        {
            var track = CreateTrack(out var registry, out var state);

            track.FitToShot(registry.Shots[1].Id);

            state.ViewStart.Should().BeLessThan(5.0);
            state.ViewEnd.Should().BeGreaterThan(10.0);
        }

        // ── Tooltip formatting ─────────────────────────────────────────

        [Fact]
        public void FormatShotTooltip__ContainsNameAndDuration__When__Called()
        {
            var track = CreateTrack(out var registry, out _);

            var text = track.FormatShotTooltip(registry.Shots[0]);

            text.Should().Contain("Shot_01");
            text.Should().Contain("5.0s");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsRipple__When__ShiftNotHeld()
        {
            var track = CreateTrack(out _, out _);

            var text = track.FormatResizeTooltip(0, false);

            text.Should().Contain("[ripple]");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsShotsOnly__When__ShiftHeld()
        {
            var track = CreateTrack(out _, out _);

            var text = track.FormatResizeTooltip(0, true);

            text.Should().Contain("[shots only]");
        }

        // ── Pixel tolerance ────────────────────────────────────────────

        [Fact]
        public void PixelToleranceToTime__ConvertsCorrectly__When__ZoomedIn()
        {
            var track = CreateTrack(out _, out var state);

            // state has 15s in 1000px → ~66.67 px/s
            var tolerance = track.PixelToleranceToTime(6.0);

            tolerance.Should().BeApproximately(6.0 / state.PixelsPerSecond, 0.001);
        }
    }
}
