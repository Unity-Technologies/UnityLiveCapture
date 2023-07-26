using System;
using System.Collections.Generic;

namespace Unity.CompanionApps.VirtualCamera
{
    // TODO could be more generic.
    class ListPool<T> : IDisposable
    {
        readonly Stack<List<T>> m_Stack = new Stack<List<T>>();

        public List<T> Get()
        {
            if (m_Stack.Count > 0)
            {
                return m_Stack.Pop();
            }

            return new List<T>();
        }

        public void Release(List<T> list)
        {
            list.Clear();
            m_Stack.Push(list);
        }

        public void Dispose()
        {
            m_Stack.Clear();
        }
    }
}
