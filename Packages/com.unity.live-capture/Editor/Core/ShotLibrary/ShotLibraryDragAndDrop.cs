using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    [InitializeOnLoad]
    static class ShotLibraryDragAndDrop
    {
        static ShotLibraryDragAndDrop()
        {
            SceneView.beforeSceneGui += HandleDragAndDrop;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
        }

        static Transform s_Parent;

        static void HierarchyWindowItemCallback(int pID, Rect pRect)
        {
            if (Event.current.type == EventType.Layout)
            {
                s_Parent = null;
            }

            if (pRect.Contains(Event.current.mousePosition))
            {
                var parentGO = EditorUtility.InstanceIDToObject(pID) as GameObject;

                if (parentGO != null)
                {
                    s_Parent = parentGO.transform;
                }
            }

            HandleDragAndDrop();
        }

        static void HandleDragAndDrop(SceneView sceneView)
        {
            HandleDragAndDrop();
        }

        static void HandleDragAndDrop()
        {
            var isDragValid = false;

            switch (Event.current.type)
            {
                case EventType.DragExited:
                case EventType.DragUpdated:

                    isDragValid = IsDraggingAny<ShotLibrary>();

                    break;

                case EventType.DragPerform:

                    isDragValid = IsDraggingAny<ShotLibrary>();

                    foreach (var asset in DragAndDrop.objectReferences)
                    {
                        if (asset is ShotLibrary library)
                        {
                            var go = new GameObject("New ShotPlayer", typeof(ShotPlayer));
                            var shotPlayer = go.GetComponent<ShotPlayer>();

                            shotPlayer.ShotLibrary = library;
                            shotPlayer.Selection = 0;

                            Selection.activeGameObject = go;

                            if (s_Parent != null)
                            {
                                Undo.SetTransformParent(go.transform, s_Parent, "Drag and Drop");
                            }

                            Undo.RegisterCreatedObjectUndo(go, "Drag and drop");
                            EditorApplication.QueuePlayerLoopUpdate();
                        }
                    }

                    break;
            }

            if (isDragValid)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
        }

        static bool IsDraggingAny<T>()
        {
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is T)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
