using System;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class PrefabScope : IDisposable
    {
        bool m_Disposed;

        public GameObject Root { get; private set; }

        public PrefabScope(string path)
        {
            var prefab = Resources.Load<GameObject>(path);

            Root = GameObject.Instantiate(prefab);
        }

        public void Dispose()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(PrefabScope));

            if (Root != null)
                GameObject.DestroyImmediate(Root);

            m_Disposed = true;
        }
    }
}
