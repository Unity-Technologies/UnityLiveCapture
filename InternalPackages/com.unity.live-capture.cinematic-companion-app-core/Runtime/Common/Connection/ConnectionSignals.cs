namespace Unity.CompanionAppCommon
{
    class SetConnectionModeSignal : Signal<ConnectionMode> {}

    class SelectServerSignal : Signal<int> {}

    class RequestConnectSignal
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string ClientType { get; set; }
    }

    class RequestDisconnectSignal {}
}
