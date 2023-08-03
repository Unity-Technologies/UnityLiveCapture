using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocalLengthView : DialogView, IFocalLengthView
    {
        [SerializeField]
        DialControl m_DialControl;
        [SerializeField]
        Toggle m_Toggle;
        FocalLengthScaler m_Scaler = new FocalLengthScaler();
        readonly DialControl.ILabelProvider m_LabelProvider = new DialControl.FloatWithUnitLabelProvider("mm");

        public event Action<float> ValueChanged = delegate {};
        public event Action Closed = delegate {};

        public Vector2 SensorSize
        {
            set => m_Scaler.SensorSize = value;
        }

        void Awake()
        {
            m_Toggle.onValueChanged.AddListener(value =>
            {
                if (!value)
                {
                    Closed.Invoke();
                }
            });

            m_DialControl.scaler = m_Scaler;
            m_DialControl.SetCustomLabelProvider(m_LabelProvider);
            m_DialControl.onSelectedValueChanged.AddListener(value => ValueChanged.Invoke(value));
        }

        public void SetFocalLength(float focalLength, Vector2 range)
        {
            m_DialControl.selectedValue = focalLength;
            m_DialControl.minimumValue = range.x;
            m_DialControl.maximumValue = range.y;
        }

        public void SetLensIntrinsics(LensIntrinsics value)
        {
            m_DialControl.minimumValue = value.FocalLengthRange.x;
            m_DialControl.maximumValue = value.FocalLengthRange.y;
        }

        protected override void OnShowChanged()
        {
            m_Toggle.isOn = IsShown;
        }
    }
}
