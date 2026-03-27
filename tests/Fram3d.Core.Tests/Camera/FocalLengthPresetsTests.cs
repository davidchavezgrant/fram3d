using FluentAssertions;
using Fram3d.Core.Cameras;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class FocalLengthPresetsTests
	{
		[Fact]
		public void QUICK__HasNineEntries__When__Accessed()
		{
			FocalLengthPresets.QUICK.Should().HaveCount(9);
		}

		[Fact]
		public void ALL__ContainsAllSpecPresets__When__Accessed()
		{
			FocalLengthPresets.ALL.Should().ContainInOrder(
				14f, 18f, 21f, 24f, 28f, 35f, 50f, 65f, 75f, 85f, 100f, 135f, 150f, 200f, 300f, 400f);
		}

		[Fact]
		public void ALL__IsSorted__When__Accessed()
		{
			FocalLengthPresets.ALL.Should().BeInAscendingOrder();
		}
	}
}
