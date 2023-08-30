using System;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class SettingsViewMediator : IInitializable, IDisposable
    {
        [Inject]
        ISettingsView m_SettingsView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_SettingsView.DoneButtonClicked += OnDoneButtonClicked;
            m_SettingsView.ResetButtonClicked += OnResetButtonClicked;
            m_SettingsView.PrivacyPolicyClicked += OnPrivacyPolicyClick;
            m_SettingsView.DocumentationClicked += OnDocumentationClick;
            m_SettingsView.SupportClicked += OnSupportClick;
            m_SettingsView.AutoHideUIChanged += OnAutoHideUIChanged;
            m_SettingsView.CameraPassthroughChanged += OnCameraPassthroughChanged;
            m_SettingsView.DimScreenChanged += OnDimScreenChanged;
            m_SettingsView.FlipHorizontallyChanged += OnFlipHorizontallyChanged;
            m_SettingsView.RecordAudioChanged += OnRecordAudioChanged;
            m_SettingsView.RecordingCountdownChanged += OnRecordingCountdownChanged;
            m_SettingsView.RecordingCountdownValueChanged += OnRecordingCountdownValueChanged;
            m_SettingsView.RecordVideoChanged += OnRecordVideoChanged;
            m_SettingsView.ShowFaceChanged += OnShowFaceChanged;
            m_SettingsView.ShowRecordButtonChanged += OnShowRecordButtonChanged;
            m_SettingsView.TimecodeSourceChanged += OnTimecodeSourceChanged;
        }

        public void Dispose()
        {
            m_SettingsView.DoneButtonClicked -= OnDoneButtonClicked;
            m_SettingsView.ResetButtonClicked -= OnResetButtonClicked;
            m_SettingsView.PrivacyPolicyClicked -= OnPrivacyPolicyClick;
            m_SettingsView.DocumentationClicked -= OnDocumentationClick;
            m_SettingsView.SupportClicked -= OnSupportClick;
            m_SettingsView.AutoHideUIChanged -= OnAutoHideUIChanged;
            m_SettingsView.CameraPassthroughChanged -= OnCameraPassthroughChanged;
            m_SettingsView.DimScreenChanged -= OnDimScreenChanged;
            m_SettingsView.FlipHorizontallyChanged -= OnFlipHorizontallyChanged;
            m_SettingsView.RecordAudioChanged -= OnRecordAudioChanged;
            m_SettingsView.RecordingCountdownChanged -= OnRecordingCountdownChanged;
            m_SettingsView.RecordingCountdownValueChanged -= OnRecordingCountdownValueChanged;
            m_SettingsView.RecordVideoChanged -= OnRecordVideoChanged;
            m_SettingsView.ShowFaceChanged -= OnShowFaceChanged;
            m_SettingsView.ShowRecordButtonChanged -= OnShowRecordButtonChanged;
            m_SettingsView.TimecodeSourceChanged -= OnTimecodeSourceChanged;
        }

        void OnPrivacyPolicyClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenPrivacyPolicy());
        }

        void OnDocumentationClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenDocumentation());
        }

        void OnSupportClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenSupport());
        }

        void OnDoneButtonClicked()
        {
            m_SignalBus.Fire(new SettingsViewSignals.Close());
        }

        void OnResetButtonClicked()
        {
            m_SignalBus.Fire(new SettingsSignals.Reset());
        }

        void OnAutoHideUIChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.AutoHideUI, value));
        }

        void OnCameraPassthroughChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.CameraPassthrough, value));
        }

        void OnDimScreenChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.DimScreen, value));
        }

        void OnFlipHorizontallyChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.FlipHorizontally, value));
        }

        void OnRecordAudioChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.RecordAudio, value));
        }

        void OnRecordVideoChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.RecordVideo, value));
        }

        void OnRecordingCountdownChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.CountdownEnabled, value));
        }

        void OnRecordingCountdownValueChanged(int value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateInt(SettingsProperty.CountdownTime, value));
        }

        void OnShowFaceChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.FaceWireframe, value));
        }

        void OnShowRecordButtonChanged(bool value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateBool(SettingsProperty.ShowRecordButton, value));
        }

        void OnTimecodeSourceChanged(string value)
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateString(SettingsProperty.TimecodeSourceId, value));
        }
    }
}
