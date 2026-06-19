using System;
using UnityEngine;

namespace Metroma
{
    //alimente le shaader avec les monobehaviour dans l"aeza
    [DisallowMultipleComponent]
    public class WaterInteraction : MonoBehaviour
    {
        [Serializable]
        public struct WakeSource
        {
            public Transform transform;
            [Tooltip("Portée des anneaux autour de cet objet, en unités monde.")]
            public float radius;
        }

        [Header("Pool area (world space, XY plane)")]
        [Tooltip("Doit couvrir toute l'eau. Visible avec le gizmo cyan.")]
        [SerializeField] private Vector2 _areaCenter = Vector2.zero;
        [SerializeField] private Vector2 _areaSize = new Vector2(40f, 28f);
        [SerializeField, Min(64)] private int _resolution = 256;

        [Header("Interactive objects (fin + children)")]
        [SerializeField] private WakeSource[] _sources;
        [Range(0f, 0.6f)] [SerializeField] private float _discEdgeSoftness = 0.35f;
        [Tooltip("Au-dessus de cette vitesse, l'objet est considéré comme en mouvement.")]
        [SerializeField, Min(0.01f)] private float _restSpeed = 0.6f;
        [Tooltip("Écrase les anneaux verticalement pour suivre le plan de l'eau.")]
        [SerializeField, Range(0.3f, 1.2f)] private float _ringIsoYScale = 0.85f;
        [SerializeField, Range(-0.5f, 0.5f)] private float _ringIsoShear = 0f;

        [Header("Bow wave (V trail)")]
        [SerializeField] private Shader _fadeShader;
        [SerializeField] private Shader _discShader;
        [Tooltip("Durée de vie de la trail")]
        [SerializeField, Range(0.5f, 0.999f)] private float _fade = 0.95f;
        [SerializeField, Range(0f, 0.5f)] private float _diffuse = 0.12f;
        [Tooltip("Vitesse nécessaire pour avoir une traînée au maximum.")]
        [SerializeField, Min(0.01f)] private float _speedForFullStrength = 3f;
        [SerializeField, Range(0f, 2f)] private float _maxDeposit = 1.2f;
        [Tooltip("Longueur de base du cône derrière l'objet")]
        [SerializeField, Range(0.1f, 20f)] private float _brushLength = 3f;
        [Tooltip("Largeur du cône à sa pointe")]
        [SerializeField, Range(0.02f, 5f)] private float _brushWidth = 0.4f;
        [Tooltip("Élargissement du cône vers l'arrière.")]
        [SerializeField, Range(0f, 1f)] private float _brushSpread = 0.35f;
        [Tooltip("Softness du cône. Plus bas = bords plus fins.")]
        [SerializeField, Range(0.05f, 0.8f)] private float _brushEdge = 0.3f;

        private RenderTexture _maskRT;
        private RenderTexture _wakeA;
        private RenderTexture _wakeB;
        private Material _fadeMat;
        private Material _discMat;
        private Vector3[] _lastPositions;
        private bool _positionsInit;
        private Vector4[] _discs;

        private float _burst;
        private float _burstDecay;

        private static readonly int MaskId = Shader.PropertyToID("_WaterMask");
        private static readonly int WakeId = Shader.PropertyToID("_WakeTex");
        private static readonly int RectId = Shader.PropertyToID("_WaterMaskRect");
        private static readonly int TexelId = Shader.PropertyToID("_WaterTexel");

        private void OnEnable()
        {
            Shader fade = _fadeShader != null ? _fadeShader : Shader.Find("Hidden/Metroma/WaterWakeFade");
            Shader disc = _discShader != null ? _discShader : Shader.Find("Hidden/Metroma/WaterDiscMask");
            if (fade == null || disc == null)
            {
                Debug.LogError($"{name} : wake/disc shader not found.");
                enabled = false;
                return;
            }
            _fadeMat = new Material(fade) { hideFlags = HideFlags.HideAndDontSave };
            _discMat = new Material(disc) { hideFlags = HideFlags.HideAndDontSave };
            _discs = new Vector4[24];

            _maskRT = CreateRT();
            _wakeA = CreateRT();
            _wakeB = CreateRT();
            ClearRT(_maskRT);
            ClearRT(_wakeA);
            ClearRT(_wakeB);

            _positionsInit = false;
            Publish();
        }

        private void OnDisable()
        {
            ReleaseRT(ref _maskRT);
            ReleaseRT(ref _wakeA);
            ReleaseRT(ref _wakeB);
            if (_fadeMat != null) { Destroy(_fadeMat); _fadeMat = null; }
            if (_discMat != null) { Destroy(_discMat); _discMat = null; }
        }

