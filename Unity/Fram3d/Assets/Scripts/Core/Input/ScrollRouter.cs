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


    public sealed class ScrollActionKind
    {
        public static readonly ScrollActionKind BLOCKED        = new("Blocked");
        public static readonly ScrollActionKind CRANE          = new("Crane");
        public static readonly ScrollActionKind DOLLY_TRUCK    = new("Dolly + Truck");
        public static readonly ScrollActionKind DOLLY_ZOOM     = new("Dolly Zoom");
        public static readonly ScrollActionKind FOCAL_LENGTH   = new("Focal Length");
        public static readonly ScrollActionKind FOCUS_DISTANCE = new("Focus Distance");
        public static readonly ScrollActionKind ROLL           = new("Roll");
        private ScrollActionKind(string name) => this.Name = name;
        public                 string           Name { get; }
    }
}