using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A circular keyframe marker on the timeline.
    /// </summary>
    public sealed class KeyframeDiamond : VisualElement
    {
        public KeyframeDiamond()
        {
            this.AddToClassList("keyframe-diamond");
            this.pickingMode = PickingMode.Position;
        }

        public void SetColor(Color color) =>
            this.style.backgroundColor = color;

        public void SetSelected(bool selected) =>
            this.EnableInClassList("keyframe-diamond--selected", selected);
    }
}

