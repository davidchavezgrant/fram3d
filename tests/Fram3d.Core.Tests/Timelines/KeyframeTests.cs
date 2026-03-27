using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class KeyframeTests
    {
        [Fact]
        public void Constructor__ThrowsArgumentNull__When__IdIsNull()
        {
            Action act = () => new Keyframe<float>(null, new TimePosition(0.0), 1.0f);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor__ThrowsArgumentNull__When__TimeIsNull()
        {
            var id = new KeyframeId(Guid.NewGuid());
            Action act = () => new Keyframe<float>(id, null, 1.0f);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor__StoresProperties__When__Valid()
        {
            var id = new KeyframeId(Guid.NewGuid());
            var time = new TimePosition(2.5);
            var kf = new Keyframe<float>(id, time, 42.0f);
            kf.Id.Should().Be(id);
            kf.Time.Should().Be(time);
            kf.Value.Should().Be(42.0f);
        }

        [Fact]
        public void CompareTo__ReturnsNegative__When__EarlierInTime()
        {
            var id1 = new KeyframeId(Guid.NewGuid());
            var id2 = new KeyframeId(Guid.NewGuid());
            var a = new Keyframe<float>(id1, new TimePosition(1.0), 0f);
            var b = new Keyframe<float>(id2, new TimePosition(2.0), 0f);
            a.CompareTo(b).Should().BeNegative();
        }

        [Fact]
        public void CompareTo__ReturnsPositive__When__LaterInTime()
        {
            var id1 = new KeyframeId(Guid.NewGuid());
            var id2 = new KeyframeId(Guid.NewGuid());
            var a = new Keyframe<float>(id1, new TimePosition(5.0), 0f);
            var b = new Keyframe<float>(id2, new TimePosition(2.0), 0f);
            a.CompareTo(b).Should().BePositive();
        }

        [Fact]
        public void CompareTo__ReturnsZero__When__SameTime()
        {
            var id1 = new KeyframeId(Guid.NewGuid());
            var id2 = new KeyframeId(Guid.NewGuid());
            var a = new Keyframe<float>(id1, new TimePosition(3.0), 0f);
            var b = new Keyframe<float>(id2, new TimePosition(3.0), 0f);
            a.CompareTo(b).Should().Be(0);
        }

        [Fact]
        public void CompareTo__ReturnsPositive__When__OtherIsNull()
        {
            var kf = new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), 0f);
            kf.CompareTo(null).Should().BePositive();
        }

        [Fact]
        public void WithTime__ReturnsNewKeyframe__When__TimeChanged()
        {
            var id = new KeyframeId(Guid.NewGuid());
            var kf = new Keyframe<float>(id, new TimePosition(1.0), 5.0f);
            var moved = kf.WithTime(new TimePosition(3.0));
            moved.Id.Should().Be(id);
            moved.Time.Seconds.Should().Be(3.0);
            moved.Value.Should().Be(5.0f);
        }

        [Fact]
        public void WithValue__ReturnsNewKeyframe__When__ValueChanged()
        {
            var id = new KeyframeId(Guid.NewGuid());
            var kf = new Keyframe<float>(id, new TimePosition(1.0), 5.0f);
            var updated = kf.WithValue(10.0f);
            updated.Id.Should().Be(id);
            updated.Time.Seconds.Should().Be(1.0);
            updated.Value.Should().Be(10.0f);
        }
    }
}
