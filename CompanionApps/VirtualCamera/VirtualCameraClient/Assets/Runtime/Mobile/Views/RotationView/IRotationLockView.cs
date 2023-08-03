using System;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IRotationLockView
    {
        void SetRotationLock(RotationAxis rotationAxis);
        void SetAutoHorizon(bool zeroDutch);
        event Action<bool> onZeroDutchChanged;
        event Action<RotationAxis> onRotationLockChanged;
    }
}
