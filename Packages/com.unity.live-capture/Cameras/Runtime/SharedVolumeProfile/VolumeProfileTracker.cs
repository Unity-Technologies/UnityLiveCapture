using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.Cameras
{
    class VolumeProfileTracker
    {
        public static VolumeProfileTracker Instance { get; } = new VolumeProfileTracker();

        Dictionary<ScriptableObject, UnityObject> m_Profiles = new Dictionary<ScriptableObject, UnityObject>();

        public bool TryRegisterProfileOwner(ScriptableObject profile, UnityObject obj)
        {
            if (profile == null || obj == null)
            {
                return false;
            }

            if (!m_Profiles.TryGetValue(profile, out var owner))
            {
                m_Profiles[profile] = obj;
                owner = obj;
            }

            return owner == obj;
        }

        public bool UnregisterProfile(ScriptableObject profile)
        {
            if (profile == null)
            {
                return false;
            }

            return m_Profiles.Remove(profile);
        }
    }
}
