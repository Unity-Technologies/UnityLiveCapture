using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a <see cref="Take"/> asset creator.
    /// </summary>
    public interface ITakeBuilder
    {
        /// <summary>
        /// The start time in seconds of the recording context.
        /// </summary>
        double ContextStartTime { get; }

        /// <summary>
        /// Creates an animation track.
        /// </summary>
        /// <remarks>
        /// This method checks if another animation track is using the same binding.
        /// In that case, the new track becomes an override of the existing one.
        /// You can optionally pass in a <paramref name="alignTime"/>, which will be used for aligning
        /// synchronized recordings under the same timecode source.
        /// If <paramref name="alignTime"/> is <c>null</c>, no alignment will be performed for this track.
        /// </remarks>
        /// <param name="name">The name of the track.</param>
        /// <param name="animator">The target animator component to bind.</param>
        /// <param name="animationClip">The animation clip to set into the new track.</param>
        /// <param name="metadata">The metadata associated with the new track (optional).</param>
        /// <param name="alignTime">The timecode (expressed in seconds) of first recorded sample (optional).</param>
        void CreateAnimationTrack(string name, Animator animator, AnimationClip animationClip, ITrackMetadata metadata = null, double? alignTime = null);

        /// <summary>
        /// Adds a <see cref="TakeBinding{T}"/> to the Take.
        /// </summary>
        /// <typeparam name="T">The type of binding created. The track type must be derived from ITakeBinding.</typeparam>
        /// <param name="binding">The binding to add.</param>
        /// <param name="value">The object to bind.</param>
        void AddBinding<T>(TakeBinding<T> binding, T value) where T : UnityEngine.Object;

        /// <summary>
        /// Creates a new track without binding.
        /// </summary>
        /// <remarks>
        /// You can optionally pass in a <paramref name="alignTime"/>, which will be used for aligning
        /// synchronized recordings under the same timecode source.
        /// If <paramref name="alignTime"/> is <c>null</c>, no alignment will be performed for this track.
        /// </remarks>
        /// <param name="name">The name of the track.</param>
        /// <param name="alignTime">The timecode (expressed in seconds) of first recorded sample (optional).</param>
        /// <typeparam name="T">The type of track being created. The track type must be derived from TrackAsset.</typeparam>
        /// <returns>Returns the created track.</returns>
        T CreateTrack<T>(string name, double? alignTime = null) where T : TrackAsset, new();

        /// <summary>
        /// Creates a new track with binding.
        /// </summary>
        /// <remarks>
        /// You can optionally pass in a <paramref name="alignTime"/>, which will be used for aligning
        /// synchronized recordings under the same timecode source.
        /// If <paramref name="alignTime"/> is <c>null</c>, no alignment will be performed for this track.
        /// </remarks>
        /// <param name="name">The name of the track.</param>
        /// <param name="binding">The binding to use for the new track.</param>
        /// <param name="alignTime">The timecode (expressed in seconds) of first recorded sample (optional).</param>
        /// <typeparam name="T">The type of track being created. The track type must be derived from TrackAsset.</typeparam>
        /// <returns>Returns the created track.</returns>
        T CreateTrack<T>(string name, ITakeBinding binding, double? alignTime = null) where T : TrackAsset, new();
    }
}
