using System;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Computes the zoom scale factor that keeps gizmo handles a constant
    /// screen size regardless of camera distance, FOV, or display resolution.
    /// Inspired by the RTG package's CalculateZoomScale approach.
    /// </summary>
    public static class GizmoScaling
    {
        /// <summary>
        /// Fallback multiplier matching the original distance-only formula.
        /// Used when pixelHeight or FOV inputs are invalid.
        /// </summary>
        private const float FALLBACK_SCALE = 0.15f;

        /// <summary>
        /// Tuned so that at 1080p / 65° FOV the result matches the original
        /// CONSTANT_SCREEN_SIZE = 0.15 behavior. Derived from:
        /// 0.15 = 65 / (K * 1080) → K = 65 / (0.15 * 1080) ≈ 0.401
        /// </summary>
        private const float SCREEN_SIZE_FACTOR = 0.401f;

        /// <summary>
        /// Computes the world-space scale factor for a gizmo at the given
        /// distance from a perspective camera. Accounts for FOV and screen
        /// resolution so the gizmo occupies the same screen area regardless
        /// of camera settings or display size.
        /// </summary>
        public static float CalculateZoomScale(float distance,
                                               float fieldOfViewDegrees,
                                               float pixelHeight)
        {
            if (pixelHeight < 1f || fieldOfViewDegrees <= 0f)
            {
                return distance * FALLBACK_SCALE;
            }

            return distance * fieldOfViewDegrees / (SCREEN_SIZE_FACTOR * pixelHeight);
        }
    }
}
