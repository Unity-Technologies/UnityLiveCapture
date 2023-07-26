using UnityEngine;

namespace Unity.LiveCapture.Mocap
{
    /// <summary>
    /// Use this class to bake <see cref="Transform"/> keyframes into a take.
    /// </summary>
    class TransformCurve
    {
        readonly Vector3Curve m_Position = new Vector3Curve();
        readonly Vector3Curve m_Scale = new Vector3Curve();
        readonly EulerCurve m_Rotation = new EulerCurve();

        public string RelativePath { get; internal set; }

        /// <inheritdoc/>
        public FrameRate FrameRate
        {
            get => m_Position.FrameRate;
            set
            {
                m_Position.FrameRate = value;
                m_Rotation.FrameRate = value;
                m_Scale.FrameRate = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="TransformCurve"/> instance.
        /// </summary>
        /// <param name="relativePath">The path of the game object this curve applies to,
        /// relative to the game object the source component is attached to.</param>
        public TransformCurve(string relativePath)
        {
            RelativePath = relativePath;
        }

        /// <summary>
        /// Adds a keyframe to the curve.
        /// </summary>
        /// <param name="time">The time in seconds to insert the keyframe at.</param>
        /// <param name="position">The position to record.</param>
        /// <param name="rotation">The rotation to record.</param>
        /// <param name="scale">The scale to record.</param>
        public void AddKey(double time, Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            if (position.HasValue)
                m_Position.AddKey(time, position.Value);

            if (rotation.HasValue)
                m_Rotation.AddKey(time, rotation.Value);

            if (scale.HasValue)
                m_Scale.AddKey(time, scale.Value);
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return m_Position.IsEmpty() && m_Rotation.IsEmpty() && m_Scale.IsEmpty();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            m_Position.Clear();
            m_Rotation.Clear();
            m_Scale.Clear();
        }

        /// <inheritdoc/>
        public void SetToAnimationClip(AnimationClip clip)
        {
            if (!m_Position.IsEmpty())
                m_Position.SetToAnimationClip(new PropertyBinding(RelativePath, "m_LocalPosition", typeof(Transform)), clip);

            if (!m_Rotation.IsEmpty())
                m_Rotation.SetToAnimationClip(new PropertyBinding(RelativePath, "m_LocalEuler", typeof(Transform)), clip);

            if (!m_Scale.IsEmpty())
                m_Scale.SetToAnimationClip(new PropertyBinding(RelativePath, "m_LocalScale", typeof(Transform)), clip);
        }
    }
}
