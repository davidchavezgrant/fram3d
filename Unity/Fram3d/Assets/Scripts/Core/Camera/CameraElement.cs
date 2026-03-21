using System;
using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Camera
{
    public class CameraElement: Element
    {
        private static readonly Vector3 DEFAULT_POSITION      = new(0f, 1.6f, 5f);
        private const           float   DEFAULT_FOCAL_LENGTH  = 50f;
        private const           float   MIN_FOCAL_LENGTH      = 14f;
        private const           float   MAX_FOCAL_LENGTH      = 400f;
        private const           float   DEFAULT_SENSOR_WIDTH  = 24.89f;
        private const           float   DEFAULT_SENSOR_HEIGHT = 18.66f;
        private                 float   _focalLength          = DEFAULT_FOCAL_LENGTH;

        public float FocalLength
        {
            get => this._focalLength;
            private set
            {
                var min = MIN_FOCAL_LENGTH;
                var max = MAX_FOCAL_LENGTH;

                // Zoom lenses clamp to their actual range
                if (this.ActiveLensSet != null && this.ActiveLensSet.IsZoom)
                {
                    min = Math.Max(min, this.ActiveLensSet.MinFocalLength);
                    max = Math.Min(max, this.ActiveLensSet.MaxFocalLength);
                }

                this._focalLength = Math.Clamp(value, min, max);
            }
        }

        public float      SensorWidth     { get; private set; } = DEFAULT_SENSOR_WIDTH;
        public float      SensorHeight    { get; private set; } = DEFAULT_SENSOR_HEIGHT;
        public CameraBody Body            { get; private set; }
        public LensSet    ActiveLensSet   { get; private set; }
        public Vector3    OrbitPivotPoint { get; set; } = Vector3.Zero;

        /// <summary>
        /// When true, CameraBehaviour applies focal length instantly instead of lerping.
        /// Set by DollyZoom to keep position and focal length perfectly synchronized —
        /// any lerp delay between them breaks the dolly zoom effect and causes visible jitter.
        /// Cleared by CameraBehaviour after consuming.
        /// </summary>
        public bool SnapFocalLength { get; set; }

        public CameraElement(ElementId id, string name): base(id, name)
        {
            this.Position = DEFAULT_POSITION;
            this.Rotation = Quaternion.Identity;
        }

        /// <summary>
        /// Horizontal rotation around the world Y axis through the camera's position.
        /// Positive amount rotates rightward.
        /// </summary>
        public void Pan(float amount)
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -amount);
            this.Rotation = Quaternion.Normalize(rotation * this.Rotation);
        }

        /// <summary>
        /// Vertical rotation around the camera's local right axis.
        /// Positive amount rotates upward.
        /// </summary>
        public void Tilt(float amount)
        {
            var right    = Vector3.Transform(Vector3.UnitX, this.Rotation);
            var rotation = Quaternion.CreateFromAxisAngle(right, amount);
            this.Rotation = Quaternion.Normalize(rotation * this.Rotation);
        }

        /// <summary>
        /// Translate along the camera's local forward axis.
        /// Positive amount moves toward whatever the camera is looking at.
        /// </summary>
        public void Dolly(float amount)
        {
            var forward = this.ComputeLookDirection();
            this.Position += forward * amount;
        }

        /// <summary>
        /// Translate laterally along the camera's local right axis.
        /// Positive amount moves rightward.
        /// </summary>
        public void Truck(float amount)
        {
            var right = Vector3.Transform(Vector3.UnitX, this.Rotation);
            this.Position += right * amount;
        }

        /// <summary>
        /// Translate vertically along the world Y axis.
        /// Positive amount moves upward. World-relative, not camera-relative.
        /// </summary>
        public void Crane(float amount)
        {
            this.Position += Vector3.UnitY * amount;
        }

        /// <summary>
        /// Rotate around the camera's local forward axis.
        /// Positive amount tilts the top of the frame rightward (clockwise from camera's perspective).
        /// </summary>
        public void Roll(float amount)
        {
            var forward  = this.ComputeLookDirection();
            var rotation = Quaternion.CreateFromAxisAngle(forward, amount);
            this.Rotation = Quaternion.Normalize(rotation * this.Rotation);
        }

        /// <summary>
        /// Rotate the camera around OrbitPivotPoint while keeping the pivot centered in frame.
        /// </summary>
        public void Orbit(float horizontalAmount, float verticalAmount)
        {
            var offset        = this.Position - this.OrbitPivotPoint;
            var horizontalRot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -horizontalAmount);
            var right         = Vector3.Transform(Vector3.UnitX, this.Rotation);
            var verticalRot   = Quaternion.CreateFromAxisAngle(right, verticalAmount);
            var combinedRot   = horizontalRot * verticalRot;
            this.Position = this.OrbitPivotPoint + Vector3.Transform(offset, combinedRot);
            this.Rotation = Quaternion.Normalize(combinedRot * this.Rotation);
        }

        /// <summary>
        /// Sets focal length. Behavior depends on the active lens set:
        /// - No lens set or zoom lens: clamps to the lens range (or 14–400mm global range)
        /// - Prime lens set: ignored — use StepFocalLength or SetFocalLengthPreset instead
        /// </summary>
        public void SetFocalLength(float mm)
        {
            if (this.ActiveLensSet != null && !this.ActiveLensSet.IsZoom)
                return;

            this.FocalLength = mm;
        }

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

            // Find the current index (nearest match)
            var currentIndex = 0;
            var minDiff      = float.MaxValue;

            for (var i = 0; i < lengths.Length; i++)
            {
                var diff = MathF.Abs(lengths[i] - this.FocalLength);

                if (diff < minDiff)
                {
                    minDiff      = diff;
                    currentIndex = i;
                }
            }

            var newIndex = Math.Clamp(currentIndex + direction, 0, lengths.Length - 1);
            this.FocalLength     = lengths[newIndex];
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Sets focal length to a specific preset value. Snaps instantly (no lerp).
        /// Works for both prime and zoom lens sets.
        /// </summary>
        public void SetFocalLengthPreset(float mm)
        {
            this.FocalLength     = mm;
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Sets the camera body, updating sensor dimensions. FOV recalculates automatically
        /// since ComputeVerticalFov reads SensorHeight. Focal length is preserved.
        /// </summary>
        public void SetBody(CameraBody body)
        {
            this.Body         = body;
            this.SensorWidth  = body.SensorWidthMm;
            this.SensorHeight = body.SensorHeightMm;
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
                // Clamp to zoom range
                this.FocalLength     = Math.Clamp(this.FocalLength, lensSet.MinFocalLength, lensSet.MaxFocalLength);
                this.SnapFocalLength = true;
            }
            else if (lensSet.FocalLengths.Length > 0)
            {
                // Snap to nearest prime focal length
                var nearest = lensSet.FocalLengths[0];
                var minDiff = MathF.Abs(this.FocalLength - nearest);

                for (var i = 1; i < lensSet.FocalLengths.Length; i++)
                {
                    var diff = MathF.Abs(this.FocalLength - lensSet.FocalLengths[i]);

                    if (diff >= minDiff)
                        continue;

                    minDiff = diff;
                    nearest = lensSet.FocalLengths[i];
                }

                this.FocalLength     = nearest;
                this.SnapFocalLength = true;
            }
        }

        /// <summary>
        /// Computes the vertical field of view in radians from the current focal length and sensor height.
        /// FOV = 2 * atan(sensorHeight / (2 * focalLength))
        /// </summary>

        // TODO: When anamorphic lens is active, compute horizontal FOV using squeeze factor:
        //   hFov = 2 * atan((sensorWidth * squeezeFactor) / (2 * focalLength))
        //   Also auto-lock aspect ratio to the computed delivery format (see 1.2.1).
        public float ComputeVerticalFov() => 2f * MathF.Atan(this.SensorHeight / (2f * this.FocalLength));

        /// <summary>
        /// Simultaneously translates the camera and adjusts focal length to maintain the apparent
        /// size of a subject at the reference point while changing perspective distortion.
        /// Uses OrbitPivotPoint as the reference (world origin by default, focused element later).
        /// </summary>
        public void DollyZoom(float amount)
        {
            // Dolly zoom requires continuous focal length adjustment — disabled for prime lenses
            if (this.ActiveLensSet != null && !this.ActiveLensSet.IsZoom)
                return;

            var forward  = this.ComputeLookDirection();
            var distance = Vector3.Distance(this.Position, this.OrbitPivotPoint);

            if (distance < 0.01f)
                return;

            // Stop if focal length is already at the limit for this direction
            if (amount > 0 && this.FocalLength <= MIN_FOCAL_LENGTH)
                return;

            if (amount < 0 && this.FocalLength >= MAX_FOCAL_LENGTH)
                return;

            // Compute what the new focal length would be
            var newPosition    = this.Position + forward * amount;
            var newDistance    = Vector3.Distance(newPosition, this.OrbitPivotPoint);
            var newFocalLength = this.FocalLength * newDistance / distance;

            // Clamp the focal length and back-compute the position to match
            newFocalLength = Math.Clamp(newFocalLength, MIN_FOCAL_LENGTH, MAX_FOCAL_LENGTH);
            var clampedDistance = distance * newFocalLength / this.FocalLength;
            var direction       = Vector3.Normalize(newPosition - this.OrbitPivotPoint);
            this.Position = this.OrbitPivotPoint + direction * clampedDistance;
            this.SetFocalLength(newFocalLength);
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Restore camera to default position, rotation, and focal length.
        /// Preserves camera body and lens set selection — reset reframes the shot,
        /// it doesn't change the equipment.
        /// </summary>
        public void Reset()
        {
            this.Position        = DEFAULT_POSITION;
            this.Rotation        = Quaternion.Identity;
            this.FocalLength     = DEFAULT_FOCAL_LENGTH;
            this.OrbitPivotPoint = Vector3.Zero;
            this.SnapFocalLength = true;
        }

        /// <summary>
        /// Returns the world-space direction the camera is currently looking at.
        /// Computed by rotating the base forward vector (-Z in right-handed System.Numerics)
        /// by the camera's current rotation.
        /// </summary>
        private Vector3 ComputeLookDirection() => Vector3.Transform(-Vector3.UnitZ, this.Rotation);
    }
}