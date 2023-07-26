using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.VirtualCamera.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    class GameViewTests
    {
        class ScreenSizeOverride : IDisposable
        {
            Func<Vector2> m_OriginalScreenSizeFunc;

            public ScreenSizeOverride(Vector2 size)
            {
                m_OriginalScreenSizeFunc = GameViewController.GetScreenSize;
                GameViewController.GetScreenSize = () => size;
            }

            public void Dispose()
            {
                GameViewController.GetScreenSize = m_OriginalScreenSizeFunc;
            }
        }

        static readonly FocusMode[] k_AllFocusModes = {FocusMode.Clear, FocusMode.Manual, FocusMode.ReticleAF, FocusMode.TrackingAF};
        static readonly Vector2 k_ScreenResolution = new Vector2(1024, 1024);

        ILiveCaptureBridge m_Bridge;
        IVirtualCameraDeviceProxy m_Proxy;

        [SetUp]
        public void Setup()
        {
            m_Bridge = Substitute.For<ILiveCaptureBridge>();
            m_Proxy = Substitute.For<IVirtualCameraDeviceProxy>();

            // Make sure the bridge return the substitute proxy.
            var anyProxy = Arg.Any<IVirtualCameraDeviceProxy>();
            m_Bridge.TryGetVirtualCameraDevice(out anyProxy).Returns(x =>
            {
                x[0] = m_Proxy;
                return true;
            });
        }

        [TearDown]
        public void TearDown()
        {
            GameViewController.instance.Disable();
        }

        [UnityTest]
        public IEnumerator CanInjectAndDisposeUIInEditMode()
        {
            Assert.IsFalse(Application.isPlaying);

            for (var it = UIInstantiationTest(true); it.MoveNext();)
            {
                yield return it.Current;
            }
        }

        [UnityTest]
        public IEnumerator CanInjectAndDisposeUIInPlayMode()
        {
            yield return new EnterPlayMode();

            for (var it = UIInstantiationTest(false); it.MoveNext();)
                yield return it.Current;

            yield return new ExitPlayMode();
        }

        IEnumerator UIInstantiationTest(bool destroyImmediate)
        {
            Assert.IsFalse(GameViewController.instance.IsActive);
            Assert.IsFalse(IsReticleInstantiated());

            GameViewController.instance.Enable(m_Bridge);
            Assert.IsTrue(GameViewController.instance.IsActive);
            yield return null;

            Assert.IsTrue(IsReticleInstantiated());
            yield return null;

            GameViewController.instance.Disable();
            Assert.IsFalse(GameViewController.instance.IsActive);

            // In playmode destruction is not immediate.
            if (destroyImmediate)
            {
                Assert.IsFalse(IsReticleInstantiated());
            }
        }

        public struct ProcessEventInput
        {
            public FocusMode FocusMode;
            public bool IsLive;
            public EventType EventType;
            public bool ShouldSetReticlePosition;
            public Vector2 MousePosition;
            public Vector2 ReticlePosition;
        }

        static IEnumerable<ProcessEventInput> ProcessEventSource()
        {
            // Invalid position.
            yield return new ProcessEventInput
            {
                FocusMode = FocusMode.Manual,
                IsLive = true,
                EventType = EventType.MouseDown,
                ShouldSetReticlePosition = false,
                MousePosition = k_ScreenResolution * -.5f,
                ReticlePosition = Vector2.zero
            };

            // Properly sends position according to focus mode.
            {
                var shouldSetReticlePosition = new[] { false, true, true, true };

                for (var i = 0; i != k_AllFocusModes.Length; ++i)
                {
                    yield return new ProcessEventInput
                    {
                        FocusMode = k_AllFocusModes[i],
                        IsLive = true,
                        EventType = EventType.MouseDown,
                        ShouldSetReticlePosition = shouldSetReticlePosition[i],
                        MousePosition = k_ScreenResolution * .5f,
                        ReticlePosition = Vector2.one * .5f
                    };
                }
            }

            // Drag on Reticle AF only.
            {
                var shouldSetReticlePosition = new[] { false, false, true, false };

                for (var i = 0; i != k_AllFocusModes.Length; ++i)
                {
                    yield return new ProcessEventInput
                    {
                        FocusMode = k_AllFocusModes[i],
                        IsLive = true,
                        EventType = EventType.MouseDrag,
                        ShouldSetReticlePosition = shouldSetReticlePosition[i],
                        MousePosition = k_ScreenResolution * .5f,
                        ReticlePosition = Vector2.one * .5f
                    };
                }
            }
        }

        [UnityTest]
        public IEnumerator TestsProcessEvent([ValueSource(nameof(ProcessEventSource))] ProcessEventInput input)
        {
            GameViewController.instance.Enable(m_Bridge);

            m_Proxy.FocusMode.Returns(input.FocusMode);
            m_Proxy.IsLive.Returns(input.IsLive);

            using (new ScreenSizeOverride(k_ScreenResolution))
            {
                // Wait a frame so that the coordinates transform is updated.
                // Screen size is pulled from the GameViewController on Update.
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;

                // Fetch controller, exactly one should have been instanciated.
                var controllers = Resources.FindObjectsOfTypeAll<GameViewReticleController>();
                Assert.IsTrue(controllers.Length == 1);
                controllers[0].ProcessEvent(input.EventType, input.MousePosition);
            }

            if (input.ShouldSetReticlePosition)
            {
                m_Proxy.Received(1).ReticlePosition = Arg.Is<Vector2>(x =>
                    Mathf.Approximately(x.x, input.ReticlePosition.x) &&
                    Mathf.Approximately(x.y, input.ReticlePosition.y));
            }
            else
            {
                m_Proxy.Received(0).ReticlePosition = Arg.Any<Vector2>();
            }

            GameViewController.instance.Disable();
        }

        [UnityTest]
        public IEnumerator FeatureDeactivatesOnDomainReload()
        {
            GameViewController.instance.Enable(m_Bridge);
            yield return null;

            Assert.IsTrue(IsReticleInstantiated());

            EditorUtility.RequestScriptReload();
            yield return new WaitForDomainReload();

            Assert.IsFalse(GameViewController.instance.IsActive);
            Assert.IsFalse(IsReticleInstantiated());
        }

        static bool IsReticleInstantiated()
        {
            return TryGetReticleController(out _);
        }

        static bool TryGetReticleController(out GameViewReticleController reticleController)
        {
            var result = Resources.FindObjectsOfTypeAll<GameViewReticleController>();
            if (result.Length == 0)
            {
                reticleController = null;
                return false;
            }
            else if (result.Length == 1) // Make sure only one instance exists.
            {
                reticleController = result[0];
                return true;
            }

            throw new InvalidOperationException(
                $"Only one instance of {nameof(GameViewReticleController)} expected, found {result.Length}.");
        }
    }
}
