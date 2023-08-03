using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine.Assertions;

namespace Unity.CompanionApps.VirtualCamera
{
    class DampingView : LinkedScroll, IDampingView
    {
#pragma warning disable 649
        [SerializeField]
        MinMaxPropertyControl m_AimField;
        [SerializeField]
        SlideToggle m_EnableDamping;
#pragma warning restore 649

        readonly List<CanvasGroup> m_CanvasGroups = new List<CanvasGroup>();

        public event Action<Vector3> onBodyDampingChanged = delegate {};
        public event Action<float> onAimChanged = delegate {};
        public event Action<bool> onDampingEnabledChanged = delegate {};

        static List<float> GetEntries()
        {
            var list = new List<float>();

            for (var i = 0; i < 61; ++i)
                list.Add(i * 0.05f);

            list.Add(4);
            list.Add(5);
            return list;
        }

        void Awake()
        {
            SetDampingEnabled(m_EnableDamping.on); // Sync.

            var aimCanvasGroup = m_AimField.GetComponent<CanvasGroup>();
            Assert.IsNotNull(aimCanvasGroup);
            var vector3ScrollerCanvasGroup = m_Vector3Scroller.GetComponent<CanvasGroup>();
            Assert.IsNotNull(vector3ScrollerCanvasGroup);

            m_CanvasGroups.Add(aimCanvasGroup);
            m_CanvasGroups.Add(vector3ScrollerCanvasGroup);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_AimField.onFloatValueChanged.AddListener(OnAimChanged);
            m_EnableDamping.onValueChanged.AddListener(OnEnabledChanged);
            m_Vector3Scroller.onValueChanged += Vector3ValueChanged;
        }

        protected override void OnDisable()
        {
            m_AimField.onFloatValueChanged.RemoveListener(OnAimChanged);
            m_EnableDamping.onValueChanged.RemoveListener(OnEnabledChanged);
            m_Vector3Scroller.onValueChanged -= Vector3ValueChanged;
            base.OnDisable();
        }

        void Start()
        {
            m_Vector3Scroller.SetEntries(GetEntries());
        }

        void OnAimChanged(float value)
        {
            onAimChanged.Invoke(value);
        }

        void OnEnabledChanged(bool on)
        {
            onDampingEnabledChanged.Invoke(on);
        }

        void Vector3ValueChanged(Vector3 value)
        {
            onBodyDampingChanged.Invoke(value);
        }

        public void SetBodyDamping(Vector3 value)
        {
            m_Vector3Scroller.SetValue(value, gameObject.activeInHierarchy);
        }

        public void SetAimDamping(float value)
        {
            m_AimField.SetValue(value);
        }

        public void SetDampingEnabled(bool value)
        {
            m_EnableDamping.on = value;

            foreach (var group in m_CanvasGroups)
            {
                group.interactable = value;
                group.alpha = value ? 1 : .2f;
            }
        }
    }
}
