using System;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Stateless utility that creates/updates keyframes based on property changes.
    /// Compares current vs previous snapshots, checks per-property stopwatch state,
    /// applies change thresholds, and either merges into nearby keyframes or creates new ones.
    /// </summary>
    public static class KeyframeRecorder
    {
        public static void RecordCamera(
            Shot shot,
            StopwatchState stopwatch,
            TimePosition localTime,
            CameraSnapshot current,
            CameraSnapshot previous)
        {
            if (stopwatch.IsRecording(CameraProperty.POSITION.Index)
                && PositionChanged(current.Position, previous.Position))
            {
                RecordToManager(shot.CameraPositionKeyframes, localTime, current.Position);
            }

            if (stopwatch.IsRecording(CameraProperty.ROTATION.Index)
                && RotationChanged(current.Rotation, previous.Rotation))
            {
                RecordToManager(shot.CameraRotationKeyframes, localTime, current.Rotation);
            }

            if (stopwatch.IsRecording(CameraProperty.FOCAL_LENGTH.Index)
                && FloatChanged(current.FocalLength, previous.FocalLength, RecordingThresholds.FOCAL_LENGTH))
            {
                RecordToManager(shot.CameraFocalLengthKeyframes, localTime, current.FocalLength);
            }

            if (stopwatch.IsRecording(CameraProperty.FOCUS_DISTANCE.Index)
                && FloatChanged(current.FocusDistance, previous.FocusDistance, RecordingThresholds.FOCUS_DISTANCE))
            {
                RecordToManager(shot.CameraFocusDistanceKeyframes, localTime, current.FocusDistance);
            }

            if (stopwatch.IsRecording(CameraProperty.APERTURE.Index)
                && FloatChanged(current.Aperture, previous.Aperture, RecordingThresholds.FOCAL_LENGTH))
            {
                RecordToManager(shot.CameraApertureKeyframes, localTime, current.Aperture);
            }
        }

        public static void RecordElement(
            ElementTrack track,
            StopwatchState stopwatch,
            TimePosition globalTime,
            ElementSnapshot current,
            ElementSnapshot previous)
        {
            if (stopwatch.IsRecording(ElementProperty.POSITION.Index)
                && PositionChanged(current.Position, previous.Position))
            {
                RecordToManager(track.PositionKeyframes, globalTime, current.Position);
            }

            if (stopwatch.IsRecording(ElementProperty.ROTATION.Index)
                && RotationChanged(current.Rotation, previous.Rotation))
            {
                RecordToManager(track.RotationKeyframes, globalTime, current.Rotation);
            }

            if (stopwatch.IsRecording(ElementProperty.SCALE.Index)
                && FloatChanged(current.Scale, previous.Scale, RecordingThresholds.SCALE))
            {
                RecordToManager(track.ScaleKeyframes, globalTime, current.Scale);
            }
        }

        public static void ForceRecordCamera(
            Shot shot,
            TimePosition localTime,
            CameraSnapshot current)
        {
            RecordToManager(shot.CameraPositionKeyframes, localTime, current.Position);
            RecordToManager(shot.CameraRotationKeyframes, localTime, current.Rotation);
            RecordToManager(shot.CameraFocalLengthKeyframes, localTime, current.FocalLength);
            RecordToManager(shot.CameraFocusDistanceKeyframes, localTime, current.FocusDistance);
            RecordToManager(shot.CameraApertureKeyframes, localTime, current.Aperture);
        }

        public static void ForceRecordElement(
            ElementTrack track,
            TimePosition globalTime,
            ElementSnapshot current)
        {
            RecordToManager(track.PositionKeyframes, globalTime, current.Position);
            RecordToManager(track.RotationKeyframes, globalTime, current.Rotation);
            RecordToManager(track.ScaleKeyframes, globalTime, current.Scale);
        }

        private static Keyframe<T> FindNearestWithinWindow<T>(
            KeyframeManager<T> manager,
            TimePosition time)
        {
            Keyframe<T> nearest = null;
            var minDistance      = double.MaxValue;

            foreach (var kf in manager.Keyframes)
            {
                var distance = Math.Abs(kf.Time.Seconds - time.Seconds);

                if (distance < RecordingThresholds.MERGE_WINDOW && distance < minDistance)
                {
                    minDistance = distance;
                    nearest    = kf;
                }
            }

            return nearest;
        }

        private static bool FloatChanged(float current, float previous, float threshold) =>
            Math.Abs(current - previous) > threshold;

        private static bool PositionChanged(Vector3 current, Vector3 previous) =>
            Math.Abs(current.X - previous.X) > RecordingThresholds.POSITION
            || Math.Abs(current.Y - previous.Y) > RecordingThresholds.POSITION
            || Math.Abs(current.Z - previous.Z) > RecordingThresholds.POSITION;

        private static void RecordToManager<T>(
            KeyframeManager<T> manager,
            TimePosition time,
            T value)
        {
            var existing = FindNearestWithinWindow(manager, time);

            if (existing != null)
            {
                manager.SetOrMerge(existing.WithValue(value));
            }
            else
            {
                var kf = new Keyframe<T>(new KeyframeId(Guid.NewGuid()), time, value);
                manager.Add(kf);
            }
        }

        private static bool RotationChanged(Quaternion current, Quaternion previous)
        {
            var currentEuler  = EulerAngles.FromQuaternion(current);
            var previousEuler = EulerAngles.FromQuaternion(previous);
            return Math.Abs(currentEuler.Pan - previousEuler.Pan) > RecordingThresholds.ROTATION_DEG
                   || Math.Abs(currentEuler.Tilt - previousEuler.Tilt) > RecordingThresholds.ROTATION_DEG
                   || Math.Abs(currentEuler.Roll - previousEuler.Roll) > RecordingThresholds.ROTATION_DEG;
        }
    }
}
