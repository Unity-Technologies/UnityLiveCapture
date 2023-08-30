using UnityEngine;
using UnityEditor;
using Unity.LiveCapture.Editor;
#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
using Unity.LiveCapture.Rendering.Editor;
using LensDistortionVolumeComponent = Unity.LiveCapture.Cameras.Rendering.LensDistortionBrownConrady;
#endif

namespace Unity.LiveCapture.Cameras.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LensDistortionBrownConrady))]
    class LensDistortionBrownConradyEditor : Editor
    {
        static class Contents
        {
            public static readonly GUIContent FakeFieldOfView = new GUIContent("Fake Field Of View", "The field of view to use for the distortion effect.");
            public static readonly GUIContent UseDistortionScale = new GUIContent("Use Distortion Scale", "Whether to use the distortion scale.");
            public static readonly GUIContent DistortionScale = new GUIContent("Distortion Scale", "The scale of the distortion effect.");
            public static readonly GUIContent RadialCoefficients = new GUIContent("Radial Coefficients", "The radial distortion coefficients.");
            public static readonly GUIContent TangentialCoefficients = new GUIContent("Tangential Coefficients", "The tangential distortion coefficients.");
            public static readonly GUIContent FixInjectionPoint = EditorGUIUtility.TrTextContentWithIcon(
                "Add effect to After Post Process in Global Settings.",
                "console.warnicon");
        }

        SerializedProperty m_FakeFieldOfView;
        SerializedProperty m_UseDistortionScale;
        SerializedProperty m_DistortionScale;
        SerializedProperty m_RadialCoefficients;
        SerializedProperty m_TangentialCoefficients;

        public void OnEnable()
        {
            m_FakeFieldOfView = serializedObject.FindProperty("m_FakeFieldOfView");
            m_UseDistortionScale = serializedObject.FindProperty("m_UseDistortionScale");
            m_DistortionScale = serializedObject.FindProperty("m_DistortionScale");
            m_RadialCoefficients = serializedObject.FindProperty("m_RadialCoefficients");
            m_TangentialCoefficients = serializedObject.FindProperty("m_TangentialCoefficients");
        }

        public override void OnInspectorGUI()
        {
#if HDRP_14_0_OR_NEWER
            if (!HDRPEditorUtilities.ContainsPostEffect<LensDistortionVolumeComponent>(CustomPostProcessInjectionPoint.AfterPostProcess))
            {
                LiveCaptureGUI.DrawFixMeBox(Contents.FixInjectionPoint, () =>
                {
                    HDRPEditorUtilities.AddPostEffect<LensDistortionVolumeComponent>(CustomPostProcessInjectionPoint.AfterPostProcess);
                });
            }
#endif
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_FakeFieldOfView, Contents.FakeFieldOfView);
            EditorGUILayout.PropertyField(m_UseDistortionScale, Contents.UseDistortionScale);

            if (m_UseDistortionScale.boolValue)
            {
                EditorGUILayout.PropertyField(m_DistortionScale, Contents.DistortionScale);
            }

            EditorGUILayout.PropertyField(m_RadialCoefficients, Contents.RadialCoefficients);
            EditorGUILayout.PropertyField(m_TangentialCoefficients, Contents.TangentialCoefficients);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
