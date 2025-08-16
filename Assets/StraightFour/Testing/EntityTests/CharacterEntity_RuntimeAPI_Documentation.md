# CharacterEntity Runtime API Documentation

This document describes the new runtime APIs added to the `CharacterEntity` class that allow dynamic updates to character properties during gameplay.

## Overview

Previously, the properties `characterGO`, `characterObjectOffset`, `characterObjectRotation`, and `characterLabelOffset` could only be set during entity initialization. The new runtime APIs allow these properties to be modified at any time during execution, enabling dynamic character management.

## New APIs

### 1. SetCharacterGO(GameObject newCharacterGO, bool synchronize = true)

**Purpose**: Replace the character GameObject at runtime with a new one.

**Parameters**:
- `newCharacterGO`: The new character GameObject to use (cannot be null)
- `synchronize`: Whether to synchronize the change (default: true)

**Returns**: `bool` - Whether the operation was successful

**Example**:
```csharp
GameObject newCharacter = Resources.Load<GameObject>("NewCharacterPrefab");
bool success = characterEntity.SetCharacterGO(newCharacter);
if (success)
{
    Debug.Log("Character GameObject updated successfully!");
}
```

**Notes**:
- Automatically updates mesh renderers and bounds calculations
- Cleans up the old character GameObject
- Preserves current offset and rotation settings

### 2. GetCharacterGO()

**Purpose**: Get the current character GameObject.

**Returns**: `GameObject` - The current character GameObject (can be null)

**Example**:
```csharp
GameObject currentCharacter = characterEntity.GetCharacterGO();
if (currentCharacter != null)
{
    Debug.Log($"Current character: {currentCharacter.name}");
}
```

### 3. SetCharacterObjectOffset(Vector3 newOffset, bool synchronize = true)

**Purpose**: Update the character object's position offset at runtime.

**Parameters**:
- `newOffset`: The new position offset to apply
- `synchronize`: Whether to synchronize the change (default: true)

**Returns**: `bool` - Whether the operation was successful

**Example**:
```csharp
// Move character slightly up
Vector3 newOffset = new Vector3(0, 0.5f, 0);
bool success = characterEntity.SetCharacterObjectOffset(newOffset);
```

**Notes**:
- Immediately updates the character GameObject's local position
- Updates the internal `characterObjectOffset` property

### 4. SetCharacterObjectRotation(Quaternion newRotation, bool synchronize = true)

**Purpose**: Update the character object's rotation at runtime.

**Parameters**:
- `newRotation`: The new rotation to apply
- `synchronize`: Whether to synchronize the change (default: true)

**Returns**: `bool` - Whether the operation was successful

**Example**:
```csharp
// Rotate character 45 degrees around Y axis
Quaternion newRotation = Quaternion.Euler(0, 45, 0);
bool success = characterEntity.SetCharacterObjectRotation(newRotation);
```

**Notes**:
- Immediately updates the character GameObject's local rotation
- Updates the internal `characterObjectRotation` property

### 5. SetCharacterLabelOffset(Vector3 newOffset, bool synchronize = true)

**Purpose**: Update the character label's position offset at runtime.

**Parameters**:
- `newOffset`: The new label position offset to apply
- `synchronize`: Whether to synchronize the change (default: true)

**Returns**: `bool` - Whether the operation was successful

**Example**:
```csharp
// Move label higher above character
Vector3 newLabelOffset = new Vector3(0, 2.5f, 0);
bool success = characterEntity.SetCharacterLabelOffset(newLabelOffset);
```

**Notes**:
- Immediately updates the character label's local position
- Updates the internal `characterLabelOffset` property
- Finds TextMeshProUGUI components in character hierarchy

## Error Handling

All setter methods include proper error handling:

- Return `false` if the operation fails
- Log appropriate error messages to the console
- Check for null character GameObject before operations
- Validate input parameters

**Common Error Cases**:
- Calling methods before character is properly initialized
- Passing null GameObject to `SetCharacterGO()`
- Character GameObject has been destroyed

## Backward Compatibility

These new APIs are fully backward compatible:

- Existing code continues to work unchanged
- Original initialization methods remain functional
- No breaking changes to existing interfaces
- Properties maintain their original getter behavior

## Usage Examples

### Dynamic Character Customization
```csharp
public class CharacterCustomizer : MonoBehaviour
{
    public CharacterEntity character;
    public GameObject[] characterVariants;
    
    public void SwitchCharacterVariant(int index)
    {
        if (index < characterVariants.Length)
        {
            character.SetCharacterGO(characterVariants[index]);
        }
    }
    
    public void AdjustCharacterHeight(float heightOffset)
    {
        Vector3 currentOffset = character.characterObjectOffset;
        currentOffset.y = heightOffset;
        character.SetCharacterObjectOffset(currentOffset);
    }
}
```

### Animation-Driven Updates
```csharp
public class CharacterAnimationController : MonoBehaviour
{
    public CharacterEntity character;
    
    void Update()
    {
        // Gradually rotate character
        Quaternion currentRotation = character.characterObjectRotation;
        Quaternion newRotation = currentRotation * Quaternion.Euler(0, Time.deltaTime * 90, 0);
        character.SetCharacterObjectRotation(newRotation);
    }
}
```

## Testing

Comprehensive tests are provided in `CharacterEntityRuntimeTests.cs` that validate:

- All new API methods function correctly
- Error handling works as expected
- GameObject transforms are updated properly
- Edge cases are handled gracefully

A demonstration script `CharacterEntityRuntimeDemo.cs` is also provided showing practical usage examples.