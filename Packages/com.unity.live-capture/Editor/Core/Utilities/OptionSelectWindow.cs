using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// An improved popup window that allows searching and scrolling, making it far more convenient when
    /// working with many options.
    /// </summary>
    /// <remarks>
    /// This implementation also avoids blocking the main thread while the window is open.
    /// </remarks>
    class OptionSelectWindow : EditorWindow
    {
        static class Contents
        {
            public static readonly float ItemHeight = 20f;
            public static GUILayoutOption[] ItemOptions =
            {
                GUILayout.MinWidth(0f),
                GUILayout.Height(ItemHeight),
            };
            public static readonly string SearchControlName = $"{typeof(OptionSelectWindow)}SearchBox";
            public static readonly GUIStyle ElementBackground = "RL Element";
        }

        GUIContent[] m_Options;
        Action<int, string> m_OnSelect;

        bool m_CanSearch;
        string m_Search;
        string[] m_SearchableOptions;
        bool[] m_SearchResults;
        bool m_FocusSearchBox;

        Vector2 m_ScrollPos;
        int? m_LastHoveredIndex;

        /// <summary>
        /// Opens a dropdown window used to select a value from a set of options.
        /// </summary>
        /// <remarks>
        /// This completes asynchronously during a different part of the editor loop on a later frame, so take care to ensure
        /// the selected value is still valid on completion and handle any serialized properties with this in mind.
        /// </remarks>
        /// <param name="rect">The rect of the control under which the dropdown window will appear.</param>
        /// <param name="size">The max size of the dropdown window.</param>
        /// <param name="options">The displayed options to pick from.</param>
        /// <param name="onSelect">The callback action executed once an option is selected. The index of the selected
        /// option and the option value are provided.</param>
        /// <param name="canSearch">Add a search box that can be used to filter the options.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="onSelect"/> is null.</exception>
        public static void SelectOption(Rect rect, Vector2 size, GUIContent[] options, Action<int, string> onSelect, bool canSearch = true)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (onSelect == null)
                throw new ArgumentNullException(nameof(onSelect));

            var window = CreateInstance<OptionSelectWindow>();

            window.m_Options = options;
            window.m_OnSelect = onSelect;

            window.m_CanSearch = canSearch;
            window.m_Search = string.Empty;
            window.m_SearchableOptions = options.Select(o => o.text.ToLowerInvariant()).ToArray();
            window.m_SearchResults = null;
            window.m_FocusSearchBox = true;

            window.m_ScrollPos = Vector2.zero;
            window.m_LastHoveredIndex = null;

            // we need to highlight the list item under the mouse
            window.wantsMouseMove = true;
            window.wantsMouseEnterLeaveWindow = true;

            // only make the window shorter when there are not enough elements to use the specified size
            var requiredHeight = (options.Length + 1.0f) * Contents.ItemHeight * 1.05f;
            if (canSearch)
                requiredHeight += EditorGUIUtility.singleLineHeight;

            // open the window under the button
            window.position = GUIUtility.GUIToScreenRect(rect);
            window.ShowAsDropDown(window.position, new Vector2(size.x, Mathf.Min(requiredHeight, size.y)));
        }

        void OnGUI()
        {
            var e = Event.current;
            var selectFirstSearchItem = false;
            var firstSearchItem = -1;

            switch (e.type)
            {
                case EventType.MouseEnterWindow:
                case EventType.MouseLeaveWindow:
                    m_LastHoveredIndex = null;
                    break;
                case EventType.KeyDown:
                {
                    switch (e.keyCode)
                    {
                        case KeyCode.Escape:
                            e.Use();
                            Close();
                            break;
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            e.Use();
                            selectFirstSearchItem = true;
                            break;
                    }
                    break;
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawSearchBox();

                using (var scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPos))
                {
                    var hoveredIndex = default(int?);

                    // draw the options that pass the search filter
                    for (var i = 0; i < m_Options.Length; i++)
                    {
                        if (m_SearchResults == null || m_SearchResults[i])
                        {
                            if (firstSearchItem < 0)
                                firstSearchItem = i;

                            if (DrawItem(i))
                            {
                                hoveredIndex = i;
                            }
                        }
                    }

                    m_ScrollPos = scroll.scrollPosition;

                    // repaint the window if the selection changes
                    if (m_LastHoveredIndex != hoveredIndex && e.type != EventType.Layout)
                    {
                        m_LastHoveredIndex = hoveredIndex;

                        if (e.type != EventType.Repaint)
                            Repaint();
                    }
                }
            }

            // If the user is searching and presses enter we should select the first result, so users don't
            // need to click after searching to select something
            if (selectFirstSearchItem && firstSearchItem >= 0 && !string.IsNullOrWhiteSpace(m_Search))
            {
                Select(firstSearchItem);
            }
        }

        void DrawSearchBox()
        {
            if (!m_CanSearch)
                return;

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                GUI.SetNextControlName(Contents.SearchControlName);
                m_Search = EditorGUILayout.TextField(m_Search, EditorStyles.toolbarSearchField);

                if (change.changed)
                {
                    var search = m_Search.ToLowerInvariant();
                    m_SearchResults = new bool[m_Options.Length];

                    for (var i = 0; i < m_SearchResults.Length; i++)
                    {
                        m_SearchResults[i] = m_SearchableOptions[i].Contains(search);
                    }
                }
            }

            // focus the search box if requested
            if (m_FocusSearchBox)
            {
                m_FocusSearchBox = false;
                GUI.FocusControl(Contents.SearchControlName);
                EditorGUI.FocusTextInControl(Contents.SearchControlName);
            }
        }

        bool DrawItem(int index)
        {
            var option = m_Options[index];

            var rect = EditorGUILayout.GetControlRect(Contents.ItemOptions);
            var e = Event.current;
            var hover = rect.Contains(e.mousePosition);

            switch (e.type)
            {
                case EventType.KeyDown:
                {
                    if (hover)
                    {
                        switch (e.keyCode)
                        {
                            case KeyCode.Return:
                            case KeyCode.KeypadEnter:
                                e.Use();
                                Select(index);
                                break;
                        }
                    }
                    break;
                }
                case EventType.MouseDown:
                {
                    if (hover && e.button == 0)
                    {
                        e.Use();
                        Select(index);
                    }
                    break;
                }
                case EventType.Repaint:
                {
                    Contents.ElementBackground.Draw(rect, false, false, hover, false);
                    break;
                }
            }

            EditorGUI.LabelField(rect, option);
            return hover;
        }

        void Select(int index)
        {
            Close();
            m_OnSelect?.Invoke(index, m_Options[index].text);
        }
    }
}
