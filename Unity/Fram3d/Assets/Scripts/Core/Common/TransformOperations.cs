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
        /// Constructs the normal of a drag plane that contains the given axis
        /// and faces the camera as much as possible. Used to create a stable
        /// intersection surface for translate drags. The double cross product
        /// <c>cross(cross(cameraForward, axis), axis)</c> yields the vector
        /// in the axis-containing plane that is most perpendicular to the
        /// camera view direction.
        /// </summary>
        public static Vector3 ConstructDragPlaneNormal(Vector3 axis, Vector3 cameraForward)
        {
            var cross1 = Vector3.Cross(cameraForward, axis);

            if (cross1.LengthSquared() < 0.0001f)
            {
                // Camera looking straight down the axis — pick any perpendicular
                var fallback = Vector3.Cross(axis, Vector3.UnitY);

                if (fallback.LengthSquared() < 0.001f)
                {
                    fallback = Vector3.Cross(axis, Vector3.UnitX);
                }

                return Vector3.Normalize(fallback);
            }

            return Vector3.Normalize(Vector3.Cross(cross1, axis));
        }

        /// <summary>
        /// Projects a camera ray onto a constrained axis via a camera-optimal
        /// drag plane. More stable than line-line closest-point when viewing
        /// the axis at a steep angle. Falls back to the axis origin when the
        /// ray is parallel to the drag plane.
        /// </summary>
        public static Vector3 ProjectOntoAxis(Vector3 axisOrigin,
                                              Vector3 axisDir,
                                              Vector3 rayOrigin,
                                              Vector3 rayDir,
                                              Vector3 cameraForward)
        {
            var planeNormal = ConstructDragPlaneNormal(axisDir, cameraForward);

            // Ray-plane intersection: t = dot(planePoint - rayOrigin, normal) / dot(rayDir, normal)
            var denom = Vector3.Dot(rayDir, planeNormal);

            if (MathF.Abs(denom) < 0.0001f)
            {
                return axisOrigin;
            }

            var t        = Vector3.Dot(axisOrigin - rayOrigin, planeNormal) / denom;
            var hitPoint = rayOrigin + rayDir * t;

            // Project the plane hit onto the axis line
            var axisDist = Vector3.Dot(hitPoint - axisOrigin, axisDir);
            return axisOrigin + axisDir * axisDist;
        }

        /// <summary>
        /// Finds the closest point on an axis line to a ray using line-line
        /// closest-point math. Legacy overload kept for callers that don't
        /// have camera forward available.
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