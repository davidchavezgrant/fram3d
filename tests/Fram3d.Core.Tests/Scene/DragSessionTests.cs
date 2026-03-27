using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Scenes;
using Xunit;
namespace Fram3d.Core.Tests.Scene
{
    public sealed class DragSessionTests
    {
        private static Element CreateElement(Vector3 position)
        {
            var element = new Element(new ElementId(Guid.NewGuid()), "Test");
            element.Position = position;
            return element;
        }

        [Fact]
        public void Constructor__CapturesStartState__When__Created()
        {
            var element = CreateElement(new Vector3(1, 2, 3));
            element.Scale    = 2.5f;
            element.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f);
            var axis    = GizmoAxis.Parse("X_Translate");

            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            session.StartPosition.Should().Be(element.Position);
            session.StartRotation.Should().Be(element.Rotation);
            session.StartScale.Should().Be(2.5f);
            session.StartMouseX.Should().Be(100f);
            session.StartMouseY.Should().Be(200f);
        }

        [Fact]
        public void UpdateRotation__UsesMouseDelta__When__MouseMoved()
        {
            var element = CreateElement(Vector3.Zero);
            var axis    = GizmoAxis.Parse("Y_Rotate");
            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            // Mouse moves from 100 to 200 → delta of 100 pixels
            session.UpdateRotation(200f);
            var rotA = element.Rotation;

            // Reset and test with doubled endpoint → delta of 200 pixels
            element.Rotation = Quaternion.Identity;
            var session2 = new DragSession(axis, element, 100f, 200f, Vector3.Zero);
            session2.UpdateRotation(300f);

            // Larger delta should produce a larger rotation angle
            MathF.Abs(element.Rotation.Y).Should().BeGreaterThan(MathF.Abs(rotA.Y));
        }

        [Fact]
        public void UpdateScale__ComputesCorrectScale__When__MouseMovedUp()
        {
            var element = CreateElement(Vector3.Zero);
            var axis    = GizmoAxis.Parse("Uniform_Scale");
            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            // delta = 300 - 200 = 100. Scale = 1 * (1 + 100 * 0.005) = 1.5
            session.UpdateScale(300f);

            element.Scale.Should().BeApproximately(1.5f, 0.01f);
        }

        [Fact]
        public void UpdateTranslation__MovesElement__When__RayProvided()
        {
            var element = CreateElement(Vector3.Zero);
            var axis    = GizmoAxis.Parse("X_Translate");
            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            // Ray from (5, 0, -10) looking +Z, camera also looking +Z
            session.UpdateTranslation(new Vector3(5, 0, -10), Vector3.UnitZ, Vector3.UnitZ);

            element.Position.X.Should().NotBe(0f);
        }

        [Fact]
        public void UpdateScale__ReducesScale__When__NegativeDelta()
        {
            var element = CreateElement(Vector3.Zero);
            var axis    = GizmoAxis.Parse("Uniform_Scale");
            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            // delta = 100 - 200 = -100. Scale = 1 * (1 + (-100) * 0.005) = 0.5
            session.UpdateScale(100f);

            element.Scale.Should().BeApproximately(0.5f, 0.01f);
        }

        [Fact]
        public void UpdateScale__ClampsToMinimum__When__LargeNegativeDelta()
        {
            var element = CreateElement(Vector3.Zero);
            var axis    = GizmoAxis.Parse("Uniform_Scale");
            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            // delta = -10000 - 200 = -10200, would produce negative scale
            session.UpdateScale(-10000f);

            element.Scale.Should().BeApproximately(0.01f, 0.001f);
        }

        [Fact]
        public void UpdateRotation__RotatesInOppositeDirection__When__NegativeDelta()
        {
            var element = CreateElement(Vector3.Zero);
            var axis    = GizmoAxis.Parse("Y_Rotate");
            var session = new DragSession(axis, element, 100f, 200f, Vector3.Zero);

            session.UpdateRotation(50f);
            var rotNeg = element.Rotation;

            element.Rotation = Quaternion.Identity;
            var session2 = new DragSession(axis, element, 100f, 200f, Vector3.Zero);
            session2.UpdateRotation(150f);
            var rotPos = element.Rotation;

            // Negative delta (50 - 100 = -50) and positive delta (150 - 100 = 50)
            // should produce Y rotations in opposite directions
            MathF.Sign(rotNeg.Y).Should().NotBe(MathF.Sign(rotPos.Y));
        }
    }
}
