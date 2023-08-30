using UnityEditor;

namespace Unity.LiveCapture.ARKitFaceCapture.Editor
{
    [CustomEditor(typeof(FaceActor))]
    class FaceActorEditor : UnityEditor.Editor
    {
        SerializedProperty m_Mapper;
        SerializedProperty m_EnabledChannels;

        SerializedProperty m_BlendShapes;
        SerializedProperty m_HeadPosition;
        SerializedProperty m_HeadOrientation;
        SerializedProperty m_LeftEyeOrientation;
        SerializedProperty m_RightEyeOrientation;

        void OnEnable()
        {
            m_Mapper = serializedObject.FindProperty("m_Mapper");
            m_EnabledChannels = serializedObject.FindProperty("m_EnabledChannels");

            m_BlendShapes = serializedObject.FindProperty("m_BlendShapes");
            m_HeadPosition = serializedObject.FindProperty("m_HeadPosition");
            m_HeadOrientation = serializedObject.FindProperty("m_HeadOrientation");
            m_LeftEyeOrientation = serializedObject.FindProperty("m_LeftEyeOrientation");
            m_RightEyeOrientation = serializedObject.FindProperty("m_RightEyeOrientation");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_Mapper);

                if (change.changed)
                {
                    foreach (var target in targets)
                    {
                        if (target is FaceActor actor)
                        {
                            actor.ClearCache();
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(m_EnabledChannels);

            var channels = (FaceChannelFlags)m_EnabledChannels.intValue;

            if (channels != FaceChannelFlags.None)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current Pose", EditorStyles.boldLabel);

                using var _ = new EditorGUI.IndentLevelScope(1);

                if (channels.HasFlag(FaceChannelFlags.BlendShapes))
                {
                    EditorGUILayout.PropertyField(m_BlendShapes);
                }
                if (channels.HasFlag(FaceChannelFlags.HeadPosition))
                {
                    EditorGUILayout.PropertyField(m_HeadPosition);
                }
                if (channels.HasFlag(FaceChannelFlags.HeadRotation))
                {
                    EditorGUILayout.PropertyField(m_HeadOrientation);
                }
                if (channels.HasFlag(FaceChannelFlags.Eyes))
                {
                    EditorGUILayout.PropertyField(m_LeftEyeOrientation);
                    EditorGUILayout.PropertyField(m_RightEyeOrientation);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
