using System.Collections.Generic;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Declares <see cref="ActionState"/>s to represent all user actions, and
    /// connects their callbacks to the rest of the application through Zenject
    /// signals.
    /// </para>
    ///
    /// <para>
    /// Also handles continuously changing values (ex time, lens aperture).
    /// </para>
    /// </summary>
    class ActionProcessor : IActionProcessor, IInitializable, ILateTickable
    {
        [Inject]
        SignalBus m_SignalBus;

        [Inject]
        ConnectionModel m_Connection;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        [Inject]
        CameraModel m_Camera;

        [Inject]
        SettingsModel m_Settings;

        [Inject]
        IActionNotifier m_Notifier;

        readonly TimeScaler m_TimeScaler = new TimeScaler();
        readonly DoubleValueTracker m_Time = new DoubleValueTracker();
        float m_TimeSpeed;

        readonly FocalLengthScaler m_FocalLengthScaler = new FocalLengthScaler();
        readonly FloatValueTracker m_FocalLength = new FloatValueTracker();
        float m_FocalLengthSpeed;

        readonly ApertureScaler m_ApertureScaler = new ApertureScaler();
        readonly FloatValueTracker m_Aperture = new FloatValueTracker();
        float m_ApertureSpeed;

        readonly FocusDistanceScaler m_FocusDistanceScaler = new FocusDistanceScaler();
        readonly FloatValueTracker m_FocusDistance = new FloatValueTracker();
        float m_FocusDistanceSpeed;

        public IValueTracker<double> Time => m_Time;
        public IValueTracker<float> FocalLength => m_FocalLength;
        public IValueTracker<float> Aperture => m_Aperture;
        public IValueTracker<float> FocusDistance => m_FocusDistance;

        public void Initialize()
        {
            m_Time.Scaler = m_TimeScaler;
            m_FocalLength.Scaler = m_FocalLengthScaler;
            m_Aperture.Scaler = m_ApertureScaler;
            m_FocusDistance.Scaler = m_FocusDistanceScaler;
        }

        T AddAction<T>(ICollection<ActionState> actions, T action) where T : ActionState
        {
            actions.Add(action);
            return action;
        }

        void AddAxis(AxisID axisID, AnalogActionState positiveAction, AnalogActionState negativeAction)
        {
            var axisState = new AxisState(axisID);
            positiveAction.Axis = negativeAction.Axis = axisState;

            positiveAction.AxisDirection = AxisState.Direction.Positive;
            negativeAction.AxisDirection = AxisState.Direction.Negative;

            positiveAction.OppositeAction = negativeAction;
            negativeAction.OppositeAction = positiveAction;
        }

        void Notify(ActionID action)
        {
            m_Notifier.Notify(action);
        }

        public void InitializeActions(ICollection<ActionState> actions)
        {
            // Movement
            var moveForward = AddAction(actions, new AnalogActionState(
                ActionID.MoveForward,
                (v) => m_SignalBus.Fire(new SetGamepadForwardSignal() { value = v })
            ));
            var moveBackward = AddAction(actions, new AnalogActionState(
                ActionID.MoveBackward,
                (v) => m_SignalBus.Fire(new SetGamepadForwardSignal() { value = v })
            ));
            AddAxis(AxisID.MoveForward, moveForward, moveBackward);

            var moveRight = AddAction(actions, new AnalogActionState(
                ActionID.MoveRight,
                (v) => m_SignalBus.Fire(new SetGamepadLateralSignal() { value = v })
            ));
            var moveLeft = AddAction(actions, new AnalogActionState(
                ActionID.MoveLeft,
                (v) => m_SignalBus.Fire(new SetGamepadLateralSignal() { value = v })
            ));
            AddAxis(AxisID.MoveLateral, moveRight, moveLeft);

            var moveUp = AddAction(actions, new AnalogActionState(
                ActionID.MoveUp,
                (v) => m_SignalBus.Fire(new SetGamepadVerticalSignal() { value = v })
            ));
            var moveDown = AddAction(actions, new AnalogActionState(
                ActionID.MoveDown,
                (v) => m_SignalBus.Fire(new SetGamepadVerticalSignal() { value = v })
            ));
            AddAxis(AxisID.MoveVertical, moveUp, moveDown);

            var lookUp = AddAction(actions, new AnalogActionState(
                ActionID.LookUp,
                (v) => m_SignalBus.Fire(new SetGamepadTiltSignal() { value = v })
            ));
            var lookDown = AddAction(actions, new AnalogActionState(
                ActionID.LookDown,
                (v) => m_SignalBus.Fire(new SetGamepadTiltSignal() { value = v })
            ));
            AddAxis(AxisID.LookTilt, lookUp, lookDown);

            var lookRight = AddAction(actions, new AnalogActionState(
                ActionID.LookRight,
                (v) => m_SignalBus.Fire(new SetGamepadPanSignal() { value = v })
            ));
            var lookLeft = AddAction(actions, new AnalogActionState(
                ActionID.LookLeft,
                (v) => m_SignalBus.Fire(new SetGamepadPanSignal() { value = v })
            ));
            AddAxis(AxisID.LookPan, lookRight, lookLeft);

            var lookClockwise = AddAction(actions, new AnalogActionState(
                ActionID.LookClockwise,
                (v) => m_SignalBus.Fire(new SetGamepadRollSignal() { value = v })
            ));
            var lookCounterClockwise = AddAction(actions, new AnalogActionState(
                ActionID.LookCounterClockwise,
                (v) => m_SignalBus.Fire(new SetGamepadRollSignal() { value = v })
            ));
            AddAxis(AxisID.LookRoll, lookClockwise, lookCounterClockwise);

            // Reset
            AddAction(actions, new ButtonActionState(
                ActionID.ResetPose,
                () =>
                {
                    Notify(ActionID.ResetPose);
                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.SetPoseToOrigin
                    });
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.ResetLens,
                () =>
                {
                    Notify(ActionID.ResetLens);
                    m_SignalBus.Fire(new ResetLensSignal());
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.Rebase,
                () =>
                {
                    Notify(ActionID.Rebase);
                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.Rebasing,
                        BoolValue = !m_Settings.Rebasing
                    });
                }
            ));

            // Lens Settings
            var zoomIn = AddAction(actions, new AnalogActionState(
                ActionID.ZoomIncrease,
                (v) => m_FocalLengthSpeed = v
            ));
            var zoomOut = AddAction(actions, new AnalogActionState(
                ActionID.ZoomDecrease,
                (v) => m_FocalLengthSpeed = v
            ));
            AddAxis(AxisID.Zoom, zoomIn, zoomOut);

            var fStopIncrease = AddAction(actions, new AnalogActionState(
                ActionID.FStopIncrease,
                (v) => m_ApertureSpeed = v
            ));
            var fStopDecrease = AddAction(actions, new AnalogActionState(
                ActionID.FStopDecrease,
                (v) => m_ApertureSpeed = v
            ));
            AddAxis(AxisID.FStop, fStopIncrease, fStopDecrease);

            var focusDistanceIncrease = AddAction(actions, new AnalogActionState(
                ActionID.FocusDistanceIncrease,
                (v) => m_FocusDistanceSpeed = v
            ));
            var focusDistanceDecrease = AddAction(actions, new AnalogActionState(
                ActionID.FocusDistanceDecrease,
                (v) => m_FocusDistanceSpeed = v
            ));
            AddAxis(AxisID.FocusDistance, focusDistanceIncrease, focusDistanceDecrease);

            AddAction(actions, new ButtonActionState(
                ActionID.NextFocusMode,
                () =>
                {
                    Notify(ActionID.NextFocusMode);
                    var newFocusMode = Utilities.GetNextFocusMode(m_Settings.FocusMode, false);
                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.FocusMode,
                        FocusModeValue = newFocusMode
                    });
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.PreviousFocusMode,
                () =>
                {
                    Notify(ActionID.PreviousFocusMode);
                    var newFocusMode = Utilities.GetNextFocusMode(m_Settings.FocusMode, true);
                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.FocusMode,
                        FocusModeValue = newFocusMode
                    });
                }
            ));

            // Timeline
            AddAction(actions, new ButtonActionState(
                ActionID.ToggleRecording,
                () =>
                {
                    if (m_CompanionApp.DeviceMode == DeviceMode.LiveStream)
                        m_SignalBus.Fire(new MainViewSignals.ToggleRecording());
                    else
                        m_SignalBus.Fire(new TogglePlaybackSignal());
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.TogglePlayback,
                () => m_SignalBus.Fire(new TogglePlaybackSignal())
            ));

            var fastForward = AddAction(actions, new AnalogActionState(
                ActionID.FastForward,
                (v) => m_TimeSpeed = v
            ));
            var rewind = AddAction(actions, new AnalogActionState(
                ActionID.Rewind,
                (v) => m_TimeSpeed = v
            ));
            AddAxis(AxisID.Time, fastForward, rewind);

            AddAction(actions, new ButtonActionState(
                ActionID.SkipOneFrame,
                () =>
                {
                    Notify(ActionID.SkipOneFrame);
                    m_SignalBus.Fire(new SkipFramesSignal() {value = 1});
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.RewindOneFrame,
                () =>
                {
                    Notify(ActionID.RewindOneFrame);
                    m_SignalBus.Fire(new SkipFramesSignal() {value = -1});
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.SkipTenFrames,
                () =>
                {
                    Notify(ActionID.SkipTenFrames);
                    m_SignalBus.Fire(new SkipFramesSignal() {value = 10});
                }
            ));

            AddAction(actions, new ButtonActionState(
                ActionID.RewindTenFrames,
                () =>
                {
                    Notify(ActionID.RewindTenFrames);
                    m_SignalBus.Fire(new SkipFramesSignal() {value = -10});
                }
            ));

            // Playback Mode
            AddAction(actions, new ButtonActionState(
                ActionID.ToggleDeviceMode,
                () =>
                {
                    Notify(ActionID.ToggleDeviceMode);
                    m_SignalBus.Fire(new MainViewSignals.ToggleDeviceMode());
                }
            ));
        }

        public void LateTick()
        {
            var deltaTime = UnityEngine.Time.unscaledDeltaTime;

            m_FocalLengthScaler.Range = m_Camera.Intrinsics.FocalLengthRange;
            m_FocalLengthScaler.SensorSize = m_Camera.Body.SensorSize;
            m_ApertureScaler.Range = m_Camera.Intrinsics.ApertureRange;
            m_FocusDistanceScaler.Range = new Vector2(m_Camera.Intrinsics.CloseFocusDistance, LensLimits.FocusDistance.y);

            m_Time.Update(m_CompanionApp.SlatePreviewTime, m_TimeSpeed, deltaTime);
            m_FocalLength.Update(m_Camera.Lens.FocalLength, m_FocalLengthSpeed, deltaTime);
            m_Aperture.Update(m_Camera.Lens.Aperture, m_ApertureSpeed, deltaTime);
            m_FocusDistance.Update(m_Camera.Lens.FocusDistance, m_FocusDistanceSpeed, deltaTime);

            if (m_Connection.State == ConnectionState.Connected)
            {
                if (m_Time.Changing)
                    m_CompanionApp.SetPlaybackTime(m_Time.Current);

                if (m_FocalLength.Changing)
                {
                    Notify(ActionID.ZoomIncrease);

                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.FocalLength,
                        FloatValue = m_FocalLength.Current
                    });
                }

                if (m_Aperture.Changing)
                {
                    Notify(ActionID.FStopIncrease);

                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.Aperture,
                        FloatValue = m_Aperture.Current
                    });
                }

                if (m_FocusDistance.Changing)
                {
                    Notify(ActionID.FocusDistanceIncrease);

                    m_SignalBus.Fire(new SendHostSignal()
                    {
                        Type = HostMessageType.FocusDistance,
                        FloatValue = m_FocusDistance.Current
                    });
                }
            }
        }
    }
}
