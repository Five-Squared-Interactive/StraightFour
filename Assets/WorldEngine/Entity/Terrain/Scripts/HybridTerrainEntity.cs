// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
#if USE_DIGGER
using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Operations;
using Digger.Modules.Core.Sources.TerrainInterface;
#endif
using FiveSQD.WebVerse.WorldEngine.Entity.Terrain;
using FiveSQD.WebVerse.WorldEngine.Utilities;
using FiveSQD.WebVerse.WorldEngine.Materials;

namespace FiveSQD.WebVerse.WorldEngine.Entity
{
    /// <summary>
    /// Class for a hybrid terrain entity.
    /// </summary>
    public class HybridTerrainEntity : BaseEntity
    {
        /// <summary>
        /// Enumeration for a voxel operation.
        /// Unset: no operation.
        /// Dig: dig operation.
        /// Build: build operation.
        /// </summary>
        public enum TerrainOperation
        {
            Unset = 0,
            Dig = 1,
            Build = 2
        }

        /// <summary>
        /// Class for a voxel map.
        /// </summary>
        public class VoxelMap
        {
            /// <summary>
            /// Hierarchical dictionary for the voxel map. Top-level dictionary is x indices,
            /// second is y indices, and third is z indices.
            /// </summary>
            private Dictionary<int, Dictionary<int, Dictionary<int, Tuple
                <TerrainOperation, int, TerrainEntityBrushType>>>> map;

            /// <summary>
            /// Constructor for a voxel map.
            /// </summary>
            public VoxelMap()
            {
                map = new Dictionary<int, Dictionary<int, Dictionary
                    <int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>>>();
            }

            /// <summary>
            /// Set a block.
            /// </summary>
            /// <param name="position">Position of the block in voxel coordinates.</param>
            /// <param name="operation">Operation being performed.</param>
            /// <param name="blockIndex">Index of the block/layer.</param>
            /// <param name="brushType">Brush type.</param>
            public void SetBlock(Vector3Int position, TerrainOperation operation, int blockIndex, TerrainEntityBrushType brushType)
            {
                if (position.x < 0 || position.y < 0 || position.z < 0)
                {
                    LogSystem.LogWarning("[VoxelMap->SetBlock] Invalid position.");
                    return;
                }

                Dictionary<int, Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>> xDictionary;
                if (map.ContainsKey(position.x))
                {
                    xDictionary = map[position.x];
                }
                else
                {
                    xDictionary = map[position.x] = new Dictionary<int,
                        Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>>();
                }

                Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>> yDictionary;
                if (xDictionary.ContainsKey(position.y))
                {
                    yDictionary = xDictionary[position.y];
                }
                else
                {
                    yDictionary = xDictionary[position.y]
                        = new Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>();
                }

                if (yDictionary.ContainsKey(position.z))
                {
                    // Check if this operation is inverting a previous one. If so, simply remove the block.
                    Tuple<TerrainOperation, int, TerrainEntityBrushType> existingValue = yDictionary[position.z];
                    if (((existingValue.Item1 == TerrainOperation.Dig && operation == TerrainOperation.Build) ||
                        (existingValue.Item1 == TerrainOperation.Build && operation == TerrainOperation.Dig)) &&
                        (existingValue.Item2 == blockIndex))
                    {
                        RemoveBlock(position);
                        return;
                    }
                }
                yDictionary[position.z] = new Tuple
                    <TerrainOperation, int, TerrainEntityBrushType>(operation, blockIndex, brushType);
            }

            /// <summary>
            /// Remove a block.
            /// </summary>
            /// <param name="position">Index of the block to remove.</param>
            public void RemoveBlock(Vector3Int position)
            {
                if (position.x < 0 || position.y < 0 || position.z < 0)
                {
                    LogSystem.LogWarning("[VoxelMap->RemoveBlock] Invalid position.");
                    return;
                }

                Dictionary<int, Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>> xDictionary;
                if (map.ContainsKey(position.x))
                {
                    xDictionary = map[position.x];
                }
                else
                {
                    xDictionary = map[position.x] = new Dictionary
                        <int, Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>>();
                }

                Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>> yDictionary;
                if (xDictionary.ContainsKey(position.y))
                {
                    yDictionary = xDictionary[position.y];
                }
                else
                {
                    yDictionary = xDictionary[position.y] = new Dictionary
                        <int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>();
                }

                if (yDictionary.ContainsKey(position.z))
                {
                    yDictionary.Remove(position.z);
                }
            }

            /// <summary>
            /// Get the voxel block at a given index.
            /// </summary>
            /// <param name="position">Position of the block to get in voxel coordinates.</param>
            /// <returns>The voxel block at the given index (tuple of operations, block index, and brush type),
            /// or (unset, -1) if it doesn't exist.</returns>
            public Tuple<TerrainOperation, int, TerrainEntityBrushType> GetBlock(Vector3Int position)
            {
                if (position.x < 0 || position.y < 0 || position.z < 0)
                {
                    LogSystem.LogWarning("[VoxelMap->GetBlock] Invalid position.");
                    return new Tuple<TerrainOperation, int, TerrainEntityBrushType>(
                        TerrainOperation.Unset, -1, TerrainEntityBrushType.sphere);
                }

                if (map.ContainsKey(position.x))
                {
                    Dictionary<int, Dictionary<
                        int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>> xDictionary = map[position.x];
                    if (xDictionary.ContainsKey(position.y))
                    {
                        Dictionary<int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>
                            yDictionary = xDictionary[position.y];
                        if (yDictionary.ContainsKey(position.z))
                        {
                            return yDictionary[position.z];
                        }
                    }
                }

                return new Tuple<TerrainOperation, int, TerrainEntityBrushType>
                    (TerrainOperation.Unset, -1, TerrainEntityBrushType.sphere);
            }

