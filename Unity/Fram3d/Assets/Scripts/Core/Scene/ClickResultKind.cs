namespace Fram3d.Core.Scene
{
    public sealed class ClickResultKind
    {
        public static readonly ClickResultKind CLICK = new("Click");
        public static readonly ClickResultKind DRAG  = new("Drag");
        public static readonly ClickResultKind NONE  = new("None");
        private ClickResultKind(string name) => this.Name = name;
        public string Name { get; }
    }
}
