using UnityEngine;

namespace Unity.CompanionAppCommon
{
    static class RectTransformExtensions
    {
        public static void StretchToParent(this RectTransform rectTransform)
        {
            Debug.Assert(rectTransform != null);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
