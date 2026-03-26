namespace Fram3d.Core.Input
{
    public sealed class DragActionKind
    {
        public static readonly DragActionKind NONE     = new("None");
        public static readonly DragActionKind ORBIT    = new("Orbit");
        public static readonly DragActionKind PAN_TILT = new("Pan/Tilt");
        private DragActionKind(string name) => this.Name = name;
        public string Name { get; }
    }
}
