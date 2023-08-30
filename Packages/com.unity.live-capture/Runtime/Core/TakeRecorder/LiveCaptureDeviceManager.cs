using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture
{
    class LiveCaptureDeviceManager
    {
        static readonly string k_IsLiveKey = $"{LiveCaptureInfo.Name}/islive";
        internal static event Action<LiveCaptureDevice> DeviceAdded;
        internal static event Action<LiveCaptureDevice> DeviceRemoved;

        public static LiveCaptureDeviceManager Instance { get; } = new LiveCaptureDeviceManager();

        bool m_NeedsSorting;
        List<LiveCaptureDevice> m_Devices = new List<LiveCaptureDevice>();

        internal List<LiveCaptureDevice> Devices => m_Devices;
        public bool IsLive
        {
#if UNITY_EDITOR
            get => SessionState.GetBool(k_IsLiveKey, true);
            set => SessionState.SetBool(k_IsLiveKey, value);
#else
            get; set;
#endif
        }

        public void Register(LiveCaptureDevice device)
        {
            if (m_Devices.AddUnique(device))
            {
                SetNeedsSorting();
                Invoke(DeviceAdded, device);
            }
        }

        public void Unregister(LiveCaptureDevice device)
        {
            if (m_Devices.Remove(device))
            {
                Invoke(DeviceRemoved, device);
            }
        }

        public void UpdateDevice()
        {
            SortIfNeeded();

            foreach (var device in m_Devices)
            {
                if (device != null && device.isActiveAndEnabled)
                {
                    device.InvokeUpdateDevice();
                }
            }
        }

        public void LiveUpdate()
        {
            if (!IsLive)
            {
                return;
            }

            SortIfNeeded();

            foreach (var device in m_Devices)
            {
                if (device != null
                    && device.isActiveAndEnabled
                    && device.IsLive
                    && device.IsReady())
                {
                    device.InvokeLiveUpdate();
                }
            }
        }

        public void SetNeedsSorting()
        {
            m_NeedsSorting = true;
        }

        void SortIfNeeded()
        {
            if (m_NeedsSorting)
            {
                m_NeedsSorting = false;

                SortDevices();
            }
        }

        void SortDevices()
        {
            m_Devices.Sort((d1, d2) => d1.SortingOrder.CompareTo(d2.SortingOrder));
        }

        static void Invoke<T>(Action<T> action, T obj)
        {
            try
            {
                action?.Invoke(obj);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
