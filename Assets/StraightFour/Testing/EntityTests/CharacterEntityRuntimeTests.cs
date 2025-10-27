// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using System;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Synchronization;
using UnityEditor;
using TMPro;

public class CharacterEntityRuntimeTests
{
    [UnityTest]
    public IEnumerator CharacterEntityRuntimeTests_SetCharacterObjectOffset()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.characterControllerLabelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/CharacterPrefabLabel.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create character entity
        GameObject go = new GameObject();
        CharacterEntity character = go.AddComponent<CharacterEntity>();
        Guid entityID = Guid.NewGuid();
        
        Vector3 initialOffset = new Vector3(1, 2, 3);
        character.Initialize(entityID, null, initialOffset, Quaternion.identity, Vector3.zero);

        // Test getting initial offset
        Assert.AreEqual(initialOffset, character.characterObjectOffset);
        
        // Test setting new offset
        Vector3 newOffset = new Vector3(4, 5, 6);
        bool result = character.SetCharacterObjectOffset(newOffset);
        Assert.IsTrue(result);
        Assert.AreEqual(newOffset, character.characterObjectOffset);
        
        // Verify the actual GameObject transform was updated
        GameObject characterGO = character.GetCharacterGO();
        Assert.IsNotNull(characterGO);
        Assert.AreEqual(newOffset, characterGO.transform.localPosition);

        // Clean up
        character.Delete();
        yield return null;
    }

    [UnityTest]
    public IEnumerator CharacterEntityRuntimeTests_SetCharacterObjectRotation()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.characterControllerLabelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/CharacterPrefabLabel.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create character entity
        GameObject go = new GameObject();
        CharacterEntity character = go.AddComponent<CharacterEntity>();
        Guid entityID = Guid.NewGuid();
        
        Quaternion initialRotation = Quaternion.Euler(45, 90, 180);
        character.Initialize(entityID, null, Vector3.zero, initialRotation, Vector3.zero);

        // Test getting initial rotation
        Assert.AreEqual(initialRotation, character.characterObjectRotation);
        
        // Test setting new rotation
        Quaternion newRotation = Quaternion.Euler(30, 60, 90);
        bool result = character.SetCharacterObjectRotation(newRotation);
        Assert.IsTrue(result);
        Assert.AreEqual(newRotation, character.characterObjectRotation);
        
        // Verify the actual GameObject transform was updated
        GameObject characterGO = character.GetCharacterGO();
        Assert.IsNotNull(characterGO);
        Assert.AreEqual(newRotation, characterGO.transform.localRotation);

        // Clean up
        character.Delete();
        yield return null;
    }

    [UnityTest]
    public IEnumerator CharacterEntityRuntimeTests_SetCharacterLabelOffset()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.characterControllerLabelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/CharacterPrefabLabel.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create character entity
        GameObject go = new GameObject();
        CharacterEntity character = go.AddComponent<CharacterEntity>();
        Guid entityID = Guid.NewGuid();
        
        Vector3 initialLabelOffset = new Vector3(0, 2, 0);
        character.Initialize(entityID, null, Vector3.zero, Quaternion.identity, initialLabelOffset);

        // Test getting initial label offset
        Assert.AreEqual(initialLabelOffset, character.characterLabelOffset);
        
        // Test setting new label offset
        Vector3 newLabelOffset = new Vector3(1, 3, 1);
        bool result = character.SetCharacterLabelOffset(newLabelOffset);
        Assert.IsTrue(result);
        Assert.AreEqual(newLabelOffset, character.characterLabelOffset);
        
        // Verify the actual label transform was updated
        GameObject characterGO = character.GetCharacterGO();
        Assert.IsNotNull(characterGO);
        TextMeshProUGUI[] labels = characterGO.GetComponentsInChildren<TextMeshProUGUI>();
        if (labels.Length > 0)
        {
            Assert.AreEqual(newLabelOffset, labels[0].transform.localPosition);
        }

        // Clean up
        character.Delete();
        yield return null;
    }

    [UnityTest]
    public IEnumerator CharacterEntityRuntimeTests_SetCharacterGO()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.characterControllerLabelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/CharacterPrefabLabel.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create character entity
        GameObject go = new GameObject();
        CharacterEntity character = go.AddComponent<CharacterEntity>();
        Guid entityID = Guid.NewGuid();
        
        character.Initialize(entityID, null, Vector3.zero, Quaternion.identity, Vector3.zero);

        // Get initial character GameObject
        GameObject initialCharacterGO = character.GetCharacterGO();
        Assert.IsNotNull(initialCharacterGO);
        
        // Create a new character GameObject (simple cube for testing)
        GameObject newCharacterGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        // Test setting new character GameObject
        bool result = character.SetCharacterGO(newCharacterGO);
        Assert.IsTrue(result);
        
        // Verify the character GameObject was updated
        GameObject currentCharacterGO = character.GetCharacterGO();
        Assert.AreEqual(newCharacterGO, currentCharacterGO);
        Assert.AreEqual(character.transform, currentCharacterGO.transform.parent);

        // Test error handling with null GameObject
        bool nullResult = character.SetCharacterGO(null);
        Assert.IsFalse(nullResult);

        // Clean up
        character.Delete();
        yield return null;
    }

    [UnityTest]
    public IEnumerator CharacterEntityRuntimeTests_ErrorHandling()
    {
        // Test error handling when character is not properly initialized
        GameObject go = new GameObject();
        CharacterEntity character = go.AddComponent<CharacterEntity>();

        // These should all fail gracefully since no characterGO exists
        bool offsetResult = character.SetCharacterObjectOffset(Vector3.one);
        Assert.IsFalse(offsetResult);

        bool rotationResult = character.SetCharacterObjectRotation(Quaternion.identity);
        Assert.IsFalse(rotationResult);

        bool labelOffsetResult = character.SetCharacterLabelOffset(Vector3.one);
        Assert.IsFalse(labelOffsetResult);

        // GetCharacterGO should return null
        GameObject characterGO = character.GetCharacterGO();
        Assert.IsNull(characterGO);

        // Clean up
        character.Delete();
        yield return null;
    }
}