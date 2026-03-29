using System;
using System.Collections.Generic;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
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
        /// Per-shot camera aperture keyframes (f-stop).
        /// </summary>
        public KeyframeManager<float> CameraApertureKeyframes { get; } = new();

        /// <summary>
        /// Per-shot camera focal length keyframes (mm).
        /// </summary>
        public KeyframeManager<float> CameraFocalLengthKeyframes { get; } = new();

        /// <summary>
        /// Per-shot camera focus distance keyframes (meters).
        /// </summary>
        public KeyframeManager<float> CameraFocusDistanceKeyframes { get; } = new();

        /// <summary>
        /// Per-shot camera position keyframes.
        /// </summary>
        public KeyframeManager<Vector3> CameraPositionKeyframes { get; }

        /// <summary>
        /// Per-shot camera rotation keyframes.
        /// </summary>
        public KeyframeManager<Quaternion> CameraRotationKeyframes { get; }

        /// <summary>
        /// Per-property stopwatch state for camera recording.
        /// </summary>
        public StopwatchState CameraStopwatch { get; } = new(CameraProperty.COUNT);

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
        /// Total number of camera keyframes across all property managers.
        /// </summary>
        public int TotalCameraKeyframeCount =>
            this.CameraPositionKeyframes.Count
            + this.CameraRotationKeyframes.Count
            + this.CameraApertureKeyframes.Count
            + this.CameraFocalLengthKeyframes.Count
            + this.CameraFocusDistanceKeyframes.Count;

        public void ClearAllCameraKeyframes()
        {
            this.CameraPositionKeyframes.Clear();
            this.CameraRotationKeyframes.Clear();
            this.CameraFocalLengthKeyframes.Clear();
            this.CameraApertureKeyframes.Clear();
            this.CameraFocusDistanceKeyframes.Clear();
        }

        /// <summary>
        /// Evaluates the camera aperture at a local shot time (0 to Duration).
        /// </summary>
        public float EvaluateCameraAperture(TimePosition localTime) =>
            this.CameraApertureKeyframes.Evaluate(localTime, Lerp);

        /// <summary>
        /// Evaluates the camera focal length at a local shot time (0 to Duration).
        /// </summary>
        public float EvaluateCameraFocalLength(TimePosition localTime) =>
            this.CameraFocalLengthKeyframes.Evaluate(localTime, Lerp);

        /// <summary>
        /// Evaluates the camera focus distance at a local shot time (0 to Duration).
        /// </summary>
        public float EvaluateCameraFocusDistance(TimePosition localTime) =>
            this.CameraFocusDistanceKeyframes.Evaluate(localTime, Lerp);

        /// <summary>
        /// Evaluates the camera position at a local shot time (0 to Duration).
        /// </summary>
        public Vector3 EvaluateCameraPosition(TimePosition localTime) =>
            this.CameraPositionKeyframes.Evaluate(localTime, Vector3.Lerp);

        /// <summary>
        /// Evaluates the camera rotation at a local shot time (0 to Duration).
        /// </summary>
        public Quaternion EvaluateCameraRotation(TimePosition localTime) =>
            this.CameraRotationKeyframes.Evaluate(localTime, Quaternion.Slerp);

        /// <summary>
        /// Returns the sorted, deduplicated union of all keyframe times across
        /// every camera property manager (position, rotation, focal length,
        /// aperture, focus distance).
        /// </summary>
        public IReadOnlyList<TimePosition> GetAllCameraKeyframeTimes()
        {
            var times = new SortedSet<double>();

            foreach (var kf in this.CameraPositionKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            foreach (var kf in this.CameraRotationKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            foreach (var kf in this.CameraFocalLengthKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            foreach (var kf in this.CameraApertureKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            foreach (var kf in this.CameraFocusDistanceKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            var result = new List<TimePosition>(times.Count);

            foreach (var t in times)
            {
                result.Add(new TimePosition(t));
            }

            return result;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

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