using System;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ITakeSelectionView : ITakeGalleryView
    {
        event Action CancelClicked;
        event Action DoneClicked;

        bool DoneButtonEnabled { set; }
    }
}
