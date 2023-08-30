using System;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A class that stores information of a lens kit. The client uses this information to build its UI.
    /// </summary>
    [Serializable]
    class LensKitDescriptor
    {
        [SerializeField]
        string m_Name;

        [SerializeField]
        int m_SelectedLensAsset = -1;

        [SerializeField]
        LensAssetDescriptor[] m_Lenses;

        /// <summary>
        /// The name of the lens kit.
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        /// The index of the selected lens asset in the lenses list.
        /// </summary>
        public int SelectedLensAsset
        {
            get => m_SelectedLensAsset;
            set => m_SelectedLensAsset = value;
        }

        /// <summary>
        /// The list of available lenses.
        /// </summary>
        public LensAssetDescriptor[] Lenses
        {
            get => m_Lenses;
            set => m_Lenses = value;
        }

        internal static LensKitDescriptor Create(LensAsset lensAsset)
        {
            var lensKit = default(LensKit);
#if UNITY_EDITOR
            lensKit = AssetDatabaseUtility.GetSubAssets<LensKit>(lensAsset).FirstOrDefault();
#endif
            return Create(lensKit, lensAsset);
        }

        static LensKitDescriptor Create(LensKit lensKit, LensAsset selectedLensAsset)
        {
            var descriptor = new LensKitDescriptor();

            if (lensKit != null)
            {
                var lenses = lensKit.Lenses;
                descriptor.Name = lensKit.name;
                descriptor.SelectedLensAsset = Array.IndexOf(lenses, selectedLensAsset);
                descriptor.Lenses = lenses.Select(lensAsset => LensAssetDescriptor.Create(lensAsset)).ToArray();
            }

            return descriptor;
        }
    }
}
