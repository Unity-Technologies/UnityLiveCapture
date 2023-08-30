using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    class TakeBindingsEditor : UnityEditor.Editor
    {
        internal static class Contents
        {
            public const string UndoSetBinding = "Inspector";
            public static readonly GUIContent BindingsLabel = EditorGUIUtility.TrTextContent("Bindings", "The list of scene objects referenced by the Take.");
            public static readonly string NullBindingsMsg = EditorGUIUtility.TrTextContent("Missing scene bindings. Set all the required object references in the Bindings list to play this take.").text;
        }

        SerializedProperty m_Bindings;
        SerializedProperty m_Entries;
        Take m_Take;
        HashSet<string> m_BindingSet = new HashSet<string>();

        void OnEnable()
        {
            m_Bindings = serializedObject.FindProperty("m_Bindings");
            m_Entries = serializedObject.FindProperty("m_Entries");
            m_Take = target as Take;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var resolver = serializedObject.context as IExposedPropertyTable;

            if (resolver != null)
            {
                DoBindingWarning(resolver);
            }

            m_Entries.isExpanded = EditorGUILayout.Foldout(m_Entries.isExpanded, Contents.BindingsLabel, true);

            if (m_Entries.isExpanded)
            {
                DoBindingsGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DoBindingWarning(IExposedPropertyTable resolver)
        {
            if (ContainsNullBindings(m_Take, resolver))
            {
                EditorGUILayout.HelpBox(Contents.NullBindingsMsg, MessageType.Warning, true);
            }
        }

        internal static bool ContainsNullBindings(Take take, IExposedPropertyTable resolver)
        {
            Debug.Assert(resolver != null);

            var serializedObject = new SerializedObject(take);
            var entries = serializedObject.FindProperty("m_Entries");
            var bindings = serializedObject.FindProperty("m_Bindings");

            try
            {
                for (var i = 0; i < entries.arraySize; ++i)
                {
                    var entry = entries.GetArrayElementAtIndex(i);
                    var binding = entry.FindPropertyRelative("m_Binding");

                    if (CheckIsBindingNull(binding, resolver))
                    {
                        return true;
                    }
                }

                for (var i = 0; i < bindings.arraySize; ++i)
                {
                    var binding = bindings.GetArrayElementAtIndex(i);

                    if (CheckIsBindingNull(binding, resolver))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                serializedObject.Dispose();
            }


            return false;
        }

        static bool CheckIsBindingNull(SerializedProperty binding, IExposedPropertyTable resolver)
        {
            Debug.Assert(resolver != null);

            var exposedPropertyName = binding.FindPropertyRelative("m_ExposedReference.exposedName");
            var name = exposedPropertyName.stringValue;
            var propertyName = new PropertyName(name);
            var value = resolver.GetReferenceValue(propertyName, out var _);

            return value == null;
        }

        void DoBindingsGUI()
        {
            m_BindingSet.Clear();

            for (var i = 0; i < m_Entries.arraySize; ++i)
            {
                var entry = m_Entries.GetArrayElementAtIndex(i);
                var binding = entry.FindPropertyRelative("m_Binding");

                DoBindingGUI(binding);
            }

            for (var i = 0; i < m_Bindings.arraySize; ++i)
            {
                var binding = m_Bindings.GetArrayElementAtIndex(i);

                DoBindingGUI(binding);
            }
        }

        void DoBindingGUI(SerializedProperty binding)
        {
            var name = binding.FindPropertyRelative("m_ExposedReference.exposedName").stringValue;

            if (m_BindingSet.Contains(name))
            {
                return;
            }

            m_BindingSet.Add(name);

            EditorGUILayout.PropertyField(binding);
        }
    }
}
