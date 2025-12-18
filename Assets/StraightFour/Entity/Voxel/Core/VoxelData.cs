// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using Unity.Mathematics;

namespace FiveSQD.StraightFour.Entity.Voxels.Core
{
    /// <summary>
    /// Core voxel data structure supporting both discrete blocky materials and density-based terrain.
    /// Uses Half (fp16) for density to balance precision and memory usage.
    /// </summary>
    public struct VoxelData
    {
        /// <summary>
        /// Density value for terrain voxels (0 = empty, 1 = solid).
        /// Uses half precision (fp16) to reduce memory footprint.
        /// For discrete voxels, density is typically 0 (empty) or 1 (solid).
        /// </summary>
        public half density;

        /// <summary>
        /// Material ID for this voxel. Determines rendering properties and behavior.
        /// 0 = Air/Empty, 1+ = Material types (stone, dirt, grass, etc.)
        /// </summary>
        public ushort materialId;

        /// <summary>
        /// Flags for extended voxel properties.
        /// Bit 0: IsSmooth (0 = blocky/discrete, 1 = smooth/terrain)
        /// Bit 1-15: Reserved for future use
        /// </summary>
        public ushort flags;

        /// <summary>
        /// Check if this voxel is solid (density >= 0.5).
        /// </summary>
        public bool IsSolid => density >= (half)0.5f;

        /// <summary>
        /// Check if this voxel is empty (density < 0.5).
        /// </summary>
        public bool IsEmpty => density < (half)0.5f;

        /// <summary>
        /// Check if this voxel should be rendered as smooth terrain.
        /// </summary>
        public bool IsSmooth => (flags & 0x0001) != 0;

        /// <summary>
        /// Create a solid blocky voxel with a material ID.
        /// </summary>
        public static VoxelData CreateSolid(ushort materialId)
        {
            return new VoxelData
            {
                density = (half)1.0f,
                materialId = materialId,
                flags = 0
            };
        }

        /// <summary>
        /// Create an empty voxel.
        /// </summary>
        public static VoxelData CreateEmpty()
        {
            return new VoxelData
            {
                density = (half)0.0f,
                materialId = 0,
                flags = 0
            };
        }

        /// <summary>
        /// Create a smooth terrain voxel with density.
        /// </summary>
        public static VoxelData CreateSmooth(float densityValue, ushort materialId)
        {
            return new VoxelData
            {
                density = (half)densityValue,
                materialId = materialId,
                flags = 0x0001 // Set smooth flag
            };
        }
    }
}
