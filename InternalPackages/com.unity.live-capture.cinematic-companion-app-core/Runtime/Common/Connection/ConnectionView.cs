using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture.Networking.Discovery;
using TMPro;
using Unity.TouchFramework;

namespace Unity.CompanionAppCommon
{
    class ConnectionView : MonoBehaviour, IConnectionView
    {
        const string k_Connect = "Connect";
        const string k_Disconnect = "Disconnect";
        const string k_TryingToConnect = "Trying to Connect";
        const string k_Scanning = "Scanning for servers...";
        static readonly Color k_HighlightButtonColor = new Color(0.2784314f, 0.2784314f, 0.2784314f, 1f);


        public event Action onConnectClicked = delegate {};
        public event Action onManualClicked = delegate {};
        public event Action onScanClicked = delegate {};
        public event Action<int> onServerSelected = delegate {};

        [SerializeField]
        GameObject m_ConnectionModeLayout;
        [SerializeField]
        Button m_ConnectButton;
        [SerializeField]
        Button m_ManualConnection;
        [SerializeField]
        Button m_ScanConnection;
        [SerializeField]
        GameObject m_ScanLayout;
        [SerializeField]
        TMP_Dropdown m_ServersList;
        [SerializeField]
        GameObject m_IPGroup;
        [SerializeField]
        TMP_Text m_DiscoveryInfo;
        [SerializeField]
        TMP_InputField m_PortInputField;
        [SerializeField]
        TMP_InputField m_IPInputField;
        [SerializeField]
        Color m_ConnectedButtonColor;
        [SerializeField]
        Color m_DisabledButtonColor;
        [SerializeField]
        Color m_DisconnectedButtonColor;

        public string Ip
        {
            get => m_IPInputField.text;
            set => m_IPInputField.SetTextWithoutNotify(value);
        }

        public int Port
        {
            get => int.Parse(m_PortInputField.text);
            set => m_PortInputField.SetTextWithoutNotify(value.ToString());
        }

        void Awake()
        {
            m_IPInputField.onValueChanged.AddListener(OnIPChanged);
            m_PortInputField.onValueChanged.AddListener(OnPortChanged);
            m_ScanConnection.onClick.AddListener(ScanClicked);
            m_ManualConnection.onClick.AddListener(ManualClicked);
            m_ConnectButton.onClick.AddListener(OnConnectClicked);
        }

        void OnIPChanged(string value)
        {
            UpdateConnectButton();
        }

        void OnPortChanged(string value)
        {
            UpdateConnectButton();
        }

