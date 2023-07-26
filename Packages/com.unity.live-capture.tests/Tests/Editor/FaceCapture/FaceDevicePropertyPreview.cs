using System;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.ARKitFaceCapture;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class FaceDevicePropertyPreview
    {
        FaceDevice m_Device;
        IFaceClientInternal m_Client;
        FaceActor m_Actor;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("camera capture");

            m_Device = go.AddComponent<FaceDevice>();
            m_Client = Substitute.For<IFaceClientInternal>();
            m_Actor = new GameObject("Actor", typeof(FaceActor))
                .GetComponent<FaceActor>();
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
            var pose0 = new FacePose()
            {
                HeadPosition = Vector3.one
            };
            var pose1 = new FacePose()
            {
                HeadPosition = Vector3.one * 2f
            };
            var faceSample1 = new FaceSample()
            {
                FacePose = pose1
            };
            
            m_Actor.HeadPosition = pose0.HeadPosition;

            m_Device.Actor = m_Actor;

            Assert.AreEqual(pose0.HeadPosition, m_Actor.HeadPosition, "Incorrect head position");

            m_Device.SetClient(m_Client, false);

            m_Client.FacePoseSampleReceived += Raise.Event<Action<FaceSample>>(faceSample1);
            
            m_Device.InvokeLiveUpdate();

            Assert.AreEqual(pose1.HeadPosition, m_Actor.HeadPosition, "Incorrect head position");

            m_Device.SetClient(null, false);

            Assert.AreEqual(pose0.HeadPosition, m_Actor.HeadPosition, "Incorrect head position");
        }

        [Test]
        public void PropertiesRegisterAndUnregisterOnSetActor()
        {
            var pose0 = new FacePose()
            {
                HeadPosition = Vector3.one
            };
            var pose1 = new FacePose()
            {
                HeadPosition = Vector3.one * 2f
            };
            var faceSample1 = new FaceSample()
            {
                FacePose = pose1
            };

            m_Actor.HeadPosition = pose0.HeadPosition;

            m_Device.SetClient(m_Client, false);
            m_Client.FacePoseSampleReceived += Raise.Event<Action<FaceSample>>(faceSample1);
            m_Device.Actor = m_Actor;
            m_Device.InvokeLiveUpdate();

            Assert.AreEqual(pose1.HeadPosition, m_Actor.HeadPosition, "Incorrect head position");

            m_Device.Actor = null;
            
            Assert.AreEqual(pose0.HeadPosition, m_Actor.HeadPosition, "Incorrect head position");

            m_Device.SetClient(null, false);
        }

        [Test]
        public void PreviwableProperties()
        {
            var previewer = Substitute.For<IPropertyPreviewer>();

            m_Device.Actor = m_Actor;
            m_Device.RegisterLiveProperties(previewer);

            previewer.Received(1).Register(m_Actor, "m_HeadPosition.x");
            previewer.Received(1).Register(m_Actor, "m_HeadPosition.y");
            previewer.Received(1).Register(m_Actor, "m_HeadPosition.z");
            previewer.Received(1).Register(m_Actor, "m_HeadOrientation.x");
            previewer.Received(1).Register(m_Actor, "m_HeadOrientation.y");
            previewer.Received(1).Register(m_Actor, "m_HeadOrientation.z");
            previewer.Received(1).Register(m_Actor, "m_LeftEyeOrientation.x");
            previewer.Received(1).Register(m_Actor, "m_LeftEyeOrientation.y");
            previewer.Received(1).Register(m_Actor, "m_LeftEyeOrientation.z");
            previewer.Received(1).Register(m_Actor, "m_RightEyeOrientation.x");
            previewer.Received(1).Register(m_Actor, "m_RightEyeOrientation.y");
            previewer.Received(1).Register(m_Actor, "m_RightEyeOrientation.z");
            previewer.Received(1).Register(m_Actor, "m_BlendShapesEnabled");
            previewer.Received(1).Register(m_Actor, "m_HeadPositionEnabled");
            previewer.Received(1).Register(m_Actor, "m_HeadOrientationEnabled");
            previewer.Received(1).Register(m_Actor, "m_EyeOrientationEnabled");

            foreach (var shape in FaceBlendShapePose.Shapes)
            {
                previewer.Received(1).Register(m_Actor, $"m_BlendShapes.{shape}");
            }
        }
    }
}
