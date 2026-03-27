using System;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Visible time range with zoom, pan, and pixel conversion.
    /// Pure geometry — no knowledge of shots. Owned by Timeline.
    /// </summary>
    internal sealed class ViewRange
    {
        private const double MIN_VISIBLE_DURATION = 0.5;
        private const double ZOOM_FACTOR          = 1.15;

        private readonly Subject<bool> _changed = new();
        private          double        _stripWidth = 1.0;
        private          double        _viewEnd;
        private          double        _viewStart;

        public IObservable<bool> Changed       => this._changed;
        public double            PixelsPerSecond => this.VisibleDuration > 0 ? this._stripWidth / this.VisibleDuration : this._stripWidth;
        public double            ViewEnd        => this._viewEnd;
        public double            ViewStart      => this._viewStart;
        public double            VisibleDuration => this._viewEnd - this._viewStart;

        public bool IsInitialized => this._viewEnd > 0;

        public void Initialize(double stripWidth, double totalDuration)
        {
            this._stripWidth = Math.Max(stripWidth, 1.0);

            if (!this.IsInitialized)
            {
                this.FitAll(totalDuration);
            }
            else
            {
                this._changed.OnNext(true);
            }
        }

        public void SetStripWidth(double width)
        {
            this._stripWidth = Math.Max(width, 1.0);
            this._changed.OnNext(true);
        }

        public double TimeToPixel(double seconds) => (seconds - this._viewStart) * this.PixelsPerSecond;
        public double PixelToTime(double px)       => this._viewStart + px / this.PixelsPerSecond;

        public void ZoomAtPoint(double anchorSeconds, float scrollDelta, double totalDuration)
        {
            var factor      = scrollDelta > 0 ? 1.0 / ZOOM_FACTOR : ZOOM_FACTOR;
            var newDuration = Math.Clamp(this.VisibleDuration * factor, MIN_VISIBLE_DURATION, totalDuration);
            var t           = this.VisibleDuration > 0
                ? (anchorSeconds - this._viewStart) / this.VisibleDuration
                : 0.5;

            this._viewStart = anchorSeconds - t * newDuration;
            this._viewEnd   = anchorSeconds + (1.0 - t) * newDuration;
            this.Clamp(totalDuration);
            this._changed.OnNext(true);
        }

        public void Pan(double deltaPx, double totalDuration)
        {
            var delta = deltaPx / this.PixelsPerSecond;
            this._viewStart += delta;
            this._viewEnd   += delta;
            this.Clamp(totalDuration);
            this._changed.OnNext(true);
        }

        public void FitAll(double totalDuration)
        {
            var total = Math.Max(totalDuration, 1.0);
            this._viewStart = 0;
            this._viewEnd   = total;
            this._changed.OnNext(true);
        }

        public void FitRange(double start, double end)
        {
            var duration = Math.Max(end - start, 1.0);
            var padding  = duration * 0.08;
            this._viewStart = start - padding;
            this._viewEnd   = end + padding;
            this._changed.OnNext(true);
        }

        public void SetRange(double start, double end, double totalDuration)
        {
            this._viewStart = start;
            this._viewEnd   = end;
            this.Clamp(totalDuration);
            this._changed.OnNext(true);
        }

        public void EnsureVisible(double seconds)
        {
            var duration = this.VisibleDuration;

            if (seconds < this._viewStart)
            {
                this._viewStart = seconds;
                this._viewEnd   = seconds + duration;
                this.ClampLeft();
                this._changed.OnNext(true);
            }
            else if (seconds > this._viewEnd)
            {
                this._viewEnd   = seconds;
                this._viewStart = seconds - duration;
                this.ClampLeft();
                this._changed.OnNext(true);
            }
        }

        private void Clamp(double totalDuration)
        {
            this.ClampLeft();

            if (this._viewEnd > totalDuration)
            {
                var duration    = this.VisibleDuration;
                this._viewEnd   = totalDuration;
                this._viewStart = totalDuration - duration;

                if (this._viewStart < 0)
                {
                    this._viewStart = 0;
                }
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
