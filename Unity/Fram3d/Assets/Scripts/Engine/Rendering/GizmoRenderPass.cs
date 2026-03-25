using System.Collections.Generic;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Fram3d.Engine.Rendering
{
    /// <summary>
    /// Renders all objects on the gizmo layer after transparent geometry.
    /// Uses <c>ZTest Always</c> and <c>ZWrite Off</c> via the objects' own
    /// materials (GizmoHandle shader). This pass provides explicit timing
    /// control instead of relying on render queue ordering.
    /// </summary>
    internal sealed class GizmoRenderPass: ScriptableRenderPass
    {
        private static readonly List<ShaderTagId> SHADER_TAGS = new()
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("SRPDefaultUnlit")
        };

        private FilteringSettings _filteringSettings;

        public GizmoRenderPass()
        {
            this.renderPassEvent      = RenderPassEvent.AfterRenderingTransparents;
            this._filteringSettings   = new FilteringSettings(RenderQueueRange.all,
                                                              1 << GizmoController.GIZMO_LAYER_INDEX);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(SHADER_TAGS,
                                                        ref renderingData,
                                                        SortingCriteria.CommonTransparent);

            context.DrawRenderers(renderingData.cullResults,
                                  ref drawingSettings,
                                  ref this._filteringSettings);
        }
    }
}
