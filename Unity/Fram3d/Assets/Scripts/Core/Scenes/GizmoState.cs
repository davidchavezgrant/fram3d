using System.Numerics;
using Fram3d.Core.Common;
namespace Fram3d.Core.Scenes
{
    /// <summary>
    /// Pure state management for the transform gizmo: active tool,
    /// selection tracking, and tool-property resets. No Unity dependencies.
    /// </summary>
    public sealed class GizmoState
    {
        private ElementId  _lastSelectedId;
        public  ActiveTool ActiveTool { get; private set; } = ActiveTool.TRANSLATE;

        /// <summary>
        /// Called each frame with the current selection. Returns true if
        /// the selection changed and the tool was reset to Translate.
        /// </summary>
        public bool OnSelectionChanged(ElementId currentId)
        {
            if (currentId == this._lastSelectedId)
            {
                return false;
            }

            this._lastSelectedId = currentId;
            this.ActiveTool      = ActiveTool.TRANSLATE;
            return true;
        }

        public void SetActiveTool(ActiveTool tool) => this.ActiveTool = tool;

        /// <summary>
        /// Resets the selected element's transform property for the active
        /// tool. Returns true if a reset was performed.
        /// </summary>
        public bool TryResetActiveTool(Element element)
        {
            if (element == null)
            {
                return false;
            }

            if (this.ActiveTool == ActiveTool.TRANSLATE)
            {
                element.Position = Vector3.Zero;
                return true;
            }

            if (this.ActiveTool == ActiveTool.ROTATE)
            {
                element.Rotation = Quaternion.Identity;
                return true;
            }

            if (this.ActiveTool == ActiveTool.SCALE)
            {
                element.Scale = 1f;
                return true;
            }

            return false;
        }
    }
}