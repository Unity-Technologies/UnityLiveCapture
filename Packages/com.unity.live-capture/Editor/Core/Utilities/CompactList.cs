using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A list implementation optimized for large numbers of complex elements.
    /// </summary>
    /// <remarks>
    /// <see cref="ReorderableList"/> can take up lots of space when many elements are stored in the list,
    /// and does not scale in performance for complex element types. By using a scroll view for the
    /// list and only allowing editing of a single element at a time, the list is more compact and has more
    /// predictable performance. It also supports search functionality.
    /// </remarks>
    class CompactList
    {
        static readonly MethodInfo s_GetPermanentControlID = typeof(GUIUtility)
            .GetMethod("GetPermanentControlID", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo s_HasCurrentWindowKeyFocus = typeof(EditorGUIUtility)
            .GetMethod("HasCurrentWindowKeyFocus", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo s_EndEditingActiveTextField = typeof(EditorGUI)
            .GetMethod("EndEditingActiveTextField", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo s_ToolbarSearchField = typeof(EditorGUI)
            .GetMethod("ToolbarSearchField", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Rect), typeof(string), typeof(bool) }, null);

        static readonly float k_MinItemHeight = 2f;
        static readonly float k_MinListHeight = 5f * EditorGUIUtility.singleLineHeight;

        class Defaults
        {
            public const float BoxPadding = 1f;
            public readonly GUIStyle BoxHeaderBackground = "RL Empty Header";
            public readonly GUIStyle BoxBackground = "RL Background";

            public const float ItemPadding = 6f;
            public readonly GUIStyle ItemBackground = "RL Element";

            public const float ResizeHandleHeight = 12f;
            public readonly GUIStyle ResizeHandle = "RL DragHandle";

            public const float SearchBarHeight = 18f;
            public const float SearchBottomPadding = 2f;
            public const float SearchSidePadding = 6f;
            public readonly GUIContent ShowSearchbar = EditorGUIUtility.TrTextContent("Show Searchbar");

            public const float ControlsSpacing = 2f;
            public const float ButtonHeight = 15f;
            public const float ButtonWidth = 20f;
            public const float ButtonSpacing = 8f;
            public readonly GUIContent IconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to list");
            public readonly GUIContent IconToolbarPlusMore = EditorGUIUtility.TrIconContent("Toolbar Plus More", "Choose to add to list");
            public readonly GUIContent IconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection or last element from list");
            public readonly GUIContent MenuButton = EditorGUIUtility.TrIconContent("_Menu", "List menu");
            public readonly GUIContent MoveUpIcon = EditorGUIUtility.TrTextContent("\u25B2", "Move selection up");
            public readonly GUIContent MoveDownIcon = EditorGUIUtility.TrTextContent("\u25BC", "Move selection down");
            public readonly GUIContent NoneListItemLabel = EditorGUIUtility.TrTextContent("List is empty");
            public readonly GUIStyle ControlButton = new GUIStyle("RL FooterButton")
            {
                fontSize = 10,
            };

            static GUIContent s_TempContent = new GUIContent();

            static GUIContent TempContent(string t)
            {
                s_TempContent.image = null;
                s_TempContent.text = t;
                s_TempContent.tooltip = null;
                return s_TempContent;
            }

            public void DoAddButton(CompactList list)
            {
                if (list.Property != null)
                {
                    list.Property.arraySize += 1;
                    list.Index = list.Property.arraySize - 1;
                }
                else
                {
                    var elementType = list.List.GetType().GetElementType();

                    if (elementType == typeof(string))
                    {
                        list.Index = list.List.Add(string.Empty);
                    }
                    else if (elementType != null && elementType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        Debug.LogError($"Cannot add element. Type {elementType} has no default constructor. Implement a default constructor or implement your own add behaviour.");
                    }
                    else if (list.List.GetType().GetGenericArguments()[0] != null)
                    {
                        list.Index = list.List.Add(Activator.CreateInstance(list.List.GetType().GetGenericArguments()[0]));
                    }
                    else if (elementType != null)
                    {
                        list.Index = list.List.Add(Activator.CreateInstance(elementType));
                    }
                    else
                    {
                        Debug.LogError("Cannot add element of type Null.");
                    }
                }
            }

            public void DoRemoveButton(CompactList list)
            {
                if (list.Property != null)
                {
                    list.Property.DeleteArrayElementAtIndex(list.Index);

                    if (list.Index >= list.Property.arraySize - 1)
                        list.Index = list.Property.arraySize - 1;
                }
                else
                {
                    list.List.RemoveAt(list.Index);

                    if (list.Index >= list.List.Count - 1)
                        list.Index = list.List.Count - 1;
                }
            }

            public bool DoSearchFilter(CompactList list, int index, string searchFilter)
            {
                var filter = searchFilter.ToLowerInvariant();
                var value = GetItemName(list, index).ToLowerInvariant();
                return value.Contains(filter);
            }

            public void DrawListBackground(Rect rect)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var topRect = new Rect(rect)
                    {
                        yMax = rect.y + 5f,
                    };
                    var bottomRect = new Rect(rect)
                    {
                        yMin = rect.y + 5f,
                    };

                    BoxHeaderBackground.Draw(topRect, false, false, false, false);
                    BoxBackground.Draw(bottomRect, false, false, false, false);
                }
            }

            public void DrawListItemBackground(Rect rect, int index, bool selected, bool focused)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    ItemBackground.Draw(rect, false, selected, selected, focused);
                }
            }

            public void DrawListItem(CompactList list, Rect rect, int index, bool selected, bool focused)
            {
                EditorGUI.LabelField(rect, TempContent(GetItemName(list, index)));
            }

            public void DrawNoneListItem(CompactList list, Rect rect)
            {
                EditorGUI.LabelField(rect, Default.NoneListItemLabel);
            }

            string GetItemName(CompactList list, int index)
            {
                if (list.Property != null)
                {
                    var element = list.Property.GetArrayElementAtIndex(index);
                    return element.displayName;
                }
                else
                {
                    var element = list.List[index];
                    return element.ToString();
                }
            }
        }

        static Defaults Default { get; set; }

        readonly int m_ID;
        readonly string m_StatePath;

        int m_Index;
        int m_ScrollToElement = -1;
        float m_ScrollPosition;
        bool m_Resizing;
        float m_ResizeStartOffset;
        float m_ResizeStartHeight;
        bool m_ShowSearchBar = true;
        string m_SearchFilter = string.Empty;
        float m_ItemHeight = EditorGUIUtility.singleLineHeight;
        float m_ListHeight = 5f * EditorGUIUtility.singleLineHeight;
        readonly Bijection<int, int> m_SearchElementToItem = new Bijection<int, int>();

        /// <summary>
        /// The serialized property being edited by instance.
        /// </summary>
        /// <value>
        /// If null, <see cref="List"/> is being used instead.
        /// </value>
        public SerializedProperty Property { get; }

        /// <summary>
        /// The list being edited by this instance.
        /// </summary>
        /// <value>
        /// If null, <see cref="Property"/> is being used instead.
        /// </value>
        public IList List { get; }

        /// <summary>
        /// The number of elements in the list.
        /// </summary>
        /// <remarks>
        /// If <see cref="Property"/> is non-null and multiple target objects are being edited,
        /// this will be the minimum array size out of all the target objects.
        /// </remarks>
        public int Count
        {
            get
            {
                if (Property != null)
                {
                    if (!Property.hasMultipleDifferentValues)
                        return Property.arraySize;

                    var minArraySize = Property.arraySize;
                    foreach (var targetObject in Property.serializedObject.targetObjects)
                    {
                        using (var serializedObject = new SerializedObject(targetObject))
                        {
                            var property = serializedObject.FindProperty(this.Property.propertyPath);
                            minArraySize = Math.Min(property.arraySize, minArraySize);
                        }
                    }
                    return minArraySize;
                }
                return List.Count;
            }
        }

        /// <summary>
        /// The index of the selected element in the list.
        /// </summary>
        public int Index
        {
            get => m_Index;
            set
            {
                if (m_Index != value)
                {
                    m_Index = value;
                    SessionState.SetInt($"{m_StatePath}/index", m_Index);
                    ScrollToItem(m_Index);
                }
            }
        }

        /// <summary>
        /// Can the list size be changed by dragging the footer.
        /// </summary>
        public bool Resizable { get; set; } = true;

        /// <summary>
        /// Can the items in the list be reordered.
        /// </summary>
        public bool Reorderable { get; set; } = true;

        /// <summary>
        /// Can the list items be searched.
        /// </summary>
        public bool Searchable { get; set; } = true;

        /// <summary>
        /// Expand the search bar.
        /// </summary>
        public bool ShowSearchBar
        {
            get => m_ShowSearchBar;
            set
            {
                if (m_ShowSearchBar != value)
                {
                    m_ShowSearchBar = value;
                    SessionState.SetBool($"{m_StatePath}/showSearchBar", m_ShowSearchBar);
                }
            }
        }

        /// <summary>
        /// The search string used to filter the list items.
        /// </summary>
        public string SearchFilter
        {
            get => m_SearchFilter;
            set
            {
                if (m_SearchFilter != value)
                {
                    m_SearchFilter = value ?? string.Empty;
                    SessionState.SetString($"{m_StatePath}/searchFilter", m_SearchFilter);
                }
            }
        }

        /// <summary>
        /// Is there a search filter currently being applied to the list.
        /// </summary>
        public bool IsSearchActive => Searchable && !string.IsNullOrEmpty(SearchFilter);

        /// <summary>
        /// The height of items in the list in pixels.
        /// </summary>
        public float ItemHeight
        {
            get => m_ItemHeight;
            set
            {
                if (m_ItemHeight != value)
                {
                    m_ItemHeight = Mathf.Max(k_MinItemHeight, value);
                    ScrollToItem(Index);
                }
            }
        }

        /// <summary>
        /// The height of the list item area in pixels.
        /// </summary>
        public float ListHeight
        {
            get => m_ListHeight;
            set
            {
                if (m_ListHeight != value)
                {
                    m_ListHeight = Mathf.Max(k_MinListHeight, value);
                    SessionState.SetFloat($"{m_StatePath}/listHeight", m_ListHeight);
                    ScrollToItem(Index);
                }
            }
        }

        /// <summary>
        /// A callback used to determine if the user is able to add items to the list.
        /// </summary>
        /// <remarks>
        /// <para>Return true if elements can be added to the list; false otherwise.</para>
        /// </remarks>
        public Func<bool> OnCanAddCallback;

        /// <summary>
        /// A callback used to determine if the user is able to remove items from the list.
        /// </summary>
        /// <remarks>
        /// <para>Return true if elements can be removed from the list; false otherwise.</para>
        /// </remarks>
        public Func<bool> OnCanRemoveCallback;

        /// <summary>
        /// A callback used to override how items are added to the list.
        /// </summary>
        public Action OnAddCallback;

        /// <summary>
        /// A callback used to display advanced options when the add button is clicked.
        /// </summary>
        /// <remarks>
        /// <para>The first parameter contains the add button position.</para>
        /// </remarks>
        public Action<Rect> OnAddDropdownCallback;

        /// <summary>
        /// A callback used to override how items are removed from the list.
        /// </summary>
        public Action OnRemoveCallback;

        /// <summary>
        /// A callback invoked after the user has clicked on the add or remove buttons.
        /// </summary>
        public Action OnChangedCallback;

        /// <summary>
        /// A callback used to add a custom menu to the list controls.
        /// </summary>
        /// <remarks>
        /// <para>The first parameter contains the menu to add items to.</para>
        /// </remarks>
        public Action<GenericMenu> OnMenuCallback;

        /// <summary>
        /// A callback invoked when the user moves an item to a new index in the list.
        /// </summary>
        /// <remarks>
        /// <para>The first parameter contains the previous index of the moved item.</para>
        /// <para>The second parameter contains the new index of the moved item.</para>
        /// </remarks>
        public Action<int, int> OnReorderCallback;

        /// <summary>
        /// A callback invoked when the user selects an item in the list.
        /// </summary>
        public Action OnSelectCallback;

        /// <summary>
        /// A callback invoked when the user releases a click on the selected item in the list.
        /// </summary>
        public Action OnMouseUpCallback;

        /// <summary>
        /// A callback invoked when the user changes the list size.
        /// </summary>
        public Action OnResizeCallback;

        /// <summary>
        /// A callback that overrides the search filter behaviour.
        /// </summary>
        /// <remarks>
        /// <para>The first parameter contains the index of the item to filter.</para>
        /// <para>The second parameter contains the search string.</para>
        /// <para>Return true to show the item in the filtered list, false to reject it.</para>
        /// </remarks>
        public Func<int, string, bool> SearchFilterCallback;

        /// <summary>
        /// A callback that overrides the height of the GUI for the selected element.
        /// </summary>
        /// <remarks>
        /// <para>The first parameter contains the index of the element to get the height of.</para>
        /// <para>Return the height of the element in pixels.</para>
        /// </remarks>
        public Func<int, float> ElementHeightCallback;

        /// <summary>
        /// A callback that overrides the background for items in the list box.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is only called for items that are currently visible in the list box.
        /// </para>
        /// <para>The first parameter contains the position to draw the item in.</para>
        /// <para>The second parameter contains the index of the item being drawn.</para>
        /// <para>The third parameter indicates if this item is currently selected.</para>
        /// <para>The fourth parameter indicates if this item is currently selected and has keyboard focus.</para>
        /// </remarks>
        public Action<Rect, int, bool, bool> DrawListItemBackgroundCallback;

        /// <summary>
        /// A callback that overrides the GUI for items in the list box.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is only called for items that are currently visible in the list box.
        /// </para>
        /// <para>The first parameter contains the position to draw the item in.</para>
        /// <para>The second parameter contains the index of the item being drawn.</para>
        /// <para>The third parameter indicates if this item is currently selected.</para>
        /// <para>The fourth parameter indicates if this item is currently selected and has keyboard focus.</para>
        /// </remarks>
        public Action<Rect, int, bool, bool> DrawListItemCallback;

        /// <summary>
        /// A callback that overrides the GUI to display when the list is empty.
        /// </summary>
        /// <remarks>
        /// Does not fire when the list is displaying search results.
        /// <para>The parameter contains the position to draw the element GUI in.</para>
        /// </remarks>
        public Action<Rect> DrawNoneListItemCallback;

        /// <summary>
        /// A callback that overrides the GUI for the selected element that is drawn beneath the list.
        /// </summary>
        /// <remarks>
        /// <para>The first parameter contains the position to draw the element GUI in.</para>
        /// <para>The second parameter contains the index of the element to draw the GUI for.</para>
        /// </remarks>
        public Action<Rect, int> DrawElementCallback;

        /// <summary>
        /// Creates a new <see cref="CompactList"/> instance.
        /// </summary>
        /// <param name="elements">The array property to edit.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="elements"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="elements"/> is not an array property.</exception>
        public CompactList(SerializedProperty elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            if (!elements.isArray)
                throw new ArgumentException($"Must be an array {nameof(SerializedProperty)}!", nameof(elements));

            m_ID = (int)s_GetPermanentControlID.Invoke(null, null);
            Property = elements;

            m_StatePath = $"{elements.serializedObject.targetObject.GetInstanceID()}/{elements.propertyPath}";
            LoadState();
        }

        /// <summary>
        /// Creates a new <see cref="CompactList"/> instance.
        /// </summary>
        /// <param name="elements">The list to edit.</param>
        /// <param name="statePath">The unique path used to store the list state in <see cref="SessionState"/>.
        /// It must not be null or empty and should not conflict with other lists.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="elements"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="statePath"/> is null or empty.</exception>
        public CompactList(IList elements, string statePath)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            if (string.IsNullOrWhiteSpace(statePath))
                throw new ArgumentException("Path \"{statePath}\" is not valid!", nameof(statePath));

            m_ID = (int)s_GetPermanentControlID.Invoke(null, null);
            List = elements;

            m_StatePath = statePath;
            LoadState();
        }

        void LoadState()
        {
            Index = SessionState.GetInt($"{m_StatePath}/index", Index);
            ShowSearchBar = SessionState.GetBool($"{m_StatePath}/showSearchBar", ShowSearchBar);
            SearchFilter = SessionState.GetString($"{m_StatePath}/searchFilter", SearchFilter);
            m_ScrollPosition = SessionState.GetFloat($"{m_StatePath}/scrollPosition", m_ScrollPosition);
            ListHeight = SessionState.GetFloat($"{m_StatePath}/listHeight", ListHeight);
        }

        /// <summary>
        /// Gets the height of the list.
        /// </summary>
        /// <returns>The height in pixels.</returns>
        public float GetHeight()
        {
            return GetListBoxHeight() + EditorGUIUtility.standardVerticalSpacing + GetElementHeight();
        }

        float GetListBoxHeight()
        {
            return GetListHeaderHeight() + GetListItemsHeight() + GetListFooterHeight();
        }

        float GetListHeaderHeight()
        {
            var height = 0f;

            if (Searchable && ShowSearchBar)
                height += Defaults.SearchBarHeight + (2f * Defaults.SearchBottomPadding);

            return height;
        }

        float GetListItemsHeight()
        {
            return ListHeight + (2f * Defaults.BoxPadding);
        }

        float GetListFooterHeight()
        {
            var height = 0f;

            if (Resizable)
                height += Defaults.ResizeHandleHeight;

            return height;
        }

        float GetElementHeight()
        {
            if (Index < 0 || Index >= Count)
                return 0f;

            if (ElementHeightCallback == null)
            {
                if (Property != null)
                {
                    var element = Property.GetArrayElementAtIndex(Index);
                    return EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(element);
                }
                return 0f;
            }
            return ElementHeightCallback(Index);
        }

        /// <summary>
        /// Draws the list using the layout system.
        /// </summary>
        public void DoGUILayout()
        {
            var listRect = GUILayoutUtility.GetRect(0, GetListBoxHeight(), GUILayout.ExpandWidth(true));

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            var elementRect = GUILayoutUtility.GetRect(0, GetElementHeight(), GUILayout.ExpandWidth(true));

            DoListInternal(listRect, elementRect);
        }

        /// <summary>
        /// Draws the list.
        /// </summary>
        /// <param name="rect">The rect to draw the list in.</param>
        public void DoGUI(Rect rect)
        {
            var listRect = new Rect()
            {
                x = rect.x,
                y = rect.y,
                width = rect.width,
                height = GetListBoxHeight(),
            };
            var elementRect = new Rect()
            {
                x = rect.x,
                y = listRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                width = rect.width,
                height = GetElementHeight(),
            };

            DoListInternal(listRect, elementRect);
        }

        void DoListInternal(Rect listRect, Rect elementRect)
        {
            Profiler.BeginSample($"{nameof(CompactList)}.{nameof(DoListInternal)}()");

            if (Default == null)
                Default = new Defaults();

            DoList(listRect);
            DoElement(elementRect);

            Profiler.EndSample();
        }

        void DoList(Rect rect)
        {
            var controlsRect = new Rect(rect)
            {
                xMin = rect.xMax - Defaults.ButtonWidth,
            };

            DoListControls(controlsRect);

            var listRect = new Rect(rect)
            {
                xMax = controlsRect.xMin - Defaults.ControlsSpacing,
            };
            var headerRect = new Rect(listRect)
            {
                height = GetListHeaderHeight(),
            };
            var itemsRect = new Rect(listRect)
            {
                y = headerRect.yMax,
                height = GetListItemsHeight(),
            };
            var footerRect = new Rect(listRect)
            {
                y = itemsRect.yMax,
                height = GetListFooterHeight(),
            };

            Default.DrawListBackground(listRect);

            // we need to draw the search field before the items, to prevent the control IDs from changing during the search
            DoListHeader(headerRect);
            DoListFooter(footerRect);
            DoListItems(itemsRect);
        }

        void DoListControls(Rect rect)
        {
            var addRect = new Rect(rect)
            {
                height = Defaults.ButtonHeight,
            };
            var removeRect = new Rect(addRect)
            {
                y = addRect.yMax,
            };
            var menuRect = new Rect(removeRect)
            {
                y = removeRect.yMax + Defaults.ButtonSpacing,
            };
            var upRect = new Rect(menuRect)
            {
                y = menuRect.yMax + Defaults.ButtonSpacing,
            };
            var downRect = new Rect(upRect)
            {
                y = upRect.yMax,
            };

            var indexInvalid = Index < 0 || Index >= Count;

            using (new EditorGUI.DisabledScope(OnCanAddCallback != null && !OnCanAddCallback()))
            {
                if (GUI.Button(addRect, OnAddDropdownCallback != null ? Default.IconToolbarPlusMore : Default.IconToolbarPlus, Default.ControlButton))
                {
                    if (OnAddDropdownCallback != null)
                    {
                        OnAddDropdownCallback(addRect);
                    }
                    else if (OnAddCallback == null)
                    {
                        Default.DoAddButton(this);
                    }
                    else
                    {
                        OnAddCallback();
                    }

                    OnChangedCallback?.Invoke();
                }
            }

            using (new EditorGUI.DisabledScope(indexInvalid || (OnCanRemoveCallback != null && !OnCanRemoveCallback())))
            {
                if (GUI.Button(removeRect, Default.IconToolbarMinus, Default.ControlButton))
                {
                    if (OnRemoveCallback == null)
                    {
                        Default.DoRemoveButton(this);
                    }
                    else
                    {
                        OnRemoveCallback();
                    }

                    OnChangedCallback?.Invoke();
                }
            }

            if (OnMenuCallback != null || Searchable)
            {
                if (GUI.Button(menuRect, Default.MenuButton, Default.ControlButton))
                {
                    var menu = new GenericMenu();

                    if (OnMenuCallback != null)
                    {
                        OnMenuCallback(menu);
                    }
                    else if (Searchable)
                    {
                        menu.AddItem(Default.ShowSearchbar, ShowSearchBar, () =>
                        {
                            ShowSearchBar = !ShowSearchBar;
                        });
                    }

                    menu.DropDown(menuRect);
                }
            }

            if (Reorderable)
            {
                using (new EditorGUI.DisabledScope(indexInvalid || Count < 2))
                {
                    if (GUI.Button(upRect, Default.MoveUpIcon, Default.ControlButton))
                    {
                        MoveSelectionUp();
                    }
                    if (GUI.Button(downRect, Default.MoveDownIcon, Default.ControlButton))
                    {
                        MoveSelectionDown();
                    }
                }
            }
        }

        void MoveSelectionUp()
        {
            var arraySize = Count;
            var dstIndex = (Index - 1 + arraySize) % arraySize;
            MoveSelection(dstIndex);
        }

        void MoveSelectionDown()
        {
            var arraySize = Count;
            var dstIndex = (Index + 1 + arraySize) % arraySize;
            MoveSelection(dstIndex);
        }

        void MoveSelection(int dstIndex)
        {
            var srcIndex = Index;

            if (Property != null)
            {
                Property.MoveArrayElement(srcIndex, dstIndex);
            }
            else
            {
                var value = List[srcIndex];
                List.RemoveAt(srcIndex);
                List.Insert(dstIndex, value);
            }

            Index = dstIndex;

            OnReorderCallback?.Invoke(srcIndex, dstIndex);
        }

        void DoListItems(Rect rect)
        {
            Profiler.BeginSample($"{nameof(CompactList)}.{nameof(DoListItems)}()");

            var itemCount = FilterList();
            var drawNoneListItem = itemCount == 0;

            // don't draw over the list box border
            var scrollRect = rect;
            scrollRect.min += Vector2.one * Defaults.BoxPadding;
            scrollRect.max -= Vector2.one * Defaults.BoxPadding;

            // determine how much space is needed for the list
            var listRect = scrollRect;
            listRect.height = drawNoneListItem ? EditorGUIUtility.singleLineHeight : ItemHeight * itemCount;

            // if the list is larger the scroll box we need to make room for the scroll bar
            var showScrollbar = listRect.yMax > scrollRect.yMax;

            if (showScrollbar)
                listRect.xMax -= 16f;

            // the list items are scrollable
            using (var scroll = new GUI.ScrollViewScope(scrollRect, new Vector2(0, m_ScrollPosition), listRect, false, showScrollbar))
            {
                // find how much content is not visible
                var viewHeight = scrollRect.height;
                var contentHeight = listRect.height;
                var excessHeight = contentHeight - viewHeight;

                // find the bounds of the currently visible area
                var normalizedScroll = m_ScrollPosition / excessHeight;
                var viewBottom = viewHeight + (excessHeight * normalizedScroll);
                var viewTop = viewBottom - viewHeight;

                // draw the items
                for (var i = 0; i < itemCount; i++)
                {
                    // clip if the rect is not visible in the scroll window
                    if (IsItemClipped(viewBottom, viewTop, i))
                        continue;

                    var itemRect = GetItemRect(listRect, i);

                    // get the original element index
                    var idx = IsSearchActive ? m_SearchElementToItem.Reverse[i] : i;
                    var activeElement = (idx == Index);
                    var focusedElement = (idx == Index && HasKeyboardControl());

                    // draw the background
                    if (DrawListItemBackgroundCallback == null)
                    {
                        Default.DrawListItemBackground(itemRect, idx, activeElement, focusedElement);
                    }
                    else
                    {
                        DrawListItemBackgroundCallback(itemRect, idx, activeElement, focusedElement);
                    }

                    // draw the list item
                    var itemContentRect = new Rect(itemRect)
                    {
                        xMin = itemRect.xMin + Defaults.ItemPadding,
                        xMax = itemRect.xMax - Defaults.ItemPadding,
                    };

                    if (DrawListItemCallback == null)
                    {
                        Default.DrawListItem(this, itemContentRect, idx, activeElement, focusedElement);
                    }
                    else
                    {
                        DrawListItemCallback(itemContentRect, idx, activeElement, focusedElement);
                    }
                }

                if (!drawNoneListItem)
                {
                    ProcessListItemInteraction(listRect, itemCount);
                }
                else if (!IsSearchActive)
                {
                    var itemContentRect = new Rect(listRect)
                    {
                        xMin = listRect.xMin + Defaults.ItemPadding,
                        xMax = listRect.xMax - Defaults.ItemPadding,
                    };

                    if (DrawNoneListItemCallback == null)
                    {
                        Default.DrawNoneListItem(this, itemContentRect);
                    }
                    else
                    {
                        DrawNoneListItemCallback(itemContentRect);
                    }
                }

                var lastScrollPosition = m_ScrollPosition;
                m_ScrollPosition = scroll.scrollPosition.y;

                if (m_ScrollToElement >= 0)
                {
                    ScrollToElement(viewHeight, viewBottom, viewTop, m_ScrollToElement);
                    m_ScrollToElement = -1;
                }

                if (m_ScrollPosition != lastScrollPosition)
                    SessionState.SetFloat($"{m_StatePath}/scrollPosition", m_ScrollPosition);
            }

            Profiler.EndSample();
        }

        int FilterList()
        {
            if (!IsSearchActive)
                return Count;

            Profiler.BeginSample($"{nameof(CompactList)}.{nameof(FilterList)}()");

            // If there is a search active we must find the mapping between filtered items shown
            // in the list and the original elements in the list.
            m_SearchElementToItem.Clear();

            for (var i = 0; i < Count; i++)
            {
                bool passesFilter;

                if (SearchFilterCallback == null)
                {
                    passesFilter = Default.DoSearchFilter(this, i, SearchFilter);
                }
                else
                {
                    passesFilter = SearchFilterCallback(i, SearchFilter);
                }

                if (passesFilter)
                {
                    m_SearchElementToItem.Add(i, m_SearchElementToItem.Count);
                }
            }

            Profiler.EndSample();

            return m_SearchElementToItem.Count;
        }

        void ProcessListItemInteraction(Rect listRect, int itemCount)
        {
            var evt = Event.current;
            var lastIndex = Index;
            var clicked = false;

            switch (evt.GetTypeForControl(m_ID))
            {
                case EventType.KeyDown:
                {
                    // if we have keyboard focus, arrow through the list
                    if (GUIUtility.keyboardControl != m_ID)
                        return;

                    if (evt.keyCode == KeyCode.DownArrow)
                    {
                        // find the next item that passes the search filter, if any
                        if (IsSearchActive)
                        {
                            for (var i = Mathf.Max(0, Index + 1); i < Count; i++)
                            {
                                if (m_SearchElementToItem.ContainsKey(i))
                                {
                                    Index = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Index = Mathf.Clamp(Index + 1, 0, itemCount - 1);
                        }
                        evt.Use();
                    }
                    if (evt.keyCode == KeyCode.UpArrow)
                    {
                        // find the previous item that passes the search filter, if any
                        if (IsSearchActive)
                        {
                            for (var i = Mathf.Min(Count - 1, Index - 1); i >= 0; i--)
                            {
                                if (m_SearchElementToItem.ContainsKey(i))
                                {
                                    Index = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Index = Mathf.Clamp(Index - 1, 0, itemCount - 1);
                        }
                        evt.Use();
                    }
                    break;
                }
                case EventType.MouseDown:
                {
                    if (!listRect.Contains(evt.mousePosition) || (evt.button != 0 && evt.button != 1))
                        break;

                    // clicking on the list should end editing any existing edits
                    s_EndEditingActiveTextField.Invoke(null, null);

                    GrabKeyboardFocus();

                    // do not auto-scroll to the new selection, it can be confusing to the user
                    var item = GetItemUnderMouse(listRect, itemCount);
                    m_Index = IsSearchActive ? m_SearchElementToItem.Reverse[item] : item;
                    SessionState.SetInt($"{m_StatePath}/index", m_Index);

                    // Prevent consuming the right mouse event in order to enable context menus
                    if (evt.button == 1)
                        break;

                    evt.Use();
                    clicked = true;
                    break;
                }
                case EventType.MouseUp:
                {
                    // if mouse up was on the same index as mouse down we fire a mouse up callback
                    // (useful if for beginning renaming on mouseup and so on)
                    if (OnMouseUpCallback != null)
                    {
                        var item = GetItemUnderMouse(listRect, itemCount);

                        if (item == Index && GetItemRect(listRect, item).Contains(evt.mousePosition))
                        {
                            OnMouseUpCallback();
                        }
                    }
                    break;
                }
            }

            // if the index has changed and there is a selected callback, call it
            if (OnSelectCallback != null && (Index != lastIndex || clicked))
            {
                OnSelectCallback();
            }
        }

        Rect GetItemRect(Rect listRect, int item)
        {
            return new Rect(listRect.x, listRect.y + (ItemHeight * item), listRect.width, ItemHeight);
        }

        int GetItemUnderMouse(Rect listRect, int itemCount)
        {
            var viewY = Event.current.mousePosition.y - listRect.y;
            return Mathf.Clamp(Mathf.FloorToInt(viewY / ItemHeight), 0, itemCount - 1);
        }

        bool IsItemClipped(float viewBottom, float viewTop, int itemIndex)
        {
            var itemTop = ItemHeight * itemIndex;

            if (itemTop > viewBottom + 1f)
                return true;

            var itemBottom = itemTop + ItemHeight;

            if (itemBottom < viewTop - 1f)
                return true;

            return false;
        }

        void ScrollToElement(float viewHeight, float viewBottom, float viewTop, int elementIndex)
        {
            // only scroll to valid elements
            if (elementIndex < 0 || elementIndex >= Count)
                return;

            // find the item corresponding to the target element
            var itemIndex = elementIndex;

            // do not scroll to elements that are filtered out by the search
            if (IsSearchActive && !m_SearchElementToItem.Forward.TryGetValue(itemIndex, out itemIndex))
                return;

            // find the bounds of the selected item
            var itemTop = ItemHeight * itemIndex;
            var itemBottom = itemTop + ItemHeight;

            // Constrain how close to the edge of the viewport the selected item can be,
            // unless the selection is near the beginning or end of the list.
            // A view margin of 0 will adjust the view to only include the selected item,
            // while larger values will show additional values past the selection in the
            // scroll direction. Each multiple reveals an additional item.
            const float viewMargin = 1f;
            var margin = Mathf.Min(ItemHeight * viewMargin, (viewHeight - ItemHeight) / 2f);

            var adjustedTop = itemTop - margin;
            var adjustedBottom = itemBottom + margin;

            float desiredViewBottom;
            if (adjustedTop < viewTop)
            {
                desiredViewBottom = adjustedTop + viewHeight;
            }
            else if (adjustedBottom > viewBottom)
            {
                desiredViewBottom = adjustedBottom;
            }
            else
            {
                return;
            }

            // find the scroll position that shows the desired view
            m_ScrollPosition = desiredViewBottom - viewHeight;
        }

        void DoListHeader(Rect rect)
        {
            if (Searchable && ShowSearchBar)
            {
                var searchBarRect = new Rect(rect)
                {
                    xMin = rect.xMin + Defaults.SearchSidePadding,
                    xMax = rect.xMax - Defaults.SearchSidePadding,
                    yMin = rect.yMin + (2f * Defaults.SearchBottomPadding),
                    yMax = rect.yMax - Defaults.SearchBottomPadding,
                };

                SearchFilter = s_ToolbarSearchField?.Invoke(null, new object[] { searchBarRect, SearchFilter, false }) as string;
            }
        }

        void DoListFooter(Rect rect)
        {
            if (Resizable)
            {
                // draw a handle icon to signify the list is resizable to the center
                var resizeHandleRect = new Rect(rect)
                {
                    xMin = rect.center.x - 10f,
                    xMax = rect.center.x + 10f,
                    y = rect.y + 3f,
                };

                if (Event.current.type == EventType.Repaint)
                {
                    Default.ResizeHandle.Draw(resizeHandleRect, false, false, false, false);
                }

                // dragging anywhere near the bottom of the list will start resizing
                var resizeRect = new Rect(rect)
                {
                    height = Defaults.ResizeHandleHeight,
                };

                // when resizing the list always show the resize cursor even if not over the hot spot
                var alwaysRect = new Rect(Vector2.zero, Vector2.one * 100000f);
                EditorGUIUtility.AddCursorRect(m_Resizing ? alwaysRect : resizeRect, MouseCursor.ResizeVertical);

                ProcessResizeInteraction(resizeRect);
            }
        }

        void ProcessResizeInteraction(Rect resizeRect)
        {
            var evt = Event.current;

            switch (evt.GetTypeForControl(m_ID))
            {
                case EventType.KeyDown:
                {
                    if (GUIUtility.keyboardControl != m_ID)
                        return;

                    if (evt.keyCode == KeyCode.Escape && GUIUtility.hotControl == m_ID)
                    {
                        GUIUtility.hotControl = 0;
                        m_Resizing = false;

                        if (ListHeight != m_ResizeStartHeight)
                        {
                            ListHeight = m_ResizeStartHeight;
                            OnResizeCallback?.Invoke();
                        }

                        evt.Use();
                    }
                    break;
                }
                case EventType.MouseDown:
                {
                    if (!resizeRect.Contains(evt.mousePosition) || (evt.button != 0 && evt.button != 1))
                        break;

                    // clicking on the list should end editing any existing edits
                    s_EndEditingActiveTextField.Invoke(null, null);

                    GrabKeyboardFocus();

                    if (evt.button == 0)
                    {
                        GUIUtility.hotControl = m_ID;
                        m_Resizing = true;
                        m_ResizeStartOffset = evt.mousePosition.y;
                        m_ResizeStartHeight = ListHeight;
                    }

                    // Prevent consuming the right mouse event in order to enable the context menu
                    if (evt.button == 1)
                        break;

                    evt.Use();
                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl != m_ID)
                        break;

                    var targetHeight = m_ResizeStartHeight + (evt.mousePosition.y - m_ResizeStartOffset);
                    var snappedHeight = Mathf.RoundToInt(targetHeight / ItemHeight) * ItemHeight;

                    if (ListHeight != snappedHeight)
                    {
                        ListHeight = snappedHeight;
                        OnResizeCallback?.Invoke();
                    }

                    evt.Use();
                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl != m_ID)
                        break;

                    GUIUtility.hotControl = 0;
                    m_Resizing = false;

                    evt.Use();
                    break;
                }
            }
        }

        void DoElement(Rect rect)
        {
            if (Index < 0 || Index >= Count)
                return;

            if (DrawElementCallback == null)
            {
                if (Property != null)
                {
                    var element = Property.GetArrayElementAtIndex(Index);
                    EditorGUI.PropertyField(rect, element, true);
                }
            }
            else
            {
                DrawElementCallback(rect, Index);
            }
        }

        /// <summary>
        /// Scrolls the list to so that the item at the given index is visible to the user.
        /// </summary>
        /// <param name="index">The index of the element to scroll to.</param>
        public void ScrollToItem(int index)
        {
            if (index < 0 || index >= Count)
                return;

            m_ScrollToElement = index;
        }

        /// <summary>
        /// Checks if the keyboard is currently focused on the list.
        /// </summary>
        /// <remarks>
        /// When the list has focus, the user is able to use arrow keys to change the active element.
        /// </remarks>
        /// <returns>True if the list is focused; false otherwise.</returns>
        public bool HasKeyboardControl()
        {
            return GUIUtility.keyboardControl == m_ID && (bool)s_HasCurrentWindowKeyFocus.Invoke(null, null);
        }

        /// <summary>
        /// Focuses the list control.
        /// </summary>
        /// <remarks>
        /// When the list has focus, the user is able to use arrow keys to change the active element.
        /// </remarks>
        public void GrabKeyboardFocus()
        {
            GUIUtility.keyboardControl = m_ID;
        }

        /// <summary>
        /// Releases the keyboard focus if the list is currently holding the focus.
        /// </summary>
        public void ReleaseKeyboardFocus()
        {
            if (GUIUtility.keyboardControl == m_ID)
                GUIUtility.keyboardControl = 0;
        }
    }
}
