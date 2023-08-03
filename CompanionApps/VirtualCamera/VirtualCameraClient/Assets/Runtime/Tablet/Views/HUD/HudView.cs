using System;
using TMPro;
using Unity.LiveCapture;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class HudView : DialogView,
        ITimeListener,
        ISensorSizeListener,
        IFocalLengthListener,
        IApertureListener,
        IFocusDistanceListener,
        IFocusModeListener,
        ILensIntrinsicsListener,
        ISlateShotNameListener,
        ISlateTakeNumberListener
    {
        [SerializeField]
        FrameHudView m_FrameHudView;
        [SerializeField]
        TimecodeView m_TimeHudView;
        [SerializeField]
        GateHudView m_GateHudView;
        [SerializeField]
        ZoomHudView m_ZoomHudView;
        [SerializeField]
        ApertureHudView m_ApertureHudView;
        [SerializeField]
        FocusDistanceHudView m_FocusDistanceHudView;
        [SerializeField]
        FocusModeHudView m_FocusModeHudView;
        [SerializeField]
        TextMeshProUGUI m_ShotNameView;
        [SerializeField]
        TextMeshProUGUI m_TakeNumberView;

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            m_FrameHudView.SetTime(time, duration, frameRate);
            m_TimeHudView.SetTime(time, duration, frameRate);
        }

        public void SetSensorSize(Vector2 sensorSize)
        {
            m_GateHudView.SetSensorSize(sensorSize);
        }

        public void SetFocalLength(float value, Vector2 range)
        {
            m_ZoomHudView.SetFocalLength(value, range);
        }

        public void SetAperture(float aperture, Vector2 range)
        {
            m_ApertureHudView.SetAperture(aperture, range);
        }

        public void SetFocusDistance(float value, Vector2 range)
        {
            m_FocusDistanceHudView.SetFocusDistance(value, range);
        }

        public void SetFocusMode(FocusMode value)
        {
            m_FocusModeHudView.SetFocusMode(value);
            m_FocusDistanceHudView.SetFocusMode(value);
        }

        public void SetLensIntrinsics(LensIntrinsics value)
        {
            m_FocusDistanceHudView.SetLensIntrinsics(value);
        }

        public void SetSlateShotName(string value)
        {
            m_ShotNameView.text = value;
        }

        public void SetSlateTakeNumber(int takeNumber)
        {
            m_TakeNumberView.text = takeNumber != -1 ? takeNumber.ToString() : String.Empty;
        }
    }
}
