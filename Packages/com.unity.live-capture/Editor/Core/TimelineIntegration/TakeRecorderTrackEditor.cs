using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;

using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.Editor
{
    [CustomTimelineEditor(typeof(TakeRecorderTrack))]
    class TakeRecorderTrackEditor : TrackEditor
    {
        static readonly string IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";
        static readonly Texture2D TrackIcon = EditorGUIUtility.IconContent($"{IconPath}/TakeRecorderTrack@64.png").image as Texture2D;

        public override TrackDrawOptions GetTrackOptions(TrackAsset track, UnityObject binding)
        {
            var options = base.GetTrackOptions(track, binding);
            options.icon = TrackIcon;
            return options;
        }
    }
}
