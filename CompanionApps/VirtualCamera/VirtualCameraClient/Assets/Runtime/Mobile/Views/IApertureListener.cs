using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IApertureListener
    {
        void SetAperture(float aperture, Vector2 range);
    }
}
