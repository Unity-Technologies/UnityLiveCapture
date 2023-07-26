using System.Runtime.CompilerServices;

// Internal access needed by other runtime assemblies to access utilities
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera")]

// Internal access needed for editor scripts
[assembly: InternalsVisibleTo("Unity.LiveCapture.Cameras.Editor")]

// Internal access needed for testing
[assembly: InternalsVisibleTo("InternalsVisible.ToDynamicProxyGenAssembly2")]
