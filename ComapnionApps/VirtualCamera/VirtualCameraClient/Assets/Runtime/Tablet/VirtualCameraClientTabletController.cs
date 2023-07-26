using System;
using System.Collections;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class VirtualCameraClientTabletController : IInitializable, IDisposable
    {
        [Inject]
        StateModel m_State;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        IConnectionScreenView m_ConnectionScreenView;
        [Inject]
        IMainView m_MainView;
        [Inject]
        INotificationView m_NotificationView;
        [Inject]
        ICoroutineRunner m_Runner;
        [Inject]
        SignalBus m_SignalBus;

        Coroutine m_SwitchToMainViewCoroutine;

        public void Initialize()
        {
            m_SignalBus.Subscribe<MainViewSignals.ToggleDeviceMode>(OnToggleDeviceMode);
            m_SignalBus.Subscribe<MainViewSignals.ToggleRecording>(OnToggleRecording);
            m_SignalBus.Subscribe<PermissionsSignals.CameraGranted>(OnCameraGranted);
            m_SignalBus.Subscribe<ConnectionScreenViewSignals.Close>(OnConnectionScreenClose);
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleDeviceMode>(OnToggleDeviceMode);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleRecording>(OnToggleRecording);
            m_SignalBus.Unsubscribe<PermissionsSignals.CameraGranted>(OnCameraGranted);
            m_SignalBus.Unsubscribe<ConnectionScreenViewSignals.Close>(OnConnectionScreenClose);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnDisconnected);

            if (!SceneState.IsBeingDestroyed)
            {
                StopSwitchToMainViewCoroutine();
            }
        }

        void OnToggleDeviceMode()
        {
            var newMode = m_CompanionApp.DeviceMode == DeviceMode.LiveStream
                ? DeviceMode.Playback
                : DeviceMode.LiveStream;

            m_CompanionApp.SetDeviceMode(newMode);
        }

        void OnToggleRecording()
        {
            if (!m_State.IsHelpMode)
            {
                if (m_CompanionApp.IsRecording)
                {
                    m_SignalBus.Fire(new RequestStopRecordingSignal());
                }
                else
                {
                    m_SignalBus.Fire(new RequestStartRecordingSignal());
                }
            }
        }

        void OnCameraGranted()
        {
            m_ConnectionScreenView.Show();
            m_NotificationView.Show();
        }

        void OnConnectionScreenClose()
        {
            m_ConnectionScreenView.Hide();
            m_MainView.Show();
        }

        void OnDisconnected() => StopSwitchToMainViewCoroutine();

        void OnConnected()
        {
            // Exit connection screen on successful connection, with a delay.
            if (m_ConnectionScreenView.IsShown)
            {
                // Make sure we don't have multiple coroutines running.
                StopSwitchToMainViewCoroutine();
                m_SwitchToMainViewCoroutine = m_Runner.StartCoroutine(SwitchToMainView());
            }
        }

        IEnumerator SwitchToMainView()
        {
            yield return new WaitForSeconds(1);

            // Close the connection screen if it is still opened.
            if (m_ConnectionScreenView.IsShown)
            {
                OnConnectionScreenClose();
            }
        }

        void StopSwitchToMainViewCoroutine()
        {
            if (m_SwitchToMainViewCoroutine != null)
            {
                m_Runner.StopCoroutine(m_SwitchToMainViewCoroutine);
            }
        }
    }
}
