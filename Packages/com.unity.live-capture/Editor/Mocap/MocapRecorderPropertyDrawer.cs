using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Mocap.Editor
{
    [CustomPropertyDrawer(typeof(MocapRecorder))]
    class MocapRecorderPropertyDrawer : PropertyDrawer
    {
        const float k_Separator = 2f;

        static class Contents
        {
            public static readonly GUIContent Channels = new GUIContent("Channels", "Display a list of transform data channels for this device.");
            public static readonly GUIContent Position = new GUIContent("P", "Position");
            public static readonly GUIContent Rotation = new GUIContent("R", "Rotation");
            public static readonly GUIContent Scale = new GUIContent("S", "Scale");
        }

        SerializedProperty m_Animator;

        float LineHeight => EditorGUIUtility.singleLineHeight;
        float RowHeight => LineHeight + k_Separator;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineCount = 1;

            if (property.isExpanded)
            {
                var arraySize = property.FindPropertyRelative("m_Transforms").arraySize;

                lineCount += arraySize;
            }

            return RowHeight * lineCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BeginLine(ref position);

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, Contents.Channels, true);

            if (property.isExpanded)
            {
                var transforms = property.FindPropertyRelative("m_Transforms");
                var channels = property.FindPropertyRelative("m_Channels.m_Values");

                DoChannelGUI(position, transforms, channels);
            }
        }

        void DoChannelGUI(Rect position, SerializedProperty transforms, SerializedProperty channels)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                for (var i = 0; i < transforms.arraySize; ++i)
                {
                    NextLine(ref position);

                    var transform = transforms.GetArrayElementAtIndex(i);
                    var channelProp = channels.GetArrayElementAtIndex(i);
                    var channel = (TransformChannels)channelProp.intValue;

                    var labelPosition = new Rect(position);
                    var offset = EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 10f - 2.5f;

                    labelPosition.x += offset;
                    labelPosition.width -= offset;

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.PropertyField(labelPosition, transform, GUIContent.none);
                    }

                    var buttonArrayPosition = new Rect(position);

                    buttonArrayPosition.xMax = labelPosition.x;

                    var width = buttonArrayPosition.width / 3f;

                    var button1 = new Rect(buttonArrayPosition);
                    button1.width = width;

                    var button2 = new Rect(buttonArrayPosition);
                    button2.x += width;
                    button2.width = width;

                    var button3 = new Rect(buttonArrayPosition);
                    button3.x += width * 2f;
                    button3.width = width;

                    var p = channel.HasFlag(TransformChannels.Position);
                    var r = channel.HasFlag(TransformChannels.Rotation);
                    var s = channel.HasFlag(TransformChannels.Scale);

                    p = GUI.Toggle(button1, p, Contents.Position, EditorStyles.miniButton);
                    r = GUI.Toggle(button2, r, Contents.Rotation, EditorStyles.miniButton);
                    s = GUI.Toggle(button3, s, Contents.Scale, EditorStyles.miniButton);

                    channel = TransformChannels.None;

                    if (p)
                        channel |= TransformChannels.Position;
                    if (r)
                        channel |= TransformChannels.Rotation;
                    if (s)
                        channel |= TransformChannels.Scale;

                    channelProp.intValue = (int)channel;
                }
            }
        }

        void BeginLine(ref Rect position)
        {
            position.height = LineHeight;
        }

        void NextLine(ref Rect position)
        {
            position.y += RowHeight;
        }
    }
}
