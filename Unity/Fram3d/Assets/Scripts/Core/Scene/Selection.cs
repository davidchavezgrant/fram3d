using Fram3d.Core.Common;
namespace Fram3d.Core.Scene
{
    public sealed class Selection
    {
        public ElementId HoveredId    { get; private set; }
        public ElementId SelectedId   { get; private set; }
        public void      ClearHover() => this.HoveredId = null;
        public void      Deselect()   => this.SelectedId = null;

        public void Hover(ElementId id)
        {
            if (id == null)
            {
                this.HoveredId = null;
                return;
            }

            // Hover does not show on the selected element
            if (id == this.SelectedId)
            {
                return;
            }

            this.HoveredId = id;
        }

        public void Select(ElementId id)
        {
            this.SelectedId = id;

            // Clear hover if the newly selected element was hovered
            if (this.HoveredId == id)
            {
                this.HoveredId = null;
            }
        }
    }
}