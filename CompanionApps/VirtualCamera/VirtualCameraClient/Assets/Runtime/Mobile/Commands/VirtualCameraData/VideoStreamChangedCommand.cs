using Unity.LiveCapture.VirtualCamera;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class VideoStreamChangedCommand
    {
        VideoStreamState m_VideoStreamState;

        [Inject]
        VideoStreamingSystem m_VideoStreamSystem;

        public void Execute(VideoStreamChangedSignal videoStreamChangedSignal)
        {
            var data = videoStreamChangedSignal.value;
            if (data.IsRunning)
            {
                if (!m_VideoStreamState.IsRunning)
                {
                    m_VideoStreamSystem.StartVideoStream(data.Port);
                }
                else if (m_VideoStreamState.Port != data.Port)
                {
                    m_VideoStreamSystem.StopVideoStream();
                    m_VideoStreamSystem.StartVideoStream(data.Port);
                }
            }
            else if (m_VideoStreamState.IsRunning)
            {
                m_VideoStreamSystem.StopVideoStream();
            }

            m_VideoStreamState = data;
        }
    }
}
