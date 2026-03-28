using System;
using System.Numerics;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Decomposes a quaternion into pan (yaw), tilt (pitch), and roll in degrees.
    /// Uses YXZ decomposition order matching CameraElement's application order:
    /// Pan around world Y, then Tilt around local X, then Roll around local Z.
    ///
    /// Convention: Pan positive = rightward, Tilt positive = upward,
    /// Roll positive = clockwise from camera POV.
    /// </summary>
    public sealed class EulerAngles
    {
        private const float DEG = 180f / MathF.PI;

        public EulerAngles(float pan, float tilt, float roll)
        {
            this.Pan  = pan;
            this.Tilt = tilt;
            this.Roll = roll;
        }

        public float Pan  { get; }
        public float Roll { get; }
        public float Tilt { get; }

        public static EulerAngles FromQuaternion(Quaternion q)
        {
            // Extract rotation matrix elements from quaternion
            var xx = q.X * q.X;
            var yy = q.Y * q.Y;
            var zz = q.Z * q.Z;
            var xy = q.X * q.Y;
            var xz = q.X * q.Z;
            var yz = q.Y * q.Z;
            var wx = q.W * q.X;
            var wy = q.W * q.Y;
            var wz = q.W * q.Z;

            // Rotation matrix (row-major)
            // R00 R01 R02     1-2(yy+zz)  2(xy-wz)    2(xz+wy)
            // R10 R11 R12  =  2(xy+wz)    1-2(xx+zz)  2(yz-wx)
            // R20 R21 R22     2(xz-wy)    2(yz+wx)    1-2(xx+yy)

            // YXZ decomposition:
            // tilt (X rotation) from R21
            var sinTilt = 2f * (yz + wx);
            sinTilt = Math.Clamp(sinTilt, -1f, 1f);

            float pan, tilt, roll;

            if (MathF.Abs(sinTilt) > 0.9999f)
            {
                // Gimbal lock — tilt at +/-90 degrees
                tilt = MathF.Asin(sinTilt);
                pan  = MathF.Atan2(2f * (xy + wz), 1f - 2f * (xx + zz));
                roll = 0f;
            }
            else
            {
                tilt = MathF.Asin(sinTilt);
                // pan (Y rotation) from R20 and R22
                pan = MathF.Atan2(-(2f * (xz - wy)), 1f - 2f * (xx + yy));
                // roll (Z rotation) from R01 and R11
                roll = MathF.Atan2(-(2f * (xy - wz)), 1f - 2f * (xx + zz));
            }

            // Negate pan because CameraElement.Pan negates the angle.
            // Negate roll because CameraElement.Roll rotates around -Z (forward),
            // but the YXZ decomposition extracts rotation around +Z.
            return new EulerAngles(-pan * DEG, tilt * DEG, -roll * DEG);
        }
    }
}
