using System;
using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Per-element animation track on the global timeline. Holds position and
    /// rotation keyframes at absolute global times. Elements without keyframes
    /// stay at their current position — evaluation returns default(T).
    /// </summary>
    public sealed class ElementTrack
    {
        public ElementTrack(ElementId elementId)
        {
            this.ElementId = elementId ?? throw new ArgumentNullException(nameof(elementId));
        }

        public ElementId                   ElementId                                 { get; }
        public bool                        HasKeyframes                              => this.KeyframeCount > 0;
        public int                         KeyframeCount                             => this.PositionKeyframes.Count + this.RotationKeyframes.Count;
        public KeyframeManager<Vector3>    PositionKeyframes                         { get; } = new();
        public KeyframeManager<Quaternion> RotationKeyframes                         { get; } = new();
        public Vector3                     EvaluatePosition(TimePosition globalTime) => this.PositionKeyframes.Evaluate(globalTime, Vector3.Lerp);
        public Quaternion                  EvaluateRotation(TimePosition globalTime) => this.RotationKeyframes.Evaluate(globalTime, Quaternion.Slerp);
    }
}