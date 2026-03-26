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
                var pointingHand = CursorTextures.PointingHand;

                if (pointingHand == null)
                {
                    Debug.LogWarning("[Cursor] Pointing hand texture failed to load. Cursor remains default.");
                    return false;
                }

                UnityEngine.Cursor.SetCursor(
                    pointingHand,
                    CursorTextures.PointingHandHotspot,
                    CursorMode.ForceSoftware);

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

                UnityEngine.Cursor.SetCursor(
                    closedHand,
                    CursorTextures.ClosedHandHotspot,
                    CursorMode.ForceSoftware);

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
