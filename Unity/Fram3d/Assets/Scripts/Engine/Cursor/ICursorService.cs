namespace Fram3d.Engine.Cursor
{
    public enum CursorType
    {
        Default,
        ClosedHand,
        IBeam,
        Link,
        ResizeHorizontal
    }

    public interface ICursorService
    {
        void ResetCursor();
        bool SetCursor(CursorType cursor);
    }
}
