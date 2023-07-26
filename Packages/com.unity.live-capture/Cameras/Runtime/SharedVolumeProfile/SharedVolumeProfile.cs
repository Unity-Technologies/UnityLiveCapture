using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.Cameras
{
    /// <summary>
    /// A component that manages an instance of <see cref="VolumeProfile"/>.
    /// </summary>
    /// <remarks>
    /// This component guarantees that the profile instance is unique and that the GameObject 
    /// can be duplicated safely, avoding the issue of multiple GameObjects sharing the same profile.
    /// </remarks>
    [ExecuteAlways]
    [ExcludeFromPreset]
    [AddComponentMenu("")]
    [RequireComponent(typeof(Camera))]
#if SRP_CORE_14_0_OR_NEWER
    [RequireComponent(typeof(Volume))]
    [RequireComponent(typeof(SphereCollider))]
#endif
    [HelpURL(Documentation.baseURL + "ref-component-shared-volume-profile" + Documentation.endURL)]
    public sealed class SharedVolumeProfile : MonoBehaviour
    {
#if SRP_CORE_14_0_OR_NEWER
        const string k_ProfileName = "Volume Profile";

        [SerializeField, HideInInspector]
        VolumeProfile m_Profile;

        void Reset()
        {
            var volume = GetComponent<Volume>();

            volume.isGlobal = false;

            var sphereCollider = GetComponent<SphereCollider>();

            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.01f;
        }

        /// <summary>
        /// Gets or creates the profile instance.
        /// </summary>
        public VolumeProfile GetOrCreateProfile()
        {
            PrepareVolume();

            Debug.Assert(m_Profile != null);

            return m_Profile;
        }

        /// <summary>
        /// Gets or creates a volume component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the volume component.</typeparam>
        public T GetOrCreateVolumeComponent<T>() where T : VolumeComponent
        {
            var profile = GetOrCreateProfile();

            return VolumeComponentUtility.GetOrAddVolumeComponent<T>(profile);
;
        }

        /// <summary>
        /// Gets the profile instance.
        /// </summary>
        /// <param name="profile">The profile instance.</param>
        /// <returns>True if the profile instance was found, false otherwise.</returns>
        public bool TryGetProfile(out VolumeProfile profile)
        {
            profile = m_Profile;

            return profile != null;
        }

        void OnDestroy()
        {
            VolumeProfileTracker.Instance.UnregisterProfile(m_Profile);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_Profile);
        }

        bool TryGetOrCreateVolume(out Volume volume)
        {
            volume = VolumeComponentUtility.GetOrAddVolume(gameObject, false);

            // Profile instances will end up being shared when duplicating objects through serialization.
            // We have to invalidate the profile when we don't know who created it.
            if (!VolumeProfileTracker.Instance.TryRegisterProfileOwner(m_Profile, volume))
            {
                m_Profile = null;
            }

            if (m_Profile == null)
            {
                m_Profile = volume.profile;

                if (string.IsNullOrEmpty(m_Profile.name))
                {
                    m_Profile.name = k_ProfileName;
                }

                VolumeProfileTracker.Instance.TryRegisterProfileOwner(m_Profile, volume);
            }

            return volume != null;
        }

        /// <summary>
        /// Gets a volume component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the volume component.</typeparam>
        /// <param name="component">The volume component.</param>
        /// <returns>True if the volume component was found, false otherwise.</returns>
        public bool TryGetVolumeComponent<T>(out T component) where T : VolumeComponent
        {
            component = null;

            return TryGetProfile(out var profile) && profile.TryGet<T>(out component);
        }

        /// <summary>
        /// Destroys a volume component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the volume component.</typeparam>
        public void DestroyVolumeComponent<T>() where T : VolumeComponent
        {
            if (TryGetVolumeComponent<T>(out var component))
            {
                AdditionalCoreUtils.DestroyIfNeeded(ref component);
            }
        }

        void PrepareVolume()
        {
            if (TryGetOrCreateVolume(out var volume))
            {
                volume.profile = m_Profile;
            }
        }
#endif
    }
}
