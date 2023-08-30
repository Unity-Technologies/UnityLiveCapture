using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IFocalLengthListener
    {
        void SetFocalLength(float value, Vector2 range);
    }
}
