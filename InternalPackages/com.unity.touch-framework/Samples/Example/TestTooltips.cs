using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Packages.com.unity.Samples
{
    enum PopUpType
    {
        Tooltip,
        ModalPopup,
        Notification
    }

    public class TestTooltips : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        PopUpType m_TestType;
        [SerializeField]
        RectTransform m_RectPopUp;
        [SerializeField]
        PopUpManager m_PopUpManager;
        [SerializeField]
        Sprite[] m_Icons;
#pragma warning restore CS0649

        Vector3 m_position;

        void OnGUI()
        {
            GUILayout.Label("Press [SPACE] to display tooltips.");
        }

        void Update()
        {
            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                switch (m_TestType)
                {
                    case PopUpType.Notification:
                        NotificationTest();
                        break;
                    case PopUpType.Tooltip:
                        TooltipTest();
                        break;
                    case PopUpType.ModalPopup:
                        ModalPopup();
                        break;
                }
            }
        }

        void NotificationTest()
        {
            // Get custom struct from manager
            var data = m_PopUpManager.GetNotificationData();

            // The Notification has an icon and a different default position
            bool hasIcon = (Random.value > 0.5f && m_Icons != null && m_Icons.Length > 0);
            var icon = hasIcon ? m_Icons[Mathf.FloorToInt(Random.value * 10e3f) % m_Icons.Length] : null;
            data.icon = icon;
            data.text = "Notification text";

            // Send back the modified struct
            m_PopUpManager.DisplayNotification(data);
        }

        void TooltipTest()
        {
            // Get custom struct from manager
            var data = m_PopUpManager.GetTooltipData();

            // Modify the struct
            // The tooltip has an icon and an arrow that points the coordinate you put in data.worldPosition
            data.text = "This tool helps you";

            bool hasIcon = (Random.value > 0.5f && m_Icons != null && m_Icons.Length > 0);
            var icon = hasIcon ? m_Icons[Mathf.FloorToInt(Random.value * 10e3f) % m_Icons.Length] : null;
            data.icon = icon;

            // Sets position to the pointer
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                m_RectPopUp, Mouse.current.position.ReadValue(), null, out var worldposition);
            data.worldPosition = worldposition;

            // Send back the modified struct
            m_PopUpManager.DisplayTooltip(data);
        }

        void ModalPopup()
        {
            // Get custom struct from manager
            var data = m_PopUpManager.GetModalPopUpData();

            // The dialog modal is a bit more complex.
            // Title + text like the big tooltip
            data.title = "Alert";
            data.text = "This needs user confirmation";

            // Button and callback configuration
            data.positiveText = "Ok";
            data.positiveCallback = PositiveAction;

            // Negative button optional
            bool hasNegativeAction = Random.value > 0.5f;
            if (hasNegativeAction)
            {
                data.negativeText = "No, thanks";
                data.negativeCallback = NegativeAction;
            }
            m_PopUpManager.DisplayModalPopUp(data);
        }

        void NegativeAction()
        {
            Debug.Log("User canceled action");
        }

        void PositiveAction()
        {
            Debug.Log("User confirmed action");
        }
    }
}
