using UnityEngine;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ISensorSizeListener
    {
        void SetSensorSize(Vector2 sensorSize);
    }

    interface IFocusDistanceOffsetListener
    {
        void SetFocusDistanceOffset(float value);
    }

    interface ILensIntrinsicsListener
    {
        void SetLensIntrinsics(LensIntrinsics value);
    }

    interface IReticlePositionListener
    {
        void SetReticlePosition(Vector2 value);
    }
}
