using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusModeView : DialogView, IFocusModeView
    {
        [SerializeField]
        ButtonControl2 m_DisableFocusMode;
        [SerializeField]
        ButtonControl2 m_ManualFocusMode;
        [SerializeField]
        ButtonControl2 m_AutoFocusMode;
        [SerializeField]
        ButtonControl2 m_SpatialFocusMode;

        public event Action<FocusMode> FocusModeChanged = delegate {};

        public void SetFocusMode(FocusMode value)
        {
            SetAllButtonControlsOff();
            GetButtonControl(value).on = true;
        }

        void SetAllButtonControlsOff()
        {
            m_DisableFocusMode.on = false;
            m_ManualFocusMode.on = false;
            m_AutoFocusMode.on = false;
            m_SpatialFocusMode.on = false;
        }

        ButtonControl2 GetButtonControl(FocusMode focusMode)
        {
            switch (focusMode)
            {
                case FocusMode.Clear: return m_DisableFocusMode;
                case FocusMode.Manual: return m_ManualFocusMode;
                case FocusMode.ReticleAF: return m_AutoFocusMode;
                case FocusMode.TrackingAF: return m_SpatialFocusMode;
            }

            return null;
        }

        void Awake()
        {
            m_DisableFocusMode.onControlTap.AddListener(OnDisableFocusClicked);
            m_ManualFocusMode.onControlTap.AddListener(OnManualFocusClicked);
            m_AutoFocusMode.onControlTap.AddListener(OnAutoFocusClicked);
            m_SpatialFocusMode.onControlTap.AddListener(OnSpatialFocusModeClicked);
        }

        void OnDisableFocusClicked(BaseEventData eventData)
        {
            FocusModeChanged.Invoke(FocusMode.Clear);
        }

        void OnManualFocusClicked(BaseEventData eventData)
        {
            FocusModeChanged.Invoke(FocusMode.Manual);
        }

        void OnAutoFocusClicked(BaseEventData eventData)
        {
            FocusModeChanged.Invoke(FocusMode.ReticleAF);
        }

        void OnSpatialFocusModeClicked(BaseEventData eventData)
        {
            FocusModeChanged.Invoke(FocusMode.TrackingAF);
        }
    }
}
