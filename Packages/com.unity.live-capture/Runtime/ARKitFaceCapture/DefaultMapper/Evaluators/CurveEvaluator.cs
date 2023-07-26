using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    /// <summary>
    /// An <see cref="IEvaluator"/> that uses an animation curve to define a custom function.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCurveEvaluator", menuName = "Live Capture/ARKit Face Capture/Evaluator/Curve", order = 5)]
    class CurveEvaluator : EvaluatorPreset
    {
        /// <inheritdoc cref="CurveEvaluator"/>
        [Serializable]
        public class Impl : IEvaluator
        {
            [SerializeField]
            AnimationCurve m_Curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 100f));

            /// <inheritdoc />
            public float Evaluate(float value)
            {
                return m_Curve.Evaluate(value);
            }

#if UNITY_EDITOR
            static class Contents
            {
                public static readonly GUIContent Curve = new GUIContent("Curve", "The curve defining a custom evaluation function. It is expected to map values in the domain [0, 1].");
            }

            /// <inheritdoc/>
            public float GetHeight()
            {
                return EditorGUIUtility.singleLineHeight;
            }

            /// <inheritdoc/>
            public void OnGUI(Rect rect)
            {
                m_Curve = EditorGUI.CurveField(rect, Contents.Curve, m_Curve);
            }

#endif
        }

        [SerializeField]
        Impl m_Evaluator = new Impl();

        /// <inheritdoc />
        public override IEvaluator Evaluator => m_Evaluator;
    }
}
