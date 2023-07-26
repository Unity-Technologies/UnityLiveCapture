using System;
using System.IO;
using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class AudioVideoRecorderController : IRecordingStateListener
    {
        [Inject]
        AudioRecorder m_AudioRecorder;
        [Inject]
        VideoRecorder m_VideoRecorder;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        ISettings m_Settings;

        public void SetRecordingState(bool isRecording)
        {
            if (isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        void StartRecording()
        {
            var path = Path.Combine(Application.persistentDataPath, "Recordings");
            var recordingName = m_CompanionApp.NextAssetName;

            if (string.IsNullOrEmpty(recordingName))
            {
                recordingName = DateTime.Now.ToString();
            }

            if (m_Settings.RecordVideo)
                m_VideoRecorder.StartRecording(Path.Combine(path, "Video"), recordingName);
            if (m_Settings.RecordAudio)
                m_AudioRecorder.StartRecording(Path.Combine(path, "Audio"), recordingName);
        }

        void StopRecording()
        {
            m_VideoRecorder.StopRecording();
            m_AudioRecorder.StopRecording();
        }
    }
}
