// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.WorldEngine.Camera;
using FiveSQD.WebVerse.WorldEngine.Entity;
using FiveSQD.WebVerse.WorldEngine.Materials;
using FiveSQD.WebVerse.WorldEngine.WorldStorage;
using FiveSQD.WebVerse.WorldEngine.Utilities;
using FiveSQD.WebVerse.WorldEngine.Environment;
using UnityEngine;
#if USE_DIGGER
using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
#endif

namespace FiveSQD.WebVerse.WorldEngine.World
{
    /// <summary>
    /// Class for a World.
    /// </summary>
    public class World : MonoBehaviour
    {
        /// <summary>
        /// Class for World information.
        /// </summary>
        public class WorldInfo
        {
            /// <summary>
            /// Entity highlight material.
            /// </summary>
            public Material highlightMaterial;

            /// <summary>
            /// Entity preview material.
            /// </summary>
            public Material previewMaterial;

            /// <summary>
            /// Environment sky material.
            /// </summary>
            public Material skyMaterial;

            /// <summary>
            /// Material to use for the lite procedural sky.
            /// </summary>
            [Tooltip("Material to use for the lite procedural sky.")]
            public Material liteProceduralSkyMaterial;

            /// <summary>
            /// Environment default sky texture.
            /// </summary>
            public Texture2D defaultCloudTexture;

            /// <summary>
            /// Environment default sky texture.
            /// </summary>
            public Texture2D defaultStarTexture;

            /// <summary>
            /// Input entity prefab.
            /// </summary>
            public GameObject inputEntityPrefab;

            /// <summary>
            /// WebView prefab.
            /// </summary>
            public GameObject webViewPrefab;

            /// <summary>
            /// Canvas webView prefab.
            /// </summary>
            public GameObject canvasWebViewPrefab;

            /// <summary>
            /// Character controller prefab.
            /// </summary>
            public GameObject characterControllerPrefab;

            /// <summary>
            /// Character controller label prefab.
            /// </summary>
            public GameObject characterControllerLabelPrefab;

            /// <summary>
            /// Voxel prefab.
            /// </summary>
            public GameObject voxelPrefab;

            /// <summary>
            /// Prefab for a water body.
            /// </summary>
            [Tooltip("Prefab for a water body.")]
            public GameObject waterBodyPrefab;

            /// <summary>
            /// Prefab for a water blocker.
            /// </summary>
            [Tooltip("Prefab for a water blocker.")]
            public GameObject waterBlockerPrefab;

            /// <summary>
            /// Camera offset.
            /// </summary>
            public GameObject cameraOffset;

            /// <summary>
            /// Whether or not world is in VR mode.
            /// </summary>
            public bool vr;

            /// <summary>
            /// Maximum number of storage entries.
            /// </summary>
            [Range(0, int.MaxValue)]
            public int maxStorageEntries;

            /// <summary>
            /// Maximum length of a storage entry.
            /// </summary>
            [Range(0, int.MaxValue)]
            public int maxEntryLength;

            /// <summary>
            /// Maximum length of a storage key.
            /// </summary>
            [Range(0, int.MaxValue)]
            public int maxKeyLength;

            /// <summary>
            /// Name/URI for the world's site.
            /// </summary>
            public string siteName;
        }

        /// <summary>
        /// The mesh manager for the world.
        /// </summary>
        public MeshManager.MeshManager meshManager { get; private set; }

        /// <summary>
        /// The entity manager for the world.
        /// </summary>
        public EntityManager entityManager { get; private set; }

        /// <summary>
        /// The storage manager for the world.
        /// </summary>
        public WorldStorageManager storageManager { get; private set; }

        /// <summary>
        /// The camera manager for the world.
        /// </summary>
        public CameraManager cameraManager { get; private set; }

        /// <summary>
        /// The material manager for the world.
        /// </summary>
        public MaterialManager materialManager { get; private set; }

