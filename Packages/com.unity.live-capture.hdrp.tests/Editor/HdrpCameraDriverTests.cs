using System;
using System.Collections;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.Tests.Editor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp
{
    class HdrpCameraDriverTests : BaseCameraDriverTests
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

        protected override void ValidateLensAndBodySynchronization(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
            var camera = m_GameObject.GetComponent<Camera>();
            Assert.IsNotNull(camera);

            ValidateCameraSynchronization(camera, lens, intrinsics, cameraBody);

            var depthOfField = GetDepthOfField();

            Assert.IsTrue(Mathf.Approximately(lens.FocusDistance, depthOfField.focusDistance.value));

            Assert.IsTrue(Mathf.Approximately(lens.Aperture, camera.aperture));
            Assert.IsTrue(Mathf.Approximately(intrinsics.BladeCount, camera.bladeCount));
            Assert.IsTrue(Mathf.Approximately(intrinsics.Curvature.x, camera.curvature.x));
            Assert.IsTrue(Mathf.Approximately(intrinsics.Curvature.y, camera.curvature.y));
            Assert.IsTrue(Mathf.Approximately(intrinsics.BarrelClipping, camera.barrelClipping));
            Assert.IsTrue(Mathf.Approximately(intrinsics.Anamorphism, camera.anamorphism));
        }
    }
}
