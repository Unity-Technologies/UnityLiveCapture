using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.VirtualCamera.Editor;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class VirtualCameraPropertyPreview
    {
        VirtualCameraDevice m_Device;
        IVirtualCameraClientInternal m_Client;
        VirtualCameraActor m_Actor;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("camera capture");

            m_Device = go.AddComponent<VirtualCameraDevice>();
            m_Client = Substitute.For<IVirtualCameraClientInternal>();
            m_Actor = VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
                .GetComponent<VirtualCameraActor>();
            m_Device.Processor.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Device.gameObject);
            GameObject.DestroyImmediate(m_Actor.gameObject);
        }

        [Test]
        public void PropertiesRegisterAndUnregisterOnSetClient()
        {
            const double tolerance = 0.01;

            var lens0 = new Lens()
            {
                FocalLength = 30f,
                FocusDistance = 31f,
                Aperture = 3f
            };

            var lens1 = new Lens()
            {
                FocalLength = 40f,
                FocusDistance = 41f,
                Aperture = 4f
            };

            m_Actor.Lens = lens0;

            m_Device.Actor = m_Actor;

            Assert.AreEqual(lens0.FocalLength, m_Actor.Lens.FocalLength, "Incorrect focal length");
            Assert.AreEqual(lens0.FocusDistance, m_Actor.Lens.FocusDistance, "Incorrect focus distance");
            Assert.AreEqual(lens0.Aperture, m_Actor.Lens.Aperture, "Incorrect aperture");

            m_Device.SetClient(m_Client, false);
            m_Device.Lens = lens1;
            m_Device.InvokeLiveUpdate();

            Assert.That(m_Actor.Lens.FocalLength, Is.EqualTo(lens1.FocalLength).Within(tolerance));
            Assert.That(m_Actor.Lens.FocusDistance, Is.EqualTo(lens1.FocusDistance).Within(tolerance));
            Assert.That(m_Actor.Lens.Aperture, Is.EqualTo(lens1.Aperture).Within(tolerance));

            m_Device.SetClient(null, false);

            Assert.AreEqual(lens0.FocalLength, m_Actor.Lens.FocalLength, "Incorrect focal length");
            Assert.AreEqual(lens0.FocusDistance, m_Actor.Lens.FocusDistance, "Incorrect focus distance");
            Assert.AreEqual(lens0.Aperture, m_Actor.Lens.Aperture, "Incorrect aperture");
        }

        [Test]
        public void PropertiesRegisterAndUnregisterOnSetActor()
        {
            const double tolerance = 0.01;

            var lens0 = new Lens()
            {
                FocalLength = 30f,
                FocusDistance = 31f,
                Aperture = 3f
            };

            var lens1 = new Lens()
            {
                FocalLength = 40f,
                FocusDistance = 41f,
                Aperture = 4f
            };

            m_Actor.Lens = lens0;

            m_Device.Lens = lens1;
            m_Device.SetClient(m_Client, false);
            m_Device.Actor = m_Actor;
            m_Device.InvokeLiveUpdate();

            Assert.That(m_Actor.Lens.FocalLength, Is.EqualTo(lens1.FocalLength).Within(tolerance));
            Assert.That(m_Actor.Lens.FocusDistance, Is.EqualTo(lens1.FocusDistance).Within(tolerance));
            Assert.That(m_Actor.Lens.Aperture, Is.EqualTo(lens1.Aperture).Within(tolerance));

            m_Device.Actor = null;

            Assert.AreEqual(lens0.FocalLength, m_Actor.Lens.FocalLength, "Incorrect focal length");
            Assert.AreEqual(lens0.FocusDistance, m_Actor.Lens.FocusDistance, "Incorrect focus distance");
            Assert.AreEqual(lens0.Aperture, m_Actor.Lens.Aperture, "Incorrect aperture");
        }

        [Test]
        public void PreviwableProperties()
        {
            var previewer = Substitute.For<IPropertyPreviewer>();

            m_Device.Actor = m_Actor;
            m_Device.RegisterLiveProperties(previewer);

            previewer.Received(1).Register(m_Actor, "m_Lens.m_FocalLength");
            previewer.Received(1).Register(m_Actor, "m_Lens.m_FocusDistance");
            previewer.Received(1).Register(m_Actor, "m_Lens.m_Aperture");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_FocalLengthRange.x");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_FocalLengthRange.y");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_CloseFocusDistance");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_ApertureRange.x");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_ApertureRange.y");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_LensShift.x");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_LensShift.y");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_BladeCount");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_Curvature.x");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_Curvature.y");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_BarrelClipping");
            previewer.Received(1).Register(m_Actor, "m_LensIntrinsics.m_Anamorphism");
            previewer.Received(1).Register(m_Actor, "m_DepthOfField");
            previewer.Received(1).Register(m_Actor, "m_CropAspect");
            previewer.Received(1).Register(m_Actor, "m_LocalPosition.x");
            previewer.Received(1).Register(m_Actor, "m_LocalPosition.y");
            previewer.Received(1).Register(m_Actor, "m_LocalPosition.z");
            previewer.Received(1).Register(m_Actor, "m_LocalEulerAngles.x");
            previewer.Received(1).Register(m_Actor, "m_LocalEulerAngles.y");
            previewer.Received(1).Register(m_Actor, "m_LocalEulerAngles.z");
            previewer.Received(1).Register(m_Actor, "m_LocalPositionEnabled");
            previewer.Received(1).Register(m_Actor, "m_LocalEulerAnglesEnabled");
        }
    }
}
