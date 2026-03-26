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

            if (!TryGetTexture(cursor, out var texture, out var hotspot))
            {
                return false;
            }

            this._overlay.Show(texture, hotspot);
            UnityEngine.Cursor.visible = false;
            this._activeCursor = cursor;
            return true;
        }

        private static bool TryGetTexture(CursorType cursor, out Texture2D texture, out Vector2 hotspot)
        {
            if (cursor == CursorType.ClosedHand)
            {
                texture = CursorTextures.ClosedHand;
                hotspot = CursorTextures.ClosedHandHotspot;
            }
            else if (cursor == CursorType.IBeam)
            {
                texture = CursorTextures.IBeam;
                hotspot = CursorTextures.IBeamHotspot;
            }
            else if (cursor == CursorType.Link)
            {
                texture = CursorTextures.PointingHand;
                hotspot = CursorTextures.PointingHandHotspot;
            }
            else if (cursor == CursorType.ResizeHorizontal)
            {
                texture = CursorTextures.ResizeHorizontal;
                hotspot = CursorTextures.ResizeHorizontalHotspot;
            }
            else
            {
                texture = null;
                hotspot = Vector2.zero;
            }

            if (texture == null)
            {
                Debug.LogWarning($"[Cursor] Texture for {cursor} failed to load. Cursor remains default.");
            }

            return texture != null;
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
