using System;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
	public class ElementIdTests
	{
		[Fact]
		public void Constructor__ThrowsArgumentException__When__GuidIsEmpty()
		{
			Action act = () => new ElementId(Guid.Empty);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor__Succeeds__When__GuidIsValid()
		{
			var guid = Guid.NewGuid();
			var id = new ElementId(guid);
			id.Value.Should().Be(guid);
		}

		[Fact]
		public void Equals__ReturnsTrue__When__SameGuid()
		{
			var guid = Guid.NewGuid();
			var a = new ElementId(guid);
			var b = new ElementId(guid);
			a.Should().Be(b);
			(a == b).Should().BeTrue();
		}

		[Fact]
		public void Equals__ReturnsFalse__When__DifferentGuid()
		{
			var a = ElementId.New();
			var b = ElementId.New();
			a.Should().NotBe(b);
			(a != b).Should().BeTrue();
		}

		[Fact]
		public void New__ReturnsUniqueId__When__CalledMultipleTimes()
		{
			var a = ElementId.New();
			var b = ElementId.New();
			a.Value.Should().NotBe(b.Value);
		}

		[Fact]
		public void GetHashCode__IsSame__When__SameGuid()
		{
			var guid = Guid.NewGuid();
			var a = new ElementId(guid);
			var b = new ElementId(guid);
			a.GetHashCode().Should().Be(b.GetHashCode());
		}
	}
}
