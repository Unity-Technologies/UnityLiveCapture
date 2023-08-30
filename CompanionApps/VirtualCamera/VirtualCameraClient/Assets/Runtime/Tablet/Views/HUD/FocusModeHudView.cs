using TMPro;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusModeHudView : MonoBehaviour, IFocusModeListener
    {
        [SerializeField]
        TextMeshProUGUI m_ValueText;

        public void SetFocusMode(FocusMode value)
        {
            m_ValueText.text = value.GetDescription();
        }
    }
}
