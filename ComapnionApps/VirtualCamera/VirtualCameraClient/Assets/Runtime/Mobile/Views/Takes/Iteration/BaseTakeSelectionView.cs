using System.Collections.Generic;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class BaseTakeSelectionView : BaseTakeGalleryView
    {
        [SerializeField]
        RectTransform m_Empty;
        [SerializeField]
        RectTransform m_ScrollViewViewport;

        void Awake()
        {
            ShowEmptyIfNeeded();
        }

        protected override void BindThumbnail(TakeThumbnail value)
        {
            value.BackgroundClicked += OnThumbnailSelected;
        }

        protected override void UnbindThumbnail(TakeThumbnail value)
        {
            value.BackgroundClicked -= OnThumbnailSelected;
        }

        void OnThumbnailSelected(TakeThumbnail thumbnail) => InvokeTakeSelected(thumbnail.Guid);

        protected override void SetThumbnailSelected(TakeThumbnail thumbnail, bool value)
        {
            thumbnail.ShowFocusOutline = value;
            thumbnail.ShowRecordBadge = value;
        }

        public override void SetThumbnails(List<TakeThumbnailData> thumbnailsData)
        {
            FlushThumbnails();

            for (var i = 0; i != thumbnailsData.Count; ++i)
            {
                var data = thumbnailsData[i];
                var thumbnail = GetThumbnail(data);
                m_TakeGuidToThumbnail.Add(thumbnail.Guid, thumbnail);
                m_Thumbnails.Add(thumbnail);
                thumbnail.gameObject.SetActive(true);
                thumbnail.gameObject.transform.SetSiblingIndex(i);
            }

            // Attempt to restore selected thumbnail.
            SetSelectedThumbnailInternal(m_SelectedThumbnail);

            ShowEmptyIfNeeded();
        }

        void ShowEmptyIfNeeded()
        {
            // Display scroll view if there are available thumbnails.
            m_ScrollViewViewport.gameObject.SetActive(m_Thumbnails.Count != 0);
            // Display empty message if scroll view is empty.
            m_Empty.gameObject.SetActive(m_Thumbnails.Count == 0);
        }
    }
}
