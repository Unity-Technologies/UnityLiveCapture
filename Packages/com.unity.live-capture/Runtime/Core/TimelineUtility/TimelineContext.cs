using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A structure to store a node of a timeline hierarchy.
    /// </summary>
    struct TimelineContext : IEquatable<TimelineContext>
    {
        PlayableDirector m_Director;
        TrackAsset m_Track;
        UnityObject m_Asset;

        /// <summary>
        /// The stored PlayableDirector.
        /// </summary>
        public PlayableDirector Director => m_Director;

        /// <summary>
        /// The TrackAsset containing the TimelineClip.
        /// </summary>
        public TrackAsset Track => m_Track;

        /// <summary>
        /// The asset that the TimelineClip references.
        /// </summary>
        public UnityObject Asset => m_Asset;

        /// <summary>
        /// Constructs a TimelineContext using a PlayableDirector.
        /// </summary>
        /// <remarks>
        /// The produced context does not operate another PlayableDirector.
        /// </remarks>
        /// <param name="director">The PlayableDirector in the context.</param>
        public TimelineContext(PlayableDirector director) : this()
        {
            if (director == null)
            {
                throw new ArgumentNullException(nameof(director));
            }

            m_Director = director;
        }

        /// <summary>
        /// Constructs a TimelineContext using a PlayableDirector and a TimelineClip.
        /// </summary>
        /// <remarks>
        /// The produced context operates another PlayableDirector within the provided clip.
        /// </remarks>
        /// <param name="director">The PlayableDirector in the context.</param>
        /// <param name="director">The TimelineClip in the context.</param>
        public TimelineContext(PlayableDirector director, TimelineClip clip) : this(director)
        {
            if (clip != null)
            {
                m_Track = clip.GetParentTrack();
                m_Asset = clip.asset;
            }
        }

        /// <summary>
        /// Checks if the <see cref="TimelineContext"/> is valid.
        /// </summary>
        /// <returns><see langword="true"/> if the context is valid; otherwise, <see langword="false"/>.</returns>
        public bool IsValid()
        {
            return m_Director != null;
        }

        /// <summary>
        /// Returns a string that represents the current context.
        /// </summary>
        /// <returns>A string that represents the current context.</returns>
        public override string ToString()
        {
            if (!IsValid())
            {
                return "Invalid";
            }

            Debug.Assert(m_Director != null);

            if (this.TryGetTimelineClip(out var clip))
            {
                return $"{m_Track.timelineAsset.name} ({clip.displayName})";
            }

            if (m_Director.playableAsset != null)
            {
                return m_Director.playableAsset.name;
            }

            return m_Director.name;
        }

        /// <summary>
        /// Determines whether the <see cref="TimelineContext"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="TimelineContext"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>

        public bool Equals(TimelineContext other)
        {
            return m_Director == other.m_Director
                && m_Track == other.m_Track
                && m_Asset == other.m_Asset;
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is TimelineContext other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for this <see cref="TimelineContext"/>.
        /// </summary>
        /// <returns>
        /// The hash value generated for this <see cref="TimelineContext"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Director.GetHashCode();
                if (m_Track != null)
                    hashCode = (hashCode * 397) ^ m_Track.GetHashCode();
                if (m_Asset != null)
                    hashCode = (hashCode * 397) ^ m_Asset.GetHashCode();
                return hashCode;
            }
        }
    }

    static class TimelineContextExtensions
    {
        /// <summary>
        /// Gets the current TimelineClip that a <see cref="TimelineContext"/> stores.
        /// </summary>
        /// <param name="context">The <see cref="TimelineContext"/> to look into.</param>
        /// <param name="clip">When this method returns, contains the TimelineClip currently stored,
        /// if any; otherwise, the default value for TimelineClip.</param>
        /// <returns><see langword="true"/> if the clip was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetTimelineClip(this TimelineContext context, out TimelineClip clip)
        {
            clip = default(TimelineClip);

            if (context.Track != null && context.Asset != null)
            {
                foreach (var c in context.Track.GetClips())
                {
                    if (c.asset == context.Asset)
                    {
                        clip = c;
                        break;
                    }
                }
            }

            return clip != null;
        }

        /// <summary>
        /// Gets the time of a <see cref="TimelineContext"/>.
        /// </summary>
        /// <remarks>
        /// The time is local to the stored TimelineClip, if any. Otherwise, the time of the PlayableDirector is used.
        /// </remarks>
        /// <returns>The time of the context.</returns>
        public static double GetTime(this TimelineContext context)
        {
            if (!context.IsValid())
                return 0d;

            var offset = 0d;

            if (context.TryGetTimelineClip(out var clip))
            {
                offset = clip.start;
            }

            return context.Director.time - offset;
        }

        /// <summary>
        /// Sets the time of a <see cref="TimelineContext"/>.
        /// </summary>
        /// <remarks>
        /// The time is local to the stored TimelineClip, if any. Otherwise, the time of the PlayableDirector is used.
        /// </remarks>
        public static void SetTime(this TimelineContext context, double time)
        {
            if (!context.IsValid())
                return;

            var offset = 0d;

            if (context.TryGetTimelineClip(out var clip))
            {
                offset = clip.start;
            }

            PlayableDirectorControls.SetTime(context.Director, time + offset);
        }
    }
}
