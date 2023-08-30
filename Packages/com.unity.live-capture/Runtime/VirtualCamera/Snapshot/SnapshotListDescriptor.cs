using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class SnapshotListDescriptor
    {
        [SerializeField]
        SnapshotDescriptor[] m_Snapshots;

        public SnapshotDescriptor[] Snapshots
        {
            get => m_Snapshots;
            set => m_Snapshots = value;
        }

        public static SnapshotListDescriptor Create(IEnumerable<Snapshot> snapshots)
        {
            var descriptor = new SnapshotListDescriptor();
#if UNITY_EDITOR
            if (snapshots != null)
            {
                descriptor.Snapshots = snapshots.Select(s => SnapshotDescriptor.Create(s)).ToArray();
            }
#endif
            return descriptor;
        }
    }
}
