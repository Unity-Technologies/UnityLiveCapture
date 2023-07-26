using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Unity.LiveCapture.Tentacle
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct DeviceState
    {
        public const int k_IdentifierLengthMax = 37;
        public const int k_NameLengthMax = 16;

        public fixed byte name[k_NameLengthMax];
        public byte nameLength;
        public fixed byte identifier[k_IdentifierLengthMax];
        public byte identifierLength;
        [MarshalAs(UnmanagedType.U1)]
        public bool isDisappeared;
        [MarshalAs(UnmanagedType.U1)]
        public bool isUnavailable;
        public byte battery;
        [MarshalAs(UnmanagedType.U1)]
        public bool isCharging;
        public byte rssi;
        public int frameRate;
        [MarshalAs(UnmanagedType.U1)]
        public bool isNtsc;
        [MarshalAs(UnmanagedType.U1)]
        public bool isDropFrame;
        public double seconds;
    }

    /// <summary>
    /// A class that detects and provides access to Tentacle timecode sources.
    /// </summary>
    public static class TentacleManager
    {
        const string k_DLL =
#if UNITY_EDITOR || UNITY_STANDALONE
            "TentaclePlugin"
#elif UNITY_IOS
            "__Internal"
#endif
        ;

        [DllImport(k_DLL)]
        static extern bool IsScanning();
        [DllImport(k_DLL)]
        static extern void StartScanning();
        [DllImport(k_DLL)]
        static extern void StopScanning();
        [DllImport(k_DLL)]
        static extern int GetDeviceCount();
        [DllImport(k_DLL)]
        static extern bool GetDeviceState(int index, out DeviceState state);

        static readonly List<TentacleDevice> m_Devices = new List<TentacleDevice>();
        static readonly HashSet<TentacleDevice> m_UpdatedDevices = new HashSet<TentacleDevice>();

        /// <summary>
        /// The currently available devices.
        /// </summary>
        public static IReadOnlyList<TentacleDevice> Devices = m_Devices;

        /// <summary>
        /// An event invoked when a new device is detected.
        /// </summary>
        public static event Action<TentacleDevice> DeviceAdded;

        /// <summary>
        /// An event invoked when a device becomes no longer available.
        /// </summary>
        public static event Action<TentacleDevice> DeviceRemoved;

        /// <summary>
        /// Are Tentacle devices supported on this platform.
        /// </summary>
        /// <returns><see langword="true"/> if supported, otherwise <see langword="false"/>.</returns>
        public static bool IsSupported()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
            return true;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        static void Init()
        {
            if (!IsSupported())
            {
                return;
            }

            ConfigurePlayerLoop();
            Enable();

#if UNITY_EDITOR
            AssemblyReloadEvents.afterAssemblyReload += ConfigurePlayerLoop;
            EditorApplication.playModeStateChanged += (_) => ConfigurePlayerLoop();
            EditorApplication.quitting += StopScanning;
#else
            Application.quitting += StopScanning;
#endif
        }

        struct TentacleUpdate
        {
        }

        static void ConfigurePlayerLoop()
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();

            // check if the update method is already added
            if (loop.TryFindSubSystem<TentacleUpdate>(out _))
            {
                return;
            }

            if (loop.TryFindSubSystem<PreUpdate>(out var preUpdate))
            {
                preUpdate.AddSubSystem<TentacleUpdate>(preUpdate.subSystemList.Length, UpdateDevices);
                loop.TryUpdate(preUpdate);
            }

            PlayerLoop.SetPlayerLoop(loop);
        }

        /// <summary>
        /// Start listening for Tentacle devices.
        /// </summary>
        public static void Enable()
        {
            if (!IsSupported())
            {
                return;
            }

            StartScanning();
        }

        /// <summary>
        /// Stop listening for Tentacle devices.
        /// </summary>
        public static void Disable()
        {
            if (!IsSupported())
            {
                return;
            }

            StopScanning();

            for (var i = m_Devices.Count - 1; i >= 0; i--)
            {
                RemoveDevice(i);
            }
        }

        static void UpdateDevices()
        {
            if (!IsScanning())
            {
                return;
            }

            m_UpdatedDevices.Clear();

            var deviceCount = GetDeviceCount();
            var state = default(DeviceState);

            for (var i = 0; i < deviceCount; i++)
            {
                if (GetDeviceState(i, out state))
                {
                    var device = UpdateDevice(ref state);
                    m_UpdatedDevices.Add(device);
                }
            }

            // remove devices that are no longer detected
            for (var i = m_Devices.Count - 1; i >= 0; i--)
            {
                var device = m_Devices[i];

                if (!m_UpdatedDevices.Contains(device))
                {
                    RemoveDevice(i);
                }
            }
        }

        static TentacleDevice UpdateDevice(ref DeviceState state)
        {
            // update the device if it already exists
            for (var i = 0; i < m_Devices.Count; i++)
            {
                var device = m_Devices[i];

                if (device.IsDevice(ref state))
                {
                    device.Update(ref state);
                    return device;
                }
            }

            // create a new device if a matching one wasn't found
            var newDevice = new TentacleDevice(ref state);
            m_Devices.Add(newDevice);
            TimecodeSourceManager.Instance.Register(newDevice);
            DeviceAdded?.Invoke(newDevice);
            return newDevice;
        }

        static void RemoveDevice(int index)
        {
            var device = m_Devices[index];
            m_Devices.RemoveAt(index);
            TimecodeSourceManager.Instance.Unregister(device);
            DeviceRemoved?.Invoke(device);
        }
    }
}
