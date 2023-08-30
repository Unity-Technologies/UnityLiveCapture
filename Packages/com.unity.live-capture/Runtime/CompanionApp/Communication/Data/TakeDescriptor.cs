using System;
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp.Networking
{
    [Serializable]
    class TakeDescriptorV0
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

        public static explicit operator TakeDescriptorV0(TakeDescriptor take)
        {
            return new TakeDescriptorV0
            {
                m_Guid = take.Guid,
                m_Name = take.Name,
                m_SceneNumber = take.SceneNumber,
                m_ShotName = take.ShotName,
                m_TakeNumber = take.TakeNumber,
                m_CreationTime = take.CreationTime,
                m_Description = take.Description,
                m_Rating = take.Rating,
                m_FrameRate = take.FrameRate,
                m_Screenshot = take.Screenshot,
                m_TimelineName = take.TimelineName,
                m_TimelineDuration = take.TimelineDuration
            };
        }

        public static explicit operator TakeDescriptor(TakeDescriptorV0 take)
        {
            return new TakeDescriptor
            {
                Guid = take.m_Guid,
                Name = take.m_Name,
                SceneNumber = take.m_SceneNumber,
                ShotName = take.m_ShotName,
                TakeNumber = take.m_TakeNumber,
                CreationTime = take.m_CreationTime,
                Description = take.m_Description,
                Rating = take.m_Rating,
                FrameRate = take.m_FrameRate,
                Screenshot = take.m_Screenshot,
                TimelineName = take.m_TimelineName,
                TimelineDuration = take.m_TimelineDuration
            };
        }
    }
}
