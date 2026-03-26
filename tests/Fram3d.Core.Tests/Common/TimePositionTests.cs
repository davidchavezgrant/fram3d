using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
    public class TimePositionTests
    {
        [Fact]
        public void Constructor__ThrowsArgumentOutOfRange__When__SecondsIsNegative()
        {
            Action act = () => new TimePosition(-1.0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Constructor__Succeeds__When__SecondsIsZero()
        {
            var tp = new TimePosition(0.0);
            tp.Seconds.Should().Be(0.0);
        }

        [Fact]
        public void Constructor__Succeeds__When__SecondsIsPositive()
        {
            var tp = new TimePosition(5.5);
            tp.Seconds.Should().Be(5.5);
        }

        [Fact]
        public void Zero__HasZeroSeconds__When__Accessed()
        {
            TimePosition.ZERO.Seconds.Should().Be(0.0);
        }

        [Fact]
        public void Add__ReturnsNewTimePosition__When__AddingPositive()
        {
            var tp = new TimePosition(3.0);
            var result = tp.Add(2.0);
            result.Seconds.Should().Be(5.0);
        }

        [Fact]
        public void Add__ClampsToZero__When__ResultWouldBeNegative()
        {
            var tp = new TimePosition(1.0);
            var result = tp.Add(-5.0);
            result.Seconds.Should().Be(0.0);
        }

        [Fact]
        public void Subtract__ReturnsNewTimePosition__When__SubtractingLessThanCurrent()
        {
            var tp = new TimePosition(5.0);
            var result = tp.Subtract(2.0);
            result.Seconds.Should().Be(3.0);
        }

        [Fact]
        public void Subtract__ClampsToZero__When__SubtractingMoreThanCurrent()
        {
            var tp = new TimePosition(2.0);
            var result = tp.Subtract(5.0);
            result.Seconds.Should().Be(0.0);
        }

        [Fact]
        public void ToFrame__ReturnsCorrectFrame__When__AtExactBoundary()
        {
            var tp = new TimePosition(1.0);
            tp.ToFrame(FrameRate.FPS_24).Should().Be(24);
        }

        [Fact]
        public void ToFrame__FloorsToFrame__When__BetweenBoundaries()
        {
            var tp = new TimePosition(0.5);
            tp.ToFrame(FrameRate.FPS_24).Should().Be(12);
        }

        [Fact]
        public void ToFrame__ReturnsZero__When__AtTimeZero()
        {
            TimePosition.ZERO.ToFrame(FrameRate.FPS_24).Should().Be(0);
        }

        [Fact]
        public void Equals__ReturnsTrue__When__SameSeconds()
        {
            var a = new TimePosition(3.5);
            var b = new TimePosition(3.5);
            a.Should().Be(b);
            (a == b).Should().BeTrue();
        }

        [Fact]
        public void Equals__ReturnsFalse__When__DifferentSeconds()
        {
            var a = new TimePosition(3.5);
            var b = new TimePosition(4.0);
            a.Should().NotBe(b);
            (a != b).Should().BeTrue();
        }

        [Fact]
        public void Equals__ReturnsFalse__When__ComparedToNull()
        {
            var a = new TimePosition(1.0);
            a.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void CompareTo__ReturnsNegative__When__LessThan()
        {
            var a = new TimePosition(1.0);
            var b = new TimePosition(2.0);
            a.CompareTo(b).Should().BeNegative();
        }

        [Fact]
        public void CompareTo__ReturnsPositive__When__GreaterThan()
        {
            var a = new TimePosition(5.0);
            var b = new TimePosition(2.0);
            a.CompareTo(b).Should().BePositive();
        }

        [Fact]
        public void CompareTo__ReturnsZero__When__Equal()
        {
            var a = new TimePosition(3.0);
            var b = new TimePosition(3.0);
            a.CompareTo(b).Should().Be(0);
        }

        [Fact]
        public void CompareTo__ReturnsPositive__When__OtherIsNull()
        {
            var a = new TimePosition(1.0);
            a.CompareTo(null).Should().BePositive();
        }

        [Fact]
        public void LessThan__ReturnsTrue__When__LeftIsSmaller()
        {
            var a = new TimePosition(1.0);
            var b = new TimePosition(2.0);
            (a < b).Should().BeTrue();
        }

        [Fact]
        public void GreaterThan__ReturnsTrue__When__LeftIsLarger()
        {
            var a = new TimePosition(5.0);
            var b = new TimePosition(2.0);
            (a > b).Should().BeTrue();
        }

        [Fact]
        public void LessThanOrEqual__ReturnsTrue__When__Equal()
        {
            var a = new TimePosition(3.0);
            var b = new TimePosition(3.0);
            (a <= b).Should().BeTrue();
        }

        [Fact]
        public void GreaterThanOrEqual__ReturnsTrue__When__Equal()
        {
            var a = new TimePosition(3.0);
            var b = new TimePosition(3.0);
            (a >= b).Should().BeTrue();
        }

        [Fact]
        public void GetHashCode__IsSame__When__SameSeconds()
        {
            var a = new TimePosition(2.5);
            var b = new TimePosition(2.5);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void ToString__ShowsSeconds__When__Called()
        {
            var tp = new TimePosition(3.5);
            tp.ToString().Should().Be("3.500s");
        }
    }
}
