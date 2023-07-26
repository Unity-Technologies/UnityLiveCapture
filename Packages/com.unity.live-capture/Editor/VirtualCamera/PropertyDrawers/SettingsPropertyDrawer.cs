using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(Settings))]
    class SettingsPropertyDrawer : PropertyDrawer
    {
        SerializedProperty m_Damping;
        SerializedProperty m_PositionLock;
        SerializedProperty m_RotationLock;
        SerializedProperty m_AutoHorizon;
        SerializedProperty m_ErgonomicTilt;
        SerializedProperty m_MotionScale;
        SerializedProperty m_JoystickSensitivity;
        SerializedProperty m_PedestalSpace;
        SerializedProperty m_MotionSpace;
        SerializedProperty m_AspectRatio;
        SerializedProperty m_FocusMode;
        SerializedProperty m_ReticlePosition;
        SerializedProperty m_FocusDistanceOffset;
        SerializedProperty m_FocusDistanceDamping;
        SerializedProperty m_FocalLengthDamping;
        SerializedProperty m_ApertureDamping;
        SerializedProperty m_GateFit;
        SerializedProperty m_GateMask;
        SerializedProperty m_AspectRatioLines;
        SerializedProperty m_CenterMarker;
        SerializedProperty m_FocusPlane;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BeginLine(ref position);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                NextLine(ref position);
                DoGUI(position, property);
            }
        }

        void DoGUI(Rect position, SerializedProperty property)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                m_Damping = property.FindPropertyRelative("Damping");
                m_PositionLock = property.FindPropertyRelative("PositionLock");
                m_RotationLock = property.FindPropertyRelative("RotationLock");
                m_AutoHorizon = property.FindPropertyRelative("AutoHorizon");
                m_ErgonomicTilt = property.FindPropertyRelative("ErgonomicTilt");
                m_MotionScale = property.FindPropertyRelative("MotionScale");
                m_JoystickSensitivity = property.FindPropertyRelative("JoystickSensitivity");
                m_PedestalSpace = property.FindPropertyRelative("PedestalSpace");
                m_MotionSpace = property.FindPropertyRelative("MotionSpace");
                m_AspectRatio = property.FindPropertyRelative("AspectRatio");
                m_FocusMode = property.FindPropertyRelative("FocusMode");
                m_ReticlePosition = property.FindPropertyRelative("ReticlePosition");
                m_FocusDistanceOffset = property.FindPropertyRelative("FocusDistanceOffset");
                m_FocusDistanceDamping = property.FindPropertyRelative("FocusDistanceDamping");
                m_FocalLengthDamping = property.FindPropertyRelative("FocalLengthDamping");
                m_ApertureDamping = property.FindPropertyRelative("ApertureDamping");
                m_GateFit = property.FindPropertyRelative("GateFit");
                m_GateMask = property.FindPropertyRelative("GateMask");
                m_AspectRatioLines = property.FindPropertyRelative("AspectRatioLines");
                m_CenterMarker = property.FindPropertyRelative("CenterMarker");
                m_FocusPlane = property.FindPropertyRelative("FocusPlane");

                PropertyField(ref position, m_Damping, true);
                PropertyField(ref position, m_PositionLock, true);
                PropertyField(ref position, m_RotationLock, true);

                var rollEnabled = ((RotationAxis)m_RotationLock.intValue).HasFlag(RotationAxis.Roll);
                using (new EditorGUI.DisabledGroupScope(!rollEnabled))
                {
                    PropertyField(ref position, m_AutoHorizon);
                }

                PropertyField(ref position, m_ErgonomicTilt);
                PropertyField(ref position, m_MotionScale, true);
                PropertyField(ref position, m_JoystickSensitivity, true);
                PropertyField(ref position, m_PedestalSpace, true);
                PropertyField(ref position, m_MotionSpace, true);
                PropertyField(ref position, m_AspectRatio, true);
                PropertyField(ref position, m_FocusMode, true);
                PropertyField(ref position, m_ReticlePosition);
                PropertyField(ref position, m_FocusDistanceOffset);
                PropertyField(ref position, m_FocusDistanceDamping);
                PropertyField(ref position, m_FocalLengthDamping);
                PropertyField(ref position, m_ApertureDamping);
                PropertyField(ref position, m_GateFit, true);
                PropertyField(ref position, m_GateMask);
                PropertyField(ref position, m_AspectRatioLines);
                PropertyField(ref position, m_CenterMarker);
                PropertyField(ref position, m_FocusPlane);
            }
        }

        void PropertyField(ref Rect position, SerializedProperty property, bool getHeight = false)
        {
            if (getHeight)
            {
                position.height = EditorGUI.GetPropertyHeight(property);
            }
            else
            {
                position.height = GetLineHeight();
            }

            EditorGUI.PropertyField(position, property);
            NextLine(ref position);
        }

        float GetLineHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        void BeginLine(ref Rect position)
        {
            position.height = GetLineHeight();
        }

        void NextLine(ref Rect position)
        {
            position.y += position.height + 2f;
        }
    }
}
