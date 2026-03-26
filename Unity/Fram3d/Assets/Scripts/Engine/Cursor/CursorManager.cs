namespace Fram3d.Engine.Cursor
{
    public static class CursorManager
    {
        private static ICursorService _service;

        public static void ResetCursor() => _service?.ResetCursor();

        public static bool SetCursor(CursorType cursor) =>
            _service != null && _service.SetCursor(cursor);

        public static void SetService(ICursorService service)
        {
            _service?.ResetCursor();
            _service = service;
        }
    }
}
