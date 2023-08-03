using System;
using UnityEngine.XR.ARFoundation;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class BackgroundController : IInitializable, IDisposable, ISettingsPropertyListener
    {
        [Inject]
        ARCameraBackground m_CameraBackground;

        [Inject(Id = FaceClientViewId.Background)]
        IDialogView m_BackgroundView;

        [Inject]
        ICalibrationView m_CalibrationView;

        bool m_CameraPassthroughSetting;
        bool m_Calibrating;
        bool CameraPassthrough => m_CameraPassthroughSetting || m_Calibrating;

        public void Initialize()
        {
            m_CalibrationView.StateChanged += OnCalibrationViewStateChanged;
        }

        public void Dispose()
        {
            m_CalibrationView.StateChanged -= OnCalibrationViewStateChanged;
        }

        public void SettingsPropertyChanged(SettingsProperty property, ISettings settings)
        {
            if (property == SettingsProperty.CameraPassthrough)
            {
                m_CameraPassthroughSetting = settings.CameraPassthrough;
                UpdateBackground(CameraPassthrough);
            }
        }

        void OnCalibrationViewStateChanged(ICalibrationView.State newState)
        {
            m_Calibrating = newState == ICalibrationView.State.Calibrating;
            UpdateBackground(CameraPassthrough);
        }

        void UpdateBackground(bool cameraPassthrough)
        {
            m_CameraBackground.enabled = cameraPassthrough;

            if (cameraPassthrough)
            {
                m_BackgroundView.Hide();
            }
            else
            {
                m_BackgroundView.Show();
            }
        }
    }
}
