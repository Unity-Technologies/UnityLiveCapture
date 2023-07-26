using System;

namespace Unity.CompanionAppCommon
{
    interface IConnectionView
    {
        event Action onConnectClicked;
        event Action onManualClicked;
        event Action onScanClicked;
        event Action<int> onServerSelected;

        string Ip { get; set; }
        int Port { get; set; }

        void UpdateLayout(ConnectionModel connection);
    }
}
