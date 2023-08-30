using System;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    static class LiveCaptureGUI
    {
        const float k_IndentMargin = 15.0f;

        static class Contents
        {
            public static readonly GUIContent InfoIcon = EditorGUIUtility.TrIconContent("console.infoicon");
            public static readonly GUIContent WarningIcon = EditorGUIUtility.TrIconContent("console.warnicon");
            public static readonly GUIContent ErrorIcon = EditorGUIUtility.TrIconContent("console.erroricon");
            public static readonly GUIStyle HelpBoxStyleNoIcon;
            public static readonly GUIStyle HelpBoxStyleWithIcon;
            public static readonly GUIStyle HelpBoxLinkStyle;
            public static readonly GUIStyle HelpBox;
            public static readonly GUIContent Fix = EditorGUIUtility.TrTextContent("Fix");

            static Contents()
            {
                HelpBoxStyleWithIcon = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(37, 5, 5, 20)
                };
                HelpBoxLinkStyle = new GUIStyle()
                {
                    font = HelpBoxStyleWithIcon.font,
                    padding = new RectOffset(5, 5, 2, 2),
                    alignment = TextAnchor.LowerRight
                };
                HelpBoxLinkStyle.normal.textColor = new Color(0.34f, 0.68f, 1f, 1f);
                HelpBoxStyleNoIcon = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(5, 5, 5, 20)
                };
                HelpBox = new GUIStyle()
                {
                    imagePosition = ImagePosition.ImageLeft,
                    fontSize = 10,
                    wordWrap = true
                };
                HelpBox.normal.textColor = EditorStyles.helpBox.normal.textColor;
            }
        }

        internal static void HelpBoxWithURL(string message, string linkText, string url, MessageType messageType)
        {
            var icon = GUIContent.none;

            switch (messageType)
            {
                case MessageType.Info: icon = Contents.InfoIcon; break;
                case MessageType.Warning: icon = Contents.WarningIcon; break;
                case MessageType.Error: icon = Contents.ErrorIcon; break;
            }

            var style = Contents.HelpBoxStyleWithIcon;

            if (messageType == MessageType.None)
            {
                style = Contents.HelpBoxStyleNoIcon;
            }

            var messageContents = new GUIContent(message);
            var linkContents = new GUIContent(linkText);
            var buttonSize = Contents.HelpBoxLinkStyle.CalcSize(linkContents);

            EditorGUILayout.BeginHorizontal();

            var rect = GUILayoutUtility.GetRect(messageContents, style);

            EditorGUI.LabelField(rect, messageContents, style);

            if (messageType != MessageType.None)
            {
                var iconWidth = style.padding.left;
                var iconHeight = icon.image.height * iconWidth / icon.image.width;
                var iconY = rect.y + rect.height * 0.5f - iconHeight * 0.5f;
                var iconRect = new Rect(rect.x + 2, iconY, iconWidth, rect.height);

                GUI.Label(iconRect, icon);
            }

            var buttonRect = new Rect(rect.x + rect.width - buttonSize.x, rect.y + rect.height - buttonSize.y, buttonSize.x, buttonSize.y);

            if (GUI.Button(buttonRect, linkContents, Contents.HelpBoxLinkStyle))
            {
                Application.OpenURL(url);
            }

            EditorGUILayout.EndHorizontal();
        }

        internal static void HelpStatusWithURL(string message, string linkText, string url, MessageType messageType)
        {
            var icon = GUIContent.none;

            switch (messageType)
            {
                case MessageType.Info: icon = Contents.InfoIcon; break;
                case MessageType.Warning: icon = Contents.WarningIcon; break;
                case MessageType.Error: icon = Contents.ErrorIcon; break;
            }

            var style = Contents.HelpBoxStyleWithIcon;

            if (messageType == MessageType.None)
            {
                style = Contents.HelpBoxStyleNoIcon;
            }

            var messageContents = new GUIContent(message);
            var linkContents = new GUIContent(linkText);
            var buttonSize = Contents.HelpBoxLinkStyle.CalcSize(linkContents);
            var rect = EditorGUILayout.GetControlRect(false, 20f);
            var linkRect = new Rect(rect) { x = rect.xMax - buttonSize.x, width = buttonSize.x };
            var iconRect = new Rect(rect) { width = 0f };

            if (messageType != MessageType.None)
            {
                iconRect.width = rect.height;
            }

            var messageRect = new Rect(rect) { x = iconRect.xMax, xMax = linkRect.xMin };

            EditorGUI.LabelField(messageRect, messageContents);

            if (messageType != MessageType.None)
            {
                GUI.Label(iconRect, icon);
            }

            if (GUI.Button(linkRect, linkContents, Contents.HelpBoxLinkStyle))
            {
                Application.OpenURL(url);
            }
        }

        public static void DrawFixMeBox(GUIContent message, Action action)
        {
            EditorGUILayout.BeginHorizontal();

            float indent = EditorGUI.indentLevel * k_IndentMargin - EditorStyles.helpBox.margin.left;
            GUILayoutUtility.GetRect(indent, EditorGUIUtility.singleLineHeight, EditorStyles.helpBox, GUILayout.ExpandWidth(false));

            Rect leftRect = GUILayoutUtility.GetRect(Contents.Fix, EditorStyles.miniButton, GUILayout.MinWidth(60));
            Rect rect = GUILayoutUtility.GetRect(message, EditorStyles.helpBox);
            Rect boxRect = new Rect(leftRect.x, rect.y, rect.xMax - leftRect.xMin, rect.height);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (Event.current.type == EventType.Repaint)
                EditorStyles.helpBox.Draw(boxRect, false, false, false, false);

            Rect labelRect = new Rect(boxRect.x + 4, boxRect.y + 3, rect.width - 8, rect.height);
            EditorGUI.LabelField(labelRect, message, Contents.HelpBox);

            var buttonRect = leftRect;
            buttonRect.x += rect.width - 2;
            buttonRect.y = rect.yMin + (rect.height - EditorGUIUtility.singleLineHeight) / 2;
            bool clicked = GUI.Button(buttonRect, Contents.Fix);

            EditorGUI.indentLevel = oldIndent;
            EditorGUILayout.EndHorizontal();

            if (clicked)
                action();
        }
    }
}
