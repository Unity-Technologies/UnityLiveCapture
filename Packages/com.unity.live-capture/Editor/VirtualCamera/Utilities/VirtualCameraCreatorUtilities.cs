using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    static class VirtualCameraCreatorUtilities
    {
        const string k_CreateObjectUndoNameFmt = "Create {0}";

        static string GetUndoName(string objName)
        {
            return string.Format(k_CreateObjectUndoNameFmt, objName);
        }

        /// <summary>
        /// Creates a new <see cref="VirtualCameraActor"/> with all the required components.
        /// </summary>
        [MenuItem("GameObject/Live Capture/Camera/Virtual Camera Actor", false, 10)]
        public static GameObject CreateVirtualCameraActor()
        {
            var go = CreateVirtualCameraActorInternal(Selection.activeTransform);

            Selection.activeObject = go;

            return go;
        }

        internal static GameObject CreateVirtualCameraActorInternal()
        {
            return CreateVirtualCameraActorInternal(null);
        }

        internal static GameObject CreateVirtualCameraActorInternal(Transform parent)
        {
            var root = new GameObject("Virtual Camera Actor",
                typeof(VirtualCameraActor),
                typeof(PhysicalCameraDriver),
                typeof(FrameLines));

            StageUtility.PlaceGameObjectInCurrentStage(root);
            Undo.RegisterCreatedObjectUndo(root, GetUndoName(root.name));

            if (parent != null)
            {
                Undo.SetTransformParent(root.transform, parent, GetUndoName(root.name));
            }

            GameObjectUtility.EnsureUniqueNameForSibling(root);

            var camera = root.GetComponent<Camera>();
            camera.usePhysicalProperties = true;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 1000;

#if HDRP_14_0_OR_NEWER
            ConfigureHDCamera(camera);
#endif

            MatchToSceneView(root.transform);

            return root.gameObject;
        }

        static void MatchToSceneView(Transform t)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                t.position = SceneView.lastActiveSceneView.camera.transform.position;
                t.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
            }
        }

#if HDRP_14_0_OR_NEWER
        static void ConfigureHDCamera(Camera camera)
        {
            var additionalCameraData = camera.GetComponent<HDAdditionalCameraData>();
            if (additionalCameraData == null)
                additionalCameraData = Undo.AddComponent<HDAdditionalCameraData>(camera.gameObject);

            additionalCameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
            additionalCameraData.taaSharpenStrength = .6f;
            additionalCameraData.dithering = true;
            additionalCameraData.stopNaNs = true;
        }

#endif
    }
}
