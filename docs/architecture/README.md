# Architecture Overview

This document provides a comprehensive overview of the WebVerse World Engine architecture, including system components, data flows, and design patterns.

## High-Level Architecture

The WebVerse World Engine follows a modular, component-based architecture built on Unity's GameObject/MonoBehaviour system. The engine is organized into several key subsystems:

```plantuml
@startuml HighLevelArchitecture
!define RECTANGLE class

package "WebVerse World Engine" {
    RECTANGLE StraightFour {
        +LoadWorld()
        +UnloadWorld()
        +Initialize()
    }
    
    package "World Management" {
        RECTANGLE World {
            +Initialize()
            +GetManager<T>()
        }
        
        RECTANGLE WorldInfo {
            +siteName: string
            +vr: bool
            +maxStorageEntries: int
        }
    }
    
    package "Core Managers" {
        RECTANGLE EntityManager {
            +LoadEntity()
            +GetEntity()
            +DeleteEntity()
        }
        
        RECTANGLE CameraManager {
            +SetCameraParent()
            +SetCameraPosition()
        }
        
        RECTANGLE EnvironmentManager {
            +SetSkybox()
            +SetLighting()
        }
        
        RECTANGLE MaterialManager {
            +GetMaterial()
            +LoadMaterial()
        }
        
        RECTANGLE WorldStorageManager {
            +Store()
            +Retrieve()
        }
    }
    
    package "Entity System" {
        RECTANGLE BaseEntity {
            +Initialize()
            +SetParent()
            +SetPosition()
        }
        
        RECTANGLE MeshEntity
        RECTANGLE TerrainEntity
        RECTANGLE CharacterEntity
        RECTANGLE LightEntity
        RECTANGLE UIEntity
        RECTANGLE VoxelEntity
    }
    
    package "Synchronization" {
        RECTANGLE BaseSynchronizer {
            +SendUpdate()
            +ReceiveUpdate()
        }
    }
    
    package "Utilities" {
        RECTANGLE LogSystem {
            +LogInfo()
            +LogWarning()
            +LogError()
        }
        
        RECTANGLE BaseManager {
            +Initialize()
        }
    }
}

' Relationships
StraightFour --> World : creates/manages
World --> EntityManager : contains
World --> CameraManager : contains
World --> EnvironmentManager : contains
World --> MaterialManager : contains
World --> WorldStorageManager : contains

EntityManager --> BaseEntity : manages
BaseEntity <|-- MeshEntity
BaseEntity <|-- TerrainEntity
BaseEntity <|-- CharacterEntity
BaseEntity <|-- LightEntity
BaseEntity <|-- UIEntity
BaseEntity <|-- VoxelEntity

BaseEntity --> BaseSynchronizer : uses
BaseManager <|-- EntityManager
BaseManager <|-- CameraManager
BaseManager <|-- EnvironmentManager
BaseManager <|-- MaterialManager
BaseManager <|-- WorldStorageManager

@enduml
```

## System Components

### 1. StraightFour (Main Engine Controller)

The `StraightFour` class serves as the primary entry point and controller for the world engine. It manages:

- World loading and unloading
- Engine initialization
- Configuration management
- Query parameter processing

**Key Responsibilities:**
- Initialize engine subsystems
- Load and manage world instances
- Handle VR/AR mode configuration
- Manage prefab references for entity creation

### 2. World Management System

The world management system handles the lifecycle of virtual worlds:

```plantuml
@startuml WorldLifecycle
!define RECTANGLE class

participant Client
participant StraightFour
participant World
participant Managers

Client -> StraightFour: LoadWorld(worldName)
activate StraightFour

StraightFour -> World: new World()
activate World

StraightFour -> World: Initialize(WorldInfo)

World -> Managers: Initialize all managers
activate Managers

Managers -> World: Ready
deactivate Managers

World -> StraightFour: World Ready
deactivate World

StraightFour -> Client: World Loaded
deactivate StraightFour

@enduml
```

### 3. Entity System Architecture

The entity system uses a hierarchical component-based architecture:

```plantuml
@startuml EntityHierarchy
!define RECTANGLE class

RECTANGLE BaseEntity {
    #id: Guid
    #parentEntity: BaseEntity
    #childEntities: List<BaseEntity>
    +Initialize(Guid)
    +SetParent(BaseEntity)
    +GetChildren(): BaseEntity[]
    +SetInteractionState(InteractionState)
}

RECTANGLE MeshEntity {
    +SetMesh(Mesh)
    +SetMaterial(Material)
    +SetCollider(ColliderType)
}

RECTANGLE TerrainEntity {
    +SetHeightmap(float[,])
    +SetTextures(TerrainLayer[])
    +SetSize(Vector3)
}

RECTANGLE CharacterEntity {
    +SetController(CharacterController)
    +SetAvatar(GameObject)
    +SetAnimation(AnimationController)
}

RECTANGLE LightEntity {
    +SetLightType(LightType)
    +SetIntensity(float)
    +SetColor(Color)
}

RECTANGLE UIEntity {
    +SetCanvas(Canvas)
    +SetRenderMode(RenderMode)
}

RECTANGLE HTMLEntity {
    +LoadURL(string)
    +LoadHTML(string)
    +ExecuteJavaScript(string)
}

RECTANGLE VoxelEntity {
    +SetVoxelData(VoxelData)
    +SetBlockType(BlockType)
    +GenerateMesh()
}

BaseEntity <|-- MeshEntity
BaseEntity <|-- TerrainEntity
BaseEntity <|-- CharacterEntity
BaseEntity <|-- LightEntity
BaseEntity <|-- UIEntity
UIEntity <|-- HTMLEntity
BaseEntity <|-- VoxelEntity

@enduml
```

