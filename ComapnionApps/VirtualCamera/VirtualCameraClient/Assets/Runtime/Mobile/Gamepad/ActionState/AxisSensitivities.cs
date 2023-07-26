using System;
using System.Collections.Generic;
using System.Linq;
using Unity.LiveCapture;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Manages the user-defined sensitivity of each <see cref="AxisID"/>.
    /// </summary>
    /// <remarks>Sensitivities are 1.0 by default.</remarks>
    class AxisSensitivities
    {
        /// <summary>
        /// Saves/loads sensitivities as a flat list of axis names and a flat list of corresponding values.
        /// </summary>
        [Serializable]
        public class JsonAxisSensitivities
        {
            static readonly string[] AxisNames = Enum.GetNames(typeof(AxisID)).ToArray();

            [SerializeField]
            List<string> m_Names = new List<string>();

            [SerializeField]
            List<float> m_Values = new List<float>();

            public void FromSensitivities(AxisSensitivities sensitivities)
            {
                m_Names.Clear();
                m_Values.Clear();

                for (var i = 0; i < sensitivities.m_Sensitivities.Length && i < AxisNames.Length; ++i)
                {
                    var axisName = AxisNames[i];
                    var axisSensitivity = sensitivities.m_Sensitivities[i];

                    m_Names.Add(axisName);
                    m_Values.Add(axisSensitivity);
                }
            }

            public AxisSensitivities ToSensitivities()
            {
                var sensitivities = new AxisSensitivities();

                for (var i = 0; i < m_Names.Count && i < m_Values.Count; i++)
                {
                    var axisName = m_Names[i];
                    var axisSensitivity = m_Values[i];

                    var axisIndex = AxisNames.FindIndex(x => x == axisName);

                    if (axisIndex >= 0)
                    {
                        var axisID = (AxisID) i;
                        sensitivities.SetSensitivity(axisID, axisSensitivity);
                    }
                }

                return sensitivities;
            }
        }

        static readonly string[] AxisDisplayNames = Enum.GetNames(typeof(AxisID)).Select(Utilities.InsertSpaceBetweenCapitals).ToArray();

        float[] m_Sensitivities = new float[(int) AxisID.Count];

        public AxisSensitivities()
        {
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < m_Sensitivities.Length; i++)
            {
                m_Sensitivities[i] = 1.0f;
            }
        }

        public float GetSensitivity(AxisID axisID)
        {
            return m_Sensitivities[(int) axisID];
        }

        public void SetSensitivity(AxisID axisID, float sensitivity)
        {
            m_Sensitivities[(int) axisID] = sensitivity;
        }

        public string GetAxisDisplayName(AxisID axisID)
        {
            return AxisDisplayNames[(int) axisID];
        }
    }
}
