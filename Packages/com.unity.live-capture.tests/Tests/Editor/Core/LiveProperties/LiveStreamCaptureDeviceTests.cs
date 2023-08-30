using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    public class LiveStreamCaptureDeviceTests
    {
        class TestDevice : LiveStreamCaptureDevice
        {
            public LivePropertyHandle Handle { get; private set; }
            public Vector3 LocalPosition { get; set; }

            public override bool IsReady() => true;
            
            public override void Write(ITakeBuilder takeBuilder) {}

            public void InvokeUpdateStream(Transform root, FrameTimeWithRate frameTime)
            {
                base.UpdateStream(root, frameTime);
            }

            public void InvokeRegisterLiveProperties()
            {
                base.RegisterLiveProperties();
            }

            public void InvokeRestoreLiveProperties()
            {
                base.RestoreLiveProperties();
            }
            
            protected override void CreateLiveProperties(LiveStream stream)
            {
                Handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            }

            protected override void ProcessFrame(LiveStream stream)
            {
                stream.SetValue(Handle, LocalPosition);
            }
        }

        class OverridePostProcessor : LiveStreamPostProcessor
        {
            LivePropertyHandle m_Handle;

            public LivePropertyHandle Handle => m_Handle;
            public Vector3 LocalPositionOverride { get; set; }

            protected override void CreateLiveProperties(LiveStream stream)
            {
                stream.TryGetHandle("m_LocalPosition", typeof(Transform), out m_Handle);
            }

            protected override void PostProcessFrame(LiveStream stream)
            {
                if (stream.IsHandleValid(m_Handle))
                {
                    stream.SetValue(m_Handle, LocalPositionOverride);
                }
            }
        }

        class CreatePropertiesPostProcessor : LiveStreamPostProcessor
        {
            LivePropertyHandle m_Handle;

            public LivePropertyHandle Handle => m_Handle;
            public Vector3 LocalScale { get; set; }

            protected override void CreateLiveProperties(LiveStream stream)
            {
                m_Handle = stream.CreateProperty<Transform, Vector3>("m_LocalScale");
            }

            protected override void RemoveLiveProperties(LiveStream stream)
            {
                stream.RemoveProperty(m_Handle);
            }

            protected override void PostProcessFrame(LiveStream stream)
            {
                if (stream.IsHandleValid(m_Handle))
                {
                    stream.SetValue(m_Handle, LocalScale);
                }
            }
        }

        class PreviwableComponent : MonoBehaviour, IPreviewable
        {
            public bool RegisterCalled { get; private set; }

            public void Register(IPropertyPreviewer previewer)
            {
                RegisterCalled = true;
            }
        }

        [Test]
        public void LiveStreamCaptureDevice_CreateLiveProperties_GetsInvokedOnEnable()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();

            Assert.IsTrue(device.Stream.IsHandleValid(device.Handle), "The handle should be valid.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_StreamResets_OnDisable()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();

            gameObject.SetActive(false);

            Assert.IsFalse(device.Stream.IsHandleValid(device.Handle), "The handle should be invalid.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_UpdateStream_InvokesProcessFrame()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();
            var frameTime = new FrameTimeWithRate(new FrameRate(1), new FrameTime(0));
            var expectedValue = Vector3.one;
            device.LocalPosition = expectedValue;
            device.InvokeUpdateStream(null, frameTime);
            
            Assert.IsTrue(device.Stream.TryGetValue<Vector3>(device.Handle, out var value), "The value should be valid.");
            Assert.AreEqual(expectedValue, value, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_UpdateStream_InvokesPostProcessors()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();
            var postProcessor = gameObject.AddComponent<OverridePostProcessor>();
            var frameTime = new FrameTimeWithRate(new FrameRate(1), new FrameTime(0));
            var expectedValue = Vector3.one;

            postProcessor.LocalPositionOverride = expectedValue;

            device.InvokeUpdateStream(null, frameTime);
            
            Assert.IsTrue(device.Stream.TryGetValue<Vector3>(device.Handle, out var value), "The value should be valid.");
            Assert.AreEqual(expectedValue, value, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_UpdateStream_InvokesPostProcessors_CreateExtraLiveProperties()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();
            var postProcessor = gameObject.AddComponent<CreatePropertiesPostProcessor>();
            var frameTime = new FrameTimeWithRate(new FrameRate(1), new FrameTime(0));
            var expectedValue = Vector3.one;

            postProcessor.LocalScale = expectedValue;

            device.InvokeUpdateStream(null, frameTime);
            
            Assert.IsTrue(device.Stream.TryGetValue<Vector3>(postProcessor.Handle, out var value), "The value should be valid.");
            Assert.AreEqual(expectedValue, value, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_OnDisablePostProcessor_CallsRemoveLiveProperties()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();
            var postProcessor = gameObject.AddComponent<CreatePropertiesPostProcessor>();
            var frameTime = new FrameTimeWithRate(new FrameRate(1), new FrameTime(0));
            var expectedValue = Vector3.one;

            postProcessor.LocalScale = expectedValue;

            device.InvokeUpdateStream(null, frameTime);
            
            Assert.IsTrue(device.Stream.TryGetValue<Vector3>(postProcessor.Handle, out var value), "The value should be valid.");
            Assert.AreEqual(expectedValue, value, "The value should be correct.");

            postProcessor.enabled = false;

            Assert.IsFalse(device.Stream.IsHandleValid(postProcessor.Handle), "The handle should be invalid.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_LiveUpdate_AppliesValues()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();
            var frameTime = new FrameTimeWithRate(new FrameRate(1), new FrameTime(0));
            var expectedValue = Vector3.one;

            device.LocalPosition = expectedValue;
            device.InvokeUpdateStream(gameObject.transform, frameTime);
            device.InvokeLiveUpdate();

            Assert.AreEqual(expectedValue, gameObject.transform.localPosition, "The value should be correct.");
        }

        [Test]
        public void LiveStreamCaptureDevice_RegisterLiveProperties_InvokesRegisterOnPreviewable()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();
            var previewable = gameObject.AddComponent<PreviwableComponent>();

            device.InvokeUpdateStream(gameObject.transform, new FrameTimeWithRate(new FrameRate(1), new FrameTime(0)));
            device.InvokeLiveUpdate();
            device.InvokeRegisterLiveProperties();

            Assert.IsTrue(previewable.RegisterCalled, "Register should be called.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStreamCaptureDevice_RestoreLiveProperties_RestoresValues()
        {
            var gameObject = new GameObject();
            var device = gameObject.AddComponent<TestDevice>();

            device.InvokeUpdateStream(gameObject.transform, default(FrameTimeWithRate));
            device.InvokeLiveUpdate();
            device.InvokeRegisterLiveProperties();
            device.LocalPosition = Vector3.one;
            device.InvokeUpdateStream(gameObject.transform, default(FrameTimeWithRate));
            device.InvokeLiveUpdate();
            device.InvokeRestoreLiveProperties();

            Assert.AreEqual(Vector3.zero, gameObject.transform.localPosition, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }
    }
}
