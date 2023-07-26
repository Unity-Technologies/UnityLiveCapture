using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Unity.TouchFramework
{
    public class DialControl : MonoBehaviour
    {
        enum MarkedEntryType
        {
            Increments,
            Manual
        }

        enum NumberType
        {
            Int,
            Float,
            Custom
        }

        /// Float to string conversion to determine entry labels.
        public interface ILabelProvider
        {
            string GetLabel(float value);

            string GetSelectedLabel(float value);

            bool isDirty { get; }

            void MarkClean();
        }

        // Default case, labels are float.
        class FloatLabelProvider : ILabelProvider
        {
            public string GetLabel(float value) => value.ToString("F2");

            public string GetSelectedLabel(float value) => GetLabel(value);

            public bool isDirty => false;

            public void MarkClean() {}
        }

        // Labels are int, rounded values.
        class IntLabelProvider : ILabelProvider
        {
            public string GetLabel(float value) => Mathf.Round(value).ToString();

            public string GetSelectedLabel(float value) => GetLabel(value);

            public bool isDirty => false;

            public void MarkClean() {}
        }

        public class FloatWithUnitLabelProvider : DialControl.ILabelProvider
        {
            string m_Unit;

            public FloatWithUnitLabelProvider(string unit)
            {
                m_Unit = unit;
            }

            public string GetLabel(float value) => value.ToString("F2");

            public string GetSelectedLabel(float value) => GetLabel(value) + m_Unit;

            public bool isDirty => false;

            public void MarkClean() {}
        }

        public interface IEntriesEvaluator
        {
            void EvaluateEntries(List<float> entries, int numEntries, float min, float max, float angularRange, IScaler scaler);
        }

        class DefaultEntriesEvaluator : IEntriesEvaluator
        {
            const float k_relativeAngleErrorMargin = .4f;

            public void EvaluateEntries(List<float> entries, int numEntries, float min, float max, float angularRange, IScaler scaler)
            {
                var angleIncrement = (2 * angularRange) / (numEntries + 1);
                var angleMargin = angleIncrement * k_relativeAngleErrorMargin;

                entries.Add(min);

                for (var i = 1; i <= numEntries; i++)
                {
                    var angle = angularRange - (angleIncrement * i);

                    var entryValue = scaler.AngleToValue(angle);
                    var rangeMin = scaler.AngleToValue(angle + angleMargin);
                    var rangeMax = scaler.AngleToValue(angle - angleMargin);

                    if (RoundNumberHeuristic(entryValue, rangeMin, rangeMax, out var roundedValue))
                    {
                        entries.Add(roundedValue);
                    }
                }

                entries.Add(max);
            }

            static bool RoundNumberHeuristic(float value, float min, float max, out float roundedValue)
            {
                if (value == 0) // log(0) = -inf
                {
                    roundedValue = 0;
                    return true;
                }

                var orderOfMagnitude = OrderOfMagnitude(value);

                for (var i = 0; i != 10; ++i)
                {
                    var base_ = .5f * Mathf.Pow(10, orderOfMagnitude - i);
                    roundedValue = RoundToBase(value, base_);

                    if (roundedValue > min && roundedValue < max)
                    {
                        return true;
                    }
                }

                roundedValue = value;
                return false;
            }

            static float OrderOfMagnitude(float value)
            {
                return Mathf.Floor(Mathf.Log(Mathf.Abs(value), 10));
            }

            static float RoundToBase(float value, float base_)
            {
                return base_ * Mathf.Round(value / base_);
            }
        }

        static readonly ILabelProvider s_FloatLabelProvider = new FloatLabelProvider();
        static readonly ILabelProvider s_IntLabelProvider = new IntLabelProvider();

        ILabelProvider m_CustomLabelProvider;
        ILabelProvider m_LabelProvider;

        IScaler m_Scaler = new DefaultScaler();

        public IScaler scaler
        {
            set
            {
                m_Scaler = value;
                ConfigureScaler();
                m_EntriesNeedUpdate = true;
            }
        }

        static readonly IEntriesEvaluator s_DefaultEntriesEvaluator = new DefaultEntriesEvaluator();
        IEntriesEvaluator m_EntriesEvaluator = s_DefaultEntriesEvaluator;

        public IEntriesEvaluator entriesEvaluator
        {
            set
            {
                m_EntriesEvaluator = value;
                m_EntriesNeedUpdate = true;
            }
        }

        public IGraduations graduations
        {
            set
            {
                m_Graduations?.Dispose();
                m_Graduations = value;
            }
        }


        [Serializable]
        public class SelectedValueChangedEvent : UnityEvent<float> {}

        SelectedValueChangedEvent m_OnSelectedValueChanged = new SelectedValueChangedEvent();

        public SelectedValueChangedEvent onSelectedValueChanged => m_OnSelectedValueChanged;

#pragma warning disable 649
        [SerializeField]
        Graphic m_DialGraphic;
        [SerializeField]
        MarkedEntryType m_MarkedEntryType = MarkedEntryType.Manual;
        [SerializeField]
        NumberType m_DisplayNumberType = NumberType.Int;
        [SerializeField]
        float m_MarkedEntryFontSize = 12;
        [SerializeField]
        Orientation m_Orientation;
        [SerializeField]
        int m_MarkedEntrySegmentCount = 5;
        [SerializeField]
        float m_MinimumValue;
        [SerializeField]
        float m_MaximumValue = 10f;
        [SerializeField]
        List<float> m_MarkedEntries = new List<float>();
        [SerializeField]
        GameObject m_MarkedEntryPrefab;
        [SerializeField]
        TextMeshProUGUI m_SelectedEntryLabel;
        [SerializeField]
        float m_AngularRange = 90f;
        [SerializeField]
        float m_DeadZoneRadius = 20;
        [SerializeField]
        Image m_ScaleGraphic;
        [SerializeField]
        bool m_TapToJumpToEntry;
        [SerializeField, Tooltip("Approximate number of graduations to be distributed along the dial.")]
        float m_ScaleDensityHint;
        [SerializeField, Tooltip("Allow selection of entries only, excluding intermediate values.")]
        bool m_RestrictSelectionToEntries;
        [SerializeField, Tooltip("Angle in degrees within which we may snap to an entry.")]
        float m_AngularSnapThreshold; // Snapping threshold is expressed as an angle since it is tied to user motion.
        [SerializeField, Tooltip("Snap to entry on pointer up.")]
        bool m_SnapOnPointerUp;
        [SerializeField, Tooltip("Snap to entry while dragging according to angular velocity.")]
        bool m_SnapOnPointerDrag;
        [SerializeField, Tooltip("Energy threshold (affected by angular velocity) determining whether or not we snap to an entry while dragging.")]
        float m_SnapKineticEnergyThreshold; // Degrees per second.
        [SerializeField, Tooltip("Angular motion threshold beyond which snap mode is exited regardless of velocity.")]
        float m_ExitSnapAngularThreshold;
        [SerializeField]
        Vector2 m_ScaleRadiusMinMax;
        [SerializeField]
        Color m_ScaleColor;
        [SerializeField]
        float m_ScaleAntialiasing;
#pragma warning restore 649

        // Angular range is exposed since it may be required by custom scalers.
        public float angularRange
        {
            set
            {
                m_AngularRange = value;
                ConfigureScaler();
            }
        }

        RectTransform m_Rect;
        Vector2 m_CenterPoint;

        bool m_Snapped;
        float m_DialAngle;
        float m_LastDialAngle;
        float m_LastSnapDialAngle;
        float m_LastDragDistance;
        float m_SelectedValue;
        float m_LastSelectedValue;
        float m_RequestedValue;
        bool m_IsValueRequested;
        bool m_IsPressed;
        bool m_EntriesNeedUpdate;
        float m_CachedAngleForLabelAlphaUpdate;
        float m_MarkedEntryRadius;
        CircleRaycastFilter m_Filter;
        IGraduations m_Graduations = new DefaultGraduation();

        struct EntryObject
        {
            public GameObject gameObject;
            public TextMeshProUGUI textField;
        }

        // Entries management.
        Stack<GameObject> m_EntryObjectPool = new Stack<GameObject>();
        List<EntryObject> m_ActiveEntryObjects = new List<EntryObject>();
        List<float> m_IncrementEntries = new List<float>();
        List<float> m_CachedCurrentEntries; // Optimization, so that snapping code does not trigger current entries update.

        // Snap-On-Drag behavior.
        float m_AngularVelocity;
        float m_LastMotionTime; // Used to compute angular velocity.
        float m_SnapKineticEnergy; // Energy level letting us determine whether we should snap or not.
        float m_LastSnapDirection;

        /// <summary>
        /// The currently selected value
        /// </summary>
        public float selectedValue
        {
            get => m_SelectedValue;
            set
            {
                if (m_IsPressed)
                {
                    m_RequestedValue = value;
                    m_IsValueRequested = true;
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }

        /// <summary>
        /// The minimum value of the dial. Setting is only valid when the <see cref="MarkedEntryType"/> is Incremental.
        /// </summary>
        public float minimumValue
        {
            get => m_MinimumValue;
            set
            {
                // Note: no early return, it is client code's responsibility to update range when appropriate
                // Once invoked, m_MinimumValue must reflect passed value
                m_EntriesNeedUpdate |= m_MinimumValue != value; // Updating entries is not free, only when needed.
                m_MinimumValue = value;
                ConfigureScaler();
                ValidateValue();
            }
        }

        /// <summary>
        /// The maximum value of the dial. Setting is only valid when the <see cref="MarkedEntryType"/> is Incremental.
        /// </summary>
        public float maximumValue
        {
            get => m_MaximumValue;
            set
            {
                // Note: no early return, it is client code's responsibility to update range when appropriate
                // Once invoked, m_MaximumValue must reflect passed value
                m_EntriesNeedUpdate |= m_MaximumValue != value; // Updating entries is not free, only when needed.
                m_MaximumValue = value;
                ConfigureScaler();
                ValidateValue();
            }
        }

        /// <summary>
        /// Set the marked entries
        /// </summary>
        /// <remarks>
        /// This will set the marked entry values regardless of the <see cref="MarkedEntryType"/> but will only be
        /// represented on the dial when the <see cref="MarkedEntryType"/> is set to Manual.
        /// </remarks>
        /// <param name="entries">A sorted array of entry values.</param>
        public void SetMarkedEntries(float[] entries)
        {
            if (m_MarkedEntryType != MarkedEntryType.Manual)
                throw new InvalidOperationException("Entries can only be set in manual mode.");

            m_MarkedEntries.Clear();
            m_MarkedEntries.AddRange(entries);

            m_MinimumValue = entries[0];
            m_MaximumValue = entries[entries.Length - 1];

            ValidateValue();
            ConfigureScaler();

            m_EntriesNeedUpdate = true;
        }

        /// <summary>
        /// Sets a custom label provider.
        /// </summary>
        /// <remarks>
        /// The numberType is set to Custom automatically.
        /// </remarks>
        /// <param name="labelProvider">The custom label provider.</param>
        public void SetCustomLabelProvider(ILabelProvider labelProvider)
        {
            m_CustomLabelProvider = labelProvider;
            m_DisplayNumberType = NumberType.Custom;
            SetLabelProvider(m_DisplayNumberType);
        }

        // The label provider depends on the number type selected.
        // Since the custom label provider may not be available,
        // we return the number type corresponding to the label provider we have selected.
        NumberType SetLabelProvider(NumberType numberType)
        {
            ILabelProvider labelProvider = null;

            switch (numberType)
            {
                case NumberType.Float:
                    labelProvider = s_FloatLabelProvider;
                    break;
                case NumberType.Int:
                    labelProvider = s_IntLabelProvider;
                    break;
                case NumberType.Custom:
                    if (m_CustomLabelProvider != null)
                    {
                        labelProvider = m_CustomLabelProvider;
                    }
                    else
                    {
                        // We can't accept custom numberType since no custom labelProvider is available.
                        // We default to float.
                        return SetLabelProvider(NumberType.Float);
                    }

                    break;
            }

            // If the label provider has changed we need to update all our labels.
            if (m_LabelProvider != labelProvider)
            {
                m_LabelProvider = labelProvider;
                m_EntriesNeedUpdate = true;
            }

            return numberType;
        }

        void Awake()
        {
            m_Rect = m_DialGraphic.rectTransform;
            m_Filter = m_DialGraphic.GetComponent<CircleRaycastFilter>();
            Assert.IsNotNull(m_Filter, "Missing CircleRaycastFilter.");

            // Position entries based on size, graduations and font size.
            var width = (transform as RectTransform).rect.width;
            m_MarkedEntryRadius = m_ScaleRadiusMinMax.x * width * .5f - m_MarkedEntryFontSize * .75f;

            var selectTransform = transform.Find("Selector") as RectTransform;
            Assert.IsNotNull(selectTransform, "Failed to retrieve Selector Background transform.");
            selectTransform.localPosition = (m_Orientation == Orientation.Left ? Vector3.left : Vector3.right) * m_MarkedEntryRadius;
            var selectorTextField = selectTransform.GetComponent<TextMeshProUGUI>();
            selectorTextField.horizontalAlignment = m_Orientation == Orientation.Left ? HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Right;

            var arrowTransform = transform.Find("Arrow") as RectTransform;
            Assert.IsNotNull(arrowTransform, "Failed to retrieve Arrow transform.");
            arrowTransform.localPosition = (m_Orientation == Orientation.Left ? Vector3.left : Vector3.right) * width * 0.5f * m_ScaleRadiusMinMax.y;
            arrowTransform.sizeDelta = Vector2.one * width * .5f * (m_ScaleRadiusMinMax.y - m_ScaleRadiusMinMax.x) * 1.1f;
            arrowTransform.localEulerAngles = new Vector3(0, 0, m_Orientation == Orientation.Left ? 0 : 180);

            // Setup scale.
            var rotation = m_ScaleGraphic.transform.rotation.eulerAngles;
            rotation.z = m_AngularRange + (m_Orientation == Orientation.Left ? -90 : 90);
            m_ScaleGraphic.transform.rotation = Quaternion.Euler(rotation);

            // List of marked entries is not expected to change, sort it once and for all.
            m_MarkedEntries.Sort();
            m_EntriesNeedUpdate = true;
        }

        // TODO: should those event be cleared?
        void Start()
        {
            EventTriggerUtility.CreateEventTrigger(m_DialGraphic.gameObject, OnBeginDrag, EventTriggerType.BeginDrag);
            EventTriggerUtility.CreateEventTrigger(m_DialGraphic.gameObject, OnDrag, EventTriggerType.Drag);
            EventTriggerUtility.CreateEventTrigger(m_DialGraphic.gameObject, OnEndDrag, EventTriggerType.EndDrag);
        }

        void OnEnable()
        {
            ConfigureScaler();
            SetLabelProvider(m_DisplayNumberType);
            ValidateValue();
        }

        void OnDestroy()
        {
            m_ActiveEntryObjects.Clear();
            m_EntryObjectPool.Clear();
            m_Graduations.Dispose();
        }

        void Update()
        {
            var labelsAlphaNeedUpdate = false;
            if (m_EntriesNeedUpdate || m_Scaler.isDirty || m_LabelProvider.isDirty)
            {
                m_EntriesNeedUpdate = false;
                m_Scaler.MarkClean();
                m_LabelProvider.MarkClean();

                UpdateEntries();
                labelsAlphaNeedUpdate = true;
            }

            var angle = m_Scaler.ValueToAngle(m_SelectedValue);
            labelsAlphaNeedUpdate |= Mathf.Abs(Mathf.DeltaAngle(angle, m_CachedAngleForLabelAlphaUpdate)) > 10e-3;
            if (labelsAlphaNeedUpdate)
            {
                m_CachedAngleForLabelAlphaUpdate = angle;
                UpdateLabelsAlpha();
            }

            m_Filter.worldRadius = m_Rect.rect.width * 0.5f * m_Rect.lossyScale.x;
            m_Filter.worldCenter = m_Rect.position;
        }

        // Dissipate kinetic energy (snap-on-drag behavior).
        void FixedUpdate()
        {
            if (!m_Snapped)
                m_SnapKineticEnergy *= 0.9f;
        }

        void UpdateGraduations(List<float> entries)
        {
            var parms = new GraduationParameters
            {
                radiusMinMax = m_ScaleRadiusMinMax,
                antialiasing = m_ScaleAntialiasing,
                color = m_ScaleColor,
                angularRange = m_AngularRange,
                orientation = m_Orientation,
                scaleDensityHint = m_ScaleDensityHint,
                entryLineWidth = 4,
                lineWidth = 2
            };

            var range = new Vector2(m_MinimumValue, m_MaximumValue);

            m_ScaleGraphic.material = m_Graduations.Update(parms, m_Scaler, range, entries);
        }

        // Returns a collection of values depending on current mode.
        // Encapsulates mode-dependent code.
        List<float> GetCurrentEntries()
        {
            if (m_MarkedEntryType == MarkedEntryType.Manual)
            {
                if (m_MarkedEntries.Count < 1)
                    throw new IndexOutOfRangeException("DialControl: entries list is empty.");
                return m_MarkedEntries;
            }

            m_IncrementEntries.Clear();

            m_EntriesEvaluator.EvaluateEntries(
                m_IncrementEntries, m_MarkedEntrySegmentCount, m_MinimumValue, m_MaximumValue, m_AngularRange, m_Scaler);

            return m_IncrementEntries;
        }

        // Returns a configured entry prefab. Fetched from pool or instantiated if pool was empty.
        GameObject AllocEntry(Transform parent, float value, float radius, float fontSize, bool interactable)
        {
            var entry = m_EntryObjectPool.Count > 0 ? m_EntryObjectPool.Pop() : Instantiate(m_MarkedEntryPrefab);
            entry.hideFlags = HideFlags.DontSave;

            var button = entry.GetComponentInChildren<Button>();
            var buttonRectTransform = button.GetComponent<RectTransform>();
            var image = button.GetComponent<Image>();
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            var angle = m_Scaler.ValueToAngle(value);

            entry.transform.SetParent(parent);
            entry.transform.localPosition = Vector3.zero;
            entry.transform.localScale = Vector3.one;

            var rectTransform = entry.GetComponent<RectTransform>();
            if (m_Orientation == Orientation.Left)
            {
                rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
                buttonRectTransform.anchoredPosition = Vector2.left * radius;
                text.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                rectTransform.localEulerAngles = new Vector3(0f, 0f, -angle);
                buttonRectTransform.anchoredPosition = Vector2.right * radius;
                text.alignment = TextAlignmentOptions.Right;
            }

            text.text = m_LabelProvider.GetLabel(value);
            text.fontSize = fontSize;

            button.interactable = interactable;
            image.raycastTarget = interactable;

            if (interactable)
            {
                button.onClick.AddListener(() => SetValueWithoutNotify(value));
            }

            return entry;
        }

        void ClearActiveEntries()
        {
            foreach (var entry in m_ActiveEntryObjects)
            {
                var button = entry.gameObject.GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                entry.gameObject.SetActive(false);
                m_EntryObjectPool.Push(entry.gameObject);
            }

            m_ActiveEntryObjects.Clear();
        }

        [ContextMenu("Update Entries")]
        void UpdateEntries()
        {
            ClearActiveEntries();
            m_CachedCurrentEntries = GetCurrentEntries();

            foreach (var val in m_CachedCurrentEntries)
            {
                var go = AllocEntry(m_Rect, val, m_MarkedEntryRadius, m_MarkedEntryFontSize, m_TapToJumpToEntry);
                go.SetActive(true);

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(text);

                m_ActiveEntryObjects.Add(new EntryObject()
                {
                    gameObject = go,
                    textField = text
                });
            }

            UpdateGraduations(m_CachedCurrentEntries);
        }

        void OnBeginDrag(BaseEventData eventData)
        {
            var pointerEventData = (PointerEventData)eventData;
            m_CenterPoint = RectTransformUtility.WorldToScreenPoint(pointerEventData.pressEventCamera, m_Rect.position);
            m_LastSelectedValue = m_SelectedValue;
            m_LastDialAngle = AngleFromPointerPosition(pointerEventData.position);
            m_LastSnapDialAngle = m_LastDialAngle;
            m_LastMotionTime = Time.time;
            m_Snapped = false;
            m_AngularVelocity = 0;
            m_LastSnapDirection = 0;
            m_SnapKineticEnergy = 0;
            m_IsPressed = true;
        }

        void OnDrag(BaseEventData eventData)
        {
            var pointerEventData = (PointerEventData)eventData;
            var pointerPosition = pointerEventData.position;
            var time = Time.time;
            var deltaTime = Mathf.Max(Mathf.Epsilon, time - m_LastMotionTime);
            var motionAngle = AngleFromPointerPosition(pointerEventData.position);

            // Dead zone.
            if (Vector2.Distance(pointerPosition, m_CenterPoint) < m_DeadZoneRadius)
            {
                m_LastDialAngle = motionAngle;
                m_LastMotionTime = time;
                return;
            }

            // Change in angle according to user motion.
            var deltaAngle = (motionAngle - m_LastDialAngle) * Mathf.Sign(pointerPosition.x - m_CenterPoint.x);

            // Translate a change in angle to a change in value.
            var newSelectedAngle = m_Scaler.ValueToAngle(m_SelectedValue) + deltaAngle;
            var newSelectedValue = m_Scaler.AngleToValue(newSelectedAngle);

            if (m_SnapOnPointerDrag)
            {
                var instantAngularVelocity = deltaAngle / deltaTime;
                m_AngularVelocity = Mathf.Lerp(m_AngularVelocity, instantAngularVelocity, 0.1f);
                m_SnapKineticEnergy += m_AngularVelocity * m_AngularVelocity * 10e-3f; // Scaling simply to make threshold more readable.

                if (m_Snapped)
                {
                    // There are 3 ways to exit snap mode.
                    // 1) Build up enough energy
                    var accumulatedEnoughEnergyToExitSnap = m_SnapKineticEnergy > m_SnapKineticEnergyThreshold;

                    // 2) Moving back to the side of the entry we were on before snapping.
                    var currentSnapDirection = Mathf.Sign(newSelectedValue - m_SelectedValue);
                    var movingBackwards = currentSnapDirection * m_LastSnapDirection < 0;

                    // 3) Move by an angle large enough (regardless of velocity)
                    var crossAngularMotionThreshold = Mathf.Abs(Mathf.DeltaAngle(m_LastSnapDialAngle, motionAngle)) > m_ExitSnapAngularThreshold;

                    // Energy boost in case we exit dial mode other than by building up energy.
                    if (movingBackwards || crossAngularMotionThreshold)
                        m_SnapKineticEnergy = m_SnapKineticEnergyThreshold * 1.2f;

                    m_Snapped = !accumulatedEnoughEnergyToExitSnap && !movingBackwards && !crossAngularMotionThreshold;
                }
                else
                {
                    // Try enter snap mode. First, has our kinetic energy fell below the threshold?
                    if (m_SnapKineticEnergy < m_SnapKineticEnergyThreshold)
                    {
                        // Then, enter snap mode if there is a value to snap to and we just crossed it.
                        var closestEntry = TrySnapToClosestEntry(newSelectedValue, m_AngularSnapThreshold, out var snappedToClosestEntry);
                        var crossing = IsWithinRange(closestEntry, m_SelectedValue, newSelectedValue);
                        if (snappedToClosestEntry && crossing)
                        {
                            m_Snapped = true;
                            m_SnapKineticEnergy = 0;
                            m_LastSnapDirection = Mathf.Sign(newSelectedValue - m_SelectedValue);
                            m_LastSnapDialAngle = motionAngle;
                            SetValueWithoutNotify(closestEntry);
                        }
                    }

                    if (!m_Snapped)
                        SetValueWithoutNotify(newSelectedValue);
                }
            }
            else
            {
                SetValueWithoutNotify(newSelectedValue);
            }

            if (m_SelectedValue != m_LastSelectedValue)
                m_OnSelectedValueChanged.Invoke(m_SelectedValue);

            m_LastDialAngle = motionAngle;
            m_LastMotionTime = time;
            m_LastSelectedValue = m_SelectedValue;
        }

        void OnEndDrag(BaseEventData eventData)
        {
            if (m_RestrictSelectionToEntries)
            {
                SetValueWithoutNotify(FindClosestEntry(m_SelectedValue));
            }
            else if (m_SnapOnPointerUp)
            {
                SetValueWithoutNotify(
                    TrySnapToClosestEntry(m_SelectedValue, m_AngularSnapThreshold, out var snapped) // TODO last arg is not used.
                );
            }

            m_IsPressed = false;

            if (m_IsValueRequested)
            {
                m_IsValueRequested = false;

                SetValueWithoutNotify(m_RequestedValue);
            }
        }

        void UpdateLabelsAlpha()
        {
            var baseRotation = m_Rect.localEulerAngles.z;
            foreach (var entry in m_ActiveEntryObjects)
            {
                var rotation = Mathf.DeltaAngle(0, baseRotation + entry.gameObject.transform.localEulerAngles.z);
                var color = entry.textField.color;
                color.a = Utilities.Smoothstep(5, 10, Mathf.Abs(rotation)); // TODO: have font-size influence those bounds.
                entry.textField.color = color;
            }
        }

        float AngleFromPointerPosition(Vector2 position)
        {
            return Vector2.Angle(m_Orientation == Orientation.Left ? Vector2.up : Vector2.down, position - m_CenterPoint);
        }

        float FindClosestEntry(float value)
        {
            Assert.IsTrue(m_CachedCurrentEntries.Count > 0);
            var bestCandidate = 0f;
            var minDist = float.MaxValue;
            for (var i = 0; i != m_CachedCurrentEntries.Count; ++i)
            {
                var dist = Mathf.Abs(m_CachedCurrentEntries[i] - value);
                if (dist < minDist)
                {
                    bestCandidate = m_CachedCurrentEntries[i];
                    minDist = dist;
                }
            }

            return bestCandidate;
        }

        float TrySnapToClosestEntry(float value, float angularSnapThreshold, out bool snapped)
        {
            var boundA = m_Scaler.AngleToValue(m_Scaler.ValueToAngle(value) - angularSnapThreshold);
            var boundB = m_Scaler.AngleToValue(m_Scaler.ValueToAngle(value) + angularSnapThreshold);
            var closestEntry = FindClosestEntry(m_SelectedValue);

            if (IsWithinRange(closestEntry, boundA, boundB))
            {
                snapped = true;
                return closestEntry;
            }

            snapped = false;
            return value;
        }

        void ConfigureScaler()
        {
            m_Scaler.Configure(m_MinimumValue, m_MaximumValue, m_AngularRange);
        }

        static bool IsWithinRange(float value, float boundA, float boundB)
        {
            var minRangeValue = Mathf.Min(boundA, boundB);
            var maxRangeValue = Mathf.Max(boundA, boundB);
            return value >= minRangeValue && value <= maxRangeValue;
        }

        void SetValueWithoutNotify(float value)
        {
            SetAndValidateValue(value);
        }

        void ValidateValue()
        {
            SetAndValidateValue(m_SelectedValue);
        }

        void SetAndValidateValue(float value)
        {
            var prevSelectedValue = m_SelectedValue;

            m_SelectedValue = Mathf.Clamp(value, m_MinimumValue, m_MaximumValue);

            if (prevSelectedValue != m_SelectedValue)
            {
                m_SelectedEntryLabel.text = m_LabelProvider.GetSelectedLabel(m_SelectedValue);
            }

            var angle = m_Scaler.ValueToAngle(m_SelectedValue);

            m_Rect.localEulerAngles = (m_Orientation == Orientation.Left ? Vector3.back : Vector3.forward) * angle;
        }
    }
}
