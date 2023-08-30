using UnityEngine;
using Unity.TouchFramework;
using NotificationData = Unity.TouchFramework.Notification.NotificationData;
using ModalPopupData = Unity.TouchFramework.ModalPopup.ModalPopupData;


namespace Unity.CompanionAppCommon
{
    class NotificationView : DialogView, INotificationView
    {
        [SerializeField]
        Notification m_Notification;
        [SerializeField]
        ModalPopup m_ModalPopup;

        void Start()
        {
            m_Notification.enabled = true;
            m_ModalPopup.enabled = true;
        }

        public void Show(NotificationData notificationData)
        {
            m_Notification.Display(notificationData);
        }

        public void Show(ModalPopupData notificationData)
        {
            m_ModalPopup.Display(notificationData);
        }

        public void HidePopup()
        {
            m_ModalPopup.Close();
        }
    }
}
