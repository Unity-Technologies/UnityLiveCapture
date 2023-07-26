using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    [CustomPropertyDrawer(typeof(TakeBinding<>), true)]
    class TakeBindingDrawer : PropertyDrawer
    {
        static class Contents
        {
            public const string UndoSetBinding = "Inspector";
            public static readonly string BindingTooltip = L10n.Tr("The name to use when resolving the object reference.");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            var serializedObject = property.serializedObject;
            var resolver = serializedObject.context as IExposedPropertyTable;
            var exposedPropertyName = property.FindPropertyRelative("m_ExposedReference.exposedName");
            var exposedNameStr = exposedPropertyName.stringValue;
            var propertyName = new PropertyName(exposedNameStr);
            var value = default(UnityEngine.Object);

            if (resolver != null)
            {
                value = resolver.GetReferenceValue(propertyName, out var _);
            }

            var propertyType = typeof(UnityEngine.Object);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
#if UNITY_2021_2_OR_NEWER
                propertyType = (property.managedReferenceValue as ITakeBinding).Type;
#else
                var assemblyAndType = property.managedReferenceFullTypename.Split(' ');
                var fullTypeName = $"{assemblyAndType[1]}, {assemblyAndType[0]}";
                var managedReferenceType = Type.GetType(fullTypeName).BaseType;

                propertyType = managedReferenceType.GenericTypeArguments[0];
#endif
            }
            else
            {
                var type = fieldInfo.FieldType;
                var isListOrArray = typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType);

                if (isListOrArray)
                {
                    type = type.GetElementType();
                }

                propertyType = type.BaseType.GetGenericArguments()[0];
            }

            using (new EditorGUI.DisabledScope(resolver == null))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var content = new GUIContent(exposedNameStr, Contents.BindingTooltip);
                var newValue = EditorGUI.ObjectField(position, content, value, propertyType, true);

                if (change.changed)
                {
                    Undo.RecordObject(serializedObject.context, Contents.UndoSetBinding);

                    resolver.SetReferenceValue(propertyName, newValue);

                    EditorUtility.SetDirty(serializedObject.context);
                }
            }
        }
    }
}
