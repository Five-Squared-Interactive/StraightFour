// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Voxels.Core
{
    /// <summary>
    /// Represents a chunk of voxels with 3D spatial organization.
    /// Chunks are the fundamental unit of spatial partitioning in the voxel engine.
    /// </summary>
    public class ChunkData
    {
        /// <summary>
        /// Chunk coordinates in chunk space (not voxel space).
        /// </summary>
        public int3 ChunkCoordinates { get; private set; }

        /// <summary>
        /// Size of the chunk in voxels per dimension (typically 32).
        /// </summary>
        public int ChunkSize { get; private set; }

        /// <summary>
        /// Total number of voxels in the chunk (ChunkSize^3).
        /// </summary>
        public int TotalVoxels => ChunkSize * ChunkSize * ChunkSize;

        /// <summary>
        /// Voxel data stored as a flat array [x + y * ChunkSize + z * ChunkSize * ChunkSize].
        /// </summary>
        private VoxelData[] voxels;

        /// <summary>
        /// Reference to the generated mesh for this chunk.
        /// </summary>
        public Mesh ChunkMesh { get; set; }

        /// <summary>
        /// Reference to the mesh collider for this chunk.
        /// </summary>
        public MeshCollider ChunkCollider { get; set; }

        /// <summary>
        /// Reference to the mesh filter for this chunk.
        /// </summary>
        public MeshFilter ChunkMeshFilter { get; set; }

        /// <summary>
        /// Flag indicating if the chunk mesh needs regeneration.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Create a new chunk with the specified coordinates and size.
        /// </summary>
        public ChunkData(int3 chunkCoordinates, int chunkSize = 32)
        {
            ChunkCoordinates = chunkCoordinates;
            ChunkSize = chunkSize;
            voxels = new VoxelData[TotalVoxels];
            IsDirty = true;

            // Initialize all voxels as empty
            for (int i = 0; i < TotalVoxels; i++)
            {
                voxels[i] = VoxelData.CreateEmpty();
            }
        }

        /// <summary>
        /// Get the 1D index from 3D coordinates.
        /// </summary>
        public int GetIndex(int x, int y, int z)
        {
            return x + y * ChunkSize + z * ChunkSize * ChunkSize;
        }

        /// <summary>
        /// Get the 3D coordinates from a 1D index.
        /// </summary>
        public int3 GetCoordinates(int index)
        {
            int z = index / (ChunkSize * ChunkSize);
            int remainder = index % (ChunkSize * ChunkSize);
            int y = remainder / ChunkSize;
            int x = remainder % ChunkSize;
            return new int3(x, y, z);
        }

        /// <summary>
        /// Get voxel data at the specified local coordinates.
        /// </summary>
        public VoxelData GetVoxel(int x, int y, int z)
        {
            if (!IsValidCoordinate(x, y, z))
            {
                return VoxelData.CreateEmpty();
            }
            return voxels[GetIndex(x, y, z)];
        }

        /// <summary>
        /// Set voxel data at the specified local coordinates.
        /// </summary>
        public void SetVoxel(int x, int y, int z, VoxelData voxel)
        {
            if (!IsValidCoordinate(x, y, z))
            {
                return;
            }
            voxels[GetIndex(x, y, z)] = voxel;
            IsDirty = true;
        }

        /// <summary>
        /// Check if coordinates are within chunk bounds.
        /// </summary>
        public bool IsValidCoordinate(int x, int y, int z)
        {
            return x >= 0 && x < ChunkSize &&
                   y >= 0 && y < ChunkSize &&
                   z >= 0 && z < ChunkSize;
        }

        /// <summary>
        /// Get a native array copy of voxel data for use in Jobs.
        /// Caller is responsible for disposing the array.
        /// </summary>
        public NativeArray<VoxelData> GetNativeVoxelArray(Allocator allocator)
        {
            NativeArray<VoxelData> nativeArray = new NativeArray<VoxelData>(TotalVoxels, allocator);
            nativeArray.CopyFrom(voxels);
            return nativeArray;
        }

        /// <summary>
        /// Copy data from a native array back into the chunk.
        /// </summary>
        public void SetFromNativeArray(NativeArray<VoxelData> nativeArray)
        {
            if (nativeArray.Length != TotalVoxels)
            {
                Debug.LogError($"[ChunkData] Native array size mismatch. Expected {TotalVoxels}, got {nativeArray.Length}");
                return;
            }
            nativeArray.CopyTo(voxels);
            IsDirty = true;
        }

        /// <summary>
        /// Fill the entire chunk with a single voxel type.
        /// </summary>
        public void Fill(VoxelData voxel)
        {
            for (int i = 0; i < TotalVoxels; i++)
            {
                voxels[i] = voxel;
            }
            IsDirty = true;
        }

        /// <summary>
        /// Get world position for this chunk.
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            return new Vector3(
                ChunkCoordinates.x * ChunkSize,
                ChunkCoordinates.y * ChunkSize,
                ChunkCoordinates.z * ChunkSize
            );
        }
    }
}
