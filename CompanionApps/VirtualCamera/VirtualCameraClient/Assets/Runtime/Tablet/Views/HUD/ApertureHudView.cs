using TMPro;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class ApertureHudView : MonoBehaviour, IApertureListener
    {
        const string k_Format = "0.##";

        [SerializeField]
        TextMeshProUGUI m_ValueText;

        public void SetAperture(float value, Vector2 range)
        {
            m_ValueText.text = value.ToString(k_Format);
        }
    }
}
