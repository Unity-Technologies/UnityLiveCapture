using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.CompanionApps.VirtualCamera
{
    enum Scenes
    {
        Phone = 1,
        Tablet = 2
    }

    public class BootLoader : MonoBehaviour
    {
        void Start()
        {
            // iPhone scene should be renamed.
            var scene = Scenes.Phone;

            var isIPad = SystemInfo.deviceModel.Contains("iPad");

            if (isIPad)
            {
                scene = Scenes.Tablet;
            }

            SceneManager.LoadScene((int)scene, LoadSceneMode.Single);
        }
    }
}
