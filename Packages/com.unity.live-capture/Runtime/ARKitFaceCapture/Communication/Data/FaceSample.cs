using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FaceSampleV0
    {
        float m_Timestamp;
        FaceBlendShapePose m_BlendShapes;
        Vector3 m_HeadPosition;
        Quaternion m_HeadOrientation;
        Quaternion m_LeftEyeOrientation;
        Quaternion m_RightEyeOrientation;

        public static explicit operator FaceSampleV0(FaceSample sample)
        {
            return new FaceSampleV0
            {
                m_Timestamp = (float)sample.Time,
                m_BlendShapes = sample.FacePose.BlendShapes,
                m_HeadPosition = sample.FacePose.HeadPosition,
                m_HeadOrientation = sample.FacePose.HeadOrientation,
                m_LeftEyeOrientation = sample.FacePose.LeftEyeOrientation,
                m_RightEyeOrientation = sample.FacePose.RightEyeOrientation,
            };
        }

        public static explicit operator FaceSample(FaceSampleV0 sample)
        {
            return new FaceSample
            {
                Time = sample.m_Timestamp,
                FacePose = new FacePose
                {
                    BlendShapes = sample.m_BlendShapes,
                    HeadPosition = sample.m_HeadPosition,
                    HeadOrientation = sample.m_HeadOrientation,
                    LeftEyeOrientation = sample.m_LeftEyeOrientation,
                    RightEyeOrientation = sample.m_RightEyeOrientation,
                },
            };
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FaceSampleV1
    {
        double m_Time;
        FaceBlendShapePose m_BlendShapes;
        Vector3 m_HeadPosition;
        Quaternion m_HeadOrientation;
        Quaternion m_LeftEyeOrientation;
        Quaternion m_RightEyeOrientation;

        public static explicit operator FaceSampleV1(FaceSample sample)
        {
            return new FaceSampleV1
            {
                m_Time = sample.Time,
                m_BlendShapes = sample.FacePose.BlendShapes,
                m_HeadPosition = sample.FacePose.HeadPosition,
                m_HeadOrientation = sample.FacePose.HeadOrientation,
                m_LeftEyeOrientation = sample.FacePose.LeftEyeOrientation,
                m_RightEyeOrientation = sample.FacePose.RightEyeOrientation,
            };
        }

        public static explicit operator FaceSample(FaceSampleV1 sample)
        {
            return new FaceSample
            {
                Time = sample.m_Time,
                FacePose = new FacePose
                {
                    BlendShapes = sample.m_BlendShapes,
                    HeadPosition = sample.m_HeadPosition,
                    HeadOrientation = sample.m_HeadOrientation,
                    LeftEyeOrientation = sample.m_LeftEyeOrientation,
                    RightEyeOrientation = sample.m_RightEyeOrientation,
                },
            };
        }
    }
}
