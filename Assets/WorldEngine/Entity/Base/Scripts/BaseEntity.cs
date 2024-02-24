﻿// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.WorldEngine.Entity.Placement;
using FiveSQD.WebVerse.WorldEngine.Synchronization;

namespace FiveSQD.WebVerse.WorldEngine.Entity
{
    /// <summary>
    /// Base class for an entity.
    /// </summary>
    public class BaseEntity : MonoBehaviour
    {
        /// <summary>
        /// Physical properties for an entity.
        /// </summary>
        public struct EntityPhysicalProperties
        {
            /// <summary>
            /// Angular drag of the entity.
            /// </summary>
            public float? angularDrag;

            /// <summary>
            /// Center of mass of the entity.
            /// </summary>
            public Vector3 centerOfMass;

            /// <summary>
            /// Drag of the entity.
            /// </summary>
            public float? drag;

            /// <summary>
            /// Whether or not the entity is gravitational.
            /// </summary>
            public bool? gravitational;

            /// <summary>
            /// Mass of the entity.
            /// </summary>
            public float? mass;
        }

        /// <summary>
        /// Motion state for an entity.
        /// </summary>
        public struct EntityMotion
        {
            /// <summary>
            /// Angular velocity of the entity.
            /// </summary>
            public Vector3 angularVelocity;

            /// <summary>
            /// Whether or not the entity is stationary.
            /// </summary>
            public bool? stationary;

            /// <summary>
            /// Velocity of the entity.
            /// </summary>
            public Vector3 velocity;
        }

        /// <summary>
        /// Interaction state for an entity.
        /// Hidden: Visibly hidden and not interactable.
        /// Static: Visible but not interactable.
        /// Physical: Visible and interactable.
        /// Placing: Visible and in a placing interaction mode.
        /// </summary>
        public enum InteractionState { Hidden, Static, Physical, Placing }

        /// <summary>
        /// Placement sockets for the entity.
        /// </summary>
        public List<PlacementSocket> sockets = new List<PlacementSocket>();

        /// <summary>
        /// Minimum time between outgoing synchronization updates for the entity.
        /// </summary>
        public float minUpdateTime = 0.05f;

        /// <summary>
        /// Interaction state of the entity.
        /// </summary>
        protected InteractionState interactionState;

        /// <summary>
        /// Synchronizer for the entity.
        /// </summary>
        protected BaseSynchronizer synchronizer = null;

        /// <summary>
        /// Whether or not the ID for the entity has been initialized.
        /// </summary>
        private bool idInitialized = false;

        /// <summary>
        /// Internally-tracked entity.
        /// </summary>
        private Guid id_internal;

        /// <summary>
        /// ID of the entity. This is an immutable property. It can only be set once and is intended
        /// to be set by the entity at initialization.
        /// </summary>
        public Guid id
        {
            set
            {
                if (idInitialized == true)
                {
                    throw new InvalidOperationException("BaseEntity id is an immutable property.");
                }
                id_internal = value;
                idInitialized = true;
            }
            get
            {
                return id_internal;
            }
        }

        /// <summary>
        /// Tag for the entity.
        /// </summary>
        public virtual string entityTag { get; set; }

        /// <summary>
        /// Counter in seconds since the last outgoing position synchronization update.
        /// </summary>
        protected float positionUpdateTime = 0;

        /// <summary>
        /// Counter in seconds since the last outgoing rotation synchronization update.
        /// </summary>
        protected float rotationUpdateTime = 0;


        /// <summary>
        /// Counter in seconds since the last outgoing scale synchronization update.
        /// </summary>
        protected float scaleUpdateTime = 0;

        /// <summary>
        /// Counter in seconds since the last outgoing size synchronization update.
        /// </summary>
        protected float sizeUpdateTime = 0;

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
            if (synchronizer != null)
            {
                synchronizer.SetVisibility(this, visible);
            }
            return true;
        }

        /// <summary>
        /// Get the visibility state of the entity.
        /// </summary>
        /// <returns>The visibility state of the entity.</returns>
        public virtual bool GetVisibility()
        {
            return gameObject.activeSelf;
        }

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool Delete()
        {
            Destroy(gameObject);
            if (synchronizer != null)
            {
                synchronizer.DeleteSynchronizedEntity(this);
            }
            return true;
        }

        /// <summary>
        /// Set the highlight state of the entity.
        /// </summary>
        /// <param name="highlight">Whether or not to turn on the highlight.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetHighlight(bool highlight)
        {
            if (synchronizer != null)
            {
                synchronizer.SetHighlight(this, highlight);
            }
            return false;
        }

        /// <summary>
        /// Get the highlight state of the entity.
        /// </summary>
        /// <returns>The highlight state of the entity.</returns>
        public virtual bool GetHighlight()
        {
            return false;
        }

