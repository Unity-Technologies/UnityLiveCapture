#if UNITY_EDITOR

#if UNITY_EDITOR_WIN
#define LIVE_CAPTURE_NLM_SUPPORTED
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Profiling;
using UnityEngine.Profiling;

namespace Unity.LiveCapture.Networking
{
    class NetworkListManagerThreaded : IDisposable
    {
#if LIVE_CAPTURE_NLM_SUPPORTED
        static readonly ProfilerMarker k_TryGetIsPublicMarker = new ProfilerMarker($"{nameof(NetworkListManagerThreaded)}.{nameof(TryGetIsPublic)}");
#endif

        class NetworkListManagerWorker : NetworkListManagerBase
        {
            static readonly ProfilerMarker k_ProcessRefreshMarker = new ProfilerMarker($"{nameof(NetworkListManagerWorker)}.{nameof(ProcessRefresh)}");
            static readonly ProfilerMarker k_ProcessResultMarker = new ProfilerMarker($"{nameof(NetworkListManagerWorker)}.{nameof(ProcessResult)}");

            readonly ConcurrentDictionary<Guid, NetworkCategory> m_Cache;

            public NetworkListManagerWorker(ConcurrentDictionary<Guid, NetworkCategory> cache)
            {
                m_Cache = cache;
            }

            protected override void ProcessRefresh()
            {
                using (k_ProcessRefreshMarker.Auto())
                {
                    m_Cache.Clear();
                }
            }

            protected override void ProcessResult(Result result, PopOutputFlags flags)
            {
                using (k_ProcessResultMarker.Auto())
                {
                    m_Cache[result.m_AdapterId] = result.m_NetworkCategory;
                }
            }
        }

        readonly ConcurrentDictionary<Guid, NetworkCategory> m_Cache;
        Thread m_Thread;

        public NetworkListManagerThreaded()
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            m_Cache = new ConcurrentDictionary<Guid, NetworkCategory>();

            m_Thread = new Thread(ThreadLoop);
            m_Thread.Start(m_Cache);
#endif
        }

        void ThreadLoop(object cacheObject)
        {
            Profiler.BeginThreadProfiling(nameof(NetworkListManager), nameof(NetworkListManagerThreaded));

            var cache = (ConcurrentDictionary<Guid, NetworkCategory>)cacheObject;

            using (var nlm = new NetworkListManagerWorker(cache))
            {
                try
                {
                    nlm.Update(true);

                    while (true)
                    {
                        nlm.Update(false);
                        Thread.Sleep(100);
                    }
                }
                catch(ThreadAbortException)
                {
                    Profiler.EndThreadProfiling();
                }
            }

            Profiler.EndThreadProfiling();
        }

        public bool TryGetIsPublic(Guid networkAdapterId, out bool isPublic)
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            using (k_TryGetIsPublicMarker.Auto())
            {
                var found = m_Cache.TryGetValue(networkAdapterId, out var category);

                if (found)
                {
                    isPublic = category == NetworkCategory.Public;
                    return true;
                }
            }
#endif

            isPublic = default;
            return false;
        }

        public bool TryGetIsPublic(in NetworkInterface network, out bool isPublic)
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            // NetworkInterface.Id might not be a valid Guid on platforms other than windows
            var guid = new Guid(network.Id);
            return TryGetIsPublic(guid, out isPublic);
#else
            isPublic = default;
            return false;
#endif
        }

        /// <inheritdoc/>
        public void Dispose()
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            m_Thread?.Abort();
            m_Thread = null;
#endif
        }
    }

    abstract class NetworkListManagerBase : IDisposable
    {
#if LIVE_CAPTURE_NLM_SUPPORTED
        static readonly ProfilerMarker k_UpdateMarker = new ProfilerMarker($"{nameof(NetworkListManagerBase)}.{nameof(Update)}");

        readonly NetworkListManagerPlugin m_Plugin;
#endif

        protected NetworkListManagerBase()
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            m_Plugin = new NetworkListManagerPlugin();
#endif
        }

        public void Update(bool forceRefresh)
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            using (k_UpdateMarker.Auto())
            {
                var updateInputFlags = UpdateInputFlags.None;
                if (forceRefresh)
                {
                    updateInputFlags |= UpdateInputFlags.ForceRefresh;
                }

                var updateOutputFlags = m_Plugin.Update(updateInputFlags);
                if (updateOutputFlags.HasFlag(UpdateOutputFlags.Refreshed))
                {
                    ProcessRefresh();
                }

                while (true)
                {
                    var flags = m_Plugin.PopResult(out var result);

                    if (flags.HasFlag(PopOutputFlags.Empty))
                    {
                        break;
                    }

                    ProcessResult(result, flags);
                }
            }
