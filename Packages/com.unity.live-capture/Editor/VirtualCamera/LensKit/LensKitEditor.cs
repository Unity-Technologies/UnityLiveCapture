using System;
using Unity.LiveCapture.Editor;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LensKit))]
    class LensKitEditor : Editor
    {
        static class Contents
        {
            public static string CreateLens = EditorGUIUtility.TrTextContent("Create Lens").text;
            public static string Unnamed = EditorGUIUtility.TrTextContent("Unnamed").text;
            public static string LockLens = EditorGUIUtility.TrTextContent("Lens Lock").text;
            public static string Reset = EditorGUIUtility.TrTextContent("Reset").text;
            public static GUIContent LensesLabel = EditorGUIUtility.TrTextContent("Lenses", "The list of available Lens Assets in the Lens Kit.");
            public static GUIContent ResetLabel = EditorGUIUtility.TrTextContent("Reset", "Reset the Lens Asset to defaults.");
            public static GUIStyle LockStyle = "IN LockButton";
        }

        CompactList m_List;
        SerializedProperty m_Lenses;
        LensKit m_LensKit;
        Editor m_Editor;

        void OnEnable()
        {
            m_LensKit = target as LensKit;
            m_Lenses = serializedObject.FindProperty("m_Lenses");

            CreateList();
        }

        void OnDisable()
        {
            if (m_Editor != null)
            {
                DestroyImmediate(m_Editor);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(Contents.LensesLabel);
            m_List.DoGUILayout();

            DoElementEditor();

            serializedObject.ApplyModifiedProperties();
        }

        void DoElementEditor()
        {
            var index = m_List.Index;

            if (index < 0 || index >= m_Lenses.arraySize)
            {
                return;
            }

            var element = m_Lenses.GetArrayElementAtIndex(index);
            var obj = element.objectReferenceValue;

            if (obj == null)
            {
                return;
            }

            CreateCachedEditor(obj, null, ref m_Editor);

            var notEditable = obj.hideFlags.HasFlag(HideFlags.NotEditable);

            using (new EditorGUI.DisabledScope(notEditable))
            {
                m_Editor.OnInspectorGUI();
            }
        }

        void CreateList()
        {
            m_List = new CompactList(m_Lenses);
            m_List.OnCanAddCallback = () => true;
            m_List.OnCanRemoveCallback = () => true;
            m_List.Reorderable = true;
            m_List.ShowSearchBar = false;
            m_List.OnMenuCallback = (menu) =>
            {
                var selectedLens = GetSelectedLens();

                menu.AddItem(Contents.ResetLabel, false, (l) => ResetLens(l as LensAsset), selectedLens);
            };
            m_List.DrawListItemCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_Lenses.GetArrayElementAtIndex(index);
                var obj = element.objectReferenceValue;

                rect.height = EditorGUIUtility.singleLineHeight;

                if (obj != null)
                {
                    EditorGUI.LabelField(rect, GetObjectName(obj));
                }
            };
            m_List.OnAddDropdownCallback = (Rect buttonRect) =>
            {
                DoAddButton();
            };
            m_List.OnRemoveCallback = () =>
            {
                DoRemoveButton();
            };
            m_List.DrawElementCallback = (Rect rect, int index) =>
            {
                var element = m_Lenses.GetArrayElementAtIndex(index);
                var obj = element.objectReferenceValue;

                if (obj != null)
                {
                    var buttonRect = new Rect(rect.xMax - 15f, rect.y, 15f, rect.height);

                    EditorGUI.LabelField(rect, GetObjectName(obj), EditorStyles.boldLabel);

                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var hideFlags = obj.hideFlags;
                        var notEditable = hideFlags.HasFlag(HideFlags.NotEditable);

                        notEditable = GUI.Toggle(buttonRect, notEditable, GUIContent.none, Contents.LockStyle);

                        if (change.changed)
                        {
                            if (notEditable)
                            {
                                hideFlags |= HideFlags.NotEditable;
                            }
                            else
                            {
                                hideFlags &= ~HideFlags.NotEditable;
                            }

                            Undo.RegisterCompleteObjectUndo(obj, Contents.LockLens);

                            obj.hideFlags = hideFlags;

                            EditorUtility.SetDirty(obj);
                        }
                    }
                }
            };
        }

        void ResetLens(LensAsset lens)
        {
            if (lens == null)
            {
                return;
            }

            var notEditable = lens.hideFlags.HasFlag(HideFlags.NotEditable);

            if (notEditable)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(lens, Contents.Reset);

            var name = lens.name;

            Unsupported.SmartReset(lens);

            lens.name = name;

            EditorUtility.SetDirty(lens);
        }

        string GetObjectName(UnityObject obj)
        {
            Debug.Assert(obj != null);

            var name = obj.name;

            if (string.IsNullOrEmpty(name))
            {
                name = Contents.Unnamed;
            }

            return name;
        }

        LensAsset GetSelectedLens()
        {
            var lens = default(LensAsset);
            var index = m_List.Index;

            if (index >= 0 && index < m_Lenses.arraySize)
            {
                lens = m_Lenses.GetArrayElementAtIndex(index).objectReferenceValue as LensAsset;
            }

            return lens;
        }

        void DoAddButton()
        {
            var lens = ScriptableObject.CreateInstance<LensAsset>();
            var selectedLens = GetSelectedLens();

            if (selectedLens != null)
            {
                EditorUtility.CopySerialized(selectedLens, lens);
            }
            else
            {
                lens.name = LensAsset.GenerateName(lens);
            }

            lens.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(lens, AssetDatabase.GetAssetPath(m_LensKit));

            Undo.RegisterCreatedObjectUndo(lens, Contents.CreateLens);

            serializedObject.Update();

            m_Lenses.arraySize++;

            var element = m_Lenses.GetArrayElementAtIndex(m_Lenses.arraySize - 1);

            element.objectReferenceValue = lens;

            serializedObject.ApplyModifiedProperties();
        }

        void DoRemoveButton()
        {
            var index = m_List.Index;

            if (index == -1)
            {
                return;
            }

            var lens = m_Lenses.GetArrayElementAtIndex(index).objectReferenceValue;

            serializedObject.Update();

            m_Lenses.DeleteArrayElementAtIndex(index);
            m_Lenses.DeleteArrayElementAtIndex(index);

            serializedObject.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(lens);
        }
    }
}
