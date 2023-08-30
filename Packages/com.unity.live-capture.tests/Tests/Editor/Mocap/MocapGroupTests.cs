using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.Mocap;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.Tests.Editor
{
    public class MocapGroupTests
    {
        const string k_TestBoneName = "Hips";
        const float k_CompareTolerance = 0.001f;

        Vector3EqualityComparer m_Vector3Comparer = new Vector3EqualityComparer(k_CompareTolerance);

        Animator m_Animator;
        MocapGroup m_Group;
        TestMocapDevice m_Device;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject(k_TestBoneName);
            m_Animator = go.AddComponent<Animator>();

            go = new GameObject("TestDevice");
            m_Group = go.AddComponent<MocapGroup>();

            go = new GameObject("TestSource");
            go.transform.SetParent(m_Group.transform); 
            m_Device = go.AddComponent<TestMocapDevice>();
            m_Device.Validate();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Animator.gameObject);
            GameObject.DestroyImmediate(m_Group.gameObject);
        }

        [Test]
        public void AssignAnimatorDoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_Group.Animator = null);
            Assert.DoesNotThrow(() => m_Group.Animator = m_Animator);
            Assert.DoesNotThrow(() => m_Group.Animator = null);
        }

        [Test]
        public void AssignAnimatorPropagatesToSources()
        {
            Assert.IsNull(m_Device.Animator, "Incorrect animator");

            m_Group.Animator = m_Animator;
            m_Group.InvokeUpdateDevice();

            Assert.AreEqual(m_Animator, m_Device.Animator, "Incorrect animator");
        }

        [Test]
        public void DeviceWithNullAnimatorDoesNotThrow()
        {
            m_Group.Animator = null;

            Assert.DoesNotThrow(() => m_Group.InvokeUpdateDevice());
            Assert.DoesNotThrow(() => m_Group.InvokeLiveUpdate());
            Assert.DoesNotThrow(() => m_Group.InvokeStartRecording());
            Assert.DoesNotThrow(() => m_Group.InvokeStopRecording());
        }

        [Test]
        public void OnRecordingChangedCalledAfterStartAndStopRecording()
        {
            var recordingChanged = 0;

            m_Device.RecordingChanged += () =>
            {
                ++recordingChanged;
            };

            m_Group.Animator = m_Animator;

            m_Group.InvokeStartRecording();

            Assert.AreEqual(1, recordingChanged, "Incorrect recording count");

            m_Group.InvokeStopRecording();

            Assert.AreEqual(2, recordingChanged, "Incorrect recording count");
        }

        [Test]
        public void MocapDeviceUsesMocapGroupRecorder()
        {
            m_Group.Animator = m_Animator;
            m_Group.InvokeUpdateDevice();

            var recorder = m_Group.GetRecorder();

            m_Device.PresentPublic(m_Device.transform, Vector3.one);

            var transforms = recorder.GetTransforms().ToArray();

            Assert.AreEqual(m_Device.transform, transforms[0], "Incorrect transform in recorder");

            m_Group.InvokeLiveUpdate();

            Assert.AreEqual(Vector3.one, transforms[0].localPosition, "Incorrect transform in recorder");
        }

        [Test]
        public void MocapGroupRecorderUsesMocapDeviceChannels()
        {
            var transform = m_Device.transform;

            m_Group.Animator = m_Animator;
            m_Group.InvokeUpdateDevice();

            var sourceRecorder = m_Device.GetRecorder();

            sourceRecorder.SetChannels(transform, TransformChannels.None);

            m_Device.PresentPublic(transform, Vector3.one);

            var deviceRecorder = m_Group.GetRecorder();
            var channels = deviceRecorder.GetChannels(transform);

            Assert.AreEqual(TransformChannels.None, channels, "Incorrect channels in recorder");

            m_Group.InvokeLiveUpdate();

            Assert.AreEqual(Vector3.zero, transform.localPosition, "Incorrect transform in recorder");
        }
    }
}
