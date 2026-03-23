using System;
using System.Numerics;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Pure transform computations for gizmo drag operations.
    /// Takes start state + delta inputs, returns new values.
    /// </summary>
    public static class TransformOperations
    {
        private const float DEG_TO_RAD = MathF.PI / 180f;

        public static Quaternion ComputeRotation(Quaternion startRotation,
                                                 Vector3    axis,
                                                 float      deltaPixels,
                                                 float      sensitivity)
        {
            var angle    = deltaPixels * sensitivity * DEG_TO_RAD;
            var rotation = Quaternion.CreateFromAxisAngle(axis, angle);
            return Quaternion.Normalize(rotation * startRotation);
        }

        public static float ComputeScale(float startScale,
                                         float deltaY,
                                         float sensitivity,
                                         float minScale)
        {
            var factor   = 1f + deltaY * sensitivity;
            var newScale = startScale * factor;
            return Math.Max(minScale, newScale);
        }

        public static Vector3 ComputeTranslation(Vector3 startPosition,
                                                 Vector3 axis,
                                                 Vector3 projected,
                                                 Vector3 origin,
                                                 Vector3 startAxisOffset)
        {
            var delta     = projected - origin;
            var axisDelta = Vector3.Dot(delta, axis) * axis;
            return startPosition + axisDelta - startAxisOffset;
        }

        /// <summary>
        /// Finds the closest point on an axis line to a ray. Used for
        /// translate-drag projection: given a camera ray from the mouse,
        /// find where it intersects the constrained axis.
        /// </summary>
        public static Vector3 ProjectOntoAxis(Vector3 axisOrigin,
                                              Vector3 axisDir,
                                              Vector3 rayOrigin,
                                              Vector3 rayDir)
        {
            var w     = rayOrigin - axisOrigin;
            var a     = Vector3.Dot(rayDir,  rayDir);
            var b     = Vector3.Dot(rayDir,  axisDir);
            var c     = Vector3.Dot(axisDir, axisDir);
            var d     = Vector3.Dot(rayDir,  w);
            var e     = Vector3.Dot(axisDir, w);
            var denom = a * c - b * b;

            if (MathF.Abs(denom) < 0.0001f)
            {
                return axisOrigin;
            }

            var s = (a * e - b * d) / denom;
            return axisOrigin + axisDir * s;
        }
    }
}