using UnityEngine;

namespace Unity.LiveCapture.Internal
{
    static class DrivenPropertyManagerInternal
    {
        public static void RegisterProperty(Object driver, Object target, string propertyPath)
        {
#if UNITY_EDITOR
            DrivenPropertyManager.TryRegisterProperty(driver, target, propertyPath);
#endif
        }

        public static void UnregisterProperties(Object driver)
        {
#if UNITY_EDITOR
            DrivenPropertyManager.UnregisterProperties(driver);
#endif
        }
    }
}
