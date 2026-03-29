using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A circular keyframe marker on the timeline. The outer element is a
    /// 22px invisible hit area; the inner dot is the visible 10px circle.
    /// </summary>
    public sealed class KeyframeDiamond : VisualElement
    {
        private readonly VisualElement _dot;

        public KeyframeDiamond()
        {
            this.AddToClassList("keyframe-diamond");
            this.pickingMode = PickingMode.Position;

            this._dot = new VisualElement();
            this._dot.AddToClassList("keyframe-diamond__dot");
            this._dot.pickingMode = PickingMode.Ignore;
            this.Add(this._dot);
        }

        public void SetColor(Color color) =>
            this._dot.style.backgroundColor = color;

        public void SetSelected(bool selected) =>
            this.EnableInClassList("keyframe-diamond--selected", selected);
    }
}

