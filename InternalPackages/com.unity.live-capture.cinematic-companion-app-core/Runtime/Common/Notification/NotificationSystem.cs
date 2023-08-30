using NotificationData = Unity.TouchFramework.Notification.NotificationData;
using ModalPopupData = Unity.TouchFramework.ModalPopup.ModalPopupData;
using Zenject;

namespace Unity.CompanionAppCommon
{
    public class NotificationSystem : INotificationSystem
    {
        [Inject]
        INotificationView m_NotificationView;

        public void Show(string text)
        {
            m_NotificationView.Show(new NotificationData()
            {
                text = text,
                displayDuration = 4f,
                fadeDuration = 0.1f
            });
        }

        public void Show(NotificationData notificationData)
        {
            m_NotificationView.Show(notificationData);
        }

        public void Show(ModalPopupData modalPopupData)
        {
            m_NotificationView.Show(modalPopupData);
        }
    }
}
