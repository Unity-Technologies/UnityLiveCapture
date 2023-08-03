using System;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class MainViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IMainView m_MainView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_MainView.RecordButtonClicked += OnRecordButtonClicked;
            m_MainView.SettingsButtonClicked += OnSettingsButtonClicked;
            m_MainView.HelpToggleChanged += OnHelpToggleChanged;
            m_MainView.TrackingButtonClicked += OnTrackingButtonClicked;
            m_MainView.TakesButtonClicked += OnTakesButtonClicked;
        }

        public void Dispose()
        {
            m_MainView.RecordButtonClicked -= OnRecordButtonClicked;
            m_MainView.SettingsButtonClicked -= OnSettingsButtonClicked;
            m_MainView.HelpToggleChanged -= OnHelpToggleChanged;
            m_MainView.TrackingButtonClicked -= OnTrackingButtonClicked;
            m_MainView.TakesButtonClicked -= OnTakesButtonClicked;
        }

        void OnRecordButtonClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleRecording());
        }

        void OnSettingsButtonClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.OpenSettings());
        }

        void OnHelpToggleChanged(bool value)
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleHelp()
            {
                IsActive = value
            });
        }

        void OnTrackingButtonClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.TrackingClicked());
        }

        void OnTakesButtonClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.TakesClicked());
        }
    }
}
