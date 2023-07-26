using Zenject;

namespace Unity.CompanionAppCommon
{
    class DisconnectedNotificationCommand
    {
        [Inject]
        ConnectionModel m_Connection;

        [Inject]
        INotificationSystem m_Notification;

        public void Notify()
        {
            if (!m_Connection.InterruptedByFocusChange)
            {
                m_Notification.Show("Disconnected from server");
            }
        }
    }
}
