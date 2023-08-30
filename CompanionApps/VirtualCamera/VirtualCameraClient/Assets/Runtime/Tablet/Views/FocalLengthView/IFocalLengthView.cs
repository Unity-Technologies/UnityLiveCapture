using System;
using UnityEngine;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IFocalLengthView : IDialogView
        , IFocalLengthListener
        , ILensIntrinsicsListener
    {
        event Action<float> ValueChanged;
        event Action Closed;

        Vector2 SensorSize { set; }
    }
}
