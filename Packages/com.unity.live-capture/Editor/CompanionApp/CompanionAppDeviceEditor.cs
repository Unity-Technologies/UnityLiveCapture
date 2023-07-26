using System.Linq;
using Unity.LiveCapture.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(CompanionAppDevice<>), true)]
    class CompanionAppDeviceEditor<TClient> : Editor where TClient : class, ICompanionAppClient
    {
        static class Contents
        {
            public static readonly GUIContent ActorLabel = EditorGUIUtility.TrTextContent("Actor", "The actor currently assigned to this device.");
            public static readonly GUIContent ChannelsLabel = EditorGUIUtility.TrTextContent("Channels", "The channels that will be recorded in the next take.");
            public static readonly GUIContent NotAssignedLabel = EditorGUIUtility.TrTextContent("None");
            public static readonly GUIContent ClientAssignLabel = EditorGUIUtility.TrTextContent("Client Device", "The remote device to capture recordings from. Only compatible connected devices are shown.");
        }

        static readonly string[] s_ExcludeProperties = { "m_Script" };

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                DoClientGUI();
                OnDeviceGUI();

                if (change.changed)
                {
                    var device = target as CompanionAppDevice<TClient>;

                    if (device.GetClient() != null)
                    {
                        device.UpdateClient();
                    }
                }
            }
        }

        /// <summary>
        /// Draws the Inspector for the inspected device.
        /// </summary>
        protected virtual void OnDeviceGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, s_ExcludeProperties);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the client selection drop-down.
        /// </summary>
        protected void DoClientGUI()
        {
            var device = target as CompanionAppDevice<TClient>;

            using (new EditorGUI.DisabledScope(!device.isActiveAndEnabled))
            {
                DoClientGUIInternal();
            }
        }

        void DoClientGUIInternal()
        {
            // Display a dropdown that enables users to select which client is assigned to a device.
            // The first value in the dropdown allows users to clear the device.
            var device = target as CompanionAppDevice<TClient>;
            var currentClient = device.GetClient();

            var currentOption = currentClient != null ? new GUIContent(currentClient.Name) : Contents.NotAssignedLabel;

            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, Contents.ClientAssignLabel);

            if (GUI.Button(rect, currentOption, EditorStyles.popup))
            {
                var clients = GetClients();
                var options = new GUIContent[clients.Length + 1];
                options[0] = Contents.NotAssignedLabel;

                var formatter = new UniqueNameFormatter();

                for (var i = 0; i < clients.Length; i++)
                {
                    var client = clients[i];
                    var name = client.Name;

                    if (ClientMappingDatabase.TryGetDevice(client, out var d))
                    {
                        name += client == currentClient ? $" (Current)" : $" (In Use)";
                    }

                    options[i + 1] = new GUIContent(formatter.Format(name));
                }

                OptionSelectWindow.SelectOption(rect, new Vector2(300f, 250f), options, (index, value) =>
                {
                    device.SetClient(index > 0 ? clients[index - 1] : null, true);
                });
            }
        }

        /// <summary>
        /// Draws the actor selection field.
        /// </summary>
        /// <param name="actor">The actor property.</param>
        protected void DoActorGUI(SerializedProperty actor)
        {
            EditorGUILayout.PropertyField(actor, Contents.ActorLabel);
        }

        /// <summary>
        /// Draws the live link channels GUI.
        /// </summary>
        /// <param name="channels">The live link channels property.</param>
        protected void DoChannelsGUI(SerializedProperty channels)
        {
            EditorGUILayout.PropertyField(channels, Contents.ChannelsLabel);
        }

        static TClient[] GetClients()
        {
            if (ConnectionManager.Instance.TryGetConnection<CompanionAppServer>(out var server))
            {
                return server
                    .GetClients()
                    .OfType<TClient>()
                    .ToArray();
            }
            return new TClient[0];
        }
    }
}
