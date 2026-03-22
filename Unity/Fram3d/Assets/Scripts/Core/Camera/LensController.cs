using System;
namespace Fram3d.Core.Camera
{
    /// <summary>
    /// Manages focal length, aperture, focus distance, and lens set selection.
    /// Owns all lens-related state — CameraElement delegates lens operations through this.
    /// Does NOT own sensor dimensions or camera body — those belong to BodyController.
    /// </summary>
    public sealed class LensController
    {
        private const           int     DEFAULT_APERTURE_INDEX = 4;
        private const           float   DEFAULT_FOCAL_LENGTH   = 50f;
        private const           float   DEFAULT_FOCUS_DISTANCE = 10f;
        private const           float   MAX_FOCAL_LENGTH       = 400f;
        private const           float   MAX_FOCUS_DISTANCE     = 100f;
        private const           float   MIN_FOCAL_LENGTH       = 14f;
        private const           float   MIN_FOCUS_DISTANCE     = 0.1f;
        private static readonly float[] APERTURE_STOPS         = { 1.4f, 2f, 2.8f, 4f, 5.6f, 8f, 11f, 16f, 22f };
        private                 int     _apertureIndex         = DEFAULT_APERTURE_INDEX;
        private                 float   _focalLength           = DEFAULT_FOCAL_LENGTH;
        private                 float   _focusDistance         = DEFAULT_FOCUS_DISTANCE;
        private                 int     _primeLensIndex;
        public                  LensSet ActiveLensSet { get; private set; }

        /// <summary>
        /// Current aperture as an f-number (e.g. 5.6 for f/5.6).
        /// Derived from the discrete aperture stop index.
        /// </summary>
        public float Aperture => APERTURE_STOPS[this._apertureIndex];

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

        /// <summary>
        /// Distance in meters from the camera to the plane of sharpest focus.
        /// Clamped to the current lens's close focus distance.
        /// </summary>
        public bool FocusAtInfinity => this._focusDistance >= MAX_FOCUS_DISTANCE;

        public float FocusDistance
        {
            get => this._focusDistance;
            set
            {
                var min = this.CurrentCloseFocusM;
                this._focusDistance = Math.Clamp(value, min > 0? min : MIN_FOCUS_DISTANCE, MAX_FOCUS_DISTANCE);
            }
        }

        /// <summary>
        /// When true, CameraBehaviour applies focal length instantly instead of lerping.
        /// Set by operations that require position and focal length to stay perfectly
        /// synchronized (dolly zoom, prime stepping, preset snapping).
        /// Cleared by CameraBehaviour after consuming.
        /// </summary>
        public bool SnapFocalLength { get; set; }

        /// <summary>
        /// The effective maximum focal length considering the active lens set.
        /// </summary>
        internal float EffectiveMaxFocalLength => this.ActiveLensSet != null && this.ActiveLensSet.IsZoom?
                                                      Math.Min(MAX_FOCAL_LENGTH, this.ActiveLensSet.MaxFocalLength) :
                                                      MAX_FOCAL_LENGTH;

        /// <summary>
        /// The effective minimum focal length considering the active lens set.
        /// </summary>
        internal float EffectiveMinFocalLength => this.ActiveLensSet != null && this.ActiveLensSet.IsZoom?
                                                      Math.Max(MIN_FOCAL_LENGTH, this.ActiveLensSet.MinFocalLength) :
                                                      MIN_FOCAL_LENGTH;

        /// <summary>
        /// Close focus distance for the current lens. For primes, this is the
        /// individual lens's value. For zooms, the set-level value.
        /// </summary>
        private float CurrentCloseFocusM
        {
            get
            {
                if (this.ActiveLensSet == null)
                    return 0f;

                if (!this.ActiveLensSet.IsZoom && this._primeLensIndex < this.ActiveLensSet.Specs.Length)
                    return this.ActiveLensSet.Specs[this._primeLensIndex].CloseFocusM;

                return this.ActiveLensSet.CloseFocusM;
            }
        }

