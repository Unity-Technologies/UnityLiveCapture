using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    static class TransformExtensions
    {
        public static Pose GetPose(this Transform transform, Space space)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            if (space == Space.World)
            {
                return new Pose(transform.position, transform.rotation);
            }
            else
            {
                return new Pose(transform.localPosition, transform.localRotation);
            }
        }
    }
}
