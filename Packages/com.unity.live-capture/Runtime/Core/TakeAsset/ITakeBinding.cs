using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a binding between a name and an object that a take uses in the scene.
    /// </summary>
    /// <remarks>
    /// To play recorded takes, the PlayableDirector requires scene bindings for its tracks.
    /// Take bindings help prepare the PlayableDirector's bindings. With the take bindings stored
    /// in a take, the playback system can recover the scene object associated with a track.
    /// All the takes recorded with the same binding name resolve to the same scene object, which
    /// facilitates swapping takes for playback.
    /// </remarks>
    public interface ITakeBinding
    {
        /// <summary>
        /// The type of the value of the binding.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The PropertyName of the binding.
        /// </summary>
        PropertyName PropertyName { get; }

        /// <summary>
        /// Sets the name of the binding.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        void SetName(string name);

        /// <summary>
        /// Gets the resolved value of the binding.
        /// </summary>
        /// <param name="resolver">The resolve table.</param>
        /// <returns>
        /// The resolved object reference.
        /// </returns>
        UnityObject GetValue(IExposedPropertyTable resolver);

        /// <summary>
        /// Sets the value of the binding.
        /// </summary>
        /// <param name="value">The object reference to set.</param>
        /// <param name="resolver">The resolve table.</param>
        void SetValue(UnityObject value, IExposedPropertyTable resolver);

        /// <summary>
        /// Clears the value of the binding.
        /// </summary>
        /// <param name="resolver">The resolve table.</param>
        void ClearValue(IExposedPropertyTable resolver);
    }
}
