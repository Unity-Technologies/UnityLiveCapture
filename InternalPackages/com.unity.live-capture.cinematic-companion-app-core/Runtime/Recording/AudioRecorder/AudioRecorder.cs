using UnityEngine;

namespace Unity.CompanionAppCommon
{
    /// <summary>
    /// A component used to record audio files.
    /// </summary>
    public class AudioRecorder : MonoBehaviour
    {
        IAudioRecorder m_Recorder;

        /// <summary>
        /// Is an audio clip currently being recorded.
        /// </summary>
        public bool IsRecording => m_Recorder != null ? m_Recorder.IsRecording : false;

        void Awake()
        {
#if UNITY_IOS && !UNITY_EDITOR
            m_Recorder = new IOSAudioRecorder();
#endif
        }

        void Update()
        {
            if (m_Recorder != null)
                m_Recorder.Update();
        }

        /// <summary>
        /// Starts recording an audio file to the given file path.
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
        /// Stops recording the current audio clip.
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
