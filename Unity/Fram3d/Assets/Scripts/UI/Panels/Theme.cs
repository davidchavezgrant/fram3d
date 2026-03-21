using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Shared visual constants for the Premiere-style dark UI theme.
    /// Derived from the ui-mockup CSS. All panels and components reference these
    /// instead of hardcoding color values.
    /// </summary>
    public static class Theme
    {
        // --- Colors ---
        public static readonly Color PANEL_BACKGROUND  = new(0.145f, 0.145f, 0.145f, 1f);  // #252525
        public static readonly Color HEADER_BACKGROUND = new(0.118f, 0.118f, 0.118f, 1f);  // #1e1e1e
        public static readonly Color BORDER            = new(0.235f, 0.235f, 0.235f, 1f);  // #3c3c3c
        public static readonly Color LABEL_DIM         = new(0.4f, 0.4f, 0.4f, 1f);        // #666
        public static readonly Color LABEL_MID         = new(0.6f, 0.6f, 0.6f, 1f);        // #999
        public static readonly Color LABEL_BRIGHT      = new(0.733f, 0.733f, 0.733f, 1f);  // #bbb
        public static readonly Color TEXT_DEFAULT       = new(0.75f, 0.75f, 0.75f, 1f);
        public static readonly Color TEXT_LIGHT         = new(0.8f, 0.8f, 0.8f, 1f);
        public static readonly Color TEXT_WHITE         = new(1f, 1f, 1f, 1f);
        public static readonly Color HIGHLIGHT         = new(0.2f, 0.4f, 0.7f, 0.4f);
        public static readonly Color HIGHLIGHT_STRONG  = new(0.2f, 0.4f, 0.7f, 0.6f);
        public static readonly Color DROPDOWN_BG       = new(0.16f, 0.16f, 0.16f, 1f);
        public static readonly Color SELECTOR_BG       = new(0.18f, 0.18f, 0.18f, 1f);
        public static readonly Color SELECTOR_BORDER   = new(0.3f, 0.3f, 0.3f, 1f);

        // --- Sizes ---
        public const float HEADER_HEIGHT = 28f;
        public const float FONT_SMALL    = 9f;
        public const float FONT_BODY     = 11f;
        public const float FONT_HEADER   = 10f;
        public const float INFO_LABEL_WIDTH = 80f;

        // --- Factory methods for common elements ---

        public static VisualElement CreateSeparator()
        {
            var separator = new VisualElement();
            separator.style.height          = 1;
            separator.style.backgroundColor = BORDER;
            separator.style.marginTop       = 8;
            separator.style.marginBottom    = 8;

            return separator;
        }

        public static Label CreateSectionLabel(string text)
        {
            var label = new Label(text);
            label.style.fontSize      = FONT_HEADER;
            label.style.color         = LABEL_DIM;
            label.style.letterSpacing = 1;
            label.style.marginBottom  = 4;

            return label;
        }
    }
}
