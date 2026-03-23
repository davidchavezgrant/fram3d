using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
	public class ElementTests
	{
		private static Element CreateElement() =>
			new Element(new ElementId(Guid.NewGuid()), "Test");

		[Fact]
		public void Position__ClampsYToZero__When__SetToNegativeY()
		{
			var element = CreateElement();
			element.Position = new Vector3(5f, -3f, 10f);
			element.Position.Should().Be(new Vector3(5f, 0f, 10f));
		}

		[Fact]
		public void Position__PreservesY__When__SetToPositiveY()
		{
			var element = CreateElement();
			element.Position = new Vector3(1f, 7f, 2f);
			element.Position.Should().Be(new Vector3(1f, 7f, 2f));
		}

		[Fact]
		public void Position__AllowsZeroY__When__SetToZeroY()
		{
			var element = CreateElement();
			element.Position = new Vector3(3f, 0f, 4f);
			element.Position.Should().Be(new Vector3(3f, 0f, 4f));
		}

		[Fact]
		public void Position__ClampsToGroundOffset__When__GroundOffsetSet()
		{
			var element = CreateElement();
			element.GroundOffset = 0.5f;
			element.Position = new Vector3(1f, 0.2f, 1f);
			element.Position.Should().Be(new Vector3(1f, 0.5f, 1f));
		}

		[Fact]
		public void Position__AllowsAboveGroundOffset__When__GroundOffsetSet()
		{
			var element = CreateElement();
			element.GroundOffset = 0.5f;
			element.Position = new Vector3(1f, 3f, 1f);
			element.Position.Should().Be(new Vector3(1f, 3f, 1f));
		}
	}
}
