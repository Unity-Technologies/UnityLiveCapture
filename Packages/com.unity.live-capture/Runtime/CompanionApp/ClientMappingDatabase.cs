using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// A class that keeps track of which devices any connected clients are currently assigned to.
    /// </summary>
    class ClientMappingDatabase : ScriptableObject
    {
        const string k_AssetPath = "UserSettings/LiveCapture/ClientMappingDatabase.asset";

        static ClientMappingDatabase s_Instance;
        static readonly Dictionary<ICompanionAppClient, ICompanionAppDevice> s_ClientToDevice = new Dictionary<ICompanionAppClient, ICompanionAppDevice>();

        static ClientMappingDatabase Instance
        {
            get
            {
                if (s_Instance == null)
                    CreateAndLoad();
                return s_Instance;
            }
        }

        [Serializable]
        struct Mapping : ISerializationCallbackReceiver
        {
            public string ClientName;
            public DateTime LastConnectTime;

            [SerializeField]
            long m_LastConnectTime;

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
                m_LastConnectTime = LastConnectTime.Ticks;
            }

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                LastConnectTime = new DateTime(m_LastConnectTime);
            }
        }

        [Serializable]
        class SceneMappings
        {
            public SerializableDictionary<string, Mapping> DevicePathToMapping;
        }

        [SerializeField]
        SerializableDictionary<string, SceneMappings> m_ScenePathToMappings = new SerializableDictionary<string, SceneMappings>();

        ClientMappingDatabase()
        {
            if (s_Instance != null)
            {
                Debug.LogError($"{nameof(ClientMappingDatabase)} already exists. Did you query the singleton in a constructor?");
            }
            else
            {
                s_Instance = this;
            }
        }

        /// <summary>
        /// Adds a device mapping for a client.
        /// </summary>
        /// <param name="device">The device the client is assigned to.</param>
        /// <param name="client">The client that is assigned to the device.</param>
        /// <param name="rememberAssignment">Try to auto-assign the client to this device when it reconnects in the future.</param>
        /// <typeparam name="TClient">The client type.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="device"/> is null.</exception>
        public static void RegisterClientAssociation<TClient>(CompanionAppDevice<TClient> device, ICompanionAppClient client, bool rememberAssignment)
            where TClient : class, ICompanionAppClient
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            s_ClientToDevice[client] = device;

            if (rememberAssignment)
            {
                var scenePath = device.gameObject.scene.path;

                if (!Instance.m_ScenePathToMappings.TryGetValue(scenePath, out var sceneMappings))
                {
                    sceneMappings = new SceneMappings
                    {
                        DevicePathToMapping = new SerializableDictionary<string, Mapping>()
                    };
                    Instance.m_ScenePathToMappings.Add(scenePath, sceneMappings);
                }

                var devicePath = GetGameObjectPath(device.gameObject);

                sceneMappings.DevicePathToMapping[devicePath] = new Mapping
                {
                    ClientName = client.Name,
                    LastConnectTime = DateTime.Now,
                };

                Save();
            }
        }

        /// <summary>
        /// Clears the device mapping for a client.
        /// </summary>
        /// <param name="device">The device to remove the mapping for.</param>
        /// <param name="client">The client to remove the mapping for.</param>
        /// <param name="rememberAssignment">Do not auto-assign the client to this device when it reconnects in the future.</param>
        /// <typeparam name="TClient">The client type.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown if or <paramref name="device"/> is null.</exception>
        public static void DeregisterClientAssociation<TClient>(CompanionAppDevice<TClient> device, ICompanionAppClient client, bool rememberAssignment)
            where TClient : class, ICompanionAppClient
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            s_ClientToDevice.Remove(client);

            if (rememberAssignment)
            {
                var scenePath = device.gameObject.scene.path;

                if (Instance.m_ScenePathToMappings.TryGetValue(scenePath, out var sceneMappings))
                {
                    var devicePath = GetGameObjectPath(device.gameObject);

                    var dirty = sceneMappings.DevicePathToMapping.Remove(devicePath);

                    if (sceneMappings.DevicePathToMapping.Count == 0)
                    {
                        Instance.m_ScenePathToMappings.Remove(scenePath);
                    }

                    if (dirty)
                    {
                        Save();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the device that currently has the given client assigned.
        /// </summary>
        /// <param name="client">The client to get the device for.</param>
        /// <param name="device">The returned device, or <see langword="default"/> if there is no device using the client.</param>
        /// <returns><see langword="true"/> if there is a device using the client; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
        public static bool TryGetDevice(ICompanionAppClient client, out ICompanionAppDevice device)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            return s_ClientToDevice.TryGetValue(client, out device);
        }

        /// <summary>
        /// Gets the client that is associated with a device.
        /// </summary>
        /// <param name="device">The device to get the assigned client for.</param>
        /// <param name="clientName">The name of the associated client.</param>
        /// <param name="time">The time at which the client was assigned to this device.</param>
        /// <typeparam name="TClient">The client type.</typeparam>
        /// <returns><see langword="true"/> if there is a client associated with the device; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="device"/> is null.</exception>
        public static bool TryGetClientAssignment<TClient>(CompanionAppDevice<TClient> device, out string clientName, out DateTime time)
            where TClient : class, ICompanionAppClient
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var scenePath = device.gameObject.scene.path;

            if (Instance.m_ScenePathToMappings.TryGetValue(scenePath, out var sceneMappings))
            {
                var devicePath = GetGameObjectPath(device.gameObject);

                if (sceneMappings.DevicePathToMapping.TryGetValue(devicePath, out var mapping))
                {
                    clientName = mapping.ClientName;
                    time = mapping.LastConnectTime;
                    return true;
                }
            }

            clientName = default;
            time = default;
            return false;
        }

        static void CreateAndLoad()
        {
#if UNITY_EDITOR
            InternalEditorUtility.LoadSerializedFileAndForget(k_AssetPath);
#endif

            if (s_Instance == null)
            {
                CreateInstance<ClientMappingDatabase>().hideFlags = HideFlags.HideAndDontSave;
            }
        }

        static void Save()
        {
#if UNITY_EDITOR
            var directoryName = Path.GetDirectoryName(k_AssetPath);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] {s_Instance}, k_AssetPath, true);
#endif
        }

        static string GetGameObjectPath(GameObject go)
        {
            return string.Join("/", go.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());
        }
    }
}
