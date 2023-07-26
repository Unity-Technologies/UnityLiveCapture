using NotificationData = Unity.TouchFramework.Notification.NotificationData;
using ModalPopupData = Unity.TouchFramework.ModalPopup.ModalPopupData;

namespace Unity.CompanionAppCommon
{
    interface INotificationView : IDialogView
    {
        void Show(NotificationData notificationData);
        void Show(ModalPopupData modalPopupData);
        void HidePopup();
    }
}
