using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Editor
{
    [CustomTimelineEditor(typeof(ShotPlayableAsset))]
    class ShotPlayableAssetClipEditor : ClipEditor
    {
        static readonly Color s_LockedColor = new Color(0.23f, 0.33f, 0.43f, 1f);
        static readonly Color s_MarkerColor = new Color(1f, 0.65f, 0f, 1f);
        static readonly Color s_RecordColor = new Color(0.75f, 0.1f, 0.1f, 0.5f);

        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var options = base.GetClipOptions(clip);
            var shotAsset = clip.asset as ShotPlayableAsset;
            var take = shotAsset.Take;
            var director = TimelineEditor.inspectedDirector;

            if (take != null
                && director != null
                && TakeBindingsEditor.ContainsNullBindings(take, director))
            {
                options.errorText = TakeBindingsEditor.Contents.NullBindingsMsg;
            }

            return options;
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            if (TakeRecorder.Context == MasterTimelineContext.Instance)
            {
                if (TakeRecorder.IsRecording())
                {
                    DrawBackground(region.position, clip, s_RecordColor);
                }
                else if (TakeRecorder.Context.HasSelection())
                {
                    DrawBackground(region.position, clip, s_LockedColor);
                }
            }

            DoTakeContentMarkers(clip, region);
        }

        void DrawBackground(Rect position, TimelineClip clip, Color color)
        {
            var hierarchy = TimelineHierarchyContextUtility.FromTimelineNavigation();
            var asset = clip.asset as PlayableAsset;
            var index = MasterTimelineContext.Instance.IndexOf(hierarchy, asset);

            if (index == MasterTimelineContext.Instance.Selection)
            {
                EditorGUI.DrawRect(position, color);
            }
        }

        void DoTakeContentMarkers(TimelineClip clip, ClipBackgroundRegion region)
        {
            var shotAsset = clip.asset as ShotPlayableAsset;
            var take = shotAsset.Take;

            if (take.TryGetContentRange(out var start, out var end))
            {
                start -= region.startTime;
                end -= region.startTime;

                var timeSpan = region.endTime - region.startTime;
                var position = region.position;
                var toPixels = position.width / timeSpan;
                var xMin = start * toPixels;
                var width = (end - start) * toPixels;

                position.x += (float)xMin;
                position.width = (float)width;

                EditorGUI.DrawRect(
                    new Rect(position.x, position.y, 2f, position.height)
                    , s_MarkerColor);
                EditorGUI.DrawRect(
                    new Rect(position.x + 2f, position.y, 5f, 2f)
                    , s_MarkerColor);
                EditorGUI.DrawRect(
                    new Rect(position.x + 2f, position.y + position.height - 2f, 5f, 2f)
                    , s_MarkerColor);
                EditorGUI.DrawRect(
                    new Rect(position.xMax - 2f, position.y, 2f, position.height)
                    , s_MarkerColor);
                EditorGUI.DrawRect(
                    new Rect(position.xMax - 7f, position.y, 5f, 2f)
                    , s_MarkerColor);
                EditorGUI.DrawRect(
                    new Rect(position.xMax - 7f, position.y + position.height - 2f, 5f, 2f)
                    , s_MarkerColor);
            }
        }

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            var shotAsset = clip.asset as ShotPlayableAsset;
            var take = shotAsset.Take;

            if (take != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(take);

                shotAsset.Directory = Path.GetDirectoryName(assetPath);
                shotAsset.SceneNumber = take.SceneNumber;
                shotAsset.ShotName = take.ShotName;
                shotAsset.TakeNumber = take.TakeNumber + 1;
                shotAsset.Description = string.Empty;

                clip.displayName = take.ShotName;

                if (take.TryGetContentRange(out var start, out var end))
                {
                    clip.duration = end - start;
                    clip.clipIn = start;

                    // Prevent Timeline from changing clip.duration.
                    // TimleineHelpers.cs AddClipOnTrack:
                    // // get the clip editor
                    // try
                    // {
                    //     CustomTimelineEditorCache.GetClipEditor(newClip).OnCreate(newClip, parentTrack, null);
                    // }
                    // catch (Exception e)
                    // {
                    //     Debug.LogException(e);
                    // }
                    //
                    // reset the duration as the newly assigned values may have changed the default
                    // if (playableAsset != null)
                    // {
                    //     var candidateDuration = playableAsset.duration;
                    //
                    //     if (!double.IsInfinity(candidateDuration) && candidateDuration > 0)
                    //         newClip.duration = Math.Min(Math.Max(candidateDuration, TimelineClip.kMinDuration), TimelineClip.kMaxTimeValue);
                    // }

                    shotAsset.SetDurationOverride(float.PositiveInfinity);
                    EditorApplication.delayCall += () =>
                    {
                        shotAsset.SetDurationOverride(null);
                    };
                }
            }
        }
    }
}
