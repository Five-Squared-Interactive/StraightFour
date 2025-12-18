# Voxel Engine Core Architecture

## Overview
This document describes the architecture and design decisions for the foundational voxel engine system supporting generic (blocky), terrain (density-based), and smoothed voxels.

## Core Components

### 1. VoxelData Structure
**File**: `VoxelData.cs`

A lightweight data structure representing individual voxels:
- **Density** (Half/fp16): 0.0 = empty, 1.0 = solid, values between for smooth terrain
- **Material ID** (ushort): Identifies the voxel material type (0 = air, 1+ = materials)
- **Flags** (ushort): Bitfield for extended properties (bit 0 = IsSmooth flag)

**Design Decision**: Using `Half` (fp16) for density balances precision with memory efficiency. A 32³ chunk uses ~68KB for density alone with fp16 vs ~128KB with fp32.

### 2. ChunkData Class
**File**: `ChunkData.cs`

Manages a 3D grid of voxels with spatial organization:
- **Chunk Coordinates**: Integer 3D position in chunk space
- **Voxel Storage**: Flat array indexed as `x + y * ChunkSize + z * ChunkSize²`
- **Mesh References**: Links to Unity MeshFilter, MeshCollider, and generated Mesh
- **Dirty Flag**: Tracks when chunk needs remeshing

**Design Decision**: Default chunk size is 32³ voxels (32,768 voxels per chunk). This balances:
- Memory usage (~134KB per chunk with full data)
- Meshing granularity for performance
- Culling efficiency
- Common in voxel engines (Minecraft uses 16³, many modern engines use 32³)

**Indexing Helper**: 
- `GetIndex(x, y, z)` converts 3D coordinates to flat array index
- `GetCoordinates(index)` converts flat index back to 3D coordinates
- `IsValidCoordinate(x, y, z)` bounds checking

### 3. Meshing System
**Files**: `IMeshingJob.cs`, `GreedyMeshingJob.cs`, `MarchingCubesJob.cs`

Interface-based meshing system supporting multiple algorithms:

#### IMeshingJob Interface
Defines contract for meshing implementations:
- `Schedule()` method returns `JobHandle` for async execution
- Works with `NativeArray<VoxelData>` for thread-safe access
- Outputs to `MeshData` structure (vertices, triangles, normals, UVs)

#### GreedyMeshingJob (Blocky Voxels)
**Algorithm**: Face culling approach
- Only generates faces exposed to air
- Each solid, non-smooth voxel gets up to 6 faces
- Uses Unity Jobs + Burst compilation for performance
- **Future Enhancement**: True greedy meshing to merge adjacent faces

**Design Decision**: Starting with simple face culling provides correct results and good performance. True greedy meshing can be added later without changing the interface.

#### MarchingCubesJob (Smooth Terrain)
**Algorithm**: Marching Cubes for smooth surfaces
- Processes each cube in the voxel grid
- Generates triangles based on density field
- Configurable iso-level (default 0.5)
- Uses Unity Jobs + Burst compilation

**Design Decision**: Marching Cubes is the industry standard for smooth voxel terrain. While the current implementation includes simplified lookup tables, it demonstrates the approach and can be expanded with full tables.

**Alternative Considered**: Dual Contouring provides better sharp feature preservation but is more complex. MC is sufficient for initial implementation.

### 4. ChunkManager
**File**: `ChunkManager.cs`

Orchestrates chunk lifecycle and mesh generation:
- **Chunk Dictionary**: Spatial hash of loaded chunks
- **Meshing Queue**: FIFO queue for chunks needing remeshing
- **Job Scheduling**: Manages one meshing job at a time (can be parallelized later)
- **Mesh Assignment**: Links generated meshes to Unity components

**Design Decision**: Sequential job processing (one chunk at a time) simplifies initial implementation. The architecture supports parallel processing - just replace single `JobHandle` with `NativeList<JobHandle>`.

**Coordinate Conversion**:
- `WorldToChunkCoordinates()`: Global voxel position → chunk coordinates
- `WorldToLocalCoordinates()`: Global position → local position within chunk

## Performance Characteristics

### Memory Usage (per 32³ chunk)
- VoxelData array: ~131KB (32,768 voxels × 4 bytes each)
- Generated mesh: Varies (blocky: ~100KB typical, smooth: ~200KB typical)
- Total: ~250-350KB per chunk

