using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// The base class for clients and servers used to communicate with external devices.
    /// </summary>
    /// <remarks>
    /// You should instantiate connections using <see cref="ConnectionManager.CreateConnection"/> to prevent creating duplicates.
    /// The <see cref="ConnectionManager"/> serializes connections to the Library folder of the project, so the
    /// connection configuration is stored locally for the user. The user manages connections via the Connections window.
    /// </remarks>
    public abstract class Connection : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable 414
        [SerializeField, HideInInspector]
        bool m_IsSelected;
#pragma warning restore 414

        internal bool IsSelected => m_IsSelected;
#endif

        /// <summary>
        /// Checks the connection state.
        /// </summary>
        /// <returns>True if the connection is enabled.</returns>
        public abstract bool IsEnabled();

        /// <summary>
        /// Enables or disables the connection.
        /// </summary>
        /// <param name="enabled">The new connection state.</param>
        public abstract void SetEnabled(bool enabled);

        /// <summary>
        /// Called when the connection is created.
        /// </summary>
        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.DontSave;

            ConnectionManager.Instance.AddConnection(this);
        }

        /// <summary>
        /// Called when the connection is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            ConnectionManager.Instance.RemoveConnection(this);
        }

        /// <summary>
        /// Called when the connection is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            ConnectionManager.Instance.Save();
        }

        /// <summary>
        /// Called when the connection's serialized fields have been changed from the inspector.
        /// </summary>
        protected virtual void OnValidate()
        {
            OnChanged(true);
        }

        /// <summary>
        /// Gets the display name for this connection.
        /// </summary>
        /// <returns>
        /// A string containing the name of the connection.
        /// </returns>
        public abstract string GetName();

        /// <summary>
        /// Unity calls this method regularly to update the connection.
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Call this when the connection's state or serialized data has been modified.
        /// </summary>
        /// <remarks>
        /// This triggers a refresh of the connection GUI.
        /// </remarks>
        /// <param name="save">Should the connection's serialized data be re-written to disk.</param>
        protected void OnChanged(bool save)
        {
            ConnectionManager.Instance.OnConnectionChanged(this);

            if (save)
            {
                ConnectionManager.Instance.Save();
            }
        }
    }
}