            /// <summary>
            /// Get all voxel blocks.
            /// </summary>
            /// <returns>A dictionary of voxel block coordinates,
            /// and a tuple of operations, block indices, and brush types.</returns>
            public Dictionary<Vector3Int, Tuple<TerrainOperation, int, TerrainEntityBrushType>> GetBlocks()
            {
                Dictionary<Vector3Int, Tuple<TerrainOperation, int, TerrainEntityBrushType>> outVal
                    = new Dictionary<Vector3Int, Tuple<TerrainOperation, int, TerrainEntityBrushType>>();

                foreach (int xIdx in map.Keys)
                {
                    Dictionary<int, Dictionary<int, Tuple<
                        TerrainOperation, int, TerrainEntityBrushType>>> xDict = map[xIdx];
                    foreach (int yIdx in xDict.Keys)
                    {
                        Dictionary<int, Tuple<TerrainOperation, int,
                            TerrainEntityBrushType>> yDict = xDict[yIdx];
                        foreach (int zIdx in yDict.Keys)
                        {
                            outVal.Add(new Vector3Int(xIdx, yIdx, zIdx), yDict[zIdx]);
                        }
                    }
                }

                return outVal;
            }
        }

        /// <summary>
        /// Terrain object for the hybrid terrain entity.
        /// </summary>
        public UnityEngine.Terrain terrain;

        /// <summary>
        /// Terrain collider for the hybrid terrain entity.
        /// </summary>
        public TerrainCollider terrainCollider;

        public uint bufferSize = 1;

        /// <summary>
        /// Highlight cube for the hybrid terrain entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// Base heights values.
        /// </summary>
        private float[,] baseHeights;

        /// <summary>
        /// Voxel values.
        /// </summary>
        private VoxelMap voxelValues;

#if USE_DIGGER
        /// <summary>
        /// Digger system.
        /// </summary>
        private DiggerSystem diggerSystem;
#endif

        /// <summary>
        /// Terrain material.
        /// </summary>
        private Material terrainMaterial;

        /// <summary>
        /// Mesh materials.
        /// </summary>
        private Material[] meshMaterials;

        /// <summary>
        /// TerrainEntityLayers
        /// </summary>
        private List<TerrainEntityLayer> terrainEntityLayers;

        /// <summary>
        /// Count of textures per pass.
        /// </summary>
        private const int TxtCountPerPass = 4;

        /// <summary>
        /// Maximum passes.
        /// </summary>
        private const int MaxPassCount = 4;

        /// <summary>
        /// Whether or not to enable height blending in the shader.
        /// </summary>
        private static readonly int EnableHeightBlend = Shader.PropertyToID("_EnableHeightBlend");

        /// <summary>
        /// Height transition in the shader.
        /// </summary>
        private static readonly int HeightTransition = Shader.PropertyToID("_HeightTransition");

        /// <summary>
        /// Terrain width inversion property in the shader.
        /// </summary>
        private static readonly int TerrainWidthInvProperty = Shader.PropertyToID("_TerrainWidthInv");

        /// <summary>
        /// Terrain height inversion property in the shader.
        /// </summary>
        private static readonly int TerrainHeightInvProperty = Shader.PropertyToID("_TerrainHeightInv");

        /// <summary>
        /// Whether or not to enable per pixel instancing in the shader.
        /// </summary>
        private static readonly int EnableInstancedPerPixelNormal = Shader.PropertyToID("_EnableInstancedPerPixelNormal");

        /// <summary>
        /// Whether or not modification operations are running.
        /// </summary>
        private bool modifying;

#if USE_DIGGER
        /// <summary>
        /// Basic operation.
        /// </summary>
        private readonly BasicOperation basicOperation = new BasicOperation();

        /// <summary>
        /// Kernel operation.
        /// </summary>
        private readonly KernelOperation kernelOperation = new KernelOperation();

        /// <summary>
        /// Modification operation buffer.
        /// </summary>
        private readonly Queue<ModificationParameters> modBuf = new Queue<ModificationParameters>();
#endif

        /// <summary>
        /// Create a hybrid terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="id">ID for the terrain.</param>
        /// <param name="layers">Array of layers for the terrain.</param>
        /// <param name="layerMasks">Dictionary of layer indices and layer masks for the terrain.</param>
        /// <returns>The requested hybrid terrain entity.</returns>
        public static HybridTerrainEntity Create(float length, float width, float height,
            float[,] heights, TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks, Guid id)
        {
            GameObject terrainGO = new GameObject("HybridTerrainEntity-" + id.ToString());
            HybridTerrainEntity terrainEntity = terrainGO.AddComponent<HybridTerrainEntity>();
            TerrainData terrainData = new TerrainData();
            GameObject terrainObject = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.transform.SetParent(terrainGO.transform);
            terrainEntity.terrain = terrainObject.GetComponent<UnityEngine.Terrain>();
            terrainEntity.terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            terrainEntity.Initialize(id, layers, layerMasks);
            terrainEntity.SetHeights(length, width, height, heights);
            return terrainEntity;
        }

