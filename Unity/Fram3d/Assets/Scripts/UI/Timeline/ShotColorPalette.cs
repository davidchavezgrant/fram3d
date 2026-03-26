using UnityEngine;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Cycling color palette for shot blocks. 8 muted tones
    /// distinguishable in both active and dimmed states.
    /// </summary>
    public static class ShotColorPalette
    {
        private static readonly Color[] COLORS =
        {
            new(0.30f, 0.50f, 0.70f, 1f), // steel blue
            new(0.55f, 0.35f, 0.60f, 1f), // muted purple
            new(0.35f, 0.58f, 0.45f, 1f), // sage green
            new(0.70f, 0.45f, 0.30f, 1f), // burnt orange
            new(0.50f, 0.55f, 0.35f, 1f), // olive
            new(0.60f, 0.35f, 0.40f, 1f), // dusty rose
            new(0.35f, 0.55f, 0.60f, 1f), // teal
            new(0.55f, 0.50f, 0.35f, 1f), // khaki
        };

        public static Color GetColor(int index) => COLORS[index % COLORS.Length];
    }
}
