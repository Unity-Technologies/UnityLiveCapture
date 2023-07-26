using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    static class AnchorUtility
    {
        public static Vector3 GetAnchorPosition(Vector3 targetPosition, in AnchorSettings settings)
        {
            if (settings.PositionLock == Axis.None)
            {
                return Vector3.zero;
            }

            return Select(targetPosition, Vector3.zero, settings.PositionLock);
        }

        public static Quaternion GetAnchorOrientation(Vector3 targetEuler, in AnchorSettings settings)
        {
            if (settings.RotationLock == Axis.None)
            {
                return Quaternion.identity;
            }

            targetEuler = Select(targetEuler, Vector3.zero, settings.RotationLock);

            return Quaternion.Euler(targetEuler);
        }

        static Vector3 Select(Vector3 left, Vector3 right, Axis mask)
        {
            return new Vector3(
                mask.HasFlag(Axis.X) ? left.x : right.x,
                mask.HasFlag(Axis.Y) ? left.y : right.y,
                mask.HasFlag(Axis.Z) ? left.z : right.z);
        }
    }
}
