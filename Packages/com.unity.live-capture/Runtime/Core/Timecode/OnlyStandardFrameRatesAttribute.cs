using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An attribute placed on serialized <see cref="FrameRate"/> fields to constrain the values the user can
    /// select in the inspector to <see cref="StandardFrameRate"/> values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OnlyStandardFrameRatesAttribute : PropertyAttribute
    {
    }
}
