using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Contains useful extension methods.
    /// </summary>
    static class LiveCaptureUtility
    {
        /// <summary>
        /// Appends an item to a list if the item is not already contained by the list.
        /// </summary>
        /// <param name="list">The list to append to.</param>
        /// <param name="item">The item to add.</param>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <returns>True if a new item was added; false otherwise.</returns>
        public static bool AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine the index of a specific item in the current instance.
        /// </summary>
        public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var element in list)
            {
                if (predicate(element))
                    return i;
                i++;
            }

            return -1;
        }

        /// <summary>
        /// Checks if a <see cref="LiveCaptureDevice" /> is ready, enabled and has live mode set.
        /// </summary>
        /// <param name="device">The device to check.</param>
        /// <returns>True if a device is live and its associated take recorder is live and enabled; false otherwise.</returns>
        public static bool IsLiveAndReady(this LiveCaptureDevice device)
        {
            return LiveCaptureDeviceManager.Instance.IsLive
                && device.isActiveAndEnabled
                && device.IsLive
                && device.IsReady();
        }

        /// <summary>
        /// Returns the camera that has the higher depth.
        /// </summary>
        /// <returns>The camera that has the higher depth, if any.</returns>
        public static Camera GetTopCamera()
        {
            return Camera.allCameras.OrderByDescending(c => c.depth).FirstOrDefault();
        }

#if !NETSTANDARD2_1
        public static bool TryPeek<T>(this Queue<T> queue, out T result)
        {
            result = default;

            if (queue.Count > 0)
            {
                result = queue.Peek();

                return true;
            }

            return false;
        }
#endif
    }
}
