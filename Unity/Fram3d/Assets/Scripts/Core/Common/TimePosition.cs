using System;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// A point in time expressed as seconds. Always non-negative.
    /// Provides frame-aware operations.
    /// </summary>
    public sealed class TimePosition: IEquatable<TimePosition>, IComparable<TimePosition>
    {
        public static readonly TimePosition ZERO = new(0.0);

        public TimePosition(double seconds)
        {
            if (seconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "TimePosition cannot be negative");
            }

            this.Seconds = seconds;
        }

        public double Seconds { get; }

        public TimePosition Add(double seconds) => new(Math.Max(0.0, this.Seconds + seconds));

        public int CompareTo(TimePosition other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.Seconds.CompareTo(other.Seconds);
        }

        public bool Equals(TimePosition other) => other != null && Math.Abs(this.Seconds - other.Seconds) < 1e-9;

        public override bool Equals(object obj) => obj is TimePosition other && this.Equals(other);

        public override int GetHashCode() => this.Seconds.GetHashCode();

        public TimePosition Subtract(double seconds) => new(Math.Max(0.0, this.Seconds - seconds));

        /// <summary>
        /// Converts this time position to a frame number at the given frame rate.
        /// Uses floor to get the frame containing this time.
        /// </summary>
        public int ToFrame(FrameRate frameRate) => (int)Math.Floor(this.Seconds * frameRate.Fps);

        public override string ToString() => $"{this.Seconds:F3}s";

        public static bool operator ==(TimePosition left, TimePosition right) => Equals(left, right);
        public static bool operator !=(TimePosition left, TimePosition right) => !Equals(left, right);
        public static bool operator <(TimePosition  left, TimePosition right) => left != null && right != null && left.Seconds < right.Seconds;
        public static bool operator >(TimePosition  left, TimePosition right) => left != null && right != null && left.Seconds > right.Seconds;
        public static bool operator <=(TimePosition left, TimePosition right) => left != null && right != null && left.Seconds <= right.Seconds;
        public static bool operator >=(TimePosition left, TimePosition right) => left != null && right != null && left.Seconds >= right.Seconds;
    }
}
