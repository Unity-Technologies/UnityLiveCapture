using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class CompanionAppHostController : ICompanionAppHost, IInitializable, IDisposable
    {
        [Inject]
        List<ISessionStateListener> m_SessionStateListeners;
        [Inject]
        List<IDeviceModeListener> m_DeviceModeListeners;
        [Inject]
        List<IRecordingStateListener> m_RecordingStateListeners;
        [Inject]
        List<IPlayStateListener> m_PlayStateListeners;
        [Inject]
        List<ITimeListener> m_TimeListeners;
        [Inject]
        List<ITakeDescriptorsListener> m_TakeDescriptorsListeners;
        [Inject]
        List<ITakeSelectionListener> m_TakeSelectionListeners;
        [Inject]
        List<ISlateIterationBaseListener> m_SlateIterationBaseListeners;
        [Inject]
        List<ISlateTakeNumberListener> m_SlateTakeNumberListeners;
        [Inject]
        List<ISlateShotNameListener> m_SlateShotNameListeners;
        [Inject]
        List<ITexturePreviewListener> m_TexturePreviewListeners;
        [Inject]
        SignalBus m_SignalBus;
        CompanionAppHost m_Host;

        public bool IsSessionActive { get; private set; }
        public bool IsRecording { get; private set; }
        public DeviceMode DeviceMode { get; private set; }
        public FrameRate FrameRate { get; private set; }
        public bool HasSlate { get; private set; }
        public double SlateDuration { get; private set; }
        public bool SlateIsPlaying { get; private set; }
        public double SlatePreviewTime { get; private set; }
        public int SlateTakeNumber { get; private set; }
        public string SlateShotName { get; private set; }
        public TakeDescriptor[] SlateTakes { get; private set; }
        public int SlateSelectedTake { get; private set; }
        public int SlateIterationBase { get; private set; }
        public string NextTakeName { get; private set; }
        public string NextAssetName { get; private set; }

        public void Initialize()
        {
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        public void StartRecording()
        {
            if (m_Host != null)
            {
                m_Host.StartRecording();
            }
        }

        public void StopRecording()
        {
            if (m_Host != null)
            {
                m_Host.StopRecording();
            }
        }

        public void SetDeviceMode(DeviceMode value)
        {
            if (m_Host != null)
            {
                m_Host.SetServerMode(value);
            }
        }

        public void SetPlaybackTime(double value)
        {
            if (m_Host != null)
            {
                m_Host.SetPlayerTime(value);
            }
        }

        public void StartPlayback()
        {
            if (m_Host != null)
            {
                m_Host.StartPlayer();
            }
        }

        public void PausePlayback()
        {
            if (m_Host != null)
            {
                m_Host.PausePlayer();
            }
        }

        public void SelectTake(Guid guid)
        {
            if (m_Host != null)
            {
                m_Host.SetSelectedTake(guid);
            }
        }

        public void SetIterationBase(Guid guid)
        {
            if (m_Host != null)
            {
                m_Host.SetIterationBase(guid);
            }
        }

        public void ClearIterationBase()
        {
            if (m_Host != null)
            {
                m_Host.ClearIterationBase();
            }
        }

        public void DeleteTake(Guid guid)
        {
            if (m_Host != null)
            {
                m_Host.DeleteTake(guid);
            }
        }

        public void UpdateTake(TakeDescriptor descriptor)
        {
            if (m_Host != null)
            {
                m_Host.SetTakeData(descriptor);
            }
        }

        public void RequestTexturePreview(Guid guid)
        {
            if (m_Host != null)
            {
                m_Host.RequestTexturePreview(guid);
            }
        }

        void OnConnected(RemoteConnectedSignal signal)
        {
            m_Host = signal.companionAppHost;

            m_Host.Initialized += OnInitialize;
            m_Host.SessionEnded += OnSessionEnded;
            m_Host.IsRecordingReceived += OnRecordingReceived;
            m_Host.ServerModeReceived += OnServerModeReceived;
            m_Host.FrameRateReceived += OnFrameRateReceived;
            m_Host.HasSlateReceived += OnHasSlateReceived;
            m_Host.SlateDurationReceived += OnSlateDurationReceived;
            m_Host.SlateIsPreviewingReceived += OnSlateIsPreviewingReceived;
            m_Host.SlatePreviewTimeReceived += OnSlatePreviewTimeReceived;
            m_Host.SlateTakesReceived += OnSlateTakesReceived;
            m_Host.SlateIterationBaseReceived += OnSlateIterationBaseReceived;
            m_Host.SlateTakeNumberReceived += SlateTakeNumberReceived;
            m_Host.SlateShotNameReceived += SlateShotNameReceived;
            m_Host.SlateSelectedTakeReceived += OnSlateSelectedTakeReceived;
            m_Host.TexturePreviewReceived += OnTexturePreviewReceived;
            m_Host.NextTakeNameReceived += OnNextTakeNameReceived;
            m_Host.NextAssetNameReceived += OnNextAssetNameReceived;
        }

        void OnDisconnected()
        {
            if (m_Host != null)
            {
                m_Host.Initialized -= OnInitialize;
                m_Host.SessionEnded -= OnSessionEnded;
                m_Host.IsRecordingReceived -= OnRecordingReceived;
                m_Host.ServerModeReceived -= OnServerModeReceived;
                m_Host.FrameRateReceived -= OnFrameRateReceived;
                m_Host.HasSlateReceived -= OnHasSlateReceived;
                m_Host.SlateDurationReceived -= OnSlateDurationReceived;
                m_Host.SlateIsPreviewingReceived -= OnSlateIsPreviewingReceived;
                m_Host.SlatePreviewTimeReceived -= OnSlatePreviewTimeReceived;
                m_Host.SlateTakesReceived -= OnSlateTakesReceived;
                m_Host.SlateIterationBaseReceived -= OnSlateIterationBaseReceived;
                m_Host.SlateTakeNumberReceived -= SlateTakeNumberReceived;
                m_Host.SlateShotNameReceived -= SlateShotNameReceived;
                m_Host.SlateSelectedTakeReceived -= OnSlateSelectedTakeReceived;
                m_Host.TexturePreviewReceived -= OnTexturePreviewReceived;
                m_Host.NextTakeNameReceived -= OnNextTakeNameReceived;
                m_Host.NextAssetNameReceived -= OnNextAssetNameReceived;
            }

            m_Host = null;

            if (IsSessionActive)
            {
                OnSessionEnded();
            }
        }

        void OnInitialize()
        {
            IsSessionActive = true;

            NotifyListeners(m_SessionStateListeners, l => l.SetSessionState(true));
        }

        void OnSessionEnded()
        {
            IsSessionActive = false;

            NotifyListeners(m_SessionStateListeners, l => l.SetSessionState(false));
        }

        void OnRecordingReceived(bool value)
        {
            IsRecording = value;

            NotifyListeners(m_RecordingStateListeners, l => l.SetRecordingState(value));
        }

        void OnServerModeReceived(DeviceMode value)
        {
            DeviceMode = value;

            NotifyListeners(m_DeviceModeListeners, l => l.SetDeviceMode(value));
        }

        void OnHasSlateReceived(bool value)
        {
            HasSlate = value;
        }

        void OnSlateIsPreviewingReceived(bool value)
        {
            SlateIsPlaying = value;

            NotifyListeners(m_PlayStateListeners, l => l.SetPlayState(value));
        }

        void OnSlatePreviewTimeReceived(double value)
        {
            SlatePreviewTime = value;

            NotifyListeners(m_TimeListeners, l => l.SetTime(value, SlateDuration, FrameRate));
        }

        void OnSlateDurationReceived(double value)
        {
            SlateDuration = value;

            NotifyListeners(m_TimeListeners, l => l.SetTime(SlatePreviewTime, value, FrameRate));
        }

        void OnFrameRateReceived(FrameRate value)
        {
            FrameRate = value;

            NotifyListeners(m_TimeListeners, l => l.SetTime(SlatePreviewTime, SlateDuration, value));
        }

        void OnSlateTakesReceived(TakeDescriptor[] descriptors)
        {
            SlateTakes = descriptors;

            NotifyListeners(m_TakeDescriptorsListeners, l => l.SetTakeDescriptors(SlateTakes));
        }

        void OnSlateSelectedTakeReceived(int value)
        {
            SlateSelectedTake = value;

            NotifyListeners(m_TakeSelectionListeners, l => l.SetSelectedTake(SlateSelectedTake));
        }

        void OnSlateIterationBaseReceived(int value)
        {
            SlateIterationBase = value;

            NotifyListeners(m_SlateIterationBaseListeners, l => l.SetSlateIterationBase(SlateIterationBase));
        }

        void SlateTakeNumberReceived(int value)
        {
            SlateTakeNumber = value;

            NotifyListeners(m_SlateTakeNumberListeners, l => l.SetSlateTakeNumber(SlateTakeNumber));
        }

        void SlateShotNameReceived(string value)
        {
            SlateShotName = value;

            NotifyListeners(m_SlateShotNameListeners, l => l.SetSlateShotName(SlateShotName));
        }

        void OnTexturePreviewReceived(Guid guid, Texture2D texture)
        {
            NotifyListeners(m_TexturePreviewListeners, l => l.SetTexturePreview(guid, texture));
        }

        void OnNextTakeNameReceived(string value)
        {
            NextTakeName = value;
        }

        void OnNextAssetNameReceived(string value)
        {
            NextAssetName = value;
        }

        void NotifyListeners<T>(IEnumerable<T> listeners, Action<T> action)
        {
            foreach (var listener in listeners)
            {
                action.Invoke(listener);
            }
        }
    }
}
