using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class Tooltip : BasePopup
    {
        enum VerticalAnchor
        {
            Top,
            Center,
            Bottom
        }

        enum HorizontalAnchor
        {
            Left,
            Center,
            Right
        }

#pragma warning disable CS0649
        [SerializeField, Tooltip("Set screen borders, further constraining tooltip orientation. Tooltip is considered at the screen edge whenever the border is crossed.")]
        Vector2 m_WorldSpaceScreenBorder;
#pragma warning restore CS0649

        Image m_Image;
        RectTransform m_ArrowRect;
        Vector2 m_WorldPosition;

        public struct TooltipData
        {
            public Vector2 worldPosition;
            public string text;
            public Sprite icon;
            public float displayDuration;
            public float fadeDuration;
        }

        public TooltipData DefaultData()
        {
            return new TooltipData
            {
                worldPosition = Vector2.zero,
                text = string.Empty,
                icon = null,
                displayDuration = m_DefaultDisplayDuration,
                fadeDuration = m_DefaultFadeDuration
            };
        }

        void Awake()
        {
            Initialize();
            m_ArrowRect = m_PopUpRect.Find("Arrow").transform as RectTransform;
            m_Image = m_PopUpRect.Find("Icon").GetComponent<Image>();
        }

        public void Display(TooltipData data)
        {
            if (data.icon != null)
            {
                m_Image.sprite = data.icon;
                m_Image.gameObject.SetActive(true);
            }
            else
                m_Image.gameObject.SetActive(false);

            m_TextField.text = data.text;
            m_WorldPosition = data.worldPosition;
            StartAnimation(AnimationInOut(data.displayDuration, data.fadeDuration));
        }

        protected override void OnAnimationInAfterLayout()
        {
            // By this point the tooltip gameObject is assumed to have been updated, and have the proper dimensions.
            var tooltipSize = new Vector2(m_PopUpRect.rect.width, m_PopUpRect.rect.height);
            var border = new Vector2(
                Mathf.Max(m_WorldSpaceScreenBorder.x, tooltipSize.x * 0.5f),
                Mathf.Max(m_WorldSpaceScreenBorder.y, tooltipSize.y * 0.5f));

            AutoAnchor(m_WorldPosition, border, out var horizontal, out var vertical);

            // Position the arrow with respect to the tooltip.
            var arrowDirection = ArrowDirection(horizontal, vertical);
            var arrowPosition = new Vector2(
                arrowDirection.x * tooltipSize.x * 0.5f,
                arrowDirection.y * tooltipSize.y * 0.5f);

            // Take in account the arrow central pivot.
            var arrowOffsetDirection = ArrowOffsetDirection(horizontal, vertical);
            var arrowOffset = new Vector2(
                arrowOffsetDirection.x * m_ArrowRect.rect.width * (vertical == VerticalAnchor.Center ? 0.5f : 2),
                arrowOffsetDirection.y * m_ArrowRect.rect.height * 0.5f);

            m_ArrowRect.localPosition = arrowPosition + arrowOffset;
            m_ArrowRect.localEulerAngles = new Vector3(0, 0, ArrowRotation(horizontal, vertical));
            m_PopUpRect.position = m_WorldPosition - arrowPosition - new Vector2(
                arrowOffset.x * (vertical == VerticalAnchor.Center ? 2 : 1),
                arrowOffset.y * 2);
        }

        void AutoAnchor(Vector2 worldPosition, Vector2 border, out HorizontalAnchor horizontal, out VerticalAnchor vertical)
        {
            // Anchor position depends on world position and screen borders.
            // The tooltip should overall be as centered as possible.
            horizontal = HorizontalAnchor.Center;

            if (worldPosition.x < m_WorldSpaceScreenRect.xMin + border.x)
                horizontal = HorizontalAnchor.Left;
            if (worldPosition.x > m_WorldSpaceScreenRect.xMax - border.x)
                horizontal = HorizontalAnchor.Right;

            // Center-Center is not a valid combination.
            vertical = horizontal == HorizontalAnchor.Center ? (worldPosition.y > m_WorldSpaceScreenRect.height * 0.5f ? VerticalAnchor.Top : VerticalAnchor.Bottom) : VerticalAnchor.Center;

            if (worldPosition.y < m_WorldSpaceScreenRect.yMin + border.y)
                vertical = VerticalAnchor.Bottom;
            if (worldPosition.y > m_WorldSpaceScreenRect.yMax - border.y)
                vertical = VerticalAnchor.Top;
        }

        // Rotation applied to an arrow pointing upwards at rotation_z=0
        static float ArrowRotation(HorizontalAnchor horizontalAnchor, VerticalAnchor verticalAnchor)
        {
            switch (verticalAnchor)
            {
                case VerticalAnchor.Top: return 0;
                case VerticalAnchor.Bottom: return 180;
                case VerticalAnchor.Center:
                {
                    switch (horizontalAnchor)
                    {
                        case HorizontalAnchor.Left: return 90;
                        case HorizontalAnchor.Right: return -90;
                        default: return 0;
                    }
                }
                default: return 0;
            }
        }

        static Vector2 ArrowDirection(HorizontalAnchor horizontalAnchor, VerticalAnchor verticalAnchor)
        {
            var val = Vector2.zero;

            switch (horizontalAnchor)
            {
                case HorizontalAnchor.Left:
                    val.x = -1;
                    break;
                case HorizontalAnchor.Center:
                    val.x = 0;
                    break;
                case HorizontalAnchor.Right:
                    val.x = 1;
                    break;
            }

            switch (verticalAnchor)
            {
                case VerticalAnchor.Bottom:
                    val.y = -1;
                    break;
                case VerticalAnchor.Center:
                    val.y = 0;
                    break;
                case VerticalAnchor.Top:
                    val.y = 1;
                    break;
            }

            return val;
        }

        // Offset applied to an arrow with a central pivot.
        static Vector2 ArrowOffsetDirection(HorizontalAnchor horizontalAnchor, VerticalAnchor verticalAnchor)
        {
            var val = Vector2.zero;

            switch (horizontalAnchor)
            {
                case HorizontalAnchor.Left:
                    val.x = 1;
                    break;
                case HorizontalAnchor.Center:
                    val.x = 0;
                    break;
                case HorizontalAnchor.Right:
                    val.x = -1;
                    break;
            }

            switch (verticalAnchor)
            {
                case VerticalAnchor.Bottom:
                    val.y = -1;
                    break;
                case VerticalAnchor.Center:
                    val.x *= -1;
                    val.y = 0;
                    break;
                case VerticalAnchor.Top:
                    val.y = 1;
                    break;
            }

            return val;
        }
    }
}
