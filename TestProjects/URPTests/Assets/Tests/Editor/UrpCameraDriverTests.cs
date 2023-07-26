using System.Collections;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.Tests.Editor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

class UrpCameraDriverTests : BaseCameraDriverTests
{
    [UnityTest]
    public IEnumerator TestFocusModeSynchronization()
    {
        var depthOfField = GetDepthOfField();
        yield return TestFocusModeSynchronizationInternal(depthOfField);
    }

    DepthOfField GetDepthOfField()
    {
        var volume = m_GameObject.GetComponent<Volume>();
        Assert.IsNotNull(volume);

        var profile = volume.profile;
        Assert.IsNotNull(volume);

        Assert.IsTrue(profile.TryGet(out DepthOfField depthOfField));

        return depthOfField;
    }

    protected override GameObject CreateVirtualCameraActor()
    {
        return new GameObject("Virtual Camera Actor", typeof(PhysicalCameraDriver));
    }

    protected override void ValidateLensAndBodySynchronization(Lens lens, LensIntrinsics instrinsics, CameraBody cameraBody)
    {
        var camera = m_GameObject.GetComponent<Camera>();
        Assert.IsNotNull(camera);

        ValidateCameraSynchronization(camera, lens, instrinsics, cameraBody);

        var volume = m_GameObject.GetComponent<Volume>();
        Assert.IsNotNull(volume);

        var profile = volume.profile;
        Assert.IsNotNull(volume);

        Assert.IsTrue(profile.TryGet(out DepthOfField depthOfField));

        Assert.IsTrue(Mathf.Approximately(lens.Aperture, depthOfField.aperture.value));
        Assert.IsTrue(Mathf.Approximately(lens.FocalLength, depthOfField.focalLength.value));
        Assert.IsTrue(Mathf.Approximately(instrinsics.BladeCount, depthOfField.bladeCount.value));
        Assert.IsTrue(Mathf.Approximately(lens.FocusDistance, depthOfField.focusDistance.value));
    }
}
