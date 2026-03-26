namespace Fram3d.Core.Input
{
    /// <summary>
    /// Routes mouse drag input to camera operations based on modifier
    /// and button state. Includes delta spike rejection (FRA-126).
    /// </summary>
    public static class DragRouter
    {
        public const float MAX_DELTA_SQR = 40000f;

        public static DragAction Route(float deltaX,
                                       float deltaY,
                                       bool  altHeld,
                                       bool  cmdHeld,
                                       bool  leftButton,
                                       bool  middleButton)
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
}