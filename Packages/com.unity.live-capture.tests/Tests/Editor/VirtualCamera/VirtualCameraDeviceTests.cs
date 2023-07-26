using System;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.Tests.Editor
{
    abstract class VirtualCameraDeviceTests
    {
        protected VirtualCameraDevice m_Device;
        protected VirtualCameraActor m_Actor;
        protected IVirtualCameraClientInternal m_Client;
        protected ITakeBuilder m_TakeBuilder;

        [SetUp]
        public void Setup()
        {
            TakeRecorder.IsLive = false;
            TakeRecorder.FrameRate = StandardFrameRate.FPS_30_00.ToValue();

            m_Actor = new GameObject("actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();
            m_Device = new GameObject("vcam device").AddComponent<VirtualCameraDevice>();

            m_Client = Substitute.For<IVirtualCameraClientInternal>();
            m_TakeBuilder = Substitute.For<ITakeBuilder>();

            m_Device.Actor = m_Actor;
            m_Device.SetClient(m_Client, false);
            m_Device.Recorder.PositionError = -1f;
            m_Device.Recorder.RotationError = -1f;
            m_Device.Recorder.LensError = -1f;
            m_Device.Processor.TimeShiftTolerance = int.MaxValue;
            m_Device.Processor.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Device.gameObject);
            GameObject.DestroyImmediate(m_Actor.gameObject);
        }

        protected bool ContainsCurveWithProperty(
            AnimationClip clip,
            string relativePath,
            string propertyName,
            Type bindingType)
        {
            return GetCurve(clip, relativePath, propertyName, bindingType) != null;
        }

        protected AnimationCurve GetCurve(
            AnimationClip clip,
            string relativePath,
            string propertyName,
            Type bindingType)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var binding = bindings.Where(c => c.propertyName == propertyName &&
                c.path == relativePath &&
                c.type == bindingType).FirstOrDefault();

            if (binding != null)
            {
                return AnimationUtility.GetEditorCurve(clip, binding);
            }

            return null;
        }

        protected bool DestroyAndReturn(AnimationClip c)
        {
            GameObject.DestroyImmediate(c);
            return true;
        }
    }

    // These tests are failing intermittently (or freezing the editor) on Bokken because of a potential issue in AssetDatabase.
    // Disabling to unblock our ci pipeline but we will reevaluate from time to time.
#if UNITY_2020_3
    [UnityPlatform(exclude = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer})]
