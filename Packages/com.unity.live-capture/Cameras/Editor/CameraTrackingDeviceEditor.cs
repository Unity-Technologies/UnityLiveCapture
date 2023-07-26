using UnityEngine;
using UnityEditor;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.Cameras.Editor
{
    /// <summary>
    /// The default Inspector for <see cref="CameraTrackingDevice"/>.
    /// </summary>
    [CustomEditor(typeof(CameraTrackingDevice), true)]
    public abstract class CameraTrackingDeviceEditor : LiveStreamCaptureDeviceEditor
    {
        static class Contents
        {
            public static GUIContent CameraLabel = EditorGUIUtility.TrTextContent("Camera", "The camera currently assigned to this device.");
        }

        SerializedProperty m_Camera;

        /// <summary>
        /// Called when the editor is being initialized.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_Camera = serializedObject.FindProperty("m_Camera");
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        /// <seealso cref="DrawDefaultCameraTrackerInspector"/>
        public override void OnInspectorGUI()
        {
            DrawDefaultCameraTrackerInspector();
            DrawDefaultLiveStreamInspector();
        }

        /// <summary>
        /// Makes a custom inspector GUI for <see cref="CameraTrackingDevice"/>.
        /// </summary>
        protected void DrawDefaultCameraTrackerInspector()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Camera, Contents.CameraLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
