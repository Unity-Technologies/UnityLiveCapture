using System.Collections;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using Unity.LiveCapture.TransformCapture;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TransformCaptureDeviceTests
    {
        Animator m_Actor;
        TransformCaptureDevice m_Device;

        [SetUp]
        public void Setup()
        {
            TakeRecorder.FrameRate = StandardFrameRate.FPS_30_00;

            m_Actor = new GameObject("actor", typeof(Animator)).GetComponent<Animator>();
            m_Device = new GameObject("device", typeof(TransformCaptureDevice)).GetComponent<TransformCaptureDevice>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_Device != null)
            {
                GameObject.DestroyImmediate(m_Device.gameObject);
            }
        }

        [Test]
        public void IsNotReadyWithoutAnimator()
        {
            Assert.False(m_Device.IsReady(), "Incorrect ready state");
        }

        [Test]
        public void IsReadyWithAnimator()
        {
            m_Device.Actor = m_Actor;

            Assert.True(m_Device.IsReady(), "Incorrect ready state");
        }

        [Test]
        public void RecordingWhenReady()
        {
            m_Device.Actor = m_Actor;

            m_Device.InvokeStartRecording();

            Assert.True(m_Device.IsRecording, "Incorrect recording state");

            m_Device.InvokeStopRecording();

            Assert.False(m_Device.IsRecording, "Incorrect recording state");
        }

        [Test]
        public void RecordsInitialKeyframes()
        {
            m_Actor.transform.localPosition = Vector3.one;

            m_Device.Actor = m_Actor;

            m_Device.InvokeStartRecording();
            m_Device.InvokeStopRecording();

            var takeBuilder = Substitute.For<ITakeBuilder>();

            m_Device.Write(takeBuilder);

            var call = takeBuilder.ReceivedCalls().FirstOrDefault();

            Assert.AreEqual("CreateAnimationTrack", call.GetMethodInfo().Name, "Incorrect call");

            var clip = call.GetArguments()[2] as AnimationClip;

            Assert.NotNull(clip, "Invalid clip");

            m_Actor.transform.localPosition = Vector3.zero;

            Assert.AreNotEqual(Vector3.one, m_Actor.transform.localPosition, "Incorrect local position");

            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(m_Actor.gameObject, clip, 0f);

            Assert.AreEqual(Vector3.one, m_Actor.transform.localPosition, "Incorrect local position");

            AnimationMode.StopAnimationMode();

            GameObject.DestroyImmediate(clip);
        }

        [UnityTest]
        public IEnumerator Records()
        {
            m_Device.Actor = m_Actor;

            m_Device.InvokeStartRecording();

            m_Actor.transform.localPosition = Vector3.one;

            TakeRecorderImpl.Instance.RecordingElapsedTime = 1d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            m_Device.InvokeStopRecording();

            var takeBuilder = Substitute.For<ITakeBuilder>();

            m_Device.Write(takeBuilder);

            var call = takeBuilder.ReceivedCalls().FirstOrDefault();

            Assert.AreEqual("CreateAnimationTrack", call.GetMethodInfo().Name, "Incorrect call");

            var clip = call.GetArguments()[2] as AnimationClip;

            Assert.NotNull(clip, "Invalid clip");

            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(m_Actor.gameObject, clip, 0f);

            Assert.AreEqual(Vector3.zero, m_Actor.transform.localPosition, "Incorrect local position");

            AnimationMode.SampleAnimationClip(m_Actor.gameObject, clip, 1f);

            Assert.AreEqual(Vector3.one, m_Actor.transform.localPosition, "Incorrect local position");

            AnimationMode.StopAnimationMode();

            GameObject.DestroyImmediate(clip);
        }
    }
}
