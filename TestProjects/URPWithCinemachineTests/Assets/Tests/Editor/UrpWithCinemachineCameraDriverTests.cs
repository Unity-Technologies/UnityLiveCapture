﻿using Cinemachine;
using Cinemachine.PostFX;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.VirtualCamera.Editor;
using Unity.LiveCapture.Tests.Editor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class UrpWithCinemachineCameraDriverTests : BaseCameraDriverTests
{
    protected override GameObject CreateVirtualCameraActor()
    {
        VirtualCameraCreatorUtilitiesCM.CreateCinemachineCameraActor();
        var driver = GameObject.FindObjectOfType<CinemachineCameraDriver>();
        Assert.IsNotNull(driver, $"Could not access {nameof(CinemachineCameraDriver)}.");
        return driver.gameObject;
    }

    protected override void ValidateLensAndBodySynchronization(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
    {
        var volume = m_GameObject.GetComponentInChildren<CinemachineVolumeSettings>();
        Assert.IsNotNull(volume);
        var profile = volume.m_Profile;
        Assert.IsNotNull(volume);
        Assert.IsTrue(profile.TryGet(out DepthOfField depthOfField));
        Assert.IsTrue(Mathf.Approximately(lens.FocusDistance, depthOfField.focusDistance.value));

        var cinemachineVirtualCamera = m_GameObject.GetComponentInChildren<CinemachineVirtualCamera>();
        Assert.IsNotNull(cinemachineVirtualCamera);
        var cinemachineLens = cinemachineVirtualCamera.m_Lens;

        Assert.IsTrue(Mathf.Approximately(cameraBody.SensorSize.x, cinemachineLens.SensorSize.x));
        Assert.IsTrue(Mathf.Approximately(cameraBody.SensorSize.y, cinemachineLens.SensorSize.y));
        var focalLength = Camera.FieldOfViewToFocalLength(cinemachineLens.FieldOfView, cinemachineLens.SensorSize.y);
        Assert.IsTrue(Mathf.Approximately(lens.FocalLength, focalLength));
    }
}
