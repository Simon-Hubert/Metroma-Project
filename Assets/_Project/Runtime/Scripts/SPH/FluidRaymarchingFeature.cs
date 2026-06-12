using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Metroma
{
    public class FluidRaymarchingFeature : ScriptableRendererFeature
    {
        class FluidRaymarchingPass : ScriptableRenderPass
        {
            class PassData
            {
                public FluidRayMarching fluidRaymarching;
                public TextureHandle cameraColorTarget;
            }

        
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                Camera cam = cameraData.camera;
                FluidRayMarching fluidRaymarching = cam.GetComponent<FluidRayMarching>();

                if (fluidRaymarching != null && fluidRaymarching.isActiveAndEnabled)
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    TextureHandle colorHandle = resourceData.activeColorTexture;

                    using (var builder = renderGraph.AddUnsafePass<PassData>("Fluid Raymarching", out var passData))
                    {
                        passData.fluidRaymarching = fluidRaymarching;
                        passData.cameraColorTarget = colorHandle;

                        builder.UseTexture(colorHandle, AccessFlags.ReadWrite);
                        
                        builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                        {
                            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                            data.fluidRaymarching.RenderFluidURP(cmd, data.cameraColorTarget);
                        });
                    }
                }
            }


#pragma warning disable 0672
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Camera cam = renderingData.cameraData.camera;
                FluidRayMarching fluidRaymarching = cam.GetComponent<FluidRayMarching>();

                if (fluidRaymarching != null && fluidRaymarching.isActiveAndEnabled)
                {
                    CommandBuffer cmd = CommandBufferPool.Get("FluidRaymarching");
                    RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    fluidRaymarching.RenderFluidURP(cmd, cameraColorTarget);
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }
            }
#pragma warning restore 0672
        }

        FluidRaymarchingPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new FluidRaymarchingPass();
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
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
