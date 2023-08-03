namespace Unity.CompanionApps.FaceCapture
{
    interface ISettingsPropertyListener
    {
        void SettingsPropertyChanged(SettingsProperty property, ISettings settings);
    }
}
