// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.StraightFour.Entity;
using System;

/// <summary>
/// Example script demonstrating the new runtime APIs for CharacterEntity
/// </summary>
public class CharacterEntityRuntimeDemo : MonoBehaviour
{
    [Header("Character Entity Runtime API Demo")]
    public CharacterEntity characterEntity;
    
    [Header("Test Values")]
    public Vector3 newOffset = new Vector3(0, 1, 0);
    public Vector3 newRotationEuler = new Vector3(0, 45, 0);
    public Vector3 newLabelOffset = new Vector3(0, 2, 0);
    public GameObject newCharacterPrefab;

    [Header("Controls")]
    [Space]
    public bool updateOffset;
    public bool updateRotation;
    public bool updateLabelOffset;
    public bool updateCharacterGO;

    void Update()
    {
        if (characterEntity == null) return;

        // Update character object offset
        if (updateOffset)
        {
            updateOffset = false;
            bool success = characterEntity.SetCharacterObjectOffset(newOffset);
            Debug.Log($"Set character object offset to {newOffset}: {(success ? "Success" : "Failed")}");
        }

        // Update character object rotation
        if (updateRotation)
        {
            updateRotation = false;
            Quaternion newRotation = Quaternion.Euler(newRotationEuler);
            bool success = characterEntity.SetCharacterObjectRotation(newRotation);
            Debug.Log($"Set character object rotation to {newRotationEuler}: {(success ? "Success" : "Failed")}");
        }

        // Update character label offset
        if (updateLabelOffset)
        {
            updateLabelOffset = false;
            bool success = characterEntity.SetCharacterLabelOffset(newLabelOffset);
            Debug.Log($"Set character label offset to {newLabelOffset}: {(success ? "Success" : "Failed")}");
        }

        // Update character GameObject
        if (updateCharacterGO && newCharacterPrefab != null)
        {
            updateCharacterGO = false;
            bool success = characterEntity.SetCharacterGO(newCharacterPrefab);
            Debug.Log($"Set character GameObject: {(success ? "Success" : "Failed")}");
        }
    }

    [ContextMenu("Demo - Update All Properties")]
    public void DemoUpdateAllProperties()
    {
        if (characterEntity == null)
        {
            Debug.LogError("No CharacterEntity assigned!");
            return;
        }

        // Get current values
        Vector3 currentOffset = characterEntity.characterObjectOffset;
        Quaternion currentRotation = characterEntity.characterObjectRotation;
        Vector3 currentLabelOffset = characterEntity.characterLabelOffset;
        GameObject currentGO = characterEntity.GetCharacterGO();

        Debug.Log("=== CharacterEntity Runtime API Demo ===");
        Debug.Log($"Current Offset: {currentOffset}");
        Debug.Log($"Current Rotation: {currentRotation.eulerAngles}");
        Debug.Log($"Current Label Offset: {currentLabelOffset}");
        Debug.Log($"Current GameObject: {(currentGO != null ? currentGO.name : "null")}");

        // Update offset
        Vector3 testOffset = currentOffset + Vector3.up;
        if (characterEntity.SetCharacterObjectOffset(testOffset))
        {
            Debug.Log($"✓ Successfully updated offset to {testOffset}");
        }

        // Update rotation
        Quaternion testRotation = currentRotation * Quaternion.Euler(0, 45, 0);
        if (characterEntity.SetCharacterObjectRotation(testRotation))
        {
            Debug.Log($"✓ Successfully updated rotation to {testRotation.eulerAngles}");
        }

        // Update label offset
        Vector3 testLabelOffset = currentLabelOffset + Vector3.forward;
        if (characterEntity.SetCharacterLabelOffset(testLabelOffset))
        {
            Debug.Log($"✓ Successfully updated label offset to {testLabelOffset}");
        }

        Debug.Log("=== Demo Complete ===");
    }
}