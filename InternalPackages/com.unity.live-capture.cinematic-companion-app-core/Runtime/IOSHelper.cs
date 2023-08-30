using System;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;

namespace Unity.CompanionAppCommon
{
    public static class IOSHelper
    {
        const string k_DLL = "__Internal";

        [DllImport(k_DLL)]
        public static extern void OpenSettings();

        [DllImport(k_DLL)]
        public static extern bool HasVideoPermission();

        [DllImport(k_DLL)]
        public static extern bool HasAudioPermission();
    }
}
#else
namespace Unity.CompanionAppCommon
{
    static class IOSHelper
    {
        public static void OpenSettings()
        {
            WarnUnavailable();
        }

        public static bool HasVideoPermission()
        {
            WarnUnavailable();
            return true;
        }

        public static bool HasAudioPermission()
        {
            WarnUnavailable();
            return true;
        }

        static void WarnUnavailable()
        {
#if !UNITY_EDITOR
            Debug.LogWarning($"{nameof(IOSHelper)} not available on the current platform.");
#endif
        }
    }
}
#endif
