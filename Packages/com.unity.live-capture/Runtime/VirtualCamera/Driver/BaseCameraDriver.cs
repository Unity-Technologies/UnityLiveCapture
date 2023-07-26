using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Implements the Camera driver, that is, the component responsible for applying the virtual camera state
    /// to the current scene's camera and post effects.
    /// </summary>
    /// <remarks>
    /// Since we support multiple render pipelines and optionally Cinemachine, we split the code responsible
    /// for each pipeline and Cinemachine among different components. Components are added based on which packages
    /// are installed/used.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VirtualCameraActor))]
    [ExecuteAlways]
    abstract class BaseCameraDriver : MonoBehaviour, ICameraDriver, IPreviewable, IAnchorable
    {
        VirtualCameraActor m_VirtualCameraActor;
        Transform m_AnchorTarget;
        AnchorSettings? m_AnchorSettings;
        int m_LastAnchorSessionID;
        int m_AnchorSessionID;
        Pose? m_AnchorPose;
        Vector3? m_AnchorEuler;

        float m_CachedFocusDistance;
        bool m_CachedFocusDistanceEnabled;

        /// <inheritdoc/>
        public abstract Camera GetCamera();

        protected abstract ICameraDriverImpl GetImplementation();

        protected virtual void OnEnable()
        {
            m_VirtualCameraActor = GetComponent<VirtualCameraActor>();

            CameraDriverUpdateManager.Instance.Register(this);
        }

        protected virtual void OnDisable()
        {
            CameraDriverUpdateManager.Instance.Unregister(this);
        }

        protected virtual void Awake() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnValidate() { }

        bool TryGetImplementation(out ICameraDriverImpl impl)
        {
            impl = null;

            try
            {
                impl = GetImplementation();
            }
            catch { }

            return impl != null;
        }

        void Update()
        {
            m_VirtualCameraActor.LocalPositionEnabled = false;
            m_VirtualCameraActor.LocalEulerAnglesEnabled = false;
        }

        /// <param name="deltaTime">Can usually be <see cref="Time.deltaTime"/></param>
        internal void PostLateUpdate(float deltaTime)
        {
            if (TryGetImplementation(out var impl))
            {
                Assert.IsNotNull(m_VirtualCameraActor);
                var lens = m_VirtualCameraActor.Lens;
                var lensIntrinsics = m_VirtualCameraActor.LensIntrinsics;
                var cameraBody = m_VirtualCameraActor.CameraBody;

                if (IsAnchorActive())
                {
                    if (m_LastAnchorSessionID != m_AnchorSessionID)
                    {
                        m_LastAnchorSessionID = m_AnchorSessionID;
                        m_AnchorPose = null;
                        m_AnchorEuler = null;
                    }

                    var settings = m_AnchorSettings.Value;

                    if (!m_AnchorEuler.HasValue)
                    {
                        m_AnchorEuler = m_AnchorTarget.eulerAngles;
                    }

                    m_AnchorEuler = MathUtility.ClosestEuler(m_AnchorTarget.rotation, m_AnchorEuler.Value);

                    var anchorPosition = m_AnchorTarget.TransformPoint(settings.PositionOffset);
                    var anchorPose = new Pose()
                    {
                        position = AnchorUtility.GetAnchorPosition(anchorPosition, settings),
                        rotation = AnchorUtility.GetAnchorOrientation(m_AnchorEuler.Value, settings)
                    };

                    if (!m_AnchorPose.HasValue)
                    {
                        m_AnchorPose = anchorPose;
                    }

                    if (settings.Damping.Enabled)
                    {
                        anchorPose = VirtualCameraDamping.Calculate(m_AnchorPose.Value, anchorPose, settings.Damping, deltaTime);
                    }

                    if (m_VirtualCameraActor.LocalPositionEnabled)
                    {
                        var localPosition = m_VirtualCameraActor.LocalPosition;

                        transform.position = anchorPose.rotation * localPosition + anchorPose.position;
                    }

                    if (m_VirtualCameraActor.LocalEulerAnglesEnabled)
                    {
                        var localRotation = Quaternion.Euler(m_VirtualCameraActor.LocalEulerAngles);

                        transform.rotation = anchorPose.rotation * localRotation;
                    }

                    m_AnchorPose = anchorPose;
                }
                else
                {
                    m_AnchorPose = null;
                    m_AnchorEuler = null;

                    if (m_VirtualCameraActor.LocalPositionEnabled)
                    {
                        transform.localPosition = m_VirtualCameraActor.LocalPosition;
                    }

                    if (m_VirtualCameraActor.LocalEulerAnglesEnabled)
                    {
                        transform.localEulerAngles = m_VirtualCameraActor.LocalEulerAngles;
                    }
                }

                lens.Validate(lensIntrinsics);
                impl.EnableDepthOfField(m_VirtualCameraActor.DepthOfFieldEnabled);
                impl.SetFocusDistance(lens.FocusDistance);
                impl.SetPhysicalCameraProperties(lens, lensIntrinsics, cameraBody);

                var driverCamera = GetCamera();
                if (driverCamera != null)
                {
                    if (FocusPlaneMap.Instance.TryGetInstance(driverCamera, out var focusPlane))
                        focusPlane.SetFocusDistance(lens.FocusDistance);

                    if (FrameLinesMap.Instance.TryGetInstance(driverCamera, out var frameLines))
                        frameLines.CropAspect = m_VirtualCameraActor.CropAspect;
                }

                m_CachedFocusDistanceEnabled = m_VirtualCameraActor.DepthOfFieldEnabled;
                m_CachedFocusDistance = lens.FocusDistance;
            }
        }

        void OnDrawGizmos()
        {
            // Visualize focus distance.
            if (m_CachedFocusDistanceEnabled)
            {
                var cameraTransform = GetCamera()?.transform;
                if (cameraTransform != null)
                {
                    var position = cameraTransform.position;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(position, position + cameraTransform.forward * m_CachedFocusDistance);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Register(IPropertyPreviewer previewer)
        {
            previewer.Register(transform, "m_LocalPosition.x");
            previewer.Register(transform, "m_LocalPosition.y");
            previewer.Register(transform, "m_LocalPosition.z");
            previewer.Register(transform, "m_LocalRotation.x");
            previewer.Register(transform, "m_LocalRotation.y");
            previewer.Register(transform, "m_LocalRotation.z");
            previewer.Register(transform, "m_LocalRotation.w");
            previewer.Register(transform, "m_LocalEulerAnglesHint.x");
            previewer.Register(transform, "m_LocalEulerAnglesHint.y");
            previewer.Register(transform, "m_LocalEulerAnglesHint.z");
        }

        void IAnchorable.AnchorTo(Transform target, AnchorSettings? settings, int? sessionID)
        {
            m_AnchorTarget = target;
            m_AnchorSettings = settings;
            m_AnchorSessionID = sessionID.GetValueOrDefault(-1);
        }

        void IAnchorable.GetConfiguration(out Transform target, out AnchorSettings? settings, out int? sessionID)
        {
            target = m_AnchorTarget;
            settings = m_AnchorSettings;
            sessionID = m_AnchorSessionID;
        }

        bool IsAnchorActive()
        {
            return m_AnchorSettings.HasValue && m_AnchorTarget != null;
        }
    }
}
