using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Emitted when the camera should evaluate at a shot-local time.
    /// </summary>
    public sealed class CameraEvaluation
    {
        public CameraEvaluation(Shot shot, TimePosition localTime)
        {
            this.LocalTime = localTime;
            this.Shot      = shot;
        }

        public TimePosition LocalTime { get; }
        public Shot         Shot      { get; }
    }
}
