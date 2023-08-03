using System;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IDampingView
    {
        event Action<Vector3> onBodyDampingChanged;
        event Action<float> onAimChanged;
        event Action<bool> onDampingEnabledChanged;
        void SetBodyDamping(Vector3 value);
        void SetAimDamping(float value);
        void SetDampingEnabled(bool value);
    }
}
