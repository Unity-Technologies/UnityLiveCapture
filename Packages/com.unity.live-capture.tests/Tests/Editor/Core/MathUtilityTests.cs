using NUnit.Framework;

namespace Unity.LiveCapture.Tests.Editor
{
    public class MathUtilityTests
    {
        [TestCase(0d, 0d, 1d, ExpectedResult = 0d)]
        [TestCase(-1d, 0d, 1d, ExpectedResult = 0d)]
        [TestCase(0d, -1d, 1d, ExpectedResult = 0d)]
        [TestCase(0d, 0d, -1d, ExpectedResult = 0d)]
        [TestCase(3d, 0d, -1d, ExpectedResult = 0d)]
        [TestCase(3d, 0d, 1d, ExpectedResult = 1d)]
        public double Clamp(double value, double min, double max)
        {
            return MathUtility.Clamp(value, min, max);
        }
    } 
}
