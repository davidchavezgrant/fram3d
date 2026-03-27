using System;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timeline
{
    /// <summary>
    /// Pure logic for shot track operations: edge detection, insertion index,
    /// boundary resize, fit-to-shot, and tooltip content. No Unity dependencies.
    ///
    /// The UI layer maps pixels to times, calls these methods, and renders.
    /// </summary>
    public sealed class ShotTrack
    {
        private readonly FrameRate     _frameRate;
        private readonly ShotRegistry  _registry;
        private readonly TimelineState _state;

        public ShotTrack(ShotRegistry registry, TimelineState state, FrameRate frameRate)
        {
            this._registry  = registry ?? throw new ArgumentNullException(nameof(registry));
            this._state     = state ?? throw new ArgumentNullException(nameof(state));
            this._frameRate = frameRate ?? throw new ArgumentNullException(nameof(frameRate));
        }

        /// <summary>
        /// Finds the shot index whose right edge is within the given pixel
        /// tolerance of a time position. Returns -1 if no edge is near.
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
        /// Computes the insertion index for a drag-and-drop reorder at the
        /// given time. Returns the index BEFORE which the shot would be inserted.
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
        /// Snaps to frame boundaries. Enforces minimum duration.
        /// Returns the actual new duration after snapping and clamping.
        /// </summary>
        public double ResizeShotAtEdge(int shotIndex, double newEndTime)
        {
            var shot      = this._registry.Shots[shotIndex];
            var startTime = this._registry.GetGlobalStartTime(shot.Id).Seconds;
            var newDuration = newEndTime - startTime;
            var snapped   = this._frameRate.SnapToFrame(
                new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));

            shot.Duration = snapped.Seconds;
            return snapped.Seconds;
        }

        /// <summary>
        /// Computes the resize tooltip text for a boundary drag.
        /// </summary>
        public string FormatResizeTooltip(int shotIndex, bool shiftHeld)
        {
            var shot   = this._registry.Shots[shotIndex];
            var frames = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var mode   = "[ripple]";

            if (shiftHeld)
            {
                mode = "[shots only]";
            }

            return $"{shot.Name}: {shot.Duration:F1}s ({frames}f) {mode}";
        }

        /// <summary>
        /// Fits the timeline view to show a specific shot with edge-aware padding.
        /// First shot: no left padding. Last shot: no right padding.
        /// Single shot: full timeline. Middle shots: 8% padding both sides.
        /// </summary>
        public void FitToShot(ShotId shotId)
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
                this._state.FitAll(totalDuration);
            }
            else if (isFirst)
            {
                this._state.SetViewRange(0, end + padding);
            }
            else if (isLast)
            {
                this._state.SetViewRange(start - padding, totalDuration);
            }
            else
            {
                this._state.FitRange(start, end);
            }
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

        /// <summary>
        /// Converts a pixel tolerance to a time tolerance at the current zoom.
        /// </summary>
        public double PixelToleranceToTime(double pixels)
        {
            if (this._state.PixelsPerSecond <= 0)
            {
                return 1.0;
            }

            return pixels / this._state.PixelsPerSecond;
        }
    }
}
