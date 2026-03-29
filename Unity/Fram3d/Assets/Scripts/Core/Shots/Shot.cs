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

        public Shot(ShotId id, string name)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.SetName(name);
            this._duration               = DEFAULT_DURATION;
            this.CameraPositionKeyframes = new KeyframeManager<Vector3>();
            this.CameraRotationKeyframes = new KeyframeManager<Quaternion>();
        }

        /// <summary>
        /// The camera position used when no position keyframes exist.
        /// Set by the engine whenever the camera is moved while recording is off.
        /// </summary>
        public Vector3 DefaultCameraPosition { get; set; }

        /// <summary>
        /// The camera rotation used when no rotation keyframes exist.
        /// Set by the engine whenever the camera is moved while recording is off.
        /// </summary>
        public Quaternion DefaultCameraRotation { get; set; } = Quaternion.Identity;

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
        /// Deletes all camera property keyframes at the given time.
        /// </summary>
        public void DeleteAllCameraKeyframesAtTime(TimePosition time)
        {
            removeAtTime(this.CameraPositionKeyframes, time);
            removeAtTime(this.CameraRotationKeyframes, time);
            removeAtTime(this.CameraFocalLengthKeyframes, time);
            removeAtTime(this.CameraApertureKeyframes, time);
            removeAtTime(this.CameraFocusDistanceKeyframes, time);

            static void removeAtTime<T>(KeyframeManager<T> mgr, TimePosition t)
            {
                var kf = mgr.GetAtTime(t);

                if (kf != null)
                {
                    mgr.RemoveById(kf.Id);
                }
            }
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
        /// Returns DefaultCameraPosition if no keyframes exist.
        /// </summary>
        public Vector3 EvaluateCameraPosition(TimePosition localTime) =>
            this.CameraPositionKeyframes.Count > 0
                ? this.CameraPositionKeyframes.Evaluate(localTime, Vector3.Lerp)
                : this.DefaultCameraPosition;

        /// <summary>
        /// Evaluates the camera rotation at a local shot time (0 to Duration).
        /// Returns DefaultCameraRotation if no keyframes exist.
        /// </summary>
        public Quaternion EvaluateCameraRotation(TimePosition localTime) =>
            this.CameraRotationKeyframes.Count > 0
                ? this.CameraRotationKeyframes.Evaluate(localTime, Quaternion.Slerp)
                : this.DefaultCameraRotation;

        /// <summary>
        /// Moves all camera property keyframes at one time to another time.
        /// Uses SetOrMerge so that if keyframes already exist at the target time,
        /// arriving values overwrite (silent merge per spec 3.2.4).
        /// </summary>
        public void MoveAllCameraKeyframesAtTime(TimePosition from, TimePosition to)
        {
            moveAtTime(this.CameraPositionKeyframes, from, to);
            moveAtTime(this.CameraRotationKeyframes, from, to);
            moveAtTime(this.CameraFocalLengthKeyframes, from, to);
            moveAtTime(this.CameraApertureKeyframes, from, to);
            moveAtTime(this.CameraFocusDistanceKeyframes, from, to);

            static void moveAtTime<T>(KeyframeManager<T> mgr, TimePosition f, TimePosition t)
            {
                var kf = mgr.GetAtTime(f);

                if (kf != null)
                {
                    mgr.SetOrMerge(kf.WithTime(t));
                }
            }
        }

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