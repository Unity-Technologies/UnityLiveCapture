using Unity.LiveCapture;

namespace Unity.CompanionApps.VirtualCamera
{
    public interface ITimeSystem
    {
        double Time { get; }

        ITimecodeSource TimecodeSource { get; set; }

        void Reset();
    }
}
