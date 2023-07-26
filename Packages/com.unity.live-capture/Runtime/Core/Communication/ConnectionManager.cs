using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Unity.LiveCapture
{
    /// <summary>
    /// The class that manages <see cref="Connection"/> and serializes instances.
    /// </summary>
    public class ConnectionManager : ScriptableObject
    {
        const string k_AssetPath = "UserSettings/LiveCapture/ConnectionManager.asset";
        const string k_AddConnectionUndoName = "Add Connection";
        const string k_RemoveConnectionUndoName = "Remove Connection";

        /// <summary>
        /// An event invoked when a <see cref="Connection"/> is added.
        /// </summary>
        public static event Action<Connection> ConnectionAdded;

        /// <summary>
        /// An event invoked when a <see cref="Connection"/> is removed.
        /// </summary>
        public static event Action<Connection> ConnectionRemoved;

        /// <summary>
        /// An event invoked when a <see cref="Connection"/> has been modified.
        /// </summary>
        internal static event Action<Connection> ConnectionChanged;

        static ConnectionManager s_Instance;
        static bool s_Loading;
        static bool s_IsDirty;

        /// <summary>
        /// The <see cref="ConnectionManager"/> instance.
        /// </summary>
        public static ConnectionManager Instance
        {
            get
            {
                if (s_Instance == null)
                    CreateAndLoad();
                return s_Instance;
            }
        }

        [SerializeField, HideInInspector]
        List<Connection> m_Connections = new List<Connection>();

        /// <summary>
        /// The connection instances.
        /// </summary>
        public IEnumerable<Connection> Connections => m_Connections.Where(c => c != null);

        ConnectionManager()
        {
            if (s_Instance != null)
            {
                Debug.LogError($"{nameof(ConnectionManager)} already exists. Did you query the singleton in a constructor?");
            }
            else
            {
                s_Instance = this;
            }
        }

        /// <summary>
        /// Gets the <see cref="Connection"/> of a specified type, if it exists.
        /// </summary>
        /// <param name="connection">The returned connection, or <see langword="default"/> if there is no matching connection instance.</param>
        /// <typeparam name="TConnection">The type of the connection to get.</typeparam>
        /// <returns><see langword="true"/> if a matching connection instance was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetConnection<TConnection>(out TConnection connection) where TConnection : Connection
        {
            foreach (var c in m_Connections)
            {
                if (c is TConnection conn)
                {
                    connection = conn;
                    return true;
                }
            }

            connection = default;
            return false;
        }

        /// <summary>
        /// Checks if a <see cref="Connection"/> of a specified type exists.
        /// </summary>
        /// <param name="type">The type of connection to look for.</param>
        /// <returns><see langword="true"/> if a connection of the specified type exists; otherwise, <see langword="false"/>.</returns>
        public bool HasConnection(Type type)
        {
            foreach (var c in m_Connections)
            {
                if (type.IsInstanceOfType(c))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a <see cref="Connection"/> of a specified type.
        /// </summary>
        /// <remarks>
        /// You should create only one instance of each connection at a time.
        /// If a connection of type <paramref name="type"/> already exists, the
        /// method returns this existing connection instead of a new instance.
        /// </remarks>
        /// <param name="type">The type of connection to create.</param>
        /// <param name="registerUndo">When <see langword="true"/>, allows the user to undo creating the connection.</param>
        /// <returns>The connection instance.</returns>
        public Connection CreateConnection(Type type, bool registerUndo = true)
        {
            var connection = m_Connections.FirstOrDefault(c => type.IsInstanceOfType(c));

            if (connection != null)
                return connection;

            connection = CreateInstance(type) as Connection;

#if UNITY_EDITOR
            if (registerUndo)
            {
                Undo.SetCurrentGroupName(k_AddConnectionUndoName);
                Undo.RecordObject(this, k_AddConnectionUndoName);
                Undo.RegisterCreatedObjectUndo(connection, k_AddConnectionUndoName);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                EditorUtility.SetDirty(this);
            }
#endif

            return connection;
        }

        /// <summary>
        /// Destroys a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connection">The connection to destroy.</param>
        /// <param name="registerUndo">When <see langword="true"/>, allows the user to undo destroying the connection.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connection"/> is not owned
        /// by this instance.</exception>
        public void DestroyConnection(Connection connection, bool registerUndo = true)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (!m_Connections.Contains(connection))
                throw new ArgumentException(nameof(connection));

#if UNITY_EDITOR
            if (registerUndo)
            {
                Undo.SetCurrentGroupName(k_RemoveConnectionUndoName);
                Undo.RecordObject(this, k_RemoveConnectionUndoName);
                Undo.DestroyObjectImmediate(connection);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                EditorUtility.SetDirty(this);
            }
            else
#endif
            {
                DestroyImmediate(connection);
            }
        }

        /// <summary>
        /// Adds a connection to the manager.
        /// </summary>
        /// <param name="connection">The connection to add.</param>
        internal void AddConnection(Connection connection)
        {
            if (connection != null && !HasConnection(connection.GetType()))
            {
                m_Connections.Add(connection);
                ConnectionAdded?.Invoke(connection);
                OnConnectionChanged(connection);

                Save();
            }
        }

        /// <summary>
        /// Removes a connection from the manager.
        /// </summary>
        /// <param name="connection">The connection to remove.</param>
        internal void RemoveConnection(Connection connection)
        {
            if (connection != null && m_Connections.Remove(connection))
            {
                ConnectionRemoved?.Invoke(connection);
                OnConnectionChanged(connection);

                Save();
            }
        }

        /// <summary>
        /// Serializes the connections to disk.
        /// </summary>
        /// <remarks>
        /// The connection data file is stored in the UserSettings folder of the project.
        /// </remarks>
        internal void Save()
        {
            if (s_Loading)
                return;

            s_IsDirty = true;
        }

        /// <summary>
        /// Updates the connection manager and all connections.
        /// </summary>
        internal void Update()
        {
            // The undo system can cause a removed connection to suddenly become null.
            // We need to check the connections and ensure we remove any that were deleted.
            // This is because OnDestroy doesn't seem to always be called when a ScriptableObject
            // is removed via the undo system, so we can't cleanly remove them.
            m_Connections.RemoveAll(c => c == null);

            foreach (var connection in m_Connections)
            {
                connection.OnUpdate();
            }
        }

        /// <summary>
        /// Called to invoke the <see cref="ConnectionChanged"/> event.
        /// </summary>
        internal void OnConnectionChanged(Connection connection)
        {
            ConnectionChanged?.Invoke(connection);
        }

        static void CreateAndLoad()
        {
            if (s_Loading)
                return;

            s_Loading = true;

#if UNITY_EDITOR
            InternalEditorUtility.LoadSerializedFileAndForget(k_AssetPath);
#endif

            if (s_Instance != null)
            {
                // remove any connections that are not valid when loaded
                s_Instance.m_Connections.RemoveAll(c => c == null);
            }
            else
            {
                CreateInstance<ConnectionManager>().hideFlags = HideFlags.HideAndDontSave;
            }

            s_Loading = false;
        }

        void OnEnable()
        {
            foreach (var connection in s_Instance.m_Connections)
            {
                ConnectionAdded?.Invoke(connection);
                OnConnectionChanged(connection);
            }
        }

#if UNITY_EDITOR
        static void WriteToFile()
        {
            if (!s_IsDirty)
                return;

            var directoryName = Path.GetDirectoryName(k_AssetPath);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            // we serialize the manager and all the connections to a single file
            var objectsToSerialize = new ScriptableObject[] { s_Instance }
                .Union(s_Instance.Connections)
                .ToArray();

            InternalEditorUtility.SaveToSerializedFileAndForget(objectsToSerialize, k_AssetPath, true);

            s_IsDirty = false;
        }

        class WriteTrigger : UnityEditor.AssetModificationProcessor
        {
            static string[] OnWillSaveAssets(string[] paths)
            {
                WriteToFile();
                return paths;
            }
        }
#endif
    }
}
