using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    /// <summary>
    /// Defines how a <see cref="FaceBlendShape"/> affects a blend shape on a skinned mesh.
    /// </summary>
    [Serializable]
    class BindingConfig : IDrawable
    {
        enum Type
        {
            Simple,
            Curve,
        }

        [SerializeField, Tooltip("Whether the smoothing value set on this binding overrides the default value for the mapper.")]
        bool m_OverrideSmoothing;
        [SerializeField]
        float m_Smoothing = 0.1f;
        [SerializeField]
        EvaluatorPreset m_EvaluatorPreset = null;
        [SerializeField]
        Type m_Type = Type.Simple;
        [SerializeField]
        SimpleEvaluator.Impl m_SimpleEvaluator = new SimpleEvaluator.Impl();
        [SerializeField]
        CurveEvaluator.Impl m_CurveEvaluator = new CurveEvaluator.Impl();

        /// <summary>
        /// The amount of smoothing to apply to the blend shape value, with a value in the range [0, 1].
        /// </summary>
        public float Smoothing => m_Smoothing;

        /// <summary>
        /// Whether the smoothing set on this binding overrides the global value.
        /// </summary>
        public bool OverrideSmoothing => m_OverrideSmoothing;

        /// <summary>
        /// Creates a new <see cref="BindingConfig"/> instance.
        /// </summary>
        /// <param name="preset">The preset evaluation function to use, or null to use a custom function.</param>
        public BindingConfig(EvaluatorPreset preset)
        {
            m_EvaluatorPreset = preset;
        }

        /// <summary>
        /// Gets the evaluation function defined by this configuration.
        /// </summary>
        public IEvaluator GetEvaluator()
        {
            if (m_EvaluatorPreset != null)
                return m_EvaluatorPreset.Evaluator;

            switch (m_Type)
            {
                case Type.Simple:
                    return m_SimpleEvaluator;
                case Type.Curve:
                    return m_CurveEvaluator;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if UNITY_EDITOR
        static class Contents
        {
            public static readonly GUIContent OverrideSmoothing = new GUIContent("Override Smoothing", "Whether the smoothing value set on this binding overrides the default value for the mapper.");
            public static readonly GUIContent Smoothing = new GUIContent("Smoothing", "The amount of smoothing to apply to the blend shape value. " +
                "It can help reduce jitter in the face capture, but it will also smooth out fast motions.");
            public static readonly GUIContent EvaluatorPreset = new GUIContent("Evaluator Preset", "A preset evaluation function to use. " +
                "If none is assigned, a new function must be configured for this blend shape.");
            public static readonly GUIContent Type = new GUIContent("Type", "The type of evaluation function to use when a preset is not assigned.");
        }

        /// <inheritdoc/>
        public float GetHeight()
        {
            const int lines = 3;
            var height = (lines * EditorGUIUtility.singleLineHeight) + ((lines - 1) * EditorGUIUtility.standardVerticalSpacing);

            if (m_EvaluatorPreset == null)
            {
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.standardVerticalSpacing + GetEvaluator().GetHeight();
            }

            return height;
        }

        /// <inheritdoc/>
        public void OnGUI(Rect rect)
        {
            var line = rect;
            line.height = EditorGUIUtility.singleLineHeight;

            m_OverrideSmoothing = EditorGUI.Toggle(line, Contents.OverrideSmoothing, m_OverrideSmoothing);
            GUIUtils.NextLine(ref line);

            using (new EditorGUI.DisabledScope(!m_OverrideSmoothing))
            {
                EditorGUI.indentLevel++;
                m_Smoothing = EditorGUI.Slider(line, Contents.Smoothing, m_Smoothing, 0f, 1f);
                EditorGUI.indentLevel--;
            }

            GUIUtils.NextLine(ref line);
            m_EvaluatorPreset = EditorGUI.ObjectField(line, Contents.EvaluatorPreset, m_EvaluatorPreset, typeof(EvaluatorPreset), false) as EvaluatorPreset;

            if (m_EvaluatorPreset == null)
            {
                GUIUtils.NextLine(ref line);
                m_Type = (Type)EditorGUI.EnumPopup(line, Contents.Type, m_Type);

                var evaluatorRect = rect;
                evaluatorRect.yMin = line.yMax;
                GetEvaluator().OnGUI(evaluatorRect);
            }
        }

#endif
    }
}
