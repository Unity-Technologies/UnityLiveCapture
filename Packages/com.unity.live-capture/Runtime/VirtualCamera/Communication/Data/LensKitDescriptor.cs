using System;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [Serializable]
    class LensKitDescriptorV0
    {
        [SerializeField]
        string m_Name;
        [SerializeField]
        LensAssetDescriptorV0[] m_Lenses;

        public static explicit operator LensKitDescriptorV0(LensKitDescriptor lensKit)
        {
            return new LensKitDescriptorV0
            {
                m_Name = lensKit.Name,
                m_Lenses = lensKit.Lenses.Select(lens => (LensAssetDescriptorV0)lens).ToArray()
            };
        }

        public static explicit operator LensKitDescriptor(LensKitDescriptorV0 lensKitDescriptor)
        {
            return new LensKitDescriptor
            {
                Name = lensKitDescriptor.m_Name,
                Lenses = lensKitDescriptor.m_Lenses.Select(lens => (LensAssetDescriptor)lens).ToArray()
            };
        }
    }
}