#endif
        }

        protected abstract void ProcessRefresh();

        protected abstract void ProcessResult(Result result, PopOutputFlags flags);

        /// <inheritdoc/>
        public void Dispose()
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            m_Plugin.Dispose();
#endif
        }
    }

    class NetworkListManager : NetworkListManagerBase
    {
        readonly Dictionary<Guid, NetworkCategory> m_Cache;

        public NetworkListManager()
        {
            m_Cache = new Dictionary<Guid, NetworkCategory>();
        }

        protected override void ProcessRefresh()
        {
            m_Cache.Clear();
        }

        protected override void ProcessResult(Result result, PopOutputFlags flags)
        {
            m_Cache.Add(result.m_AdapterId, result.m_NetworkCategory);
        }

        public bool TryGetIsPublic(Guid networkAdapterId, out bool isPublic)
        {
#if LIVE_CAPTURE_NLM_SUPPORTED
            var found = m_Cache.TryGetValue(networkAdapterId, out var category);

            if (found)
            {
                isPublic = category == NetworkCategory.Public;
                return true;
            }
#endif

            isPublic = default;
            return false;
        }
    }

    enum NetworkCategory
    {
        Public = 0,
        Private = 0x1,
        Domain = 0x2,
    }

    [Flags]
    enum UpdateInputFlags
    {
        None = 0,
        ForceRefresh = 1 << 0,
        OnlyConnectedNetworks = 1 << 1,
    }

    [Flags]
    enum UpdateOutputFlags
    {
        None = 0,
        Refreshed = 1 << 0,
    }

    [Flags]
    enum PopOutputFlags
    {
        None = 0,
        Empty = 1 << 0,
    }

    struct Result
    {
        public Guid m_AdapterId;
        public NetworkCategory m_NetworkCategory;
    }

#if LIVE_CAPTURE_NLM_SUPPORTED
    class NetworkListManagerPlugin : IDisposable
    {
        public NetworkListManagerPlugin()
        {
            m_Instance = Create();
        }

        ~NetworkListManagerPlugin()
        {
            if (m_Instance != IntPtr.Zero)
            {
                throw new InvalidOperationException("NetworkListManager instance should be disposed before finalization.");
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Destroy(m_Instance);
            m_Instance = IntPtr.Zero;
        }

        public UpdateOutputFlags Update(UpdateInputFlags flags)
        {
            var inputFlags = (int)flags;

            var outFlags = Update(m_Instance, inputFlags);
            if (outFlags < 0)
            {
                throw new InvalidOperationException("Invalid instance.");
            }

            return (UpdateOutputFlags)outFlags;
        }

        public PopOutputFlags PopResult(out Result outResult)
        {
            var flags = PopResult(m_Instance, out Guid adapterId, out int networkCategory);
            if (flags < 0)
            {
                throw new InvalidOperationException("Invalid instance.");
            }

            NetworkCategory category = (NetworkCategory)networkCategory;
            outResult = new Result{ m_AdapterId = adapterId, m_NetworkCategory = category };
            PopOutputFlags outFlags = (PopOutputFlags)flags;
            return outFlags;
        }

        IntPtr m_Instance;

        const string PluginName = "NetworkListManager";

        [DllImport(PluginName)]
        static extern IntPtr Create();

        [DllImport(PluginName)]
        static extern void Destroy(IntPtr instance);

        [DllImport(PluginName)]
        static extern int Update(IntPtr instance, int flags);

        [DllImport(PluginName)]
        static extern int PopResult(IntPtr instance, out Guid adapterId, out int networkCategory);
    }
#endif
}
#endif
