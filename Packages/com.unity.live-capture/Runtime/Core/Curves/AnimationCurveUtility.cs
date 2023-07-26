using UnityEngine;

namespace Unity.LiveCapture
{
    static class AnimationCurveUtility
    {
        const float k_DefaultWeight = 1f / 3f;
        const float k_CurveTimeEpsilon = 0.00001f;
        const float k_Bias = 0.5f;

        public static void UpdateTangents(this AnimationCurve curve, int index)
        {
            if (index < 0)
                return;

            var keyframe = curve[index];

            if (index == 0 || index == curve.length - 1)
            {
                keyframe.inTangent = 0f;
                keyframe.outTangent = 0f;

                if (curve.length > 0)
                {
                    keyframe.inWeight = k_DefaultWeight;
                    keyframe.outWeight = k_DefaultWeight;
                }
            }
            else
            {
                keyframe = UpdateTangents(keyframe, curve[index - 1], curve[index + 1]);
            }

            curve.MoveKey(index, keyframe);
        }

        public static Keyframe UpdateTangents(Keyframe keyframe, Keyframe prevKeyframe, Keyframe nextKeyframe)
        {
            var dx1 = keyframe.time - prevKeyframe.time;
            var dy1 = keyframe.value - prevKeyframe.value;
            var dx2 = nextKeyframe.time - keyframe.time;
            var dy2 = nextKeyframe.value - keyframe.value;
            var dx = dx1 + dx2;
            var dy = dy1 + dy2;
            var m1 = SafeDeltaDivide(dy1, dx1);
            var m2 = SafeDeltaDivide(dy2, dx2);
            var m = SafeDeltaDivide(dy, dx);

            if ((m1 > 0 && m2 > 0) || (m1 < 0 && m2 < 0))
            {
                var lower_bias = (1f - k_Bias) * 0.5f;
                var upper_bias = lower_bias + k_Bias;
                var lower_dy = dy * lower_bias;
                var upper_dy = dy * upper_bias;

                if (Mathf.Abs(dy1) >= Mathf.Abs(upper_dy))
                {
                    var b = SafeDeltaDivide(dy1 - upper_dy, lower_dy);
                    var mp = (1f - b) * m;

                    keyframe.inTangent = mp;
                    keyframe.outTangent = mp;
                }
                else if (Mathf.Abs(dy1) < Mathf.Abs(lower_dy))
                {
                    var b = SafeDeltaDivide(dy1, lower_dy);
                    var mp = b * m;

                    keyframe.inTangent = mp;
                    keyframe.outTangent = mp;
                }
                else
                {
                    keyframe.inTangent = m;
                    keyframe.outTangent = m;
                }
            }
            else if (dx1 == 0 || dx2 == 0)
            {
                keyframe.inTangent = m1;
                keyframe.outTangent = m2;
            }
            else
            {
                keyframe.inTangent = 0;
                keyframe.outTangent = 0;
            }

            keyframe.inWeight = k_DefaultWeight;
            keyframe.outWeight = k_DefaultWeight;

            return keyframe;
        }

        static float SafeDeltaDivide(float y, float x)
        {
            if (Mathf.Abs(x) > k_CurveTimeEpsilon)
                return y / x;
            else
                return 0f;
        }
    }
}
