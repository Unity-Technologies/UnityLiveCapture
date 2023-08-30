using System;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    [ContextEditor(typeof(ShotPlayerContext))]
    [Serializable]
    class ShotPlayerContextEditor : TakeRecorderContextEditor
    {
        static class Contents
        {
            public static readonly string CreateShotLibraryMessage = L10n.Tr("Create a Shot Library asset to start recording");
            public static readonly GUIContent CreateShotLibraryLabel = EditorGUIUtility.TrTextContent("Create Shot Library", "Create a Shot Library asset.");
            public static readonly GUILayoutOption[] LargeButtonOptions = { GUILayout.Height(30f) };
            public const string k_NewShotLibrary = "New shot library";
        }

        public override void OnInspectorGUI()
        {
            if (!Context.IsValid())
            {
                return;
            }

            var context = Context as ShotPlayerContext;

            if (context.ShotPlayer.ShotLibrary == null)
            {
                DoCreateShotLibraryButton((library) =>
                {
                    var context = Context as ShotPlayerContext;
                    var shotPlayer = context.ShotPlayer;

                    shotPlayer.ShotLibrary = library;
                    shotPlayer.Selection = 0;

                    EditorUtility.SetDirty(shotPlayer);
                });
            }
            else
            {
                DrawDefaultInspector();
            }
        }

        internal static void DoCreateShotLibraryButton(Action<ShotLibrary> onCreate)
        {
            EditorGUILayout.HelpBox(Contents.CreateShotLibraryMessage, MessageType.Info, true);

            if (GUILayout.Button(Contents.CreateShotLibraryLabel, Contents.LargeButtonOptions))
            {
                if (ShotPlayerEditor.TryCreateShotLibrary(Contents.k_NewShotLibrary, out var library))
                {
                    onCreate?.Invoke(library);
                }
            }
        }
    }
}
