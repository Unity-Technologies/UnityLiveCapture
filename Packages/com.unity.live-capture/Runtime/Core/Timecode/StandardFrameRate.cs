using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An enum that defines common frame rate values.
    /// </summary>
    public enum StandardFrameRate
    {
        /// <summary>
        /// 23.967 fps, defined as 24000 / 1001.
        /// </summary>
        [InspectorName("23.976 NDF")]
        FPS_23_976,

        /// <summary>
        /// 23.967 fps, defined as 24000 / 1001, with drop frames.
        /// </summary>
        [InspectorName("23.976 DF")]
        FPS_23_976_DF,

        /// <summary>
        /// 24 fps.
        /// </summary>
        [InspectorName("24")]
        FPS_24_00,

        /// <summary>
        /// 25 fps.
        /// </summary>
        [InspectorName("25")]
        FPS_25_00,

        /// <summary>
        /// 23.97 fps, defined as 30000 / 1001.
        /// </summary>
        [InspectorName("29.97 NDF")]
        FPS_29_97,

        /// <summary>
        /// 23.97 fps, defined as 30000 / 1001, with drop frames.
        /// </summary>
        [InspectorName("29.97 DF")]
        FPS_29_97_DF,

        /// <summary>
        /// 30 fps.
        /// </summary>
        [InspectorName("30")]
        FPS_30_00,

        /// <summary>
        /// 48 fps.
        /// </summary>
        [InspectorName("48")]
        FPS_48_00,

        /// <summary>
        /// 59.94 fps, defined as 60000 / 1001.
        /// </summary>
        [InspectorName("59.94 NDF")]
        FPS_59_94,

        /// <summary>
        /// 59.94 fps, defined as 60000 / 1001, with drop frames.
        /// </summary>
        [InspectorName("59.94 DF")]
        FPS_59_94_DF,

        /// <summary>
        /// 60 fps.
        /// </summary>
        [InspectorName("60")]
        FPS_60_00,
    }

    /// <summary>
    /// A class that contains extension methods for <see cref="StandardFrameRate"/>.
    /// </summary>
    public static class StandardFrameRateExtensions
    {
        /// <summary>
        /// Gets the <see cref="FrameRate"/> that corresponds to a <see cref="StandardFrameRate"/> value.
        /// </summary>
        /// <param name="rate">The standard frame rate.</param>
        /// <returns>The standard frame rate value.</returns>
        public static FrameRate ToValue(this StandardFrameRate rate)
        {
            return rate switch
            {
                StandardFrameRate.FPS_23_976 => new FrameRate(24000, 1001, false),
                StandardFrameRate.FPS_23_976_DF => new FrameRate(24000, 1001, true),
                StandardFrameRate.FPS_24_00 => new FrameRate(24),
                StandardFrameRate.FPS_25_00 => new FrameRate(25),
                StandardFrameRate.FPS_29_97 => new FrameRate(30000, 1001, false),
                StandardFrameRate.FPS_29_97_DF => new FrameRate(30000, 1001, true),
                StandardFrameRate.FPS_30_00 => new FrameRate(30),
                StandardFrameRate.FPS_48_00 => new FrameRate(48),
                StandardFrameRate.FPS_59_94 => new FrameRate(60000, 1001, false),
                StandardFrameRate.FPS_59_94_DF => new FrameRate(60000, 1001, true),
                StandardFrameRate.FPS_60_00 => new FrameRate(60),
                _ => throw new ArgumentOutOfRangeException(nameof(rate), rate, "No frame rate defined!")
            };
        }

        /// <summary>
        /// Gets the <see cref="StandardFrameRate"/> that corresponds to a <see cref="FrameRate"/> value.
        /// </summary>
        /// <param name="rate">A frame rate.</param>
        /// <param name="frameRate">The standard frame rate, or <see langword="default"/> if there is no corresponding standard frame rate.</param>
        /// <returns><see langword="true"/> if there is a <see cref="StandardFrameRate"/> value that corresponds to <paramref name="rate"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetStandardRate(this FrameRate rate, out StandardFrameRate frameRate)
        {
            if (rate.Denominator == 1001)
            {
                switch (rate.Numerator)
                {
                    case 24000:
                        frameRate = rate.IsDropFrame ? StandardFrameRate.FPS_23_976_DF : StandardFrameRate.FPS_23_976;
                        return true;
                    case 30000:
                        frameRate = rate.IsDropFrame ? StandardFrameRate.FPS_29_97_DF : StandardFrameRate.FPS_29_97;
                        return true;
                    case 60000:
                        frameRate = rate.IsDropFrame ? StandardFrameRate.FPS_59_94_DF : StandardFrameRate.FPS_59_94;
                        return true;
                }
            }
            else
            {
                switch (rate.Numerator)
                {
                    case 24:
                        frameRate = StandardFrameRate.FPS_24_00;
                        return true;
                    case 25:
                        frameRate = StandardFrameRate.FPS_25_00;
                        return true;
                    case 30:
                        frameRate = StandardFrameRate.FPS_30_00;
                        return true;
                    case 48:
                        frameRate = StandardFrameRate.FPS_48_00;
                        return true;
                    case 60:
                        frameRate = StandardFrameRate.FPS_60_00;
                        return true;
                }
            }

            frameRate = default;
            return false;
        }
    }
}
