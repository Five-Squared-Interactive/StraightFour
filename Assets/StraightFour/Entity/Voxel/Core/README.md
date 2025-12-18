# Voxel Core System - Usage Guide

## Quick Start

### 1. Setup ChunkManager
Add a `ChunkManager` component to a GameObject in your scene:

```csharp
GameObject managerObj = new GameObject("ChunkManager");
ChunkManager chunkManager = managerObj.AddComponent<ChunkManager>();
chunkManager.DefaultChunkSize = 32; // Optional, default is 32
chunkManager.ChunkMaterial = yourMaterial; // Assign a material
```

### 2. Create Voxels

#### Blocky/Discrete Voxels
```csharp
VoxelData blockVoxel = VoxelData.CreateSolid(materialId: 1);
chunkManager.SetVoxel(new int3(x, y, z), blockVoxel);
```

#### Smooth Terrain Voxels
```csharp
VoxelData smoothVoxel = VoxelData.CreateSmooth(density: 0.7f, materialId: 2);
chunkManager.SetVoxel(new int3(x, y, z), smoothVoxel);
```

#### Empty Voxels
```csharp
VoxelData emptyVoxel = VoxelData.CreateEmpty();
chunkManager.SetVoxel(new int3(x, y, z), emptyVoxel);
```

### 3. Mesh Generation
Mesh generation happens automatically:
- When voxels are set via `SetVoxel()`, chunks are marked dirty
- `ChunkManager` processes dirty chunks in its `Update()` loop
- Jobs run on background threads with Burst compilation
- Meshes are assigned to MeshFilter/MeshCollider when complete

## Example Scenes

### VoxelCoreExample
The included `VoxelCoreExample.cs` script demonstrates three demo types:

1. **BlockyTerrain**: Stepped terrain with discrete voxels
2. **SmoothTerrain**: Rolling hills using density fields and Marching Cubes
3. **Mixed**: Combination of blocky foundation and smooth hills

To use:
1. Create an empty GameObject
2. Add `VoxelCoreExample` component
3. Assign a material to the `voxelMaterial` field
4. Select a `DemoType` in the inspector
5. Run the scene

## API Reference

### VoxelData
```csharp
// Static factory methods
VoxelData.CreateSolid(ushort materialId)
VoxelData.CreateEmpty()
VoxelData.CreateSmooth(float density, ushort materialId)

// Properties
bool IsSolid    // density >= 0.5
bool IsEmpty    // density < 0.5
bool IsSmooth   // flag bit 0 set
```

### ChunkData
```csharp
// Constructor
ChunkData(int3 chunkCoordinates, int chunkSize = 32)

// Voxel access
VoxelData GetVoxel(int x, int y, int z)
void SetVoxel(int x, int y, int z, VoxelData voxel)

// Utility
int GetIndex(int x, int y, int z)
int3 GetCoordinates(int index)
bool IsValidCoordinate(int x, int y, int z)
void Fill(VoxelData voxel)
```

### ChunkManager
```csharp
// Chunk access
ChunkData GetOrCreateChunk(int3 chunkCoordinates)
ChunkData GetChunk(int3 chunkCoordinates)

// Voxel access (world coordinates)
void SetVoxel(int3 worldPosition, VoxelData voxel)
VoxelData GetVoxel(int3 worldPosition)

// Chunk management
void RequestChunkMesh(ChunkData chunk)
void UnloadChunk(int3 chunkCoordinates)
int GetChunkCount()

// Settings
int DefaultChunkSize
Material ChunkMaterial
```

## Performance Tips

### 1. Batch Voxel Updates
```csharp
// BAD: Sets voxels one at a time, triggering multiple remeshes
for (int i = 0; i < 1000; i++)
{
    chunkManager.SetVoxel(positions[i], voxels[i]);
}

// GOOD: Get chunk once, set multiple voxels
ChunkData chunk = chunkManager.GetOrCreateChunk(chunkCoords);
chunk.IsDirty = false; // Prevent auto-remesh
for (int i = 0; i < 1000; i++)
{
    chunk.SetVoxel(localX, localY, localZ, voxel);
}
chunk.IsDirty = true;
chunkManager.RequestChunkMesh(chunk);
```

### 2. Chunk Size Selection
- **Smaller (16³)**: Faster meshing, more draw calls, better culling
- **Larger (64³)**: Slower meshing, fewer draw calls, worse culling
- **Default (32³)**: Good balance for most scenarios

### 3. Material Optimization
- Use GPU instancing on your chunk material
- Use texture atlases for multiple material types
- Consider single material for all chunks to reduce state changes

## Extending the System

### Custom Meshing Algorithm
```csharp
public class CustomMesher : IMeshingJob
{
    public JobHandle Schedule(NativeArray<VoxelData> voxelData, 
                              int chunkSize, 
                              ref MeshData meshData)
    {
        var job = new CustomMeshingJob
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

[BurstCompile]
public struct CustomMeshingJob : IJob
{
    [ReadOnly] public NativeArray<VoxelData> voxels;
    [ReadOnly] public int chunkSize;
    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector3> normals;
    public NativeList<Vector2> uvs;

    public void Execute()
    {
        // Your meshing algorithm here
    }
}
```

### LOD System
```csharp
public class LODChunkManager : ChunkManager
{
    public float[] lodDistances = { 50f, 100f, 200f };
    
    protected override void Update()
    {
        base.Update();
        UpdateLODs();
    }
    
    private void UpdateLODs()
    {
        // Select appropriate mesher based on camera distance
        // Regenerate chunks that changed LOD level
    }
}
```

## Troubleshooting

### Meshes Not Appearing
1. Check that `ChunkMaterial` is assigned
2. Verify voxels are being set with `IsSolid = true`
3. Ensure camera can see the chunks
4. Check Unity console for errors

### Poor Performance
1. Reduce number of active chunks
2. Use smaller chunk size for faster meshing
3. Batch voxel updates instead of individual sets
4. Check Burst compilation is enabled (Jobs → Burst → Enable Compilation)

### Burst Compilation Issues
1. Open **Jobs → Burst → Inspector** in Unity menu
2. Enter Play Mode
3. Verify `GreedyMeshingJob` and `MarchingCubesJob` show as compiled
4. If not, check for compilation errors in Console

### Memory Issues
1. Unload distant chunks with `UnloadChunk()`
2. Reduce chunk size
3. Implement chunk streaming to load/unload on demand

## See Also
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture and design decisions
- [Unity Jobs Documentation](https://docs.unity3d.com/Manual/JobSystem.html)
- [Unity Burst Documentation](https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/)
