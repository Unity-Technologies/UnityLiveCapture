using TMPro;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusDistanceHudView : MonoBehaviour, IFocusDistanceListener, IFocusModeListener, ILensIntrinsicsListener
    {
        [SerializeField]
        TextMeshProUGUI m_ValueText;

        [SerializeField]
        TextMeshProUGUI m_UnitText;

        FocusMode m_CachedFocusMode = FocusMode.Clear;
        float m_CachedFocusDistance = -1; // Deliberately invalid.
        float m_CachedCloseFocusDistance = .3f;

        FocusMode m_FocusMode;
        float m_FocusDistance;
        float m_CloseFocusDistance;

        public void SetFocusDistance(float value, Vector2 range)
        {
            m_FocusDistance = value;
            UpdateView();
        }

        public void SetFocusMode(FocusMode value)
        {
            m_FocusMode = value;
            UpdateView();
        }

        public void SetLensIntrinsics(LensIntrinsics value)
        {
            m_CloseFocusDistance = value.CloseFocusDistance;
            UpdateView();
        }

        void Awake()
        {
            UpdateView(true);
        }

        void UpdateView(bool force = false)
        {
            var focusModeChanged = force || m_FocusMode != m_CachedFocusMode;

            if (m_FocusMode == FocusMode.Clear)
            {
                if (focusModeChanged)
                {
                    m_ValueText.text = "N/A";
                    m_UnitText.gameObject.SetActive(false);
                }
            }
            else if (focusModeChanged ||
                     !Mathf.Approximately(m_FocusDistance, m_CachedFocusDistance) ||
                     !Mathf.Approximately(m_CloseFocusDistance, m_CachedCloseFocusDistance))
            {
                m_ValueText.text = FocusDistanceUtility.AsString(m_FocusDistance, m_CloseFocusDistance, out var isInfinity);
                m_UnitText.gameObject.SetActive(!isInfinity);
            }

            m_CachedFocusMode = m_FocusMode;
            m_CachedFocusDistance = m_FocusDistance;
            m_CachedCloseFocusDistance = m_CloseFocusDistance;
        }
    }
}
