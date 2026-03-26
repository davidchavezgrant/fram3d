namespace Fram3d.Core.Input
{
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
        public string Name { get; }
    }
}
