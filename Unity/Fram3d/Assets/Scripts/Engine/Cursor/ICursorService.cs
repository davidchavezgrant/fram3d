namespace Fram3d.Engine.Cursor
{
    public enum CursorType
    {
        Default,
        Link
    }

    public interface ICursorService
    {
        void ResetCursor();
        bool SetCursor(CursorType cursor);
    }
}
