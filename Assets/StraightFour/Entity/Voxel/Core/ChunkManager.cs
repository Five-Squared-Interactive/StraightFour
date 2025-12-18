// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using FiveSQD.StraightFour.Entity.Voxels.Core.Meshing;

namespace FiveSQD.StraightFour.Entity.Voxels.Core
{
    /// <summary>
    /// Manages chunks and their mesh generation pipeline.
    /// Coordinates spatial partitioning, meshing jobs, and mesh assignment.
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {
        /// <summary>
        /// Default chunk size (32³ voxels).
        /// </summary>
        public int DefaultChunkSize = 32;

        /// <summary>
        /// Material to use for chunk meshes.
        /// </summary>
        public Material ChunkMaterial;

        /// <summary>
        /// Dictionary of all loaded chunks, indexed by chunk coordinates.
        /// </summary>
        private Dictionary<int3, ChunkData> chunks = new Dictionary<int3, ChunkData>();

        /// <summary>
        /// Mesher for blocky voxels.
        /// </summary>
        private IMeshingJob greedyMesher;

        /// <summary>
        /// Mesher for smooth terrain voxels.
        /// </summary>
        private IMeshingJob marchingCubesMesher;

        /// <summary>
        /// Queue of chunks waiting for mesh generation.
        /// </summary>
        private Queue<ChunkData> meshingQueue = new Queue<ChunkData>();

        /// <summary>
        /// Currently executing meshing job handle.
        /// </summary>
        private JobHandle? currentMeshingJob;

        /// <summary>
        /// Mesh data for the current job.
        /// </summary>
        private MeshData currentMeshData;

        /// <summary>
        /// Chunk being processed by current job.
        /// </summary>
        private ChunkData currentChunk;

        /// <summary>
        /// Native array for current job (needs disposal).
        /// </summary>
        private NativeArray<VoxelData> currentVoxelArray;

        private void Awake()
        {
            greedyMesher = new GreedyMesher();
            marchingCubesMesher = new MarchingCubesMesher(0.5f);
        }

        private void Update()
        {
            // Process meshing queue
            if (currentMeshingJob.HasValue)
            {
                if (currentMeshingJob.Value.IsCompleted)
                {
                    CompleteMeshingJob();
                }
            }
            else if (meshingQueue.Count > 0)
            {
                StartNextMeshingJob();
            }
        }

        private void OnDestroy()
        {
            // Clean up any pending jobs
            if (currentMeshingJob.HasValue)
            {
                currentMeshingJob.Value.Complete();
                CleanupCurrentJob();
            }

            // Dispose all chunks
            foreach (var chunk in chunks.Values)
            {
                if (chunk.ChunkMesh != null)
                {
                    Destroy(chunk.ChunkMesh);
                }
            }
        }

        /// <summary>
        /// Get or create a chunk at the specified coordinates.
        /// </summary>
        public ChunkData GetOrCreateChunk(int3 chunkCoordinates)
        {
            if (!chunks.TryGetValue(chunkCoordinates, out ChunkData chunk))
            {
                chunk = new ChunkData(chunkCoordinates, DefaultChunkSize);
                chunks[chunkCoordinates] = chunk;
            }
            return chunk;
        }

        /// <summary>
        /// Get a chunk at the specified coordinates, or null if it doesn't exist.
        /// </summary>
        public ChunkData GetChunk(int3 chunkCoordinates)
        {
            chunks.TryGetValue(chunkCoordinates, out ChunkData chunk);
            return chunk;
        }

        /// <summary>
        /// Set a voxel at world coordinates.
        /// </summary>
        public void SetVoxel(int3 worldPosition, VoxelData voxel)
        {
            int3 chunkCoords = WorldToChunkCoordinates(worldPosition);
            int3 localCoords = WorldToLocalCoordinates(worldPosition, chunkCoords);

            ChunkData chunk = GetOrCreateChunk(chunkCoords);
            chunk.SetVoxel(localCoords.x, localCoords.y, localCoords.z, voxel);

            // Mark chunk for remeshing
            if (chunk.IsDirty && !meshingQueue.Contains(chunk))
            {
                meshingQueue.Enqueue(chunk);
            }
        }

        /// <summary>
        /// Get a voxel at world coordinates.
        /// </summary>
        public VoxelData GetVoxel(int3 worldPosition)
        {
            int3 chunkCoords = WorldToChunkCoordinates(worldPosition);
            int3 localCoords = WorldToLocalCoordinates(worldPosition, chunkCoords);

            ChunkData chunk = GetChunk(chunkCoords);
            if (chunk == null)
            {
                return VoxelData.CreateEmpty();
            }

            return chunk.GetVoxel(localCoords.x, localCoords.y, localCoords.z);
        }

        /// <summary>
        /// Request mesh generation for a chunk.
        /// </summary>
        public void RequestChunkMesh(ChunkData chunk)
        {
            if (!meshingQueue.Contains(chunk))
            {
                meshingQueue.Enqueue(chunk);
            }
        }

        /// <summary>
        /// Convert world coordinates to chunk coordinates.
        /// </summary>
        private int3 WorldToChunkCoordinates(int3 worldPosition)
        {
            return new int3(
                Mathf.FloorToInt((float)worldPosition.x / DefaultChunkSize),
                Mathf.FloorToInt((float)worldPosition.y / DefaultChunkSize),
                Mathf.FloorToInt((float)worldPosition.z / DefaultChunkSize)
            );
        }

        /// <summary>
        /// Convert world coordinates to local chunk coordinates.
        /// </summary>
        private int3 WorldToLocalCoordinates(int3 worldPosition, int3 chunkCoords)
        {
            return new int3(
                worldPosition.x - chunkCoords.x * DefaultChunkSize,
                worldPosition.y - chunkCoords.y * DefaultChunkSize,
                worldPosition.z - chunkCoords.z * DefaultChunkSize
            );
        }

        private void StartNextMeshingJob()
        {
            currentChunk = meshingQueue.Dequeue();
            currentChunk.IsDirty = false;

            // Determine which mesher to use based on chunk content
            bool useSmooth = ShouldUseSmoothing(currentChunk);
            IMeshingJob mesher = useSmooth ? marchingCubesMesher : greedyMesher;

            // Prepare mesh data
            currentMeshData = new MeshData(Allocator.TempJob);
            currentVoxelArray = currentChunk.GetNativeVoxelArray(Allocator.TempJob);

            // Schedule job
            currentMeshingJob = mesher.Schedule(currentVoxelArray, DefaultChunkSize, ref currentMeshData);
        }

        private void CompleteMeshingJob()
        {
            // Ensure job is complete
            currentMeshingJob.Value.Complete();

            // Generate Unity mesh
            Mesh mesh = currentMeshData.ToMesh();

            // Assign mesh to chunk
            if (currentChunk.ChunkMeshFilter == null)
            {
                CreateChunkGameObject(currentChunk);
            }

            currentChunk.ChunkMesh = mesh;
            currentChunk.ChunkMeshFilter.sharedMesh = mesh;

            if (currentChunk.ChunkCollider != null)
            {
                currentChunk.ChunkCollider.sharedMesh = mesh;
            }

            CleanupCurrentJob();
        }

        private void CleanupCurrentJob()
        {
            currentMeshData.Dispose();
            if (currentVoxelArray.IsCreated)
            {
                currentVoxelArray.Dispose();
            }
            currentMeshingJob = null;
            currentChunk = null;
        }

        private void CreateChunkGameObject(ChunkData chunk)
        {
            GameObject chunkObject = new GameObject($"Chunk_{chunk.ChunkCoordinates.x}_{chunk.ChunkCoordinates.y}_{chunk.ChunkCoordinates.z}");
            chunkObject.transform.SetParent(transform);
            chunkObject.transform.position = chunk.GetWorldPosition();

            MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = chunkObject.AddComponent<MeshCollider>();

            if (ChunkMaterial != null)
            {
                meshRenderer.material = ChunkMaterial;
            }

            chunk.ChunkMeshFilter = meshFilter;
            chunk.ChunkCollider = meshCollider;
        }

        private bool ShouldUseSmoothing(ChunkData chunk)
        {
            // Sample a few voxels to determine if chunk contains smooth voxels
            // In a full implementation, this could be cached as chunk metadata
            for (int i = 0; i < chunk.TotalVoxels; i += 100)
            {
                int3 coords = chunk.GetCoordinates(i);
                VoxelData voxel = chunk.GetVoxel(coords.x, coords.y, coords.z);
                if (voxel.IsSmooth && voxel.IsSolid)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get total number of loaded chunks.
        /// </summary>
        public int GetChunkCount()
        {
            return chunks.Count;
        }

        /// <summary>
        /// Unload a specific chunk.
        /// </summary>
        public void UnloadChunk(int3 chunkCoordinates)
        {
            if (chunks.TryGetValue(chunkCoordinates, out ChunkData chunk))
            {
                if (chunk.ChunkMeshFilter != null)
                {
                    Destroy(chunk.ChunkMeshFilter.gameObject);
                }
                if (chunk.ChunkMesh != null)
                {
                    Destroy(chunk.ChunkMesh);
                }
                chunks.Remove(chunkCoordinates);
            }
        }
    }
}
