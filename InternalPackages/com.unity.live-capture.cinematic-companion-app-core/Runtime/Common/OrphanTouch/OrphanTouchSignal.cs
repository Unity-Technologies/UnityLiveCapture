using UnityEngine;

namespace Unity.CompanionAppCommon
{
    class OrphanTouchSignal
    {
        public Vector2 position;
        public OrphanTouchType type;
        public bool stopPropagation;
    }
}
