using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Unity.LiveCapture.Editor;
using Unity.LiveCapture.Networking;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.CompanionApp.Editor
{
    [CustomEditor(typeof(CompanionAppServer), true)]
    class CompanionAppConnectionEditor : ConnectionEditor
    {
        static class ID
        {
            public const string InfoNoClientSection = "no-client-section";
            public const string InfoClientSection = "client-section";
            public const string InfoClients = "client-list";
            public const string InfoLearnMoreButton = "learn-more-button";

            public const string SettingsPort = "port";
            public const string SettingsPortHelpBox = "port-warning";
            public const string SettingsInterfaces = "interfaces";
            public const string SettingsPublicWarning = "public-warning";
            public const string SettingsPublicWarningInfo = "public-warning-info";
            public const string SettingsPublicWarningDismiss = "public-warning-dismiss";

            public const string ClientName = "client-name";
            public const string ClientType = "client-type";
        }

        static class Constant
        {
            public const string TroubleshootingURL = Documentation.baseURL + "troubleshooting" + Documentation.endURL;
            public const string NetworkSetupURL = Documentation.baseURL + "setup-network" + Documentation.endURL;

            public const string DimissedPublicNetworkWarningKey = "dismissed-public-network-warning";

            public const long UpdateInterval = 16;
            public const long PortRefreshInterval = 2000;
            public const long InterfaceRefreshPeriod = 5000;
        }

        [SerializeField]
        protected VisualTreeAsset m_InfoUxml;
        [SerializeField]
        protected VisualTreeAsset m_ClientListElementUxml;
        [SerializeField]
        protected VisualTreeAsset m_SettingsUxml;
        [SerializeField]
        protected StyleSheet m_Stylesheet;

        VisualElement m_InfoNoClientSection;
        VisualElement m_InfoClientSection;
        VisualElement m_InfoClients;

        PropertyField m_SettingsPort;
        HelpBox m_SettingsPortHelpBox;
        VisualElement m_SettingsInterfaces;
        HelpBox m_SettingsPublicNetworkWarning;

        CompanionAppServer m_Connection;
        List<ICompanionAppClient> m_Clients = new List<ICompanionAppClient>();
        bool m_ShowPortHelpBox;
        (IPAddress, NetworkInterface)[] m_Addresses = { };
        bool m_HasPublicNetworks;
        bool m_PublicWarningDimissed;

        NetworkListManagerThreaded m_NLM;

        SerializedProperty m_ClientsExpanded;
        SerializedProperty m_InterfacesExpanded;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Connection = target as CompanionAppServer;

            CompanionAppServer.ClientConnected += ClientConnected;
            CompanionAppServer.ClientDisconnected += ClientDisconnected;

            m_ClientsExpanded = serializedObject.FindProperty("m_ClientsExpanded");
            m_InterfacesExpanded = serializedObject.FindProperty("m_InterfacesExpanded");

            m_NLM = new NetworkListManagerThreaded();
        }

        void OnDisable()
        {
            CompanionAppServer.ClientConnected -= ClientConnected;
            CompanionAppServer.ClientDisconnected -= ClientDisconnected;

            m_NLM.Dispose();
        }

        void UpdateInterfaces()
        {
            while (m_SettingsInterfaces.childCount > m_Addresses.Length)
            {
                m_SettingsInterfaces.RemoveAt(m_SettingsInterfaces.childCount - 1);
            }

            while (m_SettingsInterfaces.childCount < Math.Max(1, m_Addresses.Length))
            {
                m_SettingsInterfaces.Add(new Label());
            }

            for (var i = 0; i < m_SettingsInterfaces.childCount && i < m_Addresses.Length; i++)
            {
                var (ipAddress, networkInterface) = m_Addresses[i];

                var found = m_NLM.TryGetIsPublic(networkInterface, out var isPublic);

                var label = m_SettingsInterfaces.hierarchy.ElementAt(i) as Label;
                if (found && isPublic)
                {
                    label.text = ipAddress.ToString() + " [Public Network]";
                }
                else
                {
                    label.text = ipAddress.ToString();
                }
            }

            if (m_Addresses.Length == 0)
            {
                var label = m_SettingsInterfaces.hierarchy.ElementAt(0) as Label;
                label.text = "No interfaces available";
            }
        }

        /// <inheritdoc/>
        protected override VisualElement CreateInfoGUI()
        {
            var root = m_InfoUxml.Instantiate();
            root.styleSheets.Add(m_Stylesheet);

            m_InfoNoClientSection = root.Q<VisualElement>(ID.InfoNoClientSection);
            m_InfoClientSection = root.Q<VisualElement>(ID.InfoClientSection);
            m_InfoClients = root.Q<VisualElement>(ID.InfoClients);
            root.Q<Button>(ID.InfoLearnMoreButton).clickable.clicked += () => Application.OpenURL(Constant.TroubleshootingURL);

            var clientsFoldout = root.Q<Foldout>();
            clientsFoldout.BindProperty(m_ClientsExpanded);

            m_Clients = m_Connection.GetClients().ToList();
            RefreshClients(m_Clients);

            root.schedule.Execute(UpdateInfoGUI).Every(Constant.UpdateInterval);

            return root;
        }

        void UpdateInfoGUI()
        {
            m_InfoNoClientSection.SetDisplay(m_Connection.IsEnabled() && m_Connection.ClientCount == 0);
            m_InfoClientSection.SetDisplay(m_Connection.IsEnabled() && m_Connection.ClientCount > 0);
        }

        /// <inheritdoc/>
        protected override VisualElement CreateSettingsGUI()
        {
            var root = m_SettingsUxml.Instantiate();
            root.styleSheets.Add(m_Stylesheet);

            var so = new SerializedObject(m_Connection);
            root.Bind(so);

            m_SettingsPort = root.Q<PropertyField>(ID.SettingsPort);
            m_SettingsPort.RegisterValueChangeCallback(evt =>
            {
                RefreshPort();
            });

            m_SettingsPortHelpBox = root.Q<HelpBox>(ID.SettingsPortHelpBox);

            m_SettingsInterfaces = root.Q<VisualElement>(ID.SettingsInterfaces);

            m_SettingsPublicNetworkWarning = root.Q<HelpBox>(ID.SettingsPublicWarning);

            root.Q<Button>(ID.SettingsPublicWarningInfo).clickable.clicked += () => Application.OpenURL(Constant.NetworkSetupURL);

            root.Q<Button>(ID.SettingsPublicWarningDismiss).clickable.clicked += () =>
            {
                m_SettingsPublicNetworkWarning.SetDisplay(false);
                SessionState.SetBool(Constant.DimissedPublicNetworkWarningKey, true);
            };

            var interfacesFoldout = root.Q<Foldout>();
            interfacesFoldout.BindProperty(m_InterfacesExpanded);

            RefreshInterfaces();
            UpdateInterfaces();

            root.schedule.Execute(UpdateSettingsGUI).Every(Constant.UpdateInterval);
            root.schedule.Execute(RefreshInterfaces).Every(Constant.InterfaceRefreshPeriod);
            root.schedule.Execute(RefreshPort).Every(Constant.PortRefreshInterval);

            return root;
        }

        void UpdateSettingsGUI()
        {
            m_SettingsPort.SetEnabled(!m_Connection.IsEnabled());
            m_SettingsPortHelpBox.SetDisplay(m_ShowPortHelpBox && !m_Connection.IsEnabled());

            UpdateInterfaces();

            var shouldDimiss = SessionState.GetBool(Constant.DimissedPublicNetworkWarningKey, false);
            m_SettingsPublicNetworkWarning.SetDisplay(m_HasPublicNetworks && !shouldDimiss);
        }

        void ClientConnected(ICompanionAppClient client)
        {
            if (!m_Clients.Contains(client))
            {
                m_Clients.Add(client);
                RefreshClients(m_Clients);
            }
        }

        void ClientDisconnected(ICompanionAppClient client)
        {
            m_Clients.Remove(client);
            RefreshClients(m_Clients);
        }

        void RefreshClients(IList<ICompanionAppClient> clients)
        {
            if (m_InfoClients == null)
            {
                return;
            }

            m_InfoClients.Clear();

            foreach (var client in clients)
            {
                var element = m_ClientListElementUxml.Instantiate();
                element.Q<Label>(ID.ClientName).text = client.Name;
                element.Q<Label>(ID.ClientType).text = client.ToString();

                m_InfoClients.Add(element);
            }
        }

        void RefreshPort()
        {
            if (m_Connection.IsEnabled())
            {
                return;
            }

            var port = m_Connection.Port;

            var isValid = NetworkUtilities.IsPortValid(port, out var message);
            var isAvailable = false;

            if (isValid)
            {
                isAvailable = NetworkUtilities.IsPortAvailable(port);
            }

            m_ShowPortHelpBox = false;

            if (!isValid)
            {
                m_ShowPortHelpBox = true;
                m_SettingsPortHelpBox.messageType = HelpBoxMessageType.Error;
                m_SettingsPortHelpBox.text = message;
            }
            else
            {
                m_SettingsPortHelpBox.messageType = HelpBoxMessageType.Warning;

                if (!string.IsNullOrEmpty(message))
                {
                    m_ShowPortHelpBox = true;
                    m_SettingsPortHelpBox.text = message;

                }
                if (!isAvailable)
                {
                    m_ShowPortHelpBox = true;
                    m_SettingsPortHelpBox.text = $"Port {port} is in use by another program or Unity instance! Close the other program or assign a free port.";
                }
            }
        }

        void RefreshInterfaces()
        {
            m_Addresses = NetworkUtilities.GetIPInterfaces(false);

            m_HasPublicNetworks = m_Addresses.Any(x =>
            {
                var found = m_NLM.TryGetIsPublic(x.Item2, out var isPublic);
                return found && isPublic;
            });
        }
    }
}
