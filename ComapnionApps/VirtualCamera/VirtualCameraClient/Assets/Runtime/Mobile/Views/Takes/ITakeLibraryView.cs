using System;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ITakeLibraryView : ITakeGalleryView
    {
        event Action<Guid, bool> TakeRatingChanged;
        event Action<Guid> EditTakeClicked;
        event Action<Guid> DisplayTakeMetadataClicked;
        event Action<Guid> DeleteTakeClicked;
        event Action CloseClicked;
    }
}
