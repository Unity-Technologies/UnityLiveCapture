using System;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IJoystickGeneralTabView
    {
        void SetPedestalSpace(Space space);
        void SetMotionSpace(Space space);
        void SetSensitivity(Vector3 sensitivity);

        event Action<Space> onPedestalSpaceChanged;
        event Action<Space> onMotionSpaceChanged;
        event Action<Vector3> onSensitivityChanged;
    }
}
