using System;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timeline
{
    /// <summary>
    /// A recorded value at a specific point in time. Generic over the value type.
    /// Immutable — to change the time or value, create a new keyframe with the same ID.
    /// </summary>
    public sealed class Keyframe<T>: IComparable<Keyframe<T>>
    {
        public Keyframe(KeyframeId id, TimePosition time, T value)
        {
            this.Id    = id   ?? throw new ArgumentNullException(nameof(id));
            this.Time  = time ?? throw new ArgumentNullException(nameof(time));
            this.Value = value;
        }

        public KeyframeId   Id    { get; }
        public TimePosition Time  { get; }
        public T            Value { get; }

        public int CompareTo(Keyframe<T> other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.Time.CompareTo(other.Time);
        }

        /// <summary>
        /// Creates a new keyframe with the same ID but a different time.
        /// </summary>
        public Keyframe<T> WithTime(TimePosition newTime) => new(this.Id, newTime, this.Value);

        /// <summary>
        /// Creates a new keyframe with the same ID but a different value.
        /// </summary>
        public Keyframe<T> WithValue(T newValue) => new(this.Id, this.Time, newValue);
    }
}
