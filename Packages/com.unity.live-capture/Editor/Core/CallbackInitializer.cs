using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    [InitializeOnLoad]
    static class CallbackInitializer
    {
        static CallbackInitializer()
        {
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            ConnectionManager.Instance.Update();
        }
    }
}
