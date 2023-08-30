using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A class containing extension methods for <see cref="SerializedProperty"/>.
    /// </summary>
    static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Gets the field value backing a serialized property.
        /// </summary>
        /// <remarks>
        /// This uses reflection to traverse the property path. This is quite expensive, so use sparingly if possible.
        /// If you need to support multi-object editing, use <see cref="GetValues"/> instead.
        /// </remarks>
        /// <param name="property">A serialized property.</param>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <returns>The property value, or default if the field value could not be reached or is not
        /// of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is null.</exception>
        public static T GetValue<T>(this SerializedProperty property)
        {
            return property.GetValue() is T value ? value : default;
        }

        /// <summary>
        /// Gets the field value backing a serialized property.
        /// </summary>
        /// <remarks>
        /// This uses reflection to traverse the property path. This is quite expensive, so use sparingly if possible.
        /// If you need to support multi-object editing, use <see cref="GetValues{T}"/> instead.
        /// </remarks>
        /// <param name="property">A serialized property.</param>
        /// <returns>The property value, or null if the field value could not be reached.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is null.</exception>
        public static object GetValue(this SerializedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // Make sure the serialized object is up to date before we retrieve any values from the underlying objects
            var serializedObject = property.serializedObject;
            serializedObject.ApplyModifiedProperties();

            // convert the property path in to a more workable format
            var path = FormatPath(property.propertyPath);

            return GetFieldOfProperty(serializedObject.targetObject, path, out _);
        }

        /// <summary>
        /// Gets the field values backing a serialized property.
        /// </summary>
        /// <remarks>
        /// This uses reflection to traverse the property path. This is quite expensive, so use sparingly if possible.
        /// </remarks>
        /// <param name="property">A serialized property.</param>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <returns>A new array containing the field values for all target instances. Values
        /// will be default if the field value could not be reached or are not of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is null.</exception>
        public static T[] GetValues<T>(this SerializedProperty property)
        {
            var values = property.GetValues();
            var typedValues = new T[values.Length];

            for (var i = 0; i < typedValues.Length; i++)
                typedValues[i] = values[i] is T value ? value : default;

            return typedValues;
        }

        /// <summary>
        /// Gets the field values backing a serialized property.
        /// </summary>
        /// <remarks>
        /// This uses reflection to traverse the property path. This is quite expensive, so use sparingly if possible.
        /// </remarks>
        /// <param name="property">A serialized property.</param>
        /// <returns>A new array containing the field values for all target instances. Values
        /// will be null if the field value could not be reached.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is null.</exception>
        public static object[] GetValues(this SerializedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // Make sure the serialized object is up to date before we retrieve any values from the underlying objects
            var serializedObject = property.serializedObject;
            serializedObject.ApplyModifiedProperties();

            // convert the property path in to a more workable format
            var path = FormatPath(property.propertyPath);

            // make sure to support multi-object editing by getting the instances for all selected objects
            var targets = serializedObject.targetObjects;
            var objects = new object[targets.Length];

            for (var i = 0; i < objects.Length; i++)
                objects[i] = GetFieldOfProperty(targets[i], path, out _);

            return objects;
        }

        /// <summary>
        /// Sets a serialized property to given value.
        /// </summary>
        /// <remarks>
        /// This uses reflection to traverse the property path. This is quite expensive, so use sparingly if possible.
        /// </remarks>
        /// <param name="property">A serialized property.</param>
        /// <param name="value">The value to apply to the property. The value object type must match property type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is null.</exception>
        public static void SetValue(this SerializedProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            Profiler.BeginSample($"{nameof(SerializedPropertyExtensions)}.{nameof(SetValue)}()");

            // If the value is null, we should create a default instance of the appropriate type
            // in order to match the normal serialization behaviour, but only for properties that
            // do not support null values.
            if (value == null)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                    case SerializedPropertyType.ExposedReference:
                    case SerializedPropertyType.ManagedReference:
                        break;

                    default:
                        var instance = property.serializedObject.targetObject;
                        var path = FormatPath(property.propertyPath);
                        GetFieldOfProperty(instance, path, out var type);

                        if (type.IsArray)
                        {
                            value = Array.CreateInstance(type, 0);
                        }
                        else
                        {
                            value = Activator.CreateInstance(type);
                        }
                        break;
                }
            }

            try
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        property.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.Integer:
                    {
                        switch (value)
                        {
                            case sbyte _:
                            case byte _:
                            case short _:
                            case ushort _:
                            case int _:
                            case uint _:
                                property.intValue = (int)value;
                                break;

                            case long _:
                            case ulong _:
                                property.longValue = (long)value;
                                break;
                        }

                        break;
                    }
                    case SerializedPropertyType.Float:
                    {
                        switch (value)
                        {
                            case float _:
                                property.floatValue = (float)value;
                                break;
                            case double _:
                                property.doubleValue = (double)value;
                                break;
                        }

                        break;
                    }
                    case SerializedPropertyType.Vector2Int:
                        property.vector2IntValue = (Vector2Int)value;
                        break;
                    case SerializedPropertyType.Vector3Int:
                        property.vector3IntValue = (Vector3Int)value;
                        break;
                    case SerializedPropertyType.RectInt:
                        property.rectIntValue = (RectInt)value;
                        break;
                    case SerializedPropertyType.BoundsInt:
                        property.boundsIntValue = (BoundsInt)value;
                        break;
                    case SerializedPropertyType.Vector2:
                        property.vector2Value = (Vector2)value;
                        break;
                    case SerializedPropertyType.Vector3:
                        property.vector3Value = (Vector3)value;
                        break;
                    case SerializedPropertyType.Vector4:
                        property.vector4Value = (Vector4)value;
                        break;
                    case SerializedPropertyType.Rect:
                        property.rectValue = (Rect)value;
                        break;
                    case SerializedPropertyType.Bounds:
                        property.boundsValue = (Bounds)value;
                        break;
                    case SerializedPropertyType.Quaternion:
                        property.quaternionValue = (Quaternion)value;
                        break;
                    case SerializedPropertyType.Color:
                        property.colorValue = (Color)value;
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = (string)value;
                        break;
                    case SerializedPropertyType.Character:
                        property.intValue = (char)value;
                        break;
                    case SerializedPropertyType.Enum:
                        property.intValue = (int)value;
                        break;
                    case SerializedPropertyType.LayerMask:
                        property.intValue = ((LayerMask)value).value;
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        property.animationCurveValue = (AnimationCurve)value;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        property.objectReferenceValue = value as Object;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        property.exposedReferenceValue = value as Object;
                        break;
                    case SerializedPropertyType.ManagedReference:
                        property.managedReferenceValue = value;
                        break;
                    case SerializedPropertyType.Generic:
                    {
                        // generic is used for arrays, which need some special handling to iterate each element
                        if (property.isArray)
                        {
                            if (value == null)
                            {
                                property.arraySize = 0;
                                return;
                            }

                            switch (value)
                            {
                                case object[] array:
                                {
                                    property.arraySize = array.Length;

                                    for (var i = 0; i < array.Length; i++)
                                        property.GetArrayElementAtIndex(i).SetValue(array[i]);

                                    return;
                                }
                                case IList list:
                                {
                                    property.arraySize = list.Count;

                                    for (var i = 0; i < list.Count; i++)
                                        property.GetArrayElementAtIndex(i).SetValue(list[i]);

                                    return;
                                }
                                default:
                                    Debug.LogError($"Property type \"{property.propertyType}\" does not match instance type \"{value.GetType().FullName}\"!");
                                    return;
                            }
                        }

                        // For custom serializable types, we iterate the immediate child properties and use reflection to find
                        // the matching fields in the value object. Any deeper children are handled via recursion.
                        var iterator = property.Copy();
                        var end = property.Copy();
                        end.Next(false);

                        while (iterator.Next(SerializedProperty.EqualContents(iterator, property)) && !SerializedProperty.EqualContents(iterator, end))
                        {
                            var type = value.GetType();
                            var fieldType = FindField(type, iterator.name);

                            if (fieldType == null)
                            {
                                Debug.LogError($"Could not locate field named \"{iterator.name}\" in type \"{type.FullName}\"!");
                                continue;
                            }

                            var fieldValue = fieldType.GetValue(value);
                            iterator.SetValue(fieldValue);
                        }

                        break;
                    }
                    case SerializedPropertyType.Gradient:
                    // gradient does not have a public setter, and can only be set using painful amounts of reflection
                    case SerializedPropertyType.ArraySize:
                    // this function should not be called for array size properties as they are set when their parent array properties are encountered
                    case SerializedPropertyType.FixedBufferSize:
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (InvalidCastException)
            {
                Debug.LogError($"Property {property.propertyPath}, {property.propertyType}) does not match object type ({value.GetType().FullName})!");
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        static string[] FormatPath(string propertyPath)
        {
            var path = propertyPath.Replace(".Array.data", string.Empty);
            return path.Split('.');
        }

        static object GetFieldOfProperty(object instance, string[] path, out Type instanceType)
        {
            instanceType = null;

            for (var i = 0; i < path.Length; i++)
            {
                if (instance == null)
                {
                    Debug.LogError($"Field at path \"{string.Join(".", path.Take(i))}\" was unexpectedly null!");
                    instanceType = null;
                    return null;
                }

                // get the next field along the path
                var element = path[i];

                // make sure to get the index if a collection field
                var index = -1;
                var indexStart = element.IndexOf('[');

                if (indexStart >= 0)
                {
                    var indexEnd = element.IndexOf(']');
                    var indexStr = element.Substring(indexStart + 1, indexEnd - indexStart - 1);

                    element = element.Substring(0, indexStart);
                    index = Convert.ToInt32(indexStr);
                }

                // get the backing instance for the field on this object
                var type = instance.GetType();
                var fieldInfo = FindField(type, element);

                if (fieldInfo == null)
                {
                    Debug.LogError($"Could not locate field named \"{element}\" in type \"{type.FullName}\"!");
                    instanceType = null;
                    return null;
                }

                instanceType = fieldInfo.FieldType;
                instance = fieldInfo.GetValue(instance);

                // Check if this is a collection field. If so, we need to get the element at the correct index.
                if (index >= 0)
                {
                    switch (instance)
                    {
                        case object[] array:
                            instance = array[index];
                            break;
                        case IList list:
                            instance = list[index];
                            break;
                        default:
                            Debug.LogError($"Unsupported field type \"{fieldInfo.FieldType}\" at path \"{string.Join(".", path.Take(i + 1))}\"!");
                            return null;
                    }
                }
            }

            return instance;
        }

        static FieldInfo FindField(Type type, string fieldName)
        {
            // GetField does not find private fields in base classes of the given type, so we must search
            // all base classes until the field is found or not.
            var field = default(FieldInfo);

            for (; field == null && type != null; type = type.BaseType)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return field;
        }
    }
}
