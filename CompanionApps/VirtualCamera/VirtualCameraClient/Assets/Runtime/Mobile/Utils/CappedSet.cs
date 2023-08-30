using System;
using System.Collections.Generic;

namespace Unity.CompanionApps.VirtualCamera
{
    class CappedSet<T>
    {
        readonly int m_Size;
        readonly HashSet<T> m_HashSet = new HashSet<T>();
        readonly Queue<T> m_Queue = new Queue<T>();

        public CappedSet(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException($"{nameof(size)} must be above or equal to 1.");
            }

            m_Size = size;
        }

        public void Dispose()
        {
            m_HashSet.Clear();
            m_Queue.Clear();
        }

        public bool Contains(T item) => m_HashSet.Contains(item);

        public void Add(T item)
        {
            if (m_HashSet.Contains(item))
            {
                return;
            }

            m_HashSet.Add(item);
            m_Queue.Enqueue(item);

            if (m_Queue.Count == m_Size)
            {
                var rm = m_Queue.Dequeue();
                m_HashSet.Remove(rm);
            }
        }
    }
}
