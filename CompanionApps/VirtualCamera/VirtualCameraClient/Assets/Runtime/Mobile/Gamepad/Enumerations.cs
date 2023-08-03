namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Groups two actions (one negative, one positive) together.
    /// </summary>
    /// <seealso cref="AxisSensitivities"/>
    /// <seealso cref="AxisState"/>
    enum AxisID
    {
        MoveLateral,
        MoveForward,
        MoveVertical,
        LookTilt,
        LookPan,
        LookRoll,
        Time,
        Zoom,
        FStop,
        FocusDistance,
        Count
    }

    /// <summary>
    /// Identifies an action. <see cref="IActionResolver"/> provides 1-to-1 mapping
    /// of InputActions (as defined in the InputActionAsset) to ActionIDs.
    /// </summary>
    enum ActionID
    {
        // Movement
        MoveForward,
        MoveBackward,
        MoveRight,
        MoveLeft,
        MoveUp,
        MoveDown,
        LookUp,
        LookDown,
        LookRight,
        LookLeft,
        LookClockwise,
        LookCounterClockwise,

        // Reset
        ResetPose,
        ResetLens,
        Rebase,

        // Lens Settings
        ZoomIncrease,
        ZoomDecrease,
        FStopIncrease,
        FStopDecrease,
        FocusDistanceIncrease,
        FocusDistanceDecrease,
        NextFocusMode,
        PreviousFocusMode,

        // Timeline
        ToggleRecording,
        TogglePlayback,
        FastForward,
        Rewind,
        SkipOneFrame,
        RewindOneFrame,
        SkipTenFrames,
        RewindTenFrames,

        // Playback Mode
        ToggleDeviceMode,
        Count
    }
}
