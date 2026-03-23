namespace Riten.Native.Cursors
{
    public static class NativeCursor
    {
        static         ICursorService _instance;
        private static ICursorService _defaultService;

        public static string ServiceName => _instance == null ? "NULL" : _instance.GetType().Name;

        public static void SetFallbackService(ICursorService service)
        {
            _defaultService = service;
        }

        public static void SetService(ICursorService service)
        {
            if (_instance == service)
                return;

            _instance?.ResetCursor();
            _instance = service;
            _instance?.SetCursor(NTCursors.Default);
        }

        public static bool SetCursor(NTCursors ntCursor)
        {
            return _instance != null && _instance.SetCursor(ntCursor);
        }

        public static void ResetCursor()
        {
            _instance?.ResetCursor();
        }
    }
}
