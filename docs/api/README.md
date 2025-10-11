# API Reference

This document provides comprehensive API documentation for the WebVerse World Engine, including all public classes, methods, properties, and usage examples.

## Core API Overview

The WebVerse World Engine API is organized into several namespaces:

- **FiveSQD.StraightFour** - Main engine classes
- **FiveSQD.StraightFour.World** - World management  
- **FiveSQD.StraightFour.Entity** - Entity system
- **FiveSQD.StraightFour.Camera** - Camera management
- **FiveSQD.StraightFour.Environment** - Environmental controls
- **FiveSQD.StraightFour.Utilities** - Helper classes and utilities
- **FiveSQD.StraightFour.Synchronization** - Network synchronization

## StraightFour - Main Engine API

### Class: StraightFour

Main engine controller and entry point.

#### Static Properties

```csharp
public static World.World ActiveWorld { get; }
```
Gets the currently active world instance.

#### Static Methods

```csharp
public static bool LoadWorld(string worldName, string queryParams = null)
```
**Description:** Loads a new world with the specified name and optional query parameters.

**Parameters:**
- `worldName` (string): Name of the world to load
- `queryParams` (string, optional): URL query parameters for world configuration

**Returns:** `bool` - True if world loaded successfully, false otherwise

**Example:**
```csharp
// Load a simple world
bool success = StraightFour.LoadWorld("MyWorld");

// Load world with parameters
bool success = StraightFour.LoadWorld("MyWorld", "vr=true&multiplayer=false");
```

```csharp
public static bool UnloadWorld()
```
**Description:** Unloads the currently active world.

**Returns:** `bool` - True if world unloaded successfully

#### Instance Properties

```csharp
public Material highlightMaterial { get; set; }
public Material previewMaterial { get; set; }
public Material skyMaterial { get; set; }
public bool vr { get; set; }
public GameObject crosshair { get; set; }
```

## World Management API

### Class: World

Represents a virtual world instance.

#### Properties

```csharp
public string siteName { get; private set; }
public EntityManager entityManager { get; private set; }
public CameraManager cameraManager { get; private set; }
public EnvironmentManager environmentManager { get; private set; }
public MaterialManager materialManager { get; private set; }
public WorldStorageManager worldStorageManager { get; private set; }
```

#### Methods

```csharp
public void Initialize(WorldInfo worldInfo)
```
**Description:** Initializes the world with specified configuration.

**Parameters:**
- `worldInfo` (WorldInfo): Configuration object containing world settings

```csharp
public T GetManager<T>() where T : BaseManager
```
**Description:** Gets a manager of the specified type.

**Type Parameters:**
- `T`: Manager type (EntityManager, CameraManager, etc.)

**Returns:** `T` - Manager instance

**Example:**
```csharp
EntityManager entityManager = world.GetManager<EntityManager>();
CameraManager cameraManager = world.GetManager<CameraManager>();
```

### Class: WorldInfo

Configuration object for world initialization.

#### Properties

```csharp
public Dictionary<EntityManager.AutomobileEntityType, NWH.VehiclePhysics2.StateSettings> automobileEntityTypeMap;
public GameObject airplaneEntityPrefab;
public Material highlightMaterial;
public Material previewMaterial;
public Material skyMaterial;
public Material liteProceduralSkyMaterial;
public Texture2D defaultCloudTexture;
public Texture2D defaultStarTexture;
public GameObject inputEntityPrefab;
public GameObject webViewPrefab;
public GameObject canvasWebViewPrefab;
public GameObject characterControllerPrefab;
public GameObject voxelPrefab;
public GameObject waterBodyPrefab;
public GameObject waterBlockerPrefab;
public GameObject cameraOffset;
public bool vr;
public int maxStorageEntries;
public int maxEntryLength;
public int maxKeyLength;
public string siteName;
```

## Entity Management API

### Class: EntityManager

Manages all entities in the world.

#### Entity Creation Methods

```csharp
public bool LoadMeshEntity(Mesh mesh, Material[] materials, Vector3 position, 
    Quaternion rotation, Vector3 scale, Guid? id = null, string tag = null, 
    bool isSize = false, Action onLoaded = null)
```
**Description:** Creates a mesh entity with specified properties.

