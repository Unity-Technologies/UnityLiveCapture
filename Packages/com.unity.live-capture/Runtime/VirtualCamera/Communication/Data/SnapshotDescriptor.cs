using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [Serializable]
    class SnapshotDescriptorV0
    {
        [SerializeField]
        Pose m_Pose;
        [SerializeField]
        LensAssetDescriptorV0 m_LensAssetDescriptor;
        [SerializeField]
        float m_FocalLength;
        [SerializeField]
        float m_FocusDistance;
        [SerializeField]
        float m_Aperture;
        [SerializeField]
        Vector2 m_SensorSize;
        [SerializeField]
        int m_Iso;
        [SerializeField]
        float m_ShutterSpeed;
        [SerializeField]
        SerializableGuid m_Screenshot;
        [SerializeField]
        bool m_IsSlateValid;
        [SerializeField]
        int m_SceneNumber;
        [SerializeField]
        string m_ShotName;
        [SerializeField]
        FrameRate m_FrameRate;
        [SerializeField]
        double m_Time;

        public static explicit operator SnapshotDescriptorV0(SnapshotDescriptor snapshot)
        {
            return new SnapshotDescriptorV0
            {
                m_Pose = snapshot.Pose,
                m_LensAssetDescriptor = (LensAssetDescriptorV0)snapshot.LensAsset,
                m_FocalLength = snapshot.Lens.FocalLength,
                m_FocusDistance = snapshot.Lens.FocusDistance,
                m_Aperture = snapshot.Lens.Aperture,
                m_SensorSize = snapshot.CameraBody.SensorSize,
                m_Iso = snapshot.CameraBody.Iso,
                m_ShutterSpeed = snapshot.CameraBody.ShutterSpeed,
                m_Screenshot = snapshot.Screenshot,
                m_IsSlateValid = snapshot.IsShotValid,
                m_SceneNumber = snapshot.SceneNumber,
                m_ShotName = snapshot.ShotName,
                m_FrameRate = snapshot.FrameRate,
                m_Time = snapshot.Time,
            };
        }

        public static explicit operator SnapshotDescriptor(SnapshotDescriptorV0 snapshot)
        {
            return new SnapshotDescriptor
            {
                Pose = snapshot.m_Pose,
                LensAsset = (LensAssetDescriptor)snapshot.m_LensAssetDescriptor,
                Lens = new Lens
                {
                    FocalLength = snapshot.m_FocalLength,
                    FocusDistance = snapshot.m_FocusDistance,
                    Aperture = snapshot.m_Aperture,
                },
                CameraBody = new CameraBody
                {
                    SensorSize = snapshot.m_SensorSize,
                    Iso = snapshot.m_Iso,
                    ShutterSpeed = snapshot.m_ShutterSpeed,
                },
                Screenshot = snapshot.m_Screenshot,
                IsShotValid = snapshot.m_IsSlateValid,
                SceneNumber = snapshot.m_SceneNumber,
                ShotName = snapshot.m_ShotName,
                FrameRate = snapshot.m_FrameRate,
                Time = snapshot.m_Time,
            };
        }
    }
}