        /// <summary>
        /// Maximum aperture (widest, lowest T-stop) for the current lens.
        /// For primes, this is the individual lens's value. For zooms, the set-level value.
        /// </summary>
        private float CurrentMaxAperture
        {
            get
            {
                if (this.ActiveLensSet == null)
                    return 0f;

                if (!this.ActiveLensSet.IsZoom && this._primeLensIndex < this.ActiveLensSet.Specs.Length)
                    return this.ActiveLensSet.Specs[this._primeLensIndex].MaxAperture;

                return this.ActiveLensSet.MaxAperture;
            }
        }

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
        /// Sets the active lens set and snaps the focal length to the nearest valid value.
        /// Also clamps aperture and focus distance to the new lens's limits.
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
            }
            else if (lensSet.FocalLengths.Length > 0)
            {
                this._primeLensIndex = FindNearestIndex(lensSet.FocalLengths, this.FocalLength);
                this.FocalLength     = lensSet.FocalLengths[this._primeLensIndex];
                this.SnapFocalLength = true;
            }

            this.ClampAperture();
            this.ClampFocusDistance();
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
        /// Steps to the next narrower aperture (higher f-number, deeper DOF).
        /// No-op if already at f/22.
        /// </summary>
        public void StepApertureNarrower()
        {
            if (this._apertureIndex < APERTURE_STOPS.Length - 1)
                this._apertureIndex++;
        }

        /// <summary>
        /// Steps to the next wider aperture (lower f-number, shallower DOF).
        /// Clamped to the current lens's widest T-stop. No-op if already at widest.
        /// </summary>
        public void StepApertureWider()
        {
            if (this._apertureIndex <= 0)
                return;

            var candidate   = this._apertureIndex - 1;
            var maxAperture = this.CurrentMaxAperture;

            if (maxAperture > 0 && APERTURE_STOPS[candidate] < maxAperture)
                return;

            this._apertureIndex = candidate;
        }

        /// <summary>
        /// Steps to the next shorter focal length in the active prime lens set.
        /// No-op if no lens set, if the lens set is a zoom, or if already at the shortest lens.
        /// </summary>
        public void StepFocalLengthDown() => this.StepFocalLength(-1);

        /// <summary>
        /// Steps to the next longer focal length in the active prime lens set.
        /// No-op if no lens set, if the lens set is a zoom, or if already at the longest lens.
        /// </summary>
        public void StepFocalLengthUp() => this.StepFocalLength(1);

        /// <summary>
        /// Sets focal length directly, bypassing prime lens restrictions.
        /// Used internally by CameraElement.DollyZoom which needs to adjust
        /// focal length regardless of lens type constraints.
        /// </summary>
        internal void SetFocalLengthUnchecked(float mm) => this.FocalLength = mm;

        private void ClampAperture()
        {
            var maxAperture = this.CurrentMaxAperture;

            if (maxAperture <= 0)
                return;

            while (this._apertureIndex < APERTURE_STOPS.Length - 1 && APERTURE_STOPS[this._apertureIndex] < maxAperture)
                this._apertureIndex++;
        }

        private void ClampFocusDistance()
        {
            var closeFocus = this.CurrentCloseFocusM;

            if (closeFocus > 0 && this._focusDistance < closeFocus)
                this._focusDistance = closeFocus;
        }

        private void StepFocalLength(int direction)
        {
            if (this.ActiveLensSet == null || this.ActiveLensSet.IsZoom)
                return;

            var lengths = this.ActiveLensSet.FocalLengths;

            if (lengths.Length == 0)
                return;

            var currentIndex = FindNearestIndex(lengths, this.FocalLength);
            var newIndex     = Math.Clamp(currentIndex + direction, 0, lengths.Length - 1);
            this._primeLensIndex = newIndex;
            this.FocalLength     = lengths[newIndex];
            this.SnapFocalLength = true;
            this.ClampAperture();
            this.ClampFocusDistance();
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

        /// <summary>
        /// Resets focal length to default. Preserves lens set selection.
        /// </summary>
        public void Reset()
        {
            this.FocalLength     = DEFAULT_FOCAL_LENGTH;
            this.SnapFocalLength = true;
        }
    }
}