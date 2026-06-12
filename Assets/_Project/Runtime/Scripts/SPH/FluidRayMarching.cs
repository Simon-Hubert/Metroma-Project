using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class FluidRayMarching : MonoBehaviour
    {
        public ComputeShader raymarching;
        public Camera cam;

        List<ComputeBuffer> buffersToDispose = new List<ComputeBuffer>();

        public SPH sph;

        RenderTexture target;

        [Header("Params")] public float viewRadius;
        public float blendStrength;
        public Color waterColor;

        public Color ambientLight;

        public Light lightSource;


        void InitRenderTexture()
        {
            if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
            {
                if (target != null)
                {
                    target.Release();
                }

                cam.depthTextureMode = DepthTextureMode.Depth;

                target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat,
                    RenderTextureReadWrite.Linear);
                target.enableRandomWrite = true;
                target.Create();
            }
        }


        private bool render = false;

        public ComputeBuffer _particlesBuffer;

        private void SpawnParticlesInBox()
        {
            _particlesBuffer = new ComputeBuffer(1, 44);
            _particlesBuffer.SetData(new Particle[]
            {
                new Particle
                {
                    Position = new Vector3(0, 0, 0)
                }
            });

        }

        public void Begin()
        {
            // SpawnParticlesInBox();
            InitRenderTexture();
            raymarching.SetBuffer(0, "particles", sph.ParticlesBuffer);
            raymarching.SetInt("numParticles", sph.TotalParticles);
            raymarching.SetFloat("particleRadius", viewRadius);
            raymarching.SetFloat("blendStrength", blendStrength);
            raymarching.SetVector("waterColor", waterColor);
            raymarching.SetVector("_AmbientLight", ambientLight);
            raymarching.SetTextureFromGlobal(0, "_DepthTexture", "_CameraDepthTexture");
            render = true;
        }

        public void RenderFluidURP(UnityEngine.Rendering.CommandBuffer cmd, UnityEngine.Rendering.RenderTargetIdentifier cameraColorTarget)
        {
            if (sph == null || sph.ParticlesBuffer == null) return;

            if (!render)
            {
                Begin();
            }

            if (render)
            {
                cmd.SetComputeVectorParam(raymarching, "_Light", lightSource.transform.forward);

                cmd.SetComputeTextureParam(raymarching, 0, "Source", cameraColorTarget);
                cmd.SetComputeTextureParam(raymarching, 0, "Destination", target);
                cmd.SetComputeVectorParam(raymarching, "_CameraPos", cam.transform.position);
                cmd.SetComputeMatrixParam(raymarching, "_CameraToWorld", cam.cameraToWorldMatrix);
                cmd.SetComputeMatrixParam(raymarching, "_CameraInverseProjection", cam.projectionMatrix.inverse);

                int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
                int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
                cmd.DispatchCompute(raymarching, 0, threadGroupsX, threadGroupsY, 1);

                cmd.Blit(target, cameraColorTarget);
            }
        }

    }
}


