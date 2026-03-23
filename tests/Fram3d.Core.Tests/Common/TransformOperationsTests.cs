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

        // --- ConstructDragPlaneNormal ---

        [Fact]
        public void ConstructDragPlaneNormal__IsPerpendicularToAxis__When__Called()
        {
            var normal = TransformOperations.ConstructDragPlaneNormal(Vector3.UnitX, -Vector3.UnitZ);

            // Normal must be perpendicular to the axis (plane contains the axis)
            MathF.Abs(Vector3.Dot(normal, Vector3.UnitX)).Should().BeLessThan(0.001f);
        }

        [Fact]
        public void ConstructDragPlaneNormal__FacesCameraDirection__When__CameraLookingSideOn()
        {
            // Camera looking down -Z, axis along X → plane normal should be
            // along Z (facing the camera), perpendicular to the axis
            var normal = TransformOperations.ConstructDragPlaneNormal(Vector3.UnitX, -Vector3.UnitZ);

            MathF.Abs(normal.Z).Should().BeGreaterThan(0.9f);
        }

        [Fact]
        public void ConstructDragPlaneNormal__HandlesFallback__When__CameraParallelToAxis()
        {
            // Camera looking straight down the X axis
            var normal = TransformOperations.ConstructDragPlaneNormal(Vector3.UnitX, Vector3.UnitX);

            // Should still produce a valid, unit-length normal perpendicular to axis
            normal.Length().Should().BeApproximately(1f, 0.01f);
            MathF.Abs(Vector3.Dot(normal, Vector3.UnitX)).Should().BeLessThan(0.001f);
        }

        // --- ProjectOntoAxis (drag plane overload) ---

        [Fact]
        public void ProjectOntoAxis__FindsPoint__When__UsingDragPlane()
        {
            // Axis along X at origin. Camera looking from (0,0,-10) down +Z.
            // Ray from (3, 0, -10) pointing +Z should hit the axis at X=3.
            var result = TransformOperations.ProjectOntoAxis(
                Vector3.Zero, Vector3.UnitX,
                new Vector3(3, 0, -10), Vector3.UnitZ,
                Vector3.UnitZ);

            result.X.Should().BeApproximately(3f, 0.01f);
            result.Y.Should().BeApproximately(0f, 0.01f);
            result.Z.Should().BeApproximately(0f, 0.01f);
        }

        [Fact]
        public void ProjectOntoAxis__ReturnsOrigin__When__RayParallelToDragPlane()
        {
            // Axis along X, camera looking down -Z. Ray parallel to drag plane.
            var result = TransformOperations.ProjectOntoAxis(
                Vector3.Zero, Vector3.UnitX,
                new Vector3(0, 5, 0), Vector3.UnitX,
                -Vector3.UnitZ);

            // Ray is in the drag plane and parallel to axis → degenerate
            result.Should().Be(Vector3.Zero);
        }

        [Fact]
        public void ProjectOntoAxis__ConstrainsToAxis__When__RayHitsOffAxis()
        {
            // Axis along X. Camera above looking down.
            // Ray hits the plane at (3, 0, 2) but result should project onto axis at (3, 0, 0)
            var result = TransformOperations.ProjectOntoAxis(
                Vector3.Zero, Vector3.UnitX,
                new Vector3(3, 10, 2), -Vector3.UnitY,
                -Vector3.UnitY);

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
