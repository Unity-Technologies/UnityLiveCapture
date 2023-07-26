using System;
using UnityEngine;
using Unity.TouchFramework;
using Unity.LiveCapture.VirtualCamera;


namespace Unity.CompanionApps.VirtualCamera
{
    class RotationLockView : MonoBehaviour, IRotationLockView
    {
        [SerializeField]
        SlideToggle m_XAxis;
        [SerializeField]
        SlideToggle m_Yaxis;
        [SerializeField]
        SlideToggle m_Zaxis;
        [SerializeField]
        SlideToggle m_ZeroDutch;
        [SerializeField]
        CanvasGroup m_ZeroDutchCanvasGroup;

        RotationAxis m_CachedRotation;

        public event Action<bool> onZeroDutchChanged = delegate {};
        public event Action<RotationAxis> onRotationLockChanged = delegate {};

        void Awake()
        {
            m_XAxis.onValueChanged.AddListener(OnTiltAxis);
            m_Yaxis.onValueChanged.AddListener(OnPanAxis);
            m_Zaxis.onValueChanged.AddListener(OnRollAxis);
            m_ZeroDutch.onValueChanged.AddListener(OnAutoHorizonAxis);
        }

        public void OnTiltAxis(bool on)
        {
            SetAxisLock(RotationAxis.Tilt, on);
        }

        public void OnPanAxis(bool on)
        {
            SetAxisLock(RotationAxis.Pan, on);
        }

        public void OnRollAxis(bool on)
        {
            SetAxisLock(RotationAxis.Roll, on);
        }

        public void OnAutoHorizonAxis(bool on)
        {
            onZeroDutchChanged.Invoke(on);
        }

        void SetAxisLock(RotationAxis axis, bool isEnabled)
        {
            var rotationLock = m_CachedRotation;

            if (isEnabled)
                rotationLock |= axis;
            else
                rotationLock &= ~axis;

            // TODO don't cache, poll widgets
            m_CachedRotation = rotationLock;
            onRotationLockChanged.Invoke(m_CachedRotation);
        }

        public void SetRotationLock(RotationAxis rotationAxis)
        {
            m_XAxis.on = rotationAxis.HasFlag(RotationAxis.Tilt);
            m_Yaxis.on = rotationAxis.HasFlag(RotationAxis.Pan);
            m_Zaxis.on = rotationAxis.HasFlag(RotationAxis.Roll);
            EnableZeroDutch(rotationAxis.HasFlag(RotationAxis.Roll));
        }

        public void SetAutoHorizon(bool zeroDutch)
        {
            m_ZeroDutch.on = zeroDutch;
        }

        void EnableZeroDutch(bool value)
        {
            m_ZeroDutchCanvasGroup.interactable = value;
            m_ZeroDutchCanvasGroup.blocksRaycasts = value;
            m_ZeroDutchCanvasGroup.alpha = value ? 1 : 0.25f;
        }
    }
}
