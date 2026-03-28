using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class TrackExpansionTests
    {
        [Fact]
        public void IsExpanded__ReturnsFalse__When__Default()
        {
            var exp = new TrackExpansion();
            exp.IsExpanded(TrackId.Camera).Should().BeFalse();
        }

        [Fact]
        public void Toggle__Expands__When__Collapsed()
        {
            var exp = new TrackExpansion();
            exp.Toggle(TrackId.Camera);
            exp.IsExpanded(TrackId.Camera).Should().BeTrue();
        }

        [Fact]
        public void Toggle__Collapses__When__Expanded()
        {
            var exp = new TrackExpansion();
            exp.Toggle(TrackId.Camera);
            exp.Toggle(TrackId.Camera);
            exp.IsExpanded(TrackId.Camera).Should().BeFalse();
        }

        [Fact]
        public void Toggle__IsPerTrack__When__DifferentTracks()
        {
            var exp = new TrackExpansion();
            var elemId = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            exp.Toggle(TrackId.Camera);
            exp.IsExpanded(TrackId.Camera).Should().BeTrue();
            exp.IsExpanded(elemId).Should().BeFalse();
        }

        [Fact]
        public void IsExpanded__ReturnsFalse__When__ElementTrackNotToggled()
        {
            var exp = new TrackExpansion();
            var elemId = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            exp.IsExpanded(elemId).Should().BeFalse();
        }

        [Fact]
        public void Toggle__WorksForElementTracks__When__Toggled()
        {
            var exp = new TrackExpansion();
            var elemId = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            exp.Toggle(elemId);
            exp.IsExpanded(elemId).Should().BeTrue();
        }
    }
}
