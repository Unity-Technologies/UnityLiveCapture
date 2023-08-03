using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    /// <summary>
    /// A class containing extension method(s) for <see cref="FocusMode"/>.
    /// </summary>
    static class FocusModeExtensions
    {
        /// <summary>
        /// Provides a user-friendly description of a Focus Mode value.
        /// </summary>
        /// <param name="mode">The focus mode.</param>
        /// <returns>The description string.</returns>
        public static string GetDescription(this FocusMode mode)
        {
            switch (mode)
            {
                case FocusMode.Clear:
                    return "Clear";
                case FocusMode.Manual:
                    return "Manual";
                case FocusMode.ReticleAF:
                    return "Reticle AF";
                case FocusMode.TrackingAF:
                    return "Tracking AF";
            }

            return string.Empty;
        }
    }
}
