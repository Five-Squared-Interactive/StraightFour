// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Synchronization;
using UnityEditor;

public class SynchronizationTests
{
    [UnityTest]
    public IEnumerator SynchronizationTests_General()
    {
        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // Set Visibility.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetVisibility(null, false));

        // Delete Synchronized Entity.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.DeleteSynchronizedEntity(null));

        // Set Highlight.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetHighlight(null, false));

        // Set Parent.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetParent(null, null));

        // Set Position.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPosition(null, Vector3.zero));

        // Set Rotation.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetRotation(null, Quaternion.identity));

        // Set Scale.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetScale(null, Vector3.zero));

        // Set Size.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetSize(null, Vector3.zero));

        // Set Physical Properties.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPhysicalProperties(null, null));

        // Set Motion.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetMotion(null, null));

        // Make World Canvas.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.MakeWorldCanvas(null));

        // Make Screen Canvas.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.MakeScreenCanvas(null));
    }
}