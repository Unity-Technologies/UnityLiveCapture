using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadRotationTabView : LinkedScroll, IGamepadRotationTabView
    {
        [SerializeField]
        Button m_ResetToOne;

        public event Action<Vector3> onSensitivityChanged = delegate {};

        static List<float> GetEntries()
        {
            var list = new List<float>();

            for (var i = 0; i < 8; ++i)
                list.Add(i * 0.25f);

            for (var i = 2; i < 25; ++i)
                list.Add(i);

            for (var i = 5; i < 10; ++i)
                list.Add(i * 5);

            for (var i = 5; i < 10; ++i)
                list.Add(i * 10);

            for (var i = 1; i <= 10; ++i)
                list.Add(i * 100);

            return list;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ResetToOne.onClick.AddListener(OnResetClicked);
            m_Vector3Scroller.onValueChanged += OnSensitivityChanged;
        }

        protected override void OnDisable()
        {
            m_ResetToOne.onClick.RemoveListener(OnResetClicked);
            m_Vector3Scroller.onValueChanged -= OnSensitivityChanged;
            base.OnDisable();
        }

        void Start()
        {
            m_Vector3Scroller.SetEntries(GetEntries());
        }

        void OnSensitivityChanged(Vector3 sensitivity)
        {
            onSensitivityChanged.Invoke(sensitivity);
        }

        void OnResetClicked()
        {
            m_Vector3Scroller.SetValue(Vector3.one);
        }

        public void SetSensitivity(Vector3 sensitivity)
        {
            m_Vector3Scroller.SetValue(sensitivity, false);
        }
    }
}
