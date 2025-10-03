// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.World;
using UnityEditor;
using System.Collections;

public class WorldTests
{
    [TearDown]
    public void TearDown()
    {
        // Clean up any loaded world after each test
        if (StraightFour.ActiveWorld != null)
        {
            StraightFour.UnloadWorld();
        }
    }

    [UnityTest]
    public IEnumerator WorldTests_World()
    {
        GameObject worldGO = new GameObject();
        World world = worldGO.AddComponent<World>();

        // Initialize.
        World.WorldInfo worldInfo = new World.WorldInfo()
        {
            characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab"),
            highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")),
            previewMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")),
            inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab"),
            maxEntryLength = 128,
            maxKeyLength = 16,
            maxStorageEntries = 16,
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat"),
            liteProceduralSkyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/LiteProceduralSkybox.mat"),
            defaultCloudTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Environment/Textures/DefaultClouds.png"),
            defaultStarTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Environment/Textures/DefaultStars.png"),
            voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab")
        };
        world.Initialize(worldInfo);
        Assert.IsNotNull(world.entityManager);
        Assert.IsNotNull(world.storageManager);
        Assert.IsNotNull(world.cameraManager);
        Assert.IsNotNull(world.materialManager);

        // Unload.
        world.Unload();
        yield return null;
        Assert.IsTrue(world.entityManager == null);
        Assert.IsTrue(world.storageManager == null);
        Assert.IsTrue(world.cameraManager == null);
        Assert.IsTrue(world.materialManager == null);
    }

    [UnityTest]
    public IEnumerator WorldTests_WorldOffsetUpdate()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        we.liteProceduralSkyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/LiteProceduralSkybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create a character entity
        GameObject characterGO = new GameObject("TestCharacter");
        CharacterEntity character = characterGO.AddComponent<CharacterEntity>();
        character.Initialize(System.Guid.NewGuid(), null, Vector3.zero, Quaternion.identity, Vector3.zero);
        character.SetInteractionState(FiveSQD.StraightFour.Entity.BaseEntity.InteractionState.Physical);

        // Set the tracked character
        StraightFour.ActiveWorld.SetTrackedCharacterEntity(character);
        StraightFour.ActiveWorld.enableAutoWorldOffsetUpdate = true;
        StraightFour.ActiveWorld.worldOffsetUpdateThreshold = 100f;

        // Verify initial offset is zero
        Assert.AreEqual(Vector3.zero, StraightFour.ActiveWorld.worldOffset);

        // Move character beyond threshold
        character.SetPosition(new Vector3(150, 0, 0), false, false);
        
        // Wait a frame for Update to run
        yield return null;

        // Verify world offset was updated
        Vector3 expectedOffset = new Vector3(150, 0, 0);
        Assert.AreEqual(expectedOffset, StraightFour.ActiveWorld.worldOffset);

        // Move character closer to new origin (within threshold)
        character.SetPosition(new Vector3(160, 0, 10), false, false);
        
        // Wait a frame for Update to run
        yield return null;

        // Offset should remain the same since distance from origin is still within threshold
        Assert.AreEqual(expectedOffset, StraightFour.ActiveWorld.worldOffset);

        // Move character far again
        character.SetPosition(new Vector3(300, 0, 0), false, false);
        
        // Wait a frame for Update to run
        yield return null;

        // Verify offset updated again
        expectedOffset = new Vector3(300, 0, 0);
        Assert.AreEqual(expectedOffset, StraightFour.ActiveWorld.worldOffset);

        // Test disabling auto-update
        StraightFour.ActiveWorld.enableAutoWorldOffsetUpdate = false;
        character.SetPosition(new Vector3(500, 0, 0), false, false);
        
        // Wait a frame for Update to run
        yield return null;

        // Offset should NOT have changed
        Assert.AreEqual(new Vector3(300, 0, 0), StraightFour.ActiveWorld.worldOffset);
    }
}