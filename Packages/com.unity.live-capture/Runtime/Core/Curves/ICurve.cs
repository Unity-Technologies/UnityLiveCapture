using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents an animation curve.
    /// </summary>
    public interface ICurve
    {
        /// <summary>
        /// The sampling rate in Hz.
        /// </summary>
        FrameRate FrameRate { get; set; }

        /// <summary>
        /// Checks if the animation curve contains keyframes.
        /// </summary>
        /// <returns>true if the curve contains no keyframes; otherwise, false.</returns>
        bool IsEmpty();

        /// <summary>
        /// Clears the keyframes from the curve.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets the curve to the given animation clip.
        /// </summary>
        /// <param name="binding">The binding to use for animation.</param>
        /// <param name="clip">The animation clip to set the curve to.</param>
        void SetToAnimationClip(PropertyBinding binding, AnimationClip clip);
    }

    /// <summary>
    /// Represents an animation curve that stores keyframes of generic value.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the curve.</typeparam>
    public interface ICurve<T> : ICurve where T : struct
    {
        /// <summary>
        /// Adds a keyframe to the curve.
        /// </summary>
        /// <param name="time">The time in seconds to insert the keyframe at.</param>
        /// <param name="value">The keyframe value.</param>
        void AddKey(double time, in T value);
    }

    /// <summary>
    /// Represents an animation curve that can reduce the number of stored keyframes
    /// by tolerating certain amount of error.
    /// </summary>
    interface IReduceableCurve : ICurve
    {
        /// <summary>
        /// The tolerance allowed when simplifying the curve.
        /// </summary>
        float MaxError { get; set; }
    }
}
