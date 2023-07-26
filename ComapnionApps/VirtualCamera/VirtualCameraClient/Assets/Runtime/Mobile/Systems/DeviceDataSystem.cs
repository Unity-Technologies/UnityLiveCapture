using System;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    [Flags]
    public enum MainViewOptions : int
    {
        Joysticks = 1 << 0,
        CameraSettings = 1 << 1,
        LensSettings = 1 << 2,
        InformationBar = 1 << 3,
        All = ~0
    };

    public struct DeviceData
    {
        public float ergonomicTilt;
        public MainViewOptions mainViewOptions;
        public bool isCountdownEnabled;
        public int countdownDuration;
    }

    public class DeviceDataSystem
    {
        DeviceData m_DeviceData;

        public DeviceData deviceData
        {
            get => m_DeviceData;
            set => m_DeviceData = value;
        }

        static class Keys
        {
            private const string k_Prefix = "vcam_";

            public const string ErgonomicTilt = k_Prefix + "ErgonomicTilt";
            public const string MainViewOptions = k_Prefix + "MainViewOptions";
            public const string IsCountdownEnabled = k_Prefix + "IsCountdownEnabled";
            public const string CountdownDuration = k_Prefix + "CountdownDuration";
        }

        public void SaveData()
        {
            PlayerPrefs.SetFloat(Keys.ErgonomicTilt, m_DeviceData.ergonomicTilt);
            PlayerPrefs.SetInt(Keys.MainViewOptions, (int)m_DeviceData.mainViewOptions);
            PlayerPrefs.SetFloat(Keys.IsCountdownEnabled, m_DeviceData.isCountdownEnabled ? 1 : 0);
            PlayerPrefs.SetInt(Keys.CountdownDuration, m_DeviceData.countdownDuration);

            PlayerPrefs.Save();
        }

        public void LoadData()
        {
            m_DeviceData.ergonomicTilt = PlayerPrefs.GetFloat(Keys.ErgonomicTilt, 0);
            m_DeviceData.mainViewOptions = (MainViewOptions)PlayerPrefs.GetInt(Keys.MainViewOptions, (int)MainViewOptions.All);
            m_DeviceData.isCountdownEnabled = PlayerPrefs.GetInt(Keys.IsCountdownEnabled, 1) == 1;
            m_DeviceData.countdownDuration = PlayerPrefs.GetInt(Keys.CountdownDuration, 3);
        }
    }
}
