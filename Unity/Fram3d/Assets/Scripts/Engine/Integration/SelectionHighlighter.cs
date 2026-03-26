using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Applies visual highlights to hovered and selected elements by
    /// overriding their base color via MaterialPropertyBlock. This never
    /// creates material instances or modifies shared materials — the
    /// PropertyBlock is a per-renderer overlay that the shader reads
    /// instead of the material's own values. Clearing the block restores
    /// the original appearance.
    /// </summary>
    public sealed class SelectionHighlighter: MonoBehaviour
    {
        private static readonly int BASE_COLOR   = Shader.PropertyToID("_BaseColor");
        private static readonly int UNLIT_COLOR = Shader.PropertyToID("_Color");

        private static readonly Color HOVER_COLOR = new(1f,
                                                        0.92f,
                                                        0.016f,
                                                        1f);

        private static readonly Color SELECT_COLOR = new(0f,
                                                         1f,
                                                         1f,
                                                         1f);

        private ElementBehaviour _currentHovered;
        private ElementBehaviour _currentSelected;
        public  Selection        Selection { get; private set; }

        private void UpdateHighlight(ref ElementBehaviour current, ElementId targetId, Color color)
        {
            var target = FindBehaviour(targetId);

            if (target == current)
                return;

            RemoveHighlight(current);
            ApplyHighlight(target, color);
            current = target;
        }

        private static void ApplyHighlight(ElementBehaviour behaviour, Color color)
        {
            if (behaviour == null)
                return;

            var renderers = behaviour.GetComponentsInChildren<Renderer>();
            var block     = new MaterialPropertyBlock();

            foreach (var renderer in renderers)
            {
                renderer.GetPropertyBlock(block);
                block.SetColor(BASE_COLOR, color);
                block.SetColor(UNLIT_COLOR, color);
                renderer.SetPropertyBlock(block);
            }
        }

        private static ElementBehaviour FindBehaviour(ElementId id)
        {
            if (id == null)
                return null;

            var behaviours = FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var behaviour in behaviours)
            {
                if (behaviour.Element != null && behaviour.Element.Id == id)
                    return behaviour;
            }

            return null;
        }

        private static void RemoveHighlight(ElementBehaviour behaviour)
        {
            if (behaviour == null)
                return;

            var renderers = behaviour.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
                renderer.SetPropertyBlock(null);
        }

        private void Awake() => this.Selection = new Selection();

        private void LateUpdate()
        {
            if (this.Selection == null)
                return;

            this.UpdateHighlight(ref this._currentHovered,  this.Selection.HoveredId,  HOVER_COLOR);
            this.UpdateHighlight(ref this._currentSelected, this.Selection.SelectedId, SELECT_COLOR);
        }
    }
}