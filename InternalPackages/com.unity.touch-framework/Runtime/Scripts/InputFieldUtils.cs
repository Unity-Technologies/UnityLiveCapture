using TMPro;
using UnityEngine;

namespace Unity.TouchFramework
{
    public static class InputFieldUtils
    {
        public static void AssignText(this TMP_InputField input, int value)
        {
            input.text = value.ToString();
        }

        public static void AssignText(this TMP_InputField input, float value, string format = "F1")
        {
            input.text = value.ToString(format);
        }

        public static bool ValidateRange(this TMP_InputField input, int min, int max, int defaultValue, out int output)
        {
            if (int.TryParse(input.text, out var rawValue))
            {
                var validValue = Mathf.Clamp(rawValue, min, max);
                if (validValue != rawValue)
                    AssignText(input, validValue);
                output = validValue;
                return true;
            }

            output = defaultValue;
            AssignText(input, defaultValue);
            return false;
        }

        public static bool ValidateRange(this TMP_InputField input, float min, float max, float defaultValue, out float output)
        {
            if (float.TryParse(input.text, out var rawValue))
            {
                var validValue = Mathf.Clamp(rawValue, min, max);
                if (validValue != rawValue)
                    AssignText(input, validValue);
                output = validValue;
                return true;
            }

            output = defaultValue;
            AssignText(input, defaultValue);
            return false;
        }
    }
}
