using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    /*internal static class CameraLensUtils
    {
        internal static void SendIfNeeded(this IVirtualProductionRemoteSystem virtualProductionRemote, Lens lens, Lens lastLens, double timeStamp)
        {
            var host = virtualProductionRemote.host;

            if (host == null)
                return;

            if (lens.FocalLength != lastLens.FocalLength)
            {
                host.SendFocalLength(new FocalLengthSample
                {
                    Time = timeStamp,
                    FocalLength = lens.FocalLength,
                });
            }

            if (lens.FocusDistance != lastLens.FocusDistance)
            {
                host.SendFocusDistance(new FocusDistanceSample
                {
                    Time = timeStamp,
                    FocusDistance = lens.FocusDistance,
                });
            }

            if (lens.Aperture != lastLens.Aperture)
            {
                host.SendAperture(new ApertureSample
                {
                    Time = timeStamp,
                    Aperture = lens.Aperture,
                });
            }
        }
    }*/
}
