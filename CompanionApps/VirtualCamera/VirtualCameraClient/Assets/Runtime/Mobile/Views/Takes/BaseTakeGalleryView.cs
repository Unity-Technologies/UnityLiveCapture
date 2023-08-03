using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.CompanionApps.VirtualCamera
{
    public abstract class BaseTakeGalleryView : MonoBehaviour, ITakeGalleryView
    {
        public event Action<Guid> TakeSelected = delegate {};

        [SerializeField]
        GameObject m_Thumbnail;

        [SerializeField]
        RectTransform m_ScrollViewContent;

        protected Guid m_SelectedThumbnail = Guid.Empty;

        protected readonly List<TakeThumbnail> m_Thumbnails = new List<TakeThumbnail>();
        protected readonly Dictionary<Guid, TakeThumbnail> m_TakeGuidToThumbnail = new Dictionary<Guid, TakeThumbnail>();

        readonly Stack<TakeThumbnail> m_ThumbnailPool = new Stack<TakeThumbnail>();

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        protected virtual void OnDestroy()
        {
            // Order is important.
            FlushThumbnails();
            FlushThumbnailPool();
        }

        public abstract void SetThumbnails(List<TakeThumbnailData> thumbnailsData);

        /// <summary>
        /// Assigns a preview to a thumbnail.
        /// </summary>
        /// <param name="takeGuid">The guid of the take/thumbnail.</param>
        /// <param name="preview">The preview.</param>
        public void SetPreview(Guid takeGuid, Texture2D preview)
        {
            if (m_TakeGuidToThumbnail.TryGetValue(takeGuid, out var thumbnail))
            {
                thumbnail.Preview = preview;
            }
        }

        public void SetSelectedThumbnail(Guid guid)
        {
            OnBeforeSelectedThumbnailChange();
            SetSelectedThumbnailInternal(guid);
        }

        public Guid GetSelectedThumbnail() => m_SelectedThumbnail;

        protected virtual void OnBeforeSelectedThumbnailChange()
        {
            if (m_TakeGuidToThumbnail.TryGetValue(m_SelectedThumbnail, out var prevSelectedThumbnail))
            {
                SetThumbnailSelected(prevSelectedThumbnail, false);
            }

            m_SelectedThumbnail = Guid.Empty;
        }

        protected void InvokeTakeSelected(Guid guid) => TakeSelected.Invoke(guid);

        protected void SetSelectedThumbnailInternal(Guid guid)
        {
            if (m_TakeGuidToThumbnail.TryGetValue(guid, out var selectedThumbnail))
            {
                m_SelectedThumbnail = guid;
                SetThumbnailSelected(selectedThumbnail, true);
            }
            else
            {
                m_SelectedThumbnail = Guid.Empty;
            }
        }

        protected virtual void SetThumbnailSelected(TakeThumbnail thumbnail, bool value)
        {
            thumbnail.ShowFocusOutline = value;
        }

        TakeThumbnail GetThumbnail()
        {
            var instance = default(TakeThumbnail);

            if (m_ThumbnailPool.Count > 0)
            {
                instance = m_ThumbnailPool.Pop();
            }
            else
            {
                Assert.IsNotNull(m_ScrollViewContent);
                instance = Instantiate(m_Thumbnail, m_ScrollViewContent).GetComponent<TakeThumbnail>();
                Assert.IsTrue(instance.transform.parent == m_ScrollViewContent);
            }

            instance.transform.localScale = Vector3.one;

            return instance;
        }

        protected TakeThumbnail GetThumbnail(TakeThumbnailData data)
        {
            var thumbnail = GetThumbnail();

            thumbnail.ShowStarBadge = data.isStarred;
            thumbnail.StarBadgeIsInteractable = false;
            thumbnail.ShowFocusOutline = false;
            thumbnail.ShowOptionsMenu = false;
            thumbnail.ShowRecordBadge = false;
            thumbnail.ShowOptionsButton = false;

            thumbnail.Guid = data.guid;
            thumbnail.Name = data.name;
            thumbnail.Metadata = data.metadata;

            BindThumbnail(thumbnail);

            return thumbnail;
        }

        protected void ReleaseThumbnail(TakeThumbnail value)
        {
            UnbindThumbnail(value);

            value.Preview = null;
            value.gameObject.SetActive(false);
            //value.transform.SetParent(null); <- Creates Errors on dispose

            m_ThumbnailPool.Push(value);
        }

        protected abstract void BindThumbnail(TakeThumbnail value);

        protected abstract void UnbindThumbnail(TakeThumbnail value);

        protected virtual void FlushThumbnails()
        {
            m_TakeGuidToThumbnail.Clear();

            foreach (var value in m_Thumbnails)
            {
                if (value != null)
                {
                    ReleaseThumbnail(value);
                }
            }

            m_Thumbnails.Clear();
        }

        void FlushThumbnailPool()
        {
            while (m_ThumbnailPool.Count > 0)
            {
                Destroy(m_ThumbnailPool.Pop().gameObject);
            }
        }
    }
}
