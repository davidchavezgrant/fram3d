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


    public readonly struct ClickResult
    {
        public ClickResultKind Kind     { get; }
        public Vector2         Position { get; }

        private ClickResult(ClickResultKind kind, Vector2 position)
        {
            this.Kind     = kind;
            this.Position = position;
        }

        public static ClickResult Click(Vector2 position) => new(ClickResultKind.CLICK, position);
        public static ClickResult Drag()                  => new(ClickResultKind.DRAG, Vector2.Zero);
        public static ClickResult None()                  => new(ClickResultKind.NONE, Vector2.Zero);
    }


    public sealed class ClickResultKind
    {
        public static readonly ClickResultKind CLICK = new("Click");
        public static readonly ClickResultKind DRAG  = new("Drag");
        public static readonly ClickResultKind NONE  = new("None");
        private ClickResultKind(string name) => this.Name = name;
        public                 string          Name { get; }
    }
}