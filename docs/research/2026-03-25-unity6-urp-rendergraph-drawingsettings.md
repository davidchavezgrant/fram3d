# Raw Research Findings: Unity 6 URP RenderGraph DrawingSettings API

## Queries Executed
1. "Unity 6 URP RenderGraph RecordRenderGraph CreateDrawingSettings RenderingUtils 2025 2026" - 3 useful results
2. "Unity 6 URP ScriptableRenderPass RecordRenderGraph RendererListParams DrawingSettings example" - 4 useful results
3. "Unity 6000 URP 17 RenderingUtils.CreateDrawingSettings UniversalRenderingData UniversalCameraData signature" - 2 useful results
4. "Unity 6 URP CreateDrawingSettings deprecated obsolete ref RenderingData vs UniversalRenderingData migration" - 2 useful results
5. "Unity 6 URP RenderingUtils.CreateRendererListWithRenderStateBlock signature RenderGraph" - 1 useful result

## Findings

### Finding 1: RenderingUtils.CreateDrawingSettings has 4 overloads -- the RenderGraph path uses the UniversalRenderingData/UniversalCameraData/UniversalLightData variant
- **Confidence**: HIGH
- **Supporting sources**:
  - [RenderingUtils API Docs (URP 17.0.3)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/api/UnityEngine.Rendering.Universal.RenderingUtils.html) - Lists all 4 overloads explicitly
  - [RendererListRenderFeature.cs (official Unity sample)](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs) - Uses `RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags)`
  - [RenderObjectsPass.cs (URP source)](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs) - Same pattern
  - [Cyanilux Tutorial](https://www.cyanilux.com/tutorials/custom-renderer-features/) - Confirms same API
- **Signatures**:
  ```csharp
  // RenderGraph path (use these):
  public static DrawingSettings CreateDrawingSettings(
      List<ShaderTagId> shaderTagIdList,
      UniversalRenderingData renderingData,
      UniversalCameraData cameraData,
      UniversalLightData lightData,
      SortingCriteria sortingCriteria)

  public static DrawingSettings CreateDrawingSettings(
      ShaderTagId shaderTagId,
      UniversalRenderingData renderingData,
      UniversalCameraData cameraData,
      UniversalLightData lightData,
      SortingCriteria sortingCriteria)

  // Legacy path (deprecated, uses old RenderingData struct):
  public static DrawingSettings CreateDrawingSettings(
      List<ShaderTagId> shaderTagIdList,
      ref RenderingData renderingData,
      SortingCriteria sortingCriteria)

  public static DrawingSettings CreateDrawingSettings(
      ShaderTagId shaderTagId,
      ref RenderingData renderingData,
      SortingCriteria sortingCriteria)
  ```

### Finding 2: The data objects are retrieved from ContextContainer frameData using Get<T>()
- **Confidence**: HIGH
- **Supporting sources**:
  - [RendererListRenderFeature.cs (official sample)](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs) - `frameData.Get<UniversalRenderingData>()`, `frameData.Get<UniversalCameraData>()`, `frameData.Get<UniversalLightData>()`
  - [Unity Manual - Draw objects in render graph](https://docs.unity.cn/6000.0/Documentation/Manual/urp/render-graph-draw-objects-in-a-pass.html) - Same pattern: `frameContext.Get<UniversalRenderingData>()`
  - [RenderObjectsPass.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs) - Same pattern
  - [Cyanilux Tutorial](https://www.cyanilux.com/tutorials/custom-renderer-features/) - Same pattern

### Finding 3: RendererListParams takes (cullResults, drawingSettings, filteringSettings)
- **Confidence**: HIGH
- **Supporting sources**:
  - [RendererListRenderFeature.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs) - `new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings)`
  - [Unity Manual](https://docs.unity.cn/6000.0/Documentation/Manual/urp/render-graph-draw-objects-in-a-pass.html) - `new RendererListParams(renderingData.cullResults, drawSettings, filterSettings)`
  - [Cyanilux Tutorial](https://www.cyanilux.com/tutorials/custom-renderer-features/) - Same pattern
- **Notes**: `cullResults` comes from `UniversalRenderingData.cullResults`, NOT from `RenderingData`

### Finding 4: Layer filtering is done via FilteringSettings, not DrawingSettings
- **Confidence**: HIGH
- **Supporting sources**:
  - [RendererListRenderFeature.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs) - `new FilteringSettings(renderQueueRange, m_LayerMask)`
  - [RenderObjectsPass.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs) - `new FilteringSettings(renderQueueRange, layerMask)`
- **Notes**: FilteringSettings constructor takes (RenderQueueRange, int layerMask). Use `RenderQueueRange.opaque` or `RenderQueueRange.transparent`. Use `~0` for all layers.

### Finding 5: The complete RecordRenderGraph pattern for drawing objects on specific layers
- **Confidence**: HIGH
- **Supporting sources**: All 4 main sources agree on this pattern
- **Complete working pattern**:
  ```csharp
  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
      using (var builder = renderGraph.AddRasterRenderPass<PassData>("PassName", out var passData))
      {
          // 1. Get frame data
          var resourceData  = frameData.Get<UniversalResourceData>();
          var renderingData = frameData.Get<UniversalRenderingData>();
          var cameraData    = frameData.Get<UniversalCameraData>();
          var lightData     = frameData.Get<UniversalLightData>();

          // 2. Create drawing settings
          var sortFlags    = cameraData.defaultOpaqueSortFlags;
          var drawSettings = RenderingUtils.CreateDrawingSettings(
              shaderTagIdList, renderingData, cameraData, lightData, sortFlags);

          // 3. Create filtering settings (layer mask here)
          var filterSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);

          // 4. Create renderer list
          var param = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
          passData.rendererListHandle = renderGraph.CreateRendererList(param);

          if (!passData.rendererListHandle.IsValid())
              return;

          // 5. Configure pass
          builder.UseRendererList(passData.rendererListHandle);
          builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
          builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

          // 6. Set render function
          builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
          {
              context.cmd.DrawRendererList(data.rendererListHandle);
          });
      }
  }
  ```

### Finding 6: Standard shader tag IDs for URP forward rendering
- **Confidence**: HIGH
- **Supporting sources**:
  - [RendererListRenderFeature.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs) - Lists 4 tags
  - [Unity Manual](https://docs.unity.cn/6000.0/Documentation/Manual/urp/render-graph-draw-objects-in-a-pass.html) - Uses "UniversalForward"
- **Standard tags**:
  ```csharp
  new ShaderTagId("UniversalForwardOnly"),
  new ShaderTagId("UniversalForward"),
  new ShaderTagId("SRPDefaultUnlit"),
  new ShaderTagId("LightweightForward")  // legacy compat
  ```

### Finding 7: RenderingUtils.CreateRendererListWithRenderStateBlock exists for applying custom render state
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [RenderObjectsPass.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs) - Uses `RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawingSettings, m_FilteringSettings, m_RenderStateBlock, ref passData.rendererListHdl)`
- **Notes**: This variant takes a `RenderStateBlock` for overriding depth/stencil/blend state. The simpler `new RendererListParams()` + `renderGraph.CreateRendererList()` approach works when you don't need state overrides.

### Finding 8: DrawingSettings supports overrideMaterial for custom material rendering
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Manual](https://docs.unity.cn/6000.0/Documentation/Manual/urp/render-graph-draw-objects-in-a-pass.html) - `drawSettings.overrideMaterial = materialToUse`
  - [RenderObjectsPass.cs](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs) - `drawingSettings.overrideMaterial = overrideMaterial; drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex`

### Finding 9: The old ref RenderingData overloads are for compatibility mode (legacy) only
- **Confidence**: HIGH
- **Supporting sources**:
  - [Upgrade Guide URP 17](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/upgrade-guide-unity-6.html) - "Custom passes require complete rewriting using the render graph API. The legacy rendering API is no longer developed or supported."
  - [RenderingUtils API Docs](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/api/UnityEngine.Rendering.Universal.RenderingUtils.html) - Both legacy (ref RenderingData) and modern (Universal*Data) overloads listed
- **Notes**: The `ref RenderingData` overloads work in the old `Execute()` path. The `UniversalRenderingData` overloads are for the `RecordRenderGraph()` path.

## Contradictions
- None found. All sources are consistent on the API signatures and usage patterns.

## Source Registry
| # | Title | URL | Date | Queries that surfaced it |
|---|-------|-----|------|--------------------------|
| 1 | RenderingUtils API Docs (URP 17.0.3) | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/api/UnityEngine.Rendering.Universal.RenderingUtils.html | N/A | Q3 |
| 2 | RendererListRenderFeature.cs (official sample) | https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Samples~/URPRenderGraphSamples/RendererList/RendererListRenderFeature.cs | N/A | Q1, Q2, Q3 |
| 3 | RenderObjectsPass.cs (URP source) | https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs | N/A | Q2, Q3, Q5 |
| 4 | Unity Manual - Draw objects in render graph | https://docs.unity.cn/6000.0/Documentation/Manual/urp/render-graph-draw-objects-in-a-pass.html | N/A | Q2 |
| 5 | Cyanilux - Custom Renderer Features | https://www.cyanilux.com/tutorials/custom-renderer-features/ | N/A | Q1, Q2 |
| 6 | Upgrade Guide URP 17 | https://docs.unity3d.com/6000.3/Documentation/Manual/urp/upgrade-guide-unity-6.html | N/A | Q3, Q4 |
