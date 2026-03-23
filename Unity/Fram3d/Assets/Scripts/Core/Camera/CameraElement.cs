using System;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Viewport;
namespace Fram3d.Core.Camera
{
    public class CameraElement: Element
    {
        private const float MINIMUM_HEIGHT = 0.1f;

        private static readonly Vector3        DEFAULT_POSITION = new(0f, 1.6f, 5f);
        private readonly        BodyController _body            = new();
        private readonly        LensController _lens            = new();

        public CameraElement(ElementId id, string name): base(id, name)
        {
            this.GroundOffset = MINIMUM_HEIGHT;
            this.Position     = DEFAULT_POSITION;
            this.Rotation     = Quaternion.Identity;
        }

        public AspectRatio ActiveAspectRatio => this._body.ActiveAspectRatio;
        public LensSet     ActiveLensSet     => this._lens.ActiveLensSet;
        public SensorMode  ActiveSensorMode  => this._body.ActiveSensorMode;
        public float       Aperture          => this._lens.Aperture;
        public CameraBody  Body              => this._body.Body;
        public bool        CanDollyZoom      => this.ActiveLensSet == null || this.ActiveLensSet.IsZoom;
        public bool        DofEnabled        { get; set; }

        /// <summary>
        /// Current focal length in mm. Setting this value respects lens constraints:
        /// zoom lenses clamp to their range, prime lenses ignore the write (use
        /// StepFocalLengthUp/Down or SetFocalLengthPreset for primes).
        /// </summary>
        public float FocalLength
        {
            get => this._lens.FocalLength;
            set => this._lens.SetFocalLength(value);
        }

        public bool FocusAtInfinity => this._lens.FocusAtInfinity;

        public float FocusDistance
        {
            get => this._lens.FocusDistance;
            set => this._lens.FocusDistance = value;
        }

        /// <summary>
        /// Computes the vertical field of view in radians from the current focal length and sensor height.
        /// FOV = 2 * atan(sensorHeight / (2 * focalLength))
        /// </summary>

        // TODO: When anamorphic lens is active, compute horizontal FOV using squeeze factor:
        //   hFov = 2 * atan((sensorWidth * squeezeFactor) / (2 * focalLength))
        //   Also auto-lock aspect ratio to the computed delivery format (see 1.2.1).
        public float HorizontalFov => 2f * MathF.Atan(this.SensorWidth / (2f * this._lens.FocalLength));

        public Vector3 OrbitPivotPoint { get; set; } = Vector3.Zero;
        public float   SensorHeight    => this._body.SensorHeight;
        public float   SensorWidth     => this._body.SensorWidth;
        public float   ShakeAmplitude  { get; set; } = 0.1f;
        public bool    ShakeEnabled    { get; set; }
        public float   ShakeFrequency  { get; set; } = 1.0f;

        /// <summary>
        /// When true, CameraBehaviour applies focal length instantly instead of lerping.
        /// Cleared by CameraBehaviour after consuming.
        /// </summary>
        public bool SnapFocalLength
        {
            get => this._lens.SnapFocalLength;
            set => this._lens.SnapFocalLength = value;
        }

        public float VerticalFov => 2f * MathF.Atan(this.SensorHeight / (2f * this._lens.FocalLength));

        /// <summary>
        /// The world-space direction the camera is currently looking at.
        /// Computed by rotating the base forward vector (-Z in right-handed System.Numerics)
        /// by the camera's current rotation.
        /// </summary>
        private Vector3 LookDirection => Vector3.Transform(-Vector3.UnitZ, this.Rotation);

        /// <summary>
        /// Translate vertically along the world Y axis.
        /// Positive amount moves upward. World-relative, not camera-relative.
        /// </summary>
        public void Crane(float amount)
        {
            this.Position += Vector3.UnitY * amount;
        }

        public void CycleAspectRatioBackward() => this._body.CycleAspectRatioBackward();
        public void CycleAspectRatioForward()  => this._body.CycleAspectRatioForward();

        /// <summary>
        /// Translate along the camera's local forward axis.
        /// Positive amount moves toward whatever the camera is looking at.
        /// </summary>
        public void Dolly(float amount)
        {
            var forward = this.LookDirection;
            this.Position += forward * amount;
        }

