using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.VirtualCamera;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
#if SRP_CORE_14_0_OR_NEWER
using UnityEngine.Rendering;
#endif
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Unity.LiveCapture.Tests.Editor
{
    abstract class BaseCameraDriverTests
    {
        protected GameObject m_GameObject;
        protected VirtualCameraActor m_Actor;
        protected VirtualCameraDevice m_VirtualCameraDevice;
        protected GameObject m_CubeFacingCamera;
#if SRP_CORE_14_0_OR_NEWER
        VolumeProfile m_SharedProfile;
#endif

        [SetUp]
        public void Setup()
        {
            m_GameObject = CreateVirtualCameraActor();
#if SRP_CORE_14_0_OR_NEWER
            m_SharedProfile = Utility.AssignVolumeSharedProfileIfNeeded(m_GameObject);
#endif
            m_GameObject.hideFlags = HideFlags.DontSave;
            m_Actor = m_GameObject.GetComponent<VirtualCameraActor>();
            Assert.IsNotNull(m_Actor, $"Failed to fetch {nameof(VirtualCameraActor)} component.");
            var go = new GameObject();
            m_VirtualCameraDevice = go.AddComponent<VirtualCameraDevice>();
            m_VirtualCameraDevice.Actor = m_Actor;
            m_VirtualCameraDevice.SetClient(Substitute.For<IVirtualCameraClientInternal>(), false);
            m_VirtualCameraDevice.Processor.Reset();
            m_VirtualCameraDevice.Processor.TimeShiftTolerance = int.MaxValue;

            m_CubeFacingCamera = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Put the cube in front of the camera, so that it'll be hit when raycasting.
            m_CubeFacingCamera.transform.position = m_GameObject.transform.position + m_GameObject.transform.forward * 10;
        }

        protected abstract GameObject CreateVirtualCameraActor();

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_VirtualCameraDevice.gameObject);
            Object.DestroyImmediate(m_GameObject);
            Object.DestroyImmediate(m_CubeFacingCamera);
#if SRP_CORE_14_0_OR_NEWER
            Object.DestroyImmediate(m_SharedProfile);
#endif
        }

        [UnityTest]
        public IEnumerator CanAddPhysicalDriverToEmptyGameObject()
        {
            var enumerator = CanAddComponentToEmptyGameObject<PhysicalCameraDriver>();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

#if CINEMACHINE_2_4_OR_NEWER
        [UnityTest]
        public IEnumerator CanAddCinemachineDriverToEmptyGameObject()
        {
            var enumerator = CanAddComponentToEmptyGameObject<CinemachineCameraDriver>();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

#endif

        static IEnumerator CanAddComponentToEmptyGameObject<T>() where T : Component
        {
            var gameObject = new GameObject($"Test {typeof(T).Name}");
            gameObject.AddComponent<T>();
            yield return null;
            Object.DestroyImmediate(gameObject);
        }

        static float WithinRelativeMargin(float value, float percent)
        {
            return value * (1 + percent * Random.Range(-1, 1));
        }

        static float WithinRange(Vector2 range)
        {
            return Mathf.Lerp(range.x, range.y, Random.value);
        }

        static Vector2 RangeWithinRange(Vector2 range)
        {
            var a = WithinRange(range);
            var b = WithinRange(range);
            return new Vector2(
                Mathf.Min(a, b),
                Mathf.Max(a, b));
        }

        static void PseudoRandomizedLens(int seed, out Lens lens, out LensIntrinsics intrinsics)
        {
            Random.InitState(seed);

            var focalLengthRange = RangeWithinRange(LensIntrinsics.DefaultParams.FocalLengthRange);
            var focusDistanceRange = RangeWithinRange(LensLimits.FocusDistance);
            var apertureRange = RangeWithinRange(LensIntrinsics.DefaultParams.ApertureRange);

            lens = new Lens
            {
                FocalLength = WithinRange(focalLengthRange),
                FocusDistance = WithinRange(focusDistanceRange),
                Aperture = WithinRange(apertureRange),
            };
            intrinsics = new LensIntrinsics
            {
                FocalLengthRange = focalLengthRange,
                CloseFocusDistance = focusDistanceRange.x,
                ApertureRange = apertureRange,
                LensShift = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)),
                BladeCount = Mathf.FloorToInt(Random.Range(2, 8)),
                Curvature = apertureRange,
                // The 0.2f value means that we'll randomly pick a value within a 20% margin of the original one.
                BarrelClipping = WithinRelativeMargin(LensIntrinsics.DefaultParams.BarrelClipping, 0.2f),
                Anamorphism = Random.Range(-1, 1)
            };
        }

        static CameraBody PseudoRandomizedCameraBody(int seed)
        {
            Random.InitState(seed);

            return new CameraBody
            {
                SensorSize = new Vector2(
                    WithinRelativeMargin(CameraBody.DefaultParams.SensorSize.x, 0.2f),
                    WithinRelativeMargin(CameraBody.DefaultParams.SensorSize.y, 0.2f)),
                Iso = Mathf.FloorToInt(WithinRelativeMargin(CameraBody.DefaultParams.Iso, 0.2f)),
                ShutterSpeed = WithinRelativeMargin(CameraBody.DefaultParams.ShutterSpeed, 0.2f)
            };
        }

        protected static IEnumerable<Tuple<Lens, LensIntrinsics, CameraBody>> LensAndBodyDataSource()
        {
            for (var i = 0; i != 5; ++i)
            {
                PseudoRandomizedLens(i, out var lens, out var instrinsics);

                yield return new Tuple<Lens, LensIntrinsics, CameraBody>(
                    lens, instrinsics, PseudoRandomizedCameraBody(i));
            }
        }

