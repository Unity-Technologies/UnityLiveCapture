using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadConfigurationTabView : MonoBehaviour, IGamepadConfigurationTabView
    {
        public event Action<Space> onPedestalSpaceChanged = delegate {};
        public event Action<Space> onMotionSpaceChanged = delegate {};
        public event Action onViewLayoutPressed = delegate {};

        [SerializeField]
        TMP_Text m_DeviceNameLabel;
        [SerializeField]
        ButtonToggle m_PedestalSpaceToggle;
        [SerializeField]
        ButtonToggle m_MotionSpaceToggle;
        [SerializeField]
        Button m_ViewLayoutButton;

        void OnEnable()
        {
            m_PedestalSpaceToggle.onValueChanged.AddListener(OnPedestalSpaceChanged);
            m_MotionSpaceToggle.onValueChanged.AddListener(OnMotionSpaceChanged);
            m_ViewLayoutButton.onClick.AddListener(OnViewLayoutPressed);
        }

        void OnDisable()
        {
            m_PedestalSpaceToggle.onValueChanged.RemoveListener(OnPedestalSpaceChanged);
            m_MotionSpaceToggle.onValueChanged.RemoveListener(OnMotionSpaceChanged);
            m_ViewLayoutButton.onClick.RemoveListener(OnViewLayoutPressed);
        }

        public void SetDeviceName(string deviceName)
        {
            m_DeviceNameLabel.text = deviceName;
        }

        public void SetPedestalSpace(Space space)
        {
            m_PedestalSpaceToggle.SetValueWithoutNotify(SpaceToBool(space));
        }

        public void SetMotionSpace(Space space)
        {
            m_MotionSpaceToggle.SetValueWithoutNotify(SpaceToBool(space));
        }

        void OnPedestalSpaceChanged(bool value)
        {
            onPedestalSpaceChanged(BoolToSpace(value));
        }

        void OnMotionSpaceChanged(bool value)
        {
            onMotionSpaceChanged(BoolToSpace(value));
        }

        void OnViewLayoutPressed()
        {
            onViewLayoutPressed();
        }

        static bool SpaceToBool(Space space) => space == Space.World;
        static Space BoolToSpace(bool value) => value ? Space.World : Space.Self;
    }
}
