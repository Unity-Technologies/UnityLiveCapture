using System;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.CompanionAppCommon
{
    public class BaseHelpTooltipArgs<T> : MonoBehaviour where T : struct, Enum
    {
        public T id;
        public RectTransform pointedAtTransform;
        public RectTransform targetTransform;
        public VerticalAlign verticalAlign;
        public HorizontalAlign horizontalAlign;
        public bool overrideWidth;
        public float width;
    }
}
