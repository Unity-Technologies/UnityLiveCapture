using System;
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// A type of <see cref="LiveCaptureDevice"/> that uses a <see cref="ICompanionAppClient"/> for communication.
    /// </summary>
    interface ICompanionAppDevice
    {
        /// <summary>
        /// Clears the client assigned to this device.
        /// </summary>
        void ClearClient();
    }

    /// <summary>
    /// A type of <see cref="LiveCaptureDevice"/> that uses a <see cref="ICompanionAppClient"/> for communication.
    /// </summary>
    /// <typeparam name="TClient">The type of client this device communicates with.</typeparam>
    public abstract class CompanionAppDevice<TClient> : LiveCaptureDevice, ICompanionAppDevice
        where TClient : class, ICompanionAppClient
    {
        bool m_ClientRegistered;
        TClient m_Client;
        readonly ShotChangeTracker m_ShotChangeTracker = new ShotChangeTracker();
        readonly TakeNameFormatter m_TakeNameFormatter = new TakeNameFormatter();
        string m_LastAssetName;
        PropertyPreviewer m_Previewer;
        internal IAssetManager m_AssetManager = AssetManager.Instance;

        bool TryGetInternalClient(out ICompanionAppClientInternal client)
        {
            client = m_Client as ICompanionAppClientInternal;

            return client != null;
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            CompanionAppServer.ClientDisconnected += OnClientDisconnected;

            RegisterClient();
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled.
        /// </summary>
        /// <remaks>
        /// This is also called when the object is destroyed and can be used for any cleanup code.
        ///  When scripts are reloaded after compilation has finished, OnDisable will be called, followed by an OnEnable after the script has been loaded.
        /// </remaks>
        protected override void OnDisable()
        {
            base.OnDisable();

            CompanionAppServer.ClientDisconnected -= OnClientDisconnected;
            CompanionAppServer.DeregisterClientConnectHandler(OnClientConnected);

            OnStopRecording();
            UnregisterClient();
        }

        /// <summary>
        /// This function is called when the behaviour gets destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            ClearClient();
        }

        /// <inheritdoc/>
        public override bool IsReady()
        {
            return m_Client != null;
        }

        /// <inheritdoc/>
        protected override void OnStartRecording()
        {
            SendRecordingState();
        }

        /// <inheritdoc/>
        protected override void OnStopRecording()
        {
            SendRecordingState();
        }

        /// <summary>
        /// Gets the client currently assigned to this device.
        /// </summary>
        /// <returns>The assigned client, or null if none is assigned.</returns>
        public TClient GetClient()
        {
            return m_Client;
        }

        /// <summary>
        /// Assigns a client to this device.
        /// </summary>
        /// <param name="client">The client to assign, or null to clear the assigned client.</param>
        /// <param name="rememberAssignment">Try to auto-assign the client to this device when it reconnects in the future.</param>
        public void SetClient(TClient client, bool rememberAssignment)
        {
            if (m_Client != client)
            {
                UnregisterClient();

                if (m_Client != null)
                {
                    ClientMappingDatabase.DeregisterClientAssociation(this, m_Client, rememberAssignment);
                }

                m_Client = client;

                if (m_Client != null)
                {
                    // if any device is also using this client, we must clear the client from the previous device.
                    if (ClientMappingDatabase.TryGetDevice(client, out var previousDevice))
                    {
                        previousDevice.ClearClient();
                    }

                    ClientMappingDatabase.RegisterClientAssociation(this, m_Client, rememberAssignment);
                }

                RegisterClient();
            }
        }

        void RegisterClient()
        {
            if (!isActiveAndEnabled || m_ClientRegistered)
            {
                return;
            }

            CompanionAppServer.DeregisterClientConnectHandler(OnClientConnected);

            m_ShotChangeTracker.Reset();

            if (TryGetInternalClient(out var client))
            {
                client.SetDeviceMode += ClientSetDeviceMode;
                client.StartRecording += ClientStartRecording;
                client.StopRecording += ClientStopRecording;
                client.StartPlayer += ClientStartPlayer;
                client.StopPlayer += ClientStopPlayer;
                client.PausePlayer += ClientPausePlayer;
                client.SetPlayerTime += ClientSetPlayerTime;
                client.SetSelectedTake += ClientSetSelectedTake;
                client.SetTakeData += ClientSetTakeData;
                client.DeleteTake += ClientDeleteTake;
                client.SetIterationBase += ClientSetIterationBase;
                client.ClearIterationBase += ClientClearIterationBase;
                client.TexturePreviewRequested += OnTexturePreviewRequested;

                RegisterLiveProperties();
                OnClientAssigned();

                client.SendInitialize();

                UpdateClient();

                m_ClientRegistered = true;
            }
            else
            {
                ClientMappingDatabase.TryGetClientAssignment(this, out var clientName, out var time);
                CompanionAppServer.RegisterClientConnectHandler(OnClientConnected, clientName, time);
            }
        }

        void UnregisterClient()
        {
            if (!m_ClientRegistered)
            {
                return;
            }

            if (TryGetInternalClient(out var client))
            {
                OnClientUnassigned();
                RestoreLiveProperties();

                client.SendEndSession();
                client.SetDeviceMode -= ClientSetDeviceMode;
                client.StartRecording -= ClientStartRecording;
                client.StopRecording -= ClientStopRecording;
                client.StartPlayer -= ClientStartPlayer;
                client.StopPlayer -= ClientStopPlayer;
                client.PausePlayer -= ClientPausePlayer;
                client.SetPlayerTime -= ClientSetPlayerTime;
                client.SetSelectedTake -= ClientSetSelectedTake;
                client.SetTakeData -= ClientSetTakeData;
                client.DeleteTake -= ClientDeleteTake;
                client.SetIterationBase -= ClientSetIterationBase;
                client.ClearIterationBase -= ClientClearIterationBase;
                client.TexturePreviewRequested -= OnTexturePreviewRequested;

                m_ClientRegistered = false;
            }
        }

        /// <inheritdoc />
        public void ClearClient()
        {
            SetClient(null, true);
        }

        /// <summary>
        /// Called to send the device state to the client.
        /// </summary>
        public virtual void UpdateClient()
        {
            SendDeviceState(TakeRecorder.IsLive);

            var nShot = TakeRecorder.Shot;
            var hasShot = nShot.HasValue;
            var shot = nShot.GetValueOrDefault();
            var shotChanged = m_ShotChangeTracker.Changed(nShot);
            var take = nShot?.Take;

            var assetName = GetAssetName();
            var assetNameChanged = assetName != m_LastAssetName;
            m_LastAssetName = assetName;

            if (TryGetInternalClient(out var client))
            {
                var isPlaying = TakeRecorder.IsPreviewPlaying();

                client.SendFrameRate(TakeRecorder.IsLive || take == null ? TakeRecorder.FrameRate : take.FrameRate);
                client.SendHasShot(hasShot);
                client.SendShotDuration(TakeRecorder.GetPreviewDuration());
                client.SendIsPreviewing(isPlaying);

                if (!isPlaying && IsRecording)
                {
                    client.SendPreviewTime(TakeRecorder.GetRecordingElapsedTime());
                }
                else
                {
                    client.SendPreviewTime(TakeRecorder.GetPreviewTime());
                }

                if (shotChanged || assetNameChanged)
                {
                    if (hasShot)
                    {
                        m_TakeNameFormatter.ConfigureTake(shot.SceneNumber, shot.Name, shot.TakeNumber);
                    }
                    else
                        m_TakeNameFormatter.ConfigureTake(0, "Shot", 0);

                    client.SendNextTakeName(m_TakeNameFormatter.GetTakeName());
                    client.SendNextAssetName(m_TakeNameFormatter.GetAssetName());
                }
            }

            if (shotChanged)
            {
                SendShotDescriptor(nShot);
            }

            SendRecordingState();
        }

        /// <summary>
        /// Gets the name used for the take asset name.
        /// </summary>
        /// <returns>The name of the asset.</returns>
        protected virtual string GetAssetName() { return name; }

        /// <summary>
        /// The device calls this method when a new client is assigned.
        /// </summary>
        protected virtual void OnClientAssigned() { }

        /// <summary>
        /// The device calls this method when the client is unassigned.
        /// </summary>
        protected virtual void OnClientUnassigned() { }

        /// <summary>
        /// The device calls this method when the shot has changed.
        /// </summary>
        /// <param name="shot">The <see cref="Shot"/> that changed.</param>
        protected virtual void OnShotChanged(Shot? shot) { }

        /// <summary>
        /// The device calls this method when a live performance starts and properties are about to change.
        /// </summary>
        /// <param name="previewer">The <see cref="IPropertyPreviewer"/> to use to register live properties.</param>
        protected virtual void OnRegisterLiveProperties(IPropertyPreviewer previewer) { }

        /// <summary>
        /// Registers properties before a live performance to allow to restore their original values later.
        /// </summary>
        protected void RegisterLiveProperties()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            RestoreLiveProperties();

            if (IsReady())
            {
                if (m_Previewer == null)
                    m_Previewer = new PropertyPreviewer(this);

                RegisterLiveProperties(m_Previewer);
            }
        }

        internal void RegisterLiveProperties(IPropertyPreviewer previewer)
        {
            OnRegisterLiveProperties(previewer);
        }

        /// <summary>
        /// Restores properties that changed during a live performance.
        /// </summary>
        protected void RestoreLiveProperties()
        {
            if (m_Previewer != null)
            {
                m_Previewer.Restore();
            }
        }

        void ClientStartRecording()
        {
            TakeRecorder.StartRecording();

            Refresh();
        }

        void ClientStopRecording()
        {
            TakeRecorder.StopRecording();

            Refresh();
        }

        void ClientSetDeviceMode(DeviceMode deviceMode)
        {
            TakeRecorder.IsLive = deviceMode == DeviceMode.LiveStream;

            SendDeviceState(TakeRecorder.IsLive);
        }

        void ClientStartPlayer()
        {
            TakeRecorder.PlayPreview();

            Refresh();
        }

        void ClientStopPlayer()
        {
            TakeRecorder.PausePreview();
            TakeRecorder.SetPreviewTime(0d);

            Refresh();
        }

        void ClientPausePlayer()
        {
            TakeRecorder.PausePreview();

            Refresh();
        }

        void ClientSetPlayerTime(double time)
        {
            TakeRecorder.SetPreviewTime(time);

            Refresh();
        }

        void SendDeviceState()
        {
            SendDeviceState(TakeRecorder.IsLive);
        }

        void SendDeviceState(bool isLive)
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendDeviceMode(isLive ? DeviceMode.LiveStream : DeviceMode.Playback);
            }
        }

        void SendRecordingState()
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendRecordingState(IsRecording);
            }
        }

        void SendShotDescriptor()
        {
            SendShotDescriptor(TakeRecorder.Shot);
        }

        void SendShotDescriptor(Shot? shot)
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendShotDescriptor(ShotDescriptor.Create(shot));
            }

            OnShotChanged(shot);
        }

        void ClientSetSelectedTake(Guid guid)
        {
            var take = m_AssetManager.Load<Take>(guid);

            TakeRecorder.Context.SetTake(take);

            SendShotDescriptor();
            Refresh();
        }

        void ClientSetTakeData(TakeDescriptor descriptor)
        {
            var take = m_AssetManager.Load<Take>(descriptor.Guid);

            if (take != null)
            {
                var assetName = TakeBuilder.GetAssetName(
                    descriptor.SceneNumber,
                    descriptor.ShotName,
                    descriptor.TakeNumber);

                take.name = assetName;
                take.SceneNumber = descriptor.SceneNumber;
                take.ShotName = descriptor.ShotName;
                take.TakeNumber = descriptor.TakeNumber;
                take.CreationTime = DateTime.FromBinary(descriptor.CreationTime);
                take.Description = descriptor.Description;
                take.Rating = descriptor.Rating;
                take.FrameRate = descriptor.FrameRate;

                m_AssetManager.Save(take);
            }

            SendShotDescriptor();
            Refresh();
        }

        void ClientDeleteTake(Guid guid)
        {
            m_AssetManager.Delete<Take>(guid);

            SendShotDescriptor();
            Refresh();
        }

        void ClientSetIterationBase(Guid guid)
        {
            var take = m_AssetManager.Load<Take>(guid);

            SetIterationBase(take);
        }

        void ClientClearIterationBase()
        {
            SetIterationBase(null);
        }

        void SetIterationBase(Take take)
        {
            TakeRecorder.Context.SetIterationBase(take);

            SendShotDescriptor();
            Refresh();
        }

        void OnTexturePreviewRequested(Guid guid)
        {
            var texture = m_AssetManager.GetPreview<Texture2D>(guid);

            if (texture != null && TryGetInternalClient(out var client))
            {
                client.SendTexturePreview(guid, texture);
            }
        }

        bool OnClientConnected(ICompanionAppClient client)
        {
            if (m_Client == null && client is TClient c && (!ClientMappingDatabase.TryGetClientAssignment(this, out var clientName, out _) || c.Name == clientName))
            {
                SetClient(c, false);
                return true;
            }
            return false;
        }

        void OnClientDisconnected(ICompanionAppClient client)
        {
            if (m_Client == client)
            {
                SetClient(null, false);
            }
        }
    }
}
