using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Unity.LiveCapture.Editor
{
    static class ObjectCreatorUtilities
    {

        [MenuItem("GameObject/Live Capture/Shot Player", isValidateFunction: false, priority: 5)]
        public static GameObject CreateShotPlayer()
        {
            var name = "Shot Player";
            var undoName = "Create Shot Player";
            var selectedTransform = Selection.activeTransform;
            var go = new GameObject(name, typeof(ShotPlayer));

            StageUtility.PlaceGameObjectInCurrentStage(go);
            Undo.RegisterCreatedObjectUndo(go, undoName);

            if (selectedTransform != null)
            {
                Undo.SetTransformParent(go.transform, selectedTransform, undoName);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(go);

            Selection.activeGameObject = go;

            return go;
        }

        [MenuItem("GameObject/Live Capture/Timecode Synchronizer", isValidateFunction: false, priority: 10)]
        public static GameObject CreateSynchronizer()
        {
            var name = "Timecode Synchronizer";
            var undoName = "Create Timecode Synchronizer";
            var selectedTransform = Selection.activeTransform;
            var go = new GameObject(name, typeof(SynchronizerComponent));

            StageUtility.PlaceGameObjectInCurrentStage(go);
            Undo.RegisterCreatedObjectUndo(go, undoName);

            if (selectedTransform != null)
            {
                Undo.SetTransformParent(go.transform, selectedTransform, undoName);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(go);

            Selection.activeGameObject = go;

            return go;
        }
    }
}
