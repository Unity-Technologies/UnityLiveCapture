using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.TouchFramework
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class TooltipArrow : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        RectTransform m_PointedAtTransform;
        [SerializeField]
        float m_Border;
        [SerializeField]
        float m_ArrowAngleOffset;
#pragma warning disable 649

        RectTransform m_RectTransform;
        RectTransform m_ArrowRectTransform;

        public RectTransform pointedAtTransform
        {
            set => m_PointedAtTransform = value;
        }

        void Awake()
        {
            m_RectTransform = transform as RectTransform;
            m_ArrowRectTransform = transform.Find("Arrow").transform as RectTransform;
            Assert.IsNotNull(m_ArrowRectTransform);
        }

        void OnValidate()
        {
            m_Border = Mathf.Max(0, m_Border);
        }

        [ContextMenu("Update Transform")]
        public void UpdateTransform()
        {
            if (m_PointedAtTransform == null)
            {
                Debug.LogWarning($"{nameof(TooltipArrow)}: {nameof(m_PointedAtTransform)} is not set.");
                return;
            }

            var pointedAtWorld = m_PointedAtTransform.TransformPoint(m_PointedAtTransform.rect.center);
            var pointedAtLocal = m_RectTransform.InverseTransformPoint(pointedAtWorld);
            var border = m_Border + m_ArrowRectTransform.sizeDelta.x * .5f;
            var arrowTransform = GetArrowTransform(m_RectTransform.rect, pointedAtLocal, border);

            m_ArrowRectTransform.localPosition = arrowTransform.position;
            var rotation = m_ArrowAngleOffset + Vector2.SignedAngle(Vector2.right, arrowTransform.direction);
            m_ArrowRectTransform.localEulerAngles = Vector3.forward * rotation;
        }

        static (Vector2 position, Vector2 direction) GetArrowTransform(Rect rect, Vector2 point, float border = .1f)
        {
            float Closest(float value, float a, float b) => Mathf.Abs(value - a) < Mathf.Abs(value - b) ? a : b;

            var projectedX = new Vector2(Closest(point.x, rect.xMin, rect.xMax), point.y);
            var projectedY = new Vector2(point.x, Closest(point.y, rect.yMin, rect.yMax));

            var sqrDistX = (projectedX - rect.center).sqrMagnitude;
            var sqrDistY = (projectedY - rect.center).sqrMagnitude;

            var position = sqrDistX < sqrDistY ? projectedX : projectedY;

            // Determine arrow direction based on the edge it lies on.
            // We ignore the case of a point laying exactly on a corner.
            var direction = Vector2.left;

            if (Mathf.Approximately(rect.xMax, position.x))
            {
                direction = Vector2.right;
            }
            else if (Mathf.Approximately(rect.yMin, position.y))
            {
                direction = Vector2.down;
            }
            else if (Mathf.Approximately(rect.yMax, position.y))
            {
                direction = Vector2.up;
            }

            var borderX = border;
            var borderY = border;

            if (direction.y == 0)
            {
                borderX = 0;
            }
            else
            {
                borderY = 0;
            }

            // Constraint projected point to rect, taking border in account.
            position.x = Mathf.Clamp(position.x, rect.xMin + borderX, rect.xMax - borderX);
            position.y = Mathf.Clamp(position.y, rect.yMin + borderY, rect.yMax - borderY);

            return (position, direction);
        }
    }
}
