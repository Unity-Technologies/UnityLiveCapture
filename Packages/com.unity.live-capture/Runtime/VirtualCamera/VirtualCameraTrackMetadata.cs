using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class VirtualCameraTrackMetadata : ITrackMetadata
    {
        [SerializeField, EnumFlagButtonGroup(100f)]
        VirtualCameraChannelFlags m_Channels;
        [SerializeField]
        Lens m_Lens = Lens.DefaultParams;
        [SerializeField]
        CameraBody m_CameraBody = CameraBody.DefaultParams;
        [SerializeField]
        LensAsset m_LensAsset;
        [SerializeField, AspectRatio]
        float m_CropAspect;

        public VirtualCameraChannelFlags Channels
        {
            get => m_Channels;
            set => m_Channels = value;
        }

        public Lens Lens
        {
            get => m_Lens;
            set => m_Lens = value;
        }

        public CameraBody CameraBody
        {
            get => m_CameraBody;
            set => m_CameraBody = value;
        }

        public LensAsset LensAsset
        {
            get => m_LensAsset;
            set => m_LensAsset = value;
        }

        public float CropAspect
        {
            get => m_CropAspect;
            set => m_CropAspect = value;
        }
    }
}
