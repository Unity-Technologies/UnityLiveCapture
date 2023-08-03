using System;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IPositionView
    {
        void SetPositionLock(PositionAxis positionAxis);
        void SetMotionScale(Vector3 motionScale);

        event Action<Vector3> onMotionScaleChanged;
        event Action<PositionAxis> onAxisLockChanged;
    }
}
