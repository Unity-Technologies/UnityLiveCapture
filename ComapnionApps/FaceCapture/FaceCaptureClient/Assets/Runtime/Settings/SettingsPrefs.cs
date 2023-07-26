using System;
using UnityEngine;

namespace Unity.CompanionApps.FaceCapture
{
    static class SettingsPrefs
    {
        const string k_FlipHorizontally = "settings-flip-horizontally";
        const string k_FaceWireframe = "settings-face-wireframe-show";
        const string k_DimScreen = "settings-dim-screen-enabled";
        const string k_RecordVideo = "settings-record-video";
        const string k_RecordAudio = "settings-record-audio";
        const string k_CountdownEnabled = "settings-countdown-enabled";
        const string k_CountdownTime = "settings-countdown-time";
        const string k_ShowRecordButton = "settings-record-button-show";
        const string k_CameraPassthrough = "settings-camera-passthrough-enabled";
        const string k_AutoHideUI = "settings-auto-hide-ui-enabled";
        const string k_CalibrationPose = "calibration-pose";

        static readonly Settings s_Default = new Settings()
        {
            FlipHorizontally = false,
            FaceWireframe = false,
            DimScreen = true,
            RecordVideo = false,
            RecordAudio = false,
            CountdownEnabled = true,
            CountdownTime = 3,
            ShowRecordButton = true,
            CameraPassthrough = true,
            AutoHideUI = false,
            CalibrationPose = null
        };

        public static void Load(this Settings settings)
        {
            settings.FlipHorizontally = PlayerPrefs.GetInt(k_FlipHorizontally, ToInt(s_Default.FlipHorizontally)) == 1;
            settings.FaceWireframe = PlayerPrefs.GetInt(k_FaceWireframe, ToInt(s_Default.FaceWireframe)) == 1;
            settings.DimScreen = PlayerPrefs.GetInt(k_DimScreen, ToInt(s_Default.DimScreen)) == 1;
            settings.RecordVideo = PlayerPrefs.GetInt(k_RecordVideo, ToInt(s_Default.RecordVideo)) == 1;
            settings.RecordAudio = PlayerPrefs.GetInt(k_RecordAudio, ToInt(s_Default.RecordAudio)) == 1;
            settings.CountdownEnabled = PlayerPrefs.GetInt(k_CountdownEnabled, ToInt(s_Default.CountdownEnabled)) == 1;
            settings.CountdownTime = PlayerPrefs.GetInt(k_CountdownTime, s_Default.CountdownTime);
            settings.ShowRecordButton = PlayerPrefs.GetInt(k_ShowRecordButton, ToInt(s_Default.ShowRecordButton)) == 1;
            settings.CameraPassthrough = PlayerPrefs.GetInt(k_CameraPassthrough, ToInt(s_Default.CameraPassthrough)) == 1;
            settings.AutoHideUI = PlayerPrefs.GetInt(k_AutoHideUI, ToInt(s_Default.AutoHideUI)) == 1;

            var poseString = PlayerPrefs.GetString(k_CalibrationPose, string.Empty);
            var hasPose = poseString != string.Empty;
            if (hasPose)
            {
                try
                {
                    settings.CalibrationPose = JsonUtility.FromJson<CalibrationPose>(poseString);
                }
                catch (ArgumentException e)
                {
                    settings.CalibrationPose = null;
                    Debug.LogWarning($"CalibrationPose JSON error: {e}");
                }
            }
            else
            {
                settings.CalibrationPose = null;
            }
        }

        public static void Save(this Settings settings)
        {
            PlayerPrefs.SetInt(k_FlipHorizontally, ToInt(settings.FlipHorizontally));
            PlayerPrefs.SetInt(k_FaceWireframe, ToInt(settings.FaceWireframe));
            PlayerPrefs.SetInt(k_DimScreen, ToInt(settings.DimScreen));
            PlayerPrefs.SetInt(k_RecordVideo, ToInt(settings.RecordVideo));
            PlayerPrefs.SetInt(k_RecordAudio, ToInt(settings.RecordAudio ));
            PlayerPrefs.SetInt(k_CountdownEnabled, ToInt(settings.CountdownEnabled));
            PlayerPrefs.SetInt(k_CountdownTime, settings.CountdownTime);
            PlayerPrefs.SetInt(k_ShowRecordButton, ToInt(settings.ShowRecordButton));
            PlayerPrefs.SetInt(k_CameraPassthrough, ToInt(settings.CameraPassthrough ));
            PlayerPrefs.SetInt(k_AutoHideUI, ToInt(settings.AutoHideUI));

            var hasPose = settings.CalibrationPose.HasValue;
            PlayerPrefs.SetString(k_CalibrationPose, hasPose
                ? JsonUtility.ToJson(settings.CalibrationPose.Value)
                : string.Empty);

            PlayerPrefs.Save();
        }

        public static void RestoreDefaults(this Settings settings)
        {
            // Don't reset the calibration pose
            var backupPose = settings.CalibrationPose;

            s_Default.Save();
            settings.Load();

            settings.CalibrationPose = backupPose;
        }

        static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }
    }
}
