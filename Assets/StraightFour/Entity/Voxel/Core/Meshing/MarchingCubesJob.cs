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

        // Marching cubes lookup tables (simplified version)
        private static readonly int[,] edgeTable = GetEdgeTable();
        private static readonly int[,] triTable = GetTriTable();

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
            // Get the 8 corner values of the cube
            float[] corners = new float[8];
            corners[0] = GetDensity(x, y, z);
            corners[1] = GetDensity(x + 1, y, z);
            corners[2] = GetDensity(x + 1, y, z + 1);
            corners[3] = GetDensity(x, y, z + 1);
            corners[4] = GetDensity(x, y + 1, z);
            corners[5] = GetDensity(x + 1, y + 1, z);
            corners[6] = GetDensity(x + 1, y + 1, z + 1);
            corners[7] = GetDensity(x, y + 1, z + 1);

            // Determine the index into the edge table
            int cubeIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (corners[i] > isoLevel)
                    cubeIndex |= (1 << i);
            }

            // Cube is entirely in/out of the surface
            if (cubeIndex == 0 || cubeIndex == 255)
                return;

            // Generate vertices on edges
            float3[] edgeVertices = new float3[12];
            float3 cubePos = new float3(x, y, z);

            if ((edgeTable[cubeIndex, 0] & 1) != 0)
                edgeVertices[0] = VertexInterp(cubePos, new float3(0, 0, 0), new float3(1, 0, 0), corners[0], corners[1]);
            if ((edgeTable[cubeIndex, 0] & 2) != 0)
                edgeVertices[1] = VertexInterp(cubePos, new float3(1, 0, 0), new float3(1, 0, 1), corners[1], corners[2]);
            if ((edgeTable[cubeIndex, 0] & 4) != 0)
                edgeVertices[2] = VertexInterp(cubePos, new float3(1, 0, 1), new float3(0, 0, 1), corners[2], corners[3]);
            if ((edgeTable[cubeIndex, 0] & 8) != 0)
                edgeVertices[3] = VertexInterp(cubePos, new float3(0, 0, 1), new float3(0, 0, 0), corners[3], corners[0]);
            if ((edgeTable[cubeIndex, 0] & 16) != 0)
                edgeVertices[4] = VertexInterp(cubePos, new float3(0, 1, 0), new float3(1, 1, 0), corners[4], corners[5]);
            if ((edgeTable[cubeIndex, 0] & 32) != 0)
                edgeVertices[5] = VertexInterp(cubePos, new float3(1, 1, 0), new float3(1, 1, 1), corners[5], corners[6]);
            if ((edgeTable[cubeIndex, 0] & 64) != 0)
                edgeVertices[6] = VertexInterp(cubePos, new float3(1, 1, 1), new float3(0, 1, 1), corners[6], corners[7]);
            if ((edgeTable[cubeIndex, 0] & 128) != 0)
                edgeVertices[7] = VertexInterp(cubePos, new float3(0, 1, 1), new float3(0, 1, 0), corners[7], corners[4]);
            if ((edgeTable[cubeIndex, 0] & 256) != 0)
                edgeVertices[8] = VertexInterp(cubePos, new float3(0, 0, 0), new float3(0, 1, 0), corners[0], corners[4]);
            if ((edgeTable[cubeIndex, 0] & 512) != 0)
                edgeVertices[9] = VertexInterp(cubePos, new float3(1, 0, 0), new float3(1, 1, 0), corners[1], corners[5]);
            if ((edgeTable[cubeIndex, 0] & 1024) != 0)
                edgeVertices[10] = VertexInterp(cubePos, new float3(1, 0, 1), new float3(1, 1, 1), corners[2], corners[6]);
            if ((edgeTable[cubeIndex, 0] & 2048) != 0)
                edgeVertices[11] = VertexInterp(cubePos, new float3(0, 0, 1), new float3(0, 1, 1), corners[3], corners[7]);

            // Create triangles based on the tri table
            for (int i = 0; triTable[cubeIndex, i] != -1; i += 3)
            {
                int vertIndex = vertices.Length;

                float3 v1 = edgeVertices[triTable[cubeIndex, i]];
                float3 v2 = edgeVertices[triTable[cubeIndex, i + 1]];
                float3 v3 = edgeVertices[triTable[cubeIndex, i + 2]];

                vertices.Add(new Vector3(v1.x, v1.y, v1.z));
                vertices.Add(new Vector3(v2.x, v2.y, v2.z));
                vertices.Add(new Vector3(v3.x, v3.y, v3.z));

                // Calculate normal from triangle
                float3 normal = math.normalize(math.cross(v2 - v1, v3 - v1));
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
                normals.Add(new Vector3(normal.x, normal.y, normal.z));

                // Simple UV mapping
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0.5f, 1));

                triangles.Add(vertIndex);
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + 2);
            }
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

        // Simplified edge table (only stores edge bits for each cube config)
        private static int[,] GetEdgeTable()
        {
            // This is a minimal implementation - full table would have 256 entries
            // For demonstration, returning a small subset
            int[,] table = new int[256, 1];
            // In a full implementation, this would contain the complete Marching Cubes edge table
            // For now, we'll populate key configurations
            table[0, 0] = 0;
            table[255, 0] = 0;
            // Add more entries as needed
            for (int i = 1; i < 255; i++)
            {
                table[i, 0] = 0xFFF; // All edges active (simplified)
            }
            return table;
        }

        // Simplified triangle table
        private static int[,] GetTriTable()
        {
            // This is a minimal implementation - full table would have 256x16 entries
            int[,] table = new int[256, 16];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    table[i, j] = -1; // -1 indicates end of triangles
                }
            }
            // In a full implementation, this would contain the complete Marching Cubes triangle table
            // For basic demonstration, we'll add a simple case
            // Example: cube config 1 (only corner 0 is inside)
            table[1, 0] = 0; table[1, 1] = 8; table[1, 2] = 3;
            return table;
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
