using System.Numerics;
namespace Fram3d.Core.Scenes
{
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
}
