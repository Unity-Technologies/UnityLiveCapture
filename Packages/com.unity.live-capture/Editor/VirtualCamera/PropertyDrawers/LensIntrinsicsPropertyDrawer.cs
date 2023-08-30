using System;
using UnityEditor;
using UnityEngine;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(LensIntrinsics))]
    class LensIntrinsicsPropertyDrawer : PropertyDrawer
    {
        static class Contents
        {
            public static GUIContent ApertureShapeLabel = EditorGUIUtility.TrTextContent("Aperture Shape");
            public static GUIContent FocalLengthRange = EditorGUIUtility.TrTextContent("Focal Length Limits", "The minimum and maximum focal length.");
            public static GUIContent CloseFocusDistance = EditorGUIUtility.TrTextContent("Close Focus Distance", "The minimum focus distance.");
            public static GUIContent ApertureRange = EditorGUIUtility.TrTextContent("Aperture Limits", "The minimum and maximum aperture.");
            public static GUIContent BladeCount = EditorGUIUtility.TrTextContent("Blade Count", "Number of diaphragm blades " +
                "the camera uses to form the aperture.");
            public static GUIContent Curvature = EditorGUIUtility.TrTextContent("Curvature", "Maps an aperture range to blade " +
                "curvature. Aperture blades become more visible on bokeh at higher aperture values.");
            public static GUIContent Anamorphism = EditorGUIUtility.TrTextContent("Anamorphism", "Stretch the sensor to " +
                "simulate an anamorphic look. Positive values distort the camera vertically, negative will distort the Camera horizontally.");
            public static GUIContent BarrelClipping = EditorGUIUtility.TrTextContent("Barrel Clipping", "The strength of the " +
                "\"cat eye” effect. You can see this effect on bokeh as a result of lens shadowing (distortion along the edges of the frame).");
            public static GUIContent LensShift = EditorGUIUtility.TrTextContent("Shift", "The horizontal and vertical shift from the center.");
        }

        SerializedProperty m_FocalLengthRangeProp;
        SerializedProperty m_CloseFocusDistanceProp;
        SerializedProperty m_ApertureRangeProp;
        SerializedProperty m_LensShiftProp;
        SerializedProperty m_BladeCountProp;
        SerializedProperty m_CurvatureProp;
        SerializedProperty m_BarrelClippingProp;
        SerializedProperty m_AnamorphismProp;
        bool m_ApertureShapeExpanded;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var rows = 1;
            var rowHeight = GetLineHeight() + 2f;

            if (property.isExpanded)
            {
                rows += 5;

                if (m_ApertureShapeExpanded)
                {
                    rows += 4;
                }
            }

            return rows * rowHeight;
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
            ++EditorGUI.indentLevel;

            m_FocalLengthRangeProp = property.FindPropertyRelative("m_FocalLengthRange");
            m_CloseFocusDistanceProp = property.FindPropertyRelative("m_CloseFocusDistance");
            m_ApertureRangeProp = property.FindPropertyRelative("m_ApertureRange");
            m_LensShiftProp = property.FindPropertyRelative("m_LensShift");
            m_BladeCountProp = property.FindPropertyRelative("m_BladeCount");
            m_CurvatureProp = property.FindPropertyRelative("m_Curvature");
            m_BarrelClippingProp = property.FindPropertyRelative("m_BarrelClipping");
            m_AnamorphismProp = property.FindPropertyRelative("m_Anamorphism");

            EditorGUIUtility.wideMode = true;

            position = BeginLine(position);
            position = NextLine(position);

            DoFocalLengthGUI(ref position, property);

            DoFocusDistanceGUI(ref position, property);

            DoApertureGUI(ref position, property);

            position = NextLine(position);

            DoLensShiftGUI(ref position, property);

            position = NextLine(position);

            DoApertureShapeGUI(ref position, property);

            --EditorGUI.indentLevel;
        }

        void DoFocalLengthGUI(ref Rect position, SerializedProperty property)
        {
            var limits = LensLimits.FocalLength;

            DoRangeGUI(position, Contents.FocalLengthRange, m_FocalLengthRangeProp, limits.x, limits.y, true);
        }

        void DoFocusDistanceGUI(ref Rect position, SerializedProperty property)
        {
            position = NextLine(position);

            EditorGUI.PropertyField(position, m_CloseFocusDistanceProp, Contents.CloseFocusDistance);

            // Since we don't use a slider a validation step is needed.
            m_CloseFocusDistanceProp.floatValue = Mathf.Clamp(
                m_CloseFocusDistanceProp.floatValue,
                LensLimits.FocusDistance.x,
                LensLimits.FocusDistance.y - LensLimits.MinFocusDistanceRange);
        }

        void DoApertureGUI(ref Rect position, SerializedProperty property)
        {
            var limits = LensLimits.Aperture;

            position = NextLine(position);

            DoRangeGUI(position, Contents.ApertureRange, m_ApertureRangeProp, limits.x, limits.y, true);
        }

        void DoLensShiftGUI(ref Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(position, m_LensShiftProp, Contents.LensShift);
        }

        void DoApertureShapeGUI(ref Rect position, SerializedProperty property)
        {
            var bladeCountLimits = LensLimits.BladeCount;
            var curvatureLimits = LensLimits.Curvature;
            var barrelClippingLimits = LensLimits.BarrelClipping;
            var anamorphismLimits = LensLimits.Anamorphism;
            m_BladeCountProp.isExpanded = EditorGUI.Foldout(
                position, m_BladeCountProp.isExpanded, Contents.ApertureShapeLabel, true);

            m_ApertureShapeExpanded = m_BladeCountProp.isExpanded;

            if (m_ApertureShapeExpanded)
            {
                position = NextLine(position);

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUI.IntSlider(
                        position,
                        m_BladeCountProp,
                        bladeCountLimits.x,
                        bladeCountLimits.y,
                        Contents.BladeCount);

                    position = NextLine(position);

                    DoRangeGUI(position, Contents.Curvature, m_CurvatureProp, curvatureLimits.x, curvatureLimits.y, false);

                    position = NextLine(position);

                    EditorGUI.Slider(
                        position,
                        m_BarrelClippingProp,
                        barrelClippingLimits.x,
                        barrelClippingLimits.y,
                        Contents.BarrelClipping);

                    position = NextLine(position);

                    EditorGUI.Slider(
                        position,
                        m_AnamorphismProp,
                        anamorphismLimits.x,
                        anamorphismLimits.y,
                        Contents.Anamorphism);
                }
            }
        }

        void DoRangeGUI(Rect position, GUIContent label, SerializedProperty property, float min, float max, bool indent = true)
        {
            using (new ShowMixedValuesScope(property))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var value = DoRangeGUI(position, label, property.vector2Value, min, max, false);

                if (change.changed)
                {
                    property.vector2Value = value;
                }
            }
        }

        Vector2 DoRangeGUI(Rect position, GUIContent label, Vector2 value, float min, float max, bool indent = true)
        {
            if (indent)
                EditorGUI.indentLevel++;

            // From HDCameraUI.Drawers.cs
            var v = value;
            // The layout system breaks alignment when mixing inspector fields with custom layout'd
            // fields as soon as a scrollbar is needed in the inspector, so we'll do the layout
            // manually instead
            const int kFloatFieldWidth = 50;
            const int kSeparatorWidth = 5;
            var indentOffset = EditorGUI.indentLevel * 15f;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - indentOffset, position.height);
            var floatFieldLeft = new Rect(labelRect.xMax, position.y, kFloatFieldWidth + indentOffset, position.height);
            var sliderRect = new Rect(floatFieldLeft.xMax + kSeparatorWidth - indentOffset, position.y,
                position.width - labelRect.width - kFloatFieldWidth * 2 - kSeparatorWidth * 2, position.height);
            var floatFieldRight = new Rect(sliderRect.xMax + kSeparatorWidth - indentOffset, position.y,
                kFloatFieldWidth + indentOffset, position.height);

            EditorGUI.PrefixLabel(labelRect, label);
            v.x = EditorGUI.FloatField(floatFieldLeft, v.x);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.MinMaxSlider(sliderRect, ref v.x, ref v.y, min, max);

                if (change.changed)
                {
                    v.x = (float)Math.Round((float)v.x, 2);
                    v.y = (float)Math.Round((float)v.y, 2);
                }
            }

            v.y = EditorGUI.FloatField(floatFieldRight, v.y);

            if (indent)
                EditorGUI.indentLevel--;

            return v;
        }

        float GetLineHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        Rect BeginLine(Rect position)
        {
            return new Rect(
                position.x,
                position.y,
                position.width,
                GetLineHeight());
        }

        Rect NextLine(Rect position)
        {
            return new Rect(
                position.x,
                position.y + position.height + 2f,
                position.width,
                GetLineHeight());
        }
    }
}
