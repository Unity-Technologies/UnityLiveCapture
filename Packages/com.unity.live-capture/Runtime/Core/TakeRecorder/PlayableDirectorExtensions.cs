using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Unity.LiveCapture
{
    static class PlayableDirectorExtensions
    {
        public static void SetSceneBindings(this PlayableDirector director, IEnumerable<TrackBindingEntry> entries)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            foreach (var entry in entries)
            {
                var track = entry.Track;
                var binding = entry.Binding;

                if (track == null || binding == null)
                {
                    continue;
                }

                var value = binding.GetValue(director);

                director.SetGenericBinding(track, value);
            }
        }

        public static void ClearSceneBindings(this PlayableDirector director, IEnumerable<TrackBindingEntry> entries)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            foreach (var entry in entries)
            {
                director.ClearGenericBinding(entry.Track);
            }
        }
    }
}
