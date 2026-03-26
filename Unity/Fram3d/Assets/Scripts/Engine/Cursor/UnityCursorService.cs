using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Cross-platform cursor service using Unity's Cursor.SetCursor API.
    /// Generates cursor textures programmatically — no native plugins.
    /// Flicker-free because Unity manages the hardware cursor internally.
    /// </summary>
    public sealed class UnityCursorService : ICursorService
    {
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

            if (cursor == CursorType.Link)
            {
                UnityEngine.Cursor.SetCursor(
                    CursorTextures.PointingHand,
                    CursorTextures.PointingHandHotspot,
                    CursorMode.Auto);

                return true;
            }

            return false;
        }

        public void ResetCursor()
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
