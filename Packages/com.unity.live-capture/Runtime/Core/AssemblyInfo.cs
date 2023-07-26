using System.Runtime.CompilerServices;

// Internal access needed by other runtime assemblies to access utilities
[assembly: InternalsVisibleTo("Unity.LiveCapture.Cameras")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.CompanionApp")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.ARKitFaceCapture")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tentacle")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Ltc")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Ntp")]
[assembly: InternalsVisibleTo("Unity.VirtualCameraClient.Runtime")]
[assembly: InternalsVisibleTo("Unity.FaceCaptureClient.Runtime")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Mocap")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.TransformCapture")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Stype")]

// Internal access needed for editor scripts
[assembly: InternalsVisibleTo("Unity.LiveCapture.Editor")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.CompanionApp.Editor")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.ARKitFaceCapture.Editor")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera.Editor")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Mocap.Editor")]

// Internal access needed for testing
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.Urp")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.UrpWithCinemachine")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor.Pipelines.HdrpWithCinemachine")]
[assembly: InternalsVisibleTo("InternalsVisible.ToDynamicProxyGenAssembly2")]
