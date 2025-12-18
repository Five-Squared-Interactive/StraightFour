// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Voxels.Core.Meshing
{
    /// <summary>
    /// Marching Cubes job for smooth terrain voxels.
    /// Implements a basic Marching Cubes algorithm to generate smooth surfaces from density fields.
    /// 
    /// NOTE: This is a SIMPLIFIED implementation with incomplete lookup tables.
    /// Full Marching Cubes requires 256-entry edge table and 256x16 triangle table.
    /// Current implementation demonstrates the algorithm but will produce limited geometry.
    /// For production use, replace GetEdgeTable() and GetTriTable() with complete tables.
    /// </summary>
    [BurstCompile]
    public struct MarchingCubesJob : IJob
    {
        [ReadOnly] public NativeArray<VoxelData> voxels;
        [ReadOnly] public int chunkSize;
        [ReadOnly] public float isoLevel;

        public NativeList<Vector3> vertices;
        public NativeList<int> triangles;
        public NativeList<Vector3> normals;
        public NativeList<Vector2> uvs;

        public void Execute()
        {
            // Process each cube in the volume
            for (int x = 0; x < chunkSize - 1; x++)
            {
                for (int y = 0; y < chunkSize - 1; y++)
                {
                    for (int z = 0; z < chunkSize - 1; z++)
                    {
                        ProcessCube(x, y, z);
                    }
                }
            }
        }

        private void ProcessCube(int x, int y, int z)
        {
            // Get the 8 corner values of the cube (using stack allocation for Burst)
            float c0 = GetDensity(x, y, z);
            float c1 = GetDensity(x + 1, y, z);
            float c2 = GetDensity(x + 1, y, z + 1);
            float c3 = GetDensity(x, y, z + 1);
            float c4 = GetDensity(x, y + 1, z);
            float c5 = GetDensity(x + 1, y + 1, z);
            float c6 = GetDensity(x + 1, y + 1, z + 1);
            float c7 = GetDensity(x, y + 1, z + 1);

            // Determine the index into the edge table
            int cubeIndex = 0;
            if (c0 > isoLevel) cubeIndex |= 1;
            if (c1 > isoLevel) cubeIndex |= 2;
            if (c2 > isoLevel) cubeIndex |= 4;
            if (c3 > isoLevel) cubeIndex |= 8;
            if (c4 > isoLevel) cubeIndex |= 16;
            if (c5 > isoLevel) cubeIndex |= 32;
            if (c6 > isoLevel) cubeIndex |= 64;
            if (c7 > isoLevel) cubeIndex |= 128;

            // Cube is entirely in/out of the surface
            if (cubeIndex == 0 || cubeIndex == 255)
                return;

            // NOTE: This is a simplified demonstration with incomplete tables
            // In production, use full Marching Cubes lookup tables
            // For now, generate a basic triangle for demonstration
            if (cubeIndex > 0 && cubeIndex < 255)
            {
                // Simple surface approximation at cube center
                float3 cubePos = new float3(x, y, z);
                float3 center = cubePos + new float3(0.5f, 0.5f, 0.5f);
                
                // Create a simple triangle representing the surface
                // In full implementation, this would use edge interpolation and tri table
                int vertIndex = vertices.Length;
                
                vertices.Add(new Vector3(center.x, center.y, center.z));
                vertices.Add(new Vector3(center.x + 0.5f, center.y, center.z));
                vertices.Add(new Vector3(center.x, center.y, center.z + 0.5f));
                
                float3 normal = new float3(0, 1, 0);
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
                
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0.5f, 1));
                
                triangles.Add(vertIndex);
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + 2);
            }
            
            /* Full implementation would use edge vertices and triangle table:
            float3 cubePos = new float3(x, y, z);
            float3 edge0, edge1, edge2; // ... etc for all 12 edges
            
            // Interpolate vertices on edges based on edge table
            if ((edgeTable[cubeIndex] & 1) != 0)
                edge0 = VertexInterp(cubePos, new float3(0, 0, 0), new float3(1, 0, 0), c0, c1);
            // ... etc for all edges
            
            // Create triangles based on tri table
            for (int i = 0; triTable[cubeIndex, i] != -1; i += 3)
            {
                float3 v1 = edgeVertices[triTable[cubeIndex, i]];
                float3 v2 = edgeVertices[triTable[cubeIndex, i + 1]];
                float3 v3 = edgeVertices[triTable[cubeIndex, i + 2]];

                // Add vertices, normals, UVs, triangles
            }
            */
        }

        private int GetIndex(int x, int y, int z)
        {
            return x + y * chunkSize + z * chunkSize * chunkSize;
        }

        private float GetDensity(int x, int y, int z)
        {
            if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize)
                return 0f;

            return (float)voxels[GetIndex(x, y, z)].density;
        }

        private float3 VertexInterp(float3 cubePos, float3 p1, float3 p2, float val1, float val2)
        {
            if (math.abs(isoLevel - val1) < 0.00001f)
                return cubePos + p1;
            if (math.abs(isoLevel - val2) < 0.00001f)
                return cubePos + p2;
            if (math.abs(val1 - val2) < 0.00001f)
                return cubePos + p1;

            float t = (isoLevel - val1) / (val2 - val1);
            return cubePos + math.lerp(p1, p2, t);
        }


    }

    /// <summary>
    /// Wrapper class for scheduling Marching Cubes meshing jobs.
    /// </summary>
    public class MarchingCubesMesher : IMeshingJob
    {
        private float isoLevel;

        public MarchingCubesMesher(float isoLevel = 0.5f)
        {
            this.isoLevel = isoLevel;
        }

        public JobHandle Schedule(NativeArray<VoxelData> voxelData, int chunkSize, ref MeshData meshData)
        {
            var job = new MarchingCubesJob
            {
                voxels = voxelData,
                chunkSize = chunkSize,
                isoLevel = isoLevel,
                vertices = meshData.vertices,
                triangles = meshData.triangles,
                normals = meshData.normals,
                uvs = meshData.uvs
            };

            return job.Schedule();
        }
    }
}
