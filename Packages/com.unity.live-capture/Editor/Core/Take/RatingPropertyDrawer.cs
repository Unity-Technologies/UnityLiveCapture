using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A property drawer for int fields to display stars as rating.
    /// </summary>
    [CustomPropertyDrawer(typeof(RatingAttribute))]
    class RatingPropertyDrawer : PropertyDrawer
    {
        internal const float kIconWidth = 18f;
        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";

            public static readonly GUIContent StarFilled = EditorGUIUtility.TrIconContent($"{k_IconPath}/StarFilled@2x.png");
            public static readonly GUIContent StarOutline = EditorGUIUtility.TrIconContent($"{k_IconPath}/StarOutline@2x.png");
            public static readonly GUIContent StarMixed = EditorGUIUtility.TrIconContent($"{k_IconPath}/StarMixed@2x.png");
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var prop = new EditorGUI.PropertyScope(position, label, property))
            {
                var id = GUIUtility.GetControlID(FocusType.Keyboard, position);

                if (label != GUIContent.none)
                {
                    position = EditorGUI.PrefixLabel(position, id, prop.content);
                }

                DoRatingField(position, id, property);
            }
        }

        static void DoRatingField(Rect position, int controlId, SerializedProperty property)
        {
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            var value = property.intValue;

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                if (RectSlider.s_Area.width < 75f)
                {
                    value = DoShortField(position, controlId, value);
                }
                else
                {
                    value = DoLongField(position, controlId, value);
                }

                if (change.changed)
                {
                    property.intValue = value;
                }
            }

            EditorGUI.showMixedValue = false;
        }

        static int DoShortField(Rect position, int controlId, int value)
        {
            value = DoSlider(position, controlId, value, 5, true);
            DrawCompactField(position, value);

            return value;
        }

        static int DoLongField(Rect position, int controlId, int value)
        {
            value = DoSlider(position, controlId, value, 5);
            DrawField(position, value);

            return value;
        }

        static int s_InitialValue;
        static int DoSlider(Rect position, int controlId, int value, int max, bool relative = false, float relativeDistance = 100f)
        {
            if (relative && Event.current.type == EventType.MouseDown)
            {
                s_InitialValue = value;
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var mousePosition = Event.current.mousePosition;
                var slide = RectSlider.Do(controlId, mousePosition, position, Vector3.right, out var distance);

                if (relative && distance == 0)
                {
                    return value;
                }

                if (change.changed)
                {
                    if (relative)
                    {
                        var t = distance / relativeDistance;

                        value = Mathf.Clamp(s_InitialValue + Mathf.CeilToInt(t * max), 0, max);
                    }
                    else
                    {
                        var t = slide.x / (position.width);

                        value = Mathf.Clamp(Mathf.CeilToInt(t * max), 0, max);
                    }
                }
            }

            return value;
        }

        internal static void DrawCompactField(Rect position, int value)
        {
            var iconWidth = Mathf.Min(kIconWidth, position.width);
            var rect1 = new Rect(position) { width = iconWidth };
            var rect2 = new Rect(position) { x = rect1.xMax, xMax = position.xMax };

            if (EditorGUI.showMixedValue)
            {
                DrawIcon(rect1, Contents.StarMixed);
            }
            else
            {
                if (value > 0)
                {
                    DrawIcon(rect1, Contents.StarFilled);
                    EditorGUI.LabelField(rect2, $"x{value}");
                }
                else
                {
                    DrawIcon(rect1, Contents.StarOutline);
                }
            }
        }

        internal static void DrawField(Rect position, int value, bool stretch = true, bool showEmptySlots = true)
        {
            var elementWidth = stretch ? position.width / 5f : kIconWidth;
            var rect1 = new Rect(position) { width = elementWidth };
            var rect2 = new Rect(position) { x = rect1.xMax, width = elementWidth };
            var rect3 = new Rect(position) { x = rect2.xMax, width = elementWidth };
            var rect4 = new Rect(position) { x = rect3.xMax, width = elementWidth };
            var rect5 = new Rect(position) { x = rect4.xMax, width = elementWidth };

            if (EditorGUI.showMixedValue)
            {
                DrawIcon(rect5, Contents.StarMixed);
                DrawIcon(rect4, Contents.StarMixed);
                DrawIcon(rect3, Contents.StarMixed);
                DrawIcon(rect2, Contents.StarMixed);
                DrawIcon(rect1, Contents.StarMixed);
            }
            else
            {
                if (value > 4)
                    DrawIcon(rect5, Contents.StarFilled);
                if (value > 3)
                    DrawIcon(rect4, Contents.StarFilled);
                if (value > 2)
                    DrawIcon(rect3, Contents.StarFilled);
                if (value > 1)
                    DrawIcon(rect2, Contents.StarFilled);
                if (value > 0)
                    DrawIcon(rect1, Contents.StarFilled);

                if (showEmptySlots)
                {
                    if (value < 5)
                        DrawIcon(rect5, Contents.StarOutline);
                    if (value < 4)
                        DrawIcon(rect4, Contents.StarOutline);
                    if (value < 3)
                        DrawIcon(rect3, Contents.StarOutline);
                    if (value < 2)
                        DrawIcon(rect2, Contents.StarOutline);
                }

                if (value < 1)
                    DrawIcon(rect1, Contents.StarOutline);
            }
        }

        static void DrawIcon(Rect rect, GUIContent content)
        {
            var aspect = content.image.width / content.image.height;

            rect.x += (rect.width - aspect * rect.height) * 0.5f;
            EditorGUI.LabelField(rect, content);
        }
    }

    class RectSlider
    {
        internal static Rect s_Area;
        static Vector2 s_StartMousePosition, s_StartPosition, s_ConstraintDirection;

        public static Vector2 Do(int id, Vector2 position, Rect area, Vector2 slideDirection, out float distance)
        {
            distance = 0f;

            var evt = Event.current;
            var eventType = evt.GetTypeForControl(id);
            switch (eventType)
            {
                case EventType.Repaint:
                    s_Area = area;
                    break;
                case EventType.Layout:
                    if (s_Area.Contains(position))
                    {
                        HandleUtility.AddControl(id, 0f);
                    }
                    break;

                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == id && evt.button == 0 && GUIUtility.hotControl == 0 && !evt.alt)
                    {
                        GUIUtility.hotControl = id;
                        GUIUtility.keyboardControl = id;
                        s_StartMousePosition = evt.mousePosition;
                        s_StartPosition = position;
                        s_ConstraintDirection = Handles.matrix.MultiplyVector(slideDirection);

                        position = s_StartPosition - new Vector2(area.x, area.y);

                        GUI.changed = true;
                        evt.Use();
                    }

                    break;

                case EventType.MouseDrag:

                    if (GUIUtility.hotControl == id)
                    {
                        distance = HandleUtility.CalcLineTranslation(s_StartMousePosition, evt.mousePosition, s_StartPosition, slideDirection);
                        position = s_StartPosition - new Vector2(area.x, area.y) + s_ConstraintDirection * distance;

                        GUI.changed = true;
                        evt.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                    }
                    break;
            }
            return position;
        }
    }
}
