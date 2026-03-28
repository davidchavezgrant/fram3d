using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;
namespace Fram3d.Core.Tests.Timelines
{
    public sealed class ElementTimelineTests
    {
        private static readonly ElementId ID_A = new(Guid.NewGuid());
        private static readonly ElementId ID_B = new(Guid.NewGuid());

        // ── GetOrCreateTrack ─────────────────────────────────────────────

        [Fact]
        public void GetOrCreateTrack__CreatesTrack__When__NewElementId()
        {
            var timeline = new ElementTimeline();

            var track = timeline.GetOrCreateTrack(ID_A);

            track.Should().NotBeNull();
            track.ElementId.Should().Be(ID_A);
        }

        [Fact]
        public void GetOrCreateTrack__ReturnsSameTrack__When__CalledTwice()
        {
            var timeline = new ElementTimeline();

            var first  = timeline.GetOrCreateTrack(ID_A);
            var second = timeline.GetOrCreateTrack(ID_A);

            first.Should().BeSameAs(second);
        }

        [Fact]
        public void GetOrCreateTrack__ThrowsArgumentNullException__When__NullId() =>
            FluentActions.Invoking(() => new ElementTimeline().GetOrCreateTrack(null))
                .Should().Throw<ArgumentNullException>();

        // ── TrackCount ───────────────────────────────────────────────────

        [Fact]
        public void TrackCount__ReturnsZero__When__Empty()
        {
            var timeline = new ElementTimeline();

            timeline.TrackCount.Should().Be(0);
        }

        [Fact]
        public void TrackCount__IncrementsCorrectly__When__TracksAdded()
        {
            var timeline = new ElementTimeline();
            timeline.GetOrCreateTrack(ID_A);
            timeline.GetOrCreateTrack(ID_B);

            timeline.TrackCount.Should().Be(2);
        }

        // ── HasTrack ─────────────────────────────────────────────────────

        [Fact]
        public void HasTrack__ReturnsFalse__When__NoTrack()
        {
            var timeline = new ElementTimeline();

            timeline.HasTrack(ID_A).Should().BeFalse();
        }

        [Fact]
        public void HasTrack__ReturnsTrue__When__TrackExists()
        {
            var timeline = new ElementTimeline();
            timeline.GetOrCreateTrack(ID_A);

            timeline.HasTrack(ID_A).Should().BeTrue();
        }

        // ── GetTrack ─────────────────────────────────────────────────────

        [Fact]
        public void GetTrack__ReturnsNull__When__NoTrack()
        {
            var timeline = new ElementTimeline();

            timeline.GetTrack(ID_A).Should().BeNull();
        }

        [Fact]
        public void GetTrack__ReturnsTrack__When__Exists()
        {
            var timeline = new ElementTimeline();
            var created  = timeline.GetOrCreateTrack(ID_A);

            timeline.GetTrack(ID_A).Should().BeSameAs(created);
        }

        // ── RemoveTrack ──────────────────────────────────────────────────

        [Fact]
        public void RemoveTrack__ReturnsTrue__When__TrackExists()
        {
            var timeline = new ElementTimeline();
            timeline.GetOrCreateTrack(ID_A);

            timeline.RemoveTrack(ID_A).Should().BeTrue();
        }

        [Fact]
        public void RemoveTrack__ReturnsFalse__When__NoTrack()
        {
            var timeline = new ElementTimeline();

            timeline.RemoveTrack(ID_A).Should().BeFalse();
        }

        [Fact]
        public void RemoveTrack__DecrementsCount__When__Removed()
        {
            var timeline = new ElementTimeline();
            timeline.GetOrCreateTrack(ID_A);
            timeline.GetOrCreateTrack(ID_B);

            timeline.RemoveTrack(ID_A);

            timeline.TrackCount.Should().Be(1);
            timeline.HasTrack(ID_A).Should().BeFalse();
            timeline.HasTrack(ID_B).Should().BeTrue();
        }

        // ── Tracks collection ────────────────────────────────────────────

        [Fact]
        public void Tracks__ReturnsAllTracks__When__MultipleExist()
        {
            var timeline = new ElementTimeline();
            var trackA   = timeline.GetOrCreateTrack(ID_A);
            var trackB   = timeline.GetOrCreateTrack(ID_B);

            timeline.Tracks.Should().HaveCount(2);
            timeline.Tracks.Should().Contain(trackA);
            timeline.Tracks.Should().Contain(trackB);
        }
    }
}
