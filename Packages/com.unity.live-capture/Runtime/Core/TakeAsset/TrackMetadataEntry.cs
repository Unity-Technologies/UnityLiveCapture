using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    [Serializable]
    class TrackMetadataEntry
    {
        [SerializeField]
        LazyLoadReference<TrackAsset> m_Track;

        [SerializeReference]
        ITrackMetadata m_Metadata;

        public TrackAsset Track => m_Track.asset;
        public ITrackMetadata Metadata => m_Metadata;

        public TrackMetadataEntry(TrackAsset track, ITrackMetadata metadata)
        {
            m_Track = track;
            m_Metadata = metadata;
        }
    }
}
