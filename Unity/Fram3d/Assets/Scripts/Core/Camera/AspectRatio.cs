using System;
namespace Fram3d.Core.Camera
{
    /// <summary>
    /// A fixed set of aspect ratios for the camera view mask. Sealed class with private
    /// constructor — the set of valid ratios is closed. Each ratio carries a display name
    /// and a numeric value (width / height), with null indicating Full Screen (no mask).
    /// </summary>
    public sealed class AspectRatio
    {
        public static readonly AspectRatio FULL_SCREEN = new("Full Screen", null);
        public static readonly AspectRatio RATIO_16_9  = new("16:9",       16f / 9f);
        public static readonly AspectRatio RATIO_16_10 = new("16:10",      16f / 10f);
        public static readonly AspectRatio RATIO_185_1 = new("1.85:1",     1.85f);
        public static readonly AspectRatio RATIO_2_1   = new("2:1",        2f);
        public static readonly AspectRatio RATIO_235_1 = new("2.35:1",     2.35f);
        public static readonly AspectRatio RATIO_239_1 = new("2.39:1",     2.39f);
        public static readonly AspectRatio RATIO_4_3   = new("4:3",        4f / 3f);
        public static readonly AspectRatio RATIO_1_1   = new("1:1",        1f);
        public static readonly AspectRatio RATIO_9_16  = new("9:16",       9f / 16f);

        public static readonly AspectRatio[] ALL =
        {
            FULL_SCREEN, RATIO_16_9, RATIO_16_10, RATIO_185_1, RATIO_2_1,
            RATIO_235_1, RATIO_239_1, RATIO_4_3, RATIO_1_1, RATIO_9_16
        };

        public static readonly AspectRatio DEFAULT = RATIO_16_9;

        private AspectRatio(string displayName, float? value)
        {
            this.DisplayName = displayName;
            this.Value       = value;
        }

        public string DisplayName { get; }
        public float? Value       { get; }

        /// <summary>
        /// Computes the unmasked rectangle within a view of the given dimensions.
        /// Returns (x, y, width, height) where (x, y) is the top-left corner.
        /// Full Screen returns the full view. Other ratios are centered with
        /// letterbox (horizontal bars) or pillarbox (vertical bars) as needed.
        /// </summary>
        public UnmaskedRect ComputeUnmaskedRect(float viewWidth, float viewHeight)
        {
            if (this.Value == null || viewWidth <= 0f || viewHeight <= 0f)
                return new UnmaskedRect(0f, 0f, viewWidth, viewHeight);

            var targetRatio = this.Value.Value;
            var viewRatio   = viewWidth / viewHeight;

            if (targetRatio > viewRatio)
            {
                // Wider than view → letterbox (horizontal bars top/bottom)
                var height = viewWidth / targetRatio;
                var y      = (viewHeight - height) / 2f;
                return new UnmaskedRect(0f, y, viewWidth, height);
            }

            if (targetRatio < viewRatio)
            {
                // Narrower than view → pillarbox (vertical bars left/right)
                var width = viewHeight * targetRatio;
                var x     = (viewWidth - width) / 2f;
                return new UnmaskedRect(x, 0f, width, viewHeight);
            }

            // Exact match — no bars
            return new UnmaskedRect(0f, 0f, viewWidth, viewHeight);
        }

        public AspectRatio Next()
        {
            var index = Array.IndexOf(ALL, this);
            return ALL[(index + 1) % ALL.Length];
        }

        public AspectRatio Previous()
        {
            var index = Array.IndexOf(ALL, this);
            return ALL[(index - 1 + ALL.Length) % ALL.Length];
        }

        public override string ToString() => this.DisplayName;
    }

    public readonly struct UnmaskedRect
    {
        public UnmaskedRect(float x, float y, float width, float height)
        {
            this.X      = x;
            this.Y      = y;
            this.Width  = width;
            this.Height = height;
        }

        public float Height { get; }
        public float Width  { get; }
        public float X      { get; }
        public float Y      { get; }
    }
}
