using UnityEngine;

namespace Unity.CompanionApps.FaceCapture
{
    interface ISettingsPropertyUpdater
    {
        SettingsProperty Property { get; }

        void Update(Settings settings);
    }

    interface ISettingsPropertyUpdater<T> : ISettingsPropertyUpdater
    {
        T Value { get; }
    }

    static class SettingsUpdaterExtensions
    {
        public static bool Changes(this ISettingsPropertyUpdater updater, ISettings settings)
        {
            var property = updater.Property;

            switch (property)
            {
                case SettingsProperty.AutoHideUI:
                    return updater.GetValue<bool>() != settings.AutoHideUI;
                case SettingsProperty.CameraPassthrough:
                    return updater.GetValue<bool>() != settings.CameraPassthrough;
                case SettingsProperty.DimScreen:
                    return updater.GetValue<bool>() != settings.DimScreen;
                case SettingsProperty.RecordAudio:
                    return updater.GetValue<bool>() != settings.RecordAudio;
                case SettingsProperty.CountdownEnabled:
                    return updater.GetValue<bool>() != settings.CountdownEnabled;
                case SettingsProperty.RecordVideo:
                    return updater.GetValue<bool>() != settings.RecordVideo;
                case SettingsProperty.FaceWireframe:
                    return updater.GetValue<bool>() != settings.FaceWireframe;
                case SettingsProperty.ShowRecordButton:
                    return updater.GetValue<bool>() != settings.ShowRecordButton;
                case SettingsProperty.FlipHorizontally:
                    return updater.GetValue<bool>() != settings.FlipHorizontally;
                case SettingsProperty.CountdownTime:
                    return updater.GetValue<int>() != settings.CountdownTime;
                case SettingsProperty.TimecodeSourceId:
                    return updater.GetValue<string>() != settings.TimecodeSourceId;
                case SettingsProperty.CalibrationPose:
                    return true;
                default:
                    Debug.LogError($"Settings property {property} is not checking for changes");
                    break;
            }

            return false;
        }

        public static T GetValue<T>(this ISettingsPropertyUpdater updater)
        {
            return updater is ISettingsPropertyUpdater<T> u ? u.Value : default(T);
        }
    }

    class BooleanSettingsPropertyUpdater : ISettingsPropertyUpdater<bool>
    {
        SettingsProperty m_Property;
        bool m_Value;

        public SettingsProperty Property => m_Property;
        public bool Value => m_Value;

        public BooleanSettingsPropertyUpdater(SettingsProperty property, bool value)
        {
            m_Property = property;
            m_Value = value;
        }

        public void Update(Settings settings)
        {
            switch (m_Property)
            {
                case SettingsProperty.AutoHideUI:
                    settings.AutoHideUI = m_Value;
                    break;
                case SettingsProperty.CameraPassthrough:
                    settings.CameraPassthrough = m_Value;
                    break;
                case SettingsProperty.DimScreen:
                    settings.DimScreen = m_Value;
                    break;
                case SettingsProperty.RecordAudio:
                    settings.RecordAudio = m_Value;
                    break;
                case SettingsProperty.CountdownEnabled:
                    settings.CountdownEnabled = m_Value;
                    break;
                case SettingsProperty.RecordVideo:
                    settings.RecordVideo = m_Value;
                    break;
                case SettingsProperty.FaceWireframe:
                    settings.FaceWireframe = m_Value;
                    break;
                case SettingsProperty.ShowRecordButton:
                    settings.ShowRecordButton = m_Value;
                    break;
                case SettingsProperty.FlipHorizontally:
                    settings.FlipHorizontally = m_Value;
                    break;
                default:
                    Debug.LogError($"Settings property {Property} is not a Boolean");
                    break;
            }
        }
    }

    class IntSettingsPropertyUpdater : ISettingsPropertyUpdater<int>
    {
        SettingsProperty m_Property;
        int m_Value;

        public SettingsProperty Property => m_Property;

        public int Value => m_Value;

        public IntSettingsPropertyUpdater(SettingsProperty property, int value)
        {
            m_Property = property;
            m_Value = value;
        }

        public void Update(Settings state)
        {
            switch (m_Property)
            {
                case SettingsProperty.CountdownTime:
                    state.CountdownTime = m_Value;
                    break;
                default:
                    Debug.LogError($"Settings property {Property} is not an Integer");
                    break;
            }
        }
    }

    class StringSettingsPropertyUpdater : ISettingsPropertyUpdater<string>
    {
        SettingsProperty m_Property;
        string m_Value;

        public SettingsProperty Property => m_Property;

        public string Value => m_Value;

        public StringSettingsPropertyUpdater(SettingsProperty property, string value)
        {
            m_Property = property;
            m_Value = value;
        }

        public void Update(Settings state)
        {
            switch (m_Property)
            {
                case SettingsProperty.TimecodeSourceId:
                    state.TimecodeSourceId = m_Value;
                    break;
                default:
                    Debug.LogError($"Settings property {Property} is not a String");
                    break;
            }
        }
    }

    class CalibrationPoseSettingsPropertyUpdater : ISettingsPropertyUpdater<CalibrationPose?>
    {
        SettingsProperty m_Property;
        CalibrationPose? m_Value;

        public SettingsProperty Property => m_Property;

        public CalibrationPose? Value => m_Value;

        public CalibrationPoseSettingsPropertyUpdater(SettingsProperty property, CalibrationPose? value)
        {
            m_Property = property;
            m_Value = value;
        }

        public void Update(Settings state)
        {
            switch (m_Property)
            {
                case SettingsProperty.CalibrationPose:
                    state.CalibrationPose = m_Value;
                    break;
                default:
                    Debug.LogError($"Settings property {Property} is not a CalibrationPose");
                    break;
            }
        }
    }
}
