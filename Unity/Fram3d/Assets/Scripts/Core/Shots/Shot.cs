using System;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Timeline;
namespace Fram3d.Core.Shots
{
    /// <summary>
    /// Aggregate root for a shot. A shot is a named time range with per-shot
    /// camera animation. Element animation lives on the global element timeline
    /// (not here). The shot only stores camera keyframes.
    ///
    /// Invariants:
    /// - Must always have at least one camera position keyframe at t=0
    /// - Must always have at least one camera rotation keyframe at t=0
    /// - Duration is clamped to [MIN_DURATION, MAX_DURATION]
    /// - Name cannot be empty or longer than MAX_NAME_LENGTH
    /// </summary>
    public sealed class Shot
    {
        public const  double MAX_DURATION     = 300.0;
        public const  int    MAX_NAME_LENGTH  = 32;
        public const  double MIN_DURATION     = 0.1;
        public static double DEFAULT_DURATION = 5.0;
        private       double _duration;
        private       string _name;

        public Shot(ShotId     id,
                    string     name,
                    Vector3    cameraPosition,
                    Quaternion cameraRotation)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.SetName(name);
            this._duration               = DEFAULT_DURATION;
            this.CameraPositionKeyframes = new KeyframeManager<Vector3>();
            this.CameraRotationKeyframes = new KeyframeManager<Quaternion>();

            // Create the mandatory initial keyframes at t=0
            var positionId = new KeyframeId(Guid.NewGuid());
            var rotationId = new KeyframeId(Guid.NewGuid());
            var positionKf = new Keyframe<Vector3>(positionId, TimePosition.ZERO, cameraPosition);
            var rotationKf = new Keyframe<Quaternion>(rotationId, TimePosition.ZERO, cameraRotation);
            this.CameraPositionKeyframes.Add(positionKf);
            this.CameraRotationKeyframes.Add(rotationKf);
        }

        /// <summary>
        /// Per-shot camera position keyframes.
        /// </summary>
        public KeyframeManager<Vector3> CameraPositionKeyframes { get; }

        /// <summary>
        /// Per-shot camera rotation keyframes.
        /// </summary>
        public KeyframeManager<Quaternion> CameraRotationKeyframes { get; }

        /// <summary>
        /// Shot duration in seconds. Clamped to [MIN_DURATION, MAX_DURATION].
        /// Shortening duration below existing keyframe times does NOT delete them —
        /// they are preserved but unreachable during playback. Extending again
        /// makes them reachable.
        /// </summary>
        public double Duration
        {
            get => this._duration;
            set => this._duration = Math.Clamp(value, MIN_DURATION, MAX_DURATION);
        }

        public ShotId Id { get; }

        /// <summary>
        /// Shot name. Non-empty, max 32 characters.
        /// Setting to empty or null reverts to current value (no-op).
        /// </summary>
        public string Name
        {
            get => this._name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.SetName(value);
                }
            }
        }

        /// <summary>
        /// Total number of camera keyframes (position + rotation).
        /// </summary>
        public int TotalCameraKeyframeCount => this.CameraPositionKeyframes.Count + this.CameraRotationKeyframes.Count;

        /// <summary>
        /// Evaluates the camera position at a local shot time (0 to Duration).
        /// </summary>
        public Vector3 EvaluateCameraPosition(TimePosition localTime) => this.CameraPositionKeyframes.Evaluate(localTime, Vector3.Lerp);

        /// <summary>
        /// Evaluates the camera rotation at a local shot time (0 to Duration).
        /// </summary>
        public Quaternion EvaluateCameraRotation(TimePosition localTime) => this.CameraRotationKeyframes.Evaluate(localTime, Quaternion.Slerp);

        private void SetName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Shot name cannot be empty", nameof(value));
            }

            if (value.Length > MAX_NAME_LENGTH)
            {
                this._name = value.Substring(0, MAX_NAME_LENGTH);
            }
            else
            {
                this._name = value;
            }
        }
    }
}