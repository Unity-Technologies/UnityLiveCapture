using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    internal interface IAnchorable
    {
        void AnchorTo(Transform target, AnchorSettings? settings, int? sessionID = null);
        void GetConfiguration(out Transform target, out AnchorSettings? settings, out int? sessionID);
    }
}
