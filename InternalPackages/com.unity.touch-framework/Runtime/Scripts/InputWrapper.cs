using System;
using TMPro;

namespace Unity.TouchFramework
{
    public class InputWrapper<T> : IDisposable
    {
        public event Action<T> onValueChanged = delegate {};

        TMP_InputField m_Field;
        T m_CachedValue;

        protected virtual string ValueToString(T value) => String.Empty;

        protected virtual bool StringToValue(string strValue, out T value)
        {
            value = default;
            return false;
        }

        public InputWrapper(TMP_InputField field)
        {
            m_Field = field;
            m_Field.inputType = TMP_InputField.InputType.Standard;

            m_Field.onDeselect.AddListener(OnDeselect);
            m_Field.onEndEdit.AddListener(OnEndEdit);
        }

        public void SetValue(T value)
        {
            m_CachedValue = value;
            m_Field.SetTextWithoutNotify(ValueToString(value));
        }

        public void Dispose()
        {
            m_Field.onEndEdit.RemoveListener(OnEndEdit);
            m_Field.onDeselect.RemoveListener(OnDeselect);
        }

        void OnEndEdit(string strValue)
        {
            if (StringToValue(strValue, out var value))
            {
                m_CachedValue = value;
                onValueChanged.Invoke(value);
            }
            else
            {
                SetValue(m_CachedValue);
            }
        }

        void OnDeselect(string value) => SetValue(m_CachedValue);
    }

    public class FloatInputWrapper : InputWrapper<float>
    {
        public FloatInputWrapper(TMP_InputField field) : base(field)
        {
            field.contentType = TMP_InputField.ContentType.DecimalNumber;
        }

        protected override string ValueToString(float value) => value.ToString("F2");

        protected override bool StringToValue(string strValue, out float value)
        {
            return float.TryParse(strValue, out value);
        }
    }

    public class IntInputWrapper : InputWrapper<int>
    {
        public IntInputWrapper(TMP_InputField field) : base(field)
        {
            field.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        protected override string ValueToString(int value) => value.ToString("D");

        protected override bool StringToValue(string strValue, out int value)
        {
            return int.TryParse(strValue, out value);
        }
    }
}
