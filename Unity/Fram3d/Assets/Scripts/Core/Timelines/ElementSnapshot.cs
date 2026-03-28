using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Captures an element's animatable properties at a point in time.
    /// Used by KeyframeRecorder for change detection.
    /// </summary>
    public struct ElementSnapshot
    {
        public Vector3    Position;
        public Quaternion Rotation;
        public float      Scale;

        public static ElementSnapshot FromElement(Element element)
        {
            var snap      = new ElementSnapshot();
            snap.Position = element.Position;
            snap.Rotation = element.Rotation;
            snap.Scale    = element.Scale;
            return snap;
        }
    }
}
