using System;
using UnityEngine;
using Unity.LiveCapture.Setters;

namespace Unity.LiveCapture.LiveProperties
{
    interface ILiveProperty
    {
        Component Target { get; }
        PropertyBinding Binding { get; }
        ICurve Curve { get; }
        bool IsLive { get; set; }
        void Rebind(Transform root);
        void Record(double time);
        void ApplyValue();
    }

    interface ILiveProperty<TValue> : ILiveProperty where TValue : struct
    {
        bool TryGetValue(out TValue value);
        void SetValue(in TValue value);
    }

    class LiveProperty<TComponent, TValue> : ILiveProperty<TValue>
        where TComponent : Component
        where TValue : struct
    {
        TComponent m_Target;
        TValue? m_Value;
        PropertyBinding m_Binding;
        ICurve<TValue> m_Curve;
        Setter<TComponent, TValue> m_Setter;
        Action<TComponent, TValue> m_SetterAction;

        public Component Target => m_Target;
        public PropertyBinding Binding => m_Binding;
        public ICurve Curve => m_Curve;
        public bool IsLive { get; set; } = true;

        public LiveProperty(string relativePath, string propertyName, Action<TComponent, TValue> setter)
         : this(new PropertyBinding(relativePath, propertyName, typeof(TComponent)), setter) { }

        public LiveProperty(PropertyBinding binding, Action<TComponent, TValue> setter)
        {
            if (typeof(TComponent) != binding.Type)
            {
                throw new ArgumentException($"Invalid binding type. Expected {typeof(TComponent)} but received {binding.Type}");
            }

            m_Binding = binding;
            m_SetterAction = setter;

            try
            {
                m_Curve = CurveFactory.CreateCurve<TValue>();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (m_SetterAction == null)
            {
                if (!SetterResolver.TryResolve<TComponent, TValue>(binding.PropertyName, out m_Setter))
                {
                    m_Setter = new GenericSetter<TComponent, TValue>(binding.PropertyName);
                }

                m_SetterAction = SetValueUsingSetter;
            }
        }

        public bool TryGetValue(out TValue value)
        {
            value = default;

            if (m_Value.HasValue)
            {
                value = m_Value.Value;
            }

            return m_Value.HasValue;
        }

        public void SetValue(in TValue value)
        {
            m_Value = value;
        }

        public void Record(double time)
        {
            if (!IsLive || m_Curve == null || !m_Value.HasValue)
            {
                return;
            }

            m_Curve.AddKey(time, m_Value.Value);
        }

        public void Rebind(Transform root)
        {
            m_Target = null;

            if (root == null)
            {
                return;
            }

            var transform = root.Find(Binding.RelativePath);

            if (transform == null)
            {
                return;
            }

            transform.TryGetComponent<TComponent>(out m_Target);
        }

        public void ApplyValue()
        {
            if (IsLive && m_Target != null && m_Value.HasValue)
            {
                try
                {
                    m_SetterAction?.Invoke(m_Target, m_Value.Value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        void SetValueUsingSetter(TComponent target, TValue value)
        {
            m_Setter?.Set(target, in value);
        }
    }
}
