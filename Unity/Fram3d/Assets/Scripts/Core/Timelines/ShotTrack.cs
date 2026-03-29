using System;
using System.Collections.Generic;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Ordered collection of shots with track-level operations: add, remove,
    /// reorder, edge detection, boundary resize, fit-to-shot, global time
    /// lookups, and tooltip formatting. Owned by Timeline.
    /// </summary>
    public sealed class ShotTrack
    {
        private readonly Subject<Shot> _added          = new();
        private readonly Subject<Shot> _currentChanged = new();
        private readonly FrameRate     _frameRate;
        private readonly Random         _random         = new();
        private readonly Subject<Shot> _removed        = new();
        private readonly Subject<bool> _reordered      = new();
        private readonly List<Shot>    _shots          = new();
        private          Shot          _currentShot;
        private          int           _nextShotNumber = 1;

        public ShotTrack(FrameRate frameRate)
        {
            this._frameRate = frameRate ?? throw new ArgumentNullException(nameof(frameRate));
        }

        // ── Properties ─────────────────────────────────────────────────

        public int                  Count        => this._shots.Count;
        public Shot                 CurrentShot  => this._currentShot;
        public FrameRate            FrameRate    => this._frameRate;
        public IReadOnlyList<Shot>  Shots        => this._shots;

        public double TotalDuration
        {
            get
            {
                var total = 0.0;

                foreach (var shot in this._shots)
                {
                    total += shot.Duration;
                }

                return total;
            }
        }

        // ── Observables ────────────────────────────────────────────────

        public IObservable<Shot> CurrentShotChanged => this._currentChanged;
        public IObservable<bool> Reordered          => this._reordered;
        public IObservable<Shot> ShotAdded          => this._added;
        public IObservable<Shot> ShotRemoved        => this._removed;

        // ── CRUD ───────────────────────────────────────────────────────

        public Shot AddShot()
        {
            var name = $"Shot_{this._nextShotNumber:D2}";
            this._nextShotNumber++;
            var id   = new ShotId(Guid.NewGuid());
            var shot = new Shot(id, name);
            shot.ColorIndex = this._random.Next(8);
            this._shots.Add(shot);
            this.SetCurrentShot(id);
            this._added.OnNext(shot);
            return shot;
        }

        public Shot GetById(ShotId id) => this._shots.Find(s => s.Id == id);

        public int IndexOf(ShotId id) => this._shots.FindIndex(s => s.Id == id);

        public bool RemoveShot(ShotId id)
        {
            var index = this._shots.FindIndex(s => s.Id == id);

            if (index < 0)
            {
                return false;
            }

            var shot       = this._shots[index];
            var wasCurrent = this._currentShot == shot;
            this._shots.RemoveAt(index);
            this._removed.OnNext(shot);

            if (wasCurrent)
            {
                if (this._shots.Count == 0)
                {
                    this._currentShot = null;
                    this._currentChanged.OnNext(null);
                }
                else if (index < this._shots.Count)
                {
                    this.SetCurrentShot(this._shots[index].Id);
                }
                else
                {
                    this.SetCurrentShot(this._shots[this._shots.Count - 1].Id);
                }
            }

            return true;
        }

        public void Reorder(ShotId id, int newIndex)
        {
            var oldIndex = this._shots.FindIndex(s => s.Id == id);

            if (oldIndex < 0 || newIndex < 0 || newIndex >= this._shots.Count || oldIndex == newIndex)
            {
                return;
            }

            var shot = this._shots[oldIndex];
            this._shots.RemoveAt(oldIndex);
            this._shots.Insert(newIndex, shot);
            this._reordered.OnNext(true);
        }

        public void SetCurrentShot(ShotId id)
        {
            var shot = this._shots.Find(s => s.Id == id);

            if (shot == null || shot == this._currentShot)
            {
                return;
            }

            this._currentShot = shot;
            this._currentChanged.OnNext(shot);
        }

        // ── Global time lookups ────────────────────────────────────────

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

            throw new ArgumentException($"Shot {id} not found");
        }

        public TimePosition GetGlobalEndTime(ShotId id) =>
            this.GetGlobalStartTime(id).Add(this.GetById(id).Duration);

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

            if (this._shots.Count > 0 && Math.Abs(globalTime.Seconds - seconds) < 1e-9)
            {
                var last = this._shots[this._shots.Count - 1];
                return (last, new TimePosition(last.Duration));
            }

            return null;
        }

        // ── Track operations ───────────────────────────────────────────

        public int FindEdgeAtTime(double time, double toleranceSeconds)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._shots.Count; i++)
            {
                runningTime += this._shots[i].Duration;

                if (Math.Abs(time - runningTime) <= toleranceSeconds)
                {
                    return i;
                }
            }

            return -1;
        }

        public int FindInsertionIndex(double time)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._shots.Count; i++)
            {
                var midpoint = runningTime + this._shots[i].Duration / 2.0;

                if (time < midpoint)
                {
                    return i;
                }

                runningTime += this._shots[i].Duration;
            }

            return this._shots.Count;
        }

        public double ResizeShotAtEdge(int shotIndex, double newEndTime)
        {
            var shot        = this._shots[shotIndex];
            var startTime   = this.GetGlobalStartTime(shot.Id).Seconds;
            var newDuration = newEndTime - startTime;
            var snapped     = this._frameRate.SnapToFrame(
                new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));

            shot.Duration = snapped.Seconds;
            return snapped.Seconds;
        }

        // ── Formatting ─────────────────────────────────────────────────

        public string FormatShotTooltip(Shot shot)
        {
            var frames  = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var kfCount = shot.TotalCameraKeyframeCount;
            return $"{shot.Name}\nCam A \u00b7 {shot.Duration:F1}s ({frames}f) \u00b7 {kfCount} kf";
        }

        public string FormatResizeTooltip(int shotIndex, bool shiftHeld)
        {
            var shot   = this._shots[shotIndex];
            var frames = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var mode   = shiftHeld ? "[shots only]" : "[ripple]";
            return $"{shot.Name}: {shot.Duration:F1}s ({frames}f) {mode}";
        }
    }
}
