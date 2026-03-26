using System;
using System.Collections.Generic;
using System.Linq;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timeline
{
    /// <summary>
    /// Manages an ordered collection of keyframes for a single property track.
    /// Dual storage: sorted list for iteration/evaluation, dictionary for O(1) lookup by ID.
    /// </summary>
    public sealed class KeyframeManager<T>
    {
        private readonly Dictionary<KeyframeId, Keyframe<T>> _byId   = new();
        private readonly List<Keyframe<T>>                   _sorted = new();

        /// <summary>
        /// Number of keyframes in this manager.
        /// </summary>
        public int Count => this._sorted.Count;

        /// <summary>
        /// All keyframes in time order. Returns a read-only view.
        /// </summary>
        public IReadOnlyList<Keyframe<T>> Keyframes => this._sorted;

        /// <summary>
        /// Adds a keyframe. If a keyframe with the same ID already exists, it is replaced.
        /// If a keyframe already exists at the same time (different ID), the existing one
        /// is removed first (merge behavior per spec 3.2.4).
        /// </summary>
        public void Add(Keyframe<T> keyframe)
        {
            if (keyframe == null)
            {
                throw new ArgumentNullException(nameof(keyframe));
            }

            // Remove existing keyframe with same ID (update case)
            if (this._byId.ContainsKey(keyframe.Id))
            {
                this.RemoveById(keyframe.Id);
            }

            // Remove existing keyframe at same time (merge behavior)
            var existingAtTime = this._sorted.FindIndex(k => k.Time == keyframe.Time);
            if (existingAtTime >= 0)
            {
                var existing = this._sorted[existingAtTime];
                this._byId.Remove(existing.Id);
                this._sorted.RemoveAt(existingAtTime);
            }

            this._byId[keyframe.Id] = keyframe;
            this.InsertSorted(keyframe);
        }

        /// <summary>
        /// Removes all keyframes.
        /// </summary>
        public void Clear()
        {
            this._sorted.Clear();
            this._byId.Clear();
        }

        /// <summary>
        /// Evaluates the value at a given time using linear interpolation.
        /// Requires a lerp function since T is generic.
        /// Returns default(T) if no keyframes exist.
        /// Clamps to first/last keyframe outside the keyframe range.
        /// </summary>
        public T Evaluate(TimePosition time, Func<T, T, float, T> lerp)
        {
            if (this._sorted.Count == 0)
            {
                return default;
            }

            if (this._sorted.Count == 1)
            {
                return this._sorted[0].Value;
            }

            // Before first keyframe — clamp
            if (time <= this._sorted[0].Time)
            {
                return this._sorted[0].Value;
            }

            // After last keyframe — clamp
            var last = this._sorted[this._sorted.Count - 1];
            if (time >= last.Time)
            {
                return last.Value;
            }

            // Find the surrounding keyframes
            for (var i = 0; i < this._sorted.Count - 1; i++)
            {
                var current = this._sorted[i];
                var next    = this._sorted[i + 1];

                if (time >= current.Time && time <= next.Time)
                {
                    var span = next.Time.Seconds - current.Time.Seconds;
                    if (span < 1e-9)
                    {
                        return current.Value;
                    }

                    var t = (float)((time.Seconds - current.Time.Seconds) / span);
                    return lerp(current.Value, next.Value, t);
                }
            }

            return last.Value;
        }

        /// <summary>
        /// Gets a keyframe by ID, or null if not found.
        /// </summary>
        public Keyframe<T> GetById(KeyframeId id)
        {
            this._byId.TryGetValue(id, out var keyframe);
            return keyframe;
        }

        /// <summary>
        /// Gets keyframes within a time range (inclusive).
        /// </summary>
        public IReadOnlyList<Keyframe<T>> GetInRange(TimePosition start, TimePosition end)
        {
            return this._sorted
                .Where(k => k.Time >= start && k.Time <= end)
                .ToList();
        }

        /// <summary>
        /// Removes a keyframe by ID. Returns true if found and removed.
        /// </summary>
        public bool RemoveById(KeyframeId id)
        {
            if (!this._byId.TryGetValue(id, out var keyframe))
            {
                return false;
            }

            this._byId.Remove(id);
            this._sorted.Remove(keyframe);
            return true;
        }

        private void InsertSorted(Keyframe<T> keyframe)
        {
            var index = this._sorted.BinarySearch(keyframe);
            if (index < 0)
            {
                index = ~index;
            }

            this._sorted.Insert(index, keyframe);
        }
    }
}
