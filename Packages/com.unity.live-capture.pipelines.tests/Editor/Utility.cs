using System;
using UnityEditor;
using UnityEngine;
#if SRP_CORE_14_0_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace Unity.LiveCapture.Tests.Editor
{
    static class Utility
    {
        static readonly Type k_GameViewType = Type.GetType("UnityEditor.GameView, UnityEditor");

        public static void OpenGameView()
        {
            var gameView = EditorWindow.GetWindow(k_GameViewType);
            gameView.Show();
        }

#if SRP_CORE_14_0_OR_NEWER
        // With HDRP 12.1.2, a null sharedProfile on a Volume will lead to a null-ref error in the Volume Editor.
        // This is a bug. We introduce a workaround here.
        public static VolumeProfile AssignVolumeSharedProfileIfNeeded(GameObject gameObject)
        {
            var sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            sharedProfile.hideFlags = HideFlags.DontSave;

            var volume = gameObject.GetComponent<Volume>();
            if (volume != null && volume.sharedProfile == null)
            {
                volume.sharedProfile = sharedProfile;
            }

            return sharedProfile;
        }
#endif
    }
}
