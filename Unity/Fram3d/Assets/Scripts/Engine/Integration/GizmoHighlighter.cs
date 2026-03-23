using System.Collections.Generic;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages hover and drag color highlighting on gizmo handles.
    /// Stores the per-handle axis color registry so it can restore
    /// the original color after a highlight is cleared.
    /// </summary>
    internal sealed class GizmoHighlighter
    {
        private static readonly Color DRAG_COLOR = new(0f,
                                                       1f,
                                                       1f,
                                                       1f);

        private static readonly Color HOVER_COLOR = new(1f,
                                                        0.92f,
                                                        0.016f,
                                                        1f);

        public static readonly int                         SHADER_COLOR = Shader.PropertyToID("_Color");
        private readonly       Dictionary<Renderer, Color> _axisColors;
        private                Renderer                    _draggedRenderer;
        private                Renderer                    _hoveredRenderer;

        public GizmoHighlighter(Dictionary<Renderer, Color> axisColors)
        {
            this._axisColors = axisColors;
        }

        public bool IsHoveringHandle => this._hoveredRenderer != null;

        public void ClearDrag()
        {
            if (this._draggedRenderer == null)
            {
                return;
            }

            this.RestoreAxisColor(this._draggedRenderer);
            this._draggedRenderer = null;
        }

        public void ClearHover()
        {
            if (this._hoveredRenderer == null)
            {
                return;
            }

            this.RestoreAxisColor(this._hoveredRenderer);
            this._hoveredRenderer = null;
        }

        public void SetDrag(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            // Clear hover state — drag takes over
            if (renderer == this._hoveredRenderer)
            {
                this._hoveredRenderer = null;
            }

            this._draggedRenderer = renderer;
            renderer.material.SetColor(SHADER_COLOR, DRAG_COLOR);
        }

        public void SetHover(Renderer renderer)
        {
            if (renderer == this._hoveredRenderer)
            {
                return;
            }

            this.ClearHover();
            this._hoveredRenderer = renderer;
            renderer.material.SetColor(SHADER_COLOR, HOVER_COLOR);
        }

        private void RestoreAxisColor(Renderer renderer)
        {
            if (this._axisColors.TryGetValue(renderer, out var axisColor))
            {
                renderer.material.SetColor(SHADER_COLOR, axisColor);
            }
        }
    }
}