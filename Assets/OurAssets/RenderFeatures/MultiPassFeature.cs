using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class MultiPassFeature : ScriptableRendererFeature
{
    [SerializeField]
    List<string> m_Passes;

    MultiPassPass m_MainPass;

    public override void Create()
    {
        m_MainPass = new MultiPassPass(m_Passes);
        m_MainPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => renderer.EnqueuePass(m_MainPass);

    class MultiPassPass : ScriptableRenderPass
    {
        readonly List<ShaderTagId> m_Tags;

        public MultiPassPass(List<string> tags)
        {
            if (tags == null || tags.Count == 0) return;
            m_Tags = new List<ShaderTagId>();
            foreach (string tag in tags)
                m_Tags.Add(new ShaderTagId(tag));
        }

        private class PassData
        {
            public RendererListHandle rendererListHandle;
        }

        static void ExecutePass(PassData data, RasterGraphContext context) => context.cmd.DrawRendererList(data.rendererListHandle);

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Tags == null || m_Tags.Count == 0) return;
            foreach (ShaderTagId tag in m_Tags)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(tag.name, out var passData))
                {
                    UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    UniversalLightData lightData = frameData.Get<UniversalLightData>();
                    SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
                    RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
                    FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, ~0);
                    DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(tag, renderingData, cameraData, lightData, sortFlags);
                    RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
                    passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    builder.UseRendererList(passData.rendererListHandle);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context)); builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }
        }
    }
}
