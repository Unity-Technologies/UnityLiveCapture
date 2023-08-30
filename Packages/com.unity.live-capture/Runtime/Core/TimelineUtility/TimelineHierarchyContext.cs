using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Playables;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A class that stores the state of a timeline hierarchy.
    /// </summary>
    class TimelineHierarchyContext : IEquatable<TimelineHierarchyContext>
    {
        List<TimelineContext> m_Hierarchy = new List<TimelineContext>();

        /// <summary>
        /// The chain of <see cref="TimelineContext"/>, from leaf to root, stored.
        /// </summary>
        public IEnumerable<TimelineContext> Hierarchy => m_Hierarchy;

        protected TimelineHierarchyContext() { }

        /// <summary>
        /// Constructs a hierarchy contexts from an enumeration of <see cref="TimelineContext"/>.
        /// </summary>
        /// <param name="hierarchy">The enumeration of <see cref="TimelineContext"/> to store.</param>
        public TimelineHierarchyContext(IEnumerable<TimelineContext> hierarchy)
        {
            m_Hierarchy.AddRange(hierarchy);
        }

        /// <summary>
        /// Returns a string that represents the current context.
        /// </summary>
        /// <returns>A string that represents the current context.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            for (var i = m_Hierarchy.Count - 1; i >= 0; --i)
            {
                var context = m_Hierarchy[i];

                builder.Append(context.ToString());

                if (i > 0)
                {
                    builder.Append("/");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Determines whether the <see cref="TimelineHierarchyContext"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="TimelineHierarchyContext"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>

        public bool Equals(TimelineHierarchyContext other)
        {
            if (other == null)
            {
                return false;
            }

            return m_Hierarchy.SequenceEqual(other.m_Hierarchy);
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is TimelineHierarchyContext other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for this <see cref="TimelineHierarchyContext"/>.
        /// </summary>
        /// <returns>
        /// The hash value generated for this <see cref="TimelineHierarchyContext"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 397;

                foreach (var ctx in m_Hierarchy)
                {
                    hashCode = (hashCode * 397) ^ ctx.GetHashCode();
                }

                return hashCode;
            }
        }
    }

    static class TimelineHierarchyContextExtensions
    {
        /// <summary>
        /// Checks if the <see cref="TimelineHierarchyContext"/> is valid.
        /// </summary>
        /// <returns><see langword="true"/> if the context is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this TimelineHierarchyContext context)
        {
            return context != null
                && context.TryGetRootAndTimeOffset(out var root, out var timeOffset);
        }

        /// <summary>
        /// Gets the time of a <see cref="TimelineHierarchyContext"/> at the leaf.
        /// </summary>
        /// <returns>The time of the context.</returns>
        public static double GetLocalTime(this TimelineHierarchyContext context)
        {
            if (context.TryGetRootAndTimeOffset(out var root, out var offset))
            {
                return root.time - offset;
            }
            else
            {
                return 0d;
            }
        }

        /// <summary>
        /// Sets the time of a <see cref="TimelineHierarchyContext"/>.
        /// </summary>
        /// <param name="context">The context to set the time to.</param>
        /// <param name="time">The time to set.</param>
        public static void SetLocalTime(this TimelineHierarchyContext context, double time)
        {
            if (context.TryGetRootAndTimeOffset(out var director, out var offset))
            {
                PlayableDirectorControls.SetTime(director, offset + time);
            }
        }

        /// <summary>
        /// Gets the PlayableDirector at the root of a provided <see cref="TimelineHierarchyContext"/>.
        /// </summary>
        /// <param name="context">The context to look the root for.</param>
        /// <returns>The root PlayableDirector.</returns>
        public static PlayableDirector GetRootDirector(this TimelineHierarchyContext context)
        {
            return context.Hierarchy.LastOrDefault().Director;
        }

        /// <summary>
        /// Gets the PlayableDirector at the root of a provided <see cref="TimelineHierarchyContext"/> and the accumulated time offset from the leaf.
        /// </summary>
        /// <param name="context">The context to look the root for.</param>
        /// <param name="root">When this method returns, contains the PlayableDirector at the root; otherwise, null.</param>
        /// <param name="timeOffset">When this method returns, contains the accumulated time offset from the leaf; otherwise, 0.</param>
        /// <returns><see langword="true"/> if the root was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetRootAndTimeOffset(
            this TimelineHierarchyContext context, out PlayableDirector root, out double timeOffset)
        {
            root = default(PlayableDirector);
            timeOffset = 0d;

            foreach (var entry in context.Hierarchy)
            {
                if (!entry.IsValid())
                {
                    return false;
                }

                var director = entry.Director;

                if (entry.TryGetTimelineClip(out var clip))
                {
                    timeOffset += clip.start;
                }

                root = director;
            }

            return root != null;
        }

        /// <summary>
        /// Gets the <see cref="TimelineContext"/> at the bottom of the <see cref="TimelineHierarchyContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="TimelineHierarchyContext"/> to look the leaf for.</param>
        /// <returns>The leaf <see cref="TimelineContext"/>.</returns>
        public static TimelineContext GetLeaf(this TimelineHierarchyContext context)
        {
            if (context != null)
            {
                return context.Hierarchy.FirstOrDefault();
            }
            else
            {
                return default(TimelineContext);
            }
        }
    }
}
