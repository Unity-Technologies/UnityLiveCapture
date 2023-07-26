using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [Serializable]
    class VcamTrackMetadataDescriptorV0
    {
        [SerializeField]
        SerializableGuid m_TakeGuid;
        [SerializeField]
        float m_FocalLength;
        [SerializeField]
        float m_FocusDistance;
        [SerializeField]
        float m_Aperture;
        [SerializeField]
        Vector2 m_SensorSize;
        [SerializeField]
        string m_SensorPresetName;
        [SerializeField]
        float m_Iso;
        [SerializeField]
        float m_ShutterSpeed;
        [SerializeField]
        float m_AspectRatio;

        public static explicit operator VcamTrackMetadataDescriptorV0(VcamTrackMetadataDescriptor descriptor)
        {
            return new VcamTrackMetadataDescriptorV0
            {
                m_TakeGuid = descriptor.TakeGuid,
                m_FocalLength = descriptor.FocalLength,
                m_FocusDistance = descriptor.FocusDistance,
                m_Aperture = descriptor.Aperture,
                m_SensorSize = descriptor.SensorSize,
                m_SensorPresetName = descriptor.SensorPresetName,
                m_Iso = descriptor.Iso,
                m_ShutterSpeed = descriptor.ShutterSpeed,
                m_AspectRatio = descriptor.AspectRatio
            };
        }

        public static explicit operator VcamTrackMetadataDescriptor(VcamTrackMetadataDescriptorV0 descriptor)
        {
            return new VcamTrackMetadataDescriptor
            {
                TakeGuid = descriptor.m_TakeGuid,
                FocalLength = descriptor.m_FocalLength,
                FocusDistance = descriptor.m_FocusDistance,
                Aperture = descriptor.m_Aperture,
                SensorSize = descriptor.m_SensorSize,
                SensorPresetName = descriptor.m_SensorPresetName,
                Iso = descriptor.m_Iso,
                ShutterSpeed = descriptor.m_ShutterSpeed,
                AspectRatio = descriptor.m_AspectRatio
            };
        }
    }
}
