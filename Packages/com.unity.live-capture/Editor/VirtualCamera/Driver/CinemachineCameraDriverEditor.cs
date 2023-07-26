using UnityEditor;
using UnityEngine;
using Unity.LiveCapture.Editor;
#if SRP_CORE_14_0_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(CinemachineCameraDriver))]
    [CanEditMultipleObjects]
    class CinemachineCameraDriverEditor : Editor
    {
#if SRP_CORE_14_0_OR_NEWER
        static class SRPContents
        {
            public static readonly GUIContent UpgradeVolume = EditorGUIUtility.TrTextContentWithIcon(
                "The Volume and the Collider are not needed anymore. Remove the Volume and Collider.",
                "console.warnicon");
        }
#endif

        static readonly string[] k_ExcludeProps = { "m_Script" };

        void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, k_ExcludeProps);

            serializedObject.ApplyModifiedProperties();

            DoMigrationGUI();
        }

        void DoMigrationGUI()
        {
#if SRP_CORE_14_0_OR_NEWER
            var hasVolume = false;
            var hasCollider = false;

            foreach (var t in targets)
            {
                var driver = t as CinemachineCameraDriver;

                hasVolume |= driver.TryGetComponent<Volume>(out var volume);
                hasCollider |= driver.TryGetComponent<Collider>(out var collider);
            }

            if (hasVolume && hasCollider)
            {
                LiveCaptureGUI.DrawFixMeBox(SRPContents.UpgradeVolume, () =>
                {
                    foreach (var t in targets)
                    {
                        var driver = t as CinemachineCameraDriver;

                        if (driver.TryGetComponent<Volume>(out var volume)
                            && driver.TryGetComponent<Collider>(out var collider))
                        {
                            Undo.DestroyObjectImmediate(volume);
                            Undo.DestroyObjectImmediate(collider);
                        }
                    }
                });
            }
#endif
        }
    }
}