**Parameters:**
- `mesh` (Mesh): 3D mesh to display
- `materials` (Material[]): Materials to apply to the mesh
- `position` (Vector3): World position
- `rotation` (Quaternion): World rotation
- `scale` (Vector3): Scale factor
- `id` (Guid?, optional): Unique identifier (auto-generated if null)
- `tag` (string, optional): Tag for categorization
- `isSize` (bool, optional): Whether scale represents size or scale factor
- `onLoaded` (Action, optional): Callback when entity is fully loaded

**Returns:** `bool` - True if creation started successfully

```csharp
public bool LoadTerrainEntity(float length, float width, float height,
    float[,] heights, TerrainEntityLayer[] layers, 
    Dictionary<int, float[,]> layerMasks, Guid id, BaseEntity parent, 
    Vector3 position, Quaternion rotation, string tag, Action onLoaded)
```
**Description:** Creates a terrain entity with heightmap and texture layers.

**Parameters:**
- `length` (float): Terrain length in world units
- `width` (float): Terrain width in world units  
- `height` (float): Maximum terrain height
- `heights` (float[,]): 2D heightmap array (0-1 values)
- `layers` (TerrainEntityLayer[]): Texture layers for terrain
- `layerMasks` (Dictionary<int, float[,]>): Alpha masks for each layer
- `id` (Guid): Unique identifier
- `parent` (BaseEntity): Parent entity
- `position` (Vector3): World position
- `rotation` (Quaternion): World rotation
- `tag` (string): Tag for categorization
- `onLoaded` (Action): Callback when terrain is generated

```csharp
public bool LoadCharacterEntity(BaseEntity parent, GameObject characterPrefab,
    Vector3 position, Quaternion rotation, Guid? id = null, string tag = null,
    Action onLoaded = null)
```
**Description:** Creates a character entity for avatars or NPCs.

```csharp
public bool LoadLightEntity(BaseEntity parentEntity, Vector3 position,
    Quaternion rotation, Guid? id = null, string tag = null, 
    LightType lightType = LightType.Point, float intensity = 1f,
    Color color = default, float range = 10f, float spotAngle = 30f,
    Action onLoaded = null)
```
**Description:** Creates a light entity for scene illumination.

```csharp
public bool LoadUIEntity(BaseEntity parent, Vector3 position, 
    Quaternion rotation, Vector2 size, Guid? id = null, string tag = null,
    RenderMode renderMode = RenderMode.WorldSpace, Action onLoaded = null)
```
**Description:** Creates a UI entity for user interface elements.

```csharp
public bool LoadHTMLEntity(BaseEntity parent, Vector3 position,
    Quaternion rotation, Vector2 size, Guid? id = null, string tag = null,
    Action onLoaded = null)
```
**Description:** Creates an HTML entity for web content display.

```csharp
public bool LoadVoxelEntity(VoxelData voxelData, Guid id, BaseEntity parent,
    Vector3 position, Quaternion rotation, string tag, Action onLoaded)
```
**Description:** Creates a voxel entity for block-based objects.

#### Entity Query Methods

```csharp
public BaseEntity GetEntity(Guid id)
```
**Description:** Retrieves an entity by its unique identifier.

**Parameters:**
- `id` (Guid): Entity identifier

**Returns:** `BaseEntity` - Entity instance or null if not found

```csharp
public BaseEntity[] GetAllTopLevelEntities()
```
**Description:** Gets all entities that have no parent.

**Returns:** `BaseEntity[]` - Array of top-level entities

```csharp
public BaseEntity[] GetAllEntities()
```
**Description:** Gets all entities in the world.

**Returns:** `BaseEntity[]` - Array of all entities

```csharp
public BaseEntity[] GetEntitiesByTag(string tag)
```
**Description:** Gets all entities with the specified tag.

**Parameters:**
- `tag` (string): Tag to search for

**Returns:** `BaseEntity[]` - Array of matching entities

#### Entity Management Methods

```csharp
public bool DeleteEntity(Guid id)
```
**Description:** Deletes an entity and all its children.

**Parameters:**
- `id` (Guid): Entity identifier to delete

**Returns:** `bool` - True if entity was deleted successfully

## Entity API

### Class: BaseEntity

Base class for all entities in the world.

#### Properties

