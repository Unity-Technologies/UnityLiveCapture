using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IFocusDistanceListener
    {
        void SetFocusDistance(float value, Vector2 range);
    }
}
