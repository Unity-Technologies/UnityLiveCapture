using UnityEngine;
using Unity.CompanionAppCommon;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.ARKitFaceCapture;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class FacePoseSender : ILateTickable, IRecordingStateListener
    {
        [Inject]
        IFaceTracker m_FaceTracker;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        ISettings m_Settings;
        [Inject]
        IRemoteSystem m_Remote;
        double m_StartTime;

        public void LateTick()
        {
            var host = m_Remote.Host as FaceCaptureHost;

            if (host != null
                && m_CompanionApp.DeviceMode == DeviceMode.LiveStream
                && m_FaceTracker.IsTracking
                && m_FaceTracker.ConsumeFacePose(out var facePose, true))
            {
                host.SendPose(new FaceSample()
                {
                    Time = GetTime(),
                    FacePose = facePose
                });
            }
        }

        public void SetRecordingState(bool isRecording)
        {
            m_StartTime = Time.timeAsDouble;
        }

        double GetTime()
        {
            var timecodeSourceId = m_Settings.TimecodeSourceId;

            if (timecodeSourceId == null)
                timecodeSourceId = string.Empty;

            var timecodeSource = TimecodeSourceManager.Instance[timecodeSourceId];

            if (timecodeSource == null || timecodeSource.CurrentTime == null)
            {
                return Time.timeAsDouble - m_StartTime;
            }
            else
            {
                return timecodeSource.CurrentTime.Value.ToSeconds();
            }
        }
    }
}
