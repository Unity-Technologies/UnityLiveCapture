using System;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [Serializable]
    class SnapshotListDescriptorV0
    {
        [SerializeField]
        SnapshotDescriptorV0[] m_Snapshots;

        public static explicit operator SnapshotListDescriptorV0(SnapshotListDescriptor snapshots)
        {
            return new SnapshotListDescriptorV0
            {
                m_Snapshots = snapshots.Snapshots.Select(snapshot => (SnapshotDescriptorV0)snapshot).ToArray(),
            };
        }

        public static explicit operator SnapshotListDescriptor(SnapshotListDescriptorV0 snapshots)
        {
            return new SnapshotListDescriptor
            {
                Snapshots = snapshots.m_Snapshots.Select(snapshot => (SnapshotDescriptor)snapshot).ToArray(),
            };
        }
    }
}
