using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Asset that stores a list of snapshots.
    /// </summary>
    [CreateAssetMenu(fileName = "Snapshot Library", menuName = "Live Capture/Virtual Camera/Snapshot Library", order = 3)]
    [HelpURL(Documentation.baseURL + "virtual-camera-snapshots" + Documentation.endURL)]
    [ExcludeFromPreset]
    class SnapshotLibrary : ScriptableObject, IEnumerable<Snapshot>
    {
        [SerializeField, NonReorderable]
        List<Snapshot> m_Snapshots = new List<Snapshot>();

        /// <summary>
        /// The number of snapshots.
        /// </summary>
        public int Count => m_Snapshots.Count;

        /// <summary>
        /// The array of snapshots.
        /// </summary>
        /// <remarks>This will return a new copy of the array.</remarks>
        public Snapshot[] Snapshots => m_Snapshots.ToArray();

        /// <summary>
        /// Removes all snapshots from this library.
        /// </summary>
        public void Clear()
        {
            m_Snapshots.Clear();
        }

        /// <summary>
        /// Adds a snapshot to this library.
        /// </summary>
        public void Add(Snapshot snapshot)
        {
            m_Snapshots.Add(snapshot);
        }

        /// <summary>
        /// Removes a snapshot at the specified index.
        /// </summary>
        /// <returns>True if the provided index was valid.</returns>
        public bool RemoveAt(int index)
        {
            if (IsIndexValid(index))
            {
                m_Snapshots.RemoveAt(index);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the snapshot at the specified index.
        /// </summary>
        /// <returns>Returns the snapshot, or null if index is out of bounds.</returns>
        public Snapshot Get(int index)
        {
            if (IsIndexValid(index))
            {
                return m_Snapshots[index];
            }

            return null;
        }

        internal void Set(IEnumerable<Snapshot> snapshots)
        {
            m_Snapshots.Clear();
            m_Snapshots.AddRange(snapshots);
        }

        public IEnumerator<Snapshot> GetEnumerator()
        {
            return ((IEnumerable<Snapshot>)m_Snapshots).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_Snapshots).GetEnumerator();
        }

        bool IsIndexValid(int index)
        {
            return index >= 0 && index < Count;
        }
    }
}