        /// <summary>
        /// Set the parent of the entity.
        /// </summary>
        /// <param name="parent">Parent to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetParent(BaseEntity parent)
        {
            if (parent == null)
            {
                if (WorldEngine.ActiveWorld == null)
                {
                    Utilities.LogSystem.LogError("[BaseEntity->SetParent] No active world.");
                    return false;
                }
                transform.SetParent(WorldEngine.ActiveWorld.entityManager.transform);
                if (synchronizer != null)
                {
                    synchronizer.SetParent(this, null);
                }
                return true;
            }
            
            transform.SetParent(parent.transform);
            if (synchronizer != null)
            {
                synchronizer.SetParent(this, parent);
            }

            return true;
        }

        /// <summary>
        /// Get the parent of the entity.
        /// </summary>
        /// <returns>The parent of the entity, or null if none.</returns>
        public BaseEntity GetParent()
        {
            if (transform.parent == null) // TODO.
            {
                return null;
            }

            BaseEntity parent = transform.parent.GetComponent<BaseEntity>();
            if (parent == null)
            {
                return null;
            }

            return parent;
        }

        /// <summary>
        /// Get the children of the entity.
        /// </summary>
        /// <returns>A list of the entity's children.</returns>
        public BaseEntity[] GetChildren()
        {
            List<BaseEntity> children = new List<BaseEntity>();
            foreach (Transform tf in transform)
            {
                BaseEntity be = tf.GetComponent<BaseEntity>();
                if (be != null)
                {
                    if (be != this)
                    {
                        children.Add(be);
                    }
                }
            }
            return children.ToArray();
        }

        /// <summary>
        /// Set the position of the entity.
        /// </summary>
        /// <param name="position">Position to set.</param>
        /// <param name="local">Whether or not the position is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetPosition(Vector3 position, bool local, bool synchronize = true)
        {
            if (position == null)
            {
                Utilities.LogSystem.LogWarning("[BaseEntity->SetPosition] Position value null.");
                return false;
            }

            if (local)
            {
                transform.localPosition = position;
            }
            else
            {
                transform.position = position;
            }
            if (synchronize && synchronizer != null && positionUpdateTime > minUpdateTime)
            {
                synchronizer.SetPosition(this, position);
                positionUpdateTime = 0;
            }
            return true;
        }

        /// <summary>
        /// Get the position of the entity.
        /// </summary>
        /// <param name="local">Whether or not to provide the local position.</param>
        /// <returns>The position of the entity.</returns>
        public virtual Vector3 GetPosition(bool local)
        {
            return local ? transform.localPosition : transform.position;
        }

        /// <summary>
        /// Set the rotation of the entity.
        /// </summary>
        /// <param name="rotation">Rotation to set.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetRotation(Quaternion rotation, bool local, bool synchronize = true)
        {
            if (rotation == null)
            {
                Utilities.LogSystem.LogWarning("[BaseEntity->SetRotation] Rotation value null.");
                return false;
            }

            if (local)
            {
                transform.localRotation = rotation;
            }
            else
            {
                transform.rotation = rotation;
            }
            if (synchronize && synchronizer != null && rotationUpdateTime > minUpdateTime)
            {
                synchronizer.SetRotation(this, rotation);
                rotationUpdateTime = 0;
            }
            return true;
        }

        /// <summary>
        /// Set the Euler rotation of the entity.
        /// </summary>
        /// <param name="rotation">Rotation to set.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetEulerRotation(Vector3 rotation, bool local, bool synchronize = true)
        {
            if (rotation == null)
            {
                Utilities.LogSystem.LogWarning("[BaseEntity->SetEulerRotation] Rotation value null.");
                return false;
            }

            if (local)
            {
                transform.localEulerAngles = rotation;
            }
            else
            {
                transform.eulerAngles = rotation;
            }
            if (synchronize && synchronizer != null && rotationUpdateTime > minUpdateTime)
            {
                synchronizer.SetRotation(this, transform.rotation);
                rotationUpdateTime = 0;
            }
            return true;
        }

        /// <summary>
        /// Get the rotation of the entity.
        /// </summary>
        /// <param name="local">Whether or not to provide the local rotation.</param>
        /// <returns>The rotation of the entity.</returns>
        public Quaternion GetRotation(bool local)
        {
            return local ? transform.localRotation : transform.rotation;
        }

        /// <summary>
        /// Set the scale of the entity.
        /// </summary>
        /// <param name="scale">Scale to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetScale(Vector3 scale, bool synchronize = true)
        {
            if (scale == null)
            {
                Utilities.LogSystem.LogWarning("[BaseEntity->SetScale] Scale value null.");
                return false;
            }
            transform.localScale = scale;
            if (synchronize && synchronizer != null && scaleUpdateTime > minUpdateTime)
            {
                synchronizer.SetScale(this, scale);
                scaleUpdateTime = 0;
            }
            return true;
        }

        /// <summary>
        /// Get the scale of the entity.
        /// </summary>
        /// <returns>The scale of the entity.</returns>
        public Vector3 GetScale()
        {
            return transform.localScale;
        }

