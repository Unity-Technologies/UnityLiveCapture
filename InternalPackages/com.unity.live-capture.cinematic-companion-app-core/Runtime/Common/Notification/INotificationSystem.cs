using NotificationData = Unity.TouchFramework.Notification.NotificationData;
using ModalPopupData = Unity.TouchFramework.ModalPopup.ModalPopupData;

namespace Unity.CompanionAppCommon
{
    interface INotificationSystem
    {
        void Show(string text);
        void Show(NotificationData notificationData);
        void Show(ModalPopupData modalPopupData);
    }
}
