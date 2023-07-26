using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Stores the data of an aspect ratio preset.
    /// </summary>
    [Serializable]
    public struct AspectRatioPreset
    {
        [SerializeField, Tooltip("The name of the preset.")]
        string m_Name;

        [SerializeField, Tooltip("The aspect ratio of the preset.")]
        float m_AspectRatio;

        /// <summary>
        /// The name of the preset.
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        /// The aspect ratio of the preset.
        /// </summary>
        public float AspectRatio
        {
            get => m_AspectRatio;
            set => m_AspectRatio = value;
        }
    }

    /// <summary>
    /// Asset that stores lists of presets for aspect ratios.
    /// </summary>
    [CreateAssetMenu(fileName = "Aspect Ratio Presets", menuName = "Live Capture/Virtual Camera/Aspect Ratio Presets", order = 1)]
    [HelpURL(Documentation.baseURL + "ref-asset-aspect-ratio-presets" + Documentation.endURL)]
    [ExcludeFromPreset]
    public class AspectRatioPresets : ScriptableObject
    {
        [SerializeField]
        List<AspectRatioPreset> m_AspectRatios = new List<AspectRatioPreset>();

        /// <summary>
        /// The array of aspect ratio presets.
        /// </summary>
        /// <remarks>This will return a new copy of the array.</remarks>
        public AspectRatioPreset[] AspectRatios => m_AspectRatios.ToArray();
    }
}
