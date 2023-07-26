using System.Collections.Generic;

namespace Unity.LiveCapture
{
    static class TimelineHierarchyContextUtility
    {
        /// <summary>
        /// Creates a <see cref="TimelineHierarchyContext"/> from a provided <see cref="TimelineContext"/>.
        /// </summary>
        public static TimelineHierarchyContext FromContext(TimelineContext context)
        {
            return new TimelineHierarchyContext(EnumerateFromContext(context));
        }

        static IEnumerable<TimelineContext> EnumerateFromContext(TimelineContext context)
        {
            if (!context.IsValid())
            {
                yield return default(TimelineContext);
                yield break;
            }

            yield return context;

            var director = context.Director;

            while (TimelineHierarchy.TryGetParentContext(director, out var parentDirector, out var parentClip))
            {
                yield return new TimelineContext(parentDirector, parentClip);

                director = parentDirector;
            }
        }
    }
}
