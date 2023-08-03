using TMPro;
using UnityEngine;
using Unity.LiveCapture;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class FrameHudView : MonoBehaviour, ITimeListener
    {
        [SerializeField]
        TextMeshProUGUI m_FrameText;
        [SerializeField]
        TextMeshProUGUI m_DurationText;

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            var frameTime = FrameTime.FromSeconds(frameRate, time);
            var durationFrameTime = FrameTime.FromSeconds(frameRate, duration);

            m_FrameText.text = frameTime.FrameNumber.ToString();
            m_DurationText.text = durationFrameTime.FrameNumber.ToString();
        }
    }
}
