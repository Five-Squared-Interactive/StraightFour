// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using FiveSQD.StraightFour.Entity.Voxels.Core;

public class VoxelCoreTests
{
    [Test]
    public void VoxelData_CreateSolid_ReturnsCorrectData()
    {
        // Arrange & Act
        VoxelData voxel = VoxelData.CreateSolid(5);

        // Assert
        Assert.IsTrue(voxel.IsSolid, "Solid voxel should have IsSolid = true");
        Assert.IsFalse(voxel.IsEmpty, "Solid voxel should have IsEmpty = false");
        Assert.AreEqual(5, voxel.materialId, "Material ID should be 5");
        Assert.IsFalse(voxel.IsSmooth, "Solid voxel should not be smooth by default");
    }

    [Test]
    public void VoxelData_CreateEmpty_ReturnsCorrectData()
    {
        // Arrange & Act
        VoxelData voxel = VoxelData.CreateEmpty();

        // Assert
        Assert.IsFalse(voxel.IsSolid, "Empty voxel should have IsSolid = false");
        Assert.IsTrue(voxel.IsEmpty, "Empty voxel should have IsEmpty = true");
        Assert.AreEqual(0, voxel.materialId, "Empty voxel should have material ID 0");
        Assert.IsFalse(voxel.IsSmooth, "Empty voxel should not be smooth");
    }

    [Test]
    public void VoxelData_CreateSmooth_ReturnsCorrectData()
    {
        // Arrange & Act
        VoxelData voxel = VoxelData.CreateSmooth(0.7f, 3);

        // Assert
        Assert.IsTrue(voxel.IsSolid, "Smooth voxel with density 0.7 should be solid");
        Assert.IsTrue(voxel.IsSmooth, "Smooth voxel should have IsSmooth = true");
        Assert.AreEqual(3, voxel.materialId, "Material ID should be 3");
        Assert.Greater((float)voxel.density, 0.6f, "Density should be approximately 0.7");
    }

    [Test]
    public void ChunkData_Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        ChunkData chunk = new ChunkData(new int3(1, 2, 3), 16);

