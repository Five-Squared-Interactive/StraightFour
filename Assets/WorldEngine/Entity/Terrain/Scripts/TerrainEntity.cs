// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using FiveSQD.WebVerse.WorldEngine.Materials;
using FiveSQD.WebVerse.WorldEngine.Utilities;
using FiveSQD.WebVerse.WorldEngine.Entity.Terrain;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.WorldEngine.Entity
{
    /// <summary>
    /// WIP: Class for a terrain entity.
    /// </summary>
    public class TerrainEntity : BaseEntity
    {
        /// <summary>
        /// Terrain object for the terrain entity.
        /// </summary>
        public UnityEngine.Terrain terrain;

        /// <summary>
        /// Terrain collider for the terrain entity.
        /// </summary>
        public TerrainCollider terrainCollider;

        /// <summary>
        /// Highlight cube for the terrain entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// Heights values. Stored since the rendered terrain is interpolated.
        /// </summary>
        private float[,] heights;

        /// <summary>
        /// Create a terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="id">ID for the terrain.</param>
        /// <returns>The requested terrain entity.</returns>
        public static TerrainEntity Create(float length, float width, float height,
            float[,] heights, Guid id)
        {
            GameObject terrainGO = new GameObject("TerrainEntity-" + id.ToString());
            TerrainEntity terrainEntity = terrainGO.AddComponent<TerrainEntity>();
            TerrainData terrainData = new TerrainData();
            /*for (int i = 0; i < heights.GetLength(0); i++)
            {
                for (int j = 0; j < heights.GetLength(1); j++)
                {
                    heights[i, j] = heights[i, j] / height;
                }
            }*/
            //terrainData.heightmapResolution = GetTerrainSize(Math.Max(length, width));
            //terrainData.SetHeights(0, 0, heights);
            //terrainData.size = new Vector3(length, height, width);
            GameObject terrainObject = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.transform.SetParent(terrainGO.transform);
            terrainEntity.terrain = terrainObject.GetComponent<UnityEngine.Terrain>();
            terrainEntity.terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            terrainEntity.Initialize(id);
            terrainEntity.SetHeights(length, width, height, heights);
            return terrainEntity;
        }

        /// <summary>
        /// Delete the terrain entity.
        /// </summary>
        /// <returns></returns>
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
                    LogSystem.LogWarning("[TerrainEntity->SetInteractionState] Placing not valid for terrain.");
                    return false;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("[TerrainEntity->SetInteractionState] Interaction state invalid.");
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
            LogSystem.LogWarning("[TerrainEntity->SetMotion] Motion not settable for light.");

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
        /// Set the heights of the terrain.
        /// </summary>
        /// <param name="newLength">New length in meters of the terrain.</param>
        /// <param name="newWidth">New width in meters of the terrain.</param>
        /// <param name="newHeight">New height in meters of the terrain.</param>
        /// <param name="newHeights">New array of heights for the terrain. Array will be distributed across the terrain.
        /// For example, a terrain that is 8 meters long and has a newHeights array of 16 elements will have one data point
        /// for every half meter, Length and width of array cannot exceed 4097 units.</param>
        public void SetHeights(float newLength, float newWidth, float newHeight, float[,] newHeights)
        {
            // ------- Input Validation -------
            if (newLength < 1 || newWidth < 1 || newHeight < 1)
            {
                LogSystem.LogWarning("[TerrainEntity->SetHeights] New length, width, and height must be greater than 0.");
                return;
            }

            if (newHeights == null)
            {
                LogSystem.LogWarning("[TerrainEntity->SetHeights] Invalid new heights array.");
                return;
            }

            int newTerrainArrayLength = newHeights.GetLength(0);
            int newTerrainArrayWidth = newHeights.GetLength(1);

            if (newTerrainArrayLength > 4097 || newTerrainArrayWidth > 4097 ||
                newTerrainArrayLength < 1 || newTerrainArrayWidth < 1)
            {
                LogSystem.LogWarning("[TerrainEntity->SetHeights] Terrain cannot contain less than 1 or more than 4097 elements in any direction.");
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
            heights = newHeights;
        }

        /// <summary>
        /// Get a 2d array of heights for the terrain.
        /// </summary>
        /// <returns>A 2d array of heights for the terrain.</returns>
        public float[,] GetHeights()
        {
            /*float[,] rawHeights = terrain.terrainData.GetHeights(
                0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

            int len = rawHeights.GetLength(0);
            int width = rawHeights.GetLength(1);
            float[,] processedHeights = new float[len, width];
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    processedHeights[i, j] = rawHeights[i, j] * terrain.terrainData.size.y;
                }
            }*/

            return heights;
        }

        /// <summary>
        /// Set the height at a given x, y index.
        /// </summary>
        /// <param name="xIndex">X index to set height at.</param>
        /// <param name="yIndex">Y index to set height at.</param>
        /// <param name="height">Height to set.</param>
        public void SetHeight(int xIndex, int yIndex, float height)
        {
            if (heights.GetLength(0) <= xIndex || heights.GetLength(1) <= yIndex)
            {
                LogSystem.LogWarning("[TerrainEntity->SetHeight] Invalid index.");
                return;
            }

            float[,] heightsToSet = new float[,] { { height / terrain.terrainData.size.y } };
            terrain.terrainData.SetHeights(xIndex, yIndex, heightsToSet);
            heights[xIndex, yIndex] = height;
        }

        /// <summary>
        /// Get the height at a given x, y index.
        /// </summary>
        /// <param name="xIndex">X index to get height at.</param>
        /// <param name="yIndex">Y index to get height at.</param>
        /// <returns>Height at given index.</returns>
        public float GetHeight(int xIndex, int yIndex)
        {
            //return terrain.terrainData.GetHeight(xIndex, yIndex) * terrain.terrainData.size.y;

            if (heights.GetLength(0) <= xIndex || heights.GetLength(1) <= yIndex)
            {
                LogSystem.LogWarning("[TerrainEntity->GetHeight] Invalid index.");
                return 0;
            }

            return heights[xIndex, yIndex];
        }

        /// <summary>
        /// Add a layer to the terrain entity.
        /// </summary>
        /// <param name="layer">Layer to add to the terrain entity.</param>
        /// <returns>Index of the new layer, or -1 if failed.</returns>
        public int AddLayer(TerrainEntityLayer layer)
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
        /// Remove a layer from the terrain entity.
        /// </summary>
        /// <param name="index">Index of the layer to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveLayer(int index)
        {
            if (index < 0)
            {
                LogSystem.LogWarning("[TerrainEntity->RemoveLayer] Index must be greater than 0.");
                return false;
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

            if (index >= newLayers.Count)
            {
                LogSystem.LogWarning("[TerrainEntity->RemoveLayer] Invalid index "
                    + index + ". Only " + newLayers.Count + " layers.");
                return false;
            }

            newLayers.RemoveAt(index);
            terrain.terrainData.terrainLayers = newLayers.ToArray();

            return true;
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
                LogSystem.LogWarning("[TerrainEntity->GetLayer] Index must be greater than 0.");
                return null;
            }

            if (terrain.terrainData.terrainLayers == null)
            {
                LogSystem.LogWarning("[TerrainEntity->GetLayer] No layers.");
                return null;
            }

            if (index >= terrain.terrainData.terrainLayers.Length)
            {
                LogSystem.LogWarning("[TerrainEntity->GetLayer] Invalid index "
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
        /// Set the mask for a particular layer.
        /// </summary>
        /// <param name="layer">Layer index.</param>
        /// <param name="mask">Mask (a 2d array of values between 0 and 1 corresponding to the
        /// intensity of the given layer to set).</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLayerMask(int layer, float[,] mask)
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
                LogSystem.LogWarning("[TerrainEntity->GetLayerMask] Index must be greater than 0.");
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
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(Guid idToSet)
        {
            base.Initialize(idToSet);

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
            switch(interactionState)
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
                LogSystem.LogWarning("[TerrainEntity->GetTerrainSize] Size too small.");
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
                LogSystem.LogWarning("[TerrainEntity->GetTerrainSize] Size too large.");
                return 4097;
            }
        }
    }
}