        void UpdateConnectButton()
        {
            var parsedIp = IPAddress.TryParse(Ip, out var _);

            var portIsValid = int.TryParse(m_PortInputField.text, out var port);
            if (portIsValid)
            {
                portIsValid = PortIsValid(port);
            }

            SetConnectButtonEnabled(parsedIp && portIsValid);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void ScanClicked()
        {
            onScanClicked.Invoke();
        }

        void ManualClicked()
        {
            onManualClicked.Invoke();
        }

        public void UpdateLayout(ConnectionModel connection)
        {
            switch (connection.State)
            {
                case ConnectionState.TryingToConnect:
                    SetTryingToConnectLayout();
                    break;
                case ConnectionState.Connected:
                    SetConnectedLayout();
                    break;
                case ConnectionState.Disconnected:
                    SetDisconnectedLayout(connection);
                    break;
            }

            UpdateConnectionMode(connection.Mode);
            SetServerDiscoveryResults(connection.SelectedServer, connection.DiscoveryResults);
            UpdateRow2(connection);
        }

        void UpdateConnectionMode(ConnectionMode mode)
        {
            SetOn(m_ScanConnection, mode == ConnectionMode.Scan);
            SetOn(m_ManualConnection, mode == ConnectionMode.Manual);
        }

        void ClearServerList()
        {
            m_ServersList.onValueChanged.RemoveAllListeners();
            m_ServersList.ClearOptions();
        }

        void SetServerDiscoveryResults(int selectedServer, DiscoveryInfo[] results)
        {
            ClearServerList();

            if (results == null || results.Length == 0)
                return;

            if (results.Length > 1)
            {
                foreach (var server in results)
                {
                    m_ServersList.options.Add(new TMP_Dropdown.OptionData()
                    {
                        text = server.ServerInfo.InstanceName
                    });
                }

                m_ServersList.onValueChanged.AddListener((value) =>
                {
                    onServerSelected.Invoke(value);
                });

                m_ServersList.SetValueWithoutNotify(selectedServer);
            }
        }

        void SetTryingToConnectLayout()
        {
            SetConnectButtonEnabled(false);

            m_ConnectButton.GetComponentInChildren<TMP_Text>().text = k_TryingToConnect;
        }

        void SetConnectedLayout()
        {
            SetConnectButtonEnabled(true);

            m_ConnectButton.GetComponent<Image>().color = m_DisconnectedButtonColor;
            m_ConnectButton.GetComponentInChildren<TMP_Text>().text = k_Disconnect;
            m_ConnectionModeLayout.SetActive(false);
            m_IPInputField.interactable = false;
            m_PortInputField.interactable = false;
            m_ServersList.interactable = false;
        }

        void SetDisconnectedLayout(ConnectionModel connection)
        {
            var enableConnectButton = connection.Mode == ConnectionMode.Manual
                || (connection.DiscoveryResults != null
                    && connection.DiscoveryResults.Length > 0
                    && connection.SelectedServer != -1);

            SetConnectButtonEnabled(enableConnectButton);

            if (enableConnectButton)
            {
                m_ConnectButton.GetComponent<Image>().color = m_ConnectedButtonColor;
            }

            m_ConnectButton.GetComponentInChildren<TMP_Text>().text = k_Connect;
            m_ConnectionModeLayout.SetActive(true);
            m_IPInputField.interactable = true;
            m_PortInputField.interactable = true;
            m_ServersList.interactable = true;
        }

        void UpdateRow2(ConnectionModel connection)
        {
            m_IPGroup.gameObject.SetActive(false);
            m_ServersList.gameObject.SetActive(false);
            m_ScanLayout.SetActive(false);

            if (connection.Mode == ConnectionMode.Scan)
            {
                var isConnected = connection.State == ConnectionState.Connected;
                var serverCount = m_ServersList.options.Count;

                m_ServersList.gameObject.SetActive(serverCount > 1 && !isConnected);
                m_ScanLayout.SetActive(serverCount <= 1 || isConnected);
            }
            else
            {
                m_IPGroup.gameObject.SetActive(true);
            }

            var isSelectedServerValid = connection.DiscoveryResults != null
                && connection.DiscoveryResults.Length > 0
                && connection.SelectedServer >= 0
                && connection.SelectedServer < connection.DiscoveryResults.Length;

            if (connection.State == ConnectionState.Connected)
            {
                m_DiscoveryInfo.text = connection.ConnectedServerName;
            }
            else if (!isSelectedServerValid)
            {
                m_DiscoveryInfo.text = k_Scanning;
            }
            else
            {
                var result = connection.DiscoveryResults[connection.SelectedServer];

                m_DiscoveryInfo.text = result.ServerInfo.InstanceName;
            }
        }

        void SetConnectButtonEnabled(bool value)
        {
            var color = value ? m_ConnectedButtonColor : m_DisabledButtonColor;

            m_ConnectButton.GetComponent<Image>().color = color;
            m_ConnectButton.interactable = value;
        }

        void OnConnectClicked()
        {
            onConnectClicked.Invoke();
        }

        void SetOn(Button button, bool value)
        {
            var image = button.GetComponent<Image>();
            var text = button.GetComponentInChildren<TMP_Text>();

            image.color = value ? k_HighlightButtonColor : UIConfig.propertyBaseColor;
            text.color = value ? UIConfig.propertyTextSelectedColor : UIConfig.propertyTextBaseColor;
        }

        bool PortIsValid(int port)
        {
            return port > 0 && port <= 65535;
        }
    }
}
