namespace Unity.TouchFramework
{
    /// <summary>
    /// Returns string format for 2 decimals when less than 1.
    /// </summary>
    public class TwoDecimalsLessThanOneFormat : IFloatFormat
    {
        public string GetStringFormat(float value)
        {
            return value < 1 ? "F2" : "F0";
        }
    }
}
