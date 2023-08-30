using UnityEngine;

namespace Unity.CompanionAppCommon
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaRect : MonoBehaviour
    {
        [SerializeField, Tooltip("Apply the safe area calculation horizontally.")]
        bool m_ApplyHorizontally = true;

        [SerializeField, Tooltip("Apply the safe area calculation vertically.")]
        bool m_ApplyVertically;

        [SerializeField, Range(0.1f, 1f)]
        [Tooltip("A value near zero means that the safe area will not be used to calculate the new rect, while " +
            "a value of 1 means the new rect will be equal to the safe are rect.")]
        float m_Weighting = 1f;

        RectTransform m_RectTransform;
        ScreenOrientation m_Orientation = ScreenOrientation.LandscapeLeft;
        Vector2Int m_ScreenSize = Vector2Int.zero;
        Rect m_SafeArea = Rect.zero;

        void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            CheckNeedsUpdate(); // Sync cached data.
            ApplySafeArea();
        }

        void Update()
        {
            if (CheckNeedsUpdate())
            {
                ApplySafeArea();
            }
        }

        bool CheckNeedsUpdate()
        {
            // Update on screen size and safe area change for easier testing in the simulator.
            var orientation = Screen.orientation;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            var safeArea = Screen.safeArea;

            if (m_Orientation == orientation &&
                m_ScreenSize == screenSize &&
                m_SafeArea == safeArea)
            {
                return false;
            }

            m_Orientation = orientation;
            m_ScreenSize = screenSize;
            m_SafeArea = safeArea;
            return true;
        }

        void ApplySafeArea()
        {
            if (m_ApplyHorizontally)
                ApplyHorizontalSafeArea();

            if (m_ApplyVertically)
                ApplyVerticalSafeArea();
        }

        void ApplyHorizontalSafeArea()
        {
            var anchorMin = Vector2.zero;
            var anchorMax = Vector2.one;

            if (m_Orientation == ScreenOrientation.LandscapeRight)
            {
                var extend = (m_ScreenSize.x - m_SafeArea.max.x) * 0.3333f;
                var xMax = m_SafeArea.max.x + extend;
                var anchorMaxFraction = xMax / Screen.width;
                anchorMaxFraction = Mathf.Lerp(1f, anchorMaxFraction, m_Weighting);

                anchorMax = new Vector2(anchorMaxFraction, m_RectTransform.anchorMax.y);
            }

            if (m_Orientation == ScreenOrientation.LandscapeLeft)
            {
                var extend = m_SafeArea.min.x * 0.3333f;
                var xMin = m_SafeArea.min.x - extend;
                var anchorMinFraction = xMin / m_ScreenSize.x;
                anchorMinFraction = Mathf.Lerp(0f, anchorMinFraction, m_Weighting);

                anchorMin = new Vector2(anchorMinFraction, m_RectTransform.anchorMin.y);
            }

            m_RectTransform.anchorMin = anchorMin;
            m_RectTransform.anchorMax = anchorMax;
        }

        void ApplyVerticalSafeArea()
        {
            var anchorMaxFraction = m_SafeArea.max.y / m_ScreenSize.y;
            anchorMaxFraction = Mathf.Lerp(1f, anchorMaxFraction, m_Weighting);
            m_RectTransform.anchorMax = new Vector2(m_RectTransform.anchorMax.x, anchorMaxFraction);

            var anchorMinFraction = m_SafeArea.min.y / m_ScreenSize.y;
            anchorMinFraction = Mathf.Lerp(0f, anchorMinFraction, m_Weighting);
            m_RectTransform.anchorMin = new Vector2(m_RectTransform.anchorMin.x, anchorMinFraction);
        }
    }
}
