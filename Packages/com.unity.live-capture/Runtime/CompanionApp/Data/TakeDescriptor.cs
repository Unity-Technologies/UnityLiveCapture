using System;
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// Class that stores information of a take. The client uses this information to build its UI.
    /// </summary>
    [Serializable]
    class TakeDescriptor
    {
        [SerializeField]
        SerializableGuid m_Guid;
        [SerializeField]
        string m_Name;
        [SerializeField]
        int m_SceneNumber;
        [SerializeField]
        string m_ShotName;
        [SerializeField]
        int m_TakeNumber;
        [SerializeField]
        long m_CreationTime;
        [SerializeField]
        string m_Description;
        [SerializeField]
        int m_Rating;
        [SerializeField]
        FrameRate m_FrameRate;
        [SerializeField]
        SerializableGuid m_Screenshot;
        [SerializeField]
        string m_TimelineName;
        [SerializeField]
        double m_TimelineDuration;

        /// <summary>
        /// The globally unique identifier of the take asset.
        /// </summary>
        public Guid Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        /// <summary>
        /// The name of the take.
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        /// The number associated with the scene where the take was captured.
        /// </summary>
        public int SceneNumber
        {
            get => m_SceneNumber;
            set => m_SceneNumber = value;
        }

        /// <summary>
        /// The name of the shot where the take was captured.
        /// </summary>
        public string ShotName
        {
            get => m_ShotName;
            set => m_ShotName = value;
        }

        /// <summary>
        /// The number associated with the take.
        /// </summary>
        public int TakeNumber
        {
            get => m_TakeNumber;
            set => m_TakeNumber = value;
        }

        /// <summary>
        /// The creation time of the take, stored as binary.
        /// </summary>
        public long CreationTime
        {
            get => m_CreationTime;
            internal set => m_CreationTime = value;
        }

        /// <summary>
        /// The description of the shot where the take was captured.
        /// </summary>
        public string Description
        {
            get => m_Description;
            set => m_Description = value;
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
        /// The frame rate used during the recording.
        /// </summary>
        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set => m_FrameRate = value;
        }

        /// <summary>
        /// The Guid of the screenshot of the take.
        /// </summary>
        public Guid Screenshot
        {
            get => m_Screenshot;
            set => m_Screenshot = value;
        }

        /// <summary>
        /// The name of the timeline of the take.
        /// </summary>
        public string TimelineName
        {
            get => m_TimelineName;
            set => m_TimelineName = value;
        }

        /// <summary>
        /// The duration of the timeline of the take.
        /// </summary>
        public double TimelineDuration
        {
            get => m_TimelineDuration;
            set => m_TimelineDuration = value;
        }

        /// <summary>
        /// Copies all properties from another take descriptor.
        /// </summary>
        /// <param name="other">The take descriptor to copy properties from.</param>
        public void CopyFrom(TakeDescriptor other)
        {
            Guid = other.Guid;
            Name = other.Name;
            SceneNumber = other.SceneNumber;
            ShotName = other.ShotName;
            TakeNumber = other.TakeNumber;
            CreationTime = other.CreationTime;
            Description = other.Description;
            Rating = other.Rating;
            FrameRate = other.FrameRate;
            Screenshot = other.Screenshot;
            TimelineName = other.TimelineName;
            TimelineDuration = other.TimelineDuration;
        }

        internal static TakeDescriptor Create(Take take)
        {
            var descriptor = new TakeDescriptor();
#if UNITY_EDITOR
            descriptor.Guid = SerializableGuid.FromString(AssetDatabaseUtility.GetAssetGUID(take));
            descriptor.Name = take.name;
            descriptor.SceneNumber = take.SceneNumber;
            descriptor.ShotName = take.ShotName;
            descriptor.TakeNumber = take.TakeNumber;
            descriptor.CreationTime = take.CreationTime.ToBinary();
            descriptor.Description = take.Description;
            descriptor.Rating = take.Rating;
            descriptor.FrameRate = take.FrameRate;

            if (take.TryGetScreenshotInstanceID(out var instanceID))
            {
                descriptor.Screenshot = SerializableGuid.FromString(AssetDatabaseUtility.GetAssetGUID(instanceID));
            }

            descriptor.TimelineName = take.name;
            descriptor.TimelineDuration = take.Duration;
#endif
            return descriptor;
        }
    }
}
