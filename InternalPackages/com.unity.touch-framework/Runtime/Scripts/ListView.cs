using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    /// <summary>
    /// List View widget.
    /// Implements virtualization for improved performance.
    /// </summary>
    public class ListView : MonoBehaviour
    {
        const float k_RelativeSnapDistanceTreshold = 0.1f;
        const float k_DefaultFontSize = 14f;
        const float k_DefaultSnapVelocityMultiplier = 8f;
        const float k_SnapDetectVelocityMultiplier = 2.4f;
        const float k_OutOfBoundsSnapVelocityMultiplier = 15f;
        const float k_defaultTextTint = .9f;
        const float k_FontSizeInterpolationRemap = 0.6f; // Remaps the range from [0, 1] to [m_FontSizeInterpolationRemap, 1]
        const float k_Friction = 0.135f;

        public event Action<float> selectedValueChanged = delegate {};

#pragma warning disable 649
        [SerializeField, Tooltip("List entry prefab.")]
        GameObject m_EntryPrefab;

        [SerializeField, Tooltip("Velocity applied for scrolling movement.")]
        float m_SnapVelocity = k_DefaultSnapVelocityMultiplier;

        [SerializeField, Tooltip("Number of recycled list entries.")]
        int m_ScrollEntryCount = 7;
#pragma warning restore 649

        readonly List<RectTransform> m_ScrollListItems = new List<RectTransform>();
        readonly List<TextMeshProUGUI> m_ScrollListItemTextList = new List<TextMeshProUGUI>();
        readonly Dictionary<int, float> indexToValue = new Dictionary<int, float>();
        readonly Dictionary<float, int> valueToIndex = new Dictionary<float, int>();

        bool m_IsSnapping;
        bool m_IsDragging;
        bool m_ForceDirection;
        int m_OriginalDirection;
        int m_TargetIndex;
        float m_EntryHeight;
        float m_LastPointerPosition;
        float m_LastScrollPosition;
        float m_ScrollPosition;
        float m_Velocity;
        float m_Delta;

        IFloatFormat m_FloatFormat = new TwoDecimalsFormat();

        /// <summary>
        /// Action invoked when the scroll position changes.
        /// </summary>
        public event Action<float> onScrollPositionChanged = delegate {};

        void OnBeginDrag(BaseEventData eventData)
        {
            if (enabled)
            {
                var pointerEventData = (PointerEventData)eventData;
                m_IsDragging = true;
                m_LastPointerPosition = pointerEventData.position.y;
                StopMovement();
            }
        }

        void OnDrag(BaseEventData eventData)
        {
            if (enabled)
            {
                var pointerEventData = (PointerEventData)eventData;
                var pointerPosition = pointerEventData.position.y;
                m_Delta = pointerPosition - m_LastPointerPosition;
                m_LastPointerPosition = pointerPosition;
            }
        }

        void OnEndDrag(BaseEventData eventData)
        {
            m_IsDragging = false;
            m_Delta = 0;
        }

        /// <summary>
        /// Assign scroll position.
        /// </summary>
        /// <param name="position">new scroll position</param>
        public void SetScrollPosition(float position)
        {
            if (Mathf.Abs(position - m_ScrollPosition) > Mathf.Epsilon)
            {
                m_ScrollPosition = position;
                onScrollPositionChanged.Invoke(m_ScrollPosition);
            }
        }

        /// <summary>
        /// Returns the current scroll position.
        /// </summary>
        /// <returns></returns>
        public float GetScrollPosition()
        {
            return m_ScrollPosition;
        }

        void StopMovement()
        {
            m_Velocity = 0;
        }

        void OnValidate()
        {
            m_ScrollEntryCount = Mathf.Max(m_ScrollEntryCount, 1);
        }

        void Start()
        {
            var content = transform.Find("Content");
            Assert.IsNotNull(content, "Could not access \"Content\" transform");

            EventTriggerUtility.CreateEventTrigger(content.gameObject, OnBeginDrag, EventTriggerType.BeginDrag);
            EventTriggerUtility.CreateEventTrigger(content.gameObject, OnDrag, EventTriggerType.Drag);
            EventTriggerUtility.CreateEventTrigger(content.gameObject, OnEndDrag, EventTriggerType.EndDrag);

            onScrollPositionChanged += OnScrollPositionChanged;

            StopMovement();

            Assert.IsTrue(m_ScrollEntryCount > 0, $"{nameof(m_ScrollEntryCount)} must be above 0.");

            for (var i = 0; i < m_ScrollEntryCount; ++i)
            {
                if (!indexToValue.TryGetValue(i, out var value))
                {
                    value = (float)i;
                    indexToValue[i] = value;
                }

                var entry = AllocEntryObject(content, value);
                m_ScrollListItems.Add(entry.GetComponent<RectTransform>());
                m_ScrollListItemTextList.Add(entry.GetComponentInChildren<TextMeshProUGUI>());
            }

            // Note: all entries are assumed to have the same height.
            m_EntryHeight = m_ScrollListItems[0].sizeDelta.y;

            // Prevent early return path from being taken when setting scroll position.
            m_ScrollPosition = float.MaxValue;
            SetSelectedIndex(0, false);
        }

        void OnEnable()
        {
            UpdateTextStyle();
        }

        void OnDisable()
        {
            UpdateTextStyle();
        }

        GameObject AllocEntryObject(Transform parent, float value)
        {
            var entry = Instantiate(m_EntryPrefab, parent, true);
            entry.transform.localPosition = Vector3.zero;
            entry.transform.localScale = Vector3.one;

            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            text.text = FormatFloat(value);

            return entry;
        }

        string FormatFloat(float value)
        {
            return value.ToString(m_FloatFormat.GetStringFormat(value));
        }

        /// <summary>
        /// Set List entries.
        /// </summary>
        public void SetEntries(List<float> entries)
        {
            indexToValue.Clear();
            valueToIndex.Clear();

            for (var i = 0; i < entries.Count; i++)
            {
                indexToValue.Add(i, entries[i]);
                valueToIndex.Add(entries[i], i);
            }
        }

        /// <summary>
        /// Change the default formating for float inputs.
        /// </summary>
        /// <param name="floatFormat">The float formater object that you want to use</param>
        public void SetFloatFormat(IFloatFormat floatFormat)
        {
            m_FloatFormat = floatFormat;
        }

        void OnScrollPositionChanged(float _)
        {
            var index = GetCurrentScrollPositionIndex();
            var centerElementOffset = Mathf.CeilToInt(m_ScrollEntryCount / 2f) - 1;

            // Place recycled list entries according to the scroll position.
            for (var i = 0; i < m_ScrollEntryCount; ++i)
            {
                m_ScrollListItems[i].localPosition = new Vector2(m_ScrollListItems[i].localPosition.x,
                    (-i + centerElementOffset) * m_EntryHeight - (index * m_EntryHeight) + m_ScrollPosition);
            }

            // Update recycled list entries values.
            for (var i = 0; i < m_ScrollEntryCount; ++i)
            {
                var offsetIndex = index - centerElementOffset + i;
                if (offsetIndex >= 0 && offsetIndex < indexToValue.Count)
                {
                    m_ScrollListItemTextList[i].text = FormatFloat(indexToValue[offsetIndex]);
                }
                else
                {
                    m_ScrollListItemTextList[i].text = String.Empty;
                }
            }

            UpdateTextStyle();
        }

        void Update()
        {
            var isOutOfBoundsTop = m_ScrollPosition < -m_EntryHeight / 2;
            var isOutOfBoundsBottom = m_ScrollPosition > indexToValue.Count * m_EntryHeight - m_EntryHeight / 2;

            // In case the list was scrolled out of bounds, velocity is increased so that it is brought back within bounds quickly.
            if (isOutOfBoundsTop && !m_IsDragging)
            {
                m_SnapVelocity = k_OutOfBoundsSnapVelocityMultiplier * m_EntryHeight;
                InterpolateToIndex(0);
            }
            else if (isOutOfBoundsBottom && !m_IsDragging)
            {
                m_SnapVelocity = k_OutOfBoundsSnapVelocityMultiplier * m_EntryHeight;
                InterpolateToIndex(indexToValue.Count - 1);
            }
            else
            {
                m_SnapVelocity = k_DefaultSnapVelocityMultiplier * m_EntryHeight;
            }

            if (!m_IsDragging)
            {
                // When not dragging two cases are possible:
                // 1. We are currently scrolling towards a selected entry, aka "snapping"
                // 2. Scroll movement is decelerating and we will end up snapping to the closest entry
                if (m_IsSnapping)
                {
                    InterpolateToIndex(m_TargetIndex);
                }
                else if (Mathf.Abs(m_Velocity) <= k_SnapDetectVelocityMultiplier * m_EntryHeight)
                {
                    InterpolateToIndex(GetCurrentScrollPositionIndex());
                }
            }

            // Update scroll position based on user input motion.
            if (Math.Abs(m_Delta) > Mathf.Epsilon)
                SetScrollPosition(m_ScrollPosition + m_Delta);

            var deltaTime = Time.unscaledDeltaTime;

            // When not dragging, simply update velocity and use it to update the scroll position.
            if (!m_IsDragging && Math.Abs(m_Velocity) > Mathf.Epsilon)
            {
                m_Velocity *= Mathf.Pow(k_Friction, deltaTime);

                if (Mathf.Abs(m_Velocity) < 1)
                    StopMovement();

                SetScrollPosition(m_ScrollPosition + m_Velocity * deltaTime);
            }
            // When dragging, velocity reflects user motion.
            // Remember that by this point m_ScrollPosition has been updated according to user input.
            else if (m_IsDragging)
            {
                var newVelocity = ((m_ScrollPosition - m_LastScrollPosition) / deltaTime);
                m_Velocity = Mathf.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            m_LastScrollPosition = m_ScrollPosition;
            m_Delta = 0;
        }

        // Set the index of the list entry we want to display as selected.
        // This method is responsible for snap state management:
        // when setting the selected index, we enter snap state, since we're now moving towards a target;
        // when that target has been reached, we exit snap state.
        void InterpolateToIndex(int index)
        {
            if (m_EntryHeight == 0)
                return;

            m_TargetIndex = index;
            var difference = m_TargetIndex - (m_ScrollPosition / m_EntryHeight);

            if (!m_IsSnapping)
            {
                // Store direction when entering snap state so that overshoot can be detected.
                m_OriginalDirection = Math.Sign(difference);
                m_IsSnapping = true;
            }

            // High difference = high velocity, small difference ≈ minimum velocity set by m_SnapVelocity.
            m_Velocity = Math.Sign(difference) * (m_SnapVelocity + m_EntryHeight * Mathf.Pow(difference, 2f));

            // We snap
            if (Mathf.Abs(difference) < k_RelativeSnapDistanceTreshold || m_OriginalDirection != Math.Sign(difference))
            {
                // Settle on value.
                StopMovement();
                m_IsSnapping = false;
                SetSelectedIndex(m_TargetIndex, false);
            }
        }

        /// <summary>
        /// Returns the index of the entry corresponding to the current scroll position.
        /// </summary>
        public int GetCurrentScrollPositionIndex()
        {
            return Mathf.Clamp(
                Mathf.FloorToInt((m_ScrollPosition + m_EntryHeight / 2) / m_EntryHeight), 0,
                indexToValue.Count - 1);
        }

        /// <summary>
        /// Returns the index of the index of the currently selected value.
        /// </summary>
        public int GetSelectedIndex()
        {
            return m_TargetIndex;
        }

        /// <summary>
        /// Sets the selected value by its index in the list (as opposed to its value).
        /// </summary>
        /// <param name="index">Index of the selected value.</param>
        /// <param name="shouldAnimate">Whether or not the list should animate when scrolling to the selected value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown in case the passed index does not correspond to a list entry.</exception>
        public void SetSelectedIndex(int index, bool shouldAnimate)
        {
            if (indexToValue.TryGetValue(index, out var value))
            {
                selectedValueChanged.Invoke(value);
                m_TargetIndex = index;
                if (shouldAnimate)
                {
                    InterpolateToIndex(m_TargetIndex);
                }
                else
                {
                    SetScrollPosition(m_TargetIndex * m_EntryHeight);
                }
            }
        }

        /// <summary>
        /// Sets the selected value.
        /// </summary>
        /// <param name="value">Selected value.</param>
        /// <param name="shouldAnimate">Whether or not the list should animate when scrolling to the selected value.</param>
        /// <exception cref="InvalidOperationException">Thrown in case the passed value does not correspond to a list entry.</exception>
        public void SetSelectedValue(float value, bool shouldAnimate)
        {
            if (valueToIndex.ContainsKey(value))
            {
                SetSelectedIndex(valueToIndex[value], shouldAnimate);
            }
            else
            {
                if (TryFindValueWithTolerance(value, out int index))
                {
                    SetSelectedIndex(index, shouldAnimate);
                }
                else
                {
                    Debug.LogWarning($"Value {value} is not available in the current set of entries.");
                }
            }
        }

        /// <summary>
        /// Access the selected value.
        /// </summary>
        /// <param name="value">The selected value.</param>
        /// <returns>True if a valid selected value if available.</returns>
        public bool TryGetSelectedValue(out float value) => indexToValue.TryGetValue(m_TargetIndex, out value);

        bool TryFindValueWithTolerance(float value, out int index)
        {
            foreach (var pair in valueToIndex.Where(pair => Mathf.Abs(pair.Key - value) < Mathf.Epsilon))
            {
                index = pair.Value;
                return true;
            }

            index = -1;
            return false;
        }

        void UpdateTextStyle()
        {
            var i = 0;
            foreach (var textField in m_ScrollListItemTextList)
            {
                var positionOffset = Mathf.Abs((m_ScrollListItems[i].localPosition.y / (m_ScrollEntryCount * m_EntryHeight / 2f)));
                textField.fontSize = k_DefaultFontSize * Mathf.Lerp(k_FontSizeInterpolationRemap, 1, 1 - positionOffset);

                var colorMul = Mathf.Abs(1 - positionOffset) * k_defaultTextTint;
                textField.color = enabled
                    ? Color.white * colorMul
                    : UIConfig.propertyTextInactiveColor * colorMul;

                ++i;
            }
        }
    }
}
