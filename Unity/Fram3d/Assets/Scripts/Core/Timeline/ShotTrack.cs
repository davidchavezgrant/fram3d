using System;
using System.Collections.Generic;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timeline
{
    /// <summary>
    /// Ordered collection of shots with track-level operations: add, remove,
    /// reorder, edge detection, boundary resize, fit-to-shot, and tooltip
    /// formatting. Wraps ShotRegistry and adds timeline-aware operations.
    /// </summary>
    public sealed class ShotTrack
    {
        private readonly FrameRate     _frameRate;
        private readonly ShotRegistry  _registry;

        public ShotTrack(FrameRate frameRate)
        {
            this._frameRate = frameRate ?? throw new ArgumentNullException(nameof(frameRate));
            this._registry  = new ShotRegistry();
        }

        // ── Delegated registry properties ──────────────────────────────

        public int                  Count        => this._registry.Count;
        public Shot                 CurrentShot  => this._registry.CurrentShot;
        public FrameRate            FrameRate    => this._frameRate;
        public IReadOnlyList<Shot>  Shots        => this._registry.Shots;
        public double               TotalDuration => this._registry.TotalDuration;

        // ── Delegated registry observables ─────────────────────────────

        public IObservable<Shot>  CurrentShotChanged => this._registry.CurrentShotChanged;
        public IObservable<bool>  Reordered          => this._registry.Reordered;
        public IObservable<Shot>  ShotAdded          => this._registry.ShotAdded;
        public IObservable<Shot>  ShotRemoved        => this._registry.ShotRemoved;

        // ── Shot lifecycle ─────────────────────────────────────────────

        public Shot AddShot(Vector3 cameraPosition, Quaternion cameraRotation) =>
            this._registry.AddShot(cameraPosition, cameraRotation);

        public void Clear() => this._registry.Clear();

        public Shot GetById(ShotId id) => this._registry.GetById(id);

        public TimePosition GetGlobalEndTime(ShotId id) => this._registry.GetGlobalEndTime(id);

        public TimePosition GetGlobalStartTime(ShotId id) => this._registry.GetGlobalStartTime(id);

        public (Shot shot, TimePosition localTime)? GetShotAtGlobalTime(TimePosition globalTime) =>
            this._registry.GetShotAtGlobalTime(globalTime);

        public int IndexOf(ShotId id) => this._registry.IndexOf(id);

        public bool RemoveShot(ShotId id) => this._registry.RemoveShot(id);

        public void Reorder(ShotId id, int newIndex) => this._registry.Reorder(id, newIndex);

        public void SetCurrentShot(ShotId id) => this._registry.SetCurrentShot(id);

        // ── Track operations ───────────────────────────────────────────

        /// <summary>
        /// Finds the shot index whose right edge is within toleranceSeconds
        /// of a time position. Returns -1 if no edge is near.
        /// </summary>
        public int FindEdgeAtTime(double time, double toleranceSeconds)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._registry.Shots.Count; i++)
            {
                runningTime += this._registry.Shots[i].Duration;

                if (Math.Abs(time - runningTime) <= toleranceSeconds)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Computes the insertion index for drag-and-drop reorder at the given time.
        /// </summary>
        public int FindInsertionIndex(double time)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._registry.Shots.Count; i++)
            {
                var midpoint = runningTime + this._registry.Shots[i].Duration / 2.0;

                if (time < midpoint)
                {
                    return i;
                }

                runningTime += this._registry.Shots[i].Duration;
            }

            return this._registry.Shots.Count;
        }

        /// <summary>
        /// Resizes a shot by moving its right edge to a new time position.
        /// Snaps to frame boundaries. Returns the actual new duration.
        /// </summary>
        public double ResizeShotAtEdge(int shotIndex, double newEndTime)
        {
            var shot        = this._registry.Shots[shotIndex];
            var startTime   = this._registry.GetGlobalStartTime(shot.Id).Seconds;
            var newDuration = newEndTime - startTime;
            var snapped     = this._frameRate.SnapToFrame(
                new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));

            shot.Duration = snapped.Seconds;
            return snapped.Seconds;
        }

        /// <summary>
        /// Fits the timeline view to show a specific shot with edge-aware padding.
        /// </summary>
        public void FitToShot(ShotId shotId, TimelineState state)
        {
            var shotIndex     = this._registry.IndexOf(shotId);
            var start         = this._registry.GetGlobalStartTime(shotId).Seconds;
            var end           = this._registry.GetGlobalEndTime(shotId).Seconds;
            var duration      = end - start;
            var totalDuration = this._registry.TotalDuration;
            var padding       = duration * 0.08;
            var isFirst       = shotIndex == 0;
            var isLast        = shotIndex == this._registry.Count - 1;

            if (this._registry.Count == 1)
            {
                state.FitAll(totalDuration);
            }
            else if (isFirst)
            {
                state.SetViewRange(0, end + padding);
            }
            else if (isLast)
            {
                state.SetViewRange(start - padding, totalDuration);
            }
            else
            {
                state.FitRange(start, end);
            }
        }

        /// <summary>
        /// Computes resize tooltip text for a boundary drag.
        /// </summary>
        public string FormatResizeTooltip(int shotIndex, bool shiftHeld)
        {
            var shot   = this._registry.Shots[shotIndex];
            var frames = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var mode   = shiftHeld ? "[shots only]" : "[ripple]";
            return $"{shot.Name}: {shot.Duration:F1}s ({frames}f) {mode}";
        }

        /// <summary>
        /// Computes hover tooltip content for a shot.
        /// </summary>
        public string FormatShotTooltip(Shot shot)
        {
            var frames  = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var kfCount = shot.TotalCameraKeyframeCount;
            return $"{shot.Name}\nCam A \u00b7 {shot.Duration:F1}s ({frames}f) \u00b7 {kfCount} kf";
        }
    }
}