#if URP_14_0_OR_NEWER || HDRP_14_0_OR_NEWER
        protected IEnumerator TestFocusModeSynchronizationInternal(VolumeComponent depthOfField)
        {
            var virtualCameraDevice = m_VirtualCameraDevice;

            SetFocusMode(virtualCameraDevice, FocusMode.Clear);
            yield return Update();
            Assert.IsFalse(depthOfField.active);

            SetFocusMode(virtualCameraDevice, FocusMode.Manual);
            yield return Update();
            Assert.IsTrue(depthOfField.active);

            SetFocusMode(virtualCameraDevice, FocusMode.Clear);
            yield return Update();
            Assert.IsFalse(depthOfField.active);

            m_CubeFacingCamera.SetActive(false);

            SetFocusMode(m_VirtualCameraDevice, FocusMode.ReticleAF);
            yield return Update();
            Assert.IsFalse(depthOfField.active);

            m_CubeFacingCamera.SetActive(true);

            SetFocusMode(m_VirtualCameraDevice, FocusMode.ReticleAF);
            SetReticlePosition(m_VirtualCameraDevice, Vector2.one * .5f);
            yield return Update();
            Assert.IsTrue(depthOfField.active);

            m_CubeFacingCamera.SetActive(false);

            SetReticlePosition(m_VirtualCameraDevice, Vector2.zero);
            SetFocusMode(m_VirtualCameraDevice, FocusMode.TrackingAF);
            yield return Update();
            Assert.IsFalse(depthOfField.active);

            m_CubeFacingCamera.SetActive(true);

            SetFocusMode(m_VirtualCameraDevice, FocusMode.TrackingAF);
            SetReticlePosition(m_VirtualCameraDevice, Vector2.one * .5f);
            yield return Update();
            Assert.IsTrue(depthOfField.active);
        }

