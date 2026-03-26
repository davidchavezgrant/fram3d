using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Generates cursor textures at runtime as Texture2D. Pixel-accurate
    /// replica of the macOS pointing hand cursor (white fill, black outline).
    /// Used with Unity's Cursor.SetCursor for flicker-free cursor changes.
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

        /// <summary>
        /// Builds a 19x24 pointing hand cursor matching macOS style:
        /// white fill with 1px black outline, index finger pointing up.
        /// </summary>
        private static void BuildPointingHand()
        {
            // macOS pointing hand: index finger up, thumb left, 3 curled fingers
            // Designed at 1x scale (19x24), hotspot at index fingertip
            const int W = 19;
            const int H = 24;

            var b = new Color32(0,   0,   0,   255); // black outline
            var w = new Color32(255, 255, 255, 255); // white fill
            var t = new Color32(0,   0,   0,   0);   // transparent

            // Each row from TOP (y=23) to BOTTOM (y=0), left to right
            // Row index 0 = top of cursor visually
            var rows = new[]
            {
                //         0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
                new[] {    t, t, t, t, t, t, t, b, b, t, t, t, t, t, t, t, t, t, t }, // row 0: fingertip
                new[] {    t, t, t, t, t, t, b, w, w, b, t, t, t, t, t, t, t, t, t }, // row 1
                new[] {    t, t, t, t, t, t, b, w, w, b, t, t, t, t, t, t, t, t, t }, // row 2
                new[] {    t, t, t, t, t, t, b, w, w, b, t, t, t, t, t, t, t, t, t }, // row 3
                new[] {    t, t, t, t, t, t, b, w, w, b, t, t, t, t, t, t, t, t, t }, // row 4
                new[] {    t, t, t, t, t, t, b, w, w, b, b, b, t, t, t, t, t, t, t }, // row 5
                new[] {    t, t, t, t, t, t, b, w, w, b, w, w, b, b, t, t, t, t, t }, // row 6
                new[] {    t, t, t, t, t, t, b, w, w, b, w, w, b, w, b, b, t, t, t }, // row 7
                new[] {    t, t, t, t, t, t, b, w, w, b, w, w, b, w, b, w, b, t, t }, // row 8
                new[] {    t, b, b, t, t, t, b, w, w, w, w, w, b, w, b, w, w, b, t }, // row 9
                new[] {    b, w, w, b, t, t, b, w, w, w, w, w, w, w, w, w, w, b, t }, // row 10
                new[] {    b, w, w, w, b, t, b, w, w, w, w, w, w, w, w, w, w, b, t }, // row 11: thumb connects
                new[] {    t, b, w, w, w, b, b, w, w, w, w, w, w, w, w, w, w, b, t }, // row 12
                new[] {    t, t, b, w, w, w, w, w, w, w, w, w, w, w, w, w, w, b, t }, // row 13
                new[] {    t, t, b, w, w, w, w, w, w, w, w, w, w, w, w, w, b, t, t }, // row 14
                new[] {    t, t, t, b, w, w, w, w, w, w, w, w, w, w, w, w, b, t, t }, // row 15
                new[] {    t, t, t, b, w, w, w, w, w, w, w, w, w, w, w, b, t, t, t }, // row 16
                new[] {    t, t, t, t, b, w, w, w, w, w, w, w, w, w, w, b, t, t, t }, // row 17
                new[] {    t, t, t, t, b, w, w, w, w, w, w, w, w, w, b, t, t, t, t }, // row 18
                new[] {    t, t, t, t, t, b, w, w, w, w, w, w, w, w, b, t, t, t, t }, // row 19
                new[] {    t, t, t, t, t, b, w, w, w, w, w, w, w, b, t, t, t, t, t }, // row 20
                new[] {    t, t, t, t, t, t, b, w, w, w, w, w, w, b, t, t, t, t, t }, // row 21
                new[] {    t, t, t, t, t, t, b, w, w, w, w, w, b, t, t, t, t, t, t }, // row 22
                new[] {    t, t, t, t, t, t, t, b, b, b, b, b, t, t, t, t, t, t, t }, // row 23: bottom
            };

            _pointingHand = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            var pixels = new Color32[W * H];

            for (var row = 0; row < H; row++)
            {
                for (var col = 0; col < W; col++)
                {
                    // rows[] is top-down, Texture2D is bottom-up
                    var texY = H - 1 - row;
                    pixels[texY * W + col] = rows[row][col];
                }
            }

            _pointingHand.SetPixels32(pixels);
            _pointingHand.Apply(false, true);

            // Hotspot at tip of index finger (col 7-8, row 0)
            _pointingHandHotspot = new Vector2(7, 0);
        }
    }
}
