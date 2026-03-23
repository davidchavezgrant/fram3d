namespace Fram3d.Core.Input
{
    public readonly struct ScrollAction
    {
        public ScrollActionKind Kind { get; }
        public float            X    { get; }
        public float            Y    { get; }

        private ScrollAction(ScrollActionKind kind, float x, float y)
        {
            this.Kind = kind;
            this.X    = x;
            this.Y    = y;
        }

        public static ScrollAction Blocked()                       => new(ScrollActionKind.BLOCKED, 0, 0);
        public static ScrollAction Crane(float         y)          => new(ScrollActionKind.CRANE, 0, y);
        public static ScrollAction DollyTruck(float    x, float y) => new(ScrollActionKind.DOLLY_TRUCK, x, y);
        public static ScrollAction DollyZoom(float     y) => new(ScrollActionKind.DOLLY_ZOOM, 0, y);
        public static ScrollAction FocalLength(float   y) => new(ScrollActionKind.FOCAL_LENGTH, 0, y);
        public static ScrollAction FocusDistance(float y) => new(ScrollActionKind.FOCUS_DISTANCE, 0, y);
        public static ScrollAction Roll(float          x) => new(ScrollActionKind.ROLL, x, 0);
    }
}