        private RenderTexture CreateRT()
        {
            RenderTextureFormat format = RenderTextureFormat.R8;
            if (!SystemInfo.SupportsRenderTextureFormat(format)) format = RenderTextureFormat.RHalf;
            if (!SystemInfo.SupportsRenderTextureFormat(format)) format = RenderTextureFormat.Default;

            RenderTexture rt = new RenderTexture(_resolution, _resolution, 0, format, RenderTextureReadWrite.Linear)
            {
                name = "WaterBuffer",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            rt.Create();
            return rt;
        }

        private static void ClearRT(RenderTexture rt)
        {
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = active;
        }

        private static void ReleaseRT(ref RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            Destroy(rt);
            rt = null;
        }

        private void LateUpdate()
        {
            if (_fadeMat == null || _discMat == null) return;

            BuildDiscMask();
            AccumulateWake();

            if (_burst > 0f) _burst = Mathf.Max(0f, _burst - _burstDecay * Time.deltaTime);

            Publish();
        }

        private Vector2 AreaMin => _areaCenter - _areaSize * 0.5f;

        private Vector2 WorldToUV(Vector3 world)
        {
            Vector2 min = AreaMin;
            return new Vector2((world.x - min.x) / _areaSize.x, (world.y - min.y) / _areaSize.y);
        }

        private void BuildDiscMask() //onde marhce pas dingue
        {
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            int count = 0;
            if (_sources != null)
            {
                for (int i = 0; i < _sources.Length && count < _discs.Length; i++)
                {
                    Transform tr = _sources[i].transform;
                    if (tr == null) continue;
                    Vector2 uv = WorldToUV(tr.position);

                    float still = 1f; //fais disparaitre l'onde
                    if (_positionsInit && _lastPositions != null && i < _lastPositions.Length)
                    {
                        float speed = (tr.position - _lastPositions[i]).magnitude / dt;
                        still = 1f - Mathf.SmoothStep(_restSpeed * 0.5f, _restSpeed, speed);
                    }

                    _discs[count] = new Vector4(uv.x, uv.y, Mathf.Max(_sources[i].radius, 0.001f), still);
                    count++;
                }
            }

            _discMat.SetVectorArray("_Discs", _discs);
            _discMat.SetInt("_DiscCount", count);
            _discMat.SetVector("_AreaSize", _areaSize);
            _discMat.SetFloat("_EdgeSoftness", _discEdgeSoftness);
            _discMat.SetFloat("_IsoYScale", _ringIsoYScale);
            _discMat.SetFloat("_IsoShear", _ringIsoShear);
            Graphics.Blit(Texture2D.blackTexture, _maskRT, _discMat);
        }

        private void AccumulateWake()
        {
            int count = _sources != null ? _sources.Length : 0;
            if (_lastPositions == null || _lastPositions.Length != count)
            {
                _lastPositions = new Vector3[count];
                _positionsInit = false;
            }

            float aspect = _areaSize.x / Mathf.Max(_areaSize.y, 1e-4f);
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            int passes = Mathf.Max(1, count);

            RenderTexture src = _wakeA;
            RenderTexture dst = _wakeB;

            for (int i = 0; i < passes; i++)
            {
                //conversion monde UV (bullshit de maths encore)
                float toUv = 1f / Mathf.Max(_areaSize.y, 1e-4f);
                Vector2 brushUV = new Vector2(0.5f, 0.5f);
                Vector2 brushDir = Vector2.right;
                float strength = 0f;
                float brushLen = _brushLength * toUv;

                if (count > 0 && _sources[i].transform != null)
                {
                    Vector3 now = _sources[i].transform.position;
                    Vector3 last = _positionsInit ? _lastPositions[i] : now;

                    Vector2 deltaW = new Vector2(now.x - last.x, now.y - last.y);
                    float speed = deltaW.magnitude / dt;

                    brushUV = WorldToUV(now);
                    if (deltaW.sqrMagnitude > 1e-8f)
                    {
                        brushDir = deltaW.normalized;
                        strength = Mathf.Clamp01(speed / _speedForFullStrength) * _maxDeposit + _burst;
                        float lenWorld = Mathf.Max(_brushLength, deltaW.magnitude * 1.2f); //le V
                        brushLen = lenWorld * toUv;
                    }

                    _lastPositions[i] = now;
                }

                //set les params du shader
                bool first = i == 0;
                _fadeMat.SetFloat("_FadeAmount", first ? _fade : 1f);
                _fadeMat.SetFloat("_DiffuseAmount", first ? _diffuse : 0f);
                _fadeMat.SetFloat("_MaskStrength", 0f);   // wake is only the cones
                _fadeMat.SetFloat("_Texel", 1f / _resolution);
                _fadeMat.SetVector("_BrushUV", new Vector4(brushUV.x, brushUV.y, 0f, 0f));
                _fadeMat.SetVector("_BrushDir", new Vector4(brushDir.x, brushDir.y, 0f, 0f));
                _fadeMat.SetFloat("_BrushStrength", strength);
                _fadeMat.SetFloat("_BrushLength", brushLen);
                _fadeMat.SetFloat("_BrushWidth", _brushWidth * toUv);
                _fadeMat.SetFloat("_BrushSpread", _brushSpread);
                _fadeMat.SetFloat("_BrushEdge", _brushEdge);
                _fadeMat.SetFloat("_Aspect", aspect);

                Graphics.Blit(src, dst, _fadeMat);
                (src, dst) = (dst, src);
            }

            _wakeA = src;
            _wakeB = dst;
            _positionsInit = true;
        }

        public void Burst(float strength = 1f, float duration = 0.25f)
        {
            _burst = Mathf.Max(_burst, strength);
            _burstDecay = duration > 0f ? strength / duration : float.MaxValue;
        }

        private void Publish()
        {
            Vector2 min = AreaMin;
            Shader.SetGlobalTexture(MaskId, _maskRT);
            Shader.SetGlobalTexture(WakeId, _wakeA);
            Shader.SetGlobalVector(RectId, new Vector4(min.x, min.y, _areaSize.x, _areaSize.y));
            Shader.SetGlobalVector(TexelId, new Vector4(1f / _resolution, 1f / _resolution, _resolution, _resolution));
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireCube(_areaCenter, _areaSize);
        }
#endif
    }
}
