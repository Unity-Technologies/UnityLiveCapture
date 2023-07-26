using System;
using Unity.Collections;
using Unity.LiveCapture.ARKitFaceCapture;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class CalibrationController : IInitializable, IDisposable, ISettingsPropertyListener
    {
        [Inject]
        IFaceTracker m_FaceTracker;

        [Inject]
        ICameraSnapshotSystem m_CameraSnapshotSystem;

        [Inject]
        IBackgroundOverlayView m_BackgroundOverlayView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_SignalBus.Subscribe<CalibrationViewSignals.PerformCalibration>(OnPerformCalibration);
            m_SignalBus.Subscribe<CalibrationViewSignals.ClearCalibration>(OnClearCalibration);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<CalibrationViewSignals.PerformCalibration>(OnPerformCalibration);
            m_SignalBus.Unsubscribe<CalibrationViewSignals.ClearCalibration>(OnClearCalibration);
        }

        public void SettingsPropertyChanged(SettingsProperty property, ISettings settings)
        {
            if (property == SettingsProperty.CalibrationPose)
            {
                var calibrationPose = settings.CalibrationPose;
                if (calibrationPose.HasValue)
                {
                    m_FaceTracker.SetCalibrationPose(calibrationPose.Value.Pose);
                }
                else
                {
                    m_FaceTracker.ClearCalibrationPose();
                }
            }
        }

        void OnPerformCalibration()
        {
            if (m_FaceTracker.IsTracking)
            {
                m_FaceTracker.ConsumeFacePose(out var pose, false);
                var calibrationPose = new CalibrationPose()
                {
                    Pose = pose
                };

                if (m_CameraSnapshotSystem.Enabled &&
                    m_CameraSnapshotSystem.IsSupported &&
                    m_CameraSnapshotSystem.TryGetSnapshot(Allocator.Temp, out var texture))
                {
                    m_BackgroundOverlayView.Texture = texture;

                    m_SignalBus.Fire(new CalibrationViewSignals.ConfirmCalibration()
                    {
                        value = calibrationPose
                    });
                }
            }
            else
            {
                m_SignalBus.Fire<CalibrationViewSignals.CalibrationFailed>();
            }
        }

        void OnClearCalibration()
        {
            m_SignalBus.Fire(SettingsSignals.UpdateProperty.CreateCalibrationPose(SettingsProperty.CalibrationPose, null));
        }
    }
}
