using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class Snapshot : IEquatable<Snapshot>
    {
        [SerializeField]
        Pose m_Pose;
        [SerializeField]
        LensAsset m_LensAsset;
        [SerializeField]
        Lens m_Lens;
        [SerializeField]
        CameraBody m_CameraBody;
        [SerializeField]
        Texture2D m_Screenshot;
        [SerializeField]
        PlayableAsset m_Slate;
        [SerializeField]
        FrameRate m_FrameRate;
        [SerializeField]
        double m_Time;

        public Pose Pose
        {
            get => m_Pose;
            set => m_Pose = value;
        }

        public LensAsset LensAsset
        {
            get => m_LensAsset;
            set => m_LensAsset = value;
        }

        public Lens Lens
        {
            get => m_Lens;
            set => m_Lens = value;
        }

        public CameraBody CameraBody
        {
            get => m_CameraBody;
            set => m_CameraBody = value;
        }

        public Texture2D Screenshot
        {
            get => m_Screenshot;
            set => m_Screenshot = value;
        }

        public UnityObject Asset
        {
            get => m_Slate;
            set => m_Slate = value as PlayableAsset;
        }

        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set => m_FrameRate = value;
        }

        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }

        /// <inheritdoc/>
        public bool Equals(Snapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return m_Pose == other.m_Pose
                && m_LensAsset == other.m_LensAsset
                && m_Lens == other.m_Lens
                && m_CameraBody == other.m_CameraBody
                && m_Screenshot == other.m_Screenshot
                && m_Slate == other.m_Slate
                && m_FrameRate == other.m_FrameRate
                && m_Time == other.m_Time;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current Snapshot.
        /// </summary>
        /// <param name="obj">The object to compare with the current Snapshot.</param>
        /// <returns>
        /// true if the specified object is equal to the current Snapshot; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is Snapshot other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the Snapshot.
        /// </summary>
        /// <returns>
        /// The hash value generated for this Snapshot.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Pose.GetHashCode();
                hashCode = (hashCode * 397) ^ (m_LensAsset == null ? 0 : m_LensAsset.GetHashCode());
                hashCode = (hashCode * 397) ^ m_Lens.GetHashCode();
                hashCode = (hashCode * 397) ^ m_CameraBody.GetHashCode();
                hashCode = (hashCode * 397) ^ (m_Screenshot == null ? 0 : m_Screenshot.GetHashCode());
                hashCode = (hashCode * 397) ^ (m_Slate == null ? 0 : m_Slate.GetHashCode());
                hashCode = (hashCode * 397) ^ m_FrameRate.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Time.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified Snapshot are equal.
        /// </summary>
        /// <param name="a">The first Snapshot.</param>
        /// <param name="b">The second Snapshot.</param>
        /// <returns>
        /// true if the specified Snapshot are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(Snapshot a, Snapshot b)
        {
            if (a is null)
            {
                return b is null;
            }
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified Snapshot are different.
        /// </summary>
        /// <param name="a">The first Snpashot.</param>
        /// <param name="b">The second Snpashot.</param>
        /// <returns>
        /// true if the specified Snpashot are different; otherwise, false.
        /// </returns>
        public static bool operator !=(Snapshot a, Snapshot b)
        {
            return !(a == b);
        }
    }
}
