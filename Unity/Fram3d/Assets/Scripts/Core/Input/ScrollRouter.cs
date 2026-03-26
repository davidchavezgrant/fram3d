using System;
namespace Fram3d.Core.Input
{
    /// <summary>
    /// Routes a scroll sample to a camera action based on modifier state.
    /// Encapsulates the scroll bleed cooldown — trackpad momentum events
    /// arriving within COOLDOWN_SECONDS of the last modifier-qualified scroll
    /// are suppressed to prevent unintended focal-length changes.
    /// </summary>
    public sealed class ScrollRouter
    {
        public const float COOLDOWN_SECONDS = 0.15f;
        private      float _lastModifierScrollTime;

        public ScrollAction Route(float scrollX,
                                  float scrollY,
                                  bool  ctrl,
                                  bool  alt,
                                  bool  shift,
                                  bool  cmd,
                                  float currentTime)
        {
            // Cmd+Alt scroll → dolly zoom
            if (cmd && alt && Math.Abs(scrollY) > 0)
            {
                this._lastModifierScrollTime = currentTime;
                return ScrollAction.DollyZoom(scrollY);
            }

            // Cmd-only scroll → focus distance
            if (cmd && !alt && Math.Abs(scrollY) > 0)
            {
                this._lastModifierScrollTime = currentTime;
                return ScrollAction.FocusDistance(scrollY);
            }

            // Ctrl scroll → dolly (Y) + truck (X)
            if (ctrl)
            {
                this._lastModifierScrollTime = currentTime;
                return ScrollAction.DollyTruck(scrollX, scrollY);
            }

            // Alt scroll → crane
            if (alt && Math.Abs(scrollY) > 0)
            {
                this._lastModifierScrollTime = currentTime;
                return ScrollAction.Crane(scrollY);
            }

            // Shift scroll → roll
            if (shift && Math.Abs(scrollX) > 0)
            {
                this._lastModifierScrollTime = currentTime;
                return ScrollAction.Roll(scrollX);
            }

            // Unmodified scroll — check bleed cooldown
            var gap = currentTime - this._lastModifierScrollTime;

            if (gap < COOLDOWN_SECONDS)
            {
                this._lastModifierScrollTime = currentTime;
                return ScrollAction.Blocked();
            }

            return ScrollAction.FocalLength(scrollY);
        }
    }
}