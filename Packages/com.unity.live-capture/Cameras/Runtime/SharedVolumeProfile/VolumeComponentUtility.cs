#if SRP_CORE_14_0_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.Cameras
{
    static class VolumeComponentUtility
    {
        /// <summary>
        /// Returns a reference to a Volume component, fetched from a specified gameObject, and added if not already present.
        /// </summary>
        /// <param name="target">Game object to obtain the Volume from.</param>
        /// <param name="isGlobalVolume">Whether or not the volume is global.</param>
        /// <returns>The Volume component.</returns>
        internal static Volume GetOrAddVolume(GameObject target, bool isGlobalVolume = true)
        {
            if (!target.TryGetComponent<Volume>(out var volume))
            {
                volume = target.AddComponent<Volume>();
                volume.priority = 1;

                volume.isGlobal = isGlobalVolume;

                if (!target.TryGetComponent<SphereCollider>(out var collider))
                {
                    collider = target.AddComponent<SphereCollider>();
                }

                if (!isGlobalVolume)
                {
                    collider.radius = 0.01f;
                    collider.isTrigger = true;
                }
            }

            return volume;
        }

        /// <summary>
        /// Update the value of a Volume Parameter if needed, that is if its current value differs from the desired one.
        /// </summary>
        /// <param name="parameter">Volume parameter to be updated.</param>
        /// <param name="value">Desired value of the parameter.</param>
        /// <typeparam name="T">Type of the value.</typeparam>
        internal static void UpdateParameterIfNeeded<T>(VolumeParameter<T> parameter, T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, parameter.value))
                return;

            parameter.value = value;
            parameter.overrideState = true;
        }

        /// <summary>
        /// Returns a reference to a VolumeComponent, fetched from a specified VolumeProfile, and added if not already present.
        /// </summary>
        /// <param name="profile">VolumeProfile to get or add the volume component from.</param>
        /// <typeparam name="T">Type of the volume component.</typeparam>
        /// <returns>The volume component.</returns>
        internal static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (profile.TryGet(out T result))
            {
                return result;
            }

            return profile.Add<T>();
        }
    }
}
#endif
