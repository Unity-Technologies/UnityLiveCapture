using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Unity.LiveCapture
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    class UpdateManager
    {
        struct LiveCaptureUpdate
        {
        }

        struct LiveCaptureLiveUpdate
        {
        }

        static bool s_Installed;

        static UpdateManager()
        {
            Install();
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
#endif
        static void Install()
        {
            if (!s_Installed)
            {
                InstallUpdate();
                InstallLiveUpdate();

                s_Installed = true;
            }
        }

        static void InstallUpdate()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!playerLoop.TryFindSubSystem<Update>(out var update))
            {
                Debug.LogError($"{nameof(Update)} system not found.");

                return;
            }

            var index = update.IndexOf<Update.ScriptRunBehaviourUpdate>();

            if (index == -1)
            {
                Debug.LogError($"{nameof(Update.ScriptRunBehaviourUpdate)} system not found.");

                return;
            }

            update.AddSubSystem<LiveCaptureUpdate>(index, UpdateCallback);

            if (playerLoop.TryUpdate(update))
            {
                PlayerLoop.SetPlayerLoop(playerLoop);
            }
        }

        static void InstallLiveUpdate()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!playerLoop.TryFindSubSystem<PreLateUpdate>(out var preLateUpdate))
            {
                Debug.LogError($"{nameof(PreLateUpdate)} system not found.");

                return;
            }

            var index = preLateUpdate.IndexOf<PreLateUpdate.ScriptRunBehaviourLateUpdate>();

            if (index == -1)
            {
                Debug.LogError($"{nameof(PreLateUpdate.ScriptRunBehaviourLateUpdate)} system not found.");

                return;
            }

            preLateUpdate.AddSubSystem<LiveCaptureLiveUpdate>(index, LiveUpdateCallback);

            if (playerLoop.TryUpdate(preLateUpdate))
            {
                PlayerLoop.SetPlayerLoop(playerLoop);
            }
        }

        static void UpdateCallback()
        {
            TakeRecorderImpl.Instance.Update();
            LiveCaptureDeviceManager.Instance.UpdateDevice();
        }

        static void LiveUpdateCallback()
        {
            TakeRecorderImpl.Instance.LiveUpdate();
            LiveCaptureDeviceManager.Instance.LiveUpdate();
        }
    }
}
