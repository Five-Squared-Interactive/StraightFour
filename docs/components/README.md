# Core Components

This document provides detailed information about the core components of the WebVerse World Engine and their interactions.

## Overview

The WebVerse World Engine is composed of several key components that work together to provide a comprehensive virtual world framework:

1. **StraightFour** - Main engine controller
2. **World** - World container and manager coordinator
3. **Manager System** - Specialized subsystem managers
4. **Entity System** - Game objects and components
5. **Utilities** - Common functionality and helpers

## StraightFour - Main Engine Controller

The `StraightFour` class is the primary entry point and singleton controller for the entire world engine.

### Key Features

- **Singleton Pattern**: Provides global access to engine functionality
- **World Management**: Handles world loading, unloading, and lifecycle
- **Configuration Management**: Manages engine settings and prefab references
- **VR/AR Support**: Handles VR/AR mode initialization and configuration

### Core Methods

```csharp
public class StraightFour : MonoBehaviour
{
    // Static access to the active world
    public static World.World ActiveWorld { get; }
    
    // Load a new world
    public static bool LoadWorld(string worldName, string queryParams = null)
    
    // Initialize the engine
    private void Awake()
    
    // Load query parameters
    private void LoadQueryParams(string rawParams)
}
```

### Configuration Properties

```plantuml
@startuml StraightFourProperties
!define RECTANGLE class

RECTANGLE StraightFour {
    +highlightMaterial: Material
    +previewMaterial: Material
    +skyMaterial: Material
    +liteProceduralSkyMaterial: Material
    +liteProceduralSkyObject: GameObject
    +defaultCloudTexture: Texture2D
    +defaultStarTexture: Texture2D
    +inputEntityPrefab: GameObject
    +webViewPrefab: GameObject
    +canvasWebViewPrefab: GameObject
    +characterControllerPrefab: GameObject
    +voxelPrefab: GameObject
    +waterBodyPrefab: GameObject
    +waterBlockerPrefab: GameObject
    +airplaneEntityPrefab: GameObject
    +cameraOffset: GameObject
    +vr: bool
    +crosshair: GameObject
}

@enduml
```

## World - World Container

The `World` class represents a single virtual world instance and coordinates all managers and systems.

### Architecture

```plantuml
@startuml WorldArchitecture
!define RECTANGLE class

RECTANGLE World {
    +Initialize(WorldInfo)
    +GetManager<T>(): T
    +siteName: string
}

RECTANGLE WorldInfo {
    +automobileEntityTypeMap: Dictionary
    +airplaneEntityPrefab: GameObject
    +highlightMaterial: Material
    +previewMaterial: Material
    +skyMaterial: Material
    +liteProceduralSkyMaterial: Material
    +defaultCloudTexture: Texture2D
    +defaultStarTexture: Texture2D
    +inputEntityPrefab: GameObject
    +webViewPrefab: GameObject
    +canvasWebViewPrefab: GameObject
    +characterControllerPrefab: GameObject
    +voxelPrefab: GameObject
    +waterBodyPrefab: GameObject
    +waterBlockerPrefab: GameObject
    +cameraOffset: GameObject
    +vr: bool
    +maxStorageEntries: int
    +maxEntryLength: int
    +maxKeyLength: int
    +siteName: string
}

RECTANGLE EntityManager
RECTANGLE CameraManager  
RECTANGLE EnvironmentManager
RECTANGLE MaterialManager
RECTANGLE WorldStorageManager

World --> WorldInfo : uses
World *-- EntityManager : contains
World *-- CameraManager : contains
World *-- EnvironmentManager : contains
World *-- MaterialManager : contains
World *-- WorldStorageManager : contains

@enduml
```

### Initialization Flow