        /// <summary>
        /// Set the size of the entity.
        /// Must be implemented by inheriting classes, as the size of an entity is dependent
        /// on its type.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetSize(Vector3 size, bool synchronize = true)
        {
            throw new System.NotImplementedException("SetSize() not implemented.");
        }

        /// <summary>
        /// Get the size of the entity.
        /// Must be implemented by inheriting classes, as the size of an entity is dependent
        /// on its type.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public virtual Vector3 GetSize()
        {
            throw new System.NotImplementedException("GetSize() not implemented.");
        }

        /// <summary>
        /// Compare another entity reference with this one.
        /// </summary>
        /// <param name="otherEntity">Other entity to compare.</param>
        /// <returns>Whether or not the entities match.</returns>
        public bool Compare(BaseEntity otherEntity)
        {
            return otherEntity == this;
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetPhysicalProperties(EntityPhysicalProperties? propertiesToSet)
        {
            if (synchronizer != null)
            {
                synchronizer.SetPhysicalProperties(this, propertiesToSet);
            }
            return false;
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public virtual EntityPhysicalProperties? GetPhysicalProperties()
        {
            return null;
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetInteractionState(InteractionState stateToSet)
        {
            if (synchronizer != null)
            {
                synchronizer.SetInteractionState(this, stateToSet);
            }
            return false;
        }

        /// <summary>
        /// Get the interaction state of the entity.
        /// </summary>
        /// <returns>The interaction state for this entity.</returns>
        public InteractionState GetInteractionState()
        {
            return interactionState;
        }

        /// <summary>
        /// Set the motion state for this entity.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetMotion(EntityMotion? motionToSet)
        {
            if (synchronizer != null)
            {
                synchronizer.SetMotion(this, motionToSet);
            }
            return false;
        }

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public virtual EntityMotion? GetMotion()
        {
            return null;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public virtual void Initialize(Guid idToSet)
        {
            if (idInitialized)
            {
                Utilities.LogSystem.LogError("[BaseEntity->Initialize] Entity already initialized.");
                return;
            }

            id = idToSet;
            synchronizer = null;

            // TODO event.
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public virtual void TearDown()
        {

        }

        /// <summary>
        /// Add a placement socket to the entity.
        /// </summary>
        /// <param name="position">Position of the placement socket relative to the entity.</param>
        /// <param name="rotation">Rotation of the placement socket relative to the entity.</param>
        /// <param name="connectingOffset">Offset to apply when connecting to another socket.</param>
        public virtual void AddSocket(Vector3 position, Quaternion rotation, Vector3 connectingOffset)
        {
            GameObject newSocketObj = new GameObject("PlacementSocket");
            newSocketObj.transform.SetParent(transform);
            PlacementSocket newSocket = newSocketObj.AddComponent<PlacementSocket>();
            newSocket.Initialize(this, position, rotation, connectingOffset);
        }

        /// <summary>
        /// Start synchronizing the entity.
        /// </summary>
        /// <param name="synch">Synchronizer to use.</param>
        public virtual void StartSynchronizing(BaseSynchronizer synch)
        {
            synchronizer = synch;
        }

        /// <summary>
        /// Stop synchronizing the entity.
        /// </summary>
        public virtual void StopSynchronizing()
        {
            synchronizer = null;
        }

        /// <summary>
        /// Set the visibility of the preview.
        /// </summary>
        /// <param name="visible">Whether or not to make the preview visible.</param>
        protected virtual void SetPreviewVisibility(bool visible)
        {
            
        }

        /// <summary>
        /// Set the position of the preview.
        /// </summary>
        /// <param name="position">Position to apply to the preview.</param>
        /// <param name="local">Whether or not the position is local.</param>
        public virtual void SetPreviewPosition(Vector3 position, bool local)
        {
            
        }

        /// <summary>
        /// Set the rotation of the preview.
        /// </summary>
        /// <param name="rotation">Rotation to apply to the preview.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        public virtual void SetPreviewRotation(Quaternion rotation, bool local)
        {
            
        }

        /// <summary>
        /// Snap the preview to a certain position and rotation.
        /// </summary>
        /// <param name="position">Position to snap the preview to.</param>
        /// <param name="rotation">Rotation to snap the preview to.</param>
        public virtual void SnapPreview(Vector3 position, Quaternion rotation)
        {
            SetPreviewPosition(position, false);
            SetPreviewRotation(rotation, false);
        }

        /// <summary>
        /// Reset the position and rotation of the preview.
        /// </summary>
        public virtual void ResetPreview()
        {
            SetPreviewPosition(Vector3.zero, true);
            SetPreviewRotation(Quaternion.identity, false);
        }

        /// <summary>
        /// Accept the current preview.
        /// </summary>
        public virtual void AcceptPreview()
        {

        }

        /// <summary>
        /// Unity update method.
        /// </summary>
        private void Update()
        {
            float time = Time.deltaTime;
            if (synchronizer != null)
            {
                positionUpdateTime += time;
                rotationUpdateTime += time;
                scaleUpdateTime += time;
                sizeUpdateTime += time;
            }
        }
    }
}