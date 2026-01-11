// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Voxels.Core.Meshing
{
    /// <summary>
    /// Mesh data structure for job output.
    /// </summary>
    public struct MeshData
    {
        public NativeList<Vector3> vertices;
        public NativeList<int> triangles;
        public NativeList<Vector3> normals;
        public NativeList<Vector2> uvs;

        public MeshData(Allocator allocator)
        {
            vertices = new NativeList<Vector3>(allocator);
            triangles = new NativeList<int>(allocator);
            normals = new NativeList<Vector3>(allocator);
            uvs = new NativeList<Vector2>(allocator);
        }

        public void Dispose()
        {
            if (vertices.IsCreated) vertices.Dispose();
            if (triangles.IsCreated) triangles.Dispose();
            if (normals.IsCreated) normals.Dispose();
            if (uvs.IsCreated) uvs.Dispose();
        }

        /// <summary>
        /// Convert to Unity Mesh.
        /// Note: Uses ToArray() which creates temporary arrays. For production,
        /// consider using Unity's newer mesh APIs (SetVertexBufferData, SetIndexBufferData)
        /// that accept NativeArray directly to avoid GC allocations.
        /// </summary>
        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices.AsArray().ToArray());
            mesh.SetTriangles(triangles.AsArray().ToArray(), 0);
            mesh.SetNormals(normals.AsArray().ToArray());
            mesh.SetUVs(0, uvs.AsArray().ToArray());
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    /// <summary>
    /// Interface for voxel meshing jobs. Supports both blocky and smooth meshing algorithms.
    /// </summary>
    public interface IMeshingJob
    {
        /// <summary>
        /// Schedule the meshing job.
        /// </summary>
        /// <param name="voxelData">Native array of voxel data.</param>
        /// <param name="chunkSize">Size of the chunk.</param>
        /// <param name="meshData">Output mesh data.</param>
        /// <returns>Job handle for the meshing operation.</returns>
        JobHandle Schedule(NativeArray<VoxelData> voxelData, int chunkSize, ref MeshData meshData);
    }
}
