using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// The base editor for <see cref="Connection"/> instances.
    /// </summary>
    /// <remarks>
    /// Inherit from this class when implementing the editor for a custom <see cref="Connection"/>.
    /// </remarks>
    [CustomEditor(typeof(Connection), true)]
    public class ConnectionEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Using paths instead of a default <see cref="SerializeField"/> reference.
        /// Default references are usually preferred but they wouldn't be available to derived classes.
        /// </summary>
        static class Paths
        {
            static readonly string BasePath = $"Packages/{LiveCaptureInfo.Name}/Editor/Core/Connections/";

            /// <summary>
            /// The default layout used by <see cref="CreateToolbarGUI"/>.
            /// </summary>
            public static readonly string ToolbarLayout = BasePath + "ConnectionToolbar.uxml";
        }

        /// <summary>
        /// Classes of elements in the Connections window layout.
        /// </summary>
        protected static class Classes
        {
            /// <summary>
            /// Apply to a <see cref="VisualElement"/> for a consistent left-indent.
            /// This should be applied to the root elements returned by <see cref="ConnectionEditor.CreateSettingsGUI"/>
            /// and <see cref="ConnectionEditor.CreateInfoGUI"/>.
            /// </summary>
            public const string IndentContent = "indent-content";

            /// <summary>
            /// Apply to a <see cref="VisualElement"/> for a consistent top and bottom margin.
            /// This should be applied to the root element returned by <see cref="ConnectionEditor.CreateSettingsGUI"/>
            /// and to elements in <see cref="ConnectionEditor.CreateInfoGUI"/> on a case-by-case basis.
            /// </summary>
            public const string SpaceContent = "space-content";
        }

        /// <summary>
        /// Private classes of elements in the Connections window layout.
        /// </summary>
        static class PrivateClasses
        {
            /// <summary>
            /// Apply to a <see cref="IMGUIContainer"/> for correct metrics.
            /// </summary>
            public const string IMGUIContainer = "connection-imgui-container";
        }

        /// <summary>
        /// The toolbar representing this connection in the left/top pane of the <see cref="ConnectionsWindow"/>.
        /// It contains the result of <see cref="CreateToolbarGUI"/>.
        /// </summary>
        ToolbarUI m_Toolbar;

        /// <summary>
        /// The root element in the left/top panel.
        /// It contains the grouped results of <see cref="CreateToolbarGUI"/> and <see cref="CreateInfoGUI"/>.
        /// </summary>
        VisualElement m_InfoRoot;

        /// <summary>
        /// The root element in the right/bottom panel (when this Connection is being inspected).
        /// It contains the result of <see cref="CreateSettingsGUI"/>.
        /// </summary>
        VisualElement m_SettingsRoot;

        /// <summary>
        /// True when this Connection is the current selection in the <see cref="ConnectionsWindow"/>.
        /// </summary>
        SerializedProperty m_IsSelected;

        /// <summary>
        /// Initializes the connection inspector.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_IsSelected = serializedObject.FindProperty("m_IsSelected");
        }

        /// <summary>
        /// Automatically removes the elements created by <see cref="CreateInfoGUI"/> and <see cref="CreateSettingsGUI"/> from the hierarchy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_InfoRoot != null)
            {
                m_InfoRoot.RemoveFromHierarchy();
            }

            if (m_SettingsRoot != null)
            {
                m_SettingsRoot.RemoveFromHierarchy();
            }
        }

        /// <summary>
        /// Called when the Connection referenced by <see cref="Editor.target"/> is modified.
        /// Override this function to update UI data based on the Connection.
        /// </summary>
        protected internal virtual void OnConnectionChanged()
        {
            m_Toolbar.Update(target as Connection);
        }

        /// <summary>
        /// Builds the information UI.
        /// </summary>
        /// <returns>The contents to display in the left/top pane of the <see cref="ConnectionsWindow"/>.</returns>
        internal VisualElement BuildInfoGUI()
        {
            m_InfoRoot = new VisualElement();
            m_InfoRoot.Add(CreateToolbarGUI().Root);
            m_InfoRoot.Add(CreateInfoGUI());
            return m_InfoRoot;
        }

        /// <summary>
        /// Builds the settings UI.
        /// </summary>
        /// <returns>The contents to display in the right/bottom pane of the <see cref="ConnectionsWindow"/> when this Connection is selected.</returns>
        internal VisualElement BuildSettingsGUI()
        {
            m_SettingsRoot = CreateSettingsGUI();
            return m_SettingsRoot;
        }

        /// <summary>
        /// Creates the toolbar for this Connection.
        /// </summary>
        /// <returns>The toolbar to display in the left/top pane of the <see cref="ConnectionsWindow"/>.</returns>
        ToolbarUI CreateToolbarGUI()
        {
            m_Toolbar = new DefaultToolbar();
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.ToolbarLayout);
            m_Toolbar.Build(layout.Instantiate(), target as Connection);
            return m_Toolbar;
        }

        /// <summary>
        /// Creates the info contents for this Connection. Override to implement your UI.
        /// </summary>
        /// <returns>The contents to display in the left/top pane of the <see cref="ConnectionsWindow"/> under the Connection's toolbar.</returns>
        /// <remarks>Use <see cref="Classes.IndentContent"/> and <see cref="Classes.SpaceContent"/> USS classes to add a consistent padding to its elements.</remarks>
        /// <seealso cref="CreateIMGUIContainer"/>
        protected virtual VisualElement CreateInfoGUI()
        {
            return new VisualElement();
        }

        /// <summary>
        /// Creates the contents of the editable settings for this Connection. Override to implement your UI.
        /// </summary>
        /// <returns>The contents to display in the right/bottom pane of the <see cref="ConnectionsWindow"/> when this connection is selected for inspection.</returns>
        /// <remarks>Use <see cref="Classes.IndentContent"/> and <see cref="Classes.SpaceContent"/> USS classes to add a consistent padding to its elements.</remarks>
        /// <seealso cref="CreateIMGUIContainer"/>
        protected virtual VisualElement CreateSettingsGUI()
        {
            return new VisualElement();
        }

        /// <summary>
        /// Return this in <see cref="CreateInfoGUI"/> and <see cref="CreateSettingsGUI"/> to implement your UI with IMGUI instead of UI Toolkit.
        ///
        /// <example>
        /// <code>
        /// // Define a void function containing the IMGUI code:
        /// void OnGUI()
        /// {
        ///     EditorGUILayout.Toggle(myContent, myValue);
        /// }
        /// // ...
        /// // In CreateInfoGUI or CreateSettingsGUI:
        /// return CreateIMGUIContainer(OnGUI);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="onGUIHandler">The function that's called to render and handle IMGUI events for the container.</param>
        /// <returns>An <see cref="IMGUIContainer"/> with the standard spacing and indentation for the <see cref="ConnectionsWindow"/>.</returns>
        protected IMGUIContainer CreateIMGUIContainer(Action onGUIHandler)
        {
            var container = new IMGUIContainer();

            container.onGUIHandler = () =>
            {
                DrawGUI(container, onGUIHandler);
            };

            container.AddToClassList(PrivateClasses.IMGUIContainer);
            container.AddToClassList(Classes.SpaceContent);
            container.AddToClassList(Classes.IndentContent);

            return container;
        }

        /// <summary>
        /// Wraps <paramref name="onGUIHandler"/> inside an <see cref="InspectorScope"/>.
        /// </summary>
        void DrawGUI(IMGUIContainer container, Action onGUIHandler)
        {
            var parentWidth = container.parent.contentRect.width;
            using (new InspectorScope(parentWidth))
            {
                onGUIHandler();
            }
        }

        /// <summary>
        /// Called by <see cref="ConnectionsWindow"/> when this Connection is selected for inspection.
        /// </summary>
        protected internal virtual void StartInspection()
        {
            m_Toolbar.Inspecting = true;

            if (m_IsSelected != null)
            {
                serializedObject.Update();
                m_IsSelected.boolValue = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Called by <see cref="ConnectionsWindow"/> when this Connection is no longer selected for inspection.
        /// </summary>
        protected internal virtual void StopInspection()
        {
            m_Toolbar.Inspecting = false;

            if (m_IsSelected != null)
            {
                serializedObject.Update();
                m_IsSelected.boolValue = false;
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Gets the toolbar that represents this Connection in the top/left pane of the <see cref="ConnectionsWindow"/>.
        /// </summary>
        /// <returns>The cached toolbar object.</returns>
        protected IToolbarUI GetToolbar()
        {
            Debug.Assert(m_Toolbar != null);
            return m_Toolbar;
        }

        /// <summary>
        /// Configures the IMGUI label width correctly for an IMGUIContainer.
        /// Without this scope, the label width never shrinks in response to the IMGUIContainer's width.
        /// </summary>
        class InspectorScope : IDisposable
        {
            bool m_Disposed;
            bool m_HierarchyMode;
            float m_LabelWidth;

            /// <summary>
            /// Sets up correct metrics.
            /// </summary>
            /// <param name="width">The width of the parent container.</param>
            public InspectorScope(float width)
            {
                m_HierarchyMode = EditorGUIUtility.hierarchyMode;
                m_LabelWidth = EditorGUIUtility.labelWidth;

                // EditorGUIUtility.labelWidth uses this formula then using hierarchyMode,
                // only that IMGUIContainer fails to setup a correct contextWidth;
                EditorGUIUtility.labelWidth = Mathf.Max(width * 0.45f - 40, 120f);
                EditorGUIUtility.hierarchyMode = true;
                EditorGUILayout.BeginVertical();
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (!m_Disposed)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUIUtility.labelWidth = m_LabelWidth;
                    EditorGUIUtility.hierarchyMode = m_HierarchyMode;

                    m_Disposed = true;
                }
            }
        }

        /// <summary>
        /// <see cref="ConnectionsWindow"/> listens to the bubble-up of this event to apply a new <see cref="Connection"/> selection.
        /// </summary>
        internal class InspectConnectionEvent : EventBase<InspectConnectionEvent>
        {
            /// <summary>
            /// The <see cref="Connection"/> to inspect.
            /// </summary>
            public Connection Connection
            {
                get;
                private set;
            }

            /// <summary>
            /// Call after ex <see cref="InspectConnectionEvent.GetPooled"/> to initialize the instance.
            /// </summary>
            /// <param name="connection">The newly selected connection.</param>
            /// <param name="evtTarget">The target element for the event.</param>
            public void Initialize(Connection connection, IEventHandler evtTarget)
            {
                Connection = connection;
                target = evtTarget;
                bubbles = true;
            }
        }

        /// <summary>
        /// Represents a toolbar for the list of Connections in the top/left pane of <see cref="ConnectionsWindow"/>.
        /// The styling and logic can be extended by modifying its elements directly, for example:
        ///
        /// <code>
        /// myToolbar.Root.styleSheets.Add(myStyleSheet);
        /// myToolbar.Indicator.AddToClassList(MyClassNames.Pending);
        /// myToolbar.Menu.menu.AppendAction("MyAction", myAction);
        /// </code>
        /// </summary>
        protected interface IToolbarUI
        {
            /// <summary>
            /// The root element of the toolbar layout.
            /// </summary>
            VisualElement Root { get; }

            /// <summary>
            /// The run status indicator.
            /// </summary>
            VisualElement Indicator { get; }

            /// <summary>
            /// The title of the connection.
            /// </summary>
            Label Title { get; }

            /// <summary>
            /// A toggle that starts/stops the connection based on its run state.
            /// </summary>
            Toggle Toggle { get; }

            /// <summary>
            /// Extensible menu.
            /// </summary>
            ToolbarMenu Menu { get; }
        }

        /// <inheritdoc/>
        abstract class ToolbarUI : IToolbarUI
        {
            /// <inheritdoc/>
            public VisualElement Root
            {
                get;
                protected set;
            }

            /// <inheritdoc/>
            public VisualElement Indicator
            {
                get;
                protected set;
            }

            /// <inheritdoc/>
            public Label Title
            {
                get;
                protected set;
            }

            /// <inheritdoc/>
            public Toggle Toggle
            {
                get;
                protected set;
            }

            /// <inheritdoc/>
            public ToolbarMenu Menu
            {
                get;
                protected set;
            }

            /// <summary>
            /// Set to true when the toolbar's connection is being inspected in the <see cref="ConnectionsWindow"/>.
            /// </summary>
            public abstract bool Inspecting { set; }

            /// <summary>
            /// Setup the UI logic.
            /// </summary>
            /// <param name="root">The toolbar UI layout.</param>
            /// <param name="connection">The connection associated with this toolbar.</param>
            public abstract void Build(VisualElement root, Connection connection);

            /// <summary>
            /// Update the UI to reflect new state of the Connection object.
            /// </summary>
            /// <param name="connection">The connection associated with this toolbar</param>
            public abstract void Update(Connection connection);
        }

        /// <inheritdoc/>
        class DefaultToolbar : ToolbarUI
        {
            static class IDs
            {
                public const string Indicator = "indicator";
                public const string Toggle = "toggle";
                public const string Title = "title";
                public const string Menu = "menu";
            }

            static class Classes
            {
                public const string Selected = "connection-toolbar-selected";
                public const string IsRunning = "connection-toolbar-running";
                public const string IndicatorIsRunning = "connection-indicator-running";
            }

            /// <inheritdoc/>
            public override bool Inspecting
            {
                set => m_FirstChild.EnableInClassList(Classes.Selected, value);
            }

            VisualElement m_FirstChild;

            /// <inheritdoc/>
            public override void Build(VisualElement root, Connection connection)
            {
                Root = root;

                m_FirstChild = Root.ElementAt(0);
                Debug.Assert(m_FirstChild != null);
                m_FirstChild.RegisterCallback<ClickEvent>(evt =>
                {
                    using (var inspectEvt = InspectConnectionEvent.GetPooled())
                    {
                        inspectEvt.Initialize(connection, m_FirstChild);
                        m_FirstChild.SendEvent(inspectEvt);
                    }
                });

                Indicator = root.Q<VisualElement>(IDs.Indicator);
                Debug.Assert(Indicator != null);

                Title = root.Q<Label>(IDs.Title);
                Debug.Assert(Title != null);

                Toggle = root.Q<Toggle>(IDs.Toggle);
                Debug.Assert(Toggle != null);
                Toggle.RegisterCallback<ClickEvent>(evt =>
                {
                    // Prevent triggering the m_Toolbar ClickEvent handler
                    evt.StopPropagation();
                });
                Toggle.RegisterValueChangedCallback(evt =>
                {
                    connection.SetEnabled(evt.newValue);
                });

                var toolbarMenu = root.Q<ToolbarMenu>(IDs.Menu);
                Debug.Assert(toolbarMenu != null);
                toolbarMenu.RegisterCallback<ClickEvent>(evt =>
                {
                    // Prevent triggering the m_Toolbar ClickEvent handler
                    evt.StopPropagation();
                });
                toolbarMenu.menu.AppendAction("Delete", action =>
                {
                    ConnectionManager.Instance.DestroyConnection(connection);
                });
            }

            /// <inheritdoc/>
            public override void Update(Connection connection)
            {
                Root.EnableInClassList(Classes.IsRunning, connection.IsEnabled());
                Indicator.EnableInClassList(Classes.IndicatorIsRunning, connection.IsEnabled());
                Title.text = connection.GetName();
                Toggle.SetValueWithoutNotify(connection.IsEnabled());
            }
        }
    }
}
