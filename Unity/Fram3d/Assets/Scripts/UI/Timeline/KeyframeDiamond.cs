using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A diamond-shaped keyframe marker on the timeline.
    /// Rendered as a rotated square via USS transform.
    /// </summary>
    public sealed class KeyframeDiamond : VisualElement
    {
        public KeyframeDiamond()
        {
            this.AddToClassList("keyframe-diamond");
            this.pickingMode = PickingMode.Position;
        }

        public void SetColor(bool isCamera) =>
            this.EnableInClassList("keyframe-diamond--element", !isCamera);

        public void SetSelected(bool selected) =>
            this.EnableInClassList("keyframe-diamond--selected", selected);
    }
}
