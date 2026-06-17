using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Metroma
{
    //on fout les particules dans une texture qui est lue
    public class FluidMetaball2D : MonoBehaviour
    {
        // Un verre = un SPH + sa couleur de liquide
        [Serializable]
        public class GlassFluid
        {
            public SPH sph;
            public Color color = new Color(0.2f, 0.6f, 1f, 1f);
        }

        [Header("References")]
        [Tooltip("Un element par verre : son SPH + sa couleur.")]
        [SerializeField] private GlassFluid[] _glasses;
        [SerializeField] private Camera _cam;
        [SerializeField] private Material _accumMaterial;

        [Header("Field")]
        [Tooltip("Taille du blob de chaque particule")]
        [SerializeField] private float _blobSize = 0.35f;
        [Tooltip("Diviseur de resolution de la RT du champ (1 = plein, 2 = moitie) mais sah viens on touche pas")]
        [SerializeField] private int _downSample = 1;

        [Header("Fullscreen composite (debug / fallback)")]
        [Tooltip("Si coche, dessine le liquide par-dessus TOUTE la scene (ignore le tri).")]
        [SerializeField] private bool _fullscreenComposite = false;
        [SerializeField] private Material _compositeMaterial; // requis seulement si fullscreen

        private static readonly int FieldTex = Shader.PropertyToID("_MetaballField");
        private static readonly int ParticlesBuffer = Shader.PropertyToID("_particlesBuffer");
        private static readonly int Size = Shader.PropertyToID("_size");
        private static readonly int SimZ = Shader.PropertyToID("_simZ");
        private static readonly int BlobColor = Shader.PropertyToID("_blobColor");

        private Mesh _quad;
        private RenderTexture _fieldRT;

        // une copie de materiau + un args buffer par verre (sinon les draws s'ecrasent)
        private Material[] _accumMats;
        private ComputeBuffer[] _argsBuffers;
        private uint[][] _args;
        private bool _initialized;

        private void Begin()
        {
            if (_cam == null) _cam = GetComponent<Camera>();

            _quad = BuildQuad();

            int n = _glasses.Length;
            _accumMats = new Material[n];
            _argsBuffers = new ComputeBuffer[n];
            _args = new uint[n][];

            for (int i = 0; i < n; i++)
            {
                _accumMats[i] = new Material(_accumMaterial); // copie perso par verre
                int count = _glasses[i].sph != null ? _glasses[i].sph.ActiveParticles : 0;
                _args[i] = new uint[]
                {
                    _quad.GetIndexCount(0),
                    (uint)count,
                    _quad.GetIndexStart(0),
                    _quad.GetBaseVertex(0),
                    0
                };
                _argsBuffers[i] = new ComputeBuffer(1, _args[i].Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                _argsBuffers[i].SetData(_args[i]);
            }

            _initialized = true;
        }

        private void EnsureFieldRT(int w, int h)
        {
            if (_fieldRT != null && _fieldRT.width == w && _fieldRT.height == h) return;
            if (_fieldRT != null) _fieldRT.Release();
            _fieldRT = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf) // stocke couleur*poids dans RGB et le poids (densite) dans A
            {
                name = "MetaballField",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _fieldRT.Create();
        }

        public void RenderFluidURP(CommandBuffer cmd, RenderTargetIdentifier cameraColorTarget)
        {
            if (_glasses == null || _glasses.Length == 0 || _accumMaterial == null) return;

            if (!_initialized) Begin();

            int ds = Mathf.Max(1, _downSample);
            int w = Mathf.Max(1, _cam.pixelWidth / ds);
            int h = Mathf.Max(1, _cam.pixelHeight / ds);

            // 1) on balance tous les blobs dans la RT
            EnsureFieldRT(w, h);
            cmd.SetRenderTarget(_fieldRT);
            cmd.ClearRenderTarget(false, true, Color.clear);

            for (int i = 0; i < _glasses.Length; i++)
            {
                SPH s = _glasses[i].sph;
                if (s == null || s.ParticlesBuffer == null) continue;

                // chaque verre a son buffer, son Z et sa couleur
                _accumMats[i].SetBuffer(ParticlesBuffer, s.ParticlesBuffer);
                _accumMats[i].SetFloat(Size, _blobSize);
                _accumMats[i].SetFloat(SimZ, s.SimZ);
                _accumMats[i].SetColor(BlobColor, _glasses[i].color);

                if (_args[i][1] != (uint)s.ActiveParticles)
                {
                    _args[i][1] = (uint)s.ActiveParticles;
                    _argsBuffers[i].SetData(_args[i]);
                }

                cmd.DrawMeshInstancedIndirect(_quad, 0, _accumMats[i], 0, _argsBuffers[i], 0);
            }

            // le Quad de scene de chaque verre ira lire ce champ global
            cmd.SetGlobalTexture(FieldTex, _fieldRT);

            // debug only : composite par-dessus toute la scene
            if (_fullscreenComposite && _compositeMaterial != null)
                cmd.Blit(_fieldRT, cameraColorTarget, _compositeMaterial, 0);
        }

        private static Mesh BuildQuad()
        {
            Mesh mesh = new Mesh { name = "Metaball2D Quad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3( 0.5f, -0.5f, 0f),
                new Vector3( 0.5f,  0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f),
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            if (_argsBuffers != null)
                foreach (var b in _argsBuffers) b?.Release();
            if (_accumMats != null)
                foreach (var m in _accumMats) if (m != null) Destroy(m);
            if (_fieldRT != null) _fieldRT.Release();
        }
    }
}
