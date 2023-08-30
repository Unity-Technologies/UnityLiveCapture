using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(ReticleAttribute))]
    class ReticlePropertyDrawer : PropertyDrawer
    {
        enum FocusReticleControl
        {
            Device = 0,
            DeviceAndGameView = 1
        }

        static string GetDescription(FocusReticleControl value)
        {
            switch (value)
            {
                case FocusReticleControl.Device:
                    return "Device";
                case FocusReticleControl.DeviceAndGameView:
                    return "Device And Game View";
            }

            return string.Empty;
        }

        static class Contents
        {
            public static readonly GUIContent ReticleControl = new GUIContent("Reticle Control", "Whether the Focus Reticle position is controlled from the remote device only or also by clicking in the Game View.");
            public static readonly GUIContent[] ReticleOptions =
            {
                new GUIContent(GetDescription((FocusReticleControl)0)),
                new GUIContent(GetDescription((FocusReticleControl)1))
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            var index = (int)(GameViewController.instance.IsActive ? FocusReticleControl.DeviceAndGameView : FocusReticleControl.Device);
            var newIndex = EditorGUI.Popup(position, Contents.ReticleControl, index, Contents.ReticleOptions);

            if (newIndex != index)
            {
                if (newIndex == (int)FocusReticleControl.DeviceAndGameView)
                    GameViewController.instance.Enable();
                else
                    GameViewController.instance.Disable();
            }
        }
    }
}
