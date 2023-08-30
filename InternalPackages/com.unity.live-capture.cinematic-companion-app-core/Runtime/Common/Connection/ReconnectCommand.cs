using Zenject;

namespace Unity.CompanionAppCommon
{
    class ReconnectCommand
    {
        [Inject]
        ConnectionModel m_Connection;
        [Inject]
        IRemoteSystem m_Remote;

        public void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                if (m_Connection.State == ConnectionState.Connected)
                {
                    m_Connection.InterruptedByFocusChange = true;
                    m_Remote.Disconnect();
                }
            }
            else if (m_Connection.InterruptedByFocusChange)
            {
                m_Remote.Connect(m_Connection.Ip, m_Connection.Port, m_Connection.ClientType);
            }
        }
    }
}
