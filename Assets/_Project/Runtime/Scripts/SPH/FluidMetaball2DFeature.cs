using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Metroma
{

    public class FluidMetaball2DFeature : ScriptableRendererFeature
    {
        class FluidMetaballPass : ScriptableRenderPass
        {
            class PassData
            {
                public FluidMetaball2D metaball;
                public TextureHandle cameraColorTarget;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                Camera cam = cameraData.camera;
                FluidMetaball2D metaball = cam.GetComponent<FluidMetaball2D>();

                if (metaball != null && metaball.isActiveAndEnabled)
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    TextureHandle colorHandle = resourceData.activeColorTexture;

                    using (var builder = renderGraph.AddUnsafePass<PassData>("Fluid Metaball 2D", out var passData))
                    {
                        passData.metaball = metaball;
                        passData.cameraColorTarget = colorHandle;

                        builder.UseTexture(colorHandle, AccessFlags.ReadWrite);

                        builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                        {
                            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                            data.metaball.RenderFluidURP(cmd, data.cameraColorTarget);
                        });
                    }
                }
            }

#pragma warning disable 0672
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Camera cam = renderingData.cameraData.camera;
                FluidMetaball2D metaball = cam.GetComponent<FluidMetaball2D>();

                if (metaball != null && metaball.isActiveAndEnabled)
                {
                    CommandBuffer cmd = CommandBufferPool.Get("FluidMetaball2D");
                    RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    metaball.RenderFluidURP(cmd, cameraColorTarget);
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }
            }
#pragma warning restore 0672
        }

        FluidMetaballPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new FluidMetaballPass();
            // avant les transparents, sinon le Quad de scene lit un champ pas encore pret
            m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                renderer.EnqueuePass(m_ScriptablePass);
            }
        }
    }
}
