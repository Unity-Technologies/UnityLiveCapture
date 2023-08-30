using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Unity.CompanionAppCommon
{
    // TODO move to utilities.
    class LruCache<TKey, TObj> : IDisposable where TObj : Object
    {
        readonly uint m_Size;
        readonly List<TKey> m_List = new List<TKey>();
        readonly Dictionary<TKey, TObj> m_Map = new Dictionary<TKey, TObj>();

        public LruCache(uint size)
        {
            if (size < 1)
            {
                throw new ArgumentException($"{nameof(size)} must be superior or equal to 1.");
            }

            m_Size = size;
        }

        public void Dispose()
        {
            foreach (var item in m_Map)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(item.Value);
                }
                else
                {
                    Object.DestroyImmediate(item.Value);
                }
            }

            m_Map.Clear();
            m_List.Clear();
        }

        public bool TryStore(TKey key, TObj obj)
        {
            if (m_Map.ContainsKey(key))
            {
                return false;
            }

            m_Map.Add(key, obj);
            m_List.Add(key);

            if (m_List.Count == m_Size)
            {
                var delKey = m_List[0];
                Assert.IsTrue(m_Map.ContainsKey(delKey));
                var delObj = m_Map[delKey];
                m_List.RemoveAt(0);
                m_Map.Remove(delKey);
                Object.Destroy(delObj);
            }

            return true;
        }

        public bool TryRetrieve(TKey key, out TObj obj)
        {
            if (m_Map.TryGetValue(key, out obj))
            {
                m_List.Remove(key);
                m_Map.Remove(key);
                return true;
            }

            return false;
        }
    }
}
