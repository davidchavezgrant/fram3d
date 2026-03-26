using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Cross-platform cursor service using a software overlay for custom
    /// cursors so the platform cursor can be hidden independently.
    /// </summary>
    public sealed class UnityCursorService : ICursorService
    {
        private readonly SoftwareCursorOverlay _overlay;
        private          CursorType            _activeCursor = CursorType.Default;

        public UnityCursorService()
        {
            this._overlay = SoftwareCursorOverlay.EnsureCreated();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Setup()
        {
            var service = new UnityCursorService();
            CursorManager.SetService(service);
        }

        public bool SetCursor(CursorType cursor)
        {
            if (cursor == CursorType.Default)
            {
                this.ResetCursor();
                return true;
            }

            if (this._activeCursor == cursor)
            {
                return true;
            }

            if (cursor == CursorType.Link)
            {
                var pointingHand = CursorTextures.PointingHand;

                if (pointingHand == null)
                {
                    Debug.LogWarning("[Cursor] Pointing hand texture failed to load. Cursor remains default.");
                    return false;
                }

                this._overlay.Show(pointingHand, CursorTextures.PointingHandHotspot);
                UnityEngine.Cursor.visible = false;
                this._activeCursor = cursor;

                return true;
            }

            if (cursor == CursorType.ClosedHand)
            {
                var closedHand = CursorTextures.ClosedHand;

                if (closedHand == null)
                {
                    Debug.LogWarning("[Cursor] Closed hand texture failed to load. Cursor remains default.");
                    return false;
                }

                this._overlay.Show(closedHand, CursorTextures.ClosedHandHotspot);
                UnityEngine.Cursor.visible = false;
                this._activeCursor = cursor;

                return true;
            }

            return false;
        }

        public void ResetCursor()
        {
            this._overlay.Hide();
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            UnityEngine.Cursor.visible = true;
            this._activeCursor = CursorType.Default;
        }
    }
}
