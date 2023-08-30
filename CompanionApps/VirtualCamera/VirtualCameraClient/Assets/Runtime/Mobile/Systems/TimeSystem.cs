using Unity.LiveCapture;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    public class TimeSystem : ITimeSystem, ITickable
    {
        double m_Time;

        public ITimecodeSource TimecodeSource { get; set; }

        public double Time => TimecodeSource?.CurrentTime?.ToSeconds() ?? m_Time;

        public void Reset()
        {
            m_Time = 0f;
        }

        public void Tick()
        {
            m_Time += UnityEngine.Time.deltaTime;
        }
    }
}