### 4. Manager System

All major subsystems inherit from `BaseManager` and follow a consistent initialization pattern:

```plantuml
@startuml ManagerPattern
!define RECTANGLE class

RECTANGLE BaseManager {
    +Initialize()
    #world: World
}

RECTANGLE EntityManager {
    +LoadEntity<T>()
    +GetEntity(Guid): BaseEntity
    +GetAllEntities(): BaseEntity[]
    +DeleteEntity(Guid)
    -entities: Dictionary<Guid, BaseEntity>
}

RECTANGLE CameraManager {
    +SetCameraParent(Transform)
    +SetCameraPosition(Vector3)
    +SetCameraRotation(Quaternion)
    +cam: Camera
    +vr: bool
}

RECTANGLE EnvironmentManager {
    +SetSkybox(Material)
    +SetAmbientLight(Color)
    +SetDirectionalLight(Light)
    +SetFog(bool, Color, float, float)
}

RECTANGLE MaterialManager {
    +GetMaterial(string): Material
    +LoadMaterial(MaterialInfo): Material
    +materials: Dictionary<string, Material>
}

RECTANGLE WorldStorageManager {
    +Store(string, object)
    +Retrieve(string): object
    +maxStorageEntries: int
    +maxEntryLength: int
}

BaseManager <|-- EntityManager
BaseManager <|-- CameraManager
BaseManager <|-- EnvironmentManager
BaseManager <|-- MaterialManager
BaseManager <|-- WorldStorageManager

@enduml
```

## Data Flow Architecture

### Entity Creation Flow

```plantuml
@startuml EntityCreationFlow
!define RECTANGLE class

participant Client
participant EntityManager
participant Prefab
participant BaseEntity
participant World

Client -> EntityManager: LoadMeshEntity(params)
activate EntityManager

EntityManager -> Prefab: Instantiate(meshPrefab)
activate Prefab

Prefab -> BaseEntity: new MeshEntity()
activate BaseEntity

EntityManager -> BaseEntity: Initialize(id)
BaseEntity -> BaseEntity: SetupComponent()

EntityManager -> BaseEntity: SetParent(parent)
EntityManager -> BaseEntity: SetPosition(position)
EntityManager -> BaseEntity: SetRotation(rotation)

BaseEntity -> World: RegisterEntity(this)
activate World

World -> EntityManager: Entity Registered
deactivate World

BaseEntity -> EntityManager: Entity Ready
deactivate BaseEntity

Prefab -> EntityManager: Entity Created
deactivate Prefab

EntityManager -> Client: Entity Loaded
deactivate EntityManager

@enduml
```

### Synchronization Flow

```plantuml
@startuml SynchronizationFlow
!define RECTANGLE class

participant BaseEntity
participant BaseSynchronizer
participant NetworkLayer
participant RemoteClients

BaseEntity -> BaseEntity: Update Properties
BaseEntity -> BaseSynchronizer: SendUpdate(changes)
activate BaseSynchronizer

BaseSynchronizer -> NetworkLayer: BroadcastUpdate(entityId, changes)
activate NetworkLayer

NetworkLayer -> RemoteClients: EntityUpdate(entityId, changes)
activate RemoteClients

RemoteClients -> NetworkLayer: Acknowledge
RemoteClients -> BaseSynchronizer: ApplyUpdate(entityId, changes)

deactivate RemoteClients
deactivate NetworkLayer
deactivate BaseSynchronizer

@enduml
```

## Design Patterns

### 1. Manager Pattern

Each major subsystem is implemented as a manager that inherits from `BaseManager`. This provides:
- Consistent initialization
- World context access
- Uniform lifecycle management

### 2. Component Pattern

Entities use Unity's component system for modularity:
- BaseEntity provides core functionality
- Specialized entities extend BaseEntity
- Components can be added/removed dynamically

### 3. Factory Pattern

Entity creation uses factory methods in EntityManager:
- Centralized entity creation logic
- Consistent initialization
- Type safety

### 4. Observer Pattern

Synchronization system uses observer pattern:
- Entities notify synchronizers of changes
- Synchronizers broadcast updates
- Remote clients observe and apply changes

## Performance Considerations

### Memory Management
- Entity pooling for frequently created/destroyed objects
- Material sharing to reduce memory footprint
- Texture compression and LOD systems

### Rendering Optimization
- Frustum culling for off-screen entities
- LOD (Level of Detail) systems for complex meshes
- Batching for similar materials

### Network Optimization
- Delta compression for synchronization
- Priority-based updates
- Bandwidth throttling

## Integration Points

### Unity Integration
- Built on Unity 2021.3.26 with URP
- Leverages Unity's physics, rendering, and input systems
- Compatible with Unity's XR framework for VR/AR

### WebVerse Ecosystem
- Integrates with WebVerse main application via APIs
- Supports VEML (Virtual Environment Markup Language)
- Compatible with VOS Synchronization Service

### Third-Party Integrations
- Vuplex WebView for HTML content
- NWH Vehicle Physics for automobiles
- Digger for terrain modification (optional)

This architecture provides a solid foundation for creating immersive, interactive virtual worlds while maintaining extensibility and performance.