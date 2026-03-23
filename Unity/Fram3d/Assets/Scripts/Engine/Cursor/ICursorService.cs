namespace Fram3d.Engine.Cursor
{
    public enum CursorType
    {
        Default,
        Arrow,
        IBeam,
        Crosshair,
        Link,
        Busy,
        Invalid,
        ResizeVertical,
        ResizeHorizontal,
        ResizeDiagonalLeft,
        ResizeDiagonalRight,
        ResizeAll,
        OpenHand,
        ClosedHand
    }


    public interface ICursorService
    {
        void ResetCursor();
        bool SetCursor(CursorType cursor);
    }
}