        /// <summary>
        /// The environment manager for the world.
        /// </summary>
        public EnvironmentManager environmentManager { get; private set; }

        /// <summary>
        /// Name/URI for the world's site.
        /// </summary>
        public string siteName { get; private set; }

        /// <summary>
        /// GameObject for the lite procedural sky.
        /// </summary>
        [Tooltip("GameObject for the lite procedural sky.")]
        public GameObject liteProceduralSkyObject;

        /// <summary>
        /// The GameObject for the mesh manager.
        /// </summary>
        private GameObject meshManagerGO;

        /// <summary>
        /// The GameObject for the entity manager.
        /// </summary>
        private GameObject entityManagerGO;

        /// <summary>
        /// The GameObject for the storage manager.
        /// </summary>
        private GameObject storageManagerGO;

        /// <summary>
        /// The GameObject for the camera manager.
        /// </summary>
        private GameObject cameraManagerGO;

        /// <summary>
        /// The GameObject for the material manager.
        /// </summary>
        private GameObject materialManagerGO;

        /// <summary>
        /// The GameObject for the environment manager.
        /// </summary>
        private GameObject environmentManagerGO;

        /// <summary>
        /// The GameObject for the digger master.
        /// </summary>
        private GameObject diggerMasterGO;

        /// <summary>
        /// The GameObject for the digger master runtime.
        /// </summary>
        private GameObject diggerMasterRuntimeGO;

#if USE_DIGGER
        /// <summary>
        /// The digger master.
        /// </summary>
        private DiggerMaster diggerMaster;

        /// <summary>
        /// The digger master runtime.
        /// </summary>
        private DiggerMasterRuntime diggerMasterRuntime;
#endif

        /// <summary>
        /// Initialize the World.
        /// </summary>
        /// <param name="worldInfo">World information to use.</param>
        public void Initialize(WorldInfo worldInfo)
        {
            if (meshManager != null)
            {
                LogSystem.LogError("[World->Initialize] Mesh manager already initialized.");
                return;
            }
            meshManagerGO = new GameObject("MeshManager");
            meshManagerGO.transform.parent = transform;
            meshManager = meshManagerGO.AddComponent<MeshManager.MeshManager>();
            meshManager.Initialize();

            if (entityManager != null)
            {
                LogSystem.LogError("[World->Initialize] Entity manager already initialized.");
                return;
            }
            entityManagerGO = new GameObject("EntityManager");
            entityManagerGO.transform.parent = transform;
            entityManager = entityManagerGO.AddComponent<EntityManager>();
            entityManager.Initialize();
            entityManager.inputEntityPrefab = worldInfo.inputEntityPrefab;
            entityManager.webViewPrefab = worldInfo.webViewPrefab;
            entityManager.canvasWebViewPrefab = worldInfo.canvasWebViewPrefab;
            entityManager.characterControllerPrefab = worldInfo.characterControllerPrefab;
            entityManager.characterControllerLabelPrefab = worldInfo.characterControllerLabelPrefab;
            entityManager.voxelPrefab = worldInfo.voxelPrefab;
            entityManager.waterBodyPrefab = worldInfo.waterBodyPrefab;
            entityManager.waterBlockerPrefab = worldInfo.waterBlockerPrefab;

            if (storageManager != null)
            {
                LogSystem.LogError("[World->Initialize] Storage manager already initialized.");
                return;
            }
            storageManagerGO = new GameObject("StorageManager");
            storageManagerGO.transform.parent = transform;
            storageManager = storageManagerGO.AddComponent<WorldStorageManager>();
            storageManager.Initialize(worldInfo.maxStorageEntries, worldInfo.maxEntryLength, worldInfo.maxKeyLength);

            if (cameraManager != null)
            {
                LogSystem.LogError("[World->Initialize] Camera manager already initialized.");
                return;
            }
            cameraManagerGO = new GameObject("CameraManager");
            cameraManagerGO.transform.parent = transform;
            cameraManager = cameraManagerGO.AddComponent<CameraManager>();
            cameraManager.Initialize(UnityEngine.Camera.main, worldInfo.cameraOffset, worldInfo.vr, null);

            if (materialManager != null)
            {
                LogSystem.LogError("[World->Initialize] Material manager already initialized.");
                return;
            }
            materialManagerGO = new GameObject("MaterialManager");
            materialManagerGO.transform.parent = transform;
            materialManager = materialManagerGO.AddComponent<MaterialManager>();
            materialManager.Initialize(worldInfo.highlightMaterial, worldInfo.previewMaterial);

            if (environmentManager != null)
            {
                LogSystem.LogError("[World->Initialize] Environment manager already initialized.");
                return;
            }
            environmentManagerGO = new GameObject("EnvironmentManager");
            environmentManagerGO.transform.parent = transform;
            environmentManager = environmentManagerGO.AddComponent<EnvironmentManager>();
            environmentManager.skyMaterial = worldInfo.skyMaterial;
            environmentManager.liteProceduralSkyMaterial = worldInfo.liteProceduralSkyMaterial;
            environmentManager.liteProceduralSkyObject = liteProceduralSkyObject;
            environmentManager.defaultCloudTexture = worldInfo.defaultCloudTexture;
            environmentManager.defaultStarTexture = worldInfo.defaultStarTexture;
            environmentManager.Initialize();

#if USE_DIGGER
            diggerMasterGO = new GameObject("DiggerMaster");
            diggerMasterGO.transform.parent = transform;
            diggerMaster = diggerMasterGO.AddComponent<DiggerMaster>();
            diggerMaster.SceneDataFolder = "Untitled";

            diggerMasterRuntimeGO = new GameObject("DiggerMasterRuntime");
            diggerMasterRuntimeGO.transform.parent = transform;
            diggerMasterRuntime = diggerMasterRuntimeGO.AddComponent<DiggerMasterRuntime>();
            diggerMasterRuntime.BufferSize = 1024;
#endif

            siteName = worldInfo.siteName;
        }

