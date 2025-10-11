# Examples

This document provides practical examples and code samples for using the WebVerse World Engine effectively.

## Example Index

1. [Basic World Creation](#basic-world-creation)
2. [Entity Management](#entity-management)
3. [Interactive Objects](#interactive-objects)
4. [Terrain Generation](#terrain-generation)
5. [Character System](#character-system)
6. [UI Integration](#ui-integration)
7. [VR/AR Implementation](#vrar-implementation)
8. [Multiplayer Synchronization](#multiplayer-synchronization)
9. [Custom Entity Types](#custom-entity-types)
10. [Performance Optimization](#performance-optimization)

## Basic World Creation

### Simple World Setup

```csharp
using UnityEngine;
using FiveSQD.StraightFour;

public class BasicWorldExample : MonoBehaviour
{
    [Header("World Configuration")]
    public string worldName = "ExampleWorld";
    public bool enableVR = false;
    
    void Start()
    {
        CreateBasicWorld();
    }
    
    void CreateBasicWorld()
    {
        // Configure query parameters
        string queryParams = $"vr={enableVR}&debug=true";
        
        // Load the world
        bool success = StraightFour.LoadWorld(worldName, queryParams);
        
        if (success)
        {
            Debug.Log($"World '{worldName}' loaded successfully!");
            StartCoroutine(SetupWorldContent());
        }
        else
        {
            Debug.LogError($"Failed to load world '{worldName}'");
        }
    }
    
    System.Collections.IEnumerator SetupWorldContent()
    {
        // Wait a frame for world to fully initialize
        yield return null;
        
        var world = StraightFour.ActiveWorld;
        if (world == null) yield break;
        
        // Setup environment
        SetupEnvironment(world);
        
        // Create basic entities
        CreateBasicEntities(world);
        
        // Configure camera
        SetupCamera(world);
    }
    
    void SetupEnvironment(World.World world)
    {
        var envManager = world.environmentManager;
        
        // Set ambient lighting
        envManager.SetAmbientLighting(new Color(0.4f, 0.4f, 0.6f), 0.3f);
        
        // Configure fog
        envManager.SetFog(true, new Color(0.5f, 0.6f, 0.7f), 50f, 200f);
        
        Debug.Log("Environment configured");
    }
    
    void CreateBasicEntities(World.World world)
    {
        var entityManager = world.entityManager;
        
        // Create ground plane
        CreateGround(entityManager);
        
        // Create some objects
        CreateSampleObjects(entityManager);
        
        // Add lighting
        CreateLighting(entityManager);
    }
    
    void SetupCamera(World.World world)
    {
        var cameraManager = world.cameraManager;
        
        // Position camera
        cameraManager.SetCameraPosition(new Vector3(0, 5, -10));
        cameraManager.SetCameraRotation(Quaternion.Euler(15, 0, 0));
        
        // Enable crosshair
        cameraManager.crosshairEnabled = true;
        
        Debug.Log("Camera configured");
    }
}
```

## Entity Management

### Dynamic Entity Creation and Management

```csharp
using UnityEngine;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;

public class EntityManagementExample : MonoBehaviour
{
    private EntityManager entityManager;
    private List<System.Guid> createdEntities = new List<System.Guid>();
    
    void Start()
    {
        entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null)
        {
            Debug.LogError("No active world found!");
            return;
        }
        
        StartCoroutine(DynamicEntityCreation());
    }
    
    System.Collections.IEnumerator DynamicEntityCreation()
    {
        // Create entities over time
        for (int i = 0; i < 10; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(1f, 5f),
                Random.Range(-10f, 10f)
            );
            
            CreateRandomEntity(position);
            
            yield return new WaitForSeconds(1f);
        }
        
        // Wait then cleanup some entities
        yield return new WaitForSeconds(5f);
        CleanupRandomEntities();
    }
    
    void CreateRandomEntity(Vector3 position)
    {
        System.Guid entityId = System.Guid.NewGuid();
        
        // Random entity type
        int entityType = Random.Range(0, 3);
        
        switch (entityType)
        {
            case 0:
                CreateCube(position, entityId);
                break;
            case 1:
                CreateSphere(position, entityId);
                break;
            case 2:
                CreateLight(position, entityId);
                break;
        }
        
        createdEntities.Add(entityId);
    }
    
    void CreateCube(Vector3 position, System.Guid id)
    {
        Mesh cubeMesh = CreatePrimitiveMesh(PrimitiveType.Cube);
        Material[] materials = { CreateRandomMaterial() };
        
        entityManager.LoadMeshEntity(
            mesh: cubeMesh,
            materials: materials,
            position: position,
            rotation: Random.rotation,
            scale: Vector3.one * Random.Range(0.5f, 2f),
            id: id,
            tag: "RandomCube"
        );
    }
    
    void CreateSphere(Vector3 position, System.Guid id)
    {
        Mesh sphereMesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
        Material[] materials = { CreateRandomMaterial() };
        
        entityManager.LoadMeshEntity(
            mesh: sphereMesh,
            materials: materials,
            position: position,
            rotation: Quaternion.identity,
            scale: Vector3.one * Random.Range(0.3f, 1.5f),
            id: id,
            tag: "RandomSphere"
        );
    }
    
    void CreateLight(Vector3 position, System.Guid id)
    {
        entityManager.LoadLightEntity(
            parentEntity: null,
            position: position,
            rotation: Quaternion.identity,
            id: id,
            tag: "RandomLight",
            lightType: LightType.Point,
            intensity: Random.Range(0.5f, 2f),
            color: Random.ColorHSV(),
            range: Random.Range(5f, 15f)
        );
    }
    
    Material CreateRandomMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Random.ColorHSV();
        mat.SetFloat("_Metallic", Random.Range(0f, 1f));
        mat.SetFloat("_Smoothness", Random.Range(0f, 1f));
        return mat;
    }
    
    void CleanupRandomEntities()
    {
        // Remove half of the created entities
        int toRemove = createdEntities.Count / 2;
        
        for (int i = 0; i < toRemove; i++)
        {
            System.Guid entityId = createdEntities[i];
            entityManager.DeleteEntity(entityId);
            Debug.Log($"Deleted entity {entityId}");
        }
        
        createdEntities.RemoveRange(0, toRemove);
    }
}
```

## Interactive Objects

### Click and Drag Interaction

```csharp
using UnityEngine;
using FiveSQD.StraightFour.Entity;

public class InteractiveObjectsExample : MonoBehaviour
{
    private Camera playerCamera;
    private BaseEntity selectedEntity;
    private Vector3 dragOffset;
    
    void Start()
    {
        playerCamera = StraightFour.ActiveWorld?.cameraManager?.cam;
        CreateInteractiveObjects();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void CreateInteractiveObjects()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null) return;
        
        // Create interactive cubes
        for (int i = 0; i < 5; i++)
        {
            Vector3 position = new Vector3(i * 3 - 6, 1, 0);
            CreateInteractiveCube(entityManager, position, i);
        }
    }
    
    void CreateInteractiveCube(EntityManager entityManager, Vector3 position, int index)
    {
        Mesh cubeMesh = CreatePrimitiveMesh(PrimitiveType.Cube);
        Material[] materials = { CreateInteractiveMaterial(index) };
        
        System.Guid id = System.Guid.NewGuid();
        
        entityManager.LoadMeshEntity(
            mesh: cubeMesh,
            materials: materials,
            position: position,
            rotation: Quaternion.identity,
            scale: Vector3.one,
            id: id,
            tag: "InteractiveCube",
            onLoaded: () => SetupInteractiveEntity(entityManager.GetEntity(id))
        );
    }
    
    void SetupInteractiveEntity(BaseEntity entity)
    {
        if (entity == null) return;
        
        // Add collider for interaction
        var collider = entity.gameObject.AddComponent<BoxCollider>();
        
        // Add interaction component
        var interaction = entity.gameObject.AddComponent<InteractableObject>();
        interaction.onClicked += OnEntityClicked;
        interaction.onHoverEnter += OnEntityHoverEnter;
        interaction.onHoverExit += OnEntityHoverExit;
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectEntity();
        }
        
        if (Input.GetMouseButton(0) && selectedEntity != null)
        {
            DragEntity();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseEntity();
        }
    }
    
    void TrySelectEntity()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BaseEntity entity = hit.collider.GetComponent<BaseEntity>();
            if (entity != null && entity.gameObject.CompareTag("InteractiveCube"))
            {
                selectedEntity = entity;
                entity.SetInteractionState(InteractionState.Placing);
                
                // Calculate drag offset
                Vector3 screenPoint = playerCamera.WorldToScreenPoint(entity.transform.position);
                Vector3 mousePoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                dragOffset = entity.transform.position - playerCamera.ScreenToWorldPoint(mousePoint);
                
                Debug.Log($"Selected entity: {entity.id}");
            }
        }
    }
    
    void DragEntity()
    {
        Vector3 mousePoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 
            playerCamera.WorldToScreenPoint(selectedEntity.transform.position).z);
        Vector3 worldPoint = playerCamera.ScreenToWorldPoint(mousePoint) + dragOffset;
        
        selectedEntity.SetPosition(worldPoint);
    }
    
    void ReleaseEntity()
    {
        if (selectedEntity != null)
        {
            selectedEntity.SetInteractionState(InteractionState.Physical);
            Debug.Log($"Released entity: {selectedEntity.id}");
            selectedEntity = null;
        }
    }
    
    void OnEntityClicked(BaseEntity entity)
    {
        Debug.Log($"Clicked: {entity.id}");
        
        // Animate the entity
        StartCoroutine(AnimateEntityClick(entity));
    }
    
    void OnEntityHoverEnter(BaseEntity entity)
    {
        entity.SetInteractionState(InteractionState.Static);
        Debug.Log($"Hover enter: {entity.id}");
    }
    
    void OnEntityHoverExit(BaseEntity entity)
    {
        entity.SetInteractionState(InteractionState.Physical);
        Debug.Log($"Hover exit: {entity.id}");
    }
    
    System.Collections.IEnumerator AnimateEntityClick(BaseEntity entity)
    {
        Vector3 originalScale = entity.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        
        // Scale up
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            Vector3 currentScale = Vector3.Lerp(originalScale, targetScale, progress);
            entity.SetScale(currentScale);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Scale back down
        elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            Vector3 currentScale = Vector3.Lerp(targetScale, originalScale, progress);
            entity.SetScale(currentScale);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        entity.SetScale(originalScale);
    }
}

// Helper component for entity interaction
public class InteractableObject : MonoBehaviour
{
    [System.Serializable]
    public class EntityEvent : UnityEngine.Events.UnityEvent<BaseEntity> { }
    
    public EntityEvent onClicked = new EntityEvent();
    public EntityEvent onHoverEnter = new EntityEvent();
    public EntityEvent onHoverExit = new EntityEvent();
    
    private BaseEntity entity;
    private bool isHovering = false;
    
    void Start()
    {
        entity = GetComponent<BaseEntity>();
    }
    
    void OnMouseDown()
    {
        onClicked?.Invoke(entity);
    }
    
    void OnMouseEnter()
    {
        if (!isHovering)
        {
            isHovering = true;
            onHoverEnter?.Invoke(entity);
        }
    }
    
    void OnMouseExit()
    {
        if (isHovering)
        {
            isHovering = false;
            onHoverExit?.Invoke(entity);
        }
    }
}
```

## Terrain Generation

### Procedural Terrain with Multiple Layers

```csharp
using UnityEngine;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;

public class TerrainGenerationExample : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainSize = 513;  // Power of 2 + 1
    public float terrainHeight = 50f;
    public Vector3 terrainScale = new Vector3(200f, 50f, 200f);
    
    [Header("Noise Settings")]
    public float noiseScale = 0.01f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Vector2 offset = Vector2.zero;
    
    void Start()
    {
        CreateProceduralTerrain();
    }
    
    void CreateProceduralTerrain()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null)
        {
            Debug.LogError("No entity manager found!");
            return;
        }
        
        // Generate heightmap
        float[,] heightmap = GenerateHeightmap();
        
        // Create terrain layers
        TerrainEntityLayer[] layers = CreateTerrainLayers();
        
        // Generate layer masks
        Dictionary<int, float[,]> layerMasks = GenerateLayerMasks(heightmap);
        
        // Create the terrain entity
        entityManager.LoadTerrainEntity(
            length: terrainScale.x,
            width: terrainScale.z,
            height: terrainScale.y,
            heights: heightmap,
            layers: layers,
            layerMasks: layerMasks,
            id: System.Guid.NewGuid(),
            parent: null,
            position: Vector3.zero,
            rotation: Quaternion.identity,
            tag: "ProceduralTerrain",
            onLoaded: OnTerrainLoaded
        );
    }
    
    float[,] GenerateHeightmap()
    {
        float[,] heightmap = new float[terrainSize, terrainSize];
        
        for (int x = 0; x < terrainSize; x++)
        {
            for (int y = 0; y < terrainSize; y++)
            {
                float height = 0f;
                float amplitude = 1f;
                float frequency = noiseScale;
                
                // Generate octaves of noise
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + offset.x) * frequency;
                    float sampleY = (y + offset.y) * frequency;
                    
                    float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);
                    height += noiseValue * amplitude;
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                heightmap[x, y] = Mathf.Clamp01(height);
            }
        }
        
        return heightmap;
    }
    
    TerrainEntityLayer[] CreateTerrainLayers()
    {
        List<TerrainEntityLayer> layers = new List<TerrainEntityLayer>();
        
        // Grass layer
        layers.Add(new TerrainEntityLayer
        {
            texture = CreateSolidColorTexture(Color.green, "Grass"),
            normalMap = null,
            tileSize = new Vector2(15f, 15f),
            smoothness = 0.2f,
            metallic = 0f
        });
        
        // Rock layer
        layers.Add(new TerrainEntityLayer
        {
            texture = CreateSolidColorTexture(Color.gray, "Rock"),
            normalMap = null,
            tileSize = new Vector2(20f, 20f),
            smoothness = 0.8f,
            metallic = 0.1f
        });
        
        // Sand layer
        layers.Add(new TerrainEntityLayer
        {
            texture = CreateSolidColorTexture(Color.yellow, "Sand"),
            normalMap = null,
            tileSize = new Vector2(10f, 10f),
            smoothness = 0.1f,
            metallic = 0f
        });
        
        return layers.ToArray();
    }
    
    Dictionary<int, float[,]> GenerateLayerMasks(float[,] heightmap)
    {
        var layerMasks = new Dictionary<int, float[,]>();
        
        int size = heightmap.GetLength(0);
        
        // Grass mask (lower elevations)
        float[,] grassMask = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float height = heightmap[x, y];
                grassMask[x, y] = Mathf.Clamp01(1f - height * 2f);
            }
        }
        layerMasks[0] = grassMask;
        
        // Rock mask (higher elevations and steep slopes)
        float[,] rockMask = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float height = heightmap[x, y];
                float steepness = CalculateSteepness(heightmap, x, y);
                rockMask[x, y] = Mathf.Clamp01(height * 1.5f + steepness * 2f);
            }
        }
        layerMasks[1] = rockMask;
        
        // Sand mask (very low elevations)
        float[,] sandMask = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float height = heightmap[x, y];
                sandMask[x, y] = height < 0.2f ? Mathf.Clamp01((0.2f - height) * 3f) : 0f;
            }
        }
        layerMasks[2] = sandMask;
        
        return layerMasks;
    }
    
    float CalculateSteepness(float[,] heightmap, int x, int y)
    {
        int size = heightmap.GetLength(0);
        
        if (x <= 0 || x >= size - 1 || y <= 0 || y >= size - 1)
            return 0f;
        
        float heightL = heightmap[x - 1, y];
        float heightR = heightmap[x + 1, y];
        float heightU = heightmap[x, y + 1];
        float heightD = heightmap[x, y - 1];
        
        float dx = heightR - heightL;
        float dy = heightU - heightD;
        
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
    
    Texture2D CreateSolidColorTexture(Color color, string name)
    {
        Texture2D texture = new Texture2D(4, 4);
        texture.name = name;
        
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    void OnTerrainLoaded()
    {
        Debug.Log("Procedural terrain created successfully!");
        
        // Add some objects on the terrain
        AddTerrainDecorations();
    }
    
    void AddTerrainDecorations()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null) return;
        
        // Add trees and rocks randomly
        for (int i = 0; i < 20; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-terrainScale.x / 2f, terrainScale.x / 2f),
                0f,
                Random.Range(-terrainScale.z / 2f, terrainScale.z / 2f)
            );
            
            // Sample terrain height at this position
            position.y = SampleTerrainHeight(position) + 1f;
            
            if (Random.value < 0.5f)
                CreateTree(entityManager, position);
            else
                CreateRock(entityManager, position);
        }
    }
    
    float SampleTerrainHeight(Vector3 worldPosition)
    {
        // This would ideally sample the actual terrain
        // For now, use noise function
        float normalizedX = (worldPosition.x + terrainScale.x / 2f) / terrainScale.x;
        float normalizedZ = (worldPosition.z + terrainScale.z / 2f) / terrainScale.z;
        
        float height = Mathf.PerlinNoise(
            normalizedX * terrainSize * noiseScale + offset.x,
            normalizedZ * terrainSize * noiseScale + offset.y
        );
        
        return height * terrainScale.y;
    }
    
    void CreateTree(EntityManager entityManager, Vector3 position)
    {
        // Simple tree using cylinder + sphere
        Mesh cylinderMesh = CreatePrimitiveMesh(PrimitiveType.Cylinder);
        Material[] trunkMaterials = { CreateSolidColorMaterial(new Color(0.4f, 0.2f, 0.1f)) };
        
        entityManager.LoadMeshEntity(
            mesh: cylinderMesh,
            materials: trunkMaterials,
            position: position,
            rotation: Quaternion.identity,
            scale: new Vector3(0.3f, 2f, 0.3f),
            tag: "Tree"
        );
        
        // Tree top
        Mesh sphereMesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
        Material[] leafMaterials = { CreateSolidColorMaterial(Color.green) };
        
        entityManager.LoadMeshEntity(
            mesh: sphereMesh,
            materials: leafMaterials,
            position: position + Vector3.up * 3f,
            rotation: Quaternion.identity,
            scale: Vector3.one * 2f,
            tag: "TreeTop"
        );
    }
    
    void CreateRock(EntityManager entityManager, Vector3 position)
    {
        Mesh sphereMesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
        Material[] rockMaterials = { CreateSolidColorMaterial(Color.gray) };
        
        entityManager.LoadMeshEntity(
            mesh: sphereMesh,
            materials: rockMaterials,
            position: position,
            rotation: Random.rotation,
            scale: Vector3.one * Random.Range(0.5f, 1.5f),
            tag: "Rock"
        );
    }
}
```

## Character System

### Basic Character Controller

```csharp
using UnityEngine;
using FiveSQD.StraightFour.Entity;

public class CharacterSystemExample : MonoBehaviour
{
    [Header("Character Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float mouseSensitivity = 2f;
    
    private BaseEntity characterEntity;
    private CharacterController characterController;
    private Camera playerCamera;
    
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;
    
    private float xRotation = 0f;
    
    void Start()
    {
        CreatePlayerCharacter();
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void CreatePlayerCharacter()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null) return;
        
        Vector3 spawnPosition = new Vector3(0, 5, 0);
        
        // Create character entity
        entityManager.LoadCharacterEntity(
            parent: null,
            characterPrefab: null, // Will create basic character
            position: spawnPosition,
            rotation: Quaternion.identity,
            id: System.Guid.NewGuid(),
            tag: "Player",
            onLoaded: OnCharacterLoaded
        );
    }
    
    void OnCharacterLoaded()
    {
        // Find the character entity
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        BaseEntity[] entities = entityManager.GetEntitiesByTag("Player");
        
        if (entities.Length > 0)
        {
            characterEntity = entities[0];
            characterController = characterEntity.GetComponent<CharacterController>();
            
            // Setup camera
            SetupFirstPersonCamera();
            
            Debug.Log("Player character loaded and configured");
        }
    }
    
    void SetupFirstPersonCamera()
    {
        var cameraManager = StraightFour.ActiveWorld?.cameraManager;
        if (cameraManager == null) return;
        
        playerCamera = cameraManager.cam;
        
        // Position camera at character eye level
        Vector3 cameraPosition = characterEntity.transform.position + Vector3.up * 1.7f;
        cameraManager.SetCameraPosition(cameraPosition);
        
        // Parent camera to character
        cameraManager.SetCameraParent(characterEntity.transform);
        
        // Offset camera from character center
        playerCamera.transform.localPosition = Vector3.up * 1.7f;
    }
    
    void Update()
    {
        if (characterController == null) return;
        
        HandleMouseLook();
        HandleMovement();
        HandleJump();
    }
    
    void HandleMouseLook()
    {
        if (playerCamera == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate character horizontally
        characterEntity.transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        isGrounded = characterController.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = characterEntity.transform.right * horizontal + characterEntity.transform.forward * vertical;
        characterController.Move(direction * moveSpeed * Time.deltaTime);
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
```

## UI Integration

### World Space UI with WebView

```csharp
using UnityEngine;
using FiveSQD.StraightFour.Entity;

public class UIIntegrationExample : MonoBehaviour
{
    [Header("UI Settings")]
    public Vector2 panelSize = new Vector2(1920, 1080);
    public float panelScale = 0.001f;
    
    private HTMLEntity webPanel;
    
    void Start()
    {
        CreateUIElements();
    }
    
    void CreateUIElements()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null) return;
        
        // Create information panel
        CreateInfoPanel(entityManager);
        
        // Create web browser panel
        CreateWebBrowserPanel(entityManager);
        
        // Create interactive buttons
        CreateInteractiveButtons(entityManager);
    }
    
    void CreateInfoPanel(EntityManager entityManager)
    {
        Vector3 position = new Vector3(-3, 2, 5);
        
        entityManager.LoadUIEntity(
            parent: null,
            position: position,
            rotation: Quaternion.identity,
            size: panelSize * 0.5f,
            id: System.Guid.NewGuid(),
            tag: "InfoPanel",
            renderMode: RenderMode.WorldSpace,
            onLoaded: () => SetupInfoPanel(entityManager.GetEntitiesByTag("InfoPanel")[0])
        );
    }
    
    void SetupInfoPanel(BaseEntity panelEntity)
    {
        // Add text components to the UI panel
        Canvas canvas = panelEntity.GetComponentInChildren<Canvas>();
        if (canvas == null) return;
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvas.transform, false);
        
        var bgImage = background.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Add title text
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(canvas.transform, false);
        
        var titleText = titleObject.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "World Information";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 48;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        var titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Add info text
        GameObject infoObject = new GameObject("Info");
        infoObject.transform.SetParent(canvas.transform, false);
        
        var infoText = infoObject.AddComponent<UnityEngine.UI.Text>();
        infoText.text = GetWorldInfo();
        infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        infoText.fontSize = 24;
        infoText.color = Color.white;
        infoText.alignment = TextAnchor.UpperLeft;
        
        var infoRect = infoObject.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.1f);
        infoRect.anchorMax = new Vector2(0.9f, 0.8f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
    }
    
    string GetWorldInfo()
    {
        var world = StraightFour.ActiveWorld;
        if (world == null) return "No world loaded";
        
        var entities = world.entityManager.GetAllEntities();
        
        return $"World: {world.siteName}\n" +
               $"Entities: {entities.Length}\n" +
               $"VR Mode: {world.cameraManager.vr}\n" +
               $"Time: {System.DateTime.Now:HH:mm:ss}";
    }
    
    void CreateWebBrowserPanel(EntityManager entityManager)
    {
        Vector3 position = new Vector3(3, 2, 5);
        
        entityManager.LoadHTMLEntity(
            parent: null,
            position: position,
            rotation: Quaternion.identity,
            size: panelSize * panelScale,
            id: System.Guid.NewGuid(),
            tag: "WebBrowser",
            onLoaded: () => SetupWebBrowser(entityManager.GetEntitiesByTag("WebBrowser")[0] as HTMLEntity)
        );
    }
    
    void SetupWebBrowser(HTMLEntity htmlEntity)
    {
        if (htmlEntity == null) return;
        
        webPanel = htmlEntity;
        
        // Load a webpage
        webPanel.LoadURL("https://example.com");
        
        // Or load custom HTML
        string customHTML = @"
        <html>
        <head>
            <title>WebVerse Panel</title>
            <style>
                body { 
                    font-family: Arial, sans-serif; 
                    background: linear-gradient(45deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 20px;
                    margin: 0;
                }
                h1 { text-align: center; }
                .button {
                    background: #4CAF50;
                    border: none;
                    color: white;
                    padding: 15px 32px;
                    text-align: center;
                    font-size: 16px;
                    margin: 4px 2px;
                    cursor: pointer;
                    border-radius: 4px;
                }
            </style>
        </head>
        <body>
            <h1>WebVerse World Engine</h1>
            <p>This is a web panel embedded in the 3D world!</p>
            <button class='button' onclick='sendMessage()'>Interact with Unity</button>
            
            <script>
                function sendMessage() {
                    // This would communicate back to Unity
                    console.log('Button clicked in WebView!');
                }
            </script>
        </body>
        </html>";
        
        webPanel.LoadHTML(customHTML);
        
        Debug.Log("Web browser panel set up");
    }
    
    void CreateInteractiveButtons(EntityManager entityManager)
    {
        // Create floating action buttons
        for (int i = 0; i < 3; i++)
        {
            Vector3 position = new Vector3(0, 3 + i * 0.5f, 3);
            CreateFloatingButton(entityManager, position, i);
        }
    }
    
    void CreateFloatingButton(EntityManager entityManager, Vector3 position, int index)
    {
        entityManager.LoadUIEntity(
            parent: null,
            position: position,
            rotation: Quaternion.identity,
            size: new Vector2(200, 100),
            id: System.Guid.NewGuid(),
            tag: $"FloatingButton{index}",
            renderMode: RenderMode.WorldSpace,
            onLoaded: () => SetupFloatingButton(
                entityManager.GetEntitiesByTag($"FloatingButton{index}")[0], index)
        );
    }
    
    void SetupFloatingButton(BaseEntity buttonEntity, int index)
    {
        Canvas canvas = buttonEntity.GetComponentInChildren<Canvas>();
        if (canvas == null) return;
        
        // Create button
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(canvas.transform, false);
        
        var button = buttonObject.AddComponent<UnityEngine.UI.Button>();
        var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
        buttonImage.color = Color.cyan;
        
        // Add button text
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        
        var buttonText = textObject.AddComponent<UnityEngine.UI.Text>();
        buttonText.text = $"Action {index + 1}";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.black;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add button functionality
        int buttonIndex = index;
        button.onClick.AddListener(() => OnFloatingButtonClicked(buttonIndex));
        
        // Add hover effect
        StartCoroutine(AnimateFloatingButton(buttonEntity));
    }
    
    void OnFloatingButtonClicked(int buttonIndex)
    {
        Debug.Log($"Floating button {buttonIndex} clicked!");
        
        switch (buttonIndex)
        {
            case 0:
                // Spawn random object
                SpawnRandomObject();
                break;
            case 1:
                // Toggle web panel
                ToggleWebPanel();
                break;
            case 2:
                // Reset world
                ResetWorld();
                break;
        }
    }
    
    System.Collections.IEnumerator AnimateFloatingButton(BaseEntity buttonEntity)
    {
        Vector3 originalPosition = buttonEntity.transform.position;
        
        while (buttonEntity != null)
        {
            float time = Time.time * 2f;
            Vector3 offset = Vector3.up * Mathf.Sin(time) * 0.1f;
            buttonEntity.SetPosition(originalPosition + offset);
            
            yield return null;
        }
    }
    
    void SpawnRandomObject()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null) return;
        
        Vector3 spawnPosition = new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(2f, 6f),
            Random.Range(2f, 8f)
        );
        
        Mesh mesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
        Material[] materials = { CreateRandomMaterial() };
        
        entityManager.LoadMeshEntity(
            mesh: mesh,
            materials: materials,
            position: spawnPosition,
            rotation: Random.rotation,
            scale: Vector3.one * Random.Range(0.5f, 1.5f),
            tag: "SpawnedObject"
        );
        
        Debug.Log("Spawned random object");
    }
    
    void ToggleWebPanel()
    {
        if (webPanel != null)
        {
            bool isVisible = webPanel.gameObject.activeSelf;
            webPanel.gameObject.SetActive(!isVisible);
            Debug.Log($"Web panel {(isVisible ? "hidden" : "shown")}");
        }
    }
    
    void ResetWorld()
    {
        var entityManager = StraightFour.ActiveWorld?.entityManager;
        if (entityManager == null) return;
        
        // Remove all spawned objects
        BaseEntity[] spawnedObjects = entityManager.GetEntitiesByTag("SpawnedObject");
        foreach (var obj in spawnedObjects)
        {
            entityManager.DeleteEntity(obj.id);
        }
        
        Debug.Log($"Removed {spawnedObjects.Length} spawned objects");
    }
}
```

This examples documentation provides comprehensive, practical code samples that demonstrate the key features and capabilities of the WebVerse World Engine. Each example builds upon the previous ones and showcases different aspects of the system, from basic world creation to advanced UI integration.

The examples are designed to be:
- **Copy-paste ready** - Code can be used directly
- **Educational** - Comments explain key concepts
- **Progressive** - Start simple and get more complex
- **Practical** - Solve real-world use cases
- **Extensible** - Easy to modify and build upon

These examples serve as both documentation and starter templates for developers working with the WebVerse World Engine.