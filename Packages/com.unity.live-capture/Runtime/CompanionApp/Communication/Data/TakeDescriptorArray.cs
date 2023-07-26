using System;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp.Networking
{
    [Serializable]
    class TakeDescriptorArrayV0
    {
        [SerializeField]
        TakeDescriptorV0[] m_Takes;

        public static explicit operator TakeDescriptorArrayV0(TakeDescriptor[] takes)
        {
            TakeDescriptorV0[] result;

            if (takes == null)
            {
                result = new TakeDescriptorV0[0];
            }
            else
            {
                result = takes.Select(t => (TakeDescriptorV0)t).ToArray();
            }

            return new TakeDescriptorArrayV0
            {
                m_Takes = result,
            };
        }

        public static explicit operator TakeDescriptor[](TakeDescriptorArrayV0 takes)
        {
            if (takes == null || takes.m_Takes == null)
            {
                return new TakeDescriptor[0];
            }

            return takes.m_Takes.Select(t => (TakeDescriptor)t).ToArray();
        }
    }
}
