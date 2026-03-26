using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Shot
{
    /// <summary>
    /// Ordered collection of shots. Manages shot lifecycle (add, remove, reorder),
    /// auto-naming, current shot tracking, and global timeline time range computation.
    ///
    /// Naming follows insertion order, not position. Shot_01, Shot_02, etc.
    /// Reordering does not rename shots.
    /// </summary>
    public sealed class ShotRegistry
    {
        private const string NAME_PREFIX = "Shot_";

        private readonly List<Shot> _shots = new();

        private int  _nextNumber = 1;
        private Shot _currentShot;

        /// <summary>
        /// The shot currently being viewed/edited. Null when no shots exist.
        /// </summary>
        public Shot CurrentShot
        {
            get => this._currentShot;
            private set
            {
                if (this._currentShot == value)
                {
                    return;
                }

                this._currentShot = value;
                this.CurrentShotChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Number of shots in the registry.
        /// </summary>
        public int Count => this._shots.Count;

        /// <summary>
        /// All shots in sequence order. Read-only view.
        /// </summary>
        public IReadOnlyList<Shot> Shots => this._shots;

        /// <summary>
        /// Total duration of all shots combined.
        /// </summary>
        public double TotalDuration => this._shots.Sum(s => s.Duration);

        /// <summary>
        /// Fired when the current shot changes.
        /// </summary>
        public event EventHandler<Shot> CurrentShotChanged;

        /// <summary>
        /// Fired when a shot is added.
        /// </summary>
        public event EventHandler<Shot> ShotAdded;

        /// <summary>
        /// Fired when a shot is removed.
        /// </summary>
        public event EventHandler<Shot> ShotRemoved;

        /// <summary>
        /// Fired when shots are reordered.
        /// </summary>
        public event EventHandler ShotsReordered;

        /// <summary>
        /// Adds a new shot at the end of the sequence, capturing the given camera state.
        /// Auto-generates a name. Returns the new shot.
        /// </summary>
        public Shot AddShot(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            var name = this.GenerateName();
            var shot = new Shot(
                new ShotId(Guid.NewGuid()),
                name,
                cameraPosition,
                cameraRotation
            );
            this._shots.Add(shot);
            this.CurrentShot = shot;
            this.ShotAdded?.Invoke(this, shot);
            return shot;
        }

        /// <summary>
        /// Removes a shot by ID. Returns true if found and removed.
        /// When the current shot is removed, selects the next shot (or previous if last).
        /// </summary>
        public bool RemoveShot(ShotId id)
        {
            var index = this._shots.FindIndex(s => s.Id == id);
            if (index < 0)
            {
                return false;
            }

            var shot       = this._shots[index];
            var wasCurrent = this.CurrentShot == shot;
            this._shots.RemoveAt(index);
            this.ShotRemoved?.Invoke(this, shot);

            if (wasCurrent)
            {
                if (this._shots.Count == 0)
                {
                    this.CurrentShot = null;
                }
                else if (index < this._shots.Count)
                {
                    this.CurrentShot = this._shots[index];
                }
                else
                {
                    this.CurrentShot = this._shots[this._shots.Count - 1];
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a shot by ID, or null if not found.
        /// </summary>
        public Shot GetById(ShotId id) => this._shots.FirstOrDefault(s => s.Id == id);

        /// <summary>
        /// Gets the index of a shot in the sequence, or -1 if not found.
        /// </summary>
        public int IndexOf(ShotId id) => this._shots.FindIndex(s => s.Id == id);

        /// <summary>
        /// Sets the current shot. The shot must exist in the registry.
        /// </summary>
        public void SetCurrentShot(ShotId id)
        {
            var shot = this.GetById(id);
            if (shot == null)
            {
                throw new ArgumentException($"Shot with ID {id} not found in registry");
            }

            this.CurrentShot = shot;
        }

        /// <summary>
        /// Moves a shot from one position to another in the sequence.
        /// Names are NOT changed on reorder.
        /// </summary>
        public void Reorder(ShotId id, int newIndex)
        {
            var oldIndex = this.IndexOf(id);
            if (oldIndex < 0)
            {
                throw new ArgumentException($"Shot with ID {id} not found in registry");
            }

            if (newIndex < 0 || newIndex >= this._shots.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            if (oldIndex == newIndex)
            {
                return;
            }

            var shot = this._shots[oldIndex];
            this._shots.RemoveAt(oldIndex);
            this._shots.Insert(newIndex, shot);
            this.ShotsReordered?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Computes the start time of a shot on the global element timeline.
        /// Shots are contiguous — each starts where the previous one ends.
        /// </summary>
        public TimePosition GetGlobalStartTime(ShotId id)
        {
            var seconds = 0.0;
            foreach (var shot in this._shots)
            {
                if (shot.Id == id)
                {
                    return new TimePosition(seconds);
                }

                seconds += shot.Duration;
            }

            throw new ArgumentException($"Shot with ID {id} not found in registry");
        }

        /// <summary>
        /// Computes the end time of a shot on the global element timeline.
        /// </summary>
        public TimePosition GetGlobalEndTime(ShotId id)
        {
            var start = this.GetGlobalStartTime(id);
            var shot  = this.GetById(id);
            return start.Add(shot.Duration);
        }

        /// <summary>
        /// Converts a global timeline time to a (Shot, localTime) pair.
        /// Returns null if the time is outside all shots.
        /// </summary>
        public (Shot shot, TimePosition localTime)? GetShotAtGlobalTime(TimePosition globalTime)
        {
            var seconds = 0.0;
            foreach (var shot in this._shots)
            {
                var end = seconds + shot.Duration;
                if (globalTime.Seconds >= seconds && globalTime.Seconds < end)
                {
                    return (shot, new TimePosition(globalTime.Seconds - seconds));
                }

                seconds = end;
            }

            // Check if exactly at the end of the last shot
            if (this._shots.Count > 0 && Math.Abs(globalTime.Seconds - seconds) < 1e-9)
            {
                var lastShot = this._shots[this._shots.Count - 1];
                return (lastShot, new TimePosition(lastShot.Duration));
            }

            return null;
        }

        /// <summary>
        /// Removes all shots and resets the name counter.
        /// </summary>
        public void Clear()
        {
            var removed = new List<Shot>(this._shots);
            this._shots.Clear();
            this.CurrentShot = null;
            this._nextNumber = 1;

            foreach (var shot in removed)
            {
                this.ShotRemoved?.Invoke(this, shot);
            }
        }

        private string GenerateName()
        {
            var name = $"{NAME_PREFIX}{this._nextNumber:D2}";
            this._nextNumber++;
            return name;
        }
    }
}
