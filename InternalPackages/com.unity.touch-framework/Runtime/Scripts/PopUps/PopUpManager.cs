using UnityEngine;

namespace Unity.TouchFramework
{
    public class PopUpManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Tooltip m_Tooltip;
        [SerializeField]
        Notification m_Notification;
        [SerializeField]
        HelpTooltip m_HelpTooltip;
        [SerializeField]
        ModalPopup m_ModalPopUp;
#pragma warning restore CS0649

        public Notification.NotificationData GetNotificationData()
        {
            return m_Notification.DefaultData();
        }

        public void DisplayNotification(Notification.NotificationData data)
        {
            m_Notification.Display(data);
        }

        public Tooltip.TooltipData GetTooltipData()
        {
            return m_Tooltip.DefaultData();
        }

        public void DisplayTooltip(Tooltip.TooltipData data)
        {
            m_Tooltip.Display(data);
        }

        public void DisplayBigNotification(HelpTooltip.HelpTooltipData data)
        {
            m_HelpTooltip.Display(data);
        }

        public ModalPopup.ModalPopupData GetModalPopUpData()
        {
            return m_ModalPopUp.DefaultData();
        }

        public void DisplayModalPopUp(ModalPopup.ModalPopupData data)
        {
            m_ModalPopUp.Display(data);
        }
    }
}
