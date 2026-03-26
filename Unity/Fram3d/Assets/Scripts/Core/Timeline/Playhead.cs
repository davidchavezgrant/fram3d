using System;
using Fram3d.Core.Common;
using Fram3d.Core.Shot;
namespace Fram3d.Core.Timeline
{
    /// <summary>
    /// Current time position on the global timeline. Supports scrubbing
    /// (snap to frame boundaries), playback (advance by delta, stop at end),
    /// and shot navigation (resolves current shot from global time).
    ///
    /// Pure C# — no Unity dependencies. Testable with xUnit.
    /// </summary>
    public sealed class Playhead
    {
        private readonly Subject<double> _timeChanged = new();
        private          double          _currentTime;
        private          bool            _isPlaying;

        public Playhead(FrameRate frameRate)
        {
            this.FrameRate = frameRate ?? throw new ArgumentNullException(nameof(frameRate));
        }

        public double CurrentTime => this._currentTime;

        public FrameRate FrameRate { get; }

        public bool IsPlaying => this._isPlaying;

        /// <summary>
        /// Emits the new global time whenever it changes (scrub, playback, reset).
        /// </summary>
        public IObservable<double> TimeChanged => this._timeChanged;

        /// <summary>
        /// Scrubs to a specific time. Snaps to the nearest frame boundary
        /// and clamps to [0, totalDuration].
        /// </summary>
        public void Scrub(double seconds, double totalDuration)
        {
            var snapped = Math.Round(seconds * this.FrameRate.Fps) / this.FrameRate.Fps;
            this.SetTime(Math.Clamp(snapped, 0, totalDuration));
        }

        /// <summary>
        /// Advances the playhead by deltaSeconds during playback.
        /// Returns false if playback should stop (reached end).
        /// </summary>
        public bool Advance(double deltaSeconds, double totalDuration)
        {
            if (!this._isPlaying)
            {
                return false;
            }

            var newTime = this._currentTime + deltaSeconds;

            if (newTime >= totalDuration)
            {
                this.SetTime(totalDuration);
                this._isPlaying = false;
                return false;
            }

            this.SetTime(newTime);
            return true;
        }

        /// <summary>
        /// Toggles playback. If at the end, resets to 0 before playing.
        /// Returns true if now playing.
        /// </summary>
        public bool TogglePlayback(double totalDuration)
        {
            if (!this._isPlaying)
            {
                // If at end, restart from beginning
                if (this._currentTime >= totalDuration - this.FrameRate.FrameDuration)
                {
                    this.SetTime(0);
                }

                this._isPlaying = true;
                return true;
            }

            this._isPlaying = false;
            return false;
        }

        /// <summary>
        /// Resolves which shot the playhead is in and returns the shot-local time.
        /// </summary>
        public (Shot.Shot shot, TimePosition localTime)? ResolveShot(ShotRegistry registry) =>
            registry.GetShotAtGlobalTime(new TimePosition(this._currentTime));

        /// <summary>
        /// Jumps to the start of a specific shot.
        /// </summary>
        public void GoToShot(ShotRegistry registry, ShotId shotId)
        {
            var startTime = registry.GetGlobalStartTime(shotId).Seconds;
            this.SetTime(startTime);
        }

        /// <summary>
        /// Resets to time 0.
        /// </summary>
        public void Reset()
        {
            this._isPlaying = false;
            this.SetTime(0);
        }

        private void SetTime(double time)
        {
            if (Math.Abs(this._currentTime - time) < 1e-9)
            {
                return;
            }

            this._currentTime = time;
            this._timeChanged.OnNext(time);
        }
    }
}
