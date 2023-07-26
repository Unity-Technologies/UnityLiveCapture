using UnityEngine;
using UnityEditor;

namespace Unity.TouchFramework
{
    [CustomEditor(typeof(UITabManager))]
    public class UITabManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                DrawDefaultInspector();

                if (change.changed)
                {
                    var t = target as UITabManager;
                    t.SetActiveTab(t.activeTabIndex, true);
                }
            }
        }
    }
}
