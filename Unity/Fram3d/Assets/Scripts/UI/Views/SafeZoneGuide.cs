using Fram3d.Core.Viewports;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Safe zone overlay — title safe and action safe bordered rectangles
    /// inset from the frame edges by configurable percentages.
    /// </summary>
    internal sealed class SafeZoneGuide : VisualElement
    {
        private readonly VisualElement _actionSafe;
        private readonly VisualElement _titleSafe;

        public SafeZoneGuide()
        {
            this.pickingMode = PickingMode.Ignore;
            this._titleSafe  = CreateZone("guide-safe-zone--title");
            this._actionSafe = CreateZone("guide-safe-zone--action");
            this.Add(this._titleSafe);
            this.Add(this._actionSafe);
        }

        public void Update(UnmaskedRect rect, bool visible,
                           float titleSafePercent, float actionSafePercent)
        {
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            this._titleSafe.style.display  = display;
            this._actionSafe.style.display = display;

            if (!visible)
            {
                return;
            }

            PositionZone(this._titleSafe,  rect, titleSafePercent);
            PositionZone(this._actionSafe, rect, actionSafePercent);
        }

        private static VisualElement CreateZone(string cssClass)
        {
            var zone = new VisualElement();
            zone.pickingMode = PickingMode.Ignore;
            zone.AddToClassList("guide-safe-zone");
            zone.AddToClassList(cssClass);
            return zone;
        }

        private static void PositionZone(VisualElement zone, UnmaskedRect rect, float percent)
        {
            var insetX = rect.Width  * (1f - percent) / 2f;
            var insetY = rect.Height * (1f - percent) / 2f;
            zone.style.left   = rect.X      + insetX;
            zone.style.top    = rect.Y      + insetY;
            zone.style.width  = rect.Width  - insetX * 2f;
            zone.style.height = rect.Height - insetY * 2f;
        }
    }
}
