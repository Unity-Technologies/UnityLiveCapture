namespace Unity.CompanionAppCommon
{
    sealed class PermissionsSignals
    {
        public sealed class OpenSystemSettings {}

        public class Request
        {
            public virtual string Message { get; }
            public virtual bool Cancelable { get; }
        }

        public class MicrophoneRequest : Request
        {
            const string k_MessageFormat =
                "Microphone permission is needed to save audio. Allow access in Settings > {0} > Microphone";
            string m_AppName;

            public override string Message => string.Format(k_MessageFormat, m_AppName);
            public override bool Cancelable => true;

            public MicrophoneRequest(string appName)
            {
                m_AppName = appName;
            }
        }

        public class CameraRequest : Request
        {
            const string k_MessageFormat =
                "Camera permission is needed for \"{0}\" to work. Allow access in Settings > {0} > Camera";
            string m_AppName;

            public override string Message => string.Format(k_MessageFormat, m_AppName);
            public override bool Cancelable => false;

            public CameraRequest(string appName)
            {
                m_AppName = appName;
            }
        }

        public class CameraGranted {}
    }
}
