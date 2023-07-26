using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    public class TakeThumbnail : MonoBehaviour
    {
        public event Action<TakeThumbnail> BackgroundClicked = delegate {};
        public event Action<TakeThumbnail, bool> RatingChanged = delegate {};
        public event Action<TakeThumbnail> EditClicked = delegate {};
        public event Action<TakeThumbnail> MetadataClicked = delegate {};
        public event Action<TakeThumbnail> DeleteClicked = delegate {};
        public event Action<TakeThumbnail> ShowedOptions = delegate {};

        // TODO replace by Button for consistency?
        [SerializeField] SimpleButton m_OptionsButton;
        [SerializeField] RawImage m_RawImage;
        [SerializeField] AspectRatioFitter m_AspectRatioFitter;
        [SerializeField] Toggle m_StarToggle;
        [SerializeField] TextMeshProUGUI m_NameTextField;
        [SerializeField] TextMeshProUGUI m_MetadataTextField;
        [SerializeField] RectTransform m_FocusOutline;
        [SerializeField] RectTransform m_OptionsMenu;
        [SerializeField] RectTransform m_RecordBadge;
        [SerializeField] Button m_EditButton;
        [SerializeField] Button m_MetadataButton;
        [SerializeField] Button m_DeleteButton;
        [SerializeField] Button m_BackgroundButton;

        Guid m_Guid;

        public bool ShowStarBadge
        {
            get => m_StarToggle.isOn;
            set => m_StarToggle.SetIsOnWithoutNotify(value);
        }

        // TODO activation seems to affect toggle view update...
        public bool StarBadgeIsInteractable
        {
            set => m_StarToggle.interactable = value;
        }

        public bool ShowFocusOutline
        {
            set => m_FocusOutline.gameObject.SetActive(value);
        }

        public bool ShowOptionsMenu
        {
            set => m_OptionsMenu.gameObject.SetActive(value);
        }

        public bool ShowOptionsButton
        {
            set => m_OptionsButton.gameObject.SetActive(value);
        }

        public bool ShowRecordBadge
        {
            set => m_RecordBadge.gameObject.SetActive(value);
        }

        public Guid Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        // TODO may need max length check
        public string Name
        {
            set => m_NameTextField.text = value;
        }

        // TODO may need max length check
        public string Metadata
        {
            set => m_MetadataTextField.text = value;
        }

        public Texture Preview
        {
            get => m_RawImage.texture;
            set
            {
                m_RawImage.texture = value;
                m_AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                if (value != null)
                {
                    m_AspectRatioFitter.aspectRatio = value.width / (float)value.height;
                }
            }
        }

        void Awake()
        {
            ShowFocusOutline = false;
            ShowOptionsMenu = false;
        }

        void OnEnable()
        {
            m_StarToggle.onValueChanged.AddListener(OnSelectToggleChanged);
            m_OptionsButton.onClick += OnOptionsClicked;
            m_EditButton.onClick.AddListener(OnRenameClicked);
            m_MetadataButton.onClick.AddListener(OnMetadataClicked);
            m_DeleteButton.onClick.AddListener(OnDeleteClicked);
            m_BackgroundButton.onClick.AddListener(OnBackgroundClicked);
        }

        void OnDisable()
        {
            m_StarToggle.onValueChanged.RemoveListener(OnSelectToggleChanged);
            m_OptionsButton.onClick -= OnOptionsClicked;
            m_EditButton.onClick.RemoveListener(OnRenameClicked);
            m_MetadataButton.onClick.RemoveListener(OnMetadataClicked);
            m_DeleteButton.onClick.RemoveListener(OnDeleteClicked);
            m_BackgroundButton.onClick.RemoveListener(OnBackgroundClicked);
        }

        void OnSelectToggleChanged(bool value)
        {
            RatingChanged.Invoke(this, value);
        }

        void OnOptionsClicked()
        {
            ShowOptionsMenu = true;
            ShowedOptions.Invoke(this);
        }

        void OnRenameClicked() => EditClicked.Invoke(this);

        void OnMetadataClicked() => MetadataClicked.Invoke(this);

        void OnDeleteClicked() => DeleteClicked.Invoke(this);

        void OnBackgroundClicked()
        {
            ShowOptionsMenu = false;
            BackgroundClicked.Invoke(this);
        }
    }
}
