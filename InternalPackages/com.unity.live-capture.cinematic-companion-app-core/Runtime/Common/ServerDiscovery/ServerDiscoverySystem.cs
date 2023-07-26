using System;
using System.Collections.Generic;
using Unity.LiveCapture.Networking.Discovery;
using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    /// <summary>
    /// A system that discovers servers.
    /// </summary>
    class ServerDiscoverySystem : IInitializable, ITickable
    {
        [Inject]
        SignalBus m_SignalBus;

        readonly DiscoveryClient m_Discovery = new DiscoveryClient();
        readonly List<DiscoveryInfo> m_Servers = new List<DiscoveryInfo>();

        /// <summary>
        /// The system invokes this event when a server is discovered, changes configuration, or gets lost.
        /// </summary>
        public Action<List<DiscoveryInfo>> ServersChanged;

        public void Initialize()
        {
            m_Discovery.ServerFound += (discoveryInfo) =>
            {
                m_Servers.Add(discoveryInfo);

                m_SignalBus.Fire(new ServerDiscoveryUpdatedSignal() { value = m_Servers.ToArray() });
            };
            m_Discovery.ServerLost += (discoveryInfo) =>
            {
                for (var i = 0; i < m_Servers.Count; i++)
                {
                    if (m_Servers[i].ServerInfo.ID == discoveryInfo.ServerInfo.ID)
                    {
                        m_Servers.RemoveAt(i);
                        m_SignalBus.Fire(new ServerDiscoveryUpdatedSignal()
                        {
                            value = m_Servers.ToArray()
                        });

                        break;
                    }
                }
            };
        }

        public void StartServerDiscovery()
        {
            m_Discovery.Start("Live Capture");
        }

        public void StopServerDiscovery()
        {
            m_Discovery.Stop();
        }

        public void Tick()
        {
            m_Discovery.Update();
        }
    }
}