```csharp
public Guid id { get; }
public BaseEntity parentEntity { get; }
public List<BaseEntity> childEntities { get; }
public InteractionState interactionState { get; set; }
public EntityPhysicalProperties physicalProperties { get; set; }
public EntityMotion motionState { get; set; }
public List<PlacementSocket> sockets { get; }
```

#### Methods

```csharp
public virtual void Initialize(Guid idToSet)
```
**Description:** Initializes the entity with a unique identifier.

```csharp
public bool SetParent(BaseEntity parent, bool synchronize = true)
```
**Description:** Sets the parent entity, establishing hierarchy.

**Parameters:**
- `parent` (BaseEntity): New parent entity (null for no parent)
- `synchronize` (bool): Whether to synchronize change across network

```csharp
public BaseEntity GetParent()
```
**Description:** Gets the parent entity.

**Returns:** `BaseEntity` - Parent entity or null if top-level

```csharp
public BaseEntity[] GetChildren()
```
**Description:** Gets all child entities.

**Returns:** `BaseEntity[]` - Array of child entities

```csharp
public bool SetInteractionState(InteractionState newState, bool synchronize = true)
```
**Description:** Sets the interaction state of the entity.

**Parameters:**
- `newState` (InteractionState): New interaction state
- `synchronize` (bool): Whether to synchronize change

```csharp
public bool SetPosition(Vector3 position, bool synchronize = true)
```
**Description:** Sets the world position of the entity.

```csharp
public bool SetRotation(Quaternion rotation, bool synchronize = true)
```
**Description:** Sets the world rotation of the entity.

```csharp
public bool SetScale(Vector3 scale, bool synchronize = true)
```
**Description:** Sets the scale of the entity.

### Entity-Specific APIs

#### MeshEntity

```csharp
public bool SetMesh(Mesh mesh, bool synchronize = true)
public bool SetMaterials(Material[] materials, bool synchronize = true)
// Note: Colliders are automatically created based on mesh geometry
```

#### TerrainEntity

```csharp
public bool SetHeightmap(float[,] heights, bool synchronize = true)
public bool SetTextures(TerrainEntityLayer[] layers, bool synchronize = true)
public bool SetSize(Vector3 size, bool synchronize = true)
```

#### CharacterEntity

```csharp
public bool SetAvatar(GameObject avatarPrefab, bool synchronize = true)
public bool SetAnimation(RuntimeAnimatorController controller, bool synchronize = true)
public bool Move(Vector3 direction, float speed = 1f)
```

#### LightEntity

```csharp
public bool SetLightType(LightType type, bool synchronize = true)
public bool SetIntensity(float intensity, bool synchronize = true)
public bool SetColor(Color color, bool synchronize = true)
public bool SetRange(float range, bool synchronize = true)
public bool SetSpotAngle(float angle, bool synchronize = true)
```

#### HTMLEntity

```csharp
public bool LoadURL(string url)
public bool LoadHTML(string html)
public bool ExecuteJavaScript(string logic, Action<string> onComplete = null)
public bool SetSize(Vector2 size, bool synchronize = true)
```

#### VoxelEntity

```csharp
public bool SetVoxelData(VoxelData data, bool synchronize = true)
public bool ModifyVoxel(int x, int y, int z, BlockType blockType, bool synchronize = true)
public bool GenerateMesh()
```

## Camera Management API

### Class: CameraManager

Manages the world's camera system.

#### Properties

```csharp
public Camera cam { get; }
public bool vr { get; set; }
public GameObject cameraOffset { get; }
public bool crosshairEnabled { get; set; }
```

#### Methods

```csharp
public bool SetCameraParent(Transform parent, bool synchronize = true)
```
**Description:** Sets the camera's parent transform.

```csharp
public bool SetCameraPosition(Vector3 position, bool synchronize = true)
```
**Description:** Sets the camera's world position.

```csharp
public bool SetCameraRotation(Quaternion rotation, bool synchronize = true)
```
**Description:** Sets the camera's world rotation.

```csharp
public Vector3 GetCameraPosition()
```
**Description:** Gets the current camera position.

```csharp
public Quaternion GetCameraRotation()
```
**Description:** Gets the current camera rotation.

## Environment Management API

### Class: EnvironmentManager

Manages environmental settings and rendering.

#### Methods

