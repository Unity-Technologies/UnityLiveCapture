namespace Unity.TouchFramework
{
    /// <summary>
    /// Returns string format for 2 decimals.
    /// </summary>
    internal class TwoDecimalsFormat : IFloatFormat
    {
        public string GetStringFormat(float value)
        {
            return "F2";
        }
    }
}
