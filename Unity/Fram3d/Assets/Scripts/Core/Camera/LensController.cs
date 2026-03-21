using System;

namespace Fram3d.Core.Camera
{
    /// <summary>
    /// Manages focal length and lens set selection.
    /// Owns lens-related state — CameraElement delegates lens operations through this.
    /// Does NOT own sensor dimensions or camera body — those belong to CameraElement
    /// because they are properties of the camera, not the lens.
    /// </summary>
    public sealed class LensController
    {
        private const float DEFAULT_FOCAL_LENGTH = 50f;
        private const float MIN_FOCAL_LENGTH     = 14f;
        private const float MAX_FOCAL_LENGTH     = 400f;

        private float _focalLength = DEFAULT_FOCAL_LENGTH;

        public float FocalLength
        {
            get => this._focalLength;
            private set
            {
                var min = MIN_FOCAL_LENGTH;
                var max = MAX_FOCAL_LENGTH;

                if (this.ActiveLensSet != null && this.ActiveLensSet.IsZoom)
                {
                    min = Math.Max(min, this.ActiveLensSet.MinFocalLength);
                    max = Math.Min(max, this.ActiveLensSet.MaxFocalLength);
                }

                this._focalLength = Math.Clamp(value, min, max);
            }
        }

        public LensSet ActiveLensSet { get; private set; }

        /// <summary>
        /// When true, CameraBehaviour applies focal length instantly instead of lerping.
        /// Set by operations that require position and focal length to stay perfectly
        /// synchronized (dolly zoom, prime stepping, preset snapping).
        /// Cleared by CameraBehaviour after consuming.
        /// </summary>
        public bool SnapFocalLength { get; set; }

        /// <summary>
        /// Whether dolly zoom is allowed with the current lens set.
        /// Disabled for prime lenses which have fixed focal lengths.
        /// </summary>
        public bool CanDollyZoom =>
            this.ActiveLensSet == null || this.ActiveLensSet.IsZoom;

        /// <summary>
        /// Sets focal length continuously. Respects lens set constraints:
        /// - No lens set or zoom lens: clamps to the lens range (or 14–400mm global range)
        /// - Prime lens set: no-op — use Step or SetPreset instead
        /// </summary>
        public void SetFocalLength(float mm)
        {
            if (this.ActiveLensSet != null && !this.ActiveLensSet.IsZoom)
                return;

            this.FocalLength = mm;
        }

        /// <summary>
        /// Sets focal length directly, bypassing prime lens restrictions.
        /// Used internally by CameraElement.DollyZoom which needs to adjust
        /// focal length regardless of lens type constraints.
        /// </summary>
        internal void SetFocalLengthUnchecked(float mm) =>
            this.FocalLength = mm;

        /// <summary>
        /// Steps to the next (+1) or previous (-1) focal length in the active prime lens set.
        /// No-op if no lens set or if the lens set is a zoom.
        /// </summary>
        public void StepFocalLength(int direction)
        {
            if (this.ActiveLensSet == null || this.ActiveLensSet.IsZoom)
                return;

            var lengths = this.ActiveLensSet.FocalLengths;

            if (lengths.Length == 0)
                return;

            var currentIndex = FindNearestIndex(lengths, this.FocalLength);
            var newIndex     = Math.Clamp(currentIndex + direction, 0, lengths.Length - 1);

            this.FocalLength     = lengths[newIndex];
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Sets focal length to a specific preset value. Snaps instantly (no lerp).
        /// Works for both prime and zoom lens sets.
        /// </summary>
        public void SetPreset(float mm)
        {
            this.FocalLength     = mm;
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Sets the active lens set and snaps the focal length to the nearest valid value.
        /// For primes: snaps to the closest available focal length in the set.
        /// For zooms: clamps to the lens's min/max range.
        /// </summary>
        public void SetLensSet(LensSet lensSet)
        {
            this.ActiveLensSet = lensSet;

            if (lensSet == null)
                return;

            if (lensSet.IsZoom)
            {
                this.FocalLength     = Math.Clamp(this.FocalLength, lensSet.MinFocalLength, lensSet.MaxFocalLength);
                this.SnapFocalLength = true;

                return;
            }

            if (lensSet.FocalLengths.Length == 0)
                return;

            var nearestIndex = FindNearestIndex(lensSet.FocalLengths, this.FocalLength);
            this.FocalLength     = lensSet.FocalLengths[nearestIndex];
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Computes the vertical field of view in radians from the current focal length
        /// and the provided sensor height.
        /// FOV = 2 * atan(sensorHeight / (2 * focalLength))
        /// </summary>
        // TODO: When anamorphic lens is active, compute horizontal FOV using squeeze factor:
        //   hFov = 2 * atan((sensorWidth * squeezeFactor) / (2 * focalLength))
        //   Also auto-lock aspect ratio to the computed delivery format (see 1.2.1).
        public float ComputeVerticalFov(float sensorHeight) =>
            2f * MathF.Atan(sensorHeight / (2f * this.FocalLength));

        /// <summary>
        /// Resets focal length to default. Preserves lens set selection.
        /// </summary>
        public void Reset()
        {
            this.FocalLength     = DEFAULT_FOCAL_LENGTH;
            this.SnapFocalLength = true;
        }

        private static int FindNearestIndex(float[] values, float target)
        {
            var bestIndex = 0;
            var bestDiff  = MathF.Abs(values[0] - target);

            for (var i = 1; i < values.Length; i++)
            {
                var diff = MathF.Abs(values[i] - target);

                if (diff >= bestDiff)
                    continue;

                bestDiff  = diff;
                bestIndex = i;
            }

            return bestIndex;
        }
    }
}
