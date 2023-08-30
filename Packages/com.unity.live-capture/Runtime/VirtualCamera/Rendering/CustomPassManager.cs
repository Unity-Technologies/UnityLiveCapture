#if HDRP_14_0_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Utility that provides an API to manage HDRP Custom Passes.
    /// </summary>
    /// <remarks>
    /// This API assumes that at most one CustomPass of a given type is injected at a specific point.
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    class CustomPassManager : MonoBehaviour
    {
        /// <summary>
        /// A handle to a Custom Pass.
        /// </summary>
        /// <typeparam name="T">Type of the underlying Custom Pass.</typeparam>
        internal class Handle<T> : IDisposable where T : CustomPass, new()
        {
            static int s_InstancesCount;

            CustomPassInjectionPoint m_InjectionPoint;

            /// <summary>
            /// Returns the underlying Custom Pass.
            /// </summary>
            public T GetPass()
            {
                if (HasCustomPass<T>(m_InjectionPoint, out _, out var pass))
                    return pass;
                return null;
            }

            /// <summary>
            /// Creates a CustomPass and returns a handle to it.
            /// </summary>
            /// <param name="injectionPoint">The injection point to use for the created Custom Pass.</param>
            public Handle(CustomPassInjectionPoint injectionPoint)
            {
                ++s_InstancesCount;
                m_InjectionPoint = injectionPoint;
                RequestCustomPass<T>(m_InjectionPoint);
            }

            /// <summary>
            /// Releases the handle and removes the underlying Custom Pass.
            /// </summary>
            public void Dispose()
            {
                Assert.IsTrue(s_InstancesCount > 0);
                --s_InstancesCount;
                if (s_InstancesCount == 0)
                    RemoveCustomPass<T>(m_InjectionPoint, out _);
            }
        }

        static CustomPassManager s_Instance;

        static CustomPassManager GetOrCreateInstance()
        {
            if (s_Instance == null)
            {
                var instances = Resources.FindObjectsOfTypeAll<CustomPassManager>();
                if (instances.Length == 1)
                {
                    s_Instance = instances[0];
                }
                else if (instances.Length > 1)
                {
                    throw new InvalidOperationException($"Multiple instances of {nameof(CustomPassManager)} detected, expected at most one.");
                }
            }

            if (s_Instance == null)
            {
                var gameObject = AdditionalCoreUtils.CreateEmptyGameObject();
                gameObject.name = "Virtual Camera Custom Pass Manager";
                s_Instance = gameObject.AddComponent<CustomPassManager>();
            }

            if (!s_Instance.isActiveAndEnabled)
            {
                Debug.LogWarning($"{nameof(CustomPassManager)} component held by \"{s_Instance.gameObject.name}\" gameObject" +
                    " should be active and enabled for Live Capture rendering features to work properly.");
            }

            return s_Instance;
        }

        void Awake()
        {
            Assert.IsTrue(s_Instance == null || s_Instance == this);
            s_Instance = this;
        }

        void OnDestroy()
        {
            Assert.IsTrue(s_Instance == null || s_Instance == this);
            s_Instance = null;
        }

        CustomPassVolume GetOrCreateVolume(CustomPassInjectionPoint injectionPoint)
        {
            var volume = GetVolume(injectionPoint);
            if (volume != null)
                return volume;

            var newVolume = gameObject.AddComponent<CustomPassVolume>();
            newVolume.injectionPoint = injectionPoint;
            newVolume.hideFlags = HideFlags.HideInInspector;
            return newVolume;
        }

        CustomPassVolume GetVolume(CustomPassInjectionPoint injectionPoint)
        {
            foreach (var volume in GetComponents<CustomPassVolume>())
            {
                if (volume.injectionPoint == injectionPoint)
                {
                    return volume;
                }
            }

            return null;
        }

        /// <summary>
        /// Indicates whether or not a Custom Pass of the specified type has been added at the provided injection point.
        /// </summary>
        /// <exception cref="InvalidOperationException">Exception thrown in case multiple passes of the same type are found.</exception>
        static bool HasCustomPass<T>(CustomPassInjectionPoint injectionPoint, out CustomPassVolume volume, out T pass) where T : CustomPass
        {
            volume = GetOrCreateInstance().GetOrCreateVolume(injectionPoint);
            var passes = volume.customPasses.OfType<T>().ToList();

            if (passes.Count > 1)
                throw new InvalidOperationException($"Multiple instances of pass {typeof(T).Name} found.");

            var hasPass = passes.Count > 0;
            pass = hasPass ? passes[0] : null;
            return hasPass;
        }

        /// <summary>
        /// Returns the Custom Pass of type T injected at the provided injection point, and creates it if not already present.
        /// </summary>
        static T RequestCustomPass<T>(CustomPassInjectionPoint injectionPoint) where T : CustomPass, new()
        {
            var hasPass = HasCustomPass<T>(injectionPoint, out var volume, out var pass);
            if (hasPass)
                return pass;

            var newPass = new T();
            volume.customPasses.Add(newPass);

            // The order of passes is important and is controlled using the optional CustomPassOrderAttribute.
            // For example, we want the focus plane to be blended before the film format is rendered.
            SortCustomPasses(volume.customPasses);

            return newPass;
        }

        /// <summary>
        /// Removes the CustomPass of type T injected at the provided injection point if present.
        /// </summary>
        /// <returns>Indicates whether or not a pass existed and was removed.</returns>
        static bool RemoveCustomPass<T>(CustomPassInjectionPoint injectionPoint, out T pass) where T : CustomPass
        {
            // Do not use GetOrCreateInstance or you may spawn object OnDestroy, which would cause a leak.
            if (s_Instance == null)
            {
                pass = null;
                return false;
            }

            var volume = s_Instance.GetVolume(injectionPoint);
            if (volume == null)
            {
                pass = null;
                return false;
            }

            var passes = volume.customPasses.OfType<T>().ToList();
            if (passes.Count > 1)
                throw new InvalidOperationException($"Multiple instances of pass {typeof(T).Name} found.");

            pass = passes[0];
            volume.customPasses.Remove(pass);
            return true;
        }

        /// <summary>
        /// Sorts passes based on the optional CustomPassOrderAttribute.
        /// </summary>
        /// <param name="passes">The list of passes to sort.</param>
        static void SortCustomPasses(List<CustomPass> passes)
        {
            int GetOrderHint(Type type)
            {
                var attr = type.GetCustomAttributes(typeof(CustomPassOrderAttribute), true)
                    .FirstOrDefault() as CustomPassOrderAttribute;
                if (attr != null)
                    return attr.OrderHint;
                return -1;
            }

            passes.Sort((passA, passB) => GetOrderHint(passA.GetType()).CompareTo(GetOrderHint(passB.GetType())));
        }
    }
}
#endif
