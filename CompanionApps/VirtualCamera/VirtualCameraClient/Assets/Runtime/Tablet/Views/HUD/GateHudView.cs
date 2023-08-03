using TMPro;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class GateHudView : MonoBehaviour, ISensorSizeListener
    {
        const string k_Format = "0.00";

        [SerializeField]
        TextMeshProUGUI m_SensorWidthText;
        [SerializeField]
        TextMeshProUGUI m_SensorHeightText;

        public void SetSensorSize(Vector2 sensorSize)
        {
            m_SensorWidthText.text = sensorSize.x.ToString(k_Format);
            m_SensorHeightText.text = sensorSize.y.ToString(k_Format);
        }
    }
}