        /// <summary>
        /// Perform a dig operation.
        /// </summary>
        /// <param name="position">Position of the dig.</param>
        /// <param name="brushType">Brush to use.</param>
        /// <param name="layerIndex">Index of the layer to perform the dig on.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Dig(Vector3 position, TerrainEntityBrushType brushType, int layerIndex, bool synchronize = true)
        {
#if USE_DIGGER
            if (modBuf.Count >= bufferSize)
            {
                return false;
            }

            BrushType bt;
            switch (brushType)
            {
                case TerrainEntityBrushType.roundedCube:
                    bt = BrushType.RoundedCube;
                    break;

                case TerrainEntityBrushType.sphere:
                default:
                    bt = BrushType.Sphere;
                    break;
            }

            modBuf.Enqueue(new ModificationParameters
            {
                Position = position,
                Brush = bt,
                Action = ActionType.Dig,
                TextureIndex = layerIndex,
                Opacity = 0.5f,
                Size = 1,
                StalagmiteUpsideDown = false,
                OpacityIsTarget = false,
                Callback = null
            });

            voxelValues.SetBlock(GetVoxelPosition(position), TerrainOperation.Dig, layerIndex, brushType);
#endif

            if (synchronize && synchronizer != null)
            {
                synchronizer.ModifyTerrainEntity(this,
                    TerrainOperation.Dig, position, brushType, layerIndex);
            }

            return true;
        }

        /// <summary>
        /// Perform a build operation.
        /// </summary>
        /// <param name="position">Position of the build.</param>
        /// <param name="brushType">Brush to use.</param>
        /// <param name="layerIndex">Index of the layer to perform the build on.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Build(Vector3 position, TerrainEntityBrushType brushType, int layerIndex, bool synchronize = true)
        {
#if USE_DIGGER
            if (modBuf.Count >= bufferSize)
            {
                return false;
            }

            BrushType bt;
            switch (brushType)
            {
                case TerrainEntityBrushType.roundedCube:
                    bt = BrushType.RoundedCube;
                    break;

                case TerrainEntityBrushType.sphere:
                default:
                    bt = BrushType.Sphere;
                    break;
            }

            modBuf.Enqueue(new ModificationParameters
            {
                Position = position,
                Brush = bt,
                Action = ActionType.Dig,
                TextureIndex = layerIndex,
                Opacity = 0.5f,
                Size = 1,
                StalagmiteUpsideDown = false,
                OpacityIsTarget = false,
                Callback = null
            });

            voxelValues.SetBlock(GetVoxelPosition(position), TerrainOperation.Build, layerIndex, brushType);
#endif

            if (synchronize && synchronizer != null)
            {
                synchronizer.ModifyTerrainEntity(this,
                    TerrainOperation.Build, position, brushType, layerIndex);
            }

            return true;
        }

        /// <summary>
        /// Get the block at a given position.
        /// </summary>
        /// <param name="position">Position to get the block at.</param>
        /// <returns>Block at the given position (tuple of the operation and layer index).</returns>
        public Tuple<TerrainOperation, int, TerrainEntityBrushType> GetBlockAtPosition(Vector3 position)
        {
            return voxelValues.GetBlock(GetVoxelPosition(position));
        }

        /// <summary>
        /// Delete the hybrid terrain entity.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public override bool Delete()
        {
            return base.Delete();
        }

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public override EntityMotion? GetMotion()
        {
            return new EntityMotion
            {
                angularVelocity = Vector3.zero,
                stationary = true,
                velocity = Vector3.zero
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            return new EntityPhysicalProperties()
            {
                angularDrag = float.PositiveInfinity,
                centerOfMass = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
                drag = float.PositiveInfinity,
                gravitational = false,
                mass = float.PositiveInfinity
            };
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            return terrain.terrainData.size;
        }

        /// <summary>
        /// Set the highlight state of the entity.
        /// </summary>
        /// <param name="highlight">Whether or not to enable highlighting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override bool SetHighlight(bool highlight)
        {
            highlightCube.SetActive(highlight);

            return true;
        }

        /// <summary>
        /// Get the highlight state of the entity.
        /// </summary>
        /// <returns>The highlight state of the entity.</returns>
        public override bool GetHighlight()
        {
            return highlightCube.activeSelf;
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetInteractionState(InteractionState stateToSet)
        {
            switch (stateToSet)
            {
                case InteractionState.Physical:
                    MakePhysical();
                    return true;

                case InteractionState.Placing:
                    LogSystem.LogWarning("[HybridTerrainEntity->SetInteractionState] Placing not valid for terrain.");
                    return false;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("[HybridTerrainEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Set the motion state for this entity.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetMotion(EntityMotion? motionToSet)
        {
            LogSystem.LogWarning("[HybridTerrainEntity->SetMotion] Motion not settable for light.");

            return false;
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPhysicalProperties(EntityPhysicalProperties? propertiesToSet)
        {
            return base.SetPhysicalProperties(propertiesToSet);
        }

        /// <summary>
        /// Set the size of the entity.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            terrain.terrainData.size = size;

            return true;
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible)
        {
            terrain.drawHeightmap = visible;

            return true;
        }

        /// <summary>
        /// Get the visibility state of the entity.
        /// </summary>
        /// <returns>The visibility state of the entity.</returns>
        public override bool GetVisibility()
        {
            return terrain.drawHeightmap;
        }

        /// <summary>
        /// Get the base heights values.
        /// </summary>
        /// <returns>The base heights values.</returns>
        public float[,] GetBaseHeights()
        {
            return baseHeights;
        }

        /// <summary>
        /// Set the heights of the hybrid terrain.
        /// </summary>
        /// <param name="newLength">New length in meters of the terrain.</param>
        /// <param name="newWidth">New width in meters of the terrain.</param>
        /// <param name="newHeight">New height in meters of the terrain.</param>
        /// <param name="newHeights">New array of heights for the terrain. Array will be distributed across the terrain.
        /// For example, a terrain that is 8 meters long and has a newHeights array of 16 elements will have one data point
        /// for every half meter, Length and width of array cannot exceed 4097 units.</param>
        private void SetHeights(float newLength, float newWidth, float newHeight, float[,] newHeights)
        {
            // ------- Input Validation -------
            if (newLength < 1 || newWidth < 1 || newHeight < 1)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->SetHeights] New length, width, and height must be greater than 0.");
                return;
            }

            if (newHeights == null)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->SetHeights] Invalid new heights array.");
                return;
            }

            int newTerrainArrayLength = newHeights.GetLength(0);
            int newTerrainArrayWidth = newHeights.GetLength(1);

            if (newTerrainArrayLength > 4097 || newTerrainArrayWidth > 4097 ||
                newTerrainArrayLength < 1 || newTerrainArrayWidth < 1)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->SetHeights] Terrain cannot contain less than 1 or more than 4097 elements in any direction.");
                return;
            }

