using UnityEngine;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Generates a small tileable diagonal-stripe texture for inactive
    /// shot segments. The stripes run at 45 degrees — thin dark lines
    /// on a transparent background.
    /// </summary>
    public static class HatchTexture
    {
        private const int SIZE = 8;

        private static Texture2D _cached;

        public static Texture2D Get()
        {
            if (_cached != null)
            {
                return _cached;
            }

            _cached = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            _cached.filterMode = FilterMode.Point;
            _cached.wrapMode   = TextureWrapMode.Repeat;

            var transparent = new Color(0, 0, 0, 0);
            var stripe      = new Color(0, 0, 0, 0.25f);

            for (var y = 0; y < SIZE; y++)
            {
                for (var x = 0; x < SIZE; x++)
                {
                    var isStripe = (x + y) % SIZE < 2;
                    _cached.SetPixel(x, y, isStripe ? stripe : transparent);
                }
            }

            _cached.Apply();
            return _cached;
        }
    }
}
