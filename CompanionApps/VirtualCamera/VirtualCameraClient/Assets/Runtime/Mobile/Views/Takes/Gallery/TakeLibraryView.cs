using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    [RequireComponent(typeof(ToggleGroup))]
    public class TakeLibraryView : BaseTakeGalleryView, ITakeLibraryView
    {
        public event Action<Guid, bool> TakeRatingChanged = delegate {};
        public event Action<Guid> EditTakeClicked = delegate {};
        public event Action<Guid> DisplayTakeMetadataClicked = delegate {};
        public event Action<Guid> DeleteTakeClicked = delegate {};
        public event Action CloseClicked = delegate {};

        [SerializeField]
        RectTransform m_Empty;
        [SerializeField]
        RectTransform m_EmptyStarred;
        [SerializeField]
        RectTransform m_ScrollViewViewport;

        [SerializeField]
        Toggle m_DefaultMenuToggle;
        [SerializeField]
        Toggle m_StarredMenuToggle;
        [SerializeField]
        Button m_CloseButton;

        bool m_DisplayStarredTakes;
        Guid m_OpenedOptionsThumbnail = Guid.Empty;

        readonly List<TakeThumbnail> m_StarredThumbnails = new List<TakeThumbnail>();

        void Awake()
        {
            // ToggleGroup requires proper toggle initialization.
            m_DefaultMenuToggle.SetIsOnWithoutNotify(!m_DisplayStarredTakes);
            m_StarredMenuToggle.SetIsOnWithoutNotify(m_DisplayStarredTakes);

            var toggleGroup = GetComponent<ToggleGroup>();
            m_DefaultMenuToggle.group = toggleGroup;
            m_StarredMenuToggle.group = toggleGroup;
        }

        void OnEnable()
        {
            m_DefaultMenuToggle.onValueChanged.AddListener(OnDefaultMenuToggle);
            m_StarredMenuToggle.onValueChanged.AddListener(OnStarredMenuToggle);

            // Close button is optional.
            if (m_CloseButton != null)
            {
                m_CloseButton.onClick.AddListener(OnCloseClicked);
            }

            CloseOptionsIfNeeded();
            UpdateScrollView();
        }

        void OnDisable()
        {
            if (m_CloseButton != null)
            {
                m_CloseButton.onClick.RemoveListener(OnCloseClicked);
            }

            m_DefaultMenuToggle.onValueChanged.RemoveListener(OnDefaultMenuToggle);
            m_StarredMenuToggle.onValueChanged.RemoveListener(OnStarredMenuToggle);
        }

        void OnCloseClicked() => CloseClicked.Invoke();

        protected override void OnBeforeSelectedThumbnailChange()
        {
            CloseOptionsIfNeeded();
            base.OnBeforeSelectedThumbnailChange();
        }

        /// <summary>
        /// Assigns the collection of takes to be displayed in the gallery.
        /// </summary>
        /// <param name="thumbnailsData">Takes data used to populate the take views.</param>
        public override void SetThumbnails(List<TakeThumbnailData> thumbnailsData)
        {
            FlushThumbnails();

            for (var i = 0; i != thumbnailsData.Count; ++i)
            {
                var data = thumbnailsData[i];
                var thumbnail = GetThumbnail(data);
                m_TakeGuidToThumbnail.Add(thumbnail.Guid, thumbnail);
                GetList(false).Add(thumbnail);

                if (data.isStarred)
                {
                    GetList(true).Add(thumbnail);
                }
            }

            // Attempt to restore selected thumbnail.
            SetSelectedThumbnailInternal(m_SelectedThumbnail);

            UpdateScrollView();
        }

        protected override void BindThumbnail(TakeThumbnail value)
        {
            value.ShowOptionsButton = true;
            value.StarBadgeIsInteractable = true;

            value.RatingChanged += OnThumbnailRatingChanged;
            value.EditClicked += OnThumbnailEdition;
            value.MetadataClicked += OnThumbnailMetadata;
            value.DeleteClicked += OnThumbnailDelete;
            value.BackgroundClicked += OnThumbnailSelected;
            value.ShowedOptions += OnThumbnailShowedOptions;
        }

        protected override void UnbindThumbnail(TakeThumbnail value)
        {
            value.RatingChanged -= OnThumbnailRatingChanged;
            value.EditClicked -= OnThumbnailEdition;
            value.MetadataClicked -= OnThumbnailMetadata;
            value.DeleteClicked -= OnThumbnailDelete;
            value.BackgroundClicked -= OnThumbnailSelected;
            value.ShowedOptions -= OnThumbnailShowedOptions;
        }

        protected override void FlushThumbnails()
        {
            // Any item in m_StarredThumbnails also is in m_Thumbnails.
            m_StarredThumbnails.Clear();
            base.FlushThumbnails();
        }

        List<TakeThumbnail> GetList(bool selected) => selected ? m_StarredThumbnails : m_Thumbnails;

        RectTransform GetEmpty(bool selected) => selected ? m_EmptyStarred : m_Empty;

        void CloseOptionsIfNeeded()
        {
            if (m_TakeGuidToThumbnail.TryGetValue(m_OpenedOptionsThumbnail, out var prevOpenedOptionsThumbnail))
            {
                prevOpenedOptionsThumbnail.ShowOptionsMenu = false;
            }

            m_OpenedOptionsThumbnail = Guid.Empty;
        }

        void OnThumbnailShowedOptions(TakeThumbnail thumbnail)
        {
            if (m_OpenedOptionsThumbnail == thumbnail.Guid)
            {
                return;
            }

            // At most one thumbnail with options opened.
            if (m_TakeGuidToThumbnail.TryGetValue(m_OpenedOptionsThumbnail, out var prevOpenedOptionsThumbnail))
            {
                prevOpenedOptionsThumbnail.ShowOptionsMenu = false;
            }

            m_OpenedOptionsThumbnail = thumbnail.Guid;
        }

        void OnThumbnailRatingChanged(TakeThumbnail thumbnail, bool value)
        {
            TakeRatingChanged.Invoke(thumbnail.Guid, value);
        }

        void UpdateScrollView()
        {
            // Hide empty message not matching selected state.
            GetEmpty(!m_DisplayStarredTakes).gameObject.SetActive(false);

            var index = 0;

            foreach (var item in m_Thumbnails)
            {
                item.gameObject.SetActive(!m_DisplayStarredTakes || item.ShowStarBadge);
                item.gameObject.transform.SetSiblingIndex(index++);
            }

            ShowEmptyIfNeeded();
        }

        void ShowEmptyIfNeeded()
        {
            var list = GetList(m_DisplayStarredTakes);
            // Display scroll view if there are available thumbnails.
            m_ScrollViewViewport.gameObject.SetActive(list.Count != 0);
            // Display empty message if scroll view is empty.
            GetEmpty(m_DisplayStarredTakes).gameObject.SetActive(list.Count == 0);
        }

        void OnDefaultMenuToggle(bool value)
        {
            if (value)
            {
                m_DisplayStarredTakes = false;
                UpdateScrollView();
            }
        }

        void OnStarredMenuToggle(bool value)
        {
            if (value)
            {
                m_DisplayStarredTakes = true;
                UpdateScrollView();
            }
        }

        void OnThumbnailEdition(TakeThumbnail thumbnail)
        {
            thumbnail.ShowOptionsMenu = false;
            EditTakeClicked.Invoke(thumbnail.Guid);
        }

        void OnThumbnailMetadata(TakeThumbnail thumbnail)
        {
            thumbnail.ShowOptionsMenu = false;
            DisplayTakeMetadataClicked.Invoke(thumbnail.Guid);
        }

        void OnThumbnailDelete(TakeThumbnail thumbnail)
        {
            thumbnail.ShowOptionsMenu = false;
            DeleteTakeClicked.Invoke(thumbnail.Guid);
        }

        void OnThumbnailSelected(TakeThumbnail thumbnail) => InvokeTakeSelected(thumbnail.Guid);
    }
}
