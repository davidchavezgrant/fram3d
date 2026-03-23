using UnityEngine;
namespace Fram3d.Engine.Cursor
{
    public static class CursorManager
    {
        private static ICursorService _defaultService;
        private static ICursorService _instance;
        private static bool           _warnedNoService;
        public static  string         ServiceName => _instance == null? "NULL" : _instance.GetType().Name;

        public static void ResetCursor()
        {
            if (_instance != null)
            {
                _instance.ResetCursor();
                return;
            }

            WarnNoService();
        }

        public static bool SetCursor(CursorType cursor)
        {
            if (_instance != null)
            {
                return _instance.SetCursor(cursor);
            }

            WarnNoService();
            return false;
        }

        public static void SetFallbackService(ICursorService service)
        {
            _defaultService = service;
        }

        public static void SetService(ICursorService service)
        {
            if (_instance == service)
            {
                return;
            }

            _instance?.ResetCursor();
            _instance = service;
            _instance?.SetCursor(CursorType.Default);
            _warnedNoService = false;
        }

        private static void WarnNoService()
        {
            if (_warnedNoService)
            {
                return;
            }

            Debug.LogWarning("[CursorManager] No ICursorService registered. Platform cursor service may have failed to initialize.");
            _warnedNoService = true;
        }
    }
}