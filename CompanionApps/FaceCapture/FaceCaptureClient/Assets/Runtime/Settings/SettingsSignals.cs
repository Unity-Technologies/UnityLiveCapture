namespace Unity.CompanionApps.FaceCapture
{
    sealed class SettingsSignals
    {
        public sealed class Reset {}

        public sealed class UpdateProperty
        {
            public static UpdateProperty CreateBool(SettingsProperty property, bool value)
            {
                return new UpdateProperty(
                    new BooleanSettingsPropertyUpdater(property, value)
                );
            }

            public static UpdateProperty CreateInt(SettingsProperty property, int value)
            {
                return new UpdateProperty(
                    new IntSettingsPropertyUpdater(property, value)
                );
            }

            public static UpdateProperty CreateString(SettingsProperty property, string value)
            {
                return new UpdateProperty(
                    new StringSettingsPropertyUpdater(property, value)
                );
            }

            public static UpdateProperty CreateCalibrationPose(SettingsProperty property, CalibrationPose? value)
            {
                return new UpdateProperty(
                    new CalibrationPoseSettingsPropertyUpdater(property, value)
                );
            }

            ISettingsPropertyUpdater m_Updater;

            public ISettingsPropertyUpdater SettingsUpdater => m_Updater;

            UpdateProperty(ISettingsPropertyUpdater updater)
            {
                m_Updater = updater;
            }
        }
    }
}