        /// <summary>
        /// Unload the world.
        /// </summary>
        public void Unload()
        {
#if USE_DIGGER
            if (diggerMasterGO == null)
            {
                LogSystem.LogError("[World->Unload] No digger master.");
            }
            else
            {
                Destroy(diggerMasterGO);
            }

            if (diggerMasterRuntimeGO == null)
            {
                LogSystem.LogError("[World->Unload] No digger master runtime.");
            }
            else
            {
                Destroy(diggerMasterRuntimeGO);
            }
#endif

            if (cameraManager != null)
            {
                cameraManager.SetParent(null);
            }

            if (entityManager == null)
            {
                LogSystem.LogError("[World->Unload] No entity manager.");
            }
            else
            {
                entityManager.Unload();
                Destroy(entityManagerGO);
            }

            if (meshManager == null)
            {
                LogSystem.LogError("[World->Unload] No mesh manager.");
            }
            else
            {
                meshManager.Terminate();
                Destroy(meshManagerGO);
            }

            if (storageManager == null)
            {
                LogSystem.LogError("[World->Unload] No storage manager.");
            }
            else
            {
                storageManager.Terminate();
                Destroy(storageManagerGO);
            }

            if (cameraManager == null)
            {
                LogSystem.LogError("[World->Unload] No camera manager.");
            }
            else
            {
                cameraManager.Terminate();
                Destroy(cameraManagerGO);
            }

            if (materialManager == null)
            {
                LogSystem.LogError("[World->Unload] No material manager.");
            }
            else
            {
                materialManager.Terminate();
                Destroy(materialManagerGO);
            }

            if (environmentManager == null)
            {
                LogSystem.LogError("[World->Unload] No environment manager.");
            }
            else
            {
                environmentManager.Terminate();
                Destroy(environmentManagerGO);
            }
        }
    }
}