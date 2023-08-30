using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A take is a recorded performance of one or more actors. The <see cref="TakeRecorder"/> stores the performance
    /// as a timeline asset.
    /// </summary>
    public class Take : ScriptableObject
    {
        [SerializeField]
        int m_SceneNumber;
        [SerializeField]
        string m_ShotName;
        [SerializeField]
        int m_TakeNumber;
        [SerializeField]
        long m_CreationTime;
        [SerializeField, TextArea(2, 4)]
        string m_Description;
        [SerializeField, Rating]
        int m_Rating;
        [SerializeField, OnlyStandardFrameRates]
        FrameRate m_FrameRate;
        [SerializeField]
        double m_Duration;
        [SerializeField]
        Timecode m_StartTimecode;
        [SerializeField]
        LazyLoadReference<Texture2D> m_Screenshot;
        [SerializeField]
        LazyLoadReference<TimelineAsset> m_Timeline;
        [SerializeReference]
        List<ITakeBinding> m_Bindings = new List<ITakeBinding>();
        [SerializeField]
        List<TrackBindingEntry> m_Entries = new List<TrackBindingEntry>();
        [SerializeField]
        List<TrackMetadataEntry> m_MetadataEntries = new List<TrackMetadataEntry>();

        /// <summary>
        /// The number associated with the scene where the take was captured.
        /// </summary>
        public int SceneNumber
        {
            get => m_SceneNumber;
            internal set => m_SceneNumber = value;
        }

        /// <summary>
        /// The name of the shot where the take was captured.
        /// </summary>
        public string ShotName
        {
            get => m_ShotName;
            internal set => m_ShotName = value;
        }

        /// <summary>
        /// The number associated with the take.
        /// </summary>
        public int TakeNumber
        {
            get => m_TakeNumber;
            internal set => m_TakeNumber = value;
        }

        /// <summary>
        /// The creation time of the take.
        /// </summary>
        public DateTime CreationTime
        {
            get => DateTime.FromBinary(m_CreationTime);
            internal set => m_CreationTime = value.ToBinary();
        }

        /// <summary>
        /// The description of the shot where the take was captured.
        /// </summary>
        public string Description
        {
            get => m_Description;
            internal set => m_Description = value;
        }

        /// <summary>
        /// The rating of the take.
        /// </summary>
        public int Rating
        {
            get => m_Rating;
            set => m_Rating = value;
        }

        /// <summary>
        /// The timecode of the start of the take.
        /// </summary>
        public Timecode StartTimecode
        {
            get => m_StartTimecode;
            set => m_StartTimecode = value;
        }

        /// <summary>
        /// The frame rate used during the recording.
        /// </summary>
        public FrameRate FrameRate
        {
            get => m_FrameRate;
            internal set => m_FrameRate = value;
        }

        /// <summary>
        /// The length of the take in seconds.
        /// </summary>
        public double Duration
        {
            get => m_Duration;
            internal set => m_Duration = value;
        }

        /// <summary>
        /// The screenshot at the beginning of the take.
        /// </summary>
        public Texture2D Screenshot
        {
            get => m_Screenshot.asset;
            internal set => m_Screenshot = value;
        }

        /// <summary>
        /// Gets the object instanceID referenced by Screenshot.
        /// This doesn't load the texture asset and can be passed to a number
        /// of AssetDatabase functions.
        /// </summary>
        /// <returns>False if the Screenshot reference isn't set.</returns>
        internal bool TryGetScreenshotInstanceID(out int instanceID)
        {
            instanceID = m_Screenshot.instanceID;
            return m_Screenshot.isSet;
        }

        /// <summary>
        /// The timeline asset containing the recorded performance.
        /// </summary>
        public TimelineAsset Timeline
        {
            get => m_Timeline.asset;
            internal set => m_Timeline = value;
        }

        internal IEnumerable<TrackBindingEntry> BindingEntries => m_Entries;

        internal IEnumerable<TrackMetadataEntry> MetadataEntries => m_MetadataEntries;

        internal void AddBinding(ITakeBinding binding)
        {
            m_Bindings.Add(binding);
        }

        internal void AddTrackBinding(TrackAsset track, ITakeBinding binding)
        {
            m_Entries.Add(new TrackBindingEntry(track, binding));
        }

        internal void AddTrackMetadata(TrackAsset track, ITrackMetadata metadata)
        {
            m_MetadataEntries.Add(new TrackMetadataEntry(track, metadata));
        }
    }
}