```csharp
public bool SetSkybox(Material skyboxMaterial, bool synchronize = true)
```
**Description:** Sets the skybox material.

```csharp
public bool SetAmbientLighting(Color color, float intensity, bool synchronize = true)
```
**Description:** Sets ambient lighting color and intensity.

```csharp
public bool SetDirectionalLight(Transform lightTransform, Color color, float intensity, bool synchronize = true)
```
**Description:** Configures the main directional light (sun).

```csharp
public bool SetFog(bool enabled, Color color, float startDistance, float endDistance, bool synchronize = true)
```
**Description:** Configures atmospheric fog settings.

## Material Management API

### Class: MaterialManager

Manages materials and textures.

#### Methods

```csharp
public Material GetMaterial(string materialName)
```
**Description:** Gets a material by name from the cache.

```csharp
public Material LoadMaterial(MaterialInfo materialInfo)
```
**Description:** Loads a new material from configuration.

```csharp
public bool SetMaterialProperty(string materialName, string propertyName, object value)
```
**Description:** Sets a property on a material.

## World Storage API

### Class: WorldStorageManager

Provides persistent storage for world data.

#### Methods

```csharp
public bool Store(string key, object value)
```
**Description:** Stores a value with the specified key.

**Parameters:**
- `key` (string): Storage key (max length configured in WorldInfo)
- `value` (object): Value to store (serializable)

**Returns:** `bool` - True if stored successfully

```csharp
public T Retrieve<T>(string key)
```
**Description:** Retrieves a value by key.

**Parameters:**
- `key` (string): Storage key

**Returns:** `T` - Retrieved value or default if not found

```csharp
public bool Remove(string key)
```
**Description:** Removes a stored value.

```csharp
public string[] GetAllKeys()
```
**Description:** Gets all storage keys.

```csharp
public bool Clear()
```
**Description:** Clears all stored data.

## Synchronization API

### Class: BaseSynchronizer

Base class for entity synchronization.

#### Methods

```csharp
public void SendUpdate(string property, object value)
```
**Description:** Sends a property update across the network.

```csharp
public void ReceiveUpdate(string property, object value)
```
**Description:** Receives and applies a property update from the network.

## Utilities API

### Class: LogSystem

Centralized logging system.

#### Static Methods

```csharp
public static void LogInfo(string message)
public static void LogWarning(string message)  
public static void LogError(string message)
```

### Class: Tags

Predefined entity tags.

#### Static Properties

```csharp
public static readonly string Player = "Player";
public static readonly string Environment = "Environment";
public static readonly string UI = "UI";
public static readonly string Vehicle = "Vehicle";
public static readonly string Building = "Building";
public static readonly string Decoration = "Decoration";
```

## Data Structures

### Enumerations

```csharp
public enum InteractionState
{
    Hidden,    // Invisible and non-interactable
    Static,    // Visible but non-interactable  
    Physical,  // Visible and fully interactable
    Placing    // In placement/editing mode
}

public enum LightType
{
    Directional,
    Point, 
    Spot,
    Area
}

public enum RenderMode
{
    ScreenSpaceOverlay,
    ScreenSpaceCamera,
    WorldSpace
}


```

### Structures

```csharp
public struct EntityPhysicalProperties
{
    public float? angularDrag;
    public Vector3? centerOfMass;
    public float? drag;
    public bool? gravitational;
    public float? mass;
}

public struct EntityMotion
{
    public Vector3? angularVelocity;
    public Vector3? velocity;
    public bool? stationary;
}
```

## Error Handling

All API methods follow consistent error handling patterns:

- **Return Values**: Most methods return `bool` indicating success/failure
- **Null Checks**: Methods validate input parameters and return false for invalid inputs
- **Logging**: Errors are automatically logged using LogSystem
- **Exceptions**: Critical errors may throw exceptions (documented per method)

## Thread Safety

- **Main Thread**: All API calls must be made from Unity's main thread
- **Synchronization**: Network synchronization handles thread safety internally
- **Async Operations**: Long-running operations use Unity coroutines

## Performance Considerations

- **Batch Operations**: Group multiple API calls when possible
- **Synchronization**: Use `synchronize=false` parameter for local-only changes
- **Memory Management**: Dispose of large objects when no longer needed
- **Update Frequency**: Limit high-frequency updates to avoid performance issues