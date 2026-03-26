using System;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Manages the visible time range for timeline components (shot track, ruler,
    /// keyframe tracks). Provides time-to-pixel and pixel-to-time conversion,
    /// zoom at a point, and pan operations.
    ///
    /// All timeline UI components share a single instance so they stay in sync.
    /// </summary>
    public sealed class TimelineViewState
    {
        private const double MIN_VISIBLE_DURATION = 0.5;
        private const double ZOOM_FACTOR          = 1.15;

        private double _totalDuration;
        private double _viewEnd;
        private double _viewStart;
        private double _stripWidth;

        public TimelineViewState(double totalDuration, double stripWidth)
        {
            this._stripWidth     = Math.Max(stripWidth, 1.0);
            this._totalDuration  = Math.Max(totalDuration, 1.0);
            this.FitAll(totalDuration);
        }

        /// <summary>
        /// Start of the visible time range in seconds.
        /// </summary>
        public double ViewStart => this._viewStart;

        /// <summary>
        /// End of the visible time range in seconds.
        /// </summary>
        public double ViewEnd => this._viewEnd;

        /// <summary>
        /// Visible duration in seconds.
        /// </summary>
        public double VisibleDuration => this._viewEnd - this._viewStart;

        /// <summary>
        /// Pixels per second at the current zoom level.
        /// </summary>
        public double PixelsPerSecond
        {
            get
            {
                if (this.VisibleDuration > 0)
                {
                    return this._stripWidth / this.VisibleDuration;
                }

                return this._stripWidth;
            }
        }

        /// <summary>
        /// Fires when the view range changes (zoom, pan, resize).
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Updates the available strip width when the container resizes.
        /// </summary>
        public void SetStripWidth(double width)
        {
            this._stripWidth = Math.Max(width, 1.0);
            this.Changed?.Invoke();
        }

        /// <summary>
        /// Updates the total duration used for clamping.
        /// </summary>
        public void SetTotalDuration(double totalDuration)
        {
            this._totalDuration = Math.Max(totalDuration, 1.0);
        }

        /// <summary>
        /// Converts a time in seconds to a pixel offset from the left edge of the strip.
        /// </summary>
        public double TimeToPixel(double seconds) =>
            (seconds - this._viewStart) * this.PixelsPerSecond;

        /// <summary>
        /// Converts a pixel offset from the left edge of the strip to a time in seconds.
        /// </summary>
        public double PixelToTime(double px) =>
            this._viewStart + px / this.PixelsPerSecond;

        /// <summary>
        /// Zooms in or out, keeping the given time position at the same pixel location.
        /// Positive delta zooms in, negative zooms out.
        /// </summary>
        public void ZoomAtPoint(double anchorSeconds, float scrollDelta)
        {
            var factor = ZOOM_FACTOR;

            if (scrollDelta > 0)
            {
                factor = 1.0 / ZOOM_FACTOR;
            }
            var newDuration = this.VisibleDuration * factor;

            if (newDuration < MIN_VISIBLE_DURATION)
            {
                newDuration = MIN_VISIBLE_DURATION;
            }

            var maxDuration = this._totalDuration;

            if (newDuration > maxDuration)
            {
                newDuration = maxDuration;
            }

            // Keep the anchor at the same relative position
            var t = 0.5;

            if (this.VisibleDuration > 0)
            {
                t = (anchorSeconds - this._viewStart) / this.VisibleDuration;
            }

            this._viewStart = anchorSeconds - t * newDuration;
            this._viewEnd   = anchorSeconds + (1.0 - t) * newDuration;
            this.Clamp();
            this.Changed?.Invoke();
        }

        /// <summary>
        /// Pans the view by a pixel delta. Positive = scroll right (later times).
        /// </summary>
        public void Pan(double deltaPx)
        {
            var deltaSeconds = deltaPx / this.PixelsPerSecond;
            this._viewStart += deltaSeconds;
            this._viewEnd   += deltaSeconds;
            this.Clamp();
            this.Changed?.Invoke();
        }

        /// <summary>
        /// Sets the view range directly. Used for page-flip during playback.
        /// </summary>
        public void SetViewRange(double start, double end)
        {
            this._viewStart = start;
            this._viewEnd   = end;
            this.Clamp();
            this.Changed?.Invoke();
        }

        /// <summary>
        /// Fits the entire duration into the strip with margin.
        /// </summary>
        public void FitAll(double totalDuration)
        {
            if (totalDuration <= 0)
            {
                totalDuration = 5.0;
            }

            this._viewStart = 0;
            this._viewEnd   = totalDuration;
            this.Changed?.Invoke();
        }

        /// <summary>
        /// Fits a specific time range with 8% padding on each side.
        /// </summary>
        public void FitRange(double start, double end)
        {
            var duration = end - start;

            if (duration <= 0)
            {
                duration = 1.0;
            }

            var padding = duration * 0.08;
            this._viewStart = start - padding;
            this._viewEnd   = end + padding;
            this.Changed?.Invoke();
        }

        /// <summary>
        /// Scrolls to ensure the given time is visible.
        /// </summary>
        public void EnsureVisible(double seconds)
        {
            var duration = this.VisibleDuration;

            if (seconds < this._viewStart)
            {
                // Place the time at the left edge
                this._viewStart = seconds;
                this._viewEnd   = this._viewStart + duration;
                this.ClampLeft();
                this.Changed?.Invoke();
            }
            else if (seconds > this._viewEnd)
            {
                // Place the time at the right edge
                this._viewEnd   = seconds;
                this._viewStart = this._viewEnd - duration;
                this.ClampLeft();
                this.Changed?.Invoke();
            }
        }

        private void Clamp()
        {
            this.ClampLeft();

            // Don't let view start go past total duration (nothing to see)
            if (this._viewStart > this._totalDuration)
            {
                var duration = this.VisibleDuration;
                this._viewStart = Math.Max(0, this._totalDuration - duration);
                this._viewEnd   = this._viewStart + duration;
            }
        }

        private void ClampLeft()
        {
            if (this._viewStart < 0)
            {
                var shift = -this._viewStart;
                this._viewStart += shift;
                this._viewEnd   += shift;
            }
        }
    }
}
