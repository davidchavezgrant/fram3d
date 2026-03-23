namespace Fram3d.Core.Input
{
    /// <summary>
    /// Routes mouse drag input to camera operations based on modifier
    /// and button state. Includes delta spike rejection (FRA-126).
    /// </summary>
    public static class DragRouter
    {
        public const float MAX_DELTA_SQR = 40000f;

        public static DragAction Route(float deltaX, float deltaY,
                                       bool altHeld, bool cmdHeld,
                                       bool leftButton, bool middleButton)
        {
            var sqr = deltaX * deltaX + deltaY * deltaY;

            if (sqr < 0.001f)
            {
                return DragAction.NONE;
            }

            if (sqr > MAX_DELTA_SQR)
            {
                return DragAction.NONE;
            }

            if (altHeld && leftButton)
            {
                return DragAction.Orbit(deltaX, deltaY);
            }

            if (cmdHeld && leftButton)
            {
                return DragAction.PanTilt(deltaX, deltaY);
            }

            if (middleButton)
            {
                return DragAction.PanTilt(deltaX, deltaY);
            }

            return DragAction.NONE;
        }
    }

    public readonly struct DragAction
    {
        public static readonly DragAction NONE = new(DragActionKind.NONE, 0, 0);

        public DragActionKind Kind   { get; }
        public float          DeltaX { get; }
        public float          DeltaY { get; }

        private DragAction(DragActionKind kind, float deltaX, float deltaY)
        {
            this.Kind   = kind;
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
        }

        public static DragAction Orbit(float deltaX, float deltaY) =>
            new(DragActionKind.ORBIT, deltaX, deltaY);

        public static DragAction PanTilt(float deltaX, float deltaY) =>
            new(DragActionKind.PAN_TILT, deltaX, deltaY);
    }

    public sealed class DragActionKind
    {
        public static readonly DragActionKind NONE     = new("None");
        public static readonly DragActionKind ORBIT    = new("Orbit");
        public static readonly DragActionKind PAN_TILT = new("Pan/Tilt");

        public string Name { get; }
        private DragActionKind(string name) => this.Name = name;
    }
}
