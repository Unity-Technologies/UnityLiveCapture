using System;
using System.Collections.Generic;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class SettingsController : IInitializable, IDisposable
    {
        [Inject(Id = "AppName")]
        string m_AppName;

        [Inject]
        Settings m_Settings;

        [Inject]
        List<ISettingsPropertyListener> m_SettingsPropertyListeners;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_SignalBus.Subscribe<SettingsSignals.UpdateProperty>(OnSettingsChanged);
            m_SignalBus.Subscribe<SettingsSignals.Reset>(OnReset);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<SettingsSignals.UpdateProperty>(OnSettingsChanged);
            m_SignalBus.Unsubscribe<SettingsSignals.Reset>(OnReset);
        }

        public void NotifyAllProperties()
        {
            NotifyListerners(SettingsProperty.AutoHideUI);
            NotifyListerners(SettingsProperty.CameraPassthrough);
            NotifyListerners(SettingsProperty.DimScreen);
            NotifyListerners(SettingsProperty.FlipHorizontally);
            NotifyListerners(SettingsProperty.RecordVideo);
            NotifyListerners(SettingsProperty.RecordAudio);
            NotifyListerners(SettingsProperty.CountdownEnabled);
            NotifyListerners(SettingsProperty.CountdownTime);
            NotifyListerners(SettingsProperty.FaceWireframe);
            NotifyListerners(SettingsProperty.ShowRecordButton);
            NotifyListerners(SettingsProperty.TimecodeSourceId);
            NotifyListerners(SettingsProperty.CalibrationPose);
        }

        void OnSettingsChanged(SettingsSignals.UpdateProperty signal)
        {
            var property = signal.SettingsUpdater.Property;
            var updater = signal.SettingsUpdater;

            if (property == SettingsProperty.RecordAudio
                && !m_Settings.RecordAudio
                && updater.Changes(m_Settings)
                && !IOSHelper.HasAudioPermission())
            {
                m_SignalBus.Fire(new PermissionsSignals.MicrophoneRequest(m_AppName));
            }
            else
            {
                updater.Update(m_Settings);
            }

            NotifyListerners(property);
        }

        void OnReset()
        {
            m_Settings.RestoreDefaults();

            NotifyAllProperties();
        }

        void NotifyListerners(SettingsProperty property)
        {
            foreach (var listener in m_SettingsPropertyListeners)
            {
                listener.SettingsPropertyChanged(property, m_Settings);
            }
        }
    }
}
