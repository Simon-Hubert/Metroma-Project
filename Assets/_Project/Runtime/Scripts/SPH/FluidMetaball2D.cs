using UnityEngine;
using UnityEngine.Rendering;

namespace Metroma
{
    //   1) Accumulation : chaque particule, blob additif dans une RT (champ de densite).
    //   2) Composite    : passe plein-ecran qui seuille le champ
    public class FluidMetaball2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SPH _sph;
        [SerializeField] private Camera _cam;
        [SerializeField] private Material _accumMaterial;    
        [SerializeField] private Material _compositeMaterial;

        [Header("Field")]
        [Tooltip("Taille du blob de chaque particule (unites monde).")]
        [SerializeField] private float _blobSize = 0.35f;
        [Tooltip("Diviseur de resolution de la RT du champ (1 = plein, 2 = moitie).")]
        [SerializeField] private int _downSample = 1;

        [Header("Style")]
        [SerializeField] private Color _liquidColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _rimColor = new Color(0.7f, 0.9f, 1f, 1f);
        [Range(0f, 4f)] [SerializeField] private float _threshold = 0.6f;
        [Range(0.001f, 1f)] [SerializeField] private float _edgeSoftness = 0.08f;
        [Range(0.001f, 1f)] [SerializeField] private float _rimWidth = 0.15f;

        [Header("Debug")]
        [Tooltip("0=liquide normal, 1=champ de densite brut (gris), 2=magenta plein ecran (test pipeline)")]
        [Range(0, 2)] [SerializeField] private int _debugMode = 0;

        private static readonly int FieldTex = Shader.PropertyToID("_MetaballField");
        private static readonly int ParticlesBuffer = Shader.PropertyToID("_particlesBuffer");
        private static readonly int Size = Shader.PropertyToID("_size");
        private static readonly int LiquidColor = Shader.PropertyToID("_LiquidColor");
        private static readonly int RimColor = Shader.PropertyToID("_RimColor");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int EdgeSoftness = Shader.PropertyToID("_EdgeSoftness");
        private static readonly int RimWidth = Shader.PropertyToID("_RimWidth");
        private static readonly int DebugMode = Shader.PropertyToID("_DebugMode");

        private Mesh _quad;
        private ComputeBuffer _argsBuffer;
        private bool _initialized;

        public bool IsReady => _initialized && _sph != null && _sph.ParticlesBuffer != null;

        private void Begin()
        {
            if (_cam == null) _cam = GetComponent<Camera>();

            _quad = BuildQuad();

            uint[] args =
            {
                _quad.GetIndexCount(0),
                (uint)_sph.TotalParticles,
                _quad.GetIndexStart(0),
                _quad.GetBaseVertex(0),
                0
            };
            _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);

            _initialized = true;
        }
        
        public void RenderFluidURP(CommandBuffer cmd, RenderTargetIdentifier cameraColorTarget) //c urp
        {
            if (_sph == null || _sph.ParticlesBuffer == null) return;
            if (_accumMaterial == null || _compositeMaterial == null) return;

            if (!_initialized) Begin();

            // Parametres materiaux
            _accumMaterial.SetBuffer(ParticlesBuffer, _sph.ParticlesBuffer);
            _accumMaterial.SetFloat(Size, _blobSize);

            _compositeMaterial.SetColor(LiquidColor, _liquidColor);
            _compositeMaterial.SetColor(RimColor, _rimColor);
            _compositeMaterial.SetFloat(Threshold, _threshold);
            _compositeMaterial.SetFloat(EdgeSoftness, _edgeSoftness);
            _compositeMaterial.SetFloat(RimWidth, _rimWidth);
            _compositeMaterial.SetInt(DebugMode, _debugMode);

            int ds = Mathf.Max(1, _downSample);
            int w = Mathf.Max(1, _cam.pixelWidth / ds);
            int h = Mathf.Max(1, _cam.pixelHeight / ds);

            //Accumulation du champ de densite dans une RT
            cmd.GetTemporaryRT(FieldTex, w, h, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            cmd.SetRenderTarget(FieldTex);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.DrawMeshInstancedIndirect(_quad, 0, _accumMaterial, 0, _argsBuffer, 0);

            //Composite seuille par-dessus la couleur camera
            cmd.Blit(FieldTex, cameraColorTarget, _compositeMaterial, 0);
            cmd.ReleaseTemporaryRT(FieldTex);
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
        }
    }
}
