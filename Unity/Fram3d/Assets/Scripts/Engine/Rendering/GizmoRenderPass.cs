using System.Collections.Generic;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
namespace Fram3d.Engine.Rendering
{
    /// <summary>
    /// Renders all objects on the gizmo layer after transparent geometry.
    /// Uses <c>ZTest Always</c> and <c>ZWrite Off</c> via the objects' own
    /// materials (GizmoHandle shader). This pass provides explicit timing
    /// control instead of relying on render queue ordering.
    /// Implements both the legacy Execute path and the RenderGraph path
    /// for Unity 6 compatibility.
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
                                                              1 << GizmoBehaviour.GIZMO_LAYER_INDEX);
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(SHADER_TAGS,
                                                        ref renderingData,
                                                        SortingCriteria.CommonTransparent);

            context.DrawRenderers(renderingData.cullResults,
                                  ref drawingSettings,
                                  ref this._filteringSettings);
        }

        private class PassData
        {
            public RendererListHandle RendererListHandle;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData  = frameData.Get<UniversalResourceData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData    = frameData.Get<UniversalCameraData>();
            var lightData     = frameData.Get<UniversalLightData>();

            var drawingSettings = RenderingUtils.CreateDrawingSettings(SHADER_TAGS,
                                                                       renderingData,
                                                                       cameraData,
                                                                       lightData,
                                                                       SortingCriteria.CommonTransparent);

            var listParams = new RendererListParams(renderingData.cullResults,
                                                     drawingSettings,
                                                     this._filteringSettings);

            var rendererListHandle = renderGraph.CreateRendererList(listParams);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Gizmo Render Pass", out var passData))
            {
                passData.RendererListHandle = rendererListHandle;
                builder.UseRendererList(rendererListHandle);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.RendererListHandle);
                });
            }
        }
    }
}
