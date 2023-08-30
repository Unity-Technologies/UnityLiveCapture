using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IAssetReceiver<TKey, TObj> where TObj : UnityEngine.Object
    {
        void Receive(TKey previewGuid, TObj preview);
    }

    interface IAssetManager<TKey, TObj> : IDisposable where TObj : UnityEngine.Object
    {
        void AddReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver);

        void RemoveReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver);
    }

    class AssetManager<TKey, TObj> : IAssetManager<TKey, TObj> where TObj : UnityEngine.Object where TKey : IEquatable<TKey>
    {
        readonly ListPool<IAssetReceiver<TKey, TObj>> m_ReceiverListPool = new ListPool<IAssetReceiver<TKey, TObj>>();

        // LRU cache for assets not in use.
        readonly LruCache<TKey, TObj> m_Cache;

        readonly Dictionary<TKey, List<IAssetReceiver<TKey, TObj>>> m_Receivers = new Dictionary<TKey, List<IAssetReceiver<TKey, TObj>>>();

        // Assets referred to by one or more active receivers.
        readonly Dictionary<TKey, TObj> m_InUseAssets = new Dictionary<TKey, TObj>();

        // Guids of assets not yet available, waited on by one or more receivers.
        readonly HashSet<TKey> m_PendingGuids = new HashSet<TKey>();

        public IEnumerable<TKey> PendingGuids => m_PendingGuids;

        public AssetManager(LruCache<TKey, TObj> cache)
        {
            m_Cache = cache;
        }

        public void AddAsset(TKey key, TObj value)
        {
            var wasPending = m_PendingGuids.Remove(key);

            // If the asset was pending we have a bunch of receivers waiting for it.
            if (wasPending)
            {
                m_InUseAssets.Add(key, value);

                if (m_Receivers.TryGetValue(key, out var receivers))
                {
                    // We expect one or more receivers for the asset.
                    Assert.IsTrue(receivers.Count > 0);

                    foreach (var receiver in receivers)
                    {
                        receiver.Receive(key, value);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Pending asset has no receiver, key: [{key}]");
                }
            }
            // We may receive the same asset multiple times.
            else if (!m_InUseAssets.ContainsKey(key))
            {
                // Cache directly.
                m_Cache.TryStore(key, value);
            }
        }

        public void RemovePendingAsset(TKey key)
        {
            m_PendingGuids.Remove(key);
        }

        public void AddReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver)
        {
            if (HasReceiver(key, receiver))
            {
                Debug.LogError($"Receiver already registered, key: [{key}]");
                return;
            }

            InsertReceiver(key, receiver);

            // Asset is already in use.
            if (m_InUseAssets.TryGetValue(key, out var value))
            {
                receiver.Receive(key, value);
            }
            // Otherwise look for it in the cache.
            else if (m_Cache.TryRetrieve(key, out var cachedPreview))
            {
                // The asset is now in-use.
                m_InUseAssets.Add(key, cachedPreview);
                receiver.Receive(key, cachedPreview);
            }
            // Asset is neither in-use nor cached, append to the collection of pending assets.
            else
            {
                m_PendingGuids.Add(key);
            }
        }

        public void RemoveReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver)
        {
            var deleted = DeleteReceiver(key, receiver);

            if (!deleted)
            {
                Debug.LogError($"Receiver was not registered, key: [{key}]");
                return;
            }

            // If there's no receiver for an asset, it's now neither in-use nor pending.
            if (GetReceiversCount(key) == 0)
            {
                // Asset was never received.
                if (m_PendingGuids.Contains(key))
                {
                    m_PendingGuids.Remove(key);
                }
                // Cache asset if it was in-use.
                else if (m_InUseAssets.TryGetValue(key, out var texture))
                {
                    m_InUseAssets.Remove(key);

                    var cached = m_Cache.TryStore(key, texture);

                    if (!cached)
                    {
                        Debug.LogError($"Asset already cached, key: [{key}]");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Asset was neither in use nor pending, key: [{key}]");
                }
            }
        }

        public void Clear()
        {
            foreach (var item in m_InUseAssets)
            {
                m_Cache.TryStore(item.Key, item.Value);
            }

            m_Receivers.Clear();
            m_InUseAssets.Clear();
            m_PendingGuids.Clear();
        }

        public virtual void Dispose()
        {
            Clear();
            m_Cache.Dispose();
            m_ReceiverListPool.Dispose();
        }

        int GetReceiversCount(TKey key)
        {
            if (m_Receivers.TryGetValue(key, out var receivers))
            {
                return receivers.Count;
            }

            return 0;
        }

        void InsertReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver)
        {
            if (m_Receivers.TryGetValue(key, out var receivers))
            {
                receivers.Add(receiver);
            }
            else
            {
                var list = m_ReceiverListPool.Get();
                list.Add(receiver);
                m_Receivers.Add(key, list);
            }
        }

        bool DeleteReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver)
        {
            if (m_Receivers.TryGetValue(key, out var receivers))
            {
                var removed = receivers.Remove(receiver);

                // Recycle list if empty.
                if (receivers.Count == 0)
                {
                    m_Receivers.Remove(key);
                    m_ReceiverListPool.Release(receivers);
                }

                return removed;
            }

            return false;
        }

        bool HasReceiver(TKey key, IAssetReceiver<TKey, TObj> receiver)
        {
            if (m_Receivers.TryGetValue(key, out var receivers))
            {
                return receivers.Contains(receiver);
            }

            return false;
        }
    }
}
