using System.Runtime.CompilerServices;

// Only the virtual camera should have access to this code, we
// don't want to make it public to users.
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera")]
[assembly: InternalsVisibleTo("Unity.LiveCapture.VirtualCamera.Editor")]

// Internal access needed for testing
[assembly: InternalsVisibleTo("Unity.LiveCapture.Tests.Editor")]
