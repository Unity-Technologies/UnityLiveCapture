using System.Collections;
using UnityEngine;

namespace Unity.TouchFramework
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollToSection : MonoBehaviour
    {
        RectTransform m_ContentTransform;
        RectTransform m_ViewportTransform;
        ExpandableContainer[] m_Sections;
        Coroutine m_Coroutine;

        void Awake()
        {
            m_ViewportTransform = GetComponent<RectTransform>();
            m_ContentTransform = transform.Find("Content").GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            m_Sections = GetComponentsInChildren<ExpandableContainer>();
            foreach (var section in m_Sections)
            {
                section.onExpand += OnSectionExpand;
            }
        }

        void OnDisable()
        {
            StopAnimationIfNeeded();

            foreach (var section in m_Sections)
            {
                section.onExpand -= OnSectionExpand;
            }
        }

        void OnSectionExpand(ExpandableContainer section)
        {
            ScrollTo(section.transform as RectTransform);
        }

        void ScrollTo(RectTransform section)
        {
            StopAnimationIfNeeded();
            m_Coroutine = StartCoroutine(ScrollAnimation(section));
        }

        IEnumerator ScrollAnimation(RectTransform section)
        {
            // Wait one frame for the content height to be updated.
            yield return null;

            var startValue = m_ContentTransform.anchoredPosition.y;

            if (!GetScrollPosition(section, out var targetValue))
            {
                // No animation needed, the section is already visible.
                yield break;
            }

            var duration = .3f;
            var startTime = Time.time;
            var t = 0f;

            do
            {
                yield return null;

                t = Mathf.Clamp01((Time.time - startTime) / duration);

                t = -t * (t - 2f);

                var interpolatedValue = Mathf.Lerp(startValue, targetValue, t);

                m_ContentTransform.anchoredPosition = new Vector2(0, interpolatedValue);
            }
            while (t < 1);
        }

        void StopAnimationIfNeeded()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        bool GetScrollPosition(RectTransform section, out float value)
        {
            var viewportHeight = m_ViewportTransform.rect.height;
            var scrollPosition = m_ContentTransform.anchoredPosition.y;

            var elementTop = section.anchoredPosition.y;
            var elementBottom = elementTop - section.rect.height;

            var visibleContentTop = -scrollPosition;
            var visibleContentBottom = -scrollPosition - viewportHeight;

            var scrollDelta =
                elementTop > visibleContentTop ? visibleContentTop - elementTop :
                elementBottom < visibleContentBottom ? visibleContentBottom - elementBottom :
                0f;

            scrollPosition += scrollDelta;
            value = scrollPosition;
            return scrollDelta != 0;
        }
    }
}
