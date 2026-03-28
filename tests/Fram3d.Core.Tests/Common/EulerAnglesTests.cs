using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Xunit;

namespace Fram3d.Core.Tests.Common
{
    public class EulerAnglesTests
    {
        private const float TOLERANCE = 0.01f;

        [Fact]
        public void FromQuaternion__ReturnsZeros__When__Identity()
        {
            var euler = EulerAngles.FromQuaternion(Quaternion.Identity);
            euler.Pan.Should().BeApproximately(0f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(0f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsPan__When__PureYawApplied()
        {
            // Pan 45 degrees right = rotation around world Y by -45 degrees
            // (CameraElement.Pan negates the amount before CreateFromAxisAngle)
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(45f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(0f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsNegativePan__When__PanLeft()
        {
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 30f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(-30f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsTilt__When__PurePitchApplied()
        {
            // Tilt 20 degrees up = rotation around local right (X when identity)
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 20f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(0f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(20f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__ExtractsRoll__When__PureRollApplied()
        {
            // Roll = rotation around local forward (-Z when identity)
            var q = Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, 15f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Pan.Should().BeApproximately(0f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(0f, TOLERANCE);
            euler.Roll.Should().BeApproximately(15f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__RoundTrips__When__IntrinsicYXZ()
        {
            // Intrinsic YXZ: first Y rotation, then X around the new local X
            // Quaternion multiply order for intrinsic: Qx * Qy (rightmost first)
            var qY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -30f * MathF.PI / 180f);
            var qX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 15f * MathF.PI / 180f);
            var combined = Quaternion.Normalize(qX * qY);
            var euler = EulerAngles.FromQuaternion(combined);
            euler.Pan.Should().BeApproximately(30f, TOLERANCE);
            euler.Tilt.Should().BeApproximately(15f, TOLERANCE);
            euler.Roll.Should().BeApproximately(0f, TOLERANCE);
        }

        [Fact]
        public void FromQuaternion__HandlesGimbalEdge__When__TiltAt90()
        {
            // Looking straight up — pan and roll become ambiguous
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90f * MathF.PI / 180f);
            var euler = EulerAngles.FromQuaternion(q);
            euler.Tilt.Should().BeApproximately(90f, 0.5f);
            // Pan + Roll sum should be ~0 (they're coupled at gimbal lock)
        }
    }
}
