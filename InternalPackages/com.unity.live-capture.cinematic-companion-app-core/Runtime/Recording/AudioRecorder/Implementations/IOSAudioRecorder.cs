#if UNITY_IOS && !UNITY_EDITOR
using System.IO;
using System.Runtime.InteropServices;

namespace Unity.CompanionAppCommon
{
    class IOSAudioRecorder : IAudioRecorder
    {
        const string k_DLL = "__Internal";
        const string k_FileExtension = ".wav";

        [DllImport(k_DLL)]
        static extern void UnityAudioCaptureSetPath(string path);
        [DllImport(k_DLL)]
        static extern bool UnityAudioCaptureIsRecording();
        [DllImport(k_DLL)]
        static extern void UnityAudioCaptureToggleRecording();

        /// <inheritdoc/>
        public bool IsRecording => UnityAudioCaptureIsRecording();

        /// <inheritdoc/>
        public void Update()
        {
        }

        /// <inheritdoc/>
        public void StartRecording(string directory, string fileName)
        {
            Directory.CreateDirectory(directory);
            UnityAudioCaptureSetPath(Path.Combine(directory, fileName + k_FileExtension));

            UnityAudioCaptureToggleRecording();
        }

        /// <inheritdoc/>
        public void StopRecording()
        {
            UnityAudioCaptureToggleRecording();
        }
    }
}
#endif
