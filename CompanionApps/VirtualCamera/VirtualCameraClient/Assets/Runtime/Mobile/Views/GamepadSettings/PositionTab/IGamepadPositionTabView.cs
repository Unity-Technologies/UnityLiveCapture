using System;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IGamepadPositionTabView
    {
        void SetSensitivity(Vector3 sensitivity);

        event Action<Vector3> onSensitivityChanged;
    }
}
