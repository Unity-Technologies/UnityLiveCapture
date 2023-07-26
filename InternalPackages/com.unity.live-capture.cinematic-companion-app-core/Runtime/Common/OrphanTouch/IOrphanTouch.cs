using System;
using UnityEngine;

namespace Unity.CompanionAppCommon
{
    interface IOrphanTouch
    {
        event Action<Vector2> onPointerDown;
        event Action<Vector2> onBeginDrag;
        event Action<Vector2> onDrag;
        event Action<Vector2> onEndDrag;
    }
}
