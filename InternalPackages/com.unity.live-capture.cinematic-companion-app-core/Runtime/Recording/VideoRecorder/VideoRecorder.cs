using UnityEngine;

namespace Unity.CompanionAppCommon
{
    /// <summary>
    /// A component used to record video files.
    /// </summary>
    public class VideoRecorder : MonoBehaviour
    {
        IVideoRecorder m_Recorder;

        /// <summary>
        /// Is a video currently being recorded.
        /// </summary>
        public bool IsRecording => m_Recorder != null ? m_Recorder.IsRecording : false;

        /// <summary>
        /// Will the recorded video include audio data.
        /// </summary>
        public bool IncludeAudio
        {
            get => m_Recorder != null ? m_Recorder.IncludeAudio : false;
            set
            {
                if (m_Recorder != null)
                    m_Recorder.IncludeAudio = value;
            }
        }

        void Awake()
        {
#if UNITY_IOS && !UNITY_EDITOR
            m_Recorder = new IOSVideoRecorder();
#endif
        }

        void Update()
        {
            if (m_Recorder != null)
                m_Recorder.Update();
        }

        /// <summary>
        /// Starts recording a video file to the given file path.
        /// </summary>
        /// <param name="directory">The folder path to save the recorded audio to.</param>
        /// <param name="fileName">The name of the file, excluding the file extension.</param>
        public void StartRecording(string directory, string fileName)
        {
            if (IsRecording)
                return;

            if (m_Recorder != null)
                m_Recorder.StartRecording(directory, fileName);
        }

        /// <summary>
        /// Stops recording the current video file.
        /// </summary>
        public void StopRecording()
        {
            if (!IsRecording)
                return;

            if (m_Recorder != null)
                m_Recorder.StopRecording();
        }
    }
}
