using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// This class contains useful extension methods for the Animator component.
    /// </summary>
    static class AnimatorExtensions
    {
        static List<string> s_Names = new List<string>();

        /// <summary>
        /// Attempts to calculate the path from the specified Animator to the specified Transform.
        /// </summary>
        /// <param name="animator">The Animator to calculate the path from.</param>
        /// <param name="transform">The Transform to calculate the path to.</param>
        /// <param name="path">The calculated path.</param>
        /// <returns>True if the path is valid; false otherwise.</returns>
        public static bool TryGetAnimationPath(this Animator animator, Transform transform, out string path)
        {
            path = string.Empty;

            if (animator == null
                || transform == null
                || transform.root != animator.transform.root)
                return false;

            s_Names.Clear();

            var root = animator.transform;

            while (transform != null && transform != root)
            {
                s_Names.Add(transform.name);
                transform = transform.parent;
            }

            if (transform == root)
            {
                path = string.Join("/", s_Names.Reverse<string>());

                return true;
            }

            return false;
        }
    }
}
