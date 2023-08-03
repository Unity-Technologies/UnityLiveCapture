using System;
using System.Collections.Generic;
using Unity.CompanionAppCommon;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    public struct TakeThumbnailData
    {
        public Guid guid;
        public string name;
        public string metadata;
        public bool isStarred;
    }

    public interface ITakeGalleryView : IPresentable
    {
        event Action<Guid> TakeSelected;
        void SetSelectedThumbnail(Guid guid);
        Guid GetSelectedThumbnail();
        void SetThumbnails(List<TakeThumbnailData> thumbnailsData);
        void SetPreview(Guid takeGuid, Texture2D preview);
    }
}
