using System.Numerics;
using Fram3d.Core.Cameras;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Captures the camera's animatable properties at a point in time.
    /// Used by KeyframeRecorder for change detection.
    /// </summary>
    public struct CameraSnapshot
    {
        public float      Aperture;
        public float      FocalLength;
        public float      FocusDistance;
        public Vector3    Position;
        public Quaternion Rotation;

        public static CameraSnapshot FromCamera(CameraElement cam)
        {
            var snap       = new CameraSnapshot();
            snap.Position      = cam.Position;
            snap.Rotation      = cam.Rotation;
            snap.FocalLength   = cam.FocalLength;
            snap.FocusDistance  = cam.FocusDistance;
            snap.Aperture      = cam.Aperture;
            return snap;
        }
    }
}
