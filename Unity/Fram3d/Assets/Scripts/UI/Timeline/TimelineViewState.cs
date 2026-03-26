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
        private const double MARGIN_SECONDS       = 0.5;
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

            var maxDuration = this._totalDuration + MARGIN_SECONDS * 2;

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
        /// Fits the entire duration into the strip with margin.
        /// </summary>
        public void FitAll(double totalDuration)
        {
            if (totalDuration <= 0)
            {
                totalDuration = 5.0;
            }

            this._viewStart = 0;
            this._viewEnd   = totalDuration + MARGIN_SECONDS;
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
            if (seconds < this._viewStart)
            {
                var shift = this._viewStart - seconds + MARGIN_SECONDS;
                this._viewStart -= shift;
                this._viewEnd   -= shift;
                this.Changed?.Invoke();
            }
            else if (seconds > this._viewEnd)
            {
                var shift = seconds - this._viewEnd + MARGIN_SECONDS;
                this._viewStart += shift;
                this._viewEnd   += shift;
                this.Changed?.Invoke();
            }
        }

        private void Clamp()
        {
            var maxEnd = this._totalDuration + MARGIN_SECONDS;

            // Don't let view start go below 0
            if (this._viewStart < 0)
            {
                var shift = -this._viewStart;
                this._viewStart += shift;
                this._viewEnd   += shift;
            }

            // Don't let view end go too far right
            if (this._viewEnd > maxEnd)
            {
                var shift = this._viewEnd - maxEnd;
                this._viewStart -= shift;
                this._viewEnd   -= shift;

                // Re-clamp left
                if (this._viewStart < 0)
                {
                    this._viewStart = 0;
                }
            }
        }
    }
}
