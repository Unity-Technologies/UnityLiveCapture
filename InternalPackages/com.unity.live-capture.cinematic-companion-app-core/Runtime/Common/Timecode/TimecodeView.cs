using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.LiveCapture;

namespace Unity.CompanionAppCommon
{
    class TimecodeView : MonoBehaviour, ITimeListener
    {
        [SerializeField]
        TextMeshProUGUI m_HoursText;
        [SerializeField]
        TextMeshProUGUI m_MinutesText;
        [SerializeField]
        TextMeshProUGUI m_SecondsText;
        [SerializeField]
        TextMeshProUGUI m_FramesText;
        [SerializeField]
        TextMeshProUGUI m_FrameSeparatorText;

        readonly Dictionary<int, string> m_Cache = new Dictionary<int, string>();

        void CacheValue(int value) => m_Cache.Add(value, value.ToString("D2"));

        void PopulateCache()
        {
            m_Cache.Clear();

            for (var i = 0; i != 100; ++i)
            {
                CacheValue(i);
            }
        }

        string IntToString(int value)
        {
            if (m_Cache.TryGetValue(value, out var cachedStr))
            {
                return cachedStr;
            }

            CacheValue(value);
            return IntToString(value);
        }

        void Awake()
        {
            PopulateCache();
        }

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            var timecode = Timecode.FromSeconds(frameRate, time);

            m_HoursText.text = IntToString(timecode.Hours);
            m_MinutesText.text = IntToString(timecode.Minutes);
            m_SecondsText.text = IntToString(timecode.Seconds);
            m_FramesText.text = IntToString(timecode.Frames);

            m_FrameSeparatorText.text = timecode.IsDropFrame ? ";" : ":";
        }
    }
}
