using System;
using UnityEngine;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusReticleController :
        IInitializable,
        IDisposable,
        ITickable,
        IDeviceModeListener,
        IReticlePositionListener,
        IFocusModeListener
    {
        class CoordinatesTransform : BaseFocusReticleControllerImplementation.ICoordinatesTransform
        {
            readonly VideoStreamingSystem m_VideoStreamingSystem;

            public CoordinatesTransform(VideoStreamingSystem videoStreamingSystem)
            {
                m_VideoStreamingSystem = videoStreamingSystem;
            }

            public Vector2 NormalizedToScreen(Vector2 normalizedPosition)
            {
                return m_VideoStreamingSystem.VideoStreamNormalizedPointToScreen(normalizedPosition);
            }

            public Vector2 NormalizeScreenPoint(Vector2 screenPosition)
            {
                return m_VideoStreamingSystem.ScreenPointToVideoStreamNormalized(screenPosition);
            }
        }

        [Inject]
        IFocusReticle m_Reticle;

        [Inject]
        VideoStreamingSystem m_VideoStreamingSystem;

        [Inject]
        SettingsModel m_Settings;

        [Inject]
        StateModel m_State;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        readonly BaseFocusReticleControllerImplementation m_Implementation = new BaseFocusReticleControllerImplementation();

        CoordinatesTransform m_CoordinatesTransform;
        SignalBus m_SignalBus;

        public FocusReticleController(SignalBus signalBus)
        {
            m_SignalBus = signalBus;
        }

        public void Initialize()
        {
            m_CoordinatesTransform = new CoordinatesTransform(m_VideoStreamingSystem);
            m_Implementation.CoordinatesTransform = m_CoordinatesTransform;
            m_Implementation.FocusReticle = m_Reticle;
            m_Implementation.Initialize();
            m_SignalBus.Subscribe<OrphanTouchSignal>(OnOrphanTouch);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<OrphanTouchSignal>(OnOrphanTouch);

            if (!SceneState.IsBeingDestroyed)
            {
                m_Implementation.Dispose();
            }
        }

        public void SetDeviceMode(DeviceMode mode)
        {
            Update();
        }

        public void SetFocusMode(FocusMode value)
        {
            Update();
        }

        public void SetReticlePosition(Vector2 value)
        {
            Update();
        }

        void Update()
        {
            m_Implementation.UpdateView(
                m_Settings.ReticlePosition,
                m_Settings.FocusMode,
                m_CompanionApp.DeviceMode == DeviceMode.LiveStream);
        }

        void OnOrphanTouch(OrphanTouchSignal signal)
        {
            switch (signal.type)
            {
                case OrphanTouchType.Drag:
                    m_Implementation.PendingDrag = true;
                    m_Implementation.LastPointerPosition = signal.position;
                    break;
                case OrphanTouchType.BeginDrag:
                    m_Implementation.IsDragging = true;
                    break;
                case OrphanTouchType.EndDrag:
                    m_Implementation.IsDragging = false;
                    break;
                case OrphanTouchType.PointerDown:
                    m_Implementation.PendingTap = true;
                    m_Implementation.LastPointerPosition = signal.position;
                    break;
            }
        }

        public void Tick()
        {
            if (m_Implementation.ShouldSendPosition(out var position))
            {
                //m_Settings.ReticlePosition = position;
                m_SignalBus.Fire(new SendHostSignal()
                {
                    Type = HostMessageType.FocusReticlePosition,
                    Vector2Value = position
                });
            }
        }
    }
}
