using System;
using System.IO;
using Unity.LiveCapture.CompanionApp.Networking;
using UnityEngine;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Protocols;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// A class used to communicate with with the Unity editor from the companion apps.
    /// </summary>
    abstract class CompanionAppHost
    {
        /// <summary>
        /// The protocol used when communicating with the host.
        /// </summary>
        protected readonly Protocol m_Protocol;

        readonly BinarySender<DeviceMode> m_DeviceModeSender;
        readonly EventSender m_StartRecordingSender;
        readonly EventSender m_StopRecordingSender;
        readonly EventSender m_StartPlayerSender;
        readonly EventSender m_StopPlayerSender;
        readonly EventSender m_PausePlayerSender;
        readonly BinarySender<double> m_PlayerTimeSender;
        readonly BinarySender<SerializableGuid> m_SetSelectedTakeSender;
        readonly JsonSender<TakeDescriptorV0> m_SetTakeDataSender;
        readonly BinarySender<SerializableGuid> m_DeleteTakeSender;
        readonly BinarySender<SerializableGuid> m_SetIterationBaseSender;
        readonly EventSender m_ClearIterationBase;
        readonly BinarySender<SerializableGuid> m_RequestTexturePreview;

        /// <summary>
        /// An event invoked when the communication starts.
        /// </summary>
        public event Action Initialized;

        /// <summary>
        /// An event invoked when the communication ends.
        /// </summary>
        public event Action SessionEnded;

        /// <summary>
        /// An event invoked when the recording state has been changed.
        /// </summary>
        public event Action<bool> IsRecordingReceived;

        /// <summary>
        /// An event invoked when the server mode has been changed.
        /// </summary>
        public event Action<DeviceMode> ServerModeReceived;

        /// <summary>
        /// An event invoked when the device capture frame rate has been changed.
        /// </summary>
        public event Action<FrameRate> FrameRateReceived;

        /// <summary>
        /// An event invoked when the slate has been changed.
        /// </summary>
        public event Action<bool> HasSlateReceived;

        /// <summary>
        /// An event invoked when the slate's duration has been changed.
        /// </summary>
        public event Action<double> SlateDurationReceived;

        /// <summary>
        /// An event invoked when the slate's preview state has been changed.
        /// </summary>
        public event Action<bool> SlateIsPreviewingReceived;

        /// <summary>
        /// An event invoked when the slate's preview time has been changed.
        /// </summary>
        public event Action<double> SlatePreviewTimeReceived;

        /// <summary>
        /// An event invoked when the index of the selected take has been changed.
        /// </summary>
        public event Action<int> SlateSelectedTakeReceived;

        /// <summary>
        /// An event invoked when the index of the iteration base take has been changed.
        /// </summary>
        public event Action<int> SlateIterationBaseReceived;

        /// <summary>
        /// An event invoked when the take number of the current slate has been changed.
        /// </summary>
        public event Action<int> SlateTakeNumberReceived;

        /// <summary>
        /// An event invoked when the shot name of the current slate has been changed.
        /// </summary>
        public event Action<string> SlateShotNameReceived;

        /// <summary>
        /// An event invoked when the slate takes have been changed.
        /// </summary>
        public event Action<TakeDescriptor[]> SlateTakesReceived;

        /// <summary>
        /// An event invoked when the name the take recorder will use for the next take has changed.
        /// </summary>
        public event Action<string> NextTakeNameReceived;

        /// <summary>
        /// An event invoked when the name the device will use for the next recording has changed.
        /// </summary>
        public event Action<string> NextAssetNameReceived;

        /// <summary>
        /// An event invoked when a texture preview is received.
        /// </summary>
        public event Action<Guid, Texture2D> TexturePreviewReceived;

        /// <summary>
        /// Creates a new <see cref="CompanionAppHost"/> instance.
        /// </summary>
        /// <param name="network">The <see cref="NetworkBase"/> used to send and receive messages.</param>
        /// <param name="remote">The handle of the server on the network.</param>
        /// <param name="stream">A stream containing the protocol received from the server.</param>
        protected CompanionAppHost(NetworkBase network, Remote remote, Stream stream)
        {
            m_Protocol = new Protocol(stream);
            m_Protocol.SetNetwork(network, remote);

            BinarySender<DeviceMode>.TryGet(m_Protocol, CompanionAppMessages.ToServer.SetDeviceMode, out m_DeviceModeSender);
            EventSender.TryGet(m_Protocol, CompanionAppMessages.ToServer.StartRecording, out m_StartRecordingSender);
            EventSender.TryGet(m_Protocol, CompanionAppMessages.ToServer.StopRecording, out m_StopRecordingSender);
            EventSender.TryGet(m_Protocol, CompanionAppMessages.ToServer.PlayerStart, out m_StartPlayerSender);
            EventSender.TryGet(m_Protocol, CompanionAppMessages.ToServer.PlayerStop, out m_StopPlayerSender);
            EventSender.TryGet(m_Protocol, CompanionAppMessages.ToServer.PlayerPause, out m_PausePlayerSender);
            BinarySender<double>.TryGet(m_Protocol, CompanionAppMessages.ToServer.PlayerSetTime, out m_PlayerTimeSender);
            BinarySender<SerializableGuid>.TryGet(m_Protocol, CompanionAppMessages.ToServer.SetSelectedTake, out m_SetSelectedTakeSender);
            JsonSender<TakeDescriptorV0>.TryGet(m_Protocol, CompanionAppMessages.ToServer.SetTakeData_V0, out m_SetTakeDataSender);
            BinarySender<SerializableGuid>.TryGet(m_Protocol, CompanionAppMessages.ToServer.DeleteTake, out m_DeleteTakeSender);
            BinarySender<SerializableGuid>.TryGet(m_Protocol, CompanionAppMessages.ToServer.SetIterationBase, out m_SetIterationBaseSender);
            EventSender.TryGet(m_Protocol, CompanionAppMessages.ToServer.ClearIterationBase, out m_ClearIterationBase);
            BinarySender<SerializableGuid>.TryGet(m_Protocol, CompanionAppMessages.ToServer.RequestTexturePreview, out m_RequestTexturePreview);

            if (EventReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.Initialize, out var initialize))
            {
                initialize.AddHandler(() =>
                {
                    m_Protocol.Reset();
                    Initialized?.Invoke();
                });
            }
            if (EventReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.EndSession, out var endSession))
            {
                endSession.AddHandler(() =>
                {
                    SessionEnded?.Invoke();
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.IsRecordingChanged, out var isRecordingChanged))
            {
                isRecordingChanged.AddHandler(state =>
                {
                    IsRecordingReceived?.Invoke(state);
                });
            }
            if (BinaryReceiver<DeviceMode>.TryGet(m_Protocol, CompanionAppMessages.ToClient.DeviceModeChanged, out var serverModeChanged))
            {
                serverModeChanged.AddHandler(mode =>
                {
                    ServerModeReceived?.Invoke(mode);
                });
            }
            if (BinaryReceiver<FrameRate>.TryGet(m_Protocol, CompanionAppMessages.ToClient.FrameRate, out var frameRateChanged))
            {
                frameRateChanged.AddHandler(rate =>
                {
                    FrameRateReceived?.Invoke(rate);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.HasShotChanged, out var hasSlateChanged))
            {
                hasSlateChanged.AddHandler(hasSlate =>
                {
                    HasSlateReceived?.Invoke(hasSlate);
                });
            }
            if (BinaryReceiver<double>.TryGet(m_Protocol, CompanionAppMessages.ToClient.ShotDurationChanged, out var slateDurationChanged))
            {
                slateDurationChanged.AddHandler(duration =>
                {
                    SlateDurationReceived?.Invoke(duration);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.IsPreviewingChanged, out var slateIsPreviewingChanged))
            {
                slateIsPreviewingChanged.AddHandler(isPreviewing =>
                {
                    SlateIsPreviewingReceived?.Invoke(isPreviewing);
                });
            }
            if (BinaryReceiver<double>.TryGet(m_Protocol, CompanionAppMessages.ToClient.PreviewTimeChanged, out var slatePreviewTimeChanged))
            {
                slatePreviewTimeChanged.AddHandler(duration =>
                {
                    SlatePreviewTimeReceived?.Invoke(duration);
                });
            }
            if (BinaryReceiver<int>.TryGet(m_Protocol, CompanionAppMessages.ToClient.SelectedTake, out var slateSelectedTakeChanged))
            {
                slateSelectedTakeChanged.AddHandler(selectedTake =>
                {
                    SlateSelectedTakeReceived?.Invoke(selectedTake);
                });
            }
            if (BinaryReceiver<int>.TryGet(m_Protocol, CompanionAppMessages.ToClient.IterationBase, out var slateIterationBaseChanged))
            {
                slateIterationBaseChanged.AddHandler(iterationBase =>
                {
                    SlateIterationBaseReceived?.Invoke(iterationBase);
                });
            }
            if (BinaryReceiver<int>.TryGet(m_Protocol, CompanionAppMessages.ToClient.SlateTakeNumber, out var slateTakeNumberChanged))
            {
                slateTakeNumberChanged.AddHandler(takeNumber =>
                {
                    SlateTakeNumberReceived?.Invoke(takeNumber);
                });
            }
            if (StringReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.SlateShotName, out var slateShotNameChanged))
            {
                slateShotNameChanged.AddHandler(shotName =>
                {
                    SlateShotNameReceived?.Invoke(shotName);
                });
            }
            if (JsonReceiver<TakeDescriptorArrayV0>.TryGet(m_Protocol, CompanionAppMessages.ToClient.TakeList_V0, out var slateTakesChanged))
            {
                slateTakesChanged.AddHandler(takes =>
                {
                    SlateTakesReceived?.Invoke((TakeDescriptor[])takes);
                });
            }
            if (StringReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.NextTakeName, out var nextTakeNameChanged))
            {
                nextTakeNameChanged.AddHandler(name =>
                {
                    NextTakeNameReceived?.Invoke(name);
                });
            }
            if (StringReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.NextAssetName, out var nextAssetNameChanged))
            {
                nextAssetNameChanged.AddHandler(name =>
                {
                    NextAssetNameReceived?.Invoke(name);
                });
            }
            if (TextureReceiver.TryGet(m_Protocol, CompanionAppMessages.ToClient.TexturePreview, out var texturePreviewReceived))
            {
                texturePreviewReceived.AddHandler((data) =>
                {
                    var guid = Guid.Parse(data.metadata);
                    var texture = data.texture;

                    TexturePreviewReceived?.Invoke(guid, texture);
                });
            }
        }

        /// <summary>
        /// Sets the server mode.
        /// </summary>
        /// <param name="mode">The server mode to set.</param>
        public void SetServerMode(DeviceMode mode)
        {
            if (m_DeviceModeSender != null)
            {
                m_DeviceModeSender.Send(mode);
            }
        }

        /// <summary>
        /// Starts recording a take.
        /// </summary>
        public void StartRecording()
        {
            if (m_StartRecordingSender != null)
            {
                m_StartRecordingSender.Send();
            }
        }

        /// <summary>
        /// Stops recording the current take.
        /// </summary>
        public void StopRecording()
        {
            if (m_StopRecordingSender != null)
            {
                m_StopRecordingSender.Send();
            }
        }

        /// <summary>
        /// Starts playback of the current take.
        /// </summary>
        public void StartPlayer()
        {
            if (m_StartPlayerSender != null)
            {
                m_StartPlayerSender.Send();
            }
        }

        /// <summary>
        /// Stops playback of the current take.
        /// </summary>
        public void StopPlayer()
        {
            if (m_StopPlayerSender != null)
            {
                m_StopPlayerSender.Send();
            }
        }

        /// <summary>
        /// Pauses playback of the current take.
        /// </summary>
        public void PausePlayer()
        {
            if (m_PausePlayerSender != null)
            {
                m_PausePlayerSender.Send();
            }
        }

        /// <summary>
        /// Sets the playback time in the current take.
        /// </summary>
        /// <param name="time">The time to set in seconds since the start of the take.</param>
        public void SetPlayerTime(double time)
        {
            if (m_PlayerTimeSender != null)
            {
                m_PlayerTimeSender.Send(time);
            }
        }

        /// <summary>
        /// Requests to select a take using its guid.
        /// </summary>
        /// <param name="guid">The guid of the take to select.</param>
        public void SetSelectedTake(Guid guid)
        {
            if (m_SetSelectedTakeSender != null)
            {
                m_SetSelectedTakeSender.Send(guid);
            }
        }

        /// <summary>
        /// Requests to change take metadata.
        /// </summary>
        /// <param name="descriptor">The take descriptor containing the new metadata to set.</param>
        public void SetTakeData(TakeDescriptor descriptor)
        {
            if (m_SetTakeDataSender != null)
            {
                m_SetTakeDataSender.Send((TakeDescriptorV0)descriptor);
            }
        }

        /// <summary>
        /// Requests to delete a take using its guid.
        /// </summary>
        /// <param name="guid">The guid of the take to delete.</param>
        public void DeleteTake(Guid guid)
        {
            if (m_DeleteTakeSender != null)
            {
                m_DeleteTakeSender.Send(guid);
            }
        }

        /// <summary>
        /// Requests to set a take as an iteration base using its guid.
        /// </summary>
        /// <param name="guid">The guid of the take to set as iteration base.</param>
        public void SetIterationBase(Guid guid)
        {
            if (m_SetIterationBaseSender != null)
            {
                m_SetIterationBaseSender.Send(guid);
            }
        }

        /// <summary>
        /// Requests to clear the current iteration base.
        /// </summary>
        public void ClearIterationBase()
        {
            if (m_ClearIterationBase != null)
            {
                m_ClearIterationBase.Send();
            }
        }

        /// <summary>
        /// Requests a texture preview using the asset's guid.
        /// </summary>
        /// <param name="guid">The guid of the asset to get the preview from.</param>

        public void RequestTexturePreview(Guid guid)
        {
            if (m_RequestTexturePreview != null)
            {
                m_RequestTexturePreview.Send(guid);
            }
        }
    }
}
