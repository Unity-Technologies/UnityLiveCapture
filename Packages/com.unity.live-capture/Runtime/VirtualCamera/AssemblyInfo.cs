using System.Runtime.CompilerServices;

// Internal access needed by other runtime assemblies to access utilities
[assembly: InternalsVisibleTo("Unity.CompanionAppCommon.Runtime")]
[assembly: InternalsVisibleTo("Unity.VirtualCameraClient.Runtime")]

// Internal access needed for editor scripts
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera.Editor")]

// Internal access needed for testing
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.Urp")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.UrpWithCinemachine")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.HdrpWithCinemachine")]
[assembly: InternalsVisibleTo("InternalsVisible.ToDynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Unity.VirtualCameraClient.Tests.Runtime")]
