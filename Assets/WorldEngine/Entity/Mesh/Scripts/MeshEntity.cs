﻿// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.WorldEngine.Materials;
using FiveSQD.WebVerse.WorldEngine.Entity.Placement;
using FiveSQD.WebVerse.WorldEngine.Tags;
using FiveSQD.WebVerse.WorldEngine.Utilities;

namespace FiveSQD.WebVerse.WorldEngine.Entity
{
    /// <summary>
    /// Class for a mesh entity.
    /// </summary>
    public class MeshEntity : BaseEntity
    {
        /// <summary>
        /// Meshes on the character model.
        /// </summary>
        private Mesh[] meshes;

        /// <summary>
        /// Original size of the mesh.
        /// </summary>
        private Vector3 originalMeshSize;

        /// <summary>
        /// Current scaling of the mesh.
        /// </summary>
        private Vector3 meshScaling;

        /// <summary>
        /// Mesh colliders used by the MeshEntity.
        /// </summary>
        private MeshCollider[] meshColliders;

        /// <summary>
        /// Box colliders used by the MeshEntity.
        /// </summary>
        private BoxCollider boxCollider;

        /// <summary>
        /// Rigidbody used by the character.
        /// </summary>
        private Rigidbody rigidBody;

        /// <summary>
        /// Whether or not the character is gravitational.
        /// </summary>
        private bool gravitational = false;

        /// <summary>
        /// Cube used for highlighting the character entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <returns>Whether or not the setting was successful.</returns>
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
            if (rigidBody == null)
            {
                LogSystem.LogError("[MeshEntity->GetMotion] No rigidbody for mesh entity.");
                return null;
            }

            return new EntityMotion
            {
                angularVelocity = rigidBody.angularVelocity,
                stationary = rigidBody.isKinematic,
                velocity = rigidBody.velocity
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            if (rigidBody == null)
            {
                LogSystem.LogError("[MeshEntity->GetPhysicalProperties] No rigidbody for mesh entity.");
                return null;
            }

            return new EntityPhysicalProperties
            {
                angularDrag = rigidBody.angularDrag,
                centerOfMass = rigidBody.centerOfMass,
                drag = rigidBody.drag,
                gravitational = gravitational,
                mass = rigidBody.mass
            };
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            return new Vector3(originalMeshSize.x * meshScaling.x,
                originalMeshSize.y * meshScaling.y, originalMeshSize.z * meshScaling.z);
        }

