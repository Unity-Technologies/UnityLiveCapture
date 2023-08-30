using System.Runtime.CompilerServices;

// Internal access needed by other runtime assemblies to access utilities
[assembly: InternalsVisibleTo("Unity.CompanionAppCommon.Runtime")]
[assembly: InternalsVisibleTo("Unity.FaceCaptureClient.Runtime")]

// Internal access needed for editor scripts
[assembly: InternalsVisibleTo("Unity.LiveCapture.ARKitFaceCapture.Editor")]

// Internal access needed for testing
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor")]
[assembly: InternalsVisibleTo("InternalsVisible.ToDynamicProxyGenAssembly2")]
