using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class TrackIdTests
    {
        [Fact]
        public void Camera__IsSingleton__When__Compared()
        {
            TrackId.Camera.Should().Be(TrackId.Camera);
        }

        [Fact]
        public void ForElement__CreatesDistinct__When__DifferentElements()
        {
            var a = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            var b = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            a.Should().NotBe(b);
        }

        [Fact]
        public void ForElement__AreEqual__When__SameElement()
        {
            var id = new ElementId(Guid.NewGuid());
            TrackId.ForElement(id).Should().Be(TrackId.ForElement(id));
        }

        [Fact]
        public void Camera__IsNotEqual__When__ComparedToElement()
        {
            var element = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            TrackId.Camera.Should().NotBe(element);
        }

        [Fact]
        public void ForElement__ThrowsArgumentNull__When__NullId()
        {
            Action act = () => TrackId.ForElement(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsCamera__ReturnsTrue__When__CameraTrack()
        {
            TrackId.Camera.IsCamera.Should().BeTrue();
        }

        [Fact]
        public void IsCamera__ReturnsFalse__When__ElementTrack()
        {
            var element = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            element.IsCamera.Should().BeFalse();
        }

        [Fact]
        public void IsElement__ReturnsTrue__When__ElementTrack()
        {
            var element = TrackId.ForElement(new ElementId(Guid.NewGuid()));
            element.IsElement.Should().BeTrue();
        }

        [Fact]
        public void IsElement__ReturnsFalse__When__CameraTrack()
        {
            TrackId.Camera.IsElement.Should().BeFalse();
        }

        [Fact]
        public void ElementId__ReturnsNull__When__CameraTrack()
        {
            TrackId.Camera.ElementId.Should().BeNull();
        }

        [Fact]
        public void ElementId__ReturnsId__When__ElementTrack()
        {
            var id = new ElementId(Guid.NewGuid());
            TrackId.ForElement(id).ElementId.Should().Be(id);
        }
    }
}
