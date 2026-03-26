using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Generates cursor textures at runtime. Uses Unity's Cursor.SetCursor
    /// API which manages the hardware cursor — no native plugins, no
    /// flicker. Cursor shapes are drawn programmatically to approximate
    /// native macOS cursors.
    /// </summary>
    public static class CursorTextures
    {
        private static Texture2D _pointingHand;
        private static Vector2   _pointingHandHotspot;

        public static Texture2D PointingHand
        {
            get
            {
                if (_pointingHand == null)
                {
                    BuildPointingHand();
                }

                return _pointingHand;
            }
        }

        public static Vector2 PointingHandHotspot
        {
            get
            {
                if (_pointingHand == null)
                {
                    BuildPointingHand();
                }

                return _pointingHandHotspot;
            }
        }

        private static void BuildPointingHand()
        {
            // 24x24 pointing hand cursor — white with black outline
            const int SIZE = 24;
            _pointingHand = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            var pixels = new Color32[SIZE * SIZE];

            // All transparent
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }

            // Draw a simple pointing hand (index finger pointing up-left)
            // Coordinates are bottom-up (y=0 is bottom)
            var black = new Color32(0, 0, 0, 255);
            var white = new Color32(255, 255, 255, 255);

            // Finger (pointing up)
            FillRect(pixels, SIZE, 7, 14, 3, 9, black);   // outline
            FillRect(pixels, SIZE, 8, 15, 1, 7, white);   // fill

            // Hand body
            FillRect(pixels, SIZE, 4, 5, 14, 9, black);   // outline
            FillRect(pixels, SIZE, 5, 6, 12, 7, white);   // fill

            // Thumb
            FillRect(pixels, SIZE, 3, 7, 3, 4, black);    // outline
            FillRect(pixels, SIZE, 4, 8, 1, 2, white);    // fill

            // Wrist
            FillRect(pixels, SIZE, 6, 1, 8, 5, black);    // outline
            FillRect(pixels, SIZE, 7, 2, 6, 3, white);    // fill

            _pointingHand.SetPixels32(pixels);
            _pointingHand.Apply(false, true);

            // Hotspot at tip of index finger
            _pointingHandHotspot = new Vector2(8, SIZE - 23);
        }

        private static void FillRect(Color32[] pixels, int texWidth,
                                      int x, int y, int w, int h, Color32 color)
        {
            for (var dy = 0; dy < h; dy++)
            {
                for (var dx = 0; dx < w; dx++)
                {
                    var px = x + dx;
                    var py = y + dy;

                    if (px >= 0 && px < texWidth && py >= 0 && py < texWidth)
                    {
                        pixels[py * texWidth + px] = color;
                    }
                }
            }
        }
    }
}
