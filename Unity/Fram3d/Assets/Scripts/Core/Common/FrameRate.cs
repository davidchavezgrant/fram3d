using System;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Frame rate for timeline operations. Provides frame duration
    /// and snap-to-frame-boundary operations.
    /// </summary>
    public sealed class FrameRate: IEquatable<FrameRate>
    {
        public static readonly FrameRate FPS_24    = new(24.0);
        public static readonly FrameRate FPS_25    = new(25.0);
        public static readonly FrameRate FPS_29_97 = new(29.97);
        public static readonly FrameRate FPS_30    = new(30.0);
        public static readonly FrameRate FPS_48    = new(48.0);
        public static readonly FrameRate FPS_59_94 = new(59.94);
        public static readonly FrameRate FPS_60    = new(60.0);

        public FrameRate(double fps)
        {
            if (fps <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(fps), "Frame rate must be positive");
            }

            this.Fps = fps;
        }

        /// <summary>
        /// Duration of a single frame in seconds.
        /// </summary>
        public double FrameDuration => 1.0 / this.Fps;

        public double Fps { get; }

        public bool Equals(FrameRate other) => other != null && Math.Abs(this.Fps - other.Fps) < 1e-9;

        public override bool Equals(object obj) => obj is FrameRate other && this.Equals(other);

        public override int GetHashCode() => this.Fps.GetHashCode();

        /// <summary>
        /// Snaps a time value to the nearest frame boundary at this frame rate.
        /// </summary>
        public TimePosition SnapToFrame(TimePosition time)
        {
            var frame        = Math.Round(time.Seconds * this.Fps);
            var snappedSeconds = frame / this.Fps;
            return new TimePosition(snappedSeconds);
        }

        public override string ToString() => $"{this.Fps}fps";

        public static bool operator ==(FrameRate left, FrameRate right) => Equals(left, right);
        public static bool operator !=(FrameRate left, FrameRate right) => !Equals(left, right);
    }
}
