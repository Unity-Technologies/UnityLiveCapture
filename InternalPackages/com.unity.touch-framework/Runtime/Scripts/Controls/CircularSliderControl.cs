using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class CircularSliderControl : MonoBehaviour
    {
        [Serializable]
        public class SelectedValueChangedEvent : UnityEvent<float> {}
        SelectedValueChangedEvent m_OnSelectedValueChanged = new SelectedValueChangedEvent();
        public SelectedValueChangedEvent onSelectedValueChanged => m_OnSelectedValueChanged;

#pragma warning disable CS0649
        [SerializeField]
        Graphic m_Graphic;
        [SerializeField]
        float m_MinimumValue;
        [SerializeField]
        float m_MaximumValue;
        [SerializeField]
        Color m_Color;
        [SerializeField]
        Color m_BackgroundColor;
        [SerializeField]
        float m_Radius;
        [SerializeField]
        float m_LineWidth;
        [SerializeField]
        float m_HandleRadius;
        [SerializeField]
        float m_AngularRange;
        [SerializeField, Range(0.001f, 2)]
        float m_Antialiasing;
        [SerializeField]
        float m_DeadZoneRadius = 20;
        [SerializeField]
        Vector2 m_ScaleRadiusMinMax;
        [SerializeField]
        Color m_ScaleColor;
        [SerializeField]
        int m_GraduationSteps;
        [SerializeField]
        float m_ScaleDensityHint;
#pragma warning restore CS0649

        readonly DefaultScaler m_Scaler = new DefaultScaler();

        IGraduations m_Graduations = new DefaultGraduation();
        List<float> m_Entries = new List<float>();
        float m_SelectedValue;
        RectTransform m_Rect;
        Image m_ScaleImage;
        Material m_SliderMaterial;
        CircleRaycastFilter m_Filter;
        bool m_PositionNeedsUpdate;

        static readonly Vector2 k_GraduationRange = new Vector2(0, 1);

        static readonly int k_ShaderColor = Shader.PropertyToID("_Color");
        static readonly int k_ShaderbackgroundColor = Shader.PropertyToID("_BackgroundColor");
        static readonly int k_ShaderRadius = Shader.PropertyToID("_Radius");
        static readonly int k_ShaderHandleRadius = Shader.PropertyToID("_HandleRadius");
        static readonly int k_ShaderLineWidth = Shader.PropertyToID("_LineWidth");
        static readonly int k_ShaderPosition = Shader.PropertyToID("_Position");
        static readonly int k_ShaderScale = Shader.PropertyToID("_Scale");
        static readonly int k_ShaderRotation = Shader.PropertyToID("_Rotation");
        static readonly int k_ShaderAntialiasing = Shader.PropertyToID("_Antialiasing");

        public float selectedValue
        {
            get => m_SelectedValue;
            set
            {
                m_SelectedValue = Mathf.Clamp(value, m_MinimumValue, m_MaximumValue);
                m_PositionNeedsUpdate = true;
            }
        }

        public float minimumValue
        {
            get => m_MinimumValue;
            set
            {
                m_MinimumValue = value;
                m_PositionNeedsUpdate = true;
            }
        }

        public float maximumValue
        {
            get => m_MaximumValue;
            set
            {
                m_MaximumValue = value;
                m_PositionNeedsUpdate = true;
            }
        }

        void Awake()
        {
            m_PositionNeedsUpdate = true;
            m_Rect = m_Graphic.rectTransform;
            m_Filter = m_Graphic.GetComponent<CircleRaycastFilter>();
            Assert.IsNotNull(m_Filter, "Missing CircleRaycastFilter.");

            // TODO: use shader directly.
            var circularSliderMaterial = Resources.Load<Material>("Materials/CircularSlider");
            Assert.IsNotNull(circularSliderMaterial);
            m_SliderMaterial = new Material(circularSliderMaterial);
            m_SliderMaterial.hideFlags = HideFlags.DontSave;
            UpdateSliderMaterial();
            var sliderImg = transform.Find("Slider").GetComponent<Image>();
            sliderImg.material = m_SliderMaterial;

            m_ScaleImage = transform.Find("Scale").GetComponent<Image>();
            m_ScaleImage.transform.localRotation = Quaternion.Euler(0, 0, m_AngularRange * 2 + (180 - m_AngularRange * 2) * 0.5f);
            UpdateGraduations();
        }

        void OnValidate()
        {
            if (m_SliderMaterial != null)
                UpdateSliderMaterial();
            if (m_ScaleImage != null)
                UpdateGraduations();
        }

        void OnDestroy()
        {
            Destroy(m_SliderMaterial);
            m_Graduations.Dispose();
            m_Entries.Clear();
        }

        void Start()
        {
            EventTriggerUtility.CreateEventTrigger(m_Graphic.gameObject, OnControlPointerDown, EventTriggerType.PointerDown);
            EventTriggerUtility.CreateEventTrigger(m_Graphic.gameObject, OnControlDrag, EventTriggerType.Drag);
            EventTriggerUtility.CreateEventTrigger(m_Graphic.gameObject, OnControlPointerUp, EventTriggerType.PointerUp);
        }

        void Update()
        {
            if (m_PositionNeedsUpdate)
            {
                m_PositionNeedsUpdate = false;
                var normalizedValue = (m_SelectedValue - m_MinimumValue) / (m_MaximumValue - m_MinimumValue);
                m_SliderMaterial.SetFloat(k_ShaderPosition, normalizedValue);
            }

            m_Filter.worldRadius = m_Rect.rect.width * 0.5f * m_Rect.lossyScale.x;
            m_Filter.worldCenter = m_Rect.position;
        }

        void UpdateGraduations()
        {
            var parms = new GraduationParameters
            {
                radiusMinMax = m_ScaleRadiusMinMax,
                antialiasing = m_Antialiasing,
                color = m_ScaleColor,
                angularRange = m_AngularRange,
                orientation = Orientation.Right,
                scaleDensityHint = m_ScaleDensityHint,
                entryLineWidth = 8,
                lineWidth = 8
            };

            m_Entries.Clear();
            for (var i = 0; i < m_GraduationSteps + 1; ++i)
                m_Entries.Add(i / (float)m_GraduationSteps);

            m_Scaler.Configure(m_MinimumValue, m_MaximumValue, m_AngularRange);
            m_ScaleImage.material = m_Graduations.Update(parms, m_Scaler, k_GraduationRange, m_Entries);
        }

        void UpdateSliderMaterial()
        {
            // TODO: pack values to reduce number of uniforms.
            m_SliderMaterial.SetColor(k_ShaderColor, m_Color);
            m_SliderMaterial.SetColor(k_ShaderbackgroundColor, m_BackgroundColor);
            m_SliderMaterial.SetFloat(k_ShaderRadius, m_Radius);
            m_SliderMaterial.SetFloat(k_ShaderHandleRadius, m_HandleRadius);
            m_SliderMaterial.SetFloat(k_ShaderLineWidth, m_LineWidth);
            m_SliderMaterial.SetFloat(k_ShaderAntialiasing, m_Antialiasing);
            m_SliderMaterial.SetFloat(k_ShaderScale, GetNormalizedScale(m_AngularRange));
            m_SliderMaterial.SetFloat(k_ShaderRotation, m_AngularRange * Mathf.Deg2Rad);
        }

        void OnControlPointerDown(BaseEventData eventData) { HandleEvent(eventData); }
        void OnControlDrag(BaseEventData eventData) { HandleEvent(eventData); }
        void OnControlPointerUp(BaseEventData eventData) { HandleEvent(eventData); }

        void HandleEvent(BaseEventData eventData)
        {
            var pointerEventData = (PointerEventData)eventData;
            var centerPoint = RectTransformUtility.WorldToScreenPoint(pointerEventData.pressEventCamera, m_Rect.position);
            var dist = pointerEventData.position - centerPoint;

            if (dist.magnitude > m_DeadZoneRadius)
            {
                var rotationRad = Mathf.Deg2Rad * m_AngularRange;
                var angle = Mathf.Atan2(dist.y, dist.x);
                angle = Mathf.DeltaAngle(angle, rotationRad);

                // Filter events based on angle.
                if (angle < -0.1 || angle > 2 * Mathf.PI * GetNormalizedScale(m_AngularRange) + 0.1)
                    return;

                angle = Mathf.Max(0, angle); // Prevent wrap around 0.

                var normalizedAngle = ((angle / (2 * Mathf.PI)) + 1) % 1;
                var normalizedPosition = Mathf.Clamp01(normalizedAngle / GetNormalizedScale(m_AngularRange));
                var newSelectedValue = Mathf.Lerp(m_MinimumValue, m_MaximumValue, normalizedPosition);

                var selectedValueHasChanged = newSelectedValue != selectedValue;
                selectedValue = newSelectedValue;

                if (selectedValueHasChanged)
                    m_OnSelectedValueChanged.Invoke(selectedValue);
            }
        }

        float GetNormalizedScale(float angularRange) { return angularRange * 2 / 360f; }
    }
}