            // ------- Size New Terrain Object -------
            int newTerrainDataSize = GetTerrainSize(Math.Max(newLength, newWidth));

            // ------- Fit New Terrain Data to New Terrain Object -------
            float[,] newFittedHeights = new float[newTerrainDataSize, newTerrainDataSize];
            float xCapacityToDataRatio = (float) newTerrainArrayLength / newTerrainDataSize;
            float yCapacityToDataRatio = (float) newTerrainArrayWidth / newTerrainDataSize;
            for (int i = 0; i < newTerrainDataSize; i++)
            {
                // Index of the lower x element in the data array to use.
                int lowerXIdx = (int) (i * xCapacityToDataRatio);

                // Index of the upper x element in the data array to use.
                int upperXIdx = Math.Min((int) (i * xCapacityToDataRatio + 1), newTerrainArrayLength - 1);

                // Percentage between the lower and upper x elements to interpolate.
                float xInterpolationPercentage = lowerXIdx == upperXIdx ? 0 : (i - lowerXIdx / xCapacityToDataRatio) * xCapacityToDataRatio;

                // Run for each y element.
                for (int j = 0; j < newTerrainDataSize; j++)
                {
                    // Index of the lower y element in the data array to use.
                    int lowerYIdx = (int) (j * yCapacityToDataRatio);

                    // Index of the upper y element in the data array to use.
                    int upperYIdx = Math.Min((int) (j * yCapacityToDataRatio + 1), newTerrainArrayWidth - 1);

                    // Percentage between the lower and upper y elements to interpolate.
                    float yInterpolationPercentage = lowerYIdx == upperYIdx ? 0 : (j - lowerYIdx / yCapacityToDataRatio) * yCapacityToDataRatio;

                    // Get value between two elements on x axis.
                    float xInterpolation = newHeights[lowerXIdx, lowerYIdx]
                        + xInterpolationPercentage * (newHeights[upperXIdx, lowerYIdx] - newHeights[lowerXIdx, lowerYIdx]);

                    // Get value between two elements on y axis.
                    float yInterpolation = newHeights[lowerXIdx, lowerYIdx]
                        + yInterpolationPercentage * (newHeights[lowerXIdx, upperYIdx] - newHeights[lowerXIdx, lowerYIdx]);

                    // Average two axes to get single height value.
                    float newHeightValue = xInterpolation + yInterpolation / 2;

                    // Assign height value.
                    newFittedHeights[i, j] = newHeightValue / newHeight;
                }
            }

            // ------- Set Up Terrain -------
            terrain.terrainData.heightmapResolution = newTerrainDataSize;
            terrain.terrainData.SetHeights(0, 0, newFittedHeights);
            terrain.terrainData.size = new Vector3(newLength, newHeight, newWidth);
            baseHeights = newHeights;
        }

        /// <summary>
        /// Get a 2d array representing the base heightmap for the terrain.
        /// </summary>
        /// <returns>A 2d array representing the base heightmap for the terrain.</returns>
        public float[,] GetBaseHeightmap()
        {
            return baseHeights;
        }

