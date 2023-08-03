using System;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ResetViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IResetView m_ResetView;
        [Inject]
        SettingsModel m_Settings;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_ResetView.onResetPoseClicked += OnResetPose;
            m_ResetView.onResetLensClicked += OnResetLens;
            m_ResetView.onRebaseToggled += OnToggleRebase;
        }

        public void Dispose()
        {
            m_ResetView.onResetPoseClicked -= OnResetPose;
            m_ResetView.onResetLensClicked -= OnResetLens;
            m_ResetView.onRebaseToggled -= OnToggleRebase;
        }

        void OnToggleRebase() => m_SignalBus.Fire(new SendHostSignal
        {
            Type = HostMessageType.Rebasing,
            BoolValue = !m_Settings.Rebasing
        });

        void OnResetLens() => m_SignalBus.Fire(new ResetLensSignal());

        void OnResetPose() => m_SignalBus.Fire(new SendHostSignal()
        {
            Type = HostMessageType.SetPoseToOrigin
        });
    }
}
