using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Asset that stores a collection of <see cref="LensAsset"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="VirtualCameraDevice"/> uses the available assets in the project to show a <see cref="LensAsset"/>
    /// menu in its inspector.
    /// </remarks>
    [CreateAssetMenu(menuName = "Live Capture/Virtual Camera/Lens Kit", order = 2)]
    [HelpURL(Documentation.baseURL + "ref-asset-lens-kit" + Documentation.endURL)]
    [ExcludeFromPreset]
    public class LensKit : ScriptableObject
    {
        [SerializeField, NonReorderable]
        List<LensAsset> m_Lenses = new List<LensAsset>();

        /// <summary>
        /// The <see cref="LensAsset"/> assets contained in this <see cref="LensKit"/>.
        /// </summary>
        public LensAsset[] Lenses => m_Lenses.ToArray();
    }
}
