using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A window used to manage connections to external devices.
    /// </summary>
    public class ConnectionsWindow : EditorWindow
    {
        static class IDs
        {
            public const string AddMenu = "add-connection";
            public const string SplitViewVerticalContainer = "split-vertical";
            public const string SplitViewHorizontalContainer = "split-horizontal";
            public const string SplitView = "split-view";
            public const string InfoSection = "info-section";
            public const string SettingsSection = "settings-section";
            public const string FirewallSection = "firewall-section";
            public const string FirewallButton = "firewall-button";
        }

        static class Constants
        {
            static readonly string IconPath = $"Packages/{LiveCaptureInfo.Name}/Editor/Core/Icons";
            public const string WindowName = "Connections";
            public const string WindowPath = "Window/Live Capture/" + WindowName;
            public static readonly GUIContent WindowTitle = EditorGUIUtility.TrTextContentWithIcon(WindowName, $"{IconPath}/LiveCaptureConnectionWindow.png");
            public static readonly Vector2 WindowSize = new Vector2(100f, 100f);

            public const int SplitSize = 400;
            public static readonly Vector2 MinPaneSize = new Vector2(80f, 30f);
            public const long UpdateInterval = 16;
            public const string VerticalPaneKey = "connections-window-vertical-pane";
            public const string VerticalInfoScrollKey = "connections-window-vertical-info-scroll";
            public const string VerticalSettingsScrollKey = "connections-window-vertical-settings-scroll";
            public const string HorizontalPaneKey = "connections-window-horizontal-pane";
            public const string HorizontalInfoScrollKey = "connections-window-horizontal-info-scroll";
            public const string HorizontalSettingsScrollKey = "connections-window-horizontal-settings-scroll";
        }

        static IEnumerable<(Type, CreateConnectionMenuItemAttribute[])> s_CreateConnectionMenuItems;
        static bool s_FirewallConfigured;

        [SerializeField]
        VisualTreeAsset m_WindowUxml;
        [SerializeField]
        StyleSheet m_WindowCommonUss;
        [SerializeField]
        StyleSheet m_WindowLightUss;
        [SerializeField]
        StyleSheet m_WindowDarkUss;

        VisualElement m_SplitViewVerticalContainer;
        VisualElement m_SplitViewHorizontalContainer;
        TwoPaneSplitView m_SplitViewVertical;
        TwoPaneSplitView m_SplitViewHorizontal;
        ScrollView m_InfoSectionVertical;
        ScrollView m_SettingsSectionVertical;
        ScrollView m_InfoSectionHorizontal;
        ScrollView m_SettingsSectionHorizontal;
        VisualElement m_FirewallSection;

        float? m_InfoSectionVerticalScroll;
        float? m_InfoSectionHorizontalScroll;
        float? m_SettingsSectionVerticalScroll;
        float? m_SettingsSectionHorizontalScroll;

        readonly Dictionary<Connection, ConnectionEditor> m_EditorCache = new Dictionary<Connection, ConnectionEditor>();

        /// <summary>
        /// Opens an instance of the connections window.
        /// </summary>
        [MenuItem(Constants.WindowPath)]
        public static void ShowWindow()
        {
            GetWindow<ConnectionsWindow>();
        }

        void OnEnable()
        {
            titleContent = Constants.WindowTitle;
            minSize = Constants.WindowSize;

            if (FirewallUtility.IsSupported)
            {
                s_FirewallConfigured = FirewallUtility.IsConfigured();
                FirewallUtility.FirewallConfigured += OnFirewallConfigured;
            }

            ConnectionManager.ConnectionAdded += OnConnectionAdded;
            ConnectionManager.ConnectionRemoved += OnConnectionRemoved;
            ConnectionManager.ConnectionChanged += OnConnectionChanged;
        }

        void OnDisable()
        {
            if (FirewallUtility.IsSupported)
            {
                FirewallUtility.FirewallConfigured -= OnFirewallConfigured;
            }

            ConnectionManager.ConnectionAdded -= OnConnectionAdded;
            ConnectionManager.ConnectionRemoved -= OnConnectionRemoved;
            ConnectionManager.ConnectionChanged -= OnConnectionChanged;

            foreach (var editor in m_EditorCache.Values)
            {
                DestroyImmediate(editor);
            }

            m_EditorCache.Clear();
        }

        void OnConnectionAdded(Connection connection)
        {
            if (m_InfoSectionVertical == null)
            {
                return;
            }

            if (!m_EditorCache.TryGetValue(connection, out var editor))
            {
                editor = UnityEditor.Editor.CreateEditor(connection) as ConnectionEditor;

                var info = editor.BuildInfoGUI();
                m_InfoSectionVertical.Add(info);

                m_EditorCache.Add(connection, editor);

                if (connection.IsSelected)
                {
                    InspectConnection(connection);
                }

                editor.OnConnectionChanged();
            }
            else if (!ReferenceEquals(connection, editor.target))
            {
                DestroyImmediate(editor);
                editor = UnityEditor.Editor.CreateEditor(connection) as ConnectionEditor;
                m_EditorCache[connection] = editor;
            }
        }

        void OnConnectionRemoved(Connection connection)
        {
            if (m_EditorCache.TryGetValue(connection, out var editor))
            {
                DestroyImmediate(editor);
                m_EditorCache.Remove(connection);
            }
        }

        void OnConnectionChanged(Connection connection)
        {
            if (m_EditorCache.TryGetValue(connection, out var editor))
            {
                editor.OnConnectionChanged();
            }
        }

        void InspectConnection(Connection connection)
        {
            foreach (var editor in m_EditorCache.Values)
            {
                editor.StopInspection();
            }

            m_SettingsSectionVertical.Clear();
            m_SettingsSectionHorizontal.Clear();

            if (m_EditorCache.TryGetValue(connection, out var ed))
            {
                ed.StartInspection();

                var settings = ed.BuildSettingsGUI();
                m_SettingsSectionVertical.Add(settings);
            }
        }

        static void OnFirewallConfigured(bool successful)
        {
            s_FirewallConfigured = successful;
        }

        void CreateGUI()
        {
            m_WindowUxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(m_WindowCommonUss);
            rootVisualElement.styleSheets.Add(EditorGUIUtility.isProSkin ? m_WindowDarkUss : m_WindowLightUss);

            var addMenu = rootVisualElement.Q<ToolbarMenu>(IDs.AddMenu);
            SetupAddMenu(addMenu);

            m_SplitViewVerticalContainer = rootVisualElement.Q<VisualElement>(IDs.SplitViewVerticalContainer);
            m_SplitViewHorizontalContainer = rootVisualElement.Q<VisualElement>(IDs.SplitViewHorizontalContainer);
            m_SplitViewVertical = m_SplitViewVerticalContainer.Q<TwoPaneSplitView>(IDs.SplitView);
            m_SplitViewHorizontal = m_SplitViewHorizontalContainer.Q<TwoPaneSplitView>(IDs.SplitView);

            // Workaround for IN-21036
            m_SplitViewVertical.orientation = TwoPaneSplitViewOrientation.Vertical;

            m_InfoSectionVertical = m_SplitViewVertical.Q<ScrollView>(IDs.InfoSection);
            m_SettingsSectionVertical = m_SplitViewVertical.Q<ScrollView>(IDs.SettingsSection);
            m_InfoSectionHorizontal = m_SplitViewHorizontal.Q<ScrollView>(IDs.InfoSection);
            m_SettingsSectionHorizontal = m_SplitViewHorizontal.Q<ScrollView>(IDs.SettingsSection);
            m_FirewallSection = rootVisualElement.Q<VisualElement>(IDs.FirewallSection);
            rootVisualElement.Q<Button>(IDs.FirewallButton).clickable.clicked += OnFirewallButtonClicked;

            m_SplitViewVertical.viewDataKey = Constants.VerticalPaneKey;
            m_SplitViewHorizontal.viewDataKey = Constants.HorizontalPaneKey;
            m_InfoSectionVertical.viewDataKey = Constants.VerticalInfoScrollKey;
            m_SettingsSectionVertical.viewDataKey = Constants.VerticalSettingsScrollKey;
            m_InfoSectionHorizontal.viewDataKey = Constants.HorizontalInfoScrollKey;
            m_SettingsSectionHorizontal.viewDataKey = Constants.HorizontalSettingsScrollKey;

            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnLayoutReady);

            rootVisualElement.RegisterCallback<ConnectionEditor.InspectConnectionEvent>(evt => InspectConnection(evt.Connection));

            m_InfoSectionHorizontal.RegisterCallback<GeometryChangedEvent>(evt =>
                TransferScroll(ref m_InfoSectionVerticalScroll, m_InfoSectionHorizontal, evt));
            m_InfoSectionVertical.RegisterCallback<GeometryChangedEvent>(evt =>
                TransferScroll(ref m_InfoSectionHorizontalScroll, m_InfoSectionVertical, evt));

            m_SettingsSectionHorizontal.RegisterCallback<GeometryChangedEvent>(evt =>
                TransferScroll(ref m_SettingsSectionVerticalScroll, m_SettingsSectionHorizontal, evt));
            m_SettingsSectionVertical.RegisterCallback<GeometryChangedEvent>(evt =>
                TransferScroll(ref m_SettingsSectionHorizontalScroll, m_SettingsSectionVertical, evt));

            foreach (var connection in ConnectionManager.Instance.Connections)
            {
                OnConnectionAdded(connection);
            }

            rootVisualElement.schedule.Execute(UpdateUI).Every(Constants.UpdateInterval);
        }

        void OnLayoutReady(GeometryChangedEvent evt)
        {
            m_SplitViewVertical.fixedPaneInitialDimension = evt.newRect.height * 0.35f;
            m_SplitViewHorizontal.fixedPaneInitialDimension = evt.newRect.width * 0.5f;

            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnLayoutReady);
        }

        void UpdateUI()
        {
            UpdateSplit();
            UpdateFirewall();
        }

        void UpdateSplit()
        {
            var windowWidth = rootVisualElement.layout.width;
            var displayVertical = windowWidth < Constants.SplitSize;
            m_SplitViewVerticalContainer.SetDisplay(displayVertical);
            m_SplitViewHorizontalContainer.SetDisplay(!displayVertical);

            // Only apply minimum dimensions when the matching layout is active.
            // Otherwise the resize resulting from minimum dimensions will scramble TwoPaneSplitView.m_FixedPaneDimension.
            if (m_SplitViewVertical.fixedPane != null && m_SplitViewVertical.flexedPane != null)
            {
                m_SplitViewVertical.fixedPane.style.minWidth = displayVertical ? new StyleLength(Constants.MinPaneSize.x) : new StyleLength(StyleKeyword.None);
                m_SplitViewVertical.fixedPane.style.minHeight = displayVertical ? new StyleLength(Constants.MinPaneSize.y) : new StyleLength(StyleKeyword.None);
                m_SplitViewVertical.flexedPane.style.minWidth = displayVertical ? new StyleLength(Constants.MinPaneSize.x) : new StyleLength(StyleKeyword.None);
                m_SplitViewVertical.flexedPane.style.minHeight = displayVertical ? new StyleLength(Constants.MinPaneSize.y) : new StyleLength(StyleKeyword.None);
            }
            if (m_SplitViewHorizontal.fixedPane != null && m_SplitViewHorizontal.flexedPane != null)
            {
                m_SplitViewHorizontal.fixedPane.style.minWidth = !displayVertical ? new StyleLength(Constants.MinPaneSize.x) : new StyleLength(StyleKeyword.None);
                m_SplitViewHorizontal.fixedPane.style.minHeight = !displayVertical ? new StyleLength(Constants.MinPaneSize.y) : new StyleLength(StyleKeyword.None);
                m_SplitViewHorizontal.flexedPane.style.minWidth = !displayVertical ? new StyleLength(Constants.MinPaneSize.x) : new StyleLength(StyleKeyword.None);
                m_SplitViewHorizontal.flexedPane.style.minHeight = !displayVertical ? new StyleLength(Constants.MinPaneSize.y) : new StyleLength(StyleKeyword.None);
            }

            if (displayVertical)
            {
                m_InfoSectionHorizontal.MoveChildrenTo(m_InfoSectionVertical);
                m_SettingsSectionHorizontal.MoveChildrenTo(m_SettingsSectionVertical);

                m_InfoSectionVerticalScroll = m_InfoSectionVertical.scrollOffset.y;
                m_SettingsSectionVerticalScroll = m_SettingsSectionVertical.scrollOffset.y;
            }
            else
            {
                m_InfoSectionVertical.MoveChildrenTo(m_InfoSectionHorizontal);
                m_SettingsSectionVertical.MoveChildrenTo(m_SettingsSectionHorizontal);

                m_InfoSectionHorizontalScroll = m_InfoSectionHorizontal.scrollOffset.y;
                m_SettingsSectionHorizontalScroll = m_SettingsSectionHorizontal.scrollOffset.y;
            }
        }

        void UpdateFirewall()
        {
            var displayFirewallSection = FirewallUtility.IsSupported && !s_FirewallConfigured;
            m_FirewallSection.SetDisplay(displayFirewallSection);
        }

        void SetupAddMenu(ToolbarMenu menu)
        {
            if (s_CreateConnectionMenuItems == null)
            {
                s_CreateConnectionMenuItems = AttributeUtility.GetAllTypes<CreateConnectionMenuItemAttribute>();
            }

            MenuUtility.SetupMenu(menu.menu, s_CreateConnectionMenuItems, (t) => !ConnectionManager.Instance.HasConnection(t), (type, attribute) =>
            {
                ConnectionManager.Instance.CreateConnection(type);
            });
        }

        void OnFirewallButtonClicked()
        {
            FirewallUtility.ConfigureFirewall();
        }

        void TransferScroll(ref float? source, ScrollView destination, GeometryChangedEvent evt)
        {
            if (source.HasValue && evt.newRect.height > 0f)
            {
                var scroll = destination.scrollOffset;
                scroll.y = source.Value;
                destination.scrollOffset = scroll;
                source = null;
            }
        }
    }
}
