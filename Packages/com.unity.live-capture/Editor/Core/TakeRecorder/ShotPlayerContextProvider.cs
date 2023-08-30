using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    class ShotPlayerContextProvider : TakeRecorderContextProvider
    {
        static class Contents
        {
            public static readonly GUIContent NoShotPlayer = EditorGUIUtility.TrTextContent("None", $"Select a {nameof(ShotPlayer)} to use for recording takes.");
        }

        const string k_DisplayName = "Shot Library";
        const string k_Invalid = "Invalid";

        [NonSerialized]
        int m_Version;
        [SerializeField]
        int m_Selection;
        List<ShotPlayerContext> m_Contexts = new List<ShotPlayerContext>();

        public int Selection
        {
            get => m_Selection;
            set => UpdateSelection(value);
        }

        public ShotPlayerContextProvider() : base(k_DisplayName)
        {
        }

        public override void OnNoContextGUI()
        {
            ShotPlayerContextEditor.DoCreateShotLibraryButton((library) =>
            {
                var shotPlayerGO = new GameObject($"{library.name} player", typeof(ShotPlayer));
                var shotPlayer = shotPlayerGO.GetComponent<ShotPlayer>();

                shotPlayer.ShotLibrary = library;
                shotPlayer.Selection = 0;

                EditorUtility.SetDirty(shotPlayer);
            });
        }

        public override void OnToolbarGUI(Rect rect)
        {
            DoDropdown(rect);
        }

        public override ITakeRecorderContext GetContext()
        {
            TryGetSelectedContext(out var context);

            return context;
        }

        public override void Update()
        {
            ShotPlayerListChangeCheck();
        }

        void ShotPlayerListChangeCheck()
        {
            if (m_Version != ShotPlayer.Version)
            {
                m_Version = ShotPlayer.Version;

                Rebuild();
                TakeRecorderWindow.RepaintWindow();
            }
        }

        void Rebuild()
        {
            m_Contexts.Clear();

            foreach (var shotPlayer in ShotPlayer.Instances)
            {
                m_Contexts.Add(new ShotPlayerContext(shotPlayer));
            }

            m_Contexts.Sort((a, b) => string.Compare(GetName(a), GetName(b)));

            m_Selection = Mathf.Clamp(m_Selection, 0, m_Contexts.Count - 1);
        }

        static string GetName(ShotPlayerContext context)
        {
            // Need to check for null as exiting playmode will throw nullrefs.
            if (context.ShotPlayer != null)
            {
                return context.ShotPlayer.gameObject.name;
            }
            else
            {
                return k_Invalid;
            }
        }

        bool TryGetSelectedContext(out ShotPlayerContext context)
        {
            context = null;

            if (m_Selection >= 0 && m_Selection < m_Contexts.Count)
            {
                context = m_Contexts[m_Selection];
            }

            return context != null;
        }

        void DoDropdown(Rect rect)
        {
            var content = Contents.NoShotPlayer;

            if (TryGetSelectedContext(out var context))
            {
                content = new GUIContent(GetName(context));
            }

            if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                var formatter = new UniqueNameFormatter();

                for (var i = 0; i < m_Contexts.Count; ++i)
                {
                    var name = formatter.Format(GetName(m_Contexts[i]));

                    menu.AddItem(new GUIContent(name), i == m_Selection, OnSelect, i);
                }

                menu.ShowAsContext();
            }
        }

        void OnSelect(object obj)
        {
            UpdateSelection((int)obj);
        }

        void UpdateSelection(int selection)
        {
            m_Selection = selection;
        }
    }
}
