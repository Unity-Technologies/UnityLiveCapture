using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// The struct that stores the details of the video stream.
    /// </summary>
    [Serializable]
    struct VideoStreamState : IEquatable<VideoStreamState>
    {
        /// <summary>
        /// Is the video streaming server active.
        /// </summary>
        public bool IsRunning;

        /// <summary>
        /// The port the video streaming server is listening on.
        /// </summary>
        public int Port;

        /// <inheritdoc/>
        public bool Equals(VideoStreamState other)
        {
            return IsRunning == other.IsRunning && Port == other.Port;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current VideoStreamState.
        /// </summary>
        /// <param name="obj">The object to compare with the current VideoStreamState.</param>
        /// <returns>
        /// true if the specified object is equal to the current VideoStreamState; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is VideoStreamState other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the VideoStreamState.
        /// </summary>
        /// <returns>
        /// The hash value generated for this VideoStreamState.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IsRunning.GetHashCode();
                hashCode = (hashCode * 397) ^ Port.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current VideoStreamState.
        /// </summary>
        /// <returns>
        /// A string that represents the current VideoStreamState.
        /// </returns>
        public override string ToString()
        {
            return $"(IsRunning {IsRunning}, Port {Port})";
        }

        /// <summary>
        /// Determines whether the two specified VideoStreamState are equal.
        /// </summary>
        /// <param name="a">The first VideoStreamState.</param>
        /// <param name="b">The second VideoStreamState.</param>
        /// <returns>
        /// true if the specified VideoStreamState are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(VideoStreamState a, VideoStreamState b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified VideoStreamState are different.
        /// </summary>
        /// <param name="a">The first VideoStreamState.</param>
        /// <param name="b">The second VideoStreamState.</param>
        /// <returns>
        /// true if the specified VideoStreamState are different; otherwise, false.
        /// </returns>
        public static bool operator !=(VideoStreamState a, VideoStreamState b)
        {
            return !(a == b);
        }
    }
}
