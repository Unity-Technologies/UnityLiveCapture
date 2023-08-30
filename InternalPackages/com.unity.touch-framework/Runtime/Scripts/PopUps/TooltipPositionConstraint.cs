using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.TouchFramework
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class TooltipPositionConstraint : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        RectTransform m_TargetTransform;
        [SerializeField]
        VerticalAlign m_VerticalAlign;
        [SerializeField]
        HorizontalAlign m_HorizontalAlign;
        [SerializeField]
        float m_Margin = 40;
#pragma warning disable 649

        RectTransform m_RectTransform;
        Canvas m_RootCanvas;

        public RectTransform targetTransform
        {
            set => m_TargetTransform = value;
        }

        public HorizontalAlign horizontalAlign
        {
            set => m_HorizontalAlign = value;
        }

        public VerticalAlign verticalAlign
        {
            set => m_VerticalAlign = value;
        }

        void OnValidate()
        {
            UpdateTransform();
        }

        [ContextMenu("Update Transform")]
        public void UpdateTransform()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (m_TargetTransform == null)
            {
                Debug.LogWarning($"{nameof(TooltipPositionConstraint)}: {nameof(m_TargetTransform)} is not set.");
                return;
            }

            if (m_RectTransform == null)
            {
                m_RectTransform = GetComponent<RectTransform>();
            }

            if (m_RootCanvas == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                Assert.IsNotNull(canvas);
                m_RootCanvas = canvas.rootCanvas;
            }

            var targetRectLocal = m_TargetTransform.rect;
            var min = m_TargetTransform.TransformPoint(targetRectLocal.min);
            var max = m_TargetTransform.TransformPoint(targetRectLocal.max);
            var targetRectWorld = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            var pivot = new Vector2(GetPivotX(m_VerticalAlign, m_HorizontalAlign), GetPivotY(m_VerticalAlign));

            var margin = m_Margin * m_RootCanvas.scaleFactor;

            var position = GetPosition(targetRectWorld, m_VerticalAlign, m_HorizontalAlign, margin);

            m_RectTransform.position = position;
            m_RectTransform.pivot = pivot;
        }

        static Vector2 GetPosition(Rect rect, VerticalAlign verticalAlign, HorizontalAlign horizontalAlign, float margin)
        {
            var y = GetPositionY(rect, verticalAlign, margin);
            return GetPosition(rect, verticalAlign, horizontalAlign, margin, y);
        }

        static float GetPositionY(Rect rect, VerticalAlign verticalAlign, float margin)
        {
            switch (verticalAlign)
            {
                case VerticalAlign.Above: return rect.yMax + margin;
                case VerticalAlign.Below: return rect.yMin - margin;
                case VerticalAlign.Top: return rect.yMax;
                case VerticalAlign.Center: return rect.center.y;
                case VerticalAlign.Bottom: return rect.yMin;
            }

            return 0;
        }

        static Vector2 GetPosition(Rect rect, VerticalAlign verticalAlign, HorizontalAlign horizontalAlign, float margin, float y)
        {
            switch (verticalAlign)
            {
                case VerticalAlign.Above:
                case VerticalAlign.Below:
                {
                    switch (horizontalAlign)
                    {
                        case HorizontalAlign.Left: return new Vector2(rect.xMin, y);
                        case HorizontalAlign.Center: return new Vector2(rect.center.x, y);
                        case HorizontalAlign.Right: return new Vector2(rect.xMax, y);
                    }

                    break;
                }
                case VerticalAlign.Top:
                case VerticalAlign.Center:
                case VerticalAlign.Bottom:
                {
                    switch (horizontalAlign)
                    {
                        case HorizontalAlign.Left: return new Vector2(rect.xMin - margin, y);
                        case HorizontalAlign.Center: return new Vector2(rect.center.x, y);
                        case HorizontalAlign.Right: return new Vector2(rect.xMax + margin, y);
                    }

                    break;
                }
            }

            return Vector2.zero;
        }

        static float GetPivotX(VerticalAlign verticalAlign, HorizontalAlign horizontalAlign)
        {
            switch (verticalAlign)
            {
                case VerticalAlign.Above:
                case VerticalAlign.Below:
                {
                    switch (horizontalAlign)
                    {
                        case HorizontalAlign.Left: return 0;
                        case HorizontalAlign.Center: return .5f;
                        case HorizontalAlign.Right: return 1;
                    }

                    break;
                }
                case VerticalAlign.Top:
                case VerticalAlign.Center:
                case VerticalAlign.Bottom:
                {
                    switch (horizontalAlign)
                    {
                        case HorizontalAlign.Left: return 1;
                        case HorizontalAlign.Center: return .5f;
                        case HorizontalAlign.Right: return 0;
                    }

                    break;
                }
            }

            return 0;
        }

        static float GetPivotY(VerticalAlign align)
        {
            switch (align)
            {
                case VerticalAlign.Above: return 0;
                case VerticalAlign.Top: return 1;
                case VerticalAlign.Center: return .5f;
                case VerticalAlign.Bottom: return 0;
                case VerticalAlign.Below: return 1;
            }

            return 0;
        }
    }
}