        /// <summary>
        /// Get the height from the base heightmap at a given x, y index.
        /// </summary>
        /// <param name="xIndex">X index to get height at.</param>
        /// <param name="yIndex">Y index to get height at.</param>
        /// <returns>Height from the base heightmap at given index.</returns>
        public float GetHeight(int xIndex, int yIndex)
        {
            //return terrain.terrainData.GetHeight(xIndex, yIndex) * terrain.terrainData.size.y;

            if (baseHeights.GetLength(0) <= xIndex || baseHeights.GetLength(1) <= yIndex)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetHeight] Invalid index.");
                return 0;
            }

            return baseHeights[xIndex, yIndex];
        }

        /// <summary>
        /// Get the layer at a given index.
        /// </summary>
        /// <param name="index">Index of the layer to get.</param>
        /// <returns>The layer at a given index, or null if none.</returns>
        public TerrainEntityLayer? GetLayer(int index)
        {
            if (index < 0)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetLayer] Index must be greater than 0.");
                return null;
            }

            if (terrain.terrainData.terrainLayers == null)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetLayer] No layers.");
                return null;
            }

            if (index >= terrain.terrainData.terrainLayers.Length)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetLayer] Invalid index "
                    + index + ". Only " + terrain.terrainData.terrainLayers.Length + " layers.");
                return null;
            }

            TerrainLayer layerOfInterest = terrain.terrainData.terrainLayers[index];
            return new TerrainEntityLayer
            {
                diffuse = layerOfInterest.diffuseTexture,
                normal = layerOfInterest.normalMapTexture,
                mask = layerOfInterest.maskMapTexture,
                specular = layerOfInterest.specular,
                metallic = layerOfInterest.metallic,
                smoothness = layerOfInterest.smoothness
            };
        }

        /// <summary>
        ///  Get the layers for this terrain entity.
        /// </summary>
        /// <returns>The layers for this terrain entity.</returns>
        public TerrainEntityLayer[] GetLayers()
        {
            if (terrain.terrainData.terrainLayers == null)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetLayer] No layers.");
                return null;
            }

            List<TerrainEntityLayer> layers = new List<TerrainEntityLayer>();
            foreach (TerrainLayer layer in terrain.terrainData.terrainLayers)
            {
                layers.Add(new TerrainEntityLayer()
                    {
                        diffuse = layer.diffuseTexture,
                        normal = layer.normalMapTexture,
                        mask = layer.maskMapTexture,
                        specular = layer.specular,
                        metallic = layer.metallic,
                        smoothness = layer.smoothness
                    });
            }

            return layers.ToArray();
        }

        /// <summary>
        /// Get a layer mask.
        /// </summary>
        /// <param name="layer">Layer index.</param>
        /// <returns>A layer mask (a 2d array of values between 0 and 1 corresponding to the
        /// intensity of the given layer).</returns>
        public float[,] GetLayerMask(int layer)
        {
            if (layer < 0)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetLayerMask] Index must be greater than 0.");
                return null;
            }

            float[,] mask = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight];
            float[,,] map = terrain.terrainData.GetAlphamaps(0, 0, mask.GetLength(0), mask.GetLength(1));
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                for (int j = 0; j < mask.GetLength(1); j++)
                {
                    mask[i, j] = map[i, j, layer];
                }
            }

            return mask;
        }

        /// <summary>
        /// Get the layer masks for this terrain entity.
        /// </summary>
        /// <returns>The layer masks for this terrain entity.</returns>
        public Dictionary<int, float[,]> GetLayerMasks()
        {
            Dictionary<int, float[,]> layerMasks = new Dictionary<int, float[,]>();
            for (int i = 0; i < terrain.terrainData.terrainLayers.Length; i++)
            {
                layerMasks.Add(i, GetLayerMask(i));
            }
            return layerMasks;
        }

        /// <summary>
        /// Get the terrain modifications for this terrain entity.
        /// </summary>
        /// <returns>The terrain modifications for this terrain entity.</returns>
        public Dictionary<Vector3Int, Tuple<TerrainOperation, int, TerrainEntityBrushType>> GetTerrainModifications()
        {
            return voxelValues.GetBlocks();
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(Guid idToSet)
        {
            LogSystem.LogError("[HybridTerrainEntity->Initialize] Hybrid terrain entity must be initialized with layers and layer masks.");

            return;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="layers">Array of layers for the terrain.</param>
        /// <param name="layerMasks">Dictionary of layer indices and layer masks for the terrain.</param>
        public void Initialize(Guid idToSet, TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks)
        {
            if (layers == null || layers.Length < 1)
            {
                LogSystem.LogError("[HybridTerrainEntity->Initialize] Hybrid terrain entity must be initialized with at least one layer.");
                return;
            }

            meshMaterials = new Material[MaxPassCount];

            terrainEntityLayers = new List<TerrainEntityLayer>();

            // Set up layers.
            foreach (TerrainEntityLayer layer in layers)
            {
                AddLayer(layer);
                terrainEntityLayers.Add(layer);
            }

            // Set up layer masks.
            foreach (KeyValuePair<int, float[,]> layerMask in layerMasks)
            {
                SetLayerMask(layerMask.Key, layerMask.Value);
            }

            base.Initialize(idToSet);

            voxelValues = new VoxelMap();

            SetUpDiggerSystem();

            SetUpHighlightVolume();

            MakeHidden();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Make the entity hidden.
        /// </summary>
        private void MakeHidden()
        {
            switch (interactionState)
            {
                case InteractionState.Physical:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Hidden:
                default:
                    break;
            }

            terrainCollider.enabled = false;
            terrain.enabled = false;
            gameObject.SetActive(false);
            interactionState = InteractionState.Hidden;
        }

        /// <summary>
        /// Make the entity static.
        /// </summary>
        private void MakeStatic()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    break;

                case InteractionState.Physical:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            terrain.enabled = true;
            terrainCollider.enabled = false;
            interactionState = InteractionState.Static;
        }

        /// <summary>
        /// Make the entity physical.
        /// </summary>
        private void MakePhysical()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Physical:
                default:
                    break;
            }

            gameObject.SetActive(true);
            terrain.enabled = true;
            terrainCollider.enabled = true;
            interactionState = InteractionState.Physical;
        }

        /// <summary>
        /// Set up the highlight volume for the entity.
        /// </summary>
        private void SetUpHighlightVolume()
        {
            highlightCube = new GameObject("HighlightVolume");

            Vector3[] vertices =
            {
                    new Vector3(terrain.terrainData.bounds.min.x, terrain.terrainData.bounds.min.y, terrain.terrainData.bounds.min.z),
                    new Vector3 (terrain.terrainData.bounds.max.x, terrain.terrainData.bounds.min.y, terrain.terrainData.bounds.min.z),
                    new Vector3 (terrain.terrainData.bounds.max.x, terrain.terrainData.bounds.max.y, terrain.terrainData.bounds.min.z),
                    new Vector3 (terrain.terrainData.bounds.min.x, terrain.terrainData.bounds.min.y, terrain.terrainData.bounds.min.z),
                    new Vector3 (terrain.terrainData.bounds.min.x, terrain.terrainData.bounds.max.y, terrain.terrainData.bounds.max.z),
                    new Vector3 (terrain.terrainData.bounds.max.x, terrain.terrainData.bounds.max.y, terrain.terrainData.bounds.max.z),
                    new Vector3 (terrain.terrainData.bounds.max.x, terrain.terrainData.bounds.min.y, terrain.terrainData.bounds.max.z),
                    new Vector3 (terrain.terrainData.bounds.min.x, terrain.terrainData.bounds.min.y, terrain.terrainData.bounds.max.z),
                };

            int[] triangles =
            {
                    0, 2, 1, //face front
			        0, 3, 2,
                    2, 3, 4, //face top
			        2, 4, 5,
                    1, 2, 5, //face right
			        1, 5, 6,
                    0, 7, 4, //face left
			        0, 4, 3,
                    5, 4, 7, //face back
			        5, 7, 6,
                    0, 6, 7, //face bottom
			        0, 1, 6
                };

            Mesh mesh = highlightCube.AddComponent<MeshFilter>().mesh;
            MeshRenderer hRend = highlightCube.AddComponent<MeshRenderer>();
            hRend.material = MaterialManager.HighlightMaterial;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();

            highlightCube.transform.SetParent(transform);
            highlightCube.transform.localPosition = Vector3.zero;
            highlightCube.transform.localRotation = Quaternion.identity;
            highlightCube.transform.localScale = Vector3.one;
            highlightCube.SetActive(false);
        }

        /// <summary>
        /// Get the size of the terrain.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static int GetTerrainSize(float size)
        {
            if (size < 1)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetTerrainSize] Size too small.");
                return 33;
            }
            else if (size <= 33)
            {
                return 33;
            }
            else if (size <= 65)
            {
                return 65;
            }
            else if (size <= 129)
            {
                return 129;
            }
            else if (size <= 257)
            {
                return 257;
            }
            else if (size <= 513)
            {
                return 513;
            }
            else if (size <= 1025)
            {
                return 1025;
            }
            else if (size <= 2049)
            {
                return 2049;
            }
            else if (size <= 4097)
            {
                return 4097;
            }
            else
            {
                LogSystem.LogWarning("[HybridTerrainEntity->GetTerrainSize] Size too large.");
                return 4097;
            }
        }

        /// <summary>
        /// Set up the digger system.
        /// </summary>
        private void SetUpDiggerSystem()
        {
#if USE_DIGGER
            GameObject diggerGO = new GameObject("Digger");
            diggerGO.transform.parent = terrain.transform;
            diggerSystem = diggerGO.AddComponent<DiggerSystem>();
            diggerSystem.Terrain = terrain;
            diggerSystem.PreInit(false);
            diggerSystem.ShowDebug = true;
            TerrainCutter.CreateInstance(diggerSystem);
            diggerSystem.Terrain.terrainData.enableHolesTextureCompression = false;
            diggerSystem.Init(Application.isEditor ? LoadType.Minimal : LoadType.Minimal_and_LoadVoxels);

            SetUpMaterials(diggerSystem, true);
#endif
        }

        /// <summary>
        /// Add a layer to the hybrid terrain entity.
        /// </summary>
        /// <param name="layer">Layer to add to the hybrid terrain entity.</param>
        /// <returns>Index of the new layer, or -1 if failed.</returns>
        private int AddLayer(TerrainEntityLayer layer)
        {
            if (layer.metallic < 0 || layer.metallic > 1)
            {
                LogSystem.LogWarning("[TerrainEntity->AddLayer] Invalid metallic value. Must be between 0 and 1.");
                return -1;
            }

            if (layer.smoothness < 0 || layer.smoothness > 1)
            {
                LogSystem.LogWarning("[TerrainEntity->AddLayer] Invalid smoothness value. Must be between 0 and 1.");
                return -1;
            }

            TerrainLayer[] oldLayers = terrain.terrainData.terrainLayers;
            List<TerrainLayer> newLayers = new List<TerrainLayer>();
            if (oldLayers != null)
            {
                foreach (TerrainLayer oldLayer in oldLayers)
                {
                    newLayers.Add(oldLayer);
                }
            }

            TerrainLayer newLayer = new TerrainLayer();
            newLayer.diffuseTexture = layer.diffuse;
            newLayer.normalMapTexture = layer.normal;
            newLayer.maskMapTexture = layer.mask;
            newLayer.specular = layer.specular;
            newLayer.metallic = layer.metallic;
            newLayer.smoothness = layer.smoothness;
            newLayers.Add(newLayer);

            terrain.terrainData.terrainLayers = newLayers.ToArray();
            return newLayers.Count - 1;
        }

        /// <summary>
        /// Set the mask for a particular layer.
        /// </summary>
        /// <param name="layer">Layer index.</param>
        /// <param name="mask">Mask (a 2d array of values between 0 and 1 corresponding to the
        /// intensity of the given layer to set).</param>
        /// <returns>Whether or not the setting was successful.</returns>
        private bool SetLayerMask(int layer, float[,] mask)
        {
            if (layer < 0)
            {
                LogSystem.LogWarning("[TerrainEntity->SetLayerMask] Index must be greater than 0.");
                return false;
            }

            if (terrain.terrainData.terrainLayers == null)
            {
                LogSystem.LogWarning("[TerrainEntity->SetLayerMask] No layers.");
                return false;
            }

            if (layer >= terrain.terrainData.terrainLayers.Length)
            {
                LogSystem.LogWarning("[TerrainEntity->SetLayerMask] Invalid index "
                    + layer + ". Only " + terrain.terrainData.terrainLayers.Length + " layers.");
                return false;
            }

            float[,,] map = terrain.terrainData.GetAlphamaps(0, 0, mask.GetLength(0), mask.GetLength(1));
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                for (int j = 0; j < mask.GetLength(1); j++)
                {
                    map[i, j, layer] = mask[i, j];
                }
            }

            terrain.terrainData.SetAlphamaps(0, 0, map);

            return true;
        }

