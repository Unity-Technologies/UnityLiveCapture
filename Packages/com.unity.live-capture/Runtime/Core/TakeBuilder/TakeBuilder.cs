using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture
{
    class TakeBuilder : ITakeBuilder, IDisposable
    {
        readonly TakeNameFormatter m_Formatter = new TakeNameFormatter();
        IExposedPropertyTable m_Resolver;
        string m_ContentsDirectory;
        Take m_Take;
        readonly Dictionary<TrackAsset, double> m_TrackAlignTimes = new Dictionary<TrackAsset, double>();

        public Take Take => m_Take;

        /// <inheritdoc/>
        public double ContextStartTime { get; private set; }
        public double ContextDuration { get; private set; }
        public double RecordingStartTime { get; private set; }
        public double RecordingDuration { get; private set; }
        double ClipStartTime => ContextStartTime + RecordingStartTime;

        /// <summary>
        /// The directory that stores the take content data, like the recorded clips.
        /// </summary>
        public string ContentsDirectory => m_ContentsDirectory;

        internal static string GetAssetName(
            int sceneNumber,
            string shotName,
            int takeNumber)
        {
            return GetAssetName(
                new TakeNameFormatter(),
                sceneNumber,
                shotName,
                takeNumber);
        }

        static string GetAssetName(
            TakeNameFormatter formatter,
            int sceneNumber,
            string shotName,
            int takeNumber)
        {
            formatter.ConfigureTake(sceneNumber, shotName, takeNumber);
            return formatter.GetTakeName();
        }

        public TakeBuilder(
            double contextStartTime,
            double contextDuration,
            double recordingStartTime,
            double recordingDuration,
            int sceneNumber,
            string shotName,
            int takeNumber,
            string description,
            string directory,
            DateTime creationTime,
            Take iterationBase,
            FrameRate frameRate,
            Texture2D screenshot,
            IExposedPropertyTable resolver)
        {
#if UNITY_EDITOR
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            shotName = FileNameFormatter.Instance.Format(shotName);

            if (string.IsNullOrEmpty(shotName))
            {
                throw new InvalidDataException("Shot name is invalid or empty");
            }

            m_Resolver = resolver;
            ContextStartTime = contextStartTime;
            ContextDuration = contextDuration;
            RecordingStartTime = recordingStartTime;
            RecordingDuration = recordingDuration;

            var assetName = GetAssetName(m_Formatter, sceneNumber, shotName, takeNumber);
            var assetPath = $"{directory}/{assetName}.asset";
            var exists = AssetDatabase.LoadMainAssetAtPath(assetPath) != null;

            if (exists)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                shotName = Path.GetFileNameWithoutExtension(assetPath);
            }

            m_ContentsDirectory = Path.GetDirectoryName(assetPath) + "/" + shotName;

            Directory.CreateDirectory(m_ContentsDirectory);

            if (iterationBase == null)
            {
                var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                m_Take = ScriptableObject.CreateInstance<Take>();

                AssetDatabase.CreateAsset(m_Take, assetPath);
                AssetDatabase.AddObjectToAsset(timeline, m_Take);
                AssetDatabase.SetMainObject(m_Take, assetPath);

                m_Take.Timeline = timeline;
            }
            else
            {
                var sourceAssetPath = AssetDatabase.GetAssetPath(iterationBase);
                AssetDatabase.CopyAsset(sourceAssetPath, assetPath);
                m_Take = AssetDatabase.LoadAssetAtPath<Take>(assetPath);
                m_Take.Rating = default;
            }

            m_Take.SceneNumber = sceneNumber;
            m_Take.ShotName = shotName;
            m_Take.TakeNumber = takeNumber;
            m_Take.CreationTime = creationTime;
            m_Take.Description = description;
            m_Take.FrameRate = frameRate;
            m_Take.Screenshot = SaveAsPNG(screenshot, assetName, m_ContentsDirectory);

            {
                var timeline = m_Take.Timeline;

                timeline.name = m_Take.name;
#if TIMELINE_1_6_0_OR_NEWER
                timeline.editorSettings.frameRate = frameRate.AsFloat();
#else
                timeline.editorSettings.fps = frameRate.AsFloat();
#endif

                if (contextDuration > 0d)
                {
                    timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
                    timeline.fixedDuration = contextStartTime + contextDuration;
                }
                else
                {
                    timeline.durationMode = TimelineAsset.DurationMode.BasedOnClips;
                }
            }
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();

            m_Take = null;
            m_Resolver = null;
#endif
        }

        /// <inheritdoc/>
        public void CreateAnimationTrack(string name, Animator animator, AnimationClip animationClip, ITrackMetadata metadata = null, double? alignTime = null)
        {
            var track = CreateAnimationTrackInternal(name, animator, animationClip, alignTime);

            if (metadata != null)
            {
                AddMetadata(track, metadata);
            }
        }

        AnimationTrack CreateAnimationTrackInternal(string name, Animator animator, AnimationClip animationClip, double? alignTime)
        {
            CheckDisposed();

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Track name is null or empty.", nameof(name));
            }

            if (animator == null)
            {
                throw new ArgumentNullException(nameof(animator));
            }

            if (animationClip == null)
            {
                throw new ArgumentNullException(nameof(animationClip));
            }

            var assetName = animator.gameObject.name;

            SaveAsAsset(animationClip, assetName);

            var binding = CreateBinding<AnimatorTakeBinding>(assetName, animator);
            var parent = GetTracks<AnimationTrack>(binding).FirstOrDefault();
            var track = CreateTrackWithParent<AnimationTrack>(name, parent);

            if (alignTime != null)
            {
                m_TrackAlignTimes.Add(track, alignTime.Value);
            }

            m_Take.AddTrackBinding(track, binding);

            var clip = track.CreateClip(animationClip);
            var playableAsset = clip.asset as AnimationPlayableAsset;

            playableAsset.removeStartOffset = false;

            clip.start = ClipStartTime;

            return track;
        }

        void AddMetadata(TrackAsset track, ITrackMetadata metadata)
        {
            m_Take.AddTrackMetadata(track, metadata);
        }

        /// <summary>
        /// Modify the animation tracks such that their clips are aligned by their start times.
        /// </summary>
        /// <remarks>
        /// If <see cref="CreateAnimationTrack"/>
        /// was called with respective start times for two or more tracks,
        /// this method aligns those clips with t=0 corresponding to the earliest sample.
        /// Tracks without start times have their clips aligned at t=0.
        /// </remarks>
        /// <param name="defaultStartTime">The start time to use for the take if none of the tracks had a start time.</param>
        public void AlignTracks(double defaultStartTime)
        {
            var takeStartTime = m_TrackAlignTimes.Any() ? m_TrackAlignTimes.Values.Min() : defaultStartTime;
            m_Take.StartTimecode = Timecode.FromSeconds(m_Take.FrameRate, takeStartTime);

            // Shortcut: If we have no start times specified or if we only have the start time for
            // a single track, we can skip alignment.
            if (m_TrackAlignTimes.Count <= 1) return;

            foreach (var track in m_Take.Timeline.GetRootTracks())
            {
                if (m_TrackAlignTimes.TryGetValue(track, out var alignTime))
                {
                    // We can guarantee that trackStartTime >= 0
                    var trackStartTime = alignTime - takeStartTime + ClipStartTime;
                    foreach (var clip in track.GetClips())
                    {
                        clip.start = trackStartTime;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void AddBinding<T>(TakeBinding<T> binding, T value) where T : UnityEngine.Object
        {
            CheckDisposed();

            binding.SetValue(value, m_Resolver);

            m_Take.AddBinding(binding);
        }

        /// <summary>
        /// Creates a new <see cref="ITakeBinding"/>.
        /// </summary>
        /// <param name="name">The binding name.</param>
        /// <param name="value">The binding object.</param>
        /// <typeparam name="T">The type of binding created. The track type must be derived from ITakeBinding.</typeparam>
        /// <returns>Returns the created binding.</returns>
        internal T CreateBinding<T>(string name, UnityObject value) where T : ITakeBinding, new()
        {
            CheckDisposed();

            var binding = new T();

            binding.SetName(name);
            binding.SetValue(value, m_Resolver);

            return binding;
        }

        /// <inheritdoc/>
        public T CreateTrack<T>(string name, double? alignmentTime = null) where T : TrackAsset, new()
        {
            return CreateTrackWithParent<T>(name, null);
        }

        /// <inheritdoc/>
        public T CreateTrack<T>(string name, ITakeBinding binding, double? alignmentTime = null) where T : TrackAsset, new()
        {
            var track = CreateTrack<T>(name);

            m_Take.AddTrackBinding(track, binding);

            return track;
        }

        /// <summary>
        /// Gets all the available tracks.
        /// </summary>
        /// <remarks>
        /// When recording take iterations, the capture device might overwrite existing tracks
        /// instead of creating new ones.
        /// </remarks>
        /// <returns>
        /// An enumerable of the tracks.
        /// </returns>
        public IEnumerable<T> GetTracks<T>() where T : TrackAsset, new()
        {
            return m_Take.Timeline
                .GetRootTracks()
                .OfType<T>();
        }

        /// <summary>
        /// Gets all the available tracks that use the provided <see cref="ITakeBinding"/>.
        /// </summary>
        /// <remarks>
        /// When recording take iterations, the capture device might overwrite existing tracks
        /// instead of creating new ones.
        /// </remarks>
        /// <param name="binding">The binding to query from.</param>
        /// <returns>
        /// An enumerable of the tracks.
        /// </returns>
        public IEnumerable<T> GetTracks<T>(ITakeBinding binding) where T : TrackAsset, new()
        {
            return m_Take.BindingEntries
                .Where(e => e.Track is T
                    && ITakeBindingEqualityComparer.Instance.Equals(e.Binding, binding))
                .Select(e => e.Track as T);
        }

        /// <summary>
        /// Stores an object into an asset inside the contents directory.
        /// </summary>
        /// <remarks>
        /// Devices might use this method to store the artifacts of the recording, like a clip.
        /// </remarks>
        /// <param name="obj">The object to store.</param>
        /// <param name="name">The name to use in the asset.</param>
        public void SaveAsAsset<T>(T obj, string name) where T : UnityObject
        {
#if UNITY_EDITOR
            var extension = string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Can't save as asset. Provided object has no name.");
            }

            if (typeof(T) == typeof(AnimationClip))
            {
                extension = "anim";
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                extension = "asset";
            }
            else
            {
                throw new Exception($"Can't save assets of type {typeof(T)}");
            }

            m_Formatter.ConfigureAsset(name);
            var assetName = m_Formatter.GetAssetName();

            var path = $"{m_ContentsDirectory}/{assetName}.{extension}";

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(obj, path);
#endif
        }

        void CheckDisposed()
        {
            if (m_Take == null)
            {
                throw new ObjectDisposedException($"{nameof(LiveCapture.Take)} is invalid or has been disposed.");
            }

            if (m_Resolver == null)
            {
                throw new ObjectDisposedException($"{nameof(IExposedPropertyTable)} is invalid or has been disposed.");
            }
        }

        T CreateTrackWithParent<T>(string name, TrackAsset parent) where T : TrackAsset, new()
        {
            CheckDisposed();

            var timeline = m_Take.Timeline;
            var track = timeline.CreateTrack<T>(parent, name);

            track.locked = true;

            return track;
        }

        Texture2D SaveAsPNG(Texture2D texture, string filename, string directory)
        {
#if UNITY_EDITOR
            if (texture != null)
            {
                var assetPath = Screenshot.SaveAsPNG(texture, filename, directory);

                AssetDatabase.ImportAsset(assetPath);

                return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }
#endif
            return null;
        }
    }
}
