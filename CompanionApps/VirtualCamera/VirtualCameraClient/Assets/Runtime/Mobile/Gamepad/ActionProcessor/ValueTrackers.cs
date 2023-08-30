using System;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Smoothly increases/decreases a server-controlled value.
    /// </para>
    ///
    /// <para>
    /// When the user starts modifying the value, the tracker caches the server
    /// value. The modifications are applied to this local copy (which in turn
    /// gets sent to the server). This avoids loopback issues from always operating
    /// on values received from the server.
    /// </para>
    /// </summary>
    abstract class ValueTracker<T> : IValueTracker<T>
    {
        protected T m_Current;
        protected bool m_Changing;
        protected IScaler<T> m_Scaler;

        /// <inheritdoc/>
        public T Current => m_Scaler.ToValueScale(m_Current);

        /// <inheritdoc/>
        public bool Changing => m_Changing;

        /// <summary>
        /// Performs the mapping between the input and value scales.
        /// </summary>
        public IScaler<T> Scaler
        {
            get => m_Scaler;
            set => m_Scaler = value;
        }

        /// <summary>
        /// This function should be called every frame to modify the value with a specified speed.
        /// </summary>
        /// <param name="value">The last value received from the server.</param>
        /// <param name="speed">Amount to increase/decrease the value per second (in the <see cref="Scaler"/>'s input range).</param>
        /// <param name="deltaTime">Time in seconds since the last call of this function.</param>
        public void Update(T value, float speed, float deltaTime)
        {
            Debug.Assert(m_Scaler != null);

            var newChanging = !Mathf.Approximately(speed, 0.0f);
            if (newChanging != m_Changing)
            {
                m_Changing = newChanging;

                if (m_Changing)
                    m_Current = m_Scaler.ToInputScale(value);
            }

            if (m_Changing)
                Add(speed * deltaTime);

            m_Current = m_Scaler.ClampToInputScale(m_Current);
        }

        /// <summary>
        /// Type-specific increment/decrement operation on <see cref="m_Current"/>.
        /// </summary>
        protected virtual void Add(float toAdd)
        {
            throw new NotImplementedException();
        }
    }

    class FloatValueTracker : ValueTracker<float>
    {
        /// <inheritdoc/>
        protected override void Add(float toAdd)
        {
            m_Current += toAdd;
        }
    }

    class DoubleValueTracker : ValueTracker<double>
    {
        /// <inheritdoc/>
        protected override void Add(float toAdd)
        {
            m_Current += toAdd;
        }
    }
}
