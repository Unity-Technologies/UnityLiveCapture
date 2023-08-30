using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Unity.LiveCapture.Mocap;

namespace Unity.LiveCapture.Tests.Editor
{
    class TestMocapDevice : MocapDevice<int>
    {
        public event Action<int> FrameProcessed;
        public event Action RecordingChanged;

        public List<int> ProcessedFrames = new List<int>();

        public override bool IsReady() => true;

        protected override void UpdateDevice() {}

        public void AddFramePublic(int frame, Timecode timecode, FrameRate frameRate)
        {
            AddFrame(frame, new FrameTimeWithRate(frameRate, timecode.ToFrameTime(frameRate)));
        }

        public void PresentPublic(Transform transform, Vector3 position)
        {
            Present(transform, position, null, null);
        }

        protected override void ProcessFrame(int frame)
        {
            ProcessedFrames.Add(frame);

            FrameProcessed?.Invoke(frame);
        }

        protected override void OnRecordingChanged()
        {
            RecordingChanged?.Invoke();
        }
    }

    public class MocapDeviceTests
    {
        TestMocapDevice m_Device;
        Animator m_Animator;

        [SetUp]
        public void Setup()
        {
            m_Device = new GameObject("Source", typeof(TestMocapDevice)).GetComponent<TestMocapDevice>();
            m_Animator = new GameObject("Actor", typeof(Animator)).GetComponent<Animator>();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Device.gameObject);
            GameObject.DestroyImmediate(m_Animator.gameObject);
        }

        [Test]
        public void AddFrameCallsProcessFrameImmediatellyWhenNotSyncronized()
        {
            m_Device.AddFramePublic(3, default, StandardFrameRate.FPS_30_00);

            Assert.AreEqual(1, m_Device.ProcessedFrames.Count, "Incorrect frame count");
            Assert.AreEqual(3, m_Device.ProcessedFrames[0], "Incorrect frame value");
        }

        [Test]
        public void AddFrameDoesNotProcessFrameImmediatellyWhenSyncronized()
        {
            m_Device.SyncBuffer.IsSynchronized = true;
            m_Device.AddFramePublic(3, default, StandardFrameRate.FPS_30_00);

            Assert.AreEqual(0, m_Device.ProcessedFrames.Count, "Incorrect frame count");
        }

        [Test]
        public void CannotCallAddFrameFromProcessFrame()
        {
            var attemptedToAddFrame = false;

            m_Device.FrameProcessed += (f) =>
            {
                m_Device.AddFramePublic(4, default, default);
                attemptedToAddFrame = true;
            };

            m_Device.AddFramePublic(3, default, StandardFrameRate.FPS_30_00);

            Assert.True(attemptedToAddFrame, "Did not attempt to add a frame from ProcessFrame");
            Assert.AreEqual(1, m_Device.ProcessedFrames.Count, "Incorrect frame count");
            Assert.AreEqual(3, m_Device.ProcessedFrames[0], "Incorrect frame value");
        }

        [Test]
        public void PresentDoesNotUpdateTransformImmediatelly()
        {
            m_Device.Animator = m_Animator;
            m_Device.PresentPublic(m_Device.transform, Vector3.one);

            Assert.AreNotEqual(Vector3.one, m_Device.transform.localPosition, "Incorrect position");

            m_Device.InvokeLiveUpdate();

            Assert.AreEqual(Vector3.one, m_Device.transform.localPosition, "Incorrect position");
        }

        [Test]
        public void PresentTransformRegistersLivePropertiesAndDisableRestores()
        {
            Assert.AreEqual(Vector3.zero, m_Device.transform.localPosition, "Incorrect position");

            m_Device.Animator = m_Animator;
            m_Device.PresentPublic(m_Device.transform, Vector3.one);
            m_Device.InvokeLiveUpdate();

            Assert.AreEqual(Vector3.one, m_Device.transform.localPosition, "Incorrect position");

            m_Device.enabled = false;

            Assert.AreEqual(Vector3.zero, m_Device.transform.localPosition, "Incorrect position");
        }
    }
}
