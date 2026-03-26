using System.Numerics;
namespace Fram3d.Core.Scene
{
    /// <summary>
    /// Discriminates between clicks and drags using a 3-frame lifecycle:
    ///   Frame 1: button pressed → record position
    ///   Frame 2: button held → check drag threshold
    ///   Frame 3: button released → click (under threshold) or drag (over)
    /// Modifier keys (alt, command) suppress selection entirely.
    /// </summary>
    public sealed class ClickDetector
    {
        public const  float   CLICK_THRESHOLD    = 5f;
        private const float   CLICK_THRESHOLD_SQ = CLICK_THRESHOLD * CLICK_THRESHOLD;
        private       Vector2 _downPosition;
        private       bool    _isDragging;
        private       bool    _valid;

        public void Suppress()
        {
            this._valid = false;
        }

        public ClickResult Update(bool    pressed,
                                  bool    held,
                                  bool    released,
                                  Vector2 position,
                                  bool    cameraModifierHeld)
        {
            if (pressed)
            {
                if (cameraModifierHeld)
                {
                    this._valid = false;
                    return ClickResult.None();
                }

                // Same-frame press+release — instant click, no drag possible.
                // Happens when a frame hitch (GC, domain reload) exceeds the
                // duration of the physical mouse click.
                if (released)
                {
                    this._valid = false;
                    return ClickResult.Click(position);
                }

                this._downPosition = position;
                this._valid        = true;
                this._isDragging   = false;
                return ClickResult.None();
            }

            if (this._valid && held)
            {
                var dx = position.X - this._downPosition.X;
                var dy = position.Y - this._downPosition.Y;

                if (dx * dx + dy * dy > CLICK_THRESHOLD_SQ)
                {
                    this._isDragging = true;
                }

                return ClickResult.None();
            }

            if (this._valid && released)
            {
                this._valid = false;

                if (this._isDragging)
                {
                    return ClickResult.Drag();
                }

                return ClickResult.Click(position);
            }

            return ClickResult.None();
        }
    }
}