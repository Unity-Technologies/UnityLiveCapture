using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;

namespace Unity.CompanionAppCommon
{
    /// <summary>
    /// Utility ui that simplify edition of vector3 values within a scroller
    /// </summary>
    class Vector3Scroller : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        ListView m_YAxisControlData;
        [SerializeField]
        ListView m_XAxisControlData;
        [SerializeField]
        ListView m_ZAxisControlData;
#pragma warning restore 649

        float m_XValue;
        float m_YValue;
        float m_ZValue;
        bool m_IsDirty;
        Axis m_LastAxisChanged = Axis.X;

        /// <summary>
        /// Called when one of the values of the scroller have changed.
        /// </summary>
        public Action<Vector3> onValueChanged = delegate {};

        static List<float> GetDefaultEntries()
        {
            var list = new List<float>();

            for (var i = 1; i < 100; ++i)
                list.Add(i);

            return list;
        }

        void Awake()
        {
            var entries = GetDefaultEntries();
            SetEntries(entries);

            m_YAxisControlData.selectedValueChanged += (f) =>
            {
                if (Mathf.Abs(m_YValue - f) > Mathf.Epsilon)
                {
                    m_LastAxisChanged = Axis.Y;
                    m_YValue = f;
                    m_IsDirty = true;
                }
            };

            m_XAxisControlData.selectedValueChanged += (f) =>
            {
                if (Math.Abs(m_XValue - f) > Mathf.Epsilon)
                {
                    m_LastAxisChanged = Axis.X;
                    m_XValue = f;
                    m_IsDirty = true;
                }
            };

            m_ZAxisControlData.selectedValueChanged += (f) =>
            {
                if (Math.Abs(m_ZValue - f) > Mathf.Epsilon)
                {
                    m_LastAxisChanged = Axis.Z;
                    m_ZValue = f;
                    m_IsDirty = true;
                }
            };
        }

        /// <summary>
        /// Returns the last axis edited on the list view
        /// </summary>
        /// <returns>The last axis that was changed. The enum is X, Y or Z</returns>
        public Axis LastAxisChanged()
        {
            return m_LastAxisChanged;
        }

        /// <summary>
        /// Changes the default formatting of the floats values in the list view.
        /// </summary>
        /// <param name="floatFormat">The float formater you want to use.</param>
        public void SetFloatFormat(IFloatFormat floatFormat)
        {
            m_XAxisControlData.SetFloatFormat(floatFormat);
            m_YAxisControlData.SetFloatFormat(floatFormat);
            m_ZAxisControlData.SetFloatFormat(floatFormat);
        }

        void Update()
        {
            if (m_IsDirty)
                onValueChanged(GetValue());

            m_IsDirty = false;
        }

        /// <summary>
        /// Returns the result of the user input.
        /// </summary>
        /// <returns> The current values of the scroller.</returns>
        public Vector3 GetValue()
        {
            return new Vector3(m_XValue, m_YValue, m_ZValue);
        }

        /// <summary>
        /// Sets the granularity of the scroller.
        /// </summary>
        /// <param name="entries">Each float represents one step.</param>
        public void SetEntries(List<float> entries)
        {
            m_YAxisControlData.SetEntries(entries);
            m_XAxisControlData.SetEntries(entries);
            m_ZAxisControlData.SetEntries(entries);
        }

        /// <summary>
        /// Updates the X axis to the targeted value with an optional animation.
        /// </summary>
        /// <param name="value">The value to set the X axis.</param>
        /// <param name="animate">If true, animates to the updated value.param>
        public void SetXValue(float value, bool animate = true)
        {
            m_XAxisControlData.SetSelectedValue(value, animate);
            m_XValue = value;
        }

        /// <summary>
        /// Updates the Y axis to the targeted value with an optional animation.
        /// </summary>
        /// <param name="value">The value to set the Y axis.</param>
        /// <param name="animate">If true, animates to the updated value.</param>
        public void SetYValue(float value, bool animate = true)
        {
            m_YAxisControlData.SetSelectedValue(value, animate);
            m_YValue = value;
        }

        /// <summary>
        /// Updates the Z axis to the targeted value with an optional animation.
        /// </summary>
        /// <param name="value">The value to set for the Z axis.</param>
        /// <param name="animate">If true, animates to the updated value.</param>
        public void SetZValue(float value, bool animate = true)
        {
            m_ZAxisControlData.SetSelectedValue(value, animate);
            m_ZValue = value;
        }

        /// <summary>
        /// Overrides the value of the scroller with an optional animation.
        /// </summary>
        /// <param name="value">The value to set for all 3 axis.</param>
        /// <param name="animate">If true, animates to the updated value.</param>
        public void SetValue(Vector3 values, bool animate = true)
        {
            SetYValue(values.y, animate);
            SetXValue(values.x, animate);
            SetZValue(values.z, animate);
        }
    }
}
