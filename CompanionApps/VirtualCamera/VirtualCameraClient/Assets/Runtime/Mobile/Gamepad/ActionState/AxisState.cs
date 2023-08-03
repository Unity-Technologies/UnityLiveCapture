namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Combines the positive and negative inputs of an axis specified by <see cref="AxisID"/>.
    /// The value of the axis is the difference of the positive and negative inputs.
    /// </summary>
    class AxisState
    {
        public enum Direction
        {
            Positive,
            Negative
        }

        readonly AxisID m_ID;
        float m_PositiveValue;
        float m_NegativeValue;

        public AxisID ID => m_ID;

        public AxisState(AxisID ID)
        {
            m_ID = ID;
        }

        /// <summary>
        /// Updates an input and returns the new axis value.
        /// </summary>
        /// <param name="direction">The input to update.</param>
        /// <param name="directionValue">The new value of the input.</param>
        /// <returns>The value of the axis.</returns>
        public float GetUpdatedValue(Direction direction, float directionValue)
        {
            if (direction == Direction.Positive)
                m_PositiveValue = directionValue;
            else if (direction == Direction.Negative)
                m_NegativeValue = directionValue;

            return m_PositiveValue - m_NegativeValue;
        }

        public void Reset()
        {
            m_PositiveValue = 0f;
            m_NegativeValue = 0f;
        }
    }
}
