using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Metroma
{
    // Particule SPH SURTOUT identique a la struct HLSL (compute + shaders) sinon tu es mort
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct Particle
    {
        public float Pressure;
        public float Density;
        public Vector2 CurrentForce;
        public Vector2 Velocity;
        public Vector2 Position;
    }

    public class SPH : MonoBehaviour
    {
        [Header("General (de la merde touche pas)")]
        [SerializeField] private bool _showSpheres = false;
        [Tooltip("Bloc de particules pose au demarrage (X * Y).")]
        [SerializeField] private Vector2Int _nToSpawn = new Vector2Int(20, 20);
        [SerializeField] private Vector2 _spawnCenter = new Vector2(0, 0.4f);
        [Tooltip("Demi-espacement des particules au spawn (espacement = 2 * cette valeur).")]
        [SerializeField] private float _particleRadius = 0.04f;
        [SerializeField] private Vector2 _boxSize = new Vector2(2, 2); // flemme de clean (t pas content c pas grave)

        [Header("Capacity")]
        [Tooltip("Nombre max de particules, en puissance de 2 par pitié")]
        [SerializeField] private int _maxParticles = 4096;

        [Header("2D Collision (Collider2D)")]
        [SerializeField] private Collider2D _boundaryCollider;
        [SerializeField] private int _maxBoundaryPoints = 256;

        [Header("Rendering (debug spheres)")]
        [SerializeField] private Mesh _particleMesh;
        [SerializeField] private float _particlerRenderSize = 0.08f;
        [SerializeField] private Material _mat;

        [Header("SPH Constants (water 2D)")]
        [SerializeField] private ComputeShader _shader;
        [Tooltip("Rayon de lissage (influence) sur kes autres particules")]
        [SerializeField] private float _smoothingRadius = 0.16f;
        [SerializeField] private float _particleMass = 1f;
        [SerializeField] private float _restDensity = 200f;
        [SerializeField] private float _gasConstant = 250f;
        [SerializeField] private float _viscosity = 0.25f;
        [SerializeField] private float _boundDamping = -0.3f;
        [SerializeField] private Vector2 _gravity = new Vector2(0, -9.81f);
        [Tooltip("pas par FixedUpdate (stabilite + vitesse reelle).")]
        [Range(1, 8)] [SerializeField] private int _subSteps = 4; // plus c haut plus tu baises ton pc par contre
        [Tooltip("Calibre _restDensity sur la densite au repos AVANT la 1re integ (anti-explosion).")]
        [SerializeField] private bool _autoCalibrateRestDensity = true;

        [Header("Container follow")]
        [SerializeField] private bool _followContainer = true;

        [Header("Auto-fit to glass")]
        [Tooltip("Calcule automatiquement l'espacement/rayon pour remplir le verre")]
        [SerializeField] private bool _autoFitToCollider = true;
        [Tooltip("Nombre de particules vise pour remplir le verre au demarrage.")]
        [SerializeField] private int _targetParticleCount = 600;

        // Buffers
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _particlesBuffer;
        private ComputeBuffer _particlesIndices;
        private ComputeBuffer _particlesCellIndices;
        private ComputeBuffer _cellOffsets;
        private ComputeBuffer _boundaryBuffer;

        private Particle[] _particles;
        private Vector2[] _boundaryCpu;
        private int _paddedCount; // capacite reelle des buffers (puissance de 2)
        private int _activeCount;
        private bool _calibrated;

        // Kernels (fonction des computes shaders)
        private int _integrateKernel;
        private int _computeDensityPressureKernel;
        private int _computeForcesKernel;
        private int _hashParticlesKernel;
        private int _sortKernel;
        private int _calculateCellOffsetsKernel;
        private int _applyFrameMotionKernel;

        private Vector3 _lastContainerPos;
        private bool _frameInit;

        private static readonly int SIZE = Shader.PropertyToID("_size");
        private static readonly int PARTICLES_BUFFER = Shader.PropertyToID("_particlesBuffer");

        public ComputeBuffer ParticlesBuffer => _particlesBuffer;
        public int ActiveParticles => _activeCount;
        public int MaxParticles => _maxParticles;

        // Z du verre : on rend le fluide sur ce plan, sinon il est decale en perspective
        public float SimZ => _boundaryCollider != null ? _boundaryCollider.transform.position.z : 0f;

        private void Awake()
        {
            // copie perso du compute : sinon plusieurs verres se marchent dessus (buffers partages)
            _shader = Instantiate(_shader);

            _paddedCount = Mathf.NextPowerOfTwo(Mathf.Max(256, _maxParticles));
            _maxParticles = Mathf.Min(_maxParticles, _paddedCount);

            if (_autoFitToCollider) AutoFitToCollider();
            SpawnParticles();

            _particlesBuffer = new ComputeBuffer(_paddedCount, 32);
            _particlesBuffer.SetData(_particles);

            _particlesIndices = new ComputeBuffer(_paddedCount, 4);
            _particlesCellIndices = new ComputeBuffer(_paddedCount, 4);
            _cellOffsets = new ComputeBuffer(_paddedCount, 4);

            uint[] indices = new uint[_paddedCount];
            for (int i = 0; i < _paddedCount; i++) indices[i] = (uint)i;
            _particlesIndices.SetData(indices);

            _maxBoundaryPoints = Mathf.Max(4, _maxBoundaryPoints);
            _boundaryBuffer = new ComputeBuffer(_maxBoundaryPoints, sizeof(float) * 2);
            _boundaryCpu = new Vector2[_maxBoundaryPoints];

            if (_particleMesh != null)
            {
                uint[] args = { _particleMesh.GetIndexCount(0), (uint)_activeCount, _particleMesh.GetIndexStart(0), _particleMesh.GetBaseVertex(0), 0 };
                _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                _argsBuffer.SetData(args);
            }

            SetupComputeBuffers();
        }

        private void Start()
        {
            // on calibre AVANT de bouger quoi que ce soit, sinon ca explose frame 1 et t'as bien le seum
            if (_autoCalibrateRestDensity && _activeCount > 0)
                Warmup();
            _calibrated = true;
        }

        // Calcule l'espacement pour remplir le verre avec targetParticleCout
        private void AutoFitToCollider()
        {
            Vector2[] poly = GetBoundaryWorldPoints();
            if (poly == null || poly.Length < 3) return;

            float area = Mathf.Abs(PolygonArea(poly));
            int n = Mathf.Min(_targetParticleCount, _maxParticles);
            if (area <= 1e-6f || n <= 0) return;

            float spacing = Mathf.Sqrt(area / n);
            _particleRadius = spacing * 0.5f;
            _smoothingRadius = spacing * 2f;

            Debug.Log($"[SPH] Auto-fit: aire verre={area:F3}, spacing={spacing:F4}, " +
                      $"smoothingRadius={_smoothingRadius:F4}. Conseil Blob _size ~= {spacing * 2.5f:F3}");
        }

        private static float PolygonArea(Vector2[] poly)
        {
            float a = 0f;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i, i++)
                a += (poly[j].x + poly[i].x) * (poly[j].y - poly[i].y);
            return a * 0.5f;
        }

        // evite l'explosion tah les fous
        private void Warmup()
        {
            _shader.SetInt("_activeCount", _activeCount);
            UpdateBoundary();
            int threadGroups = Mathf.CeilToInt(_paddedCount / 256f);

            _shader.Dispatch(_hashParticlesKernel, threadGroups, 1, 1);
            SortParticles();
            _shader.Dispatch(_calculateCellOffsetsKernel, threadGroups, 1, 1);
            _shader.Dispatch(_computeDensityPressureKernel, threadGroups, 1, 1);

            CalibrateRestDensity();
        }

        private void SpawnParticles()
        {
            _particles = new Particle[_paddedCount];
            float spacing = _particleRadius * 2f;
            int desired = _autoFitToCollider
                ? Mathf.Min(_targetParticleCount, _maxParticles)
                : Mathf.Min(_nToSpawn.x * _nToSpawn.y, _maxParticles);

            int idx = 0;
            Vector2[] poly = GetBoundaryWorldPoints();

            if (poly != null && poly.Length >= 3)
            {
                // on remplit le verre par le bas
                float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                foreach (Vector2 v in poly)
                {
                    minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x);
                    minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y);
                }

                for (float y = minY + spacing * 0.5f; y <= maxY && idx < desired; y += spacing)
                {
                    for (float x = minX + spacing * 0.5f; x <= maxX && idx < desired; x += spacing)
                    {
                        Vector2 p = new Vector2(x, y);
                        if (!PointInPolygonCpu(poly, p)) continue; //clear
                        p += Random.insideUnitCircle * _particleRadius * 0.25f;
                        _particles[idx++] = new Particle { Position = p };
                    }
                }
            }
            else
            {
                // pas de collider bullshit qui reste faut que je clean encore mais azy flem il est 21h et on est dimanche sur la vie de ma mere
                for (int y = 0; y < _nToSpawn.y && idx < desired; y++)
                    for (int x = 0; x < _nToSpawn.x && idx < desired; x++)
                    {
                        Vector2 pos = _spawnCenter + new Vector2(x * spacing, y * spacing);
                        pos += Random.insideUnitCircle * _particleRadius * 0.3f;
                        _particles[idx++] = new Particle { Position = pos };
                    }
            }
            _activeCount = idx;

            // le padding tres loin, bien etale (chacun sa cellule, qu'on me foute la paix)
            Vector2 park = new Vector2(100000f, -100000f);
            for (; idx < _paddedCount; idx++)
                _particles[idx] = new Particle { Position = park + new Vector2(idx * _smoothingRadius, 0) };
        }

        // Points du collider en monde (V ou polygone), comme une boucle fermee
        private Vector2[] GetBoundaryWorldPoints()
        {
            if (_boundaryCollider is PolygonCollider2D poly && poly.points.Length >= 3)
            {
                Vector2[] pts = poly.points;
                Vector2[] world = new Vector2[pts.Length];
                for (int i = 0; i < pts.Length; i++) world[i] = poly.transform.TransformPoint(pts[i]);
                return world;
            }
            if (_boundaryCollider is EdgeCollider2D edge && edge.points.Length >= 3)
            {
                Vector2[] pts = edge.points;
                Vector2[] world = new Vector2[pts.Length];
                for (int i = 0; i < pts.Length; i++) world[i] = edge.transform.TransformPoint(pts[i]);
                return world;
            }
            return null;
        }

        private static bool PointInPolygonCpu(Vector2[] poly, Vector2 p)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i, i++)
            {
                if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y + 1e-8f) + poly[i].x))
                    inside = !inside;
            }
            return inside;
        }

        private void SetupComputeBuffers()
        {
            //YOUHOUUUU LE FUN DE SETUP A LA MAIN (oui oui j'ai fais à la main putain)
            _integrateKernel = _shader.FindKernel("Integrate");
            _computeDensityPressureKernel = _shader.FindKernel("ComputeDensityPressure");
            _computeForcesKernel = _shader.FindKernel("ComputeForces");
            _hashParticlesKernel = _shader.FindKernel("HashParticles");
            _sortKernel = _shader.FindKernel("BitonicSort");
            _calculateCellOffsetsKernel = _shader.FindKernel("CalculateCellOffsets");
            _applyFrameMotionKernel = _shader.FindKernel("ApplyFrameMotion");

            _shader.SetInt("_particleLenght", _paddedCount);
            _shader.SetFloat("_particleMass", _particleMass);
            _shader.SetFloat("_viscosity", _viscosity);
            _shader.SetFloat("_gasConstant", _gasConstant);
            _shader.SetFloat("_restDensity", _restDensity);
            _shader.SetFloat("_boundDamping", _boundDamping);
            _shader.SetVector("_gravity", _gravity);
            _shader.SetVector("_boxSize", _boxSize);

            // bullshit de maths des noyaux SPH, precalcule une bonne fois ici
            float h = _smoothingRadius;
            float h2 = h * h;
            float h5 = Mathf.Pow(h, 5);
            float h8 = Mathf.Pow(h, 8);
            _shader.SetFloat("_h", h);
            _shader.SetFloat("_h2", h2);
            _shader.SetFloat("_poly6", 4f / (Mathf.PI * h8));
            _shader.SetFloat("_spikyGrad", -30f / (Mathf.PI * h5));
            _shader.SetFloat("_viscLap", 40f / (Mathf.PI * h5));

            // le bon gros binding a la main (chaque buffer sur chaque kernel qui l'utilise). courage.
            BindBuffer(_particlesBuffer, "_particles",
                _integrateKernel, _computeDensityPressureKernel, _computeForcesKernel, _hashParticlesKernel, _applyFrameMotionKernel);
            BindBuffer(_particlesIndices, "_particleIndices",
                _computeForcesKernel, _computeDensityPressureKernel, _hashParticlesKernel, _sortKernel, _calculateCellOffsetsKernel);
            BindBuffer(_particlesCellIndices, "_particlesCellIndices",
                _computeForcesKernel, _computeDensityPressureKernel, _hashParticlesKernel, _sortKernel, _calculateCellOffsetsKernel);
            BindBuffer(_cellOffsets, "_cellOffsets",
                _computeForcesKernel, _computeDensityPressureKernel, _calculateCellOffsetsKernel, _hashParticlesKernel);

            _shader.SetBuffer(_integrateKernel, "_boundaryPoints", _boundaryBuffer);
        }

        private void BindBuffer(ComputeBuffer buffer, string name, params int[] kernels)
        {
            foreach (int k in kernels) _shader.SetBuffer(k, name, buffer);
        }

        private void UpdateBoundary()
        {
            int count = 0;
            int closed = 0;

            if (_boundaryCollider is PolygonCollider2D poly && poly.points.Length >= 2)
            {
                Vector2[] pts = poly.points;
                count = Mathf.Min(pts.Length, _maxBoundaryPoints);
                for (int i = 0; i < count; i++) _boundaryCpu[i] = poly.transform.TransformPoint(pts[i]);
                closed = 1;
            }
            else if (_boundaryCollider is EdgeCollider2D edge && edge.points.Length >= 2)
            {
                Vector2[] pts = edge.points;
                count = Mathf.Min(pts.Length, _maxBoundaryPoints);
                for (int i = 0; i < count; i++) _boundaryCpu[i] = edge.transform.TransformPoint(pts[i]);
                closed = 0;
            }

            if (count >= 2) _boundaryBuffer.SetData(_boundaryCpu, 0, 0, count);
            _shader.SetInt("_boundaryCount", count);
            _shader.SetInt("_boundaryClosed", closed);
        }

        // suivie de l'eau dans le verre (j'ai pas trouvé comment faire autrement)
        private void ApplyContainerMotion(int threadGroups)
        {
            Transform t = _boundaryCollider != null ? _boundaryCollider.transform : transform;
            Vector3 cur = t.position;

            if (!_frameInit)
            {
                _lastContainerPos = cur;
                _frameInit = true;
                return;
            }

            Vector2 delta = new Vector2(cur.x - _lastContainerPos.x, cur.y - _lastContainerPos.y);
            _lastContainerPos = cur;
            if (delta.sqrMagnitude < 1e-12f) return;

            _shader.SetVector("_frameOffset", delta);
            _shader.Dispatch(_applyFrameMotionKernel, threadGroups, 1, 1);
        }

        private void FixedUpdate()
        {
            _shader.SetInt("_activeCount", _activeCount);
            _shader.SetVector("_gravity", _gravity);
            UpdateBoundary();

            int threadGroups = Mathf.CeilToInt(_paddedCount / 256f);

            if (_followContainer) ApplyContainerMotion(threadGroups);
            
            float dt = Time.fixedDeltaTime / _subSteps;
            _shader.SetFloat("_timestep", dt);

            // l'ordre est sacre
            for (int s = 0; s < _subSteps; s++)
            {
                _shader.Dispatch(_hashParticlesKernel, threadGroups, 1, 1);
                SortParticles();
                _shader.Dispatch(_calculateCellOffsetsKernel, threadGroups, 1, 1);
                _shader.Dispatch(_computeDensityPressureKernel, threadGroups, 1, 1);
                _shader.Dispatch(_computeForcesKernel, threadGroups, 1, 1);
                _shader.Dispatch(_integrateKernel, threadGroups, 1, 1);
            }

            if (_autoCalibrateRestDensity && !_calibrated && _activeCount > 0)
                CalibrateRestDensity();
        }

        private void CalibrateRestDensity()
        {
            Particle[] tmp = new Particle[_activeCount];
            _particlesBuffer.GetData(tmp, 0, 0, _activeCount);
            float sum = 0;
            for (int i = 0; i < _activeCount; i++) sum += tmp[i].Density;
            float avg = sum / _activeCount;
            if (avg > 0.0001f)
            {
                _restDensity = avg;
                _shader.SetFloat("_restDensity", _restDensity);
            }
            _calibrated = true;
        }

        private void SortParticles()
        {
            for (int dim = 2; dim <= _paddedCount; dim <<= 1)
            {
                _shader.SetInt("dim", dim);
                for (int block = dim >> 1; block > 0; block >>= 1)
                {
                    _shader.SetInt("block", block);
                    _shader.Dispatch(_sortKernel, Mathf.CeilToInt(_paddedCount / 256f), 1, 1);
                }
            }
        }

        private void Update()
        {
            if (!_showSpheres || _mat == null || _particleMesh == null || _argsBuffer == null) return;

            uint[] args = { _particleMesh.GetIndexCount(0), (uint)_activeCount, _particleMesh.GetIndexStart(0), _particleMesh.GetBaseVertex(0), 0 };
            _argsBuffer.SetData(args);

            _mat.SetFloat(SIZE, _particlerRenderSize);
            _mat.SetBuffer(PARTICLES_BUFFER, _particlesBuffer);
            Graphics.DrawMeshInstancedIndirect(
                _particleMesh, 0, _mat, new Bounds(Vector3.zero, Vector3.one * 1000f),
                _argsBuffer, castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
        }

        private void OnDestroy()
        {
            _argsBuffer?.Release();
            _particlesBuffer?.Release();
            _particlesIndices?.Release();
            _particlesCellIndices?.Release();
            _cellOffsets?.Release();
            _boundaryBuffer?.Release();
            if (_shader != null) Destroy(_shader); // on degage la copie du compute
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_spawnCenter, 0.05f);
            }
        }
    }
}
