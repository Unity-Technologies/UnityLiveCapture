using System.Runtime.CompilerServices;

// Internal access needed by other runtime assemblies to access utilities
[assembly: InternalsVisibleTo("Unity.LiveCapture.ARKitFaceCapture")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera")]
[assembly: InternalsVisibleTo("Unity.CompanionAppCommon.Runtime")]
[assembly: InternalsVisibleTo("Unity.VirtualCameraClient.Runtime")]
[assembly: InternalsVisibleTo("Unity.FaceCaptureClient.Runtime")]

// Internal access needed for editor scripts
[assembly: InternalsVisibleTo("Unity.LiveCapture.CompanionApp.Editor")]

// Internal access needed for testing
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor")]
[assembly: InternalsVisibleTo("InternalsVisible.ToDynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Unity.VirtualCameraClient.Tests.Runtime")]
