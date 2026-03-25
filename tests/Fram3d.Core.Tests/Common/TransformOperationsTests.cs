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
        public void ComputeRotation__ProducesCorrectAngle__When__180DegreesAroundY()
        {
            // 180 pixels * 1.0 sensitivity * π/180 = π radians around Y
            var result = TransformOperations.ComputeRotation(
                Quaternion.Identity, Vector3.UnitY, 180f, 1f);

            // After 180° around Y, forward (-Z) should flip to +Z
            var forward = Vector3.Transform(-Vector3.UnitZ, result);
            forward.Z.Should().BeApproximately(1f, 0.05f);
        }

        [Fact]
        public void ComputeRotation__ScalesBySensitivity__When__HalfSensitivity()
        {
            var full = TransformOperations.ComputeRotation(
                Quaternion.Identity, Vector3.UnitY, 90f, 1f);
            var half = TransformOperations.ComputeRotation(
                Quaternion.Identity, Vector3.UnitY, 90f, 0.5f);

            // Half sensitivity should produce half the rotation angle.
            // For axis-angle, the Y component of the quaternion is sin(angle/2).
            MathF.Abs(half.Y).Should().BeLessThan(MathF.Abs(full.Y));
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

        [Fact]
        public void ProjectOntoAxis__HandlesOffsetOrigin__When__AxisNotAtWorldOrigin()
        {
            // Axis along X at (0,5,0). Ray from (3,5,-5) looking +Z
            var result = TransformOperations.ProjectOntoAxis(
                new Vector3(0, 5, 0), Vector3.UnitX,
                new Vector3(3, 5, -5), Vector3.UnitZ);

            result.X.Should().BeApproximately(3f, 0.01f);
            result.Y.Should().BeApproximately(5f, 0.01f);
            result.Z.Should().BeApproximately(0f, 0.01f);
        }

        [Fact]
        public void ProjectOntoAxis__WorksOnYAxis__When__RayFromSide()
        {
            // Axis along Y at origin. Ray from (5,3,0) looking -X → closest to axis at (0,3,0)
            var result = TransformOperations.ProjectOntoAxis(
                Vector3.Zero, Vector3.UnitY,
                new Vector3(5, 3, 0), -Vector3.UnitX);

            result.X.Should().BeApproximately(0f, 0.01f);
            result.Y.Should().BeApproximately(3f, 0.01f);
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

        [Fact]
        public void ConstructDragPlaneNormal__UsesSecondaryFallback__When__AxisIsUnitY()
        {
            // Camera looking straight down Y, axis is Y.
            // First fallback Cross(Y, UnitY) is zero → falls through to Cross(Y, UnitX)
            var normal = TransformOperations.ConstructDragPlaneNormal(Vector3.UnitY, Vector3.UnitY);

            normal.Length().Should().BeApproximately(1f, 0.01f);
            MathF.Abs(Vector3.Dot(normal, Vector3.UnitY)).Should().BeLessThan(0.001f);
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

        [Fact]
        public void ProjectOntoAxis__HandlesOffsetOrigin__When__DragPlaneWithOffset()
        {
            // Axis along X at (0,3,0). Camera looking down -Z.
            // Ray from (4,3,-10) toward +Z should hit axis at (4,3,0).
            var result = TransformOperations.ProjectOntoAxis(
                new Vector3(0, 3, 0), Vector3.UnitX,
                new Vector3(4, 3, -10), Vector3.UnitZ,
                Vector3.UnitZ);

            result.X.Should().BeApproximately(4f, 0.01f);
            result.Y.Should().BeApproximately(3f, 0.01f);
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

        [Fact]
        public void ComputeTranslation__UsesOriginAsDeltaBase__When__OriginDiffersFromStart()
        {
            // start=(1,0,0), projected=(7,0,0), origin=(2,0,0)
            // delta = projected - origin = (5,0,0)
            // axisDelta = dot((5,0,0), UnitX) * UnitX = 5 * UnitX = (5,0,0)
            // result = start + axisDelta - offset = (1,0,0) + (5,0,0) - 0 = (6,0,0)
            var result = TransformOperations.ComputeTranslation(
                new Vector3(1, 0, 0), Vector3.UnitX,
                new Vector3(7, 0, 0), new Vector3(2, 0, 0), Vector3.Zero);

            result.X.Should().BeApproximately(6f, 0.001f);
        }
    }
}
