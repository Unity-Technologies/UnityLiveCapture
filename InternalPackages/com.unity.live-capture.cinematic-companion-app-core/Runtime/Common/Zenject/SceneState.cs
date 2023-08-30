using System;
using Zenject;

namespace Unity.CompanionAppCommon
{
    /// <summary>
    /// Bind to Zenject with the max execution order. It will be the first to know
    /// that the scene is being destroyed, so that other Zenject instances can ask it.
    /// </summary>
    class SceneState : IInitializable, IDisposable
    {
        public static bool IsBeingDestroyed => s_IsBeingDestroyed;

        static bool s_IsBeingDestroyed;

        public void Initialize()
        {
            s_IsBeingDestroyed = false;
        }

        public void Dispose()
        {
            s_IsBeingDestroyed = true;
        }
    }
}
