using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Camera
{
    public class CameraElement: Element
    {
        private static readonly Vector3 DEFAULT_POSITION     = new(0f, 1.6f, 5f);
        private const           float   DEFAULT_FOCAL_LENGTH = 50f;
        public                  float   FocalLength     { get; set; } = DEFAULT_FOCAL_LENGTH;
        public                  Vector3 OrbitPivotPoint { get; set; } = Vector3.Zero;

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
            this.Rotation = rotation * this.Rotation;
        }

        /// <summary>
        /// Vertical rotation around the camera's local right axis.
        /// Positive amount rotates upward.
        /// </summary>
        public void Tilt(float amount)
        {
            var right    = Vector3.Transform(Vector3.UnitX, this.Rotation);
            var rotation = Quaternion.CreateFromAxisAngle(right, amount);
            this.Rotation = rotation * this.Rotation;
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
            this.Rotation = rotation * this.Rotation;
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
            this.Rotation = combinedRot * this.Rotation;
        }

        /// <summary>
        /// Translate along forward axis to maintain subject size while perspective changes.
        /// Focal length adjustment is a stub until 1.1.2 (lens system).
        /// </summary>
        public void DollyZoom(float amount)
        {
            var forward = this.ComputeLookDirection();
            this.Position += forward * amount;

            // TODO 1.1.2: Adjust FocalLength to compensate for distance change
        }

        /// <summary>
        /// Restore camera to default position, rotation, and focal length.
        /// </summary>
        public void Reset()
        {
            this.Position        = DEFAULT_POSITION;
            this.Rotation        = Quaternion.Identity;
            this.FocalLength     = DEFAULT_FOCAL_LENGTH;
            this.OrbitPivotPoint = Vector3.Zero;
        }

        /// <summary>
        /// Returns the world-space direction the camera is currently looking at.
        /// Computed by rotating the base forward vector (-Z in right-handed System.Numerics)
        /// by the camera's current rotation.
        /// </summary>
        private Vector3 ComputeLookDirection() => Vector3.Transform(-Vector3.UnitZ, this.Rotation);
    }
}