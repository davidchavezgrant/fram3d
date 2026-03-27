using Fram3d.Core.Viewports;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Center cross overlay — horizontal and vertical lines crossing at the
    /// center of the frame.
    /// </summary>
    internal sealed class CenterCrossGuide : VisualElement
    {
        private const float ARM   = 20f;
        private const float WIDTH = 1.5f;

        private readonly VisualElement _h;
        private readonly VisualElement _v;

        public CenterCrossGuide()
        {
            this.pickingMode = PickingMode.Ignore;
            this._h = CreateLine();
            this._v = CreateLine();
            this.Add(this._h);
            this.Add(this._v);
        }

        public void Update(UnmaskedRect rect, bool visible)
        {
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            this._h.style.display = display;
            this._v.style.display = display;

            if (!visible)
            {
                return;
            }

            var cx = rect.X + rect.Width  / 2f;
            var cy = rect.Y + rect.Height / 2f;

            this._h.style.left   = cx - ARM;
            this._h.style.top    = cy - WIDTH / 2f;
            this._h.style.width  = ARM * 2f;
            this._h.style.height = WIDTH;

            this._v.style.left   = cx - WIDTH / 2f;
            this._v.style.top    = cy - ARM;
            this._v.style.width  = WIDTH;
            this._v.style.height = ARM * 2f;
        }

        private static VisualElement CreateLine()
        {
            var line = new VisualElement();
            line.pickingMode = PickingMode.Ignore;
            line.AddToClassList("guide-line");
            line.AddToClassList("guide-line--center");
            return line;
        }
    }
}
