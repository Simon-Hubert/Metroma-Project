using UnityEngine;
using UnityEngine.Rendering;

namespace Metroma
{
    //on fout les particules dans une texture qui est lue
    public class FluidMetaball2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SPH _sph;
        [SerializeField] private Camera _cam;
        [SerializeField] private Material _accumMaterial;

        [Header("Field")]
        [Tooltip("Taille du blob de chaque particule")]
        [SerializeField] private float _blobSize = 0.35f;
        [Tooltip("Diviseur de resolution de la RT du champ (1 = plein, 2 = moitie) mais sah viens on touche pas")]
        [SerializeField] private int _downSample = 1;

        [Header("Fullscreen composite (debug / fallback)")]
        [Tooltip("Si coche, dessine le liquide par-dessus TOUTE la scene (ignore le tri). Sinon, utilise un Quad de scene.")]
        [SerializeField] private bool _fullscreenComposite = false;
        [SerializeField] private Material _compositeMaterial; // requis seulement si fullscreen

        private static readonly int FieldTex = Shader.PropertyToID("_MetaballField");
        private static readonly int ParticlesBuffer = Shader.PropertyToID("_particlesBuffer");
        private static readonly int Size = Shader.PropertyToID("_size");
        private static readonly int SimZ = Shader.PropertyToID("_simZ");

        private Mesh _quad;
        private ComputeBuffer _argsBuffer;
        private uint[] _args;
        private RenderTexture _fieldRT;
        private bool _initialized;

        public bool IsReady => _initialized && _sph != null && _sph.ParticlesBuffer != null;

        private void Begin()
        {
            if (_cam == null) _cam = GetComponent<Camera>();

            _quad = BuildQuad();

            _args = new uint[]
            {
                _quad.GetIndexCount(0),
                (uint)_sph.ActiveParticles,
                _quad.GetIndexStart(0),
                _quad.GetBaseVertex(0),
                0
            };
            _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(_args);

            _initialized = true;
        }

        private void EnsureFieldRT(int w, int h)
        {
            if (_fieldRT != null && _fieldRT.width == w && _fieldRT.height == h) return;
            if (_fieldRT != null) _fieldRT.Release();
            _fieldRT = new RenderTexture(w, h, 0, RenderTextureFormat.RHalf)
            {
                name = "MetaballField",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _fieldRT.Create();
        }

        public void RenderFluidURP(CommandBuffer cmd, RenderTargetIdentifier cameraColorTarget)
        {
            if (_sph == null || _sph.ParticlesBuffer == null || _accumMaterial == null) return;

            if (!_initialized) Begin();

            // Met a jour le nombre d'instances a dessiner
            if (_args[1] != (uint)_sph.ActiveParticles)
            {
                _args[1] = (uint)_sph.ActiveParticles;
                _argsBuffer.SetData(_args);
            }

            _accumMaterial.SetBuffer(ParticlesBuffer, _sph.ParticlesBuffer);
            _accumMaterial.SetFloat(Size, _blobSize);
            _accumMaterial.SetFloat(SimZ, _sph.SimZ);

            int ds = Mathf.Max(1, _downSample);
            int w = Mathf.Max(1, _cam.pixelWidth / ds);
            int h = Mathf.Max(1, _cam.pixelHeight / ds);

            // 1) on balance tous les blobs dans la RT
            EnsureFieldRT(w, h);
            cmd.SetRenderTarget(_fieldRT);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.DrawMeshInstancedIndirect(_quad, 0, _accumMaterial, 0, _argsBuffer, 0);

            // 2) on l'expose en global, le Quad de scene ira la lire
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
            if (_argsBuffer != null) _argsBuffer.Release();
            if (_fieldRT != null) _fieldRT.Release();
        }
    }
}
