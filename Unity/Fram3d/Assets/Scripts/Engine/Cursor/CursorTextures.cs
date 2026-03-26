using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Loads cursor textures from Resources/Cursors. Textures must be
    /// imported as Cursor type, readable, RGBA32, no mipmaps, with
    /// alphaIsTransparency enabled.
    /// </summary>
    public static class CursorTextures
    {
        private static readonly Vector2 POINTING_HAND_HOTSPOT = new(19, 4);

        private static Texture2D _pointingHand;

        public static Texture2D PointingHand
        {
            get
            {
                if (_pointingHand == null)
                {
                    _pointingHand = Resources.Load<Texture2D>("Cursors/pointer");

                    if (_pointingHand == null)
                    {
                        Debug.LogWarning("[Cursor] Cursors/pointer texture not found in Resources.");
                    }
                }

                return _pointingHand;
            }
        }

        public static Vector2 PointingHandHotspot => POINTING_HAND_HOTSPOT;
    }
}
