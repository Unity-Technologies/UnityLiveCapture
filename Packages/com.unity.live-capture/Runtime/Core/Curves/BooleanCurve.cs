using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A type of <see cref="ICurve"/> that stores keyframes of type bool.
    /// </summary>
    public class BooleanCurve : ICurve<bool>
    {
        readonly IntegerCurve m_Curve = new IntegerCurve();

        /// <inheritdoc/>
        public FrameRate FrameRate
        {
            get => m_Curve.FrameRate;
            set => m_Curve.FrameRate = value;
        }

        /// <inheritdoc/>
        public void AddKey(double time, in bool value)
        {
            m_Curve.AddKey(time, value ? 1 : 0);
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return m_Curve.IsEmpty();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            m_Curve.Clear();
        }

        /// <inheritdoc/>
        public void SetToAnimationClip(PropertyBinding binding, AnimationClip clip)
        {
            m_Curve.SetToAnimationClip(binding, clip);
        }
    }
}