#endif
    class VirtualCameraDeviceTestsUnstable : VirtualCameraDeviceTests
    {
        [Test]
        public void StartRecordingRecordsInitialKeyframes()
        {
            m_Device.InvokeStartRecording();
            m_Device.InvokeStopRecording();

            var clip = m_Device.Recorder.Bake();
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            var actorType = typeof(VirtualCameraActor);

            Assert.AreEqual(25, curveBindings.Length, "Incorrect number of curve bindings");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalPosition.x", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalPosition.y", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalPosition.z", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalEulerAngles.x", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalEulerAngles.y", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalEulerAngles.z", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalPositionEnabled", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LocalEulerAnglesEnabled", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_Lens.m_FocalLength", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_Lens.m_FocusDistance", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_Lens.m_Aperture", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_FocalLengthRange.x", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_FocalLengthRange.y", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_CloseFocusDistance", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_ApertureRange.x", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_ApertureRange.y", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_LensShift.x", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_LensShift.y", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_BladeCount", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_Curvature.x", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_Curvature.y", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_BarrelClipping", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_LensIntrinsics.m_Anamorphism", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_DepthOfField", actorType), "Did not bake the expected property");
            Assert.True(ContainsCurveWithProperty(clip, "", "m_CropAspect", actorType), "Did not bake the expected property");

            foreach (var binding in curveBindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var keys = curve.keys;

                Assert.AreEqual(1, keys.Length, $"Incorrect number of keys in curve {binding.propertyName}");
            }

            UnityEngine.Object.DestroyImmediate(clip);
        }

        [Test]
        public void ReceiveInputSampleUpdatesAndRecords()
        {
            var inputSample0 = new InputSample
            {
                Time = 0.0,
                ARPose = new Pose(Vector3.zero, Quaternion.identity)
            };
            var inputSample1 = new InputSample
            {
                Time = 1.0,
                ARPose = new Pose(Vector3.one, Quaternion.identity)
            };
            var inputSample2 = new InputSample
            {
                Time = 2.0,
                ARPose = new Pose(Vector3.left, Quaternion.identity)
            };
            var tick = SampleProcessor.k_KeyframeBufferFrameRate.FrameInterval;

            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample0);

            m_Device.MarkClientTime(1d - tick);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();

            m_Device.InvokeStartRecording();

            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample1);
            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample2);

            m_Device.MarkClientTime(2d);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStopRecording();

            var clip = m_Device.Recorder.Bake();
            var curve1 = GetCurve(clip, "", "m_LocalPosition.x", typeof(VirtualCameraActor));

            Assert.AreEqual(inputSample2.ARPose, m_Device.Pose, "Incorrect pose");
            Assert.AreEqual(31, curve1.keys.Length, "Incorrect count");
            Assert.AreEqual(0f, curve1.keys[0].time, "Incorrect time");
            Assert.AreEqual(1f, curve1.Evaluate(0f), "Incorrect value at time");
            Assert.AreEqual(-1f, curve1.Evaluate(1f), "Incorrect value at time");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void RecordsClipStartingAtTimeZero()
        {
            var inputSample0 = new InputSample()
            {
                Time = 0.0,
                ARPose = new Pose(Vector3.zero, Quaternion.identity)
            };
            var inputSample1 = new InputSample()
            {
                Time = 1.0,
                ARPose = new Pose(Vector3.one, Quaternion.identity)
            };
            var inputSample2 = new InputSample()
            {
                Time = 2.0,
                ARPose = new Pose(Vector3.left, Quaternion.identity)
            };
            var lensData = new FocalLengthSample()
            {
                Time = 20.0,
                FocalLength = 10f,
            };
            var lensData2 = new FocalLengthSample()
            {
                Time = 21.0,
                FocalLength = 20f,
            };

            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample0);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStartRecording();
            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample1);
            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample2);
            m_Client.FocalLengthSampleReceived += Raise.Event<Action<FocalLengthSample>>(lensData);
            m_Client.FocalLengthSampleReceived += Raise.Event<Action<FocalLengthSample>>(lensData2);

            m_Device.MarkClientTime(25.0);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStopRecording();

            var clip = m_Device.Recorder.Bake();
            var curve1 = GetCurve(clip, "", "m_LocalPosition.x", typeof(VirtualCameraActor));
            var curve2 = GetCurve(clip, "", "m_LocalPositionEnabled", typeof(VirtualCameraActor));
            var curve3 = GetCurve(clip, "", "m_Lens.m_FocalLength", typeof(VirtualCameraActor));

            Assert.AreEqual(inputSample2.ARPose, m_Device.Pose, "Incorrect pose");
            Assert.AreEqual(0f, curve1.keys[0].time, "Incorrect time");
            Assert.AreEqual(0f, curve2.keys[0].time, "Incorrect time");
            Assert.AreEqual(0f, curve3.keys[0].time, "Incorrect time");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void ReceiveFocalLengthUpdatesAndRecords()
        {
            var lensData = new FocalLengthSample
            {
                Time = 1.0,
                FocalLength = 15f,  // minimum value is 14
            };
            var lensData2 = new FocalLengthSample
            {
                Time = 2.0,
                FocalLength = 20f,
            };

            var tick = SampleProcessor.k_KeyframeBufferFrameRate.FrameInterval;

            m_Device.MarkClientTime(1.0 - tick);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStartRecording();
            m_Client.FocalLengthSampleReceived += Raise.Event<Action<FocalLengthSample>>(lensData);
            m_Client.FocalLengthSampleReceived += Raise.Event<Action<FocalLengthSample>>(lensData2);

            m_Device.MarkClientTime(6.0);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStopRecording();

            var clip = m_Device.Recorder.Bake();
            var comparer = new FloatEqualityComparer(0.001f);

            Assert.AreEqual(lensData2.FocalLength, m_Device.Lens.FocalLength, "Incorrect lens");

            var curve = GetCurve(clip, "", "m_Lens.m_FocalLength", typeof(VirtualCameraActor));

            var expectedClipLength = 1 + Mathf.CeilToInt(5 / (float)TakeRecorder.FrameRate.FrameInterval);

            Assert.AreEqual(expectedClipLength, curve.keys.Length, "Incorrect sample count");
            Assert.AreEqual(0f, curve.keys[0].time, "Incorrect time");

            // First sample will apply immediately, initializes the timestamp tracker.
            Assert.That(curve.keys[1].value, Is.EqualTo(15f).Using(comparer), "Incorrect value");

            // Second sample applies 1s later
            var frame = 1 + Mathf.CeilToInt(1 / (float)TakeRecorder.FrameRate.FrameInterval);
            Assert.That(curve.keys[frame].value, Is.EqualTo(20f).Using(comparer), "Incorrect value");

            GameObject.DestroyImmediate(clip);
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        public void ReceiveFocusDistanceUpdatesAndRecords(FocusMode focusMode)
        {
            var lensData = new FocusDistanceSample
            {
                Time = 1.0,
                FocusDistance = 12f,
            };
            var lensData2 = new FocusDistanceSample
            {
                Time = 2.0,
                FocusDistance = 33f,
            };

            var tick = SampleProcessor.k_KeyframeBufferFrameRate.FrameInterval;

            m_Client.FocusModeReceived += Raise.Event<Action<FocusMode>>(focusMode);

            m_Device.MarkClientTime(1.0 - tick);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStartRecording();
            m_Client.FocusDistanceSampleReceived += Raise.Event<Action<FocusDistanceSample>>(lensData);
            m_Client.FocusDistanceSampleReceived += Raise.Event<Action<FocusDistanceSample>>(lensData2);

            m_Device.MarkClientTime(6.0);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStopRecording();

            var clip = m_Device.Recorder.Bake();
            var comparer = new FloatEqualityComparer(0.001f);

            Assert.AreEqual(lensData2.FocusDistance, m_Device.Lens.FocusDistance, "Incorrect lens");

            var curve = GetCurve(clip, "", "m_Lens.m_FocusDistance", typeof(VirtualCameraActor));
            var expectedClipLength = 1 + Mathf.CeilToInt(5 / (float)TakeRecorder.FrameRate.FrameInterval);

            Assert.AreEqual(expectedClipLength, curve.keys.Length, "Incorrect sample count");
            Assert.AreEqual(0f, curve.keys[0].time, "Incorrect time");

            Assert.That(curve.keys[1].value, Is.EqualTo(12f).Using(comparer), "Incorrect value");

            var frame = 1 + Mathf.CeilToInt(1 / (float)TakeRecorder.FrameRate.FrameInterval);
            Assert.That(curve.keys[frame].value, Is.EqualTo(33f).Using(comparer), "Incorrect value");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void ReceiveApertureUpdatesAndRecords()
        {
            var lensData = new ApertureSample
            {
                Time = 1.0,
                Aperture = 5f,
            };
            var lensData2 = new ApertureSample
            {
                Time = 2.0,
                Aperture = 10f,
            };

            var frameRate = TakeRecorder.FrameRate;
            var tick = SampleProcessor.k_KeyframeBufferFrameRate.FrameInterval;
            var settings = m_Device.Settings;

            settings.ApertureDamping = 0f;
            m_Device.Settings = settings;
            m_Device.MarkClientTime(1d - tick);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();

            m_Client.ApertureSampleReceived += Raise.Event<Action<ApertureSample>>(lensData);
            m_Client.ApertureSampleReceived += Raise.Event<Action<ApertureSample>>(lensData2);

            m_Device.InvokeStartRecording();
            m_Device.MarkClientTime(6d);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeLiveUpdate();
            m_Device.InvokeStopRecording();

            var clip = m_Device.Recorder.Bake();
            var comparer = new FloatEqualityComparer(0.001f);

            Assert.AreEqual(lensData2.Aperture, m_Device.Lens.Aperture, "Incorrect lens");

            var curve = GetCurve(clip, "", "m_Lens.m_Aperture", typeof(VirtualCameraActor));

            var expectedClipLength = 1 + Mathf.CeilToInt(5 / (float)frameRate.FrameInterval);

            Assert.AreEqual(expectedClipLength, curve.keys.Length, "Incorrect sample count");
            Assert.AreEqual(0f, curve.keys[0].time, "Incorrect time");

            // First sample will apply immediately.
            Assert.That(curve.keys[0].value, Is.EqualTo(5f).Using(comparer), "Incorrect value");
            Assert.That(curve.keys[29].value, Is.EqualTo(5f).Using(comparer), "Incorrect value");
            Assert.That(curve.keys[30].value, Is.EqualTo(10f).Using(comparer), "Incorrect value");

            // Second sample applies 1s later
            var frame = 1 + Mathf.CeilToInt(1 / (float)frameRate.FrameInterval);
            Assert.That(curve.keys[frame].value, Is.EqualTo(10f).Using(comparer), "Incorrect value");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void AutoFocusRecordsSamples()
        {
            var raycasterImpl = Substitute.For<IRaycasterFactoryImpl>();
            var raycaster = Substitute.For<IRaycaster>();

            RaycasterFactory.SetImplementation(raycasterImpl);

            raycaster.Raycast(Arg.Any<Camera>(), Arg.Any<Vector2>(), out var focusDistance)
                .ReturnsForAnyArgs(x =>
                {
                    x[2] = 5f;
                    return true;
                });

            raycasterImpl.Create().Returns(x => raycaster);

            var actor = new GameObject("actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();
            var go = new GameObject();
            var device = go.AddComponent<VirtualCameraDevice>();
            var settings = device.Settings;
            var frameRate = StandardFrameRate.FPS_30_00.ToValue();

            settings.FocusMode = FocusMode.ReticleAF;

            device.SetClient(m_Client, false);
            device.Actor = actor;
            device.Settings = settings;
            device.Processor.TimeShiftTolerance = int.MaxValue;

            Assert.True(device.IsReady(), "Incorrect device ready state.");

            // Run raycaster in frame 1, as frame 0 is set on Reset.
            device.MarkClientTime(new FrameTime(1).ToSeconds(frameRate));
            device.InvokeUpdateDevice();
            device.InvokeLiveUpdate();

            device.InvokeStartRecording();

            Assert.True(device.IsRecording, "Incorrect device recording state.");

            // Process until frame 30.
            device.MarkClientTime(new FrameTime(30).ToSeconds(frameRate));
            device.InvokeUpdateDevice();
            device.InvokeLiveUpdate();

            device.InvokeStopRecording();

            var clip = device.Recorder.Bake();

            Assert.AreEqual(5f, device.Lens.FocusDistance, "Incorrect focus distance");

            var curve = GetCurve(clip, "", "m_Lens.m_FocusDistance", typeof(VirtualCameraActor));
            var lastFrame = curve.keys[curve.keys.Length - 1];
            var comparer = new FloatEqualityComparer(0.001f);

            Assert.That(lastFrame.value, Is.EqualTo(5f).Using(comparer), "Incorrect value");

            // Frame 0 and 1 do not participate in the recording.
            var lastTime = (float)(new FrameTime(28).ToSeconds(frameRate));

            Assert.That(lastFrame.time, Is.EqualTo(lastTime).Using(comparer), "Incorrect value");

            ScriptableObject.DestroyImmediate(device);
            GameObject.DestroyImmediate(clip);
            GameObject.DestroyImmediate(actor.gameObject);
            RaycasterFactory.RestoreDefaultImplementation();
        }

        [Test]
        public void RecordsMetadata()
        {
            var takeBuilder = Substitute.For<ITakeBuilder>();

            m_Device.Lens = new Lens()
            {
                FocalLength = 47f
            };

            m_Device.InvokeUpdateDevice();
            m_Device.InvokeStartRecording();
            m_Device.InvokeStopRecording();
            m_Device.Write(takeBuilder);

            takeBuilder.Received().CreateAnimationTrack(
                Arg.Any<string>(),
                Arg.Any<Animator>(),
                Arg.Is<AnimationClip>(x => DestroyAndReturn(x)),
                Arg.Is<VirtualCameraTrackMetadata>(x =>
                    x.Channels == m_Device.Recorder.Channels
                    && x.Lens == m_Device.Lens));
        }
    }

    class VirtualCameraDeviceTestsStable : VirtualCameraDeviceTests
    {
        [Test]
        public void DefaultLensAssetIsSetByDefault()
        {
            Assert.NotNull(m_Device.m_DefaultLensAsset, "DefaultLensAsset is not set.");
        }

        [Test]
        public void DefaultLensAssetParamsAreUsedWhenLensAssetIsNull()
        {
            m_Device.LensAsset = null;

            Assert.Null(m_Device.LensAsset, "Lens Asset should not be set.");
            Assert.AreEqual(m_Device.m_DefaultLensAsset.Intrinsics, m_Device.LensIntrinsics, "DefaultLensAsset is not set.");
        }

        internal static IEnumerable LensSource
        {
            get
            {
                yield return new TestCaseData(new Lens()
                {
                    FocalLength = 301f,
                    FocusDistance = 1e12f, // Beyond infinity threshold.
                    Aperture = 100f
                }, new Lens()
                {
                    FocalLength = 300f,
                    FocusDistance = LensLimits.FocusDistance.y,
                    Aperture = 22f
                }).SetName("Clamp High");

                yield return new TestCaseData(new Lens()
                {
                    FocalLength = 0f,
                    FocusDistance = 0f,
                    Aperture = 0f
                }, new Lens()
                {
                    FocalLength = 14f,
                    FocusDistance = 0.3f,
                    Aperture = 1f
                }).SetName("Clamp Low");
            }
        }

        [Test, TestCaseSource(nameof(LensSource))]
        public void SetLensValidates(Lens input, Lens expected)
        {
            var comparer = new FloatEqualityComparer(0.001f);

            m_Device.Lens = input;

            var output = m_Device.Lens;

            Assert.That(output.FocalLength, Is.EqualTo(expected.FocalLength).Using(comparer), "Incorrect focal length.");
            Assert.That(output.FocusDistance, Is.EqualTo(expected.FocusDistance).Using(comparer), "Incorrect focus distance.");
            Assert.That(output.Aperture, Is.EqualTo(expected.Aperture).Using(comparer), "Incorrect aperture.");
        }

        [Test]
        public void ClientReceivesUpdatedLensParameters()
        {
            m_Device.UpdateClient();
            m_Device.InvokeUpdateDevice();

            // Make sure the client received the proper parameters.
            m_Client.Received().SendLens(Arg.Any<Lens>());
            m_Client.Received().SendLensKitDescriptor(Arg.Any<LensKitDescriptor>());
        }

        [Test]
        public void SetClientSendsCameraLens()
        {
            m_Client.Received(1).SendLens(Arg.Any<Lens>());
        }

        [Test]
        public void SetClientSendsCameraBody()
        {
            m_Client.Received(1).SendCameraBody(Arg.Any<CameraBody>());
        }

        [Test]
        public void SetClientSendsSettings()
        {
            var settings = m_Device.Settings;
            m_Client.Received(1).SendSettings(settings);
        }

        [Test]
        public void SetClientSendsVideoStreamState()
        {
            m_Client.Received(1).SendVideoStreamState(Arg.Any<VideoStreamState>());
        }

        [Test]
        public void ReceiveSettingsWorks()
        {
            var settings = Settings.DefaultData;
            settings.Damping = new Damping
            {
                Enabled = true,
                Body = new Vector3(0.5f, 0.5f, 1),
                Aim = 0.5f
            };
            settings.PositionLock = PositionAxis.Dolly;
            settings.Rebasing = true;
            settings.MotionScale = Vector3.zero;
            settings.Validate();

            m_Client.DampingEnabledReceived += Raise.Event<Action<bool>>(settings.Damping.Enabled);
            m_Client.BodyDampingReceived += Raise.Event<Action<Vector3>>(settings.Damping.Body);
            m_Client.AimDampingReceived += Raise.Event<Action<float>>(settings.Damping.Aim);
            m_Client.PositionLockReceived += Raise.Event<Action<PositionAxis>>(settings.PositionLock);
            m_Client.RebasingReceived += Raise.Event<Action<bool>>(settings.Rebasing);
            m_Client.MotionScaleReceived += Raise.Event<Action<Vector3>>(settings.MotionScale);

            Assert.AreEqual(settings, m_Device.Settings, "Incorrect state");
        }

        [Test]
        public void ReceiveCommandSetReticlePositionUsesFallbackDistance()
        {
            var lensData = new FocusDistanceSample
            {
                Time = 0.01,
                FocusDistance = -100f,
            };
            var state = new Settings
            {
                ReticlePosition = new Vector2(0.1f, 0.6f)
            };

            m_Client.FocusDistanceSampleReceived += Raise.Event<Action<FocusDistanceSample>>(lensData);
            m_Client.FocusReticlePositionReceived += Raise.Event<Action<Vector2>>(state.ReticlePosition);

            m_Device.InvokeUpdateDevice();

            m_Client.FocusReticlePositionReceived += Raise.Event<Action<Vector2>>(Vector2.one);

            Assert.AreEqual(Vector2.one, m_Device.Settings.ReticlePosition, "Incorrect reticle position");
            Assert.AreEqual(m_Device.LensIntrinsics.CloseFocusDistance, m_Device.Lens.FocusDistance, "Incorrect focus distance");
        }

        [Test]
        public void OriginStaysTheSameWhenClientDisconnects()
        {
            var actor = new GameObject("actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();
            var initialPosition = Vector3.zero;
            var initialRotation = Quaternion.identity;
            var positionFromClient = new Vector3(10, 15, 4);
            var rotationFromClient = Quaternion.Euler(new Vector3(170, 10, 80));

            // Set the Pose inside m_Device to make sure the value is not the default
            var serializedObject = new SerializedObject(m_Device);
            var serializedPropertyPosePosition = serializedObject.FindProperty("m_Rig.Pose.position");
            var serializedPropertyPoseRotation = serializedObject.FindProperty("m_Rig.Pose.rotation");
            serializedPropertyPosePosition.vector3Value = initialPosition;
            serializedPropertyPoseRotation.quaternionValue = initialRotation;
            serializedObject.ApplyModifiedProperties();

            m_Device.SetClient(m_Client, false);

            // Needs to be initialized once since the first frame is used for setting the initial pose
            var inputSample = new InputSample()
            {
                Time = 0d,
                ARPose = new Pose(Vector3.zero, Quaternion.identity),
            };
            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample);

            m_Device.InvokeLiveUpdate();

            var lastRecordedOrigin = m_Device.Origin;

            // Update the pose to see if the origin stays the same
            var inputSample2 = new InputSample()
            {
                Time = 1d,
                ARPose = new Pose(positionFromClient, rotationFromClient),
            };
            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample2);

            m_Device.InvokeLiveUpdate();

            var comparer = new Vector3EqualityComparer(0.001f);

            // Value needs to be rounded to avoid the test failing because of float precision
            Assert.That(positionFromClient, Is.EqualTo(m_Device.Pose.position).Using(comparer), "Position was not updated on receive");
            Assert.That(rotationFromClient.eulerAngles, Is.EqualTo(m_Device.Pose.rotation.eulerAngles).Using(comparer), "Rotation was not updated on receive");
            Assert.AreEqual(lastRecordedOrigin, m_Device.Origin, "Origin changed while it should stay the same when the local pose is updated");

            m_Device.SetClient(null, false);

            // when client is disconnected values should stay the same
            Assert.That(positionFromClient, Is.EqualTo(m_Device.Pose.position).Using(comparer), "Pose position was reseted when client was disconnected");
            Assert.That(rotationFromClient.eulerAngles, Is.EqualTo(m_Device.Pose.rotation.eulerAngles).Using(comparer), "Pose rotation was reseted when client was disconnected");
            Assert.That(lastRecordedOrigin, Is.EqualTo(m_Device.Origin).Using(comparer), "Origin changed while it should stay the same when the server disconnects");
            serializedObject.Dispose();
        }

        [Test]
        public void RequestAlignWithActor()
        {
            var comparer = new Vector3EqualityComparer(0.001f);

            m_Device.InvokeUpdateDevice();

            // Assign new actor
            var actor = new GameObject("actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();
            actor.transform.position = new Vector3(8, 9, 10);
            actor.transform.localEulerAngles = new Vector3(15, 20, 25);
            m_Device.Actor = actor;

            // Check that pose and origin are aligned to new actor
            Assert.That(m_Device.Pose.position, Is.EqualTo(actor.transform.localPosition).Using(comparer), "Device pose position not aligned with new actor");
            Assert.That(m_Device.Pose.rotation.eulerAngles, Is.EqualTo(actor.transform.localEulerAngles).Using(comparer), "Device pose rotation not aligned with new actor");
            Assert.That(m_Device.Origin.position, Is.EqualTo(actor.transform.localPosition).Using(comparer), "Device origin position not aligned with new actor");

            // The origin discards rotation around the X and Z axis
            Assert.That(m_Device.Origin.rotation.eulerAngles, Is.EqualTo(new Vector3(0, actor.transform.localEulerAngles.y, 0)).Using(comparer), "Device origin pan not aligned with new actor");

            // Modify actor transform, request alignment, and update device to perform the alignment
            actor.transform.position = new Vector3(11, 12, 13);
            actor.transform.localEulerAngles = new Vector3(30, 35, 40);
            m_Device.RequestAlignWithActor();
            m_Device.InvokeUpdateDevice();

            // Check that pose and origin are aligned to new transform
            Assert.That(m_Device.Pose.position, Is.EqualTo(actor.transform.localPosition).Using(comparer), "Device pose position not aligned with actor");
            Assert.That(m_Device.Pose.rotation.eulerAngles, Is.EqualTo(actor.transform.localEulerAngles).Using(comparer), "Device pose rotation not aligned with actor");
            Assert.That(m_Device.Origin.position, Is.EqualTo(actor.transform.localPosition).Using(comparer), "Device origin position not aligned with actor");

            // The origin discards rotation around the X and Z axis
            Assert.That(m_Device.Origin.rotation.eulerAngles, Is.EqualTo(new Vector3(0, actor.transform.localEulerAngles.y, 0)).Using(comparer), "Device origin pan not aligned with actor");

            GameObject.DestroyImmediate(actor.gameObject);
        }

        [Test]
        public void LensIntrinsicsChangeExternalyUpdatesTheClient()
        {
            var lensAsset = ScriptableObject.CreateInstance<LensAsset>();
            var intrinsics = default(LensIntrinsics);

            m_Device.LensAsset = lensAsset;

            m_Device.InvokeUpdateDevice();

            m_Client.Received(2).SendLensKitDescriptor(Arg.Any<LensKitDescriptor>());

            intrinsics.FocalLengthRange = new Vector2(1f, 101f);
            lensAsset.Intrinsics = intrinsics;

            m_Device.InvokeUpdateDevice();

            m_Client.Received(3).SendLensKitDescriptor(Arg.Any<LensKitDescriptor>());

            ScriptableObject.DestroyImmediate(lensAsset);
        }

        [Test]
        public void StartTimecodeNotSet()
        {
            var actor = new GameObject("actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();

            var lensData = new ApertureSample
            {
                Time = 1.0,
                Aperture = 5f,
            };

            m_Device.IsSynchronized = false;

            m_Device.MarkClientTime(1.0);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeStartRecording();

            m_Client.ApertureSampleReceived += Raise.Event<Action<ApertureSample>>(lensData);

            m_Device.MarkClientTime(6.0);
            m_Device.InvokeUpdateDevice();
            m_Device.InvokeStopRecording();

            m_Device.Write(m_TakeBuilder);

            m_TakeBuilder.Received(1).CreateAnimationTrack(
                Arg.Any<string>(),
                Arg.Any<Animator>(),
                Arg.Any<AnimationClip>(),
                Arg.Any<ITrackMetadata>(),
                Arg.Is(default(double?))
            );
        }

        [Test]
        public void StartTimecodeSet()
        {
            var timestamp = 1d;
            var inputSample = new InputSample()
            {
                Time = timestamp,
                ARPose = new Pose(Vector3.zero, Quaternion.identity)
            };
            var lensData = new ApertureSample()
            {
                Time = timestamp,
                Aperture = 5f,
            };

            var frameRate = TakeRecorder.FrameRate;
            m_Device.IsSynchronized = true;

            m_Device.InvokeStartRecording();

            m_Client.InputSampleReceived += Raise.Event<Action<InputSample>>(inputSample);
            m_Client.ApertureSampleReceived += Raise.Event<Action<ApertureSample>>(lensData);

            m_Device.PresentAt(new FrameTimeWithRate(frameRate, FrameTime.FromSeconds(frameRate, timestamp)));
            m_Device.InvokeLiveUpdate();

            m_Device.InvokeStopRecording();

            m_Device.Write(m_TakeBuilder);

            m_TakeBuilder.Received(1).CreateAnimationTrack(
                Arg.Any<string>(),
                Arg.Any<Animator>(),
                Arg.Any<AnimationClip>(),
                Arg.Any<ITrackMetadata>(),
                Arg.Is(timestamp)
            );
        }
    }
}
