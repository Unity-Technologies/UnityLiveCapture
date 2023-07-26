using System;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    static class EnumExtensions
    {
        /// <summary>
        /// Gets the display name of the enum value.
        /// </summary>
        /// <param name="value">The enum value to get the display name of.</param>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <returns>The display name of the enum.</returns>
        public static string GetDisplayName<T>(this T value) where T : Enum
        {
            var memberInfo = typeof(T).GetMember(value.ToString());

            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(InspectorNameAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((InspectorNameAttribute)attrs[0]).displayName;
                }
            }

            return value.ToString();
        }
    }
}
