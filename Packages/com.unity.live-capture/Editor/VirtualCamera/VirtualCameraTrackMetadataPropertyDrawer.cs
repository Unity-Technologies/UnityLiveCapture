using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(VirtualCameraTrackMetadata))]
    class VirtualCameraTrackMetadataPropertyDrawer : PropertyDrawer
    {
        static class Contents
        {
            public static readonly GUIContent CropAspect = EditorGUIUtility.TrTextContent("Crop Aspect", "The aspect ratio of the crop mask.");
            public static string[] Channels =
            {
                EditorGUIUtility.TrTextContent("Position").text,
                EditorGUIUtility.TrTextContent("Rotation").text,
                EditorGUIUtility.TrTextContent("Focal Length").text,
                EditorGUIUtility.TrTextContent("Focus Distance").text,
                EditorGUIUtility.TrTextContent("Aperture").text,
            };
            public static GUIStyle TextAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
            };
        }

        List<string> m_Channels = new List<string>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var rowHeight = EditorGUIUtility.singleLineHeight + 2f;
            var height = rowHeight;

            if (property.isExpanded)
            {
                var channelsProp = property.FindPropertyRelative("m_Channels");
                var channels = (VirtualCameraChannelFlags)channelsProp.enumValueIndex;
                var rows = 8;

                if (channels.HasFlag(VirtualCameraChannelFlags.FocalLength))
                {
                    ++rows;
                }

                if (channels.HasFlag(VirtualCameraChannelFlags.FocusDistance))
                {
                    ++rows;
                }

                if (channels.HasFlag(VirtualCameraChannelFlags.Aperture))
                {
                    ++rows;
                }

                height = rows * rowHeight;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_CropAspect"));
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                DoGUI(position, property);
            }
        }

        void DoGUI(Rect position, SerializedProperty property)
        {
            using var _ = new EditorGUI.IndentLevelScope();

            var lensProp = property.FindPropertyRelative("m_Lens");
            var channelsProp = property.FindPropertyRelative("m_Channels");
            var cameraBodyProp = property.FindPropertyRelative("m_CameraBody");
            var channels = (VirtualCameraChannelFlags)channelsProp.enumValueIndex;
            var sensorSize = cameraBodyProp.FindPropertyRelative("m_SensorSize").vector2Value;
            var isoProp = cameraBodyProp.FindPropertyRelative("m_Iso");
            var shutterSpeedProp = cameraBodyProp.FindPropertyRelative("m_ShutterSpeed");
            var lensAsset = property.FindPropertyRelative("m_LensAsset");
            var cropMask = property.FindPropertyRelative("m_CropAspect");
            var sensorSizes = SensorPresetsCache.GetSensorSizes();
            var options = SensorPresetsCache.GetSensorNameContents();
            var index = Array.FindIndex(sensorSizes, (s) => s == sensorSize);

            m_Channels.Clear();

            for (var i = 0; i < 5; ++i)
            {
                if (channels.HasFlag((VirtualCameraChannelFlags)(1 << i)))
                {
                    m_Channels.Add(Contents.Channels[i]);
                }
            }

            var channelsStr = m_Channels.Count == 0 ? "None" : string.Join(", ", m_Channels);

            position.y += position.height + 2f;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, new GUIContent("Channels"), GUIContent.none);

            position.y += position.height + 2f;
            position.height = EditorGUIUtility.singleLineHeight * 2f;
            EditorGUI.TextArea(position, channelsStr, Contents.TextAreaStyle);

            if (channels.HasFlag(VirtualCameraChannelFlags.FocalLength))
            {
                var prop = lensProp.FindPropertyRelative("m_FocalLength");

                position.y += position.height + 2f;
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, prop, LensDrawerUtility.Contents.FocalLength);
            }

            if (channels.HasFlag(VirtualCameraChannelFlags.FocusDistance))
            {
                var prop = lensProp.FindPropertyRelative("m_FocusDistance");

                position.y += position.height + 2f;
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, prop, LensDrawerUtility.Contents.FocusDistance);
            }

            if (channels.HasFlag(VirtualCameraChannelFlags.Aperture))
            {
                var prop = lensProp.FindPropertyRelative("m_Aperture");

                position.y += position.height + 2f;
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, prop, LensDrawerUtility.Contents.Aperture);
            }

            position.y += position.height + 2f;
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, lensAsset, VirtualCameraDeviceEditor.Contents.LensAssetLabel);

            position.y += position.height + 2f;
            position.height = EditorGUIUtility.singleLineHeight;

            if (index == -1)
            {
                EditorGUI.Vector2Field(position, SensorSizePropertyDrawer.Contents.SensorSize, sensorSize);
            }
            else
            {
                EditorGUI.LabelField(position, SensorSizePropertyDrawer.Contents.SensorSize, new GUIContent(options[index]));
            }

            position.y += position.height + 2f;
            EditorGUI.PropertyField(position, isoProp, CameraBodyPropertyDrawer.Contents.Iso);

            position.y += position.height + 2f;
            EditorGUI.PropertyField(position, shutterSpeedProp, CameraBodyPropertyDrawer.Contents.ShutterSpeed);

            position.y += position.height + 2f;
            EditorGUI.PropertyField(position, cropMask, Contents.CropAspect);
        }
    }
}
