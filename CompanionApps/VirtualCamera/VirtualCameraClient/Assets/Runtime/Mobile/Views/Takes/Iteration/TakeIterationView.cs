using System;
using TMPro;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    [DisallowMultipleComponent]
    class TakeIterationView : DialogView, ITakeIterationView
    {
        public event Action<VirtualCameraChannelFlags> onChannelFlagsChanged = delegate {};
        public event Action onClearIterationBase = delegate {};
        public event Action onSetIterationBase = delegate {};
        public event Action onCloseClicked = delegate {};

        [SerializeField]
        TextMeshProUGUI m_IterationBaseName;
        [SerializeField]
        RoundedRectButton m_ClearIterationBaseButton;
        [SerializeField]
        RoundedRectButton m_SetIterationBaseButton;

        [SerializeField]
        SlideToggle m_LockPositionToggle;
        [SerializeField]
        SlideToggle m_LockRotationToggle;
        [SerializeField]
        SlideToggle m_LockFocalLengthToggle;
        [SerializeField]
        SlideToggle m_LockApertureToggle;
        [SerializeField]
        SlideToggle m_LockFocusDistanceToggle;

        [SerializeField]
        Button m_CloseButton;

        VirtualCameraChannelFlags m_ChannelFlags;

        public string IterationBaseName
        {
            set => m_IterationBaseName.text = value;
        }

        public bool CanClearIterationBase
        {
            set => m_ClearIterationBaseButton.enabled = value;
        }

        public void SetChannelFlags(VirtualCameraChannelFlags channelFlags)
        {
            m_ChannelFlags = channelFlags;
            m_LockPositionToggle.on = m_ChannelFlags.HasFlag(VirtualCameraChannelFlags.Position);
            m_LockRotationToggle.on = m_ChannelFlags.HasFlag(VirtualCameraChannelFlags.Rotation);
            m_LockFocalLengthToggle.on = m_ChannelFlags.HasFlag(VirtualCameraChannelFlags.FocalLength);
            m_LockApertureToggle.on = m_ChannelFlags.HasFlag(VirtualCameraChannelFlags.Aperture);
            m_LockFocusDistanceToggle.on = m_ChannelFlags.HasFlag(VirtualCameraChannelFlags.FocusDistance);
        }

        void Awake()
        {
            SetChannelFlags(m_ChannelFlags);
        }

        void OnEnable()
        {
            m_ClearIterationBaseButton.onClick.AddListener(OnClearIterationBase);
            m_SetIterationBaseButton.onClick.AddListener(OnSetIterationBase);

            m_LockPositionToggle.onValueChanged.AddListener(OnLockPositionChanged);
            m_LockRotationToggle.onValueChanged.AddListener(OnLockRotationChanged);
            m_LockFocalLengthToggle.onValueChanged.AddListener(OnLockFocalLengthChanged);
            m_LockApertureToggle.onValueChanged.AddListener(OnLockApertureChanged);
            m_LockFocusDistanceToggle.onValueChanged.AddListener(OnLockFocusDistanceChanged);

            // Close button is optional.
            if (m_CloseButton != null)
            {
                m_CloseButton.onClick.AddListener(OnCloseClicked);
            }
        }

        void OnDisable()
        {
            if (m_CloseButton != null)
            {
                m_CloseButton.onClick.RemoveListener(OnCloseClicked);
            }

            m_ClearIterationBaseButton.onClick.RemoveListener(OnClearIterationBase);
            m_SetIterationBaseButton.onClick.RemoveListener(OnSetIterationBase);

            m_LockPositionToggle.onValueChanged.RemoveListener(OnLockPositionChanged);
            m_LockRotationToggle.onValueChanged.RemoveListener(OnLockRotationChanged);
            m_LockFocalLengthToggle.onValueChanged.RemoveListener(OnLockFocalLengthChanged);
            m_LockApertureToggle.onValueChanged.RemoveListener(OnLockApertureChanged);
            m_LockFocusDistanceToggle.onValueChanged.RemoveListener(OnLockFocusDistanceChanged);
        }

        void OnCloseClicked() => onCloseClicked.Invoke();
        void OnClearIterationBase() => onClearIterationBase.Invoke();
        void OnSetIterationBase() => onSetIterationBase.Invoke();

        void OnLockPositionChanged(bool value) => SetChannelFlagAndDispatch(VirtualCameraChannelFlags.Position, value);
        void OnLockRotationChanged(bool value) => SetChannelFlagAndDispatch(VirtualCameraChannelFlags.Rotation, value);
        void OnLockFocalLengthChanged(bool value) => SetChannelFlagAndDispatch(VirtualCameraChannelFlags.FocalLength, value);
        void OnLockApertureChanged(bool value) => SetChannelFlagAndDispatch(VirtualCameraChannelFlags.Aperture, value);
        void OnLockFocusDistanceChanged(bool value) => SetChannelFlagAndDispatch(VirtualCameraChannelFlags.FocusDistance, value);

        void SetChannelFlagAndDispatch(VirtualCameraChannelFlags flag, bool value)
        {
            if (value)
            {
                m_ChannelFlags |= flag;
            }
            else
            {
                m_ChannelFlags &= ~flag;
            }
            onChannelFlagsChanged.Invoke(m_ChannelFlags);
        }
    }
}
