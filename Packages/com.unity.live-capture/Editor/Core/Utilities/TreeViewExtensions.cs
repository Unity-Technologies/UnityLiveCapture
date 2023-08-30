using System;
using System.Reflection;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    static class TreeViewExtensions
    {
        static readonly PropertyInfo s_DeselectOnUnhandledMouseDown = typeof(TreeView)
            .GetProperty("deselectOnUnhandledMouseDown", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void DeselectOnUnhandledMouseDown(this TreeView treeView, bool value)
        {
            if (treeView == null)
            {
                throw new ArgumentNullException(nameof(treeView));
            }

            Debug.Assert(s_DeselectOnUnhandledMouseDown != null);

            s_DeselectOnUnhandledMouseDown.SetValue(treeView, value);
        }
    }
}