        /// <summary>
        /// Simultaneously translates the camera and adjusts focal length to maintain the apparent
        /// size of a subject at the reference point while changing perspective distortion.
        /// Uses OrbitPivotPoint as the reference (world origin by default, focused element later).
        /// </summary>
        public void DollyZoom(float amount)
        {
            if (!this.CanDollyZoom)
                return;

            var forward     = this.LookDirection;
            var distance    = Vector3.Distance(this.Position, this.OrbitPivotPoint);
            var focalLength = this._lens.FocalLength;
            var minFocal    = this._lens.EffectiveMinFocalLength;
            var maxFocal    = this._lens.EffectiveMaxFocalLength;

            if (distance < 0.01f)
                return;

            if (amount > 0 && focalLength <= minFocal)
                return;

            if (amount < 0 && focalLength >= maxFocal)
                return;

            var newPosition     = this.Position + forward * amount;
            var newDistance     = Vector3.Distance(newPosition, this.OrbitPivotPoint);
            var newFocalLength  = Math.Clamp(focalLength * newDistance / distance, minFocal, maxFocal);
            var clampedDistance = distance * newFocalLength / focalLength;
            var direction       = Vector3.Normalize(newPosition - this.OrbitPivotPoint);
            this.Position = this.OrbitPivotPoint + direction * clampedDistance;
            this._lens.SetFocalLengthUnchecked(newFocalLength);
            this._lens.SnapFocalLength = true;
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
        /// Horizontal rotation around the world Y axis through the camera's position.
        /// Positive amount rotates rightward.
        /// </summary>
        public void Pan(float amount)
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -amount);
            this.Rotation = Quaternion.Normalize(rotation * this.Rotation);
        }

        /// <summary>
        /// Rotate around the camera's local forward axis.
        /// Positive amount tilts the top of the frame rightward (clockwise from camera's perspective).
        /// </summary>
        public void Roll(float amount)
        {
            var forward  = this.LookDirection;
            var rotation = Quaternion.CreateFromAxisAngle(forward, amount);
            this.Rotation = Quaternion.Normalize(rotation * this.Rotation);
        }

        /// <summary>
        /// Sets the camera body, updating sensor dimensions. FOV recalculates automatically.
        /// Focal length is preserved.
        /// </summary>
        public void SetBody(CameraBody body) => this._body.SetBody(body);

        /// <summary>
        /// Sets focal length to a specific preset value, bypassing prime lens restrictions.
        /// Snaps instantly (no lerp). Used by number key presets.
        /// </summary>
        public void SetFocalLengthPreset(float mm) => this._lens.SetPreset(mm);

        /// <summary>
        /// Sets the active lens set and snaps focal length to the nearest valid value.
        /// Also clamps aperture and focus distance to the lens's physical limits.
        /// </summary>
        public void SetLensSet(LensSet lensSet) => this._lens.SetLensSet(lensSet);

        public void SetSensorMode(SensorMode mode) => this._body.SetSensorMode(mode);
        public void StepApertureNarrower()         => this._lens.StepApertureNarrower();
        public void StepApertureWider()            => this._lens.StepApertureWider();

        /// <summary>
        /// Steps to the next shorter focal length in the active prime lens set.
        /// </summary>
        public void StepFocalLengthDown() => this._lens.StepFocalLengthDown();

        /// <summary>
        /// Steps to the next longer focal length in the active prime lens set.
        /// </summary>
        public void StepFocalLengthUp() => this._lens.StepFocalLengthUp();

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
        /// Translate laterally along the camera's local right axis.
        /// Positive amount moves rightward.
        /// </summary>
        public void Truck(float amount)
        {
            var right = Vector3.Transform(Vector3.UnitX, this.Rotation);
            this.Position += right * amount;
        }

        /// <summary>
        /// Restore camera to default position, rotation, and focal length.
        /// Preserves camera body, lens set, and DOF settings — reset reframes the shot,
        /// it doesn't change the equipment.
        /// </summary>
        public void Reset()
        {
            this.Position        = DEFAULT_POSITION;
            this.Rotation        = Quaternion.Identity;
            this.OrbitPivotPoint = Vector3.Zero;
            this._lens.Reset();
        }
    }
}