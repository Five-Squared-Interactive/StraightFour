// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using Unity.Mathematics;
using FiveSQD.StraightFour.Entity.Voxels.Core;

namespace FiveSQD.StraightFour.Entity.Voxels.Examples
{
    /// <summary>
    /// Example script demonstrating the voxel core system.
    /// Creates test chunks with both blocky and smooth voxels.
    /// </summary>
    public class VoxelCoreExample : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Material to use for voxel chunks")]
        public Material voxelMaterial;

        [Header("Settings")]
        [Tooltip("Type of demo to run")]
        public DemoType demoType = DemoType.BlockyTerrain;

        [Tooltip("Chunk size (default 32)")]
        public int chunkSize = 32;

        private ChunkManager chunkManager;

        public enum DemoType
        {
            BlockyTerrain,
            SmoothTerrain,
            Mixed
        }

        private void Start()
        {
            // Create chunk manager
            GameObject managerObj = new GameObject("ChunkManager");
            managerObj.transform.SetParent(transform);
            chunkManager = managerObj.AddComponent<ChunkManager>();
            chunkManager.DefaultChunkSize = chunkSize;
            chunkManager.ChunkMaterial = voxelMaterial;

            // Generate demo based on selected type
            switch (demoType)
            {
                case DemoType.BlockyTerrain:
                    GenerateBlockyTerrain();
                    break;
                case DemoType.SmoothTerrain:
                    GenerateSmoothTerrain();
                    break;
                case DemoType.Mixed:
                    GenerateMixedTerrain();
                    break;
            }
        }

        /// <summary>
        /// Generate a simple blocky terrain with discrete voxels.
        /// </summary>
        private void GenerateBlockyTerrain()
        {
            Debug.Log("[VoxelCoreExample] Generating blocky terrain...");

            // Create a simple stepped terrain
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    // Create steps
                    int height = (x / 4) + (z / 4);
                    
                    for (int y = 0; y <= height && y < chunkSize; y++)
                    {
                        // Material varies by height
                        ushort materialId = (ushort)(1 + (y % 3));
                        VoxelData voxel = VoxelData.CreateSolid(materialId);
                        chunkManager.SetVoxel(new int3(x, y, z), voxel);
                    }
                }
            }

            Debug.Log("[VoxelCoreExample] Blocky terrain generated!");
        }

        /// <summary>
        /// Generate smooth terrain using density field.
        /// </summary>
        private void GenerateSmoothTerrain()
        {
            Debug.Log("[VoxelCoreExample] Generating smooth terrain...");

            float noiseScale = 0.1f;
            
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    // Use Perlin noise to create height variation
                    float noiseValue = Mathf.PerlinNoise(x * noiseScale, z * noiseScale);
                    int baseHeight = Mathf.FloorToInt(noiseValue * chunkSize * 0.5f);

                    for (int y = 0; y < chunkSize; y++)
                    {
                        // Calculate density based on distance from surface
                        float distanceFromSurface = baseHeight - y;
                        float density = Mathf.Clamp01(distanceFromSurface / 4f + 0.5f);

                        if (density > 0.1f)
                        {
                            VoxelData voxel = VoxelData.CreateSmooth(density, 2);
                            chunkManager.SetVoxel(new int3(x, y, z), voxel);
                        }
                    }
                }
            }

            Debug.Log("[VoxelCoreExample] Smooth terrain generated!");
        }

        /// <summary>
        /// Generate a mix of blocky and smooth voxels.
        /// </summary>
        private void GenerateMixedTerrain()
        {
            Debug.Log("[VoxelCoreExample] Generating mixed terrain...");

            // Bottom half: blocky foundation
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int y = 0; y < chunkSize / 2; y++)
                    {
                        VoxelData voxel = VoxelData.CreateSolid(1);
                        chunkManager.SetVoxel(new int3(x, y, z), voxel);
                    }
                }
            }

            // Top half: smooth hills
            float noiseScale = 0.15f;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * noiseScale, z * noiseScale);
                    int hillHeight = Mathf.FloorToInt(noiseValue * chunkSize * 0.3f);

                    for (int y = chunkSize / 2; y < chunkSize / 2 + hillHeight; y++)
                    {
                        float density = 1.0f - ((float)(y - chunkSize / 2) / hillHeight);
                        VoxelData voxel = VoxelData.CreateSmooth(density, 3);
                        chunkManager.SetVoxel(new int3(x, y, z), voxel);
                    }
                }
            }

            Debug.Log("[VoxelCoreExample] Mixed terrain generated!");
        }

        private void OnDestroy()
        {
            // Cleanup is handled by ChunkManager.OnDestroy()
        }
    }
}
