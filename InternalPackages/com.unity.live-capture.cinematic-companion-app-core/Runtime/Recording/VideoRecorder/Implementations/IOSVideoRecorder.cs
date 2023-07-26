#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.CompanionAppCommon
{
    class IOSVideoRecorder : IVideoRecorder
    {
        const string k_DLL = "__Internal";
        const string k_FileExtension = ".mp4";

        [DllImport(k_DLL)]
        static extern void UnityVideoCaptureSetup(IntPtr session, int fps);
        [DllImport(k_DLL)]
        static extern void UnityVideoCaptureSetPath(string path);
        [DllImport(k_DLL)]
        static extern bool UnityVideoCaptureIsRecording();
        [DllImport(k_DLL)]
        static extern void UnityVideoCaptureStartRecording(bool includeAudio);
        [DllImport(k_DLL)]
        static extern void UnityVideoCaptureStopRecording();
        [DllImport(k_DLL)]
        static extern bool UnityCaptureVideoFrame();

        /// <inheritdoc/>
        public bool IsRecording => UnityVideoCaptureIsRecording();

        /// <inheritdoc/>
        public bool IncludeAudio { get; set; }

        IntPtr m_Session = IntPtr.Zero;

        /// <inheritdoc/>
        public void Update()
        {
            if (m_Session == IntPtr.Zero)
            {
                var arSession = Resources.FindObjectsOfTypeAll<ARSession>()[0];

                if (arSession != null)
                {
                    m_Session = arSession.subsystem.nativePtr;

                    var fps = Application.targetFrameRate;
                    if (fps == -1)
                        fps = 30;

                    UnityVideoCaptureSetup(m_Session, fps);
                }
            }

            UnityCaptureVideoFrame();
        }

        /// <inheritdoc/>
        public void StartRecording(string directory, string fileName)
        {
            Directory.CreateDirectory(directory);
            UnityVideoCaptureSetPath(Path.Combine(directory, fileName + k_FileExtension));

            UnityVideoCaptureStartRecording(IncludeAudio);
        }

        /// <inheritdoc/>
        public void StopRecording()
        {
            UnityVideoCaptureStopRecording();
        }
    }
}
#endif