#if USE_DIGGER
        /// <summary>
        /// Set up materials.
        /// </summary>
        /// <param name="diggerSystem">Digger system.</param>
        /// <param name="forceRefresh">Whether or not to force a refresh.</param>
        private void SetUpMaterials(DiggerSystem diggerSystem, bool forceRefresh)
        {
            SetUpTerrainMaterial(diggerSystem, forceRefresh);

            var tData = diggerSystem.Terrain.terrainData;
            var passCount = GetPassCount(tData);

            if (diggerSystem.Materials == null || diggerSystem.Materials.Length != passCount)
            {
                diggerSystem.Materials = new Material[passCount];
            }

            var textures = new List<Texture2D>();
            for (var pass = 0; pass < passCount; ++pass)
            {
                SetUpMaterial(pass, diggerSystem, textures);
            }

            var warnUseOpacityAsDensity = -1;
            for (var i = 0; i < tData.terrainLayers.Length; i++)
            {
                if (tData.terrainLayers[i].diffuseRemapMin.w > 0.1f)
                {
                    warnUseOpacityAsDensity = i;
                    break;
                }
            }

            if (warnUseOpacityAsDensity >= 0)
            {
                LogSystem.LogWarning("[HybridTerrainEntity->SetUpURPMaterials] Terrain layer " + tData.terrainLayers[warnUseOpacityAsDensity].name +
                    "is not set up correctly.");
            }

            diggerSystem.TerrainTextures = textures.ToArray();
        }

        /// <summary>
        /// Set up a material.
        /// </summary>
        /// <param name="pass">Pass of the material.</param>
        /// <param name="diggerSystem">Digger system.</param>
        /// <param name="textures">Textures.</param>
        private void SetUpMaterial(int pass, DiggerSystem diggerSystem, List<Texture2D> textures)
        {
            meshMaterials[pass] = diggerSystem.Materials[pass];
            var expectedShaderName = $"Digger/Mesh/URP/Lit-Pass{pass}";
            if (!meshMaterials[pass] || meshMaterials[pass].shader.name != expectedShaderName)
            {
                meshMaterials[pass] = new Material(Shader.Find(expectedShaderName));
            }

#if USING_URP_14_OR_ABOVE
            material.EnableKeyword("USING_URP_14_OR_ABOVE");
#endif

            var tData = diggerSystem.Terrain.terrainData;

            if (tData.terrainLayers.Length <= 4 && diggerSystem.Terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_BLEND_HEIGHT"))
            {
                meshMaterials[pass].EnableKeyword("_TERRAIN_BLEND_HEIGHT");
                meshMaterials[pass].SetFloat(EnableHeightBlend, 1);
                meshMaterials[pass].SetFloat(HeightTransition, diggerSystem.Terrain.materialTemplate.GetFloat("_HeightTransition"));
            }
            else
            {
                meshMaterials[pass].DisableKeyword("_TERRAIN_BLEND_HEIGHT");
                meshMaterials[pass].SetFloat(EnableHeightBlend, 0);
            }

            var normalmap = false;
            var maskmap = false;
            var offset = pass * TxtCountPerPass;
            for (var i = 0; i + offset < tData.terrainLayers.Length && i < TxtCountPerPass; i++)
            {
                var terrainLayer = tData.terrainLayers[i + offset];
                if (terrainLayer == null || terrainLayer.diffuseTexture == null)
                    continue;

                if (terrainLayer.normalMapTexture)
                    normalmap = true;
                if (terrainLayer.maskMapTexture)
                    maskmap = true;

                meshMaterials[pass].SetFloat($"_tiles{i}x", 1.0f / terrainLayer.tileSize.x);
                meshMaterials[pass].SetFloat($"_tiles{i}y", 1.0f / terrainLayer.tileSize.y);
                meshMaterials[pass].SetFloat($"_offset{i}x", terrainLayer.tileOffset.x);
                meshMaterials[pass].SetFloat($"_offset{i}y", terrainLayer.tileOffset.y);
                meshMaterials[pass].SetFloat($"_NormalScale{i}", terrainLayer.normalScale);
                meshMaterials[pass].SetFloat($"_LayerHasMask{i}", terrainLayer.maskMapTexture ? 1 : 0);
                meshMaterials[pass].SetFloat($"_Metallic{i}", terrainLayer.metallic);
                meshMaterials[pass].SetFloat($"_Smoothness{i}", terrainLayer.smoothness);
                meshMaterials[pass].SetTexture($"_Splat{i}", terrainLayer.diffuseTexture);
                meshMaterials[pass].SetTexture($"_Normal{i}", terrainLayer.normalMapTexture);
                meshMaterials[pass].SetTexture($"_Mask{i}", terrainLayer.maskMapTexture);
                meshMaterials[pass].SetVector($"_MaskMapRemapOffset{i}", terrainLayer.maskMapRemapMin);
                meshMaterials[pass].SetVector($"_MaskMapRemapScale{i}", terrainLayer.maskMapRemapMax);
                meshMaterials[pass].SetVector($"_DiffuseRemapScale{i}", terrainLayer.diffuseRemapMax - terrainLayer.diffuseRemapMin);
                meshMaterials[pass].SetTextureScale($"_Splat{i}", new Vector2(1f / terrainLayer.tileSize.x, 1f / terrainLayer.tileSize.y));
                meshMaterials[pass].SetTextureOffset($"_Splat{i}", terrainLayer.tileOffset);
                textures.Add(terrainLayer.diffuseTexture);
            }

            if (normalmap)
            {
                meshMaterials[pass].EnableKeyword("_NORMALMAP");
            }
            else
            {
                meshMaterials[pass].DisableKeyword("_NORMALMAP");
            }

            if (maskmap)
            {
                meshMaterials[pass].EnableKeyword("_MASKMAP");
            }
            else
            {
                meshMaterials[pass].DisableKeyword("_MASKMAP");
            }

            meshMaterials[pass].name = $"meshMaterialPass{pass}";
            diggerSystem.Materials[pass] = meshMaterials[pass];
        }

        /// <summary>
        /// Set up a terrain material.
        /// </summary>
        /// <param name="diggerSystem">Digger system.</param>
        /// <param name="forceRefresh">Whether or not to force a refresh.</param>
        private void SetUpTerrainMaterial(DiggerSystem diggerSystem, bool forceRefresh)
        {
            var terrainAlreadyHasDiggerMaterial = diggerSystem.Terrain.materialTemplate &&
                                                  diggerSystem.Terrain.materialTemplate.shader.name == "Digger/Terrain/URP/Lit";

            if (forceRefresh || !terrainAlreadyHasDiggerMaterial)
            {
                terrainMaterial = new Material(Shader.Find("Digger/Terrain/URP/Lit"));
#if USING_URP_14_OR_ABOVE
                terrainMaterial.EnableKeyword("USING_URP_14_OR_ABOVE");
#endif
                terrainMaterial.SetFloat(TerrainWidthInvProperty, 1f / diggerSystem.Terrain.terrainData.size.x);
                terrainMaterial.SetFloat(TerrainHeightInvProperty, 1f / diggerSystem.Terrain.terrainData.size.z);
                if (diggerSystem.Terrain.materialTemplate && diggerSystem.Terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_BLEND_HEIGHT"))
                {
                    terrainMaterial.EnableKeyword("_TERRAIN_BLEND_HEIGHT");
                    terrainMaterial.SetFloat(EnableHeightBlend, 1);
                }
                else
                {
                    terrainMaterial.DisableKeyword("_TERRAIN_BLEND_HEIGHT");
                    terrainMaterial.SetFloat(EnableHeightBlend, 0);
                }

                if (diggerSystem.Terrain.materialTemplate && diggerSystem.Terrain.materialTemplate.IsKeywordEnabled("ENABLE_TERRAIN_PERPIXEL_NORMAL"))
                {
                    terrainMaterial.EnableKeyword("ENABLE_TERRAIN_PERPIXEL_NORMAL");
                    terrainMaterial.SetFloat(EnableInstancedPerPixelNormal, 1);
                }
                else
                {
                    terrainMaterial.DisableKeyword("ENABLE_TERRAIN_PERPIXEL_NORMAL");
                    terrainMaterial.SetFloat(EnableInstancedPerPixelNormal, 0);
                }

                if (diggerSystem.Terrain.materialTemplate)
                {
                    terrainMaterial.SetFloat(HeightTransition, diggerSystem.Terrain.materialTemplate.GetFloat(HeightTransition));
                }

                terrainMaterial.name = "terrainMaterial";
                diggerSystem.Terrain.materialTemplate = terrainMaterial;
            }

            if (diggerSystem.Terrain.materialTemplate.shader.name != "Digger/Terrain/URP/Lit")
            {
                LogSystem.LogWarning("[HybridTerrainEntity->SetUpTerrainMaterial] Invalid shader setting for terrain material.");
            }
        }

        /// <summary>
        /// Get the number of passes.
        /// </summary>
        /// <param name="tData">Terrain data.</param>
        /// <returns>Number of passes.</returns>
        private static int GetPassCount(TerrainData tData)
        {
            var passCount = tData.terrainLayers.Length / TxtCountPerPass;
            if (tData.terrainLayers.Length % TxtCountPerPass != 0)
            {
                passCount++;
            }

            return Mathf.Min(passCount, MaxPassCount);
        }

        /// <summary>
        /// Perform an asynchronous modification.
        /// </summary>
        /// <param name="p">Parameters.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator ModifyAsync(ModificationParameters p)
        {
            if (p.Action == ActionType.Smooth && p.Brush != BrushType.Sphere)
            {
                LogSystem.LogError("[HybridTerrain->ModifyAsync] Sphere brush is required to smooth.");
                p.Brush = BrushType.Sphere;
            }

            if (p.Action == ActionType.Smooth || p.Action == ActionType.BETA_Sharpen)
            {
                kernelOperation.Params = p;
                return ModifyAsync(kernelOperation, p.Callback);
            }

            basicOperation.Params = p;
            return ModifyAsync(basicOperation, p.Callback);
        }

        /// <summary>
        /// Perform an asynchronous modification.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="operation">Modification operation.</param>
        /// <param name="callback">Callback to invoke upon completion.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator ModifyAsync<T>(IOperation<T> operation, Action callback = null) where T : struct, IJobParallelFor
        {
            if (modifying)
            {
                LogSystem.LogError("[HybridTerrainEntity->ModifyAsync] Attempting to perform a modification while another is ongoing.");
                yield break;
            }

            modifying = true;

            var area = operation.GetAreaToModify(diggerSystem);
            if (!area.NeedsModification)
            {

            }
            else
            {
                yield return diggerSystem.ModifyAsync(operation);
            }

            area = operation.GetAreaToModify(diggerSystem);
            if (!area.NeedsModification)
            {

            }
            else
            {
                diggerSystem.ApplyModify();
            }

            modifying = false;
            callback?.Invoke();
        }
#endif

        /// <summary>
        /// Get a position in voxel coordinates.
        /// </summary>
        /// <param name="position">Base position.</param>
        /// <returns>Position in voxel coordinates.</returns>
        private Vector3Int GetVoxelPosition(Vector3 position)
        {
            Vector3Int voxelPosition = new Vector3Int();
            voxelPosition.x = (int) Math.Round(position.x, 0, MidpointRounding.AwayFromZero);
            voxelPosition.y = (int) Math.Round(position.y, 0, MidpointRounding.AwayFromZero);
            voxelPosition.z = (int) Math.Round(position.z, 0, MidpointRounding.AwayFromZero);
            return voxelPosition;
        }

#if USE_DIGGER
        private void Update()
        {
            if (!modifying && modBuf.Count > 0)
            {
                var parameters = modBuf.Dequeue();
                StartCoroutine(ModifyAsync(parameters));
            }
        }
#endif
    }
}