```plantuml
@startuml WorldInitialization
!define RECTANGLE class

participant StraightFour
participant World
participant EntityManager
participant CameraManager
participant EnvironmentManager
participant MaterialManager
participant WorldStorageManager

StraightFour -> World: Initialize(WorldInfo)
activate World

World -> EntityManager: Initialize()
activate EntityManager
EntityManager -> World: Ready
deactivate EntityManager

World -> CameraManager: Initialize()
activate CameraManager
CameraManager -> World: Ready
deactivate CameraManager

World -> EnvironmentManager: Initialize()
activate EnvironmentManager
EnvironmentManager -> World: Ready
deactivate EnvironmentManager

World -> MaterialManager: Initialize()
activate MaterialManager
MaterialManager -> World: Ready
deactivate MaterialManager

World -> WorldStorageManager: Initialize()
activate WorldStorageManager
WorldStorageManager -> World: Ready
deactivate WorldStorageManager

World -> StraightFour: World Initialized
deactivate World

@enduml
```

## Manager System

### BaseManager

All managers inherit from `BaseManager`, providing a consistent interface and lifecycle:

```csharp
public abstract class BaseManager : MonoBehaviour
{
    protected World.World world;
    
    public abstract void Initialize();
}
```

### EntityManager

Manages all entities in the world, providing creation, retrieval, and destruction functionality.

```plantuml
@startuml EntityManagerDetail
!define RECTANGLE class

RECTANGLE EntityManager {
    +LoadMeshEntity(...)
    +LoadTerrainEntity(...)
    +LoadCharacterEntity(...)
    +LoadLightEntity(...)
    +LoadUIEntity(...)
    +LoadHTMLEntity(...)
    +LoadVoxelEntity(...)
    +GetEntity(Guid): BaseEntity
    +GetAllTopLevelEntities(): BaseEntity[]
    +DeleteEntity(Guid): bool
    -entities: Dictionary<Guid, BaseEntity>
    -GetEntityID(): Guid
}

note right of EntityManager
  Centralized entity management
  - Factory methods for all entity types
  - Lifecycle management
  - Parent-child relationships
  - Synchronization coordination
end note

@enduml
```

#### Key Methods

- **Entity Creation**: Factory methods for each entity type
- **Entity Retrieval**: Get entities by ID or query all entities
- **Entity Management**: Delete, modify, and organize entities
- **Prefab Management**: Handle entity prefab instantiation

### CameraManager

Manages the world's camera system, including VR/AR support:

```plantuml
@startuml CameraManagerDetail
!define RECTANGLE class

RECTANGLE CameraManager {
    +cam: Camera
    +vr: bool
    +cameraOffset: GameObject
    +defaultCameraParent: GameObject
    +crosshairEnabled: bool
    +SetCameraParent(Transform)
    +SetCameraPosition(Vector3)
    +SetCameraRotation(Quaternion)
    +GetCameraPosition(): Vector3
    +GetCameraRotation(): Quaternion
}

note right of CameraManager
  Camera system management
  - VR/AR mode support
  - Camera positioning
  - Crosshair management
  - Input handling
end note

@enduml
```

### EnvironmentManager

Handles environmental settings and rendering:

```plantuml
@startuml EnvironmentManagerDetail
!define RECTANGLE class

RECTANGLE EnvironmentManager {
    +skyboxMaterial: Material
    +SetSkybox(Material)
    +SetAmbientLighting(Color, float)
    +SetDirectionalLight(Transform, Color, float)
    +SetFog(bool, Color, float, float)
    +SetPostProcessing(PostProcessProfile)
}

note right of EnvironmentManager
  Environmental control
  - Skybox management
  - Lighting control
  - Weather effects
  - Post-processing
end note

@enduml
```

### MaterialManager

Manages materials and textures for the world:

```plantuml
@startuml MaterialManagerDetail
!define RECTANGLE class

RECTANGLE MaterialManager {
    +materials: Dictionary<string, Material>
    +GetMaterial(string): Material
    +LoadMaterial(MaterialInfo): Material
    +CreateMaterial(MaterialInfo): Material
    +SetMaterialProperty(string, object)
    +GetMaterialProperty(string): object
}

note right of MaterialManager
  Material system
  - Material caching
  - Dynamic material creation
  - Property management
  - Texture loading
end note

@enduml
```

