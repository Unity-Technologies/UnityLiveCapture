using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Class to register the camera driver updates so that they run after the animation directors evaluate.
    /// This ensures that drivers update after anchoring targets have been animated.
    /// </summary>
    class CameraDriverUpdateManager
    {
        struct CameraDriverPostLateUpdate
        {
        }

        public static CameraDriverUpdateManager Instance { get; } = new CameraDriverUpdateManager();

        static CameraDriverUpdateManager()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!playerLoop.TryFindSubSystem<PostLateUpdate>(out var postLateUpdate))
            {
                Debug.LogError($"{nameof(PostLateUpdate)} system not found.");

                return;
            }

            var index = postLateUpdate.IndexOf<PostLateUpdate.DirectorLateUpdate>();

            if (index == -1)
            {
                Debug.LogError($"{nameof(PostLateUpdate.DirectorLateUpdate)} system not found.");

                return;
            }

            var indexAfter = index + 1;
            postLateUpdate.AddSubSystem<CameraDriverPostLateUpdate>(indexAfter, RunPostLateUpdate);

            if (playerLoop.TryUpdate(postLateUpdate))
            {
                PlayerLoop.SetPlayerLoop(playerLoop);
            }
        }

        static void RunPostLateUpdate()
        {
            foreach (var driver in Instance.m_Drivers)
            {
                driver.PostLateUpdate(Time.deltaTime);
            }
        }

        List<BaseCameraDriver> m_Drivers = new List<BaseCameraDriver>();

        public void Register(BaseCameraDriver driver)
        {
            m_Drivers.AddUnique(driver);
        }

        public void Unregister(BaseCameraDriver driver)
        {
            m_Drivers.Remove(driver);
        }
    }
}
