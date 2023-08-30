using System;
using System.Reflection;
using UnityEngine;

namespace Unity.LiveCapture.Setters
{
    class GenericSetter<TComponent, TValue> : Setter<TComponent, TValue>
        where TComponent : Component
        where TValue : struct
    {
        const BindingFlags k_Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        string m_PropertyName;
        object[] m_Targets;
        FieldInfo[] m_Fields;

        public override string PropertyName => m_PropertyName;

        public GenericSetter(string propertyName)
        {
            m_PropertyName = propertyName;
        }

        public override void Set(TComponent target, in TValue value)
        {
            if (m_Targets == null)
            {
                Prepare();
            }

            SetValue(target, value);
        }

        void Prepare()
        {
            var type = typeof(TComponent);
            var properties = m_PropertyName.Split('.');

            m_Targets = new object[properties.Length];
            m_Fields = new FieldInfo[properties.Length];

            for (var i = 0; i < properties.Length; ++i)
            {
                var property = properties[i];

                if (string.IsNullOrEmpty(property))
                {
                    throw new ArgumentException($"Invalid property name: {m_PropertyName}");
                }

                var field = type.GetField(property, k_Flags);

                if (field == null)
                {
                    return;
                }

                m_Fields[i] = field;
                type = field.FieldType;
            }
        }

        void SetValue(object target, object value)
        {
            m_Targets[0] = target;

            for (var i = 0; i < m_Fields.Length - 1; ++i)
            {
                var field = m_Fields[i];

                if (field == null)
                {
                    return;
                }

                m_Targets[i + 1] = field.GetValue(m_Targets[i]);
            }

            for (var i = m_Fields.Length - 1; i >= 0; --i)
            {
                target = m_Targets[i];

                if (target == null)
                {
                    return;
                }

                var field = m_Fields[i];

                if (field == null)
                {
                    return;
                }

                field.SetValue(target, value);

                value = target;
            }
        }
    }
}
