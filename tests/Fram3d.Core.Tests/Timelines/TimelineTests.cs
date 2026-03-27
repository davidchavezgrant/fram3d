using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timelines
{
    public sealed class TimelineTests
    {
        private static Timeline Create(int shotCount = 2)
        {
            var timeline = new Timeline(FrameRate.FPS_24);

            for (var i = 0; i < shotCount; i++)
            {
                timeline.AddShot(Vector3.Zero, Quaternion.Identity);
            }

            timeline.InitializeViewRange(1000);
            return timeline;
        }

        // ── Shot lifecycle ─────────────────────────────────────────────

        [Fact]
        public void AddShot__IncrementsCount__When__Called() =>
            Create(3).Count.Should().Be(3);

        [Fact]
        public void AddShot__SetsAsCurrent__When__Called()
        {
            var t = Create(2);

            t.CurrentShot.Name.Should().Be("Shot_02");
        }

        [Fact]
        public void RemoveShot__SelectsNext__When__CurrentRemoved()
        {
            var t = Create(3);
            t.SetCurrentShot(t.Shots[1].Id);

            t.RemoveShot(t.Shots[1].Id);

            t.CurrentShot.Should().Be(t.Shots[1]); // was index 2, now index 1
        }

        [Fact]
        public void Reorder__MovesShot__When__Called()
        {
            var t     = Create(3);
            var shot3 = t.Shots[2];

            t.Reorder(shot3.Id, 0);

            t.Shots[0].Should().Be(shot3);
        }

        // ── View range ─────────────────────────────────────────────────

        [Fact]
        public void FitAll__SetsViewToFullDuration__When__Called()
        {
            var t = Create();

            t.ViewStart.Should().Be(0);
            t.ViewEnd.Should().Be(t.TotalDuration);
        }

        [Fact]
        public void TimeToPixel__ReturnsCorrectPixel__When__Called()
        {
            var t = Create();

            t.TimeToPixel(5.0).Should().BeApproximately(500, 0.1);
        }

        [Fact]
        public void PixelToTime__ReturnsCorrectTime__When__Called()
        {
            var t = Create();

            t.PixelToTime(500).Should().BeApproximately(5.0, 0.01);
        }

        [Fact]
        public void ZoomAtPoint__ZoomsIn__When__PositiveDelta()
        {
            var t      = Create();
            var before = t.VisibleDuration;

            t.ZoomAtPoint(5.0, 1f);

            t.VisibleDuration.Should().BeLessThan(before);
        }

        [Fact]
        public void Pan__ClampsLeft__When__PanningPastZero()
        {
            var t = Create();

            t.Pan(-99999);

            t.ViewStart.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Pan__ClampsRight__When__PanningPastEnd()
        {
            var t = Create();
            t.ZoomAtPoint(2.0, 1f);

            t.Pan(99999);

            t.ViewEnd.Should().BeLessOrEqualTo(t.TotalDuration);
        }

        // ── Edge detection ─────────────────────────────────────────────

        [Fact]
        public void FindEdgeAtTime__ReturnsIndex__When__NearShotEdge()
        {
            var t = Create(3);

            t.FindEdgeAtTime(5.0, 0.1).Should().Be(0);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsNegativeOne__When__NotNearEdge()
        {
            var t = Create(3);

            t.FindEdgeAtTime(7.5, 0.1).Should().Be(-1);
        }

        // ── Resize ─────────────────────────────────────────────────────

        [Fact]
        public void ResizeShotAtEdge__SnapsToFrame__When__Called()
        {
            var t = Create(3);

            var newDuration = t.ResizeShotAtEdge(0, 3.03);

            var expected = System.Math.Round(3.03 * 24) / 24;
            newDuration.Should().BeApproximately(expected, 1e-9);
        }

        // ── Fit to shot ────────────────────────────────────────────────

        [Fact]
        public void FitToShot__FitsAll__When__SingleShot()
        {
            var t = Create(1);

            t.FitToShot(t.Shots[0].Id);

            t.ViewStart.Should().Be(0);
            t.ViewEnd.Should().Be(t.TotalDuration);
        }

        [Fact]
        public void FitToShot__NoLeftPadding__When__FirstShot()
        {
            var t = Create(3);

            t.FitToShot(t.Shots[0].Id);

            t.ViewStart.Should().Be(0);
        }

        [Fact]
        public void FitToShot__NoRightPadding__When__LastShot()
        {
            var t = Create(3);

            t.FitToShot(t.Shots[2].Id);

            t.ViewEnd.Should().Be(t.TotalDuration);
        }

        // ── Playback ───────────────────────────────────────────────────

        [Fact]
        public void TogglePlayback__StartsPlaying__When__Stopped()
        {
            var t = Create();

            t.TogglePlayback().Should().BeTrue();
            t.Playhead.IsPlaying.Should().BeTrue();
        }

        [Fact]
        public void TogglePlayback__ResetsToZero__When__AtEnd()
        {
            var t = Create();
            t.Playhead.Scrub(t.TotalDuration, t.TotalDuration);

            t.TogglePlayback();

            t.Playhead.CurrentTime.Should().Be(0);
        }

        [Fact]
        public void Advance__StopsAtEnd__When__ReachingTotalDuration()
        {
            var t = Create();
            t.TogglePlayback();

            t.Advance(999);

            t.Playhead.IsPlaying.Should().BeFalse();
        }

        [Fact]
        public void Advance__FiresCameraEvaluation__When__Playing()
        {
            var t     = Create();
            Shot shot = null;
            t.CameraEvaluationRequested.Subscribe(eval => shot = eval.Shot);
            t.TogglePlayback();

            t.Advance(0.1);

            shot.Should().NotBeNull();
        }

        // ── Scrub ──────────────────────────────────────────────────────

        [Fact]
        public void ScrubToPixel__MovesPlayhead__When__Called()
        {
            var t = Create();

            t.ScrubToPixel(500);

            t.Playhead.CurrentTime.Should().BeGreaterThan(0);
        }

        // ── Strip interaction ──────────────────────────────────────────

        [Fact]
        public void ShotTrackPointerDown__StartsBoundaryDrag__When__NearEdge()
        {
            var t      = Create();
            var edgePx = t.TimeToPixel(5.0);

            t.ShotTrackPointerDown(edgePx, 0).Should().Be(ShotTrackAction.BOUNDARY_DRAG);
        }

        [Fact]
        public void ShotTrackPointerDown__StartsPotentialClick__When__OnShot()
        {
            var t = Create();

            t.ShotTrackPointerDown(250, 0).Should().Be(ShotTrackAction.POTENTIAL_CLICK);
        }

        [Fact]
        public void ShotTrackPointerUp__CompletesClick__When__NoMove()
        {
            var t = Create();
            t.ShotTrackPointerDown(250, 0);

            t.ShotTrackPointerUp().Should().Be(ShotTrackAction.CLICK);
        }

        [Fact]
        public void ShotTrackPointerMove__StartsDrag__When__HeldLongEnough()
        {
            var t = Create();
            t.ShotTrackPointerDown(250, 0);

            t.ShotTrackPointerMove(250, 300).Should().Be(ShotTrackAction.DRAG_START);
        }

        // ── Formatting ─────────────────────────────────────────────────

        [Fact]
        public void FormatShotTooltip__ContainsName__When__Called()
        {
            var t = Create();

            t.FormatShotTooltip(t.Shots[0]).Should().Contain("Shot_01");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsRipple__When__ShiftNotHeld()
        {
            var t = Create();

            t.FormatResizeTooltip(0, false).Should().Contain("[ripple]");
        }
    }
}
