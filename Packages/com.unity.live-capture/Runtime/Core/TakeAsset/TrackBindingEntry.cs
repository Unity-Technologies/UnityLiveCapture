using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    [Serializable]
    class TrackBindingEntry
    {
        [SerializeField]
        LazyLoadReference<TrackAsset> m_Track;

        [SerializeReference]
        ITakeBinding m_Binding;

        public TrackAsset Track => m_Track.asset;
        public ITakeBinding Binding => m_Binding;

        public TrackBindingEntry(TrackAsset track, ITakeBinding binding)
        {
            m_Track = track;
            m_Binding = binding;
        }
    }
}
