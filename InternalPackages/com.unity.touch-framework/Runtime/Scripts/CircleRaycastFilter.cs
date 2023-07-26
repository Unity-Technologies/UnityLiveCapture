using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.TouchFramework
{
    [RequireComponent(typeof(RectTransform))]
    public class CircleRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
    {
#pragma warning disable 649
        [SerializeField]
        bool m_FitLocalRect;
#pragma warning disable 649

        Vector2 m_WorldCenter;
        float m_WorldRadius;
        RectTransform m_Rect;

        public Vector2 worldCenter
        {
            set { m_WorldCenter = value; }
        }

        public float worldRadius
        {
            set { m_WorldRadius = value; }
        }

        void Awake()
        {
            m_Rect = GetComponent<RectTransform>();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(m_WorldCenter, Vector3.forward, m_WorldRadius);
        }

#endif

        void Update()
        {
            if (m_FitLocalRect)
            {
                m_WorldCenter = m_Rect.TransformPoint(m_Rect.rect.center);
                var halfWidth = new Vector3(m_Rect.rect.width * 0.5f, 0, 0);
                m_WorldRadius = m_Rect.TransformVector(halfWidth).magnitude;
            }
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_Rect, sp, eventCamera, out worldPoint))
            {
                var distToCenter = (m_WorldCenter - new Vector2(worldPoint.x, worldPoint.y)).magnitude;
                if (distToCenter > m_WorldRadius)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
