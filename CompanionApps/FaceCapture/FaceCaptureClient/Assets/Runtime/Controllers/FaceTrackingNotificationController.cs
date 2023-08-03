using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class FaceTrackingNotificationController : ITickable
    {
        const string k_TrackingLostMessage = "Face tracking lost. Be sure to center\nyour face in the view.";

        [Inject]
        IARFaceManagerSystem m_FaceManager;

        [Inject]
        IFaceTracker m_FaceTracker;

        [Inject]
        SignalBus m_SignalBus;

        bool m_IsTracking;

        public void Tick()
        {
            if (!m_FaceManager.Enabled)
            {
                return;
            }

            var isTracking = m_FaceTracker.IsTracking;

            if (m_IsTracking != isTracking && !isTracking)
            {
                m_SignalBus.Fire(new ShowNotificationSignal()
                {
                    value = k_TrackingLostMessage
                });
            }

            m_IsTracking = isTracking;
        }
    }
}
