using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class KeyframeSelectionTests
    {
        [Fact]
        public void HasSelection__ReturnsFalse__When__NothingSelected()
        {
            var sel = new KeyframeSelection();
            sel.HasSelection.Should().BeFalse();
        }

        [Fact]
        public void Select__SetsSelection__When__Called()
        {
            var sel = new KeyframeSelection();
            var kfId = new KeyframeId(Guid.NewGuid());
            var time = new TimePosition(1.5);
            sel.Select(TrackId.Camera, kfId, time);
            sel.HasSelection.Should().BeTrue();
            sel.TrackId.Should().Be(TrackId.Camera);
            sel.KeyframeId.Should().Be(kfId);
            sel.Time.Should().Be(time);
        }

        [Fact]
        public void Select__ReplacesExisting__When__CalledAgain()
        {
            var sel = new KeyframeSelection();
            var first = new KeyframeId(Guid.NewGuid());
            var second = new KeyframeId(Guid.NewGuid());
            sel.Select(TrackId.Camera, first, new TimePosition(1.0));
            sel.Select(TrackId.Camera, second, new TimePosition(2.0));
            sel.KeyframeId.Should().Be(second);
            sel.Time.Seconds.Should().Be(2.0);
        }

        [Fact]
        public void Clear__RemovesSelection__When__Called()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            sel.Clear();
            sel.HasSelection.Should().BeFalse();
        }

        [Fact]
        public void IsSelected__ReturnsTrue__When__MatchingKeyframe()
        {
            var sel = new KeyframeSelection();
            var kfId = new KeyframeId(Guid.NewGuid());
            sel.Select(TrackId.Camera, kfId, new TimePosition(1.0));
            sel.IsSelected(kfId).Should().BeTrue();
        }

        [Fact]
        public void IsSelected__ReturnsFalse__When__DifferentKeyframe()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            sel.IsSelected(new KeyframeId(Guid.NewGuid())).Should().BeFalse();
        }

        [Fact]
        public void IsSelected__ReturnsFalse__When__NothingSelected()
        {
            var sel = new KeyframeSelection();
            sel.IsSelected(new KeyframeId(Guid.NewGuid())).Should().BeFalse();
        }

        [Fact]
        public void Changed__Fires__When__SelectionChanges()
        {
            var sel = new KeyframeSelection();
            var fired = false;
            sel.Changed.Subscribe(_ => fired = true);
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            fired.Should().BeTrue();
        }

        [Fact]
        public void Changed__Fires__When__Cleared()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            var fired = false;
            sel.Changed.Subscribe(_ => fired = true);
            sel.Clear();
            fired.Should().BeTrue();
        }

        [Fact]
        public void TrackId__IsNull__When__NothingSelected()
        {
            var sel = new KeyframeSelection();
            sel.TrackId.Should().BeNull();
        }

        [Fact]
        public void KeyframeId__IsNull__When__NothingSelected()
        {
            var sel = new KeyframeSelection();
            sel.KeyframeId.Should().BeNull();
        }

        [Fact]
        public void Time__IsNull__When__NothingSelected()
        {
            var sel = new KeyframeSelection();
            sel.Time.Should().BeNull();
        }

        [Fact]
        public void Clear__TrackIdIsNull__When__Cleared()
        {
            var sel = new KeyframeSelection();
            sel.Select(TrackId.Camera, new KeyframeId(Guid.NewGuid()), new TimePosition(1.0));
            sel.Clear();
            sel.TrackId.Should().BeNull();
        }
    }
}
