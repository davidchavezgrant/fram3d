using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Scene;
using Xunit;
namespace Fram3d.Core.Tests.Scene
{
    public sealed class GizmoAxisTests
    {
        [Fact]
        public void Parse__ReturnsX__When__NameContainsX()
        {
            GizmoAxis.Parse("TranslateX").Should().BeSameAs(GizmoAxis.X);
        }

        [Fact]
        public void Parse__ReturnsY__When__NameContainsY()
        {
            GizmoAxis.Parse("RotateY").Should().BeSameAs(GizmoAxis.Y);
        }

        [Fact]
        public void Parse__ReturnsZ__When__NameContainsZ()
        {
            GizmoAxis.Parse("TranslateZ").Should().BeSameAs(GizmoAxis.Z);
        }

        [Fact]
        public void Parse__ReturnsUniform__When__NoAxisInName()
        {
            GizmoAxis.Parse("ScaleUniform").Should().BeSameAs(GizmoAxis.UNIFORM);
        }

        [Fact]
        public void Direction__IsNegativeZ__When__ZAxis()
        {
            GizmoAxis.Z.Direction.Should().Be(-Vector3.UnitZ);
        }

        [Fact]
        public void Direction__IsUnitX__When__XAxis()
        {
            GizmoAxis.X.Direction.Should().Be(Vector3.UnitX);
        }
    }
}