### WorldStorageManager

Provides persistent storage for world data:

```plantuml
@startuml WorldStorageManagerDetail
!define RECTANGLE class

RECTANGLE WorldStorageManager {
    +maxStorageEntries: int
    +maxEntryLength: int
    +maxKeyLength: int
    +Store(string, object): bool
    +Retrieve(string): object
    +Remove(string): bool
    +GetAllKeys(): string[]
    +Clear(): bool
    +GetStorageSize(): int
}

note right of WorldStorageManager
  Persistent storage
  - Key-value storage
  - Size limitations
  - Data serialization
  - Cross-session persistence
end note

@enduml
```

## Component Interactions

### Manager Communication

```plantuml
@startuml ManagerCommunication
!define RECTANGLE class

participant EntityManager
participant CameraManager
participant EnvironmentManager
participant MaterialManager

EntityManager -> MaterialManager: GetMaterial(materialName)
activate MaterialManager
MaterialManager -> EntityManager: Material
deactivate MaterialManager

EntityManager -> CameraManager: GetCameraPosition()
activate CameraManager
CameraManager -> EntityManager: Vector3
deactivate CameraManager

EnvironmentManager -> MaterialManager: GetSkyboxMaterial()
activate MaterialManager
MaterialManager -> EnvironmentManager: Material
deactivate MaterialManager

@enduml
```

### Entity-Manager Interaction

```plantuml
@startuml EntityManagerInteraction
!define RECTANGLE class

participant Client
participant EntityManager
participant BaseEntity
participant MaterialManager
participant CameraManager

Client -> EntityManager: LoadMeshEntity(params)
activate EntityManager

EntityManager -> BaseEntity: Initialize(id)
activate BaseEntity

EntityManager -> MaterialManager: GetMaterial(materialName)
activate MaterialManager
MaterialManager -> EntityManager: Material
deactivate MaterialManager

EntityManager -> BaseEntity: SetMaterial(material)
EntityManager -> BaseEntity: SetPosition(position)
EntityManager -> BaseEntity: SetRotation(rotation)

BaseEntity -> EntityManager: Entity Ready
deactivate BaseEntity

EntityManager -> Client: Entity Created
deactivate EntityManager

@enduml
```

## Utilities System

### LogSystem

Provides centralized logging functionality:

```csharp
public static class LogSystem
{
    public static void LogInfo(string message)
    public static void LogWarning(string message) 
    public static void LogError(string message)
}
```

### Tags System

Manages entity tags for categorization and querying:

```csharp
public static class Tags
{
    public const string Player = "Player";
    public const string Environment = "Environment";
    public const string UI = "UI";
    // ... other predefined tags
}
```

## Integration Points

### Unity Integration

- **MonoBehaviour Lifecycle**: All components follow Unity's lifecycle
- **GameObject Hierarchy**: Entities maintain Unity's transform hierarchy  
- **Physics Integration**: Full integration with Unity's physics system
- **Rendering Pipeline**: Compatible with Unity's URP (Universal Render Pipeline)

### External Systems

- **WebVerse API**: Provides JavaScript APIs for world manipulation
- **VEML Support**: Virtual Environment Markup Language integration
- **VR/AR Systems**: Unity XR Toolkit integration
- **Networking**: VSS (VOS Synchronization Service) integration

## Performance Considerations

### Memory Management

- **Object Pooling**: Reuse frequently created/destroyed objects
- **Material Sharing**: Single material instances shared across entities
- **Texture Compression**: Automatic texture optimization

### Rendering Optimization  

- **Frustum Culling**: Automatic culling of off-screen objects
- **LOD System**: Level-of-detail for complex meshes
- **Batching**: Automatic draw call batching for similar objects

### Update Optimization

- **Dirty Flagging**: Only update changed properties
- **Batch Processing**: Group similar operations
- **Frame Distribution**: Spread expensive operations across frames

This component architecture provides a robust, extensible foundation for building immersive virtual worlds while maintaining high performance and ease of use.