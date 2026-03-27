using Fram3d.Core.Viewports;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Rule of thirds overlay — two horizontal and two vertical guide lines
    /// dividing the frame into a 3x3 grid.
    /// </summary>
    internal sealed class ThirdsGuide : VisualElement
    {
        private const float LINE_WIDTH = 1f;

        private readonly VisualElement _h1;
        private readonly VisualElement _h2;
        private readonly VisualElement _v1;
        private readonly VisualElement _v2;

        public ThirdsGuide()
        {
            this.pickingMode = PickingMode.Ignore;
            this._h1 = CreateLine();
            this._h2 = CreateLine();
            this._v1 = CreateLine();
            this._v2 = CreateLine();
            this.Add(this._h1);
            this.Add(this._h2);
            this.Add(this._v1);
            this.Add(this._v2);
        }

        public void Update(UnmaskedRect rect, bool visible)
        {
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            this._h1.style.display = display;
            this._h2.style.display = display;
            this._v1.style.display = display;
            this._v2.style.display = display;

            if (!visible)
            {
                return;
            }

            var thirdW = rect.Width  / 3f;
            var thirdH = rect.Height / 3f;

            this._h1.style.left   = rect.X;
            this._h1.style.top    = rect.Y + thirdH;
            this._h1.style.width  = rect.Width;
            this._h1.style.height = LINE_WIDTH;

            this._h2.style.left   = rect.X;
            this._h2.style.top    = rect.Y + thirdH * 2f;
            this._h2.style.width  = rect.Width;
            this._h2.style.height = LINE_WIDTH;

            this._v1.style.left   = rect.X + thirdW;
            this._v1.style.top    = rect.Y;
            this._v1.style.width  = LINE_WIDTH;
            this._v1.style.height = rect.Height;

            this._v2.style.left   = rect.X + thirdW * 2f;
            this._v2.style.top    = rect.Y;
            this._v2.style.width  = LINE_WIDTH;
            this._v2.style.height = rect.Height;
        }

        private static VisualElement CreateLine()
        {
            var line = new VisualElement();
            line.pickingMode = PickingMode.Ignore;
            line.AddToClassList("guide-line");
            line.AddToClassList("guide-line--thirds");
            return line;
        }
    }
}
