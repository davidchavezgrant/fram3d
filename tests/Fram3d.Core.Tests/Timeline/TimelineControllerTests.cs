using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timeline;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timeline
{
    public sealed class TimelineControllerTests
    {
        private static TimelineController CreateController(int shotCount = 2)
        {
            var controller = new TimelineController(FrameRate.FPS_24);

            for (var i = 0; i < shotCount; i++)
            {
                controller.Track.AddShot(Vector3.Zero, Quaternion.Identity);
            }

            controller.InitializeState(1000);
            return controller;
        }

        // ── Playback ───────────────────────────────────────────────────

        [Fact]
        public void TogglePlayback__StartsPlaying__When__Stopped()
        {
            var c = CreateController();

            c.TogglePlayback().Should().BeTrue();

            c.Playhead.IsPlaying.Should().BeTrue();
        }

        [Fact]
        public void TogglePlayback__ResetsToZero__When__AtEnd()
        {
            var c = CreateController();
            c.Playhead.Scrub(c.Track.TotalDuration, c.Track.TotalDuration);

            c.TogglePlayback();

            c.Playhead.CurrentTime.Should().Be(0);
        }

        [Fact]
        public void Advance__MovesPlayhead__When__Playing()
        {
            var c = CreateController();
            c.TogglePlayback();

            c.Advance(0.5);

            c.Playhead.CurrentTime.Should().BeApproximately(0.5, 1e-9);
        }

        [Fact]
        public void Advance__StopsAtEnd__When__ReachingTotalDuration()
        {
            var c = CreateController();
            c.TogglePlayback();

            c.Advance(999);

            c.Playhead.IsPlaying.Should().BeFalse();
            c.Playhead.CurrentTime.Should().Be(c.Track.TotalDuration);
        }

        [Fact]
        public void Advance__FiresCameraEvaluation__When__Playing()
        {
            var c = CreateController();
            Shot evaluatedShot = null;
            c.CameraEvaluationRequested += (shot, _) => evaluatedShot = shot;
            c.TogglePlayback();

            c.Advance(0.1);

            evaluatedShot.Should().NotBeNull();
        }

        // ── Scrub ──────────────────────────────────────────────────────

        [Fact]
        public void ScrubToPixel__MovesPlayhead__When__Called()
        {
            var c = CreateController();

            c.ScrubToPixel(500);

            c.Playhead.CurrentTime.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ScrubToPixel__FiresCameraEvaluation__When__Called()
        {
            var c = CreateController();
            var fired = false;
            c.CameraEvaluationRequested += (_, _) => fired = true;

            c.ScrubToPixel(500);

            fired.Should().BeTrue();
        }

        // ── Strip pointer interaction ──────────────────────────────────

        [Fact]
        public void StripPointerDown__StartsBoundaryDrag__When__NearEdge()
        {
            var c = CreateController();
            // Shot_01 ends at 5.0s. At pps=100 (10s in 1000px), 5.0s = 500px.
            var edgePx = c.State.TimeToPixel(5.0);

            var result = c.StripPointerDown(edgePx, 0);

            result.Should().Be(StripInteraction.BOUNDARY_DRAG);
            c.IsBoundaryDragging.Should().BeTrue();
        }

        [Fact]
        public void StripPointerDown__StartsPotentialClick__When__OnShot()
        {
            var c = CreateController();

            var result = c.StripPointerDown(250, 0);

            result.Should().Be(StripInteraction.POTENTIAL_CLICK);
        }

        [Fact]
        public void StripPointerUp__CompletesClick__When__NoMove()
        {
            var c = CreateController();
            c.StripPointerDown(250, 0);

            var result = c.StripPointerUp();

            result.Should().Be(StripInteraction.CLICK);
            c.Track.CurrentShot.Should().Be(c.Track.Shots[0]);
        }

        [Fact]
        public void StripPointerMove__StartsDrag__When__HeldLongEnough()
        {
            var c = CreateController();
            c.StripPointerDown(250, 0);

            var result = c.StripPointerMove(250, 300); // 300ms > HOLD_THRESHOLD_MS

            result.Should().Be(StripInteraction.DRAG_START);
            c.IsDragging.Should().BeTrue();
        }

        [Fact]
        public void StripPointerMove__ReturnsNearEdge__When__HoveringBoundary()
        {
            var c = CreateController();
            var edgePx = c.State.TimeToPixel(5.0);

            var result = c.StripPointerMove(edgePx, 0);

            result.Should().Be(StripInteraction.NEAR_EDGE);
        }

        // ── Zoom / Pan ─────────────────────────────────────────────────

        [Fact]
        public void ZoomIn__ReducesVisibleDuration__When__Called()
        {
            var c = CreateController();
            var before = c.State.VisibleDuration;

            c.ZoomIn();

            c.State.VisibleDuration.Should().BeLessThan(before);
        }

        [Fact]
        public void Pan__ShiftsView__When__Called()
        {
            var c = CreateController();
            c.ZoomIn(); // Zoom in so we can pan
            var before = c.State.ViewStart;

            c.Pan(100);

            c.State.ViewStart.Should().BeGreaterThan(before);
        }

        // ── Formatting ─────────────────────────────────────────────────

        [Fact]
        public void FormatTransportTime__ReturnsZero__When__AtStart()
        {
            var c = CreateController();

            c.FormatTransportTime().Should().Be("00;00;00;00");
        }

        [Fact]
        public void FormatTransportDuration__ReturnsShotDuration__When__ShotExists()
        {
            var c = CreateController();

            c.FormatTransportDuration().Should().Be("00;00;05;00");
        }

        // ── AddShot ────────────────────────────────────────────────────

        [Fact]
        public void AddShot__UpdatesStateAndFits__When__Called()
        {
            var c = CreateController(1);
            var before = c.Track.TotalDuration;

            c.AddShot(Vector3.Zero, Quaternion.Identity);

            c.Track.TotalDuration.Should().BeGreaterThan(before);
            c.State.ViewEnd.Should().Be(c.Track.TotalDuration);
        }
    }
}
