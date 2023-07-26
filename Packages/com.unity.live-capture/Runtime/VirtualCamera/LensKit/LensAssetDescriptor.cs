using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Class that stores information of a LensAsset. The client uses this information to build its UI.
    /// </summary>
    [Serializable]
    class LensAssetDescriptor
    {
        [SerializeField]
        SerializableGuid m_Guid;

        [SerializeField]
        string m_Name;

        [SerializeField]
        string m_Manufacturer;

        [SerializeField]
        string m_Model;

        [SerializeField]
        Lens m_DefaultValues;

        [SerializeField, Tooltip("The parameters of the current lens asset.")]
        LensIntrinsics m_Intrinsics;

        /// <summary>
        /// The globally unique identifier of the take asset.
        /// </summary>
        public Guid Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        /// <summary>
        /// The name of the lens asset.
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        /// The manufacturer of the lens.
        /// </summary>
        public string Manufacturer
        {
            get => m_Manufacturer;
            set => m_Manufacturer = value;
        }

        /// <summary>
        /// The model of lens.
        /// </summary>
        public string Model
        {
            get => m_Model;
            set => m_Model = value;
        }

        /// <summary>
        /// The default <see cref="Lens"/> parameters of the current lens asset.
        /// </summary>
        public Lens DefaultValues
        {
            get => m_DefaultValues;
            set => m_DefaultValues = value;
        }

        /// <summary>
        /// The <see cref="LensIntrinsics"/> parameters of the current lens asset.
        /// </summary>
        public LensIntrinsics Intrinsics
        {
            get => m_Intrinsics;
            set => m_Intrinsics = value;
        }

        internal static LensAssetDescriptor Create(LensAsset lensAsset)
        {
            var descriptor = new LensAssetDescriptor();
#if UNITY_EDITOR
            if (lensAsset != null)
            {
                descriptor.Guid = SerializableGuid.FromString(AssetDatabaseUtility.GetAssetGUID(lensAsset));
                descriptor.Name = lensAsset.name;
                descriptor.Manufacturer = lensAsset.Manufacturer;
                descriptor.Model = lensAsset.Model;
                descriptor.DefaultValues = lensAsset.DefaultValues;
                descriptor.Intrinsics = lensAsset.Intrinsics;
            }
#endif
            return descriptor;
        }
    }
}