        /// <summary>
        /// Set the highlight state of the entity.
        /// </summary>
        /// <param name="highlight">Whether or not to turn on the highlight.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetHighlight(bool highlight)
        {
            highlightCube.SetActive(highlight);

            return true;
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
                    MakePlacing();
                    return true;

                case InteractionState.Static:
                    MakeStatic();
                    return true;
                
                case InteractionState.Hidden:
                    MakeHidden();
                    return true;
                
                default:
                    LogSystem.LogWarning("[MeshEntity->SetInteractionState] Interaction state invalid.");
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
            if (!motionToSet.HasValue)
            {
                LogSystem.LogWarning("[MeshEntity->SetMotion] Invalid motion.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[MeshEntity->SetMotion] No rigidbody for mesh entity.");
                return false;
            }

            if (motionToSet.Value.stationary.HasValue)
            {
                if (motionToSet.Value.stationary == true)
                {
                    rigidBody.isKinematic = true;
                    rigidBody.useGravity = false;
                    rigidBody.angularVelocity = Vector3.zero;
                    rigidBody.velocity = Vector3.zero;
                    return true;
                }
                else
                {
                    rigidBody.isKinematic = false;
                    rigidBody.useGravity = gravitational;
                }
            }

            if (motionToSet.Value.angularVelocity != null)
            {
                rigidBody.angularVelocity = motionToSet.Value.angularVelocity;
            }

            if (motionToSet.Value.velocity != null)
            {
                rigidBody.velocity = motionToSet.Value.velocity;
            }

            return true;
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPhysicalProperties(EntityPhysicalProperties? epp)
        {
            if (!epp.HasValue)
            {
                LogSystem.LogWarning("[MeshEntity->SetPhysicalProperties] Invalid physical properties.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[MeshEntity->SetPhysicalProperties] No rigidbody for mesh entity.");
                return false;
            }

            if (epp.Value.angularDrag.HasValue)
            {
                rigidBody.angularDrag = epp.Value.angularDrag.Value;
            }

            if (epp.Value.centerOfMass != null)
            {
                rigidBody.centerOfMass = epp.Value.centerOfMass;
            }

            if (epp.Value.drag.HasValue)
            {
                rigidBody.drag = epp.Value.drag.Value;
            }

            if (epp.Value.gravitational.HasValue)
            {
                gravitational = epp.Value.gravitational.Value;
                rigidBody.useGravity = gravitational;
            }

            if (epp.Value.mass.HasValue)
            {
                rigidBody.mass = epp.Value.mass.Value;
            }

            return true;
        }

        /// <summary>
        /// Set the size of the entity.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            float xSclFactor = size.x / originalMeshSize.x;
            float ySclFactor = size.y / originalMeshSize.y;
            float zSclFactor = size.z / originalMeshSize.z;
            meshScaling = new Vector3(xSclFactor, ySclFactor, zSclFactor);
            transform.localScale = new Vector3(transform.localScale.x * xSclFactor,
                transform.localScale.y * ySclFactor, transform.localScale.z * zSclFactor);

            return true;
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible)
        {
            // Use base functionality.
            return base.SetVisibility(visible);
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(System.Guid idToSet)
        {
            base.Initialize(idToSet);

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            SetRigidbody(rb);

            List<Mesh> ms = new List<Mesh>();
            foreach (MeshFilter filt in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                ms.Add(filt.sharedMesh);
            }
            SetRenderers(ms.ToArray());
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Mesh m in meshes)
            {
                m.RecalculateBounds();
                bounds.Encapsulate(m.bounds);
            }
            originalMeshSize = bounds.size;
            meshScaling = Vector3.one;

            BoxCollider boxCollider = null;
            foreach (BoxCollider bc in gameObject.GetComponentsInChildren<BoxCollider>())
            {
                if (bc.tag == TagManager.physicsColliderTag)
                {
                    boxCollider = bc;
                    break;
                }
            }

            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            List<MeshCollider> mcs = new List<MeshCollider>();
            foreach (MeshCollider mc in gameObject.GetComponentsInChildren<MeshCollider>())
            {
                if (mc.tag == TagManager.meshColliderTag)
                {
                    mcs.Add(mc);
                    break;
                }
            }

            SetColliders(boxCollider, mcs.ToArray());

            MakeHidden();
            SetUpHighlightVolume();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Set mesh renderers for the entity.
        /// </summary>
        /// <param name="ms">Meshes to apply.</param>
        private void SetRenderers(Mesh[] ms)
        {
            if (ms == null)
            {
                LogSystem.LogWarning("[MeshEntity->SetRenderer] No mesh.");
            }
            meshes = ms;
        }

        /// <summary>
        /// Set colliders for the entity.
        /// </summary>
        /// <param name="cc">Colliders to apply.</param>
        private void SetColliders(BoxCollider bc, MeshCollider[] mc)
        {
            if (bc == null)
            {
                LogSystem.LogWarning("[MeshEntity->SetColliders] No box collider.");
            }
            boxCollider = bc;

            if (mc == null)
            {
                LogSystem.LogWarning("[MeshEntity->SetColliders] No mesh collider.");
            }
            meshColliders = mc;
        }

        /// <summary>
        /// Set rigidbody for the entity.
        /// </summary>
        /// <param name="rb">Rigidbody to apply.</param>
        private void SetRigidbody(Rigidbody rb)
        {
            if (rb == null)
            {
                LogSystem.LogWarning("[MeshEntity->SetRigidbody] No rigidbody.");
            }
            rigidBody = rb;
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

            rigidBody.isKinematic = true;

            foreach (MeshCollider meshCollider in meshColliders)
            {
                meshCollider.enabled = false;
            }
            boxCollider.enabled = false;
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
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            rigidBody.isKinematic = true;
            foreach (MeshCollider meshCollider in meshColliders)
            {
                meshCollider.enabled = false;
            }
            boxCollider.enabled = false;
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
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                default:
                    break;
            }

            gameObject.SetActive(true);
            interactionState = InteractionState.Physical;
        }

        /// <summary>
        /// Make the entity placing.
        /// </summary>
        private void MakePlacing()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    break;

                case InteractionState.Physical:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Placing:
                default:
                    break;
            }

#if USE_EBS
            EBSPartCollectionManager.StartPlacing(this);
#endif
            
            gameObject.SetActive(true);
            interactionState = InteractionState.Placing;
        }

        /// <summary>
        /// Set up the highlight volume for the entity.
        /// </summary>
        private void SetUpHighlightVolume()
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            MeshRenderer[] rends = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in rends)
            {
                bounds.Encapsulate(rend.bounds);
            }

            bounds.center = bounds.center - transform.position;

            highlightCube = new GameObject("HighlightVolume");

            Vector3[] vertices =
            {
                    new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.max.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.max.x, bounds.max.y, bounds.min.z),
                    new Vector3 (bounds.min.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.min.x, bounds.max.y, bounds.max.z),
                    new Vector3 (bounds.max.x, bounds.max.y, bounds.max.z),
                    new Vector3 (bounds.max.x, bounds.min.y, bounds.max.z),
                    new Vector3 (bounds.min.x, bounds.min.y, bounds.max.z),
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
    }
}