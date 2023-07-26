using System.IO;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    [RequireComponent(typeof(Animator))]
    class TestBindingsDevice : LiveCaptureDevice
    {
        public override bool IsReady() => true;
        public override void Write(ITakeBuilder takeBuilder)
        {
            takeBuilder.CreateAnimationTrack(gameObject.name, GetComponent<Animator>(), new AnimationClip());
        }
    }

    public class TakeRecorderBindingsTests
    {
        const string shotName = "shot name";
        const string tmpDir = "Assets/tmp";

        internal ContextMock m_Context;
        internal TestBindingsDevice m_Device;

        [SetUp]
        public void Setup()
        {
            m_Context = new ContextMock()
            {
                shot = new Shot()
                {
                    Name = shotName,
                    Directory = tmpDir
                }
            };

            m_Device = new GameObject("test device", typeof(TestBindingsDevice)).GetComponent<TestBindingsDevice>();

            TakeRecorder.SetContext(m_Context);
            TakeRecorder.IsLive = true;
        }

        [TearDown]
        public void TearDown()
        {
            TakeRecorder.SetContext(null);

            if (m_Device != null)
            {
                GameObject.DestroyImmediate(m_Device.gameObject);
            }

            FileUtil.DeleteFileOrDirectory(tmpDir);
            FileUtil.DeleteFileOrDirectory($"{tmpDir}.meta");
            AssetDatabase.Refresh();
        }

        [Test]
        public void BindingAddedInTakeRecorderResolver()
        {
            var propertyName = new PropertyName(m_Device.gameObject.name);
            var animator = m_Device.GetComponent<Animator>();

            TakeRecorder.StartRecording();
            TakeRecorder.StopRecording();

            m_Context.resolver.Received(1).SetReferenceValue(Arg.Is(propertyName), Arg.Is(animator));
        }
    }
}