#endif

        [UnityTest]
        public IEnumerator TestLensAndBodySynchronization([ValueSource(nameof(LensAndBodyDataSource))] Tuple<Lens, LensIntrinsics, CameraBody> input)
        {
            // Make sure the device doesn't interfere as we are testing the driver only.
            m_VirtualCameraDevice.Actor = null;

            var lens = input.Item1;
            var lensIntrinsics = input.Item2;
            var cameraBody = input.Item3;
            SetLensAndCameraBody(m_Actor, lens, lensIntrinsics, cameraBody);

            // Wait one frame, the driver is expected to pull the lens form the actor and apply it.
            yield return Update();

            ValidateLensAndBodySynchronization(lens, lensIntrinsics, cameraBody);
        }

        static void SetLensAndCameraBody(VirtualCameraActor actor, Lens lens, LensIntrinsics intrinsics, CameraBody body)
        {
            var serializedObject = new SerializedObject(actor);

            var lensProp = serializedObject.FindProperty("m_Lens");
            var lensIntrinsicsProp = serializedObject.FindProperty("m_LensIntrinsics");

            lensProp.FindPropertyRelative("m_FocalLength").floatValue = lens.FocalLength;
            lensIntrinsicsProp.FindPropertyRelative("m_FocalLengthRange").vector2Value = intrinsics.FocalLengthRange;
            lensProp.FindPropertyRelative("m_FocusDistance").floatValue = lens.FocusDistance;
            lensIntrinsicsProp.FindPropertyRelative("m_CloseFocusDistance").floatValue = intrinsics.CloseFocusDistance;
            lensProp.FindPropertyRelative("m_Aperture").floatValue = lens.Aperture;
            lensIntrinsicsProp.FindPropertyRelative("m_ApertureRange").vector2Value = intrinsics.ApertureRange;
            lensIntrinsicsProp.FindPropertyRelative("m_LensShift").vector2Value = intrinsics.LensShift;
            lensIntrinsicsProp.FindPropertyRelative("m_BladeCount").intValue = intrinsics.BladeCount;
            lensIntrinsicsProp.FindPropertyRelative("m_Curvature").vector2Value = intrinsics.Curvature;
            lensIntrinsicsProp.FindPropertyRelative("m_BarrelClipping").floatValue = intrinsics.BarrelClipping;
            lensIntrinsicsProp.FindPropertyRelative("m_Anamorphism").floatValue = intrinsics.Anamorphism;

            var bodyProp = serializedObject.FindProperty("m_CameraBody");

            bodyProp.FindPropertyRelative("m_SensorSize").vector2Value = body.SensorSize;
            bodyProp.FindPropertyRelative("m_Iso").intValue = body.Iso;
            bodyProp.FindPropertyRelative("m_ShutterSpeed").floatValue = body.ShutterSpeed;

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract void ValidateLensAndBodySynchronization(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody);

        protected static void ValidateCameraSynchronization(Camera camera, Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
            Assert.IsTrue(Mathf.Approximately(camera.sensorSize.x, cameraBody.SensorSize.x));
            Assert.IsTrue(Mathf.Approximately(camera.sensorSize.y, cameraBody.SensorSize.y));
            Assert.IsTrue(Mathf.Approximately(camera.focalLength, lens.FocalLength));
        }

        protected static void SetFocusMode(VirtualCameraDevice device, FocusMode focusMode)
        {
            var setFocusModeMethod = typeof(VirtualCameraDevice).GetMethod(
                "SetFocusMode", BindingFlags.NonPublic | BindingFlags.Instance);
            setFocusModeMethod.Invoke(device, new object[] { focusMode });
        }

        protected static void SetReticlePosition(VirtualCameraDevice device, Vector2 reticlePosition)
        {
            var setFocusModeMethod = typeof(VirtualCameraDevice).GetMethod(
                "SetReticlePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            setFocusModeMethod.Invoke(device, new object[] { reticlePosition });
        }

        protected IEnumerator Update()
        {
            m_VirtualCameraDevice.InvokeUpdateDevice();

            if (m_VirtualCameraDevice.Actor != null)
            {
                m_VirtualCameraDevice.InvokeLiveUpdate();
            }

            EditorApplication.QueuePlayerLoopUpdate();
            yield return null;
        }
    }
}
