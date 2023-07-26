using System;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [Serializable]
    class VcamTrackMetadataListDescriptorV0
    {
        [SerializeField]
        VcamTrackMetadataDescriptorV0[] m_VcamTrackMetadataDescriptors;

        public static explicit operator VcamTrackMetadataListDescriptorV0(VcamTrackMetadataListDescriptor list)
        {
            return new VcamTrackMetadataListDescriptorV0
            {
                m_VcamTrackMetadataDescriptors = list.Descriptors.Select(x => (VcamTrackMetadataDescriptorV0)x).ToArray()
            };
        }

        public static explicit operator VcamTrackMetadataListDescriptor(VcamTrackMetadataListDescriptorV0 descriptor)
        {
            return new VcamTrackMetadataListDescriptor(descriptor.m_VcamTrackMetadataDescriptors
                .Select(x => (VcamTrackMetadataDescriptor)x));
        }
    }
}
