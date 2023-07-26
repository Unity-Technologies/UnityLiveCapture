using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class SnapshotDescriptor
    {
        [SerializeField]
        Pose m_Pose;
        [SerializeField]
        LensAssetDescriptor m_LensAsset;
        [SerializeField]
        Lens m_Lens;
        [SerializeField]
        CameraBody m_CameraBody;
        [SerializeField]
        SerializableGuid m_Screenshot;
        [SerializeField]
        bool m_IsShotValid;
        [SerializeField]
        int m_SceneNumber;
        [SerializeField]
        string m_ShotName;
        [SerializeField]
        FrameRate m_FrameRate;
        [SerializeField]
        double m_Time;

        public Pose Pose
        {
            get => m_Pose;
            set => m_Pose = value;
        }

        public LensAssetDescriptor LensAsset
        {
            get => m_LensAsset;
            set => m_LensAsset = value;
        }

        public Lens Lens
        {
            get => m_Lens;
            set => m_Lens = value;
        }

        public CameraBody CameraBody
        {
            get => m_CameraBody;
            set => m_CameraBody = value;
        }

        public Guid Screenshot
        {
            get => m_Screenshot;
            set => m_Screenshot = value;
        }

        public bool IsShotValid
        {
            get => m_IsShotValid;
            set => m_IsShotValid = value;
        }

        public int SceneNumber
        {
            get => m_SceneNumber;
            set => m_SceneNumber = value;
        }

        public string ShotName
        {
            get => m_ShotName;
            set => m_ShotName = value;
        }

        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set => m_FrameRate = value;
        }

        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }

        internal static SnapshotDescriptor Create(Snapshot snapshot)
        {
            var descriptor = new SnapshotDescriptor();
#if UNITY_EDITOR
            if (snapshot != null)
            {
                descriptor.Pose = snapshot.Pose;
                descriptor.LensAsset = LensAssetDescriptor.Create(snapshot.LensAsset);
                descriptor.Lens = snapshot.Lens;
                descriptor.CameraBody = snapshot.CameraBody;

                if (snapshot.Screenshot != null)
                {
                    descriptor.Screenshot = SerializableGuid.FromString(AssetDatabaseUtility.GetAssetGUID(snapshot.Screenshot));
                }

                var shotAsset = snapshot.Asset as ShotPlayableAsset;
                var isShotValid = shotAsset != null;

                descriptor.IsShotValid = isShotValid;

                if (isShotValid)
                {
                    descriptor.ShotName = shotAsset.ShotName;
                    descriptor.SceneNumber = shotAsset.SceneNumber;
                }

                descriptor.FrameRate = snapshot.FrameRate;
                descriptor.Time = snapshot.Time;
            }
#endif
            return descriptor;
        }
    }
}
