using System.Collections.Generic;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Populates a list of <see cref="ActionState"/> representing all possible
    /// user actions. Sends signals to the rest of the application in response
    /// to the user actions.
    /// </para>
    ///
    /// <para>
    /// Also handles continuously changing values (ex time, lens aperture).
    /// </para>
    /// </summary>
    interface IActionProcessor
    {
        IValueTracker<double> Time { get; }
        IValueTracker<float> FocalLength { get; }
        IValueTracker<float> Aperture { get; }
        IValueTracker<float> FocusDistance { get; }

        void InitializeActions(ICollection<ActionState> actions);
    }
}
