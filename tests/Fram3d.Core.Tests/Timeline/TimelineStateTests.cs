using FluentAssertions;
using Fram3d.Core.Timeline;
using Xunit;
namespace Fram3d.Core.Tests.Timeline
{
    public sealed class TimelineStateTests
    {
        [Fact]
        public void FitAll__SetsViewToFullDuration__When__Called()
        {
            var state = new TimelineState(10.0, 1000);

            state.ViewStart.Should().Be(0);
            state.ViewEnd.Should().Be(10.0);
        }

        [Fact]
        public void TimeToPixel__ReturnsCorrectPixel__When__TimeInRange()
        {
            var state = new TimelineState(10.0, 1000);

            state.TimeToPixel(5.0).Should().BeApproximately(500, 0.1);
        }

        [Fact]
        public void PixelToTime__ReturnsCorrectTime__When__PixelInRange()
        {
            var state = new TimelineState(10.0, 1000);

            state.PixelToTime(500).Should().BeApproximately(5.0, 0.01);
        }

        [Fact]
        public void ZoomAtPoint__ZoomsIn__When__PositiveDelta()
        {
            var state = new TimelineState(10.0, 1000);
            var originalDuration = state.VisibleDuration;

            state.ZoomAtPoint(5.0, 1f);

            state.VisibleDuration.Should().BeLessThan(originalDuration);
        }

        [Fact]
        public void ZoomAtPoint__ZoomsOut__When__NegativeDelta()
        {
            var state = new TimelineState(10.0, 1000);
            state.ZoomAtPoint(5.0, 1f); // Zoom in first
            var zoomedDuration = state.VisibleDuration;

            state.ZoomAtPoint(5.0, -1f);

            state.VisibleDuration.Should().BeGreaterThan(zoomedDuration);
        }

        [Fact]
        public void ZoomAtPoint__ClampsMinDuration__When__ZoomedInTooFar()
        {
            var state = new TimelineState(10.0, 1000);

            for (var i = 0; i < 100; i++)
            {
                state.ZoomAtPoint(5.0, 1f);
            }

            state.VisibleDuration.Should().BeGreaterOrEqualTo(0.5);
        }

        [Fact]
        public void ZoomAtPoint__ClampsMaxDuration__When__ZoomedOutTooFar()
        {
            var state = new TimelineState(10.0, 1000);

            for (var i = 0; i < 100; i++)
            {
                state.ZoomAtPoint(5.0, -1f);
            }

            state.VisibleDuration.Should().BeLessOrEqualTo(10.0);
        }

        [Fact]
        public void Pan__ShiftsViewRight__When__PositivePixels()
        {
            var state = new TimelineState(10.0, 1000);
            state.ZoomAtPoint(5.0, 1f); // Zoom in so we can pan
            var originalStart = state.ViewStart;

            state.Pan(100);

            state.ViewStart.Should().BeGreaterThan(originalStart);
        }

        [Fact]
        public void Pan__ClampsLeft__When__PanningPastZero()
        {
            var state = new TimelineState(10.0, 1000);

            state.Pan(-99999);

            state.ViewStart.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Pan__ClampsRight__When__PanningPastEnd()
        {
            var state = new TimelineState(10.0, 1000);
            state.ZoomAtPoint(2.0, 1f); // Zoom in

            state.Pan(99999);

            state.ViewEnd.Should().BeLessOrEqualTo(10.0);
        }

        [Fact]
        public void EnsureVisible__ScrollsRight__When__TimePastViewEnd()
        {
            var state = new TimelineState(10.0, 1000);
            state.ZoomAtPoint(2.0, 1f); // Zoom in
            var originalEnd = state.ViewEnd;

            state.EnsureVisible(originalEnd + 1.0);

            state.ViewEnd.Should().BeGreaterThan(originalEnd);
        }

        [Fact]
        public void EnsureVisible__ScrollsLeft__When__TimeBeforeViewStart()
        {
            var state = new TimelineState(10.0, 1000);
            state.ZoomAtPoint(5.0, 1f); // Zoom in
            state.Pan(500); // Pan right so viewStart > 0
            var originalStart = state.ViewStart;

            state.EnsureVisible(0);

            state.ViewStart.Should().BeLessThan(originalStart);
        }

        [Fact]
        public void SetTotalDuration__UpdatesClampBounds__When__Called()
        {
            var state = new TimelineState(10.0, 1000);
            state.SetTotalDuration(5.0);

            state.FitAll(5.0);

            state.ViewEnd.Should().Be(5.0);
        }

        [Fact]
        public void FitRange__SetsViewWithPadding__When__Called()
        {
            var state = new TimelineState(10.0, 1000);

            state.FitRange(2.0, 4.0);

            state.ViewStart.Should().BeLessThan(2.0);
            state.ViewEnd.Should().BeGreaterThan(4.0);
        }

        [Fact]
        public void SetViewRange__SetsExactRange__When__Called()
        {
            var state = new TimelineState(10.0, 1000);

            state.SetViewRange(3.0, 7.0);

            state.ViewStart.Should().Be(3.0);
            state.ViewEnd.Should().Be(7.0);
        }
    }
}
