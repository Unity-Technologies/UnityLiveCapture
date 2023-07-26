namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Displays notifications corresponding to triggered actions.
    /// </summary>
    interface IActionNotifier
    {
        void Notify(ActionID action);
    }
}
