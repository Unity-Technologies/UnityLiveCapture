using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.CompanionApps.VirtualCamera
{
    public class TakeMetadataView : BaseTakeDialog
    {
        public enum Field
        {
            TakeName = 0,
            Framerate = 1,
            Description = 2,
            Date = 3,
            Time = 4,
            Length = 5,
            Timeline = 6,
            FocalLength = 7,
            FocusDistance = 8,
            Aperture = 9,
            SensorSize = 10,
            Iso = 11,
            ShutterSpeed = 12,
            AspectRatio = 13
        }

        const int k_NumFields = 14;

        readonly (Field field, string label)[] k_Labels = new[]
        {
            (Field.TakeName, "Take Name"),
            (Field.Framerate, "Framerate"),
            (Field.Description, "Description"),
            (Field.Date, "Date"),
            (Field.Time, "Time"),
            (Field.Length, "Length"),
            (Field.Timeline, "Timeline"),
            (Field.FocalLength, "Focal Length"),
            (Field.FocusDistance, "Focus Distance"),
            (Field.Aperture, "Aperture"),
            (Field.SensorSize, "Sensor Size"),
            (Field.Iso, "ISO"),
            (Field.ShutterSpeed, "Shutter Speed"),
            (Field.AspectRatio, "Aspect Ratio")
        };

        public event Action doneClicked = delegate {};

        [SerializeField]
        Button m_DoneButton;
        [SerializeField]
        GameObject m_SingleLineEntryPrefab;
        [SerializeField]
        GameObject m_MultiLineEntryPrefab;
        [SerializeField]
        GameObject m_SeparatorPrefab;
        [SerializeField]
        RectTransform m_ScrollViewContent;
        [SerializeField, HideInInspector]
        TextMeshProUGUI[] m_TextFields;


        // Design lends itself to procedural generation.
#if UNITY_EDITOR
        [ContextMenu(nameof(BuildUI))]
        void BuildUI()
        {
            for (var i = m_ScrollViewContent.childCount; i != 0; --i)
            {
                DestroyImmediate(m_ScrollViewContent.GetChild(0).gameObject);
            }

            if (m_TextFields == null || m_TextFields.Length != k_NumFields)
            {
                m_TextFields = new TextMeshProUGUI[k_NumFields];
            }

            void AddSpace(float height)
            {
                var space = new GameObject("Space", typeof(RectTransform), typeof(LayoutElement));
                var trs = space.GetComponent<RectTransform>();
                trs.SetParent(m_ScrollViewContent);
                trs.sizeDelta = new Vector2(100, height); // x will be overriden by layout group.
                var layout = space.GetComponent<LayoutElement>();
                layout.minHeight = height;
                layout.preferredHeight = height;
            }

            foreach (var item in k_Labels)
            {
                var prefab = item.field == Field.Description ? m_MultiLineEntryPrefab : m_SingleLineEntryPrefab;

                var entry = Instantiate(prefab, m_ScrollViewContent);
                var trs = entry.GetComponent<RectTransform>();

                var labelTf = trs.Find("Label").GetComponent<TextMeshProUGUI>();
                labelTf.text = item.label;

                var valueTf = trs.Find("Value").GetComponent<TextMeshProUGUI>();
                m_TextFields[(int)item.field] = valueTf;

                Instantiate(m_SeparatorPrefab, m_ScrollViewContent);

                // Jump after Description and Timeline.
                if (item.field == Field.Description || item.field == Field.Timeline)
                {
                    AddSpace(40);
                }
            }

            AddSpace(40);

            EditorUtility.SetDirty(gameObject);
        }

#endif

        bool IsBuilt()
        {
            if (m_TextFields == null)
            {
                return false;
            }

            if (m_TextFields.Length != k_NumFields)
            {
                return false;
            }

            for (var i = 0; i != k_NumFields; ++i)
            {
                if (m_TextFields[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        void Awake()
        {
            if (!IsBuilt())
            {
                throw new InvalidOperationException(
                    $"{nameof(TakeMetadataView)} is not configured properly," +
                    $" use the BuildUI function to configure it at edit time.");
            }
        }

        private void OnEnable()
        {
            m_DoneButton.onClick.AddListener(OnDoneClicked);
        }

        private void OnDisable()
        {
            m_DoneButton.onClick.RemoveListener(OnDoneClicked);
        }

        void OnDoneClicked()
        {
            doneClicked.Invoke();
        }

        void SetValue(Field field, string value) => m_TextFields[(int)field].text = value;

        public string TakeName
        {
            set => SetValue(Field.TakeName, value);
        }

        public string Framerate
        {
            set => SetValue(Field.Framerate, value);
        }

        public string Description
        {
            set => SetValue(Field.Description, value);
        }

        public string Date
        {
            set => SetValue(Field.Date, value);
        }

        public string Time
        {
            set => SetValue(Field.Time, value);
        }

        public string Length
        {
            set => SetValue(Field.Length, value);
        }

        public string Timeline
        {
            set => SetValue(Field.Timeline, value);
        }

        public string FocalLength
        {
            set => SetValue(Field.FocalLength, value);
        }

        public string FocusDistance
        {
            set => SetValue(Field.FocusDistance, value);
        }

        public string Aperture
        {
            set => SetValue(Field.Aperture, value);
        }

        public string SensorSize
        {
            set => SetValue(Field.SensorSize, value);
        }

        public string Iso
        {
            set => SetValue(Field.Iso, value);
        }

        public string ShutterSpeed
        {
            set => SetValue(Field.ShutterSpeed, value);
        }

        public string AspectRatio
        {
            set => SetValue(Field.AspectRatio, value);
        }
    }
}