        // Assert
        Assert.AreEqual(new int3(1, 2, 3), chunk.ChunkCoordinates, "Chunk coordinates should match");
        Assert.AreEqual(16, chunk.ChunkSize, "Chunk size should be 16");
        Assert.AreEqual(16 * 16 * 16, chunk.TotalVoxels, "Total voxels should be 16^3");
        Assert.IsTrue(chunk.IsDirty, "New chunk should be marked dirty");
    }

    [Test]
    public void ChunkData_GetIndex_ReturnsCorrectIndex()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 10);

        // Act & Assert
        Assert.AreEqual(0, chunk.GetIndex(0, 0, 0), "Index for (0,0,0) should be 0");
        Assert.AreEqual(1, chunk.GetIndex(1, 0, 0), "Index for (1,0,0) should be 1");
        Assert.AreEqual(10, chunk.GetIndex(0, 1, 0), "Index for (0,1,0) should be 10");
        Assert.AreEqual(100, chunk.GetIndex(0, 0, 1), "Index for (0,0,1) should be 100");
        Assert.AreEqual(111, chunk.GetIndex(1, 1, 1), "Index for (1,1,1) should be 111");
    }

    [Test]
    public void ChunkData_GetCoordinates_ReturnsCorrectCoordinates()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 10);

        // Act & Assert
        Assert.AreEqual(new int3(0, 0, 0), chunk.GetCoordinates(0), "Coordinates for index 0");
        Assert.AreEqual(new int3(1, 0, 0), chunk.GetCoordinates(1), "Coordinates for index 1");
        Assert.AreEqual(new int3(0, 1, 0), chunk.GetCoordinates(10), "Coordinates for index 10");
        Assert.AreEqual(new int3(0, 0, 1), chunk.GetCoordinates(100), "Coordinates for index 100");
        Assert.AreEqual(new int3(1, 1, 1), chunk.GetCoordinates(111), "Coordinates for index 111");
    }

    [Test]
    public void ChunkData_SetAndGetVoxel_WorksCorrectly()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 16);
        VoxelData testVoxel = VoxelData.CreateSolid(7);

        // Act
        chunk.SetVoxel(5, 5, 5, testVoxel);
        VoxelData retrieved = chunk.GetVoxel(5, 5, 5);

        // Assert
        Assert.AreEqual(testVoxel.materialId, retrieved.materialId, "Material ID should match");
        Assert.AreEqual(testVoxel.IsSolid, retrieved.IsSolid, "Solid state should match");
        Assert.IsTrue(chunk.IsDirty, "Chunk should be marked dirty after SetVoxel");
    }

    [Test]
    public void ChunkData_IsValidCoordinate_ValidatesCorrectly()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 16);

        // Act & Assert
        Assert.IsTrue(chunk.IsValidCoordinate(0, 0, 0), "Origin should be valid");
        Assert.IsTrue(chunk.IsValidCoordinate(15, 15, 15), "Max corner should be valid");
        Assert.IsTrue(chunk.IsValidCoordinate(8, 8, 8), "Middle should be valid");
        
        Assert.IsFalse(chunk.IsValidCoordinate(-1, 0, 0), "Negative X should be invalid");
        Assert.IsFalse(chunk.IsValidCoordinate(0, -1, 0), "Negative Y should be invalid");
        Assert.IsFalse(chunk.IsValidCoordinate(0, 0, -1), "Negative Z should be invalid");
        Assert.IsFalse(chunk.IsValidCoordinate(16, 0, 0), "X = chunkSize should be invalid");
        Assert.IsFalse(chunk.IsValidCoordinate(0, 16, 0), "Y = chunkSize should be invalid");
        Assert.IsFalse(chunk.IsValidCoordinate(0, 0, 16), "Z = chunkSize should be invalid");
    }

    [Test]
    public void ChunkData_Fill_FillsAllVoxels()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 8);
        VoxelData fillVoxel = VoxelData.CreateSolid(10);

        // Act
        chunk.Fill(fillVoxel);

        // Assert
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int z = 0; z < 8; z++)
                {
                    VoxelData voxel = chunk.GetVoxel(x, y, z);
                    Assert.AreEqual(10, voxel.materialId, $"Voxel at ({x},{y},{z}) should have material ID 10");
                    Assert.IsTrue(voxel.IsSolid, $"Voxel at ({x},{y},{z}) should be solid");
                }
            }
        }
    }

    [Test]
    public void ChunkData_GetWorldPosition_ReturnsCorrectPosition()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(2, 3, 4), 32);

        // Act
        Vector3 worldPos = chunk.GetWorldPosition();

        // Assert
        Assert.AreEqual(new Vector3(64, 96, 128), worldPos, "World position should be chunk coords * chunk size");
    }

    [Test]
    public void ChunkData_GetVoxelOutOfBounds_ReturnsEmpty()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 16);

        // Act
        VoxelData voxel = chunk.GetVoxel(-1, 0, 0);

        // Assert
        Assert.IsTrue(voxel.IsEmpty, "Out of bounds access should return empty voxel");
    }

    [Test]
    public void ChunkData_SetVoxelOutOfBounds_DoesNotCrash()
    {
        // Arrange
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 16);
        VoxelData testVoxel = VoxelData.CreateSolid(5);

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => chunk.SetVoxel(-1, 0, 0, testVoxel));
        Assert.DoesNotThrow(() => chunk.SetVoxel(16, 0, 0, testVoxel));
    }

    [Test]
    public void VoxelData_DensityThreshold_WorksCorrectly()
    {
        // Test boundary conditions for solid/empty
        VoxelData voxel1 = VoxelData.CreateSmooth(0.4f, 1);
        Assert.IsTrue(voxel1.IsEmpty, "Density 0.4 should be empty");
        Assert.IsFalse(voxel1.IsSolid, "Density 0.4 should not be solid");

        VoxelData voxel2 = VoxelData.CreateSmooth(0.5f, 1);
        Assert.IsFalse(voxel2.IsEmpty, "Density 0.5 should not be empty");
        Assert.IsTrue(voxel2.IsSolid, "Density 0.5 should be solid");

        VoxelData voxel3 = VoxelData.CreateSmooth(0.6f, 1);
        Assert.IsFalse(voxel3.IsEmpty, "Density 0.6 should not be empty");
        Assert.IsTrue(voxel3.IsSolid, "Density 0.6 should be solid");
    }

    [Test]
    public void ChunkData_RoundTripIndexing_Consistent()
    {
        // Test that converting index->coords->index returns the same value
        ChunkData chunk = new ChunkData(new int3(0, 0, 0), 16);

        // Use prime number step to test various positions without testing all 4096 voxels
        const int testStep = 137; // Prime number ensures good coverage across the chunk
        for (int testIndex = 0; testIndex < chunk.TotalVoxels; testIndex += testStep)
        {
            int3 coords = chunk.GetCoordinates(testIndex);
            int reconstructedIndex = chunk.GetIndex(coords.x, coords.y, coords.z);
            
            Assert.AreEqual(testIndex, reconstructedIndex, 
                $"Round trip indexing failed for index {testIndex}");
        }
    }
}
