namespace Fram3d.Core.Input
{
    public readonly struct DragAction
    {
        public static readonly DragAction     NONE = new(DragActionKind.NONE, 0, 0);
        public                 DragActionKind Kind   { get; }
        public                 float          DeltaX { get; }
        public                 float          DeltaY { get; }

        private DragAction(DragActionKind kind, float deltaX, float deltaY)
        {
            this.Kind   = kind;
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
        }

        public static DragAction Orbit(float   deltaX, float deltaY) => new(DragActionKind.ORBIT, deltaX, deltaY);
        public static DragAction PanTilt(float deltaX, float deltaY) => new(DragActionKind.PAN_TILT, deltaX, deltaY);
    }
}
