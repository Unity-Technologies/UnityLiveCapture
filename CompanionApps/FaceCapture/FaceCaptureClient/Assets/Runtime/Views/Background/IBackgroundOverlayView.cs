using Unity.CompanionAppCommon;
using UnityEngine;

namespace Unity.CompanionApps.FaceCapture
{
    interface IBackgroundOverlayView : IDialogView
    {
        Texture2D Texture { set; }
        void DisposeTexture();
    }
}
