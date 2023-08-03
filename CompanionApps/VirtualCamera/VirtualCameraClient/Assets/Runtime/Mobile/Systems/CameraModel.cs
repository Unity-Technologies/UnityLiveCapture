using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    class CameraModel
    {
        public Lens Lens { get; set; }
        public LensIntrinsics Intrinsics { get; set; }
        public CameraBody Body { get; set; }
        public LensKitDescriptor LensKit { get; set; }
        public int SelectedLensIndex { get; set; }
    }
}
