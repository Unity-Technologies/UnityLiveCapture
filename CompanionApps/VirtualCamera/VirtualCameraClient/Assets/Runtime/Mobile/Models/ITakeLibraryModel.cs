using System;
using System.Collections.ObjectModel;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ITakeLibraryModel
    {
        Guid SelectedTake { get; }

        Guid SlateIterationBase { get; }

        ReadOnlyCollection<TakeDescriptor> TakeDescriptors { get; }

        bool TryGetMetadata(Guid takeGuid, out VcamTrackMetadataDescriptor descriptor);

        bool TryGetDescriptor(Guid takeGuid, out TakeDescriptor descriptor);
    }
}
