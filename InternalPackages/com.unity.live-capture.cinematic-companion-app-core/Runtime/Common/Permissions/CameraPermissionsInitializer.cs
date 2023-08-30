using Zenject;

namespace Unity.CompanionAppCommon
{
    class CameraPermissionsInitializer : IInitializable
    {
        [Inject(Id = "AppName")]
        string m_AppName;

        [Inject]
        ICoroutineRunner m_Runner;

        [Inject]
        IPermissionsView m_PermissionsView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_Runner.StartCoroutine(CameraPermissionsCoroutine());
        }

        System.Collections.IEnumerator CameraPermissionsCoroutine()
        {
            if (IOSHelper.HasVideoPermission())
            {
                m_PermissionsView.Hide();

                m_SignalBus.Fire(new PermissionsSignals.CameraGranted());

                yield break;
            }

            m_SignalBus.Fire(new PermissionsSignals.CameraRequest(m_AppName));

            while (!IOSHelper.HasVideoPermission())
            {
                yield return null;
            }

            m_PermissionsView.Hide();

            m_SignalBus.Fire(new PermissionsSignals.CameraGranted());
        }
    }
}
