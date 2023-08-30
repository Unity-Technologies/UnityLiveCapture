#if CINEMACHINE_2_4_OR_NEWER
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
using CinemachineCamera = Cinemachine.CinemachineVirtualCamera;
#endif

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    static class VirtualCameraCreatorUtilitiesCM
    {
        const string k_CreateObjectUndoNameFmt = "Create {0}";

        static string GetUndoName(string objName)
        {
            return string.Format(k_CreateObjectUndoNameFmt, objName);
        }

        /// <summary>
        /// Creates a Cinemachine Camera Actor with all the required components.
        /// </summary>
        [MenuItem("GameObject/Live Capture/Camera/Cinemachine Camera Actor", false, 10)]
        public static GameObject CreateCinemachineCameraActor()
        {
            var go = CreateCinemachineCameraActorInternal(Selection.activeTransform);

            Selection.activeObject = go;

            return go;
        }

        internal static GameObject CreateCinemachineCameraActorInternal()
        {
            return CreateCinemachineCameraActorInternal(null);
        }

        internal static GameObject CreateCinemachineCameraActorInternal(Transform parent)
        {
            var stage = StageUtility.GetCurrentStage();
            var name = "Cinemachine Camera Actor";
            var undoName = GetUndoName(name);
            var brains = stage.FindComponentsOfType<CinemachineBrain>()
                .Where(b => b != null && b.isActiveAndEnabled && b.OutputCamera != null);
            var brain = brains.FirstOrDefault();
            var camera = default(Camera);

            if (brain != null)
            {
                camera = brain.OutputCamera;
            }

            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                camera = stage.FindComponentOfType<Camera>();
            }

            if (camera == null)
            {
                var cameraRoot = new GameObject("Camera", typeof(Camera), typeof(CinemachineBrain));
                camera = cameraRoot.GetComponent<Camera>();
                cameraRoot.tag = "MainCamera";
                StageUtility.PlaceGameObjectInCurrentStage(cameraRoot);
                GameObjectUtility.EnsureUniqueNameForSibling(cameraRoot);
                Undo.RegisterCreatedObjectUndo(cameraRoot, undoName);
            }
            else if (brain == null)
            {
                brain = Undo.AddComponent<CinemachineBrain>(camera.gameObject);
            }

#if HDRP_14_0_OR_NEWER
            ConfigureHDCamera(camera);
#endif
            if (camera.GetComponent<FrameLines>() == null)
            {
                Undo.AddComponent<FrameLines>(camera.gameObject);
            }

            var root = new GameObject(name, typeof(CinemachineCameraDriver));
            StageUtility.PlaceGameObjectInCurrentStage(root);
            Undo.RegisterCreatedObjectUndo(root, undoName);

            if (parent != null)
            {
                Undo.SetTransformParent(root.transform, parent, undoName);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(root);

            var virtualCameraRoot = new GameObject("Cinemachine Virtual Camera",
                typeof(CinemachineCamera));

            Undo.RegisterCreatedObjectUndo(virtualCameraRoot, undoName);
            Undo.SetTransformParent(virtualCameraRoot.transform, root.transform, undoName);

            var virtualCamera = virtualCameraRoot.GetComponent<CinemachineCamera>();
#if CINEMACHINE_3_0_0_OR_NEWER
            virtualCameraRoot.AddComponent<CinemachineFollow>();
            virtualCameraRoot.AddComponent<CinemachineSameAsFollowTarget>();
#else
            virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            virtualCamera.AddCinemachineComponent<CinemachineSameAsFollowTarget>();
#endif
            virtualCamera.Follow = root.transform;
            var driver = root.GetComponent<CinemachineCameraDriver>();
            driver.CinemachineVirtualCamera = virtualCamera;


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
#endif
