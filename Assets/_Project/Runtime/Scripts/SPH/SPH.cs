using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Metroma
{
    [Serializable] 
    [StructLayout(LayoutKind.Sequential, Size=44)] 
    public struct Particle
    {
        public float Pressure;
        public float Density;
        public Vector3 CurrentForce;
        public Vector3 Velocity;
        public Vector3 Position;
    }
    public class SPH : MonoBehaviour
    {
        [Header("General")] 
        [SerializeField] private bool _showSpheres = true;
        [SerializeField] private Vector3Int _nToSpawn = new Vector3Int(8, 8, 8);
        [SerializeField] private Vector3 _boxSize = new Vector3(1, 1, 1);
        [SerializeField] private Vector3 _spawnCenter;
        [SerializeField] private float _particleRadius = 0.1f;
        [SerializeField] private float spawnJitter = 0.2f;

        [Header("Particle Rendering")] 
        [SerializeField] private Mesh _particleMesh;
        [SerializeField] private float _particlerRenderSize = 8f;
        [SerializeField] private Material _mat;

        [Header("2D Collision (Collider2D)")]
        [Tooltip("PolygonCollider2D (ferme) ou EdgeCollider2D (ouvert). Lu chaque FixedUpdate pour supporter le mouvement runtime.")]
        [SerializeField] private Collider2D _boundaryCollider;
        [Tooltip("Capacite max de points alloues pour le buffer de frontiere.")]
        [SerializeField] private int _maxBoundaryPoints = 256;

        [Header("Compute")]
        [SerializeField] private ComputeShader _shader;
        [SerializeField] private Particle[] _particles;
        [SerializeField] private float _boundDamping = -0.5f;
        [SerializeField] private float _viscosity = 200f;
        [SerializeField] private float _particleMass = 2.5f;
        [SerializeField] private float _gasContant = 2000f;
        [SerializeField] private float _restDensity = 300f;
        [SerializeField] private float _timestep = 0.005f; //Encule ton pc si c trop bas (à vos risques et périls)
        
        private int _totaltParticles => _nToSpawn.x * _nToSpawn.y * _nToSpawn.z;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _particlesBuffer;

        // Buffer des points du collider
        private ComputeBuffer _boundaryBuffer;
        private Vector2[] _boundaryCpu;
        private int _boundaryCount;

        public ComputeBuffer ParticlesBuffer => _particlesBuffer;
        public ComputeBuffer ArgsBuffer => _argsBuffer;
        public int TotalParticles => _totaltParticles;

		private ComputeBuffer _particlesIndices;
		private ComputeBuffer _particlesCellIndices;
        private ComputeBuffer _cellOffsets;
        
        //bullshit du compute shader
        private int _integrateKernel;
        private int _computeDensityPressureKernel;
        private int _computeForcesKernel;
        private int _hashParticlesKernel;
        private int _sortKernel;
        private int _calculateCellOffsetsKernel;

        private static readonly int SIZE = Shader.PropertyToID("_size");
        private static readonly int PARTICLES_BUFFER = Shader.PropertyToID("_particlesBuffer");

        private void Awake()
        {
            SpawnParticlesInBox();
            
            uint[] args =
            {
                _particleMesh.GetIndexCount(0),
                (uint)_totaltParticles,
                _particleMesh.GetIndexStart(0),
                _particleMesh.GetBaseVertex(0),
                0
            };
            
            _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);

            _particlesBuffer = new ComputeBuffer(_totaltParticles, 44);
            _particlesBuffer.SetData(_particles);

            _particlesIndices = new ComputeBuffer(_totaltParticles, 4);
            _particlesCellIndices = new ComputeBuffer(_totaltParticles, 4);
            _cellOffsets = new ComputeBuffer(_totaltParticles, 4);

            uint[] particleIndices = new uint[_totaltParticles];

            for(int i = 0; i < _totaltParticles; i++) particleIndices[i] = (uint)i;

            _particlesIndices.SetData(particleIndices);
            
            _maxBoundaryPoints = Mathf.Max(4, _maxBoundaryPoints);
            _boundaryBuffer = new ComputeBuffer(_maxBoundaryPoints, sizeof(float) * 2);
            _boundaryCpu = new Vector2[_maxBoundaryPoints];

            SetupComputeBuffers();
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
            
            _shader.SetInt("_particleLenght", _totaltParticles);
            _shader.SetFloat("_particleMass", _particleMass);
            _shader.SetFloat("_viscosity", _viscosity);
            _shader.SetFloat("_gasContant", _gasContant);
            _shader.SetFloat("_restDensity", _restDensity);
            _shader.SetFloat("_boundDamping", _boundDamping);
            _shader.SetFloat("_pi", Mathf.PI);
            _shader.SetVector("_boxSize", _boxSize);
            
            _shader.SetFloat("_radius", _particleRadius);
            _shader.SetFloat("_radius2", _particleRadius * _particleRadius);
            _shader.SetFloat("_radius3", _particleRadius * _particleRadius * _particleRadius);
            _shader.SetFloat("_radius4", _particleRadius * _particleRadius * _particleRadius * _particleRadius);
            _shader.SetFloat("_radius5", _particleRadius * _particleRadius * _particleRadius * _particleRadius * _particleRadius);
            
            _shader.SetBuffer(_integrateKernel,"_particles", _particlesBuffer);
            _shader.SetBuffer(_integrateKernel, "_boundaryPoints", _boundaryBuffer);
            _shader.SetBuffer(_computeDensityPressureKernel,"_particles", _particlesBuffer);
            _shader.SetBuffer(_computeForcesKernel,"_particles", _particlesBuffer);
            _shader.SetBuffer(_hashParticlesKernel,"_particles", _particlesBuffer);
            
            _shader.SetBuffer(_computeForcesKernel, "_particleIndices", _particlesIndices);
            _shader.SetBuffer(_computeDensityPressureKernel, "_particleIndices", _particlesIndices);
            _shader.SetBuffer(_hashParticlesKernel, "_particleIndices", _particlesIndices);
            _shader.SetBuffer(_sortKernel, "_particleIndices", _particlesIndices);
            _shader.SetBuffer(_calculateCellOffsetsKernel, "_particleIndices", _particlesIndices);
            
            _shader.SetBuffer(_computeForcesKernel, "_particlesCellIndices", _particlesCellIndices);
            _shader.SetBuffer(_computeDensityPressureKernel, "_particlesCellIndices", _particlesCellIndices);
            _shader.SetBuffer(_hashParticlesKernel, "_particlesCellIndices", _particlesCellIndices);
            _shader.SetBuffer(_sortKernel, "_particlesCellIndices", _particlesCellIndices);
            _shader.SetBuffer(_calculateCellOffsetsKernel, "_particlesCellIndices", _particlesCellIndices);
            
            _shader.SetBuffer(_computeForcesKernel, "_cellOffsets", _cellOffsets);
            _shader.SetBuffer(_computeDensityPressureKernel, "_cellOffsets", _cellOffsets);
            _shader.SetBuffer(_calculateCellOffsetsKernel, "_cellOffsets", _cellOffsets);
            _shader.SetBuffer(_hashParticlesKernel, "_cellOffsets", _cellOffsets);
        }

        private void OnDestroy()
        {
            if (_argsBuffer != null) _argsBuffer.Release();
            if (_particlesBuffer != null) _particlesBuffer.Release();
            if (_particlesIndices != null) _particlesIndices.Release();
            if (_particlesCellIndices != null) _particlesCellIndices.Release();
            if (_cellOffsets != null) _cellOffsets.Release();
            if (_boundaryBuffer != null) _boundaryBuffer.Release();
        }

        private void SpawnParticlesInBox()
        {
            Vector3 spawnPoint = _spawnCenter;
            List<Particle> particles = new List<Particle>();

            for (int x = 0; x < _nToSpawn.x; x++)
            {
                for(int y = 0; y < _nToSpawn.y; y++)
                {
                    for(int z = 0; z < _nToSpawn.z; z++)
                    {
                        Vector3 spawnPos = spawnPoint + new Vector3(x * _particleRadius * 2, y * _particleRadius * 2, 0f); //j'applatis
                        Vector2 jitter = Random.insideUnitCircle * spawnJitter * _particleRadius;
                        spawnPos += new Vector3(jitter.x, jitter.y, 0f);
                        Particle particle = new Particle
                        {
                            Position = spawnPos,
                        };
                        particles.Add(particle);
                    }
                }
            }

            _particles = particles.ToArray();
        }
        
        private void UpdateBoundary() //recup les pts du collider
        {
            _boundaryCount = 0;
            int closed = 0;

            if (_boundaryCollider is PolygonCollider2D poly && poly.points.Length >= 2)
            {
                Vector2[] pts = poly.points; // espace local du collider
                _boundaryCount = Mathf.Min(pts.Length, _maxBoundaryPoints);
                for (int i = 0; i < _boundaryCount; i++)
                    _boundaryCpu[i] = poly.transform.TransformPoint(pts[i]);
                closed = 1; // polygone ferme
            }
            else if (_boundaryCollider is EdgeCollider2D edge && edge.points.Length >= 2)
            {
                Vector2[] pts = edge.points;
                _boundaryCount = Mathf.Min(pts.Length, _maxBoundaryPoints);
                for (int i = 0; i < _boundaryCount; i++)
                    _boundaryCpu[i] = edge.transform.TransformPoint(pts[i]);
                closed = 0; // chaine ouverte
            }

            if (_boundaryCount >= 2)
                _boundaryBuffer.SetData(_boundaryCpu, 0, 0, _boundaryCount);

            _shader.SetInt("_boundaryCount", _boundaryCount);
            _shader.SetInt("_boundaryClosed", closed);
        }

        private void FixedUpdate()
        {
            _shader.SetVector("_boxSize", _boxSize);
            _shader.SetFloat("_timestep", _timestep);

            UpdateBoundary();

            int threadGroups = Mathf.CeilToInt(_totaltParticles / 256f);
            
            _shader.Dispatch(_hashParticlesKernel, threadGroups, 1, 1);

            SortParticles();
            
            _shader.Dispatch(_calculateCellOffsetsKernel, threadGroups, 1, 1);
            
            _shader.Dispatch(_computeDensityPressureKernel, threadGroups, 1, 1);
            _shader.Dispatch(_computeForcesKernel, threadGroups, 1, 1);
            _shader.Dispatch(_integrateKernel, threadGroups, 1, 1);
        }

        private void Update()
        {
            _mat.SetFloat(SIZE, _particlerRenderSize);
            _mat.SetBuffer(PARTICLES_BUFFER, _particlesBuffer);

            if (_showSpheres)
            {
                Graphics.DrawMeshInstancedIndirect(
                    _particleMesh,
                    0,
                    _mat,
                    new Bounds(Vector3.zero, _boxSize),
                    _argsBuffer,
                    castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
            }
        }

        private void SortParticles()
        {
            for (int dim = 2; dim <= _totaltParticles; dim <<= 1)
            {
                _shader.SetInt("dim", dim);
                for (int block = dim >> 1; block > 0; block >>= 1)
                {
                    _shader.SetInt("block", block);
                    int threadGroups = Mathf.CeilToInt(_totaltParticles / 256f);
                    _shader.Dispatch(_sortKernel, threadGroups, 1, 1);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, _boxSize);

            if (!Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_spawnCenter, 0.1f);
            }
        }
    }
}
