using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An asset that stores shot definitions. They are a convenient way to preconfigure sets of shots
    /// to use with the <see cref="ShotPlayer" />
    /// </summary>
    [CreateAssetMenu(menuName = "Live Capture/Shot Library", order = -1)]
    [HelpURL(Documentation.baseURL + "ref-asset-shot-library" + Documentation.endURL)]
    public class ShotLibrary : ScriptableObject
    {
        const int k_MinSceneNumber = 1;
        const int k_MinTakeNumber = 1;
        const string k_Assets = "Assets";

        [SerializeField]
        List<Shot> m_Shots = new List<Shot>();

        internal int Version { get; private set; }

        /// <summary>
        /// The array of <see cref="Shot" /> contained in this asset.
        /// </summary>
        public Shot[] Shots
        {
            get => m_Shots.ToArray();
            set
            {
                m_Shots = new List<Shot>(value);

                Validate();
                IncrementVersion();
            }
        }

        /// <summary>
        /// The number of shots currently stored.
        /// </summary>
        public int Count => m_Shots.Count;

        void OnValidate()
        {
            Validate();
            IncrementVersion();
        }

        void Validate()
        {
            for (var i = 0; i < m_Shots.Count; ++i)
            {
                var shot = m_Shots[i];

                if (string.IsNullOrEmpty(shot.Directory) || !shot.Directory.StartsWith(k_Assets))
                {
                    shot.Directory = k_Assets;
                }

                if (string.IsNullOrEmpty(shot.Name))
                {
                    shot.Name = $"Shot {i}";
                }

                shot.SceneNumber = Mathf.Clamp(shot.SceneNumber, k_MinSceneNumber, int.MaxValue);
                shot.TakeNumber = Mathf.Clamp(shot.TakeNumber, k_MinTakeNumber, int.MaxValue);
                m_Shots[i] = shot;
            }
        }

        /// <summary>
        /// Returns the shot at the specified index.
        /// </summary>
        /// <param name="index">The index of the shot.</param>
        /// <returns>The shot at the requested index.</returns>
        public Shot GetShot(int index)
        {
            return m_Shots[index];
        }

        /// <summary>
        /// Updates the data of a shot at a specified index.
        /// </summary>
        /// <param name="index">The index of the shot.</param>
        /// <param name="shot">The shot data.</param>
        public void SetShot(int index, Shot shot)
        {
            if (index >= 0 && index < m_Shots.Count)
            {
                m_Shots[index] = shot;

                Validate();
                IncrementVersion();
            }
        }

        void IncrementVersion()
        {
            unchecked
            {
                ++Version;
            }
        }
    }
}
