using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    // Focus distance related utilities.
    static class FocusDistanceUtility
    {
        const string k_Infinity = "<size=180%>\u221E";

        // String representation of a focus distance value.
        public static string AsString(float value, float closeFocus, out bool isInfinity, string unit = "")
        {
            var threshold = Denormalize(closeFocus, LensLimits.FocusDistance.y, .99f);

            if (value >= threshold)
            {
                isInfinity = true;
                return k_Infinity;
            }

            isInfinity = false;

            if (value < 100)
            {
                return value.ToString("F2") + unit;
            }

            if (value < 1000)
            {
                return value.ToString("F1") + unit;
            }

            return Mathf.RoundToInt(value) + unit;
        }

        public static string AsString(float value, float closeFocus, string unit = "")
        {
            return AsString(value, closeFocus, out _, unit);
        }

        // Normalizes a focus distance value belonging to the [min, max] range.
        public static float Normalize(float min, float max, float value)
        {
            return Mathf.InverseLerp(0, 1 - min / max, 1 - min / value);
        }

        // Denormalizes a focus distance value belonging to the [min, max] range.
        public static float Denormalize(float min, float max, float normalizedValue)
        {
            return min / (1 - Mathf.Lerp(0, 1 - min / max, normalizedValue));
        }
    }
}
