// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.World;
using UnityEditor;
using System.Collections;

public class WorldTests
{
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
            skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/skybox.mat"),
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
}