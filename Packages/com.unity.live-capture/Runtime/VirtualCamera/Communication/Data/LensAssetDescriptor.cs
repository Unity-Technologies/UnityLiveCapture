using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [Serializable]
    class LensAssetDescriptorV0
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
        float m_DefaultFocalLength;
        [SerializeField]
        float m_DefaultFocusDistance;
        [SerializeField]
        float m_DefaultAperture;
        [SerializeField]
        Vector2 m_FocalLengthRange;
        [SerializeField]
        float m_CloseFocusDistance;
        [SerializeField]
        Vector2 m_ApertureRange;
        [SerializeField]
        Vector2 m_LensShift;
        [SerializeField]
        int m_BladeCount;
        [SerializeField]
        Vector2 m_Curvature;
        [SerializeField]
        float m_BarrelClipping;
        [SerializeField]
        float m_Anamorphism;

        public static explicit operator LensAssetDescriptorV0(LensAssetDescriptor lensAsset)
        {
            return new LensAssetDescriptorV0
            {
                m_Guid = lensAsset.Guid,
                m_Name = lensAsset.Name,
                m_Manufacturer = lensAsset.Manufacturer,
                m_Model = lensAsset.Model,
                m_DefaultFocalLength = lensAsset.DefaultValues.FocalLength,
                m_DefaultFocusDistance = lensAsset.DefaultValues.FocusDistance,
                m_DefaultAperture = lensAsset.DefaultValues.Aperture,
                m_FocalLengthRange = lensAsset.Intrinsics.FocalLengthRange,
                m_CloseFocusDistance = lensAsset.Intrinsics.CloseFocusDistance,
                m_ApertureRange = lensAsset.Intrinsics.ApertureRange,
                m_LensShift = lensAsset.Intrinsics.LensShift,
                m_BladeCount = lensAsset.Intrinsics.BladeCount,
                m_Curvature = lensAsset.Intrinsics.Curvature,
                m_BarrelClipping = lensAsset.Intrinsics.BarrelClipping,
                m_Anamorphism = lensAsset.Intrinsics.Anamorphism,
            };
        }

        public static explicit operator LensAssetDescriptor(LensAssetDescriptorV0 lensAssetDescriptor)
        {
            return new LensAssetDescriptor
            {
                Guid = lensAssetDescriptor.m_Guid,
                Name = lensAssetDescriptor.m_Name,
                Manufacturer = lensAssetDescriptor.m_Manufacturer,
                Model = lensAssetDescriptor.m_Model,
                DefaultValues = new Lens
                {
                    FocalLength = lensAssetDescriptor.m_DefaultFocalLength,
                    FocusDistance = lensAssetDescriptor.m_DefaultFocusDistance,
                    Aperture = lensAssetDescriptor.m_DefaultAperture,
                },
                Intrinsics = new LensIntrinsics
                {
                    FocalLengthRange = lensAssetDescriptor.m_FocalLengthRange,
                    CloseFocusDistance = lensAssetDescriptor.m_CloseFocusDistance,
                    ApertureRange = lensAssetDescriptor.m_ApertureRange,
                    LensShift = lensAssetDescriptor.m_LensShift,
                    BladeCount = lensAssetDescriptor.m_BladeCount,
                    Curvature = lensAssetDescriptor.m_Curvature,
                    BarrelClipping = lensAssetDescriptor.m_BarrelClipping,
                    Anamorphism = lensAssetDescriptor.m_Anamorphism,
                },
            };
        }
    }
}
