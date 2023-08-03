using System;
using System.Linq;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Uses <see cref="INotificationSystem"/> to display text notifications
    /// corresponding to triggered actions.
    /// </summary>
    class ActionNotifier : IActionNotifier, ITickable
    {
        // Corresponds to the focus mode labels in the UI
        static readonly string[] FocusModeNames = Enum.GetNames(typeof(FocusMode)).Select(Utilities.InsertSpaceBetweenCapitals).ToArray();

        [Inject]
        IActionProcessor m_Processor;

        [Inject]
        INotificationSystem m_Notification;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        [Inject]
        CameraModel m_Camera;

        [Inject]
        SettingsModel m_Settings;

        [Inject]
        ARSystem m_AR;

        float m_TimeTilting;
        float m_TimeRolling;

        public void Notify(ActionID action)
        {
            var text = string.Empty;

            // Will be true for actions that may repeat every frame due to
            // their continuous nature. Otherwise the fade-in resets the
            // alpha on each repeated frame.
            var persistentAlpha = false;

            switch (action)
            {
                case ActionID.ResetPose:
                    text = "Reset pose";
                    break;

                case ActionID.ResetLens:
                    text = "Reset lens";
                    break;

                case ActionID.Rebase:
                    var noun = m_Settings.Rebasing ? "on" : "off";
                    text = $"AR {noun}";
                    break;

                case ActionID.ZoomIncrease:
                case ActionID.ZoomDecrease:
                    text = $"{m_Processor.FocalLength.Current:F1}mm";
                    persistentAlpha = true;
                    break;

                case ActionID.FStopIncrease:
                case ActionID.FStopDecrease:
                    text = $"f/{m_Processor.Aperture.Current:F1}";
                    persistentAlpha = true;
                    break;

                case ActionID.FocusDistanceIncrease:
                case ActionID.FocusDistanceDecrease:
                    var minFocusDistance = m_Camera.Intrinsics.CloseFocusDistance;
                    text = FocusDistanceUtility.AsString(m_Processor.FocusDistance.Current, minFocusDistance, "m");
                    persistentAlpha = true;
                    break;

                case ActionID.NextFocusMode:
                    var nextFocusMode = Utilities.GetNextFocusMode(m_Settings.FocusMode, false);
                    var nextFocusModeIndex = (int) nextFocusMode;
                    text = $"Focus mode {FocusModeNames[nextFocusModeIndex]}";
                    break;

                case ActionID.PreviousFocusMode:
                    var previousFocusMode = Utilities.GetNextFocusMode(m_Settings.FocusMode, true);
                    var previousFocusModeIndex = (int) previousFocusMode;
                    text = $"Focus mode {FocusModeNames[previousFocusModeIndex]}";
                    break;

                case ActionID.SkipOneFrame:
                    text = "+1 frame";
                    break;

                case ActionID.RewindOneFrame:
                    text = "-1 frame";
                    break;

                case ActionID.SkipTenFrames:
                    text = "+10 frames";
                    break;

                case ActionID.RewindTenFrames:
                    text = "-10 frames";
                    break;

                case ActionID.ToggleDeviceMode:
                    var mode = m_CompanionApp.DeviceMode == DeviceMode.Playback ? "Live" : "Playback";
                    text = $"{mode} mode";
                    break;
            }

            if (text != string.Empty)
            {
                DisplayNotification(text, persistentAlpha);
            }
        }

        public void Tick()
        {
            var tilt = Mathf.Abs(m_AR.GamepadLookValue.x);
            var pan = Mathf.Abs(m_AR.GamepadLookValue.y);
            var roll = Mathf.Abs(m_AR.GamepadLookValue.z);

            if (tilt > Mathf.Epsilon && tilt > pan * 1.5f)
            {
                m_TimeTilting += Time.unscaledDeltaTime;
            }
            else
            {
                m_TimeTilting = 0f;
            }

            if (roll > Mathf.Epsilon && roll > pan * 1.5f)
            {
                m_TimeRolling += Time.unscaledDeltaTime;
            }
            else
            {
                m_TimeRolling = 0f;
            }

            if (m_TimeTilting > 0.5f && !m_Settings.Rebasing)
            {
                DisplayNotification("Tilt angle is fixed while AR tracking is on", true);
            }
            else if (m_TimeRolling > 0.5f && !m_Settings.Rebasing)
            {
                DisplayNotification("Dutch angle is fixed while AR tracking is on", true);
            }
        }

        void DisplayNotification(string text, bool persistentAlpha)
        {
            var data = new Notification.NotificationData()
            {
                displayDuration = 1.0f,
                fadeDuration = 0.1f,
                text = text,
                persistentAlpha = persistentAlpha
            };

            m_Notification.Show(data);
        }
    }
}
