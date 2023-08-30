using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class TogglePlaybackCommand
    {
        [Inject]
        ICompanionAppHost m_CompanionApp;

        public void Execute(TogglePlaybackSignal togglePlaybackSignal)
        {
            if (m_CompanionApp.SlateIsPlaying)
            {
                m_CompanionApp.PausePlayback();
            }
            else
            {
                m_CompanionApp.StartPlayback();
            }
        }
    }
}
