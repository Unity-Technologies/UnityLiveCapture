using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using Unity.LiveCapture.ARKitFaceCapture;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class FaceTracker : IFaceTracker, IInitializable, IDisposable, ITickable
    {
        [Inject]
        IARFaceManagerSystem m_FaceManager;
        [Inject]
        ISettings m_Settings;
        List<ARFace> m_Faces = new List<ARFace>();
        bool m_IsTracking;
        bool m_NewPoseAvailable;
        FaceBlendShapePose m_BlendShapes;
        Vector3 m_HeadPosition;
        Quaternion m_HeadOrientation;
        Quaternion m_LeftEyeOrientation;
        Quaternion m_RightEyeOrientation;
        FacePose m_CalibrationPose = FacePose.Identity;
        public bool HasCalibrationPose { get; private set; }

        public bool IsTracking => m_IsTracking;

        public FacePose CalibrationPose => m_CalibrationPose;

        public void SetCalibrationPose(FacePose pose)
        {
            HasCalibrationPose = true;
            m_CalibrationPose = pose;
        }

        public void ClearCalibrationPose()
        {
            HasCalibrationPose = false;
            m_CalibrationPose = FacePose.Identity;
        }

        public void Initialize()
        {
            m_FaceManager.FaceAdded += FaceAdded;
            m_FaceManager.FaceUpdated += FaceUpdated;
            m_FaceManager.FaceRemoved += FaceRemoved;

            m_FaceManager.RequestWorldAlignment(ARWorldAlignment.Camera);
        }

        public void Dispose()
        {
            m_FaceManager.FaceAdded -= FaceAdded;
            m_FaceManager.FaceUpdated -= FaceUpdated;
            m_FaceManager.FaceRemoved -= FaceRemoved;
        }

        public void Tick()
        {
            m_IsTracking = false;

            if (TryGetFirstTrackableFace(out var face))
            {
                m_IsTracking = true;
            }
        }

        public bool ConsumeFacePose(out FacePose facePose, bool applyCalibration = true)
        {
            var newPose = m_NewPoseAvailable;

            m_NewPoseAvailable = false;

            facePose = new FacePose()
            {
                BlendShapes = m_BlendShapes,
                HeadPosition = m_HeadPosition,
                HeadOrientation = m_HeadOrientation,
                LeftEyeOrientation = m_LeftEyeOrientation,
                RightEyeOrientation = m_RightEyeOrientation
            };

            if (applyCalibration && HasCalibrationPose)
            {
                ApplyCalibration(ref facePose, ref m_CalibrationPose);
            }

            return newPose;
        }

        void FaceAdded(ARFace face)
        {
            m_Faces.Add(face);

            UpdateFacePose();
        }

        void FaceUpdated(ARFace face)
        {
            Debug.Assert(m_Faces.Count > 0);

            if (TryGetFirstTrackableFace(out var firstTrackableFace)
                && firstTrackableFace.trackableId.Equals(face.trackableId))
            {
                UpdateFacePose();
            }
        }

        void FaceRemoved(ARFace face)
        {
            Debug.Assert(m_Faces.Contains(face), "The face tracking system has unregistered " +
                "a face that was never registered to the Face Capture Client");

            m_Faces.Remove(face);

            UpdateFacePose();
        }

        bool TryGetFirstTrackableFace(out ARFace face)
        {
            if (ARSession.state > ARSessionState.Ready)
            {
                foreach (var f in m_Faces)
                {
                    if (f.trackingState == TrackingState.Tracking)
                    {
                        face = f;

                        return true;
                    }
                }
            }

            face = default(ARFace);

            return false;
        }

        void UpdateFacePose()
        {
            if (TryGetFirstTrackableFace(out var face))
            {
                UpdateHeadPose(face);
                UpdateBlendshapes(face);
                UpdateEyeOrientations(face);

                m_NewPoseAvailable = true;
            }
        }

        void UpdateHeadPose(ARFace face)
        {
            var portraitRotationCompensate = Quaternion.Euler(0f, 0f, -90);
            var anchorTransform = face.transform;
            var position = anchorTransform.localPosition;

            position.z *= -1f;

            if (m_Settings.FlipHorizontally)
                position.x *= -1f;

            m_HeadPosition = portraitRotationCompensate * position;
            m_HeadOrientation =
                ConvertToUnityQuaternion(portraitRotationCompensate * anchorTransform.localRotation);
        }

        void UpdateBlendshapes(ARFace face)
        {
            using var blendShapeCoefficients = m_FaceManager.GetBlendShapeCoefficients(face, Allocator.Temp);
            foreach (var featureCoefficient in blendShapeCoefficients)
            {
                var location = (int)featureCoefficient.blendShapeLocation;
                m_BlendShapes[location] = featureCoefficient.coefficient;
            }

            // ARKit's default blend shapes are set so that "right" would be the right side when viewing
            // from the front.
            if (!m_Settings.FlipHorizontally)
            {
                m_BlendShapes.FlipHorizontally();
            }
        }

        void UpdateEyeOrientations(ARFace face)
        {
            m_LeftEyeOrientation = ConvertToUnityQuaternion(face.leftEye.localRotation);
            m_RightEyeOrientation = ConvertToUnityQuaternion(face.rightEye.localRotation);
        }

        void ApplyCalibration(ref FacePose pose, ref FacePose calibrationPose)
        {
            var calibrationRotation = Quaternion.Inverse(calibrationPose.HeadOrientation);
            pose.HeadOrientation = calibrationRotation * pose.HeadOrientation;

            for (var i = 0; i < FaceBlendShapePose.ShapeCount; i++)
            {
                var rawCoefficient = pose.BlendShapes[i];
                var calibratedBaseCoefficient = calibrationPose.BlendShapes[i];

               var calibratedCoefficient = Mathf.InverseLerp(calibratedBaseCoefficient, 1f, rawCoefficient);

               pose.BlendShapes[i] = calibratedCoefficient;
            }
        }

        Quaternion ConvertToUnityQuaternion(Quaternion quat)
        {
            return m_Settings.FlipHorizontally
                ? new Quaternion(-quat.x, quat.y, -quat.z, quat.w)
                : new Quaternion(-quat.x, -quat.y, quat.z, quat.w);
        }
    }
}
