using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A property drawer for flags enums. Does not block the main thread.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagButtonGroupAttribute))]
    sealed class EnumFlagButtonGroupAttributeDrawer : EnumButtonGroupAttributeDrawer
    {
        /// <inheritdoc />
        protected override List<EnumValue> GetDisplayedEnumValues(SerializedProperty property)
        {
            k_TmpDisplayedEnumValues.Clear();

            var enumNames = property.enumNames;
            var enumDisplayNames = property.enumDisplayNames;

            var startIndex = 0;

            // Optionally, there may be an *All* OR *Everything* flag.
            {
                var allIndex = Array.IndexOf(property.enumNames, k_All);
                var everyThingIndex = Array.IndexOf(property.enumNames, k_Everything);
                if (allIndex != -1 && everyThingIndex != -1)
                {
                    throw new InvalidOperationException($"Flags enum {property.name}: cannot have both {k_All} and {k_Everything} values.");
                }

                allIndex = Mathf.Max(allIndex, everyThingIndex);
                if (allIndex != -1)
                {
                    if (allIndex != startIndex)
                    {
                        var name = everyThingIndex == -1 ? k_All : k_Everything;
                        throw new InvalidOperationException($"Flags enum {property.name}: {name} value is incorrect.");
                    }

                    ++startIndex; // Pass All.
                }
            }

            // Then the None value is mandatory.
            {
                var noneIndex = Array.IndexOf(property.enumNames, k_None);
                if (noneIndex != startIndex)
                {
                    throw new InvalidOperationException($"Flags enum {property.name}: first value must be {k_None}.");
                }

                ++startIndex;
            }

            for (var i = startIndex; i != enumNames.Length; ++i)
            {
                k_TmpDisplayedEnumValues.Add(new EnumValue
                {
                    Name = enumNames[i],
                    DisplayName = enumDisplayNames[i],
                    Index = i - startIndex,
                    Selected = IsSelected(property.intValue, i - startIndex)
                });
            }

            return k_TmpDisplayedEnumValues;
        }

        /// <inheritdoc />
        protected override void UpdatePropertyValue(SerializedProperty property, List<EnumValue> enumValues)
        {
            var intValue = 0;

            for (var i = 0; i != enumValues.Count; ++i)
            {
                var entry = enumValues[i];
                if (entry.NewSelected)
                {
                    intValue |= 1 << entry.Index;
                }
            }

            property.intValue = intValue;
        }

        /// <inheritdoc />
        protected override bool IsSelected(int intValue, int index)
        {
            var flagValue = 1 << index;
            return (intValue & flagValue) > 0;
        }
    }
}
