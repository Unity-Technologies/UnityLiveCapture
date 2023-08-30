using System;
using System.Linq;

namespace Unity.LiveCapture.Editor
{
    static class TimelineHierarchyContextExtensions
    {
        /// <summary>
        /// Checks if the provided <see cref="TimelineHierarchyContext"/> matches the hierarchy displayed in the Timeline window.
        /// </summary>
        /// <param name="context">The <see cref="TimelineHierarchyContext"/> to check for.</param>
        /// <returns><see langword="true"/> if the <see cref="TimelineHierarchyContext"/> matches; otherwise, <see langword="false"/>.</returns>
        public static bool MatchesTimelineNavigation(this TimelineHierarchyContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Enumerable.SequenceEqual(context.Hierarchy, TimelineHierarchyContextUtility.EnumerateFromTimelineNavigation());
        }
    }
}
