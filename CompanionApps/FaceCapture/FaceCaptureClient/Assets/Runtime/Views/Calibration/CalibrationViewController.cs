using System;
using System.Collections;
using Unity.CompanionAppCommon;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class CalibrationViewController : IInitializable, IDisposable, ISettingsPropertyListener
    {
        [Inject]
        ICalibrationView m_CalibrationView;

        [Inject(Id = FaceClientViewId.CalibrationCountdown)]
        ICountdownView m_CountdownView;

        [Inject]
        ICoroutineRunner m_Runner;

        [Inject]
        ICameraSnapshotSystem m_CameraSnapshotSystem;

        [Inject]
        SignalBus m_SignalBus;

        ICalibrationView.State State => m_CalibrationView.ViewState;

        Coroutine m_CountdownCoroutine;

        CalibrationPose m_CalibrationPoseCandidate;

        public void Initialize()
        {
            m_SignalBus.Subscribe<CalibrationViewSignals.CalibrationFailed>(OnCalibrationFailed);
            m_SignalBus.Subscribe<CalibrationViewSignals.CalibrationSucceeded>(OnCalibrationSucceeded);
            m_SignalBus.Subscribe<CalibrationViewSignals.ConfirmCalibration>(OnConfirmCalibration);

            m_CalibrationView.CalibrateButtonClicked += OnCalibrateButtonClicked;
            m_CalibrationView.RecordButtonClicked += OnRecordButtonClicked;
            m_CalibrationView.CalibrateMenuButtonClicked += OnCalibrateMenuButtonClicked;
            m_CalibrationView.ClearMenuButtonClicked += OnClearMenuButtonClicked;
            m_CalibrationView.CancelButtonClicked += OnCancelButtonClicked;
            m_CalibrationView.DoneButtonClicked += OnDoneButtonClicked;
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<CalibrationViewSignals.CalibrationFailed>(OnCalibrationFailed);
            m_SignalBus.Unsubscribe<CalibrationViewSignals.CalibrationSucceeded>(OnCalibrationSucceeded);
            m_SignalBus.Unsubscribe<CalibrationViewSignals.ConfirmCalibration>(OnConfirmCalibration);

            m_CalibrationView.CalibrateButtonClicked -= OnCalibrateButtonClicked;
            m_CalibrationView.RecordButtonClicked -= OnRecordButtonClicked;
            m_CalibrationView.CalibrateMenuButtonClicked -= OnCalibrateMenuButtonClicked;
            m_CalibrationView.ClearMenuButtonClicked -= OnClearMenuButtonClicked;
            m_CalibrationView.CancelButtonClicked -= OnCancelButtonClicked;
            m_CalibrationView.DoneButtonClicked -= OnDoneButtonClicked;
        }

        public void SettingsPropertyChanged(SettingsProperty property, ISettings settings)
        {
            if (property == SettingsProperty.CalibrationPose)
            {
                m_CalibrationView.IsCalibrated = settings.CalibrationPose.HasValue;
            }
        }

        void OnCalibrationFailed()
        {
            m_SignalBus.Fire(new ShowNotificationSignal()
            {
                value = "Face not detected. Try again."
            });
        }

        void OnCalibrationSucceeded()
        {
            m_SignalBus.Fire(new ShowNotificationSignal()
            {
                value = "Calibration successful."
            });

            m_CalibrationView.ViewState = ICalibrationView.State.MenuClosed;
        }

        void OnCalibrateButtonClicked()
        {
            switch (State)
            {
                case ICalibrationView.State.MenuClosed:
                    m_CalibrationView.ViewState = ICalibrationView.State.MenuOpen;
                    break;

                case ICalibrationView.State.MenuOpen:
                    m_CalibrationView.ViewState = ICalibrationView.State.MenuClosed;
                    break;

                case ICalibrationView.State.Calibrating:
                    m_CalibrationView.ViewState = ICalibrationView.State.MenuClosed;
                    DisableCountdown();
                    break;
            }
        }

        void OnRecordButtonClicked()
        {
            var wasPlaying = m_CountdownView.IsPlaying;

            DisableCountdown();

            if (!wasPlaying)
            {
                m_CountdownCoroutine = m_Runner.StartCoroutine(CalibrationCoroutine());
            }
        }

        void OnCalibrateMenuButtonClicked()
        {
            m_CalibrationView.ViewState = ICalibrationView.State.Calibrating;
        }

        void OnClearMenuButtonClicked()
        {
            m_CalibrationView.ViewState = ICalibrationView.State.MenuClosed;

            m_SignalBus.Fire(new ShowNotificationSignal()
            {
                value = "Calibration cleared."
            });

            m_SignalBus.Fire(new CalibrationViewSignals.ClearCalibration());
        }

        void OnCancelButtonClicked()
        {
            m_CalibrationView.ViewState = ICalibrationView.State.Calibrating;
        }

        void OnDoneButtonClicked()
        {
            m_CalibrationView.ViewState = ICalibrationView.State.MenuClosed;

            m_SignalBus.Fire(new ShowNotificationSignal()
            {
                value = "Calibration successful."
            });

            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateCalibrationPose(SettingsProperty.CalibrationPose, m_CalibrationPoseCandidate));
        }

        void OnConfirmCalibration(CalibrationViewSignals.ConfirmCalibration signal)
        {
            m_CalibrationPoseCandidate = signal.value;
            m_CalibrationView.ViewState = ICalibrationView.State.Reviewing;
        }

        void DisableCountdown()
        {
            m_CountdownView.Hide();
            m_CountdownView.StopCountdown();

            if (m_CountdownCoroutine != null)
            {
                m_Runner.StopCoroutine(m_CountdownCoroutine);
                m_CountdownCoroutine = null;
            }
        }

        IEnumerator CalibrationCoroutine()
        {
            m_CountdownView.Show();
            m_CountdownView.PlayCountdown(3);

            while (m_CountdownView.IsPlaying)
            {
                yield return null;
            }

            DisableCountdown();

            m_SignalBus.Fire(new CalibrationViewSignals.PerformCalibration());
        }
    }
}
