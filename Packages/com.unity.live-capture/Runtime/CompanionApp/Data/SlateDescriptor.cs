using System;
using System.Linq;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// Class that stores information of a shot. The client uses this information to build its UI.
    /// </summary>
    [Serializable]
    class ShotDescriptor
    {
        /// <summary>
        ///
        /// </summary>
        public Slate Slate = Slate.Empty;

        /// <summary>
        /// The duration of the shot in seconds.
        /// </summary>
        public double Duration;

        /// <summary>
        /// The index of the selected take in the take list.
        /// </summary>
        public int SelectedTake = -1;

        /// <summary>
        /// The index of the iteration base in the take list.
        /// </summary>
        public int IterationBase = -1;

        /// <summary>
        /// The list of available takes in the slate.
        /// </summary>
        public TakeDescriptor[] Takes;

        internal static ShotDescriptor Create(Shot? nShot)
        {
            var descriptor = new ShotDescriptor();
#if UNITY_EDITOR
            if (nShot is Shot shot)
            {
                var takes = AssetDatabaseUtility.GetAssetsAtPath<Take>(shot.Directory);
                descriptor.Slate = shot.Slate;
                descriptor.Duration = shot.Duration;
                descriptor.SelectedTake = takes.IndexOf(shot.Take);
                descriptor.IterationBase = takes.IndexOf(shot.IterationBase);
                descriptor.Takes = takes.Select(take => TakeDescriptor.Create(take)).ToArray();
            }
#endif
            return descriptor;
        }
    }
}
