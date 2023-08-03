using TMPro;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class ZoomHudView : MonoBehaviour, IFocalLengthListener
    {
        [SerializeField]
        TextMeshProUGUI m_ValueText;

        public void SetFocalLength(float value, Vector2 range)
        {
            m_ValueText.text = ((int)value).ToString();
        }
    }
}
