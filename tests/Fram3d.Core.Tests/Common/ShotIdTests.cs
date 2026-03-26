using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
    public class ShotIdTests
    {
        [Fact]
        public void Constructor__ThrowsArgumentException__When__GuidIsEmpty()
        {
            Action act = () => new ShotId(Guid.Empty);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor__Succeeds__When__GuidIsValid()
        {
            var guid = Guid.NewGuid();
            var id = new ShotId(guid);
            id.Value.Should().Be(guid);
        }

        [Fact]
        public void Equals__ReturnsTrue__When__SameGuid()
        {
            var guid = Guid.NewGuid();
            var a = new ShotId(guid);
            var b = new ShotId(guid);
            a.Should().Be(b);
            (a == b).Should().BeTrue();
        }

        [Fact]
        public void Equals__ReturnsFalse__When__DifferentGuid()
        {
            var a = new ShotId(Guid.NewGuid());
            var b = new ShotId(Guid.NewGuid());
            a.Should().NotBe(b);
            (a != b).Should().BeTrue();
        }

        [Fact]
        public void Equals__ReturnsFalse__When__ComparedToNull()
        {
            var a = new ShotId(Guid.NewGuid());
            a.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode__IsSame__When__SameGuid()
        {
            var guid = Guid.NewGuid();
            var a = new ShotId(guid);
            var b = new ShotId(guid);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void ToString__ReturnsGuidString__When__Called()
        {
            var guid = Guid.NewGuid();
            var id = new ShotId(guid);
            id.ToString().Should().Be(guid.ToString());
        }
    }
}
