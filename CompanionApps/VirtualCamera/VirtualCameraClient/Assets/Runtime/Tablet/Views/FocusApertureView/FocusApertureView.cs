using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusDistanceLabelProvider : DialControl.ILabelProvider
    {
        float m_CloseFocusDistance = 1;
        bool m_IsDirty;

        public float closeFocus
        {
            get => m_CloseFocusDistance;
            set
            {
                if (value != m_CloseFocusDistance)
                {
                    m_CloseFocusDistance = value;
                    m_IsDirty = true;
                }
            }
        }

        public string GetLabel(float value)
        {
            return FocusDistanceUtility.AsString(value, m_CloseFocusDistance);
        }

        public string GetSelectedLabel(float value)
        {
            return FocusDistanceUtility.AsString(value, m_CloseFocusDistance, "m");
        }

        public bool isDirty => m_IsDirty;

        public void MarkClean()
        {
            m_IsDirty = false;
        }
    }

    class ApertureLabelProvider : DialControl.ILabelProvider
    {
        public string GetLabel(float value) => value.ToString("F2");

        public string GetSelectedLabel(float value) => "f/" + GetLabel(value);

        public bool isDirty => false;

        public void MarkClean() {}
    }

    class FocusDistanceGraduations : DefaultGraduation
    {
        public override Material Update(GraduationParameters parms, IScaler scaler, Vector2 range, List<float> entries)
        {
            // Graduations do not go up to the last entry which is infinity,
            // Otherwise all graduations would be concentrated around this value.
            Assert.IsTrue(entries.Count > 2);
            var subRange = new Vector2(range.x, entries[entries.Count - 2]);
            return base.Update(parms, scaler, subRange, entries);
        }
    }

    interface IFocusDistanceView : IDialogView,
        IFocusDistanceListener,
        IFocusDistanceOffsetListener,
        ILensIntrinsicsListener
    {
        event Action<float> FocusDistanceValueChanged;
        event Action<float> FocusOffsetValueChanged;
        event Action Closed;

        float FocusDistance { get; set; }
        float FocusOffset { get; set; }
    }

    interface IApertureView : IDialogView,
        IApertureListener,
        ILensIntrinsicsListener
    {
        event Action<float> ApertureValueChanged;

        float Aperture { get; set; }
    }

    class FocusApertureView : DialogView,
        IFocusDistanceView,
        IApertureView,
        IFocusModeListener
    {
        static readonly float[] k_FNumberStops = { 1f, 1.4f, 2f, 2.8f, 4f, 5.6f, 8f, 11f, 16f, 22f };

        [SerializeField]
        DialControl m_FocusDialControl;
        [SerializeField]
        DialControl m_FocusOffsetDialControl;
        [SerializeField]
        DialControl m_ApertureDialControl;
        [SerializeField]
        Toggle m_Toggle;
        [SerializeField]
        Toggle m_ApertureToggle;
        [SerializeField]
        Image m_ApretureToggleIcon;
        [SerializeField]
        Sprite m_ApertureDialOpenSprite;
        [SerializeField]
        Sprite m_ApertureDialClosedSprite;
        float m_CachedCloseFocusDistance = -1;
        Vector2 m_CachedApertureRange = Vector2.one * -1;

        readonly FocusDistanceScaler m_FocusDistanceScaler = new FocusDistanceScaler();
        readonly FocusDistanceLabelProvider m_FocusDistanceLabelProvider = new FocusDistanceLabelProvider();
        readonly DialControl.ILabelProvider m_ApertureLabelProvider = new ApertureLabelProvider();
        readonly DialControl.ILabelProvider m_FocusDistanceOffsetLabelProvider = new DialControl.FloatWithUnitLabelProvider("m");

        public event Action<float> FocusDistanceValueChanged = delegate {};
        public event Action<float> FocusOffsetValueChanged = delegate {};
        public event Action<float> ApertureValueChanged = delegate {};
        public event Action Closed = delegate {};

        public float FocusDistance
        {
            get => m_FocusDialControl.selectedValue;
            set => m_FocusDialControl.selectedValue = value;
        }

        public float FocusOffset
        {
            get => m_FocusOffsetDialControl.selectedValue;
            set => m_FocusOffsetDialControl.selectedValue = value;
        }

        public float Aperture
        {
            get => m_ApertureDialControl.selectedValue;
            set => m_ApertureDialControl.selectedValue = value;
        }

        void Awake()
        {
            m_FocusDialControl.onSelectedValueChanged.AddListener(value => FocusDistanceValueChanged.Invoke(value));
            m_FocusOffsetDialControl.onSelectedValueChanged.AddListener(value => FocusOffsetValueChanged.Invoke(value));
            m_ApertureDialControl.onSelectedValueChanged.AddListener(value => ApertureValueChanged.Invoke(value));
            m_ApertureToggle.onValueChanged.AddListener(OnApretureToggle);
            m_Toggle.onValueChanged.AddListener(value =>
            {
                if (!value)
                {
                    Closed.Invoke();
                }
            });
        }

        public void SetFocusDistance(float focusDistance, Vector2 range)
        {
            m_FocusDialControl.selectedValue = focusDistance;
            m_FocusDialControl.minimumValue = range.x;
            m_FocusDialControl.maximumValue = range.y;
        }

        public void SetFocusDistanceOffset(float value)
        {
            m_FocusOffsetDialControl.selectedValue = value;
        }

        public void SetAperture(float aperture, Vector2 range)
        {
            m_ApertureDialControl.selectedValue = aperture;
            m_ApertureDialControl.minimumValue = range.x;
            m_ApertureDialControl.maximumValue = range.y;
        }

        void Start()
        {
            m_FocusOffsetDialControl.minimumValue = -Settings.k_MaxAbsFocusDistanceOffset;
            m_FocusOffsetDialControl.maximumValue = Settings.k_MaxAbsFocusDistanceOffset;
            m_FocusOffsetDialControl.SetCustomLabelProvider(m_FocusDistanceOffsetLabelProvider);

            m_FocusDialControl.maximumValue = LensLimits.FocusDistance.y;
            m_FocusDialControl.graduations = new FocusDistanceGraduations();
            m_FocusDialControl.scaler = m_FocusDistanceScaler;
            m_FocusDialControl.SetCustomLabelProvider(m_FocusDistanceLabelProvider);

            m_ApertureDialControl.SetCustomLabelProvider(m_ApertureLabelProvider);
        }

        void OnApretureToggle(bool value)
        {
            m_ApertureDialControl.gameObject.SetActive(value);
            m_ApretureToggleIcon.sprite = value ? m_ApertureDialOpenSprite : m_ApertureDialClosedSprite;
        }

        protected override void OnShowChanged()
        {
            m_Toggle.isOn = IsShown;
        }

        public void SetFocusMode(FocusMode value)
        {
            var autoFocus =
                value == FocusMode.ReticleAF ||
                value == FocusMode.TrackingAF;

            m_FocusOffsetDialControl.gameObject.SetActive(autoFocus);
            m_FocusDialControl.gameObject.SetActive(!autoFocus);
        }

        public void SetLensIntrinsics(LensIntrinsics value)
        {
            var closeFocusDistance = value.CloseFocusDistance;

            m_FocusDistanceLabelProvider.closeFocus = closeFocusDistance;

            if (m_CachedCloseFocusDistance != closeFocusDistance)
            {
                m_CachedCloseFocusDistance = closeFocusDistance;
                m_FocusDialControl.minimumValue = closeFocusDistance;
            }

            var apertureRange = value.ApertureRange;

            if (m_CachedApertureRange != apertureRange)
            {
                m_CachedApertureRange = apertureRange;

                if (apertureRange.x == apertureRange.y)
                {
                    // Only generate a single value if the min and max are the same
                    m_ApertureDialControl.SetMarkedEntries(new[] { apertureRange.x });
                }
                else
                {
                    var minIndex = 0;
                    var maxIndex = k_FNumberStops.Length - 1;
                    for (var i = 0; i < k_FNumberStops.Length; i++)
                    {
                        if (apertureRange.x <= k_FNumberStops[i])
                        {
                            minIndex = i;
                            break;
                        }
                    }

                    for (var i = 0; i < k_FNumberStops.Length; i++)
                    {
                        if (apertureRange.y >= k_FNumberStops[i])
                        {
                            maxIndex = i;
                        }
                    }

                    if (minIndex == maxIndex)
                    {
                        // If the min and max index are the same just set the entries equal to the min and max aperture
                        m_ApertureDialControl.SetMarkedEntries(new[] { apertureRange.x, apertureRange.y });
                    }
                    else
                    {
                        var len = maxIndex - minIndex + 1;
                        var entries = new float[len];
                        Array.ConstrainedCopy(k_FNumberStops, minIndex, entries,
                            0, len);
                        m_ApertureDialControl.SetMarkedEntries(entries);
                    }
                }
            }
        }
    }
}
