// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Voxels.Core.Meshing
{
    /// <summary>
    /// Greedy meshing job for blocky/discrete voxels.
    /// Uses a simple face culling approach - only renders faces that are exposed to air.
    /// This is a basic implementation that can be optimized with true greedy meshing later.
    /// </summary>
    [BurstCompile]
    public struct GreedyMeshingJob : IJob
    {
        [ReadOnly] public NativeArray<VoxelData> voxels;
        [ReadOnly] public int chunkSize;

        public NativeList<Vector3> vertices;
        public NativeList<int> triangles;
        public NativeList<Vector3> normals;
        public NativeList<Vector2> uvs;

        public void Execute()
        {
            // Iterate through all voxels and generate visible faces
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        int index = GetIndex(x, y, z);
                        VoxelData voxel = voxels[index];

                        // Only generate mesh for solid, non-smooth voxels
                        if (!voxel.IsSolid || voxel.IsSmooth)
                            continue;

                        // Check each face and add if exposed
                        AddFaceIfExposed(x, y, z, new int3(0, 1, 0));  // Top
                        AddFaceIfExposed(x, y, z, new int3(0, -1, 0)); // Bottom
                        AddFaceIfExposed(x, y, z, new int3(1, 0, 0));  // Right
                        AddFaceIfExposed(x, y, z, new int3(-1, 0, 0)); // Left
                        AddFaceIfExposed(x, y, z, new int3(0, 0, 1));  // Front
                        AddFaceIfExposed(x, y, z, new int3(0, 0, -1)); // Back
                    }
                }
            }
        }

        private int GetIndex(int x, int y, int z)
        {
            return x + y * chunkSize + z * chunkSize * chunkSize;
        }

        private bool IsValidCoordinate(int x, int y, int z)
        {
            return x >= 0 && x < chunkSize &&
                   y >= 0 && y < chunkSize &&
                   z >= 0 && z < chunkSize;
        }

        private void AddFaceIfExposed(int x, int y, int z, int3 direction)
        {
            int3 neighborPos = new int3(x, y, z) + direction;
            
            // Check if neighbor is outside chunk (exposed) or empty
            bool isExposed = !IsValidCoordinate(neighborPos.x, neighborPos.y, neighborPos.z);
            
            if (!isExposed)
            {
                VoxelData neighbor = voxels[GetIndex(neighborPos.x, neighborPos.y, neighborPos.z)];
                isExposed = neighbor.IsEmpty;
            }

            if (!isExposed)
                return;

            // Add quad for this face
            AddQuad(new float3(x, y, z), direction);
        }

        private void AddQuad(float3 position, int3 normal)
        {
            int startVertex = vertices.Length;

            // Define quad vertices based on normal direction
            float3[] quadVerts = GetQuadVertices(position, normal);
            
            foreach (var vert in quadVerts)
            {
                vertices.Add(new Vector3(vert.x, vert.y, vert.z));
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
            }

            // Add UVs (simple 0-1 mapping)
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));

            // Add triangles (two triangles per quad)
            triangles.Add(startVertex + 0);
            triangles.Add(startVertex + 2);
            triangles.Add(startVertex + 1);

            triangles.Add(startVertex + 0);
            triangles.Add(startVertex + 3);
            triangles.Add(startVertex + 2);
        }

        private float3[] GetQuadVertices(float3 pos, int3 normal)
        {
            float3[] verts = new float3[4];

            if (normal.y == 1) // Top face
            {
                verts[0] = pos + new float3(0, 1, 0);
                verts[1] = pos + new float3(1, 1, 0);
                verts[2] = pos + new float3(1, 1, 1);
                verts[3] = pos + new float3(0, 1, 1);
            }
            else if (normal.y == -1) // Bottom face
            {
                verts[0] = pos + new float3(0, 0, 1);
                verts[1] = pos + new float3(1, 0, 1);
                verts[2] = pos + new float3(1, 0, 0);
                verts[3] = pos + new float3(0, 0, 0);
            }
            else if (normal.x == 1) // Right face
            {
                verts[0] = pos + new float3(1, 0, 0);
                verts[1] = pos + new float3(1, 0, 1);
                verts[2] = pos + new float3(1, 1, 1);
                verts[3] = pos + new float3(1, 1, 0);
            }
            else if (normal.x == -1) // Left face
            {
                verts[0] = pos + new float3(0, 0, 1);
                verts[1] = pos + new float3(0, 0, 0);
                verts[2] = pos + new float3(0, 1, 0);
                verts[3] = pos + new float3(0, 1, 1);
            }
            else if (normal.z == 1) // Front face
            {
                verts[0] = pos + new float3(0, 0, 1);
                verts[1] = pos + new float3(1, 0, 1);
                verts[2] = pos + new float3(1, 1, 1);
                verts[3] = pos + new float3(0, 1, 1);
            }
            else // Back face (normal.z == -1)
            {
                verts[0] = pos + new float3(1, 0, 0);
                verts[1] = pos + new float3(0, 0, 0);
                verts[2] = pos + new float3(0, 1, 0);
                verts[3] = pos + new float3(1, 1, 0);
            }

            return verts;
        }
    }

    /// <summary>
    /// Wrapper class for scheduling greedy meshing jobs.
    /// </summary>
    public class GreedyMesher : IMeshingJob
    {
        public JobHandle Schedule(NativeArray<VoxelData> voxelData, int chunkSize, ref MeshData meshData)
        {
            var job = new GreedyMeshingJob
            {
                voxels = voxelData,
                chunkSize = chunkSize,
                vertices = meshData.vertices,
                triangles = meshData.triangles,
                normals = meshData.normals,
                uvs = meshData.uvs
            };

            return job.Schedule();
        }
    }
}
