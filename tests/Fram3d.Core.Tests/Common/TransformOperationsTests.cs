using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;
namespace Fram3d.Core.Tests.Common
{
    public sealed class TransformOperationsTests
    {
        // --- Scale ---

        [Fact]
        public void ComputeScale__ScalesUp__When__PositiveDelta()
        {
            var result = TransformOperations.ComputeScale(1f, 100f, 0.005f, 0.01f);

            result.Should().BeApproximately(1.5f, 0.001f);
        }

        [Fact]
        public void ComputeScale__ScalesDown__When__NegativeDelta()
        {
            var result = TransformOperations.ComputeScale(1f, -100f, 0.005f, 0.01f);

            result.Should().BeApproximately(0.5f, 0.001f);
        }

        [Fact]
        public void ComputeScale__ClampsToMin__When__BelowMinimum()
        {
            var result = TransformOperations.ComputeScale(0.1f, -1000f, 0.005f, 0.01f);

            result.Should().Be(0.01f);
        }

        [Fact]
        public void ComputeScale__PreservesScale__When__ZeroDelta()
        {
            var result = TransformOperations.ComputeScale(2.5f, 0f, 0.005f, 0.01f);

            result.Should().Be(2.5f);
        }

        // --- Rotation ---

        [Fact]
        public void ComputeRotation__RotatesAroundAxis__When__PositiveDelta()
        {
            var result = TransformOperations.ComputeRotation(
                Quaternion.Identity, Vector3.UnitY, 90f, 1f);

            // 90° around Y at sensitivity 1 should produce a non-identity rotation
            result.Should().NotBe(Quaternion.Identity);
        }

        [Fact]
        public void ComputeRotation__ReturnsIdentity__When__ZeroDelta()
        {
            var result = TransformOperations.ComputeRotation(
                Quaternion.Identity, Vector3.UnitY, 0f, 0.5f);

            result.W.Should().BeApproximately(1f, 0.001f);
            result.X.Should().BeApproximately(0f, 0.001f);
            result.Y.Should().BeApproximately(0f, 0.001f);
            result.Z.Should().BeApproximately(0f, 0.001f);
        }

        [Fact]
        public void ComputeRotation__ComposesWithStart__When__StartNotIdentity()
        {
            var start  = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4);
            var result = TransformOperations.ComputeRotation(start, Vector3.UnitY, 45f, 1f);

            // Result should differ from both identity and start
            result.Should().NotBe(Quaternion.Identity);
            result.Should().NotBe(start);
        }

        // --- ProjectOntoAxis ---

        [Fact]
        public void ProjectOntoAxis__ReturnsOrigin__When__RayParallelToAxis()
        {
            var result = TransformOperations.ProjectOntoAxis(
                Vector3.Zero, Vector3.UnitX,
                new Vector3(0, 0, -5), Vector3.UnitX);

            // Parallel rays → degenerate, should return origin
            result.Should().Be(Vector3.Zero);
        }

        [Fact]
        public void ProjectOntoAxis__FindsClosestPoint__When__RayPerpendicular()
        {
            // Axis along X at origin. Ray from (3, 0, -5) looking at (3, 0, 0)
            var result = TransformOperations.ProjectOntoAxis(
                Vector3.Zero, Vector3.UnitX,
                new Vector3(3, 0, -5), Vector3.UnitZ);

            result.X.Should().BeApproximately(3f, 0.01f);
            result.Y.Should().BeApproximately(0f, 0.01f);
            result.Z.Should().BeApproximately(0f, 0.01f);
        }

        // --- ComputeTranslation ---

        [Fact]
        public void ComputeTranslation__MovesAlongAxis__When__ProjectedOnAxis()
        {
            var start     = new Vector3(0, 0, 0);
            var axis      = Vector3.UnitX;
            var projected = new Vector3(5, 0, 0);
            var origin    = start;
            var offset    = Vector3.Zero;

            var result = TransformOperations.ComputeTranslation(start, axis, projected, origin, offset);

            result.X.Should().BeApproximately(5f, 0.001f);
            result.Y.Should().BeApproximately(0f, 0.001f);
            result.Z.Should().BeApproximately(0f, 0.001f);
        }

        [Fact]
        public void ComputeTranslation__SubtractsOffset__When__OffsetProvided()
        {
            var start     = new Vector3(0, 0, 0);
            var axis      = Vector3.UnitX;
            var projected = new Vector3(5, 0, 0);
            var origin    = start;
            var offset    = new Vector3(2, 0, 0);

            var result = TransformOperations.ComputeTranslation(start, axis, projected, origin, offset);

            result.X.Should().BeApproximately(3f, 0.001f);
        }
    }
}
