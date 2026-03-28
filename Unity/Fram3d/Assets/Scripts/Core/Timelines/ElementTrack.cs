using System;
using System.Collections.Generic;
using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Per-element animation track on the global timeline. Holds position,
    /// rotation, and scale keyframes at absolute global times. Elements without
    /// keyframes stay at their current position — evaluation returns default(T).
    /// </summary>
    public sealed class ElementTrack
    {
        public ElementTrack(ElementId elementId)
        {
            this.ElementId = elementId ?? throw new ArgumentNullException(nameof(elementId));
        }

        public ElementId                   ElementId         { get; }
        public bool                        HasKeyframes      => this.KeyframeCount > 0;
        public int                         KeyframeCount     => this.PositionKeyframes.Count + this.RotationKeyframes.Count + this.ScaleKeyframes.Count;
        public KeyframeManager<Vector3>    PositionKeyframes { get; } = new();
        public KeyframeManager<Quaternion> RotationKeyframes { get; } = new();
        public KeyframeManager<float>      ScaleKeyframes    { get; } = new();

        public Vector3    EvaluatePosition(TimePosition globalTime) => this.PositionKeyframes.Evaluate(globalTime, Vector3.Lerp);
        public Quaternion EvaluateRotation(TimePosition globalTime) => this.RotationKeyframes.Evaluate(globalTime, Quaternion.Slerp);
        public float      EvaluateScale(TimePosition globalTime)    => this.ScaleKeyframes.Evaluate(globalTime, Lerp);

        /// <summary>
        /// Returns the sorted, deduplicated union of all keyframe times across
        /// every property manager (position, rotation, scale).
        /// </summary>
        public IReadOnlyList<TimePosition> GetAllKeyframeTimes()
        {
            var times = new SortedSet<double>();

            foreach (var kf in this.PositionKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            foreach (var kf in this.RotationKeyframes.Keyframes)
            {
                times.Add(kf.Time.Seconds);
            }

            foreach (var kf in this.ScaleKeyframes.Keyframes)
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
    }
}
