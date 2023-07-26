using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class VcamTrackMetadataDescriptor
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

        public Guid TakeGuid
        {
            get => m_TakeGuid;
            set => m_TakeGuid = value;
        }

        public float FocalLength
        {
            get => m_FocalLength;
            set => m_FocalLength = value;
        }

        public float FocusDistance
        {
            get => m_FocusDistance;
            set => m_FocusDistance = value;
        }

        public float Aperture
        {
            get => m_Aperture;
            set => m_Aperture = value;
        }

        public Vector2 SensorSize
        {
            get => m_SensorSize;
            set => m_SensorSize = value;
        }

        public string SensorPresetName
        {
            get => m_SensorPresetName;
            set => m_SensorPresetName = value;
        }

        public float Iso
        {
            get => m_Iso;
            set => m_Iso = value;
        }

        public float ShutterSpeed
        {
            get => m_ShutterSpeed;
            set => m_ShutterSpeed = value;
        }

        public float AspectRatio
        {
            get => m_AspectRatio;
            set => m_AspectRatio = value;
        }

        internal static VcamTrackMetadataDescriptor Create(Take take, VirtualCameraTrackMetadata metadata)
        {
            var descriptor = new VcamTrackMetadataDescriptor();
#if UNITY_EDITOR
            descriptor.TakeGuid = SerializableGuid.FromString(AssetDatabaseUtility.GetAssetGUID(take));
            descriptor.FocalLength = metadata.Lens.FocalLength;
            descriptor.FocusDistance = metadata.Lens.FocusDistance;
            descriptor.Aperture = metadata.Lens.Aperture;
            descriptor.SensorSize = metadata.CameraBody.SensorSize;
            descriptor.SensorPresetName = SensorPresetsCacheProxy.GetSensorSizeName(metadata.CameraBody.SensorSize);
            descriptor.Iso = metadata.CameraBody.Iso;
            descriptor.ShutterSpeed = metadata.CameraBody.ShutterSpeed;
            descriptor.AspectRatio = metadata.CropAspect;
#endif
            return descriptor;
        }
    }
}