### Threading Model
- **Main Thread**: ChunkManager.Update() checks job completion, assigns meshes
- **Job Thread**: All meshing computation (Burst-compiled)
- **No locks needed**: Jobs use NativeArray for thread-safe data access

### Scalability
Current implementation targets:
- 10-50 active chunks in view (320K-1.6M voxels)
- 1-2 chunks meshed per frame (60 FPS)
- Can be improved with:
  - Parallel job execution
  - LOD system (different chunk sizes at distance)
  - Chunk streaming (load/unload based on camera)

## Extensibility Points

### 1. Additional Meshing Algorithms
Implement `IMeshingJob` interface:
```csharp
public class CustomMesher : IMeshingJob
{
    public JobHandle Schedule(NativeArray<VoxelData> voxelData, 
                              int chunkSize, 
                              ref MeshData meshData)
    {
        // Custom algorithm
    }
}
```

### 2. LOD System
- Add `ChunkLOD` class with multiple mesh resolutions
- Modify `ChunkManager` to select LOD based on distance
- Use different chunk sizes (16³, 32³, 64³)

### 3. Streaming System
- Add `ChunkStreamer` component
- Load/unload chunks based on camera position
- Save/load chunk data to disk

### 4. Voxel Editing
- `ChunkManager.SetVoxel()` already supports editing
- Add undo/redo by storing delta operations
- Multi-voxel operations (fill, copy, paste)

### 5. Material System
- Expand `materialId` to index into material database
- Add texture arrays for material rendering
- Implement material blending for smooth terrain

## Integration with Existing System

The core voxel system is **additive** to the existing `VoxelEntity` system:
- Old system: GameObject-based, per-face rendering, editor-friendly
- New system: Data-oriented, chunk-based, runtime-optimized
- Both can coexist: Use `VoxelEntity` for small, detailed objects; use Core for large terrain

Future work could integrate them by having `VoxelEntity` use `ChunkManager` internally.

## Build Requirements

### Unity Packages Required
- **Unity.Burst** (1.8.12+): For high-performance job compilation
- **Unity.Mathematics** (included): For math types (float3, int3, half)
- **Unity.Collections** (included): For NativeArray, NativeList

### Assembly Definition
The system is part of the `FiveSQD.StraightFour` assembly and has access to:
- Unity.Burst
- Unity.Jobs
- Unity.Collections
- Unity.Mathematics

### Burst Compilation
All job structs are marked with `[BurstCompile]` attribute. Verify compilation:
1. Open **Jobs → Burst → Inspector**
2. Enter Play Mode
3. Check that `GreedyMeshingJob` and `MarchingCubesJob` show as compiled

## Testing Strategy

### Unit Tests (Recommended)
1. **VoxelData Tests**: Create solid, empty, smooth voxels; verify flags
2. **ChunkData Tests**: Set/get voxels, indexing helpers, bounds checking
3. **Meshing Tests**: Generate simple geometry, verify vertex/triangle counts
4. **ChunkManager Tests**: Create chunks, coordinate conversions, meshing queue

### Manual Testing
1. Create a GameObject with `ChunkManager` component
2. Assign a material to `ChunkMaterial` field
3. In code, populate chunks with test data
4. Verify meshes generate and render correctly

### Performance Testing
- Profile with Unity Profiler
- Check Job thread utilization (should be ~100% during meshing)
- Verify no GC allocations per frame (NativeContainers only)

## Future Enhancements

### High Priority
1. **Full Marching Cubes Tables**: Complete 256-entry lookup tables
2. **True Greedy Meshing**: Merge adjacent faces to reduce triangle count
3. **Texture Atlas**: Support multiple materials in single mesh

### Medium Priority
4. **LOD System**: Different resolutions based on distance
5. **Chunk Serialization**: Save/load to disk
6. **Ambient Occlusion**: Basic lighting for blocky voxels

### Low Priority (Framework Exists)
7. **Dual Contouring**: Better sharp features for smooth terrain
8. **Chunk Streaming**: Dynamic load based on player position
9. **GPU Meshing**: Compute shader-based mesh generation

## References
- **Marching Cubes**: Lorensen & Cline, 1987
- **Greedy Meshing**: Robert O'Leary, 2013
- **Unity Jobs**: https://docs.unity3d.com/Manual/JobSystem.html
- **Burst Compiler**: https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/
