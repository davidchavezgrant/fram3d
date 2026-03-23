using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Shared visual constants and factory methods for the dark UI theme.
    /// Static styling is defined in fram3d.uss; this class provides the
    /// runtime colors needed for hover/highlight callbacks and the factory
    /// methods that create elements with the right USS classes.
    /// </summary>
    public static class Theme
    {
        // --- Runtime colors (used in hover/highlight callbacks, not USS) ---
        public static readonly Color HIGHLIGHT        = new(0.2f, 0.4f, 0.7f, 0.4f);
        public static readonly Color HIGHLIGHT_STRONG = new(0.2f, 0.4f, 0.7f, 0.6f);
        public static readonly Color TEXT_DEFAULT     = new(0.75f, 0.75f, 0.75f, 1f);
        public static readonly Color TEXT_WHITE       = new(1f, 1f, 1f, 1f);

        public static Label CreateSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("section-label");
            return label;
        }

        public static VisualElement CreateSeparator()
        {
            var separator = new VisualElement();
            separator.AddToClassList("section-separator");
            return separator;
        }
    }
}
