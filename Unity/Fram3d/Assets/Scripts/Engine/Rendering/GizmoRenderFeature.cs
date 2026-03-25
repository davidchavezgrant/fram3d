using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Fram3d.Engine.Rendering
{
    /// <summary>
    /// URP Renderer Feature that draws gizmo layer objects after all transparent
    /// geometry. More deterministic than relying on render queue ordering —
    /// ensures gizmos render after transparents but before post-processing,
    /// regardless of material queue values.
    ///
    /// To enable: add this feature to the URP Renderer Asset (PC_Renderer)
    /// in the Unity Inspector.
    /// </summary>
    public sealed class GizmoRenderFeature: ScriptableRendererFeature
    {
        private GizmoRenderPass _pass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            renderer.EnqueuePass(this._pass);
        }

        public override void Create()
        {
            this._pass = new GizmoRenderPass();
        }
    }
}
