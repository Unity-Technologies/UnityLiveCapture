using System;
using System.Collections.Generic;
using System.Linq;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A device used to control a virtual camera.
    /// </summary>
    /// <remarks>
    /// The virtual camera mimics the experience of using a real camera in a Unity scene. The connected client can
    /// control most of the state, such as the camera pose and lens settings, but other features like autofocus need to
    /// be computed in the editor as it needs to query the scene. The render from the virtual camera in the editor can
    /// be streamed to the client to give visual feedback of the camera state, similar to a camera viewfinder.
    /// A <see cref="VirtualCameraActor"/> and a <see cref="IVirtualCameraClient"/> must be assigned before the device
    /// is useful. The actor is needed to store live or evaluated playback state and affect the scene.
    /// </remarks>
    [RequireComponent(typeof(FocusPlaneRenderer))]
    [CreateDeviceMenuItemAttribute("Virtual Camera Device")]
    [HelpURL(Documentation.baseURL + "ref-component-virtual-camera-device" + Documentation.endURL)]
    [DisallowMultipleComponent]
    public class VirtualCameraDevice : CompanionAppDevice<IVirtualCameraClient>, ITimedDataSource
    {
        // If the editor/app runs slower than this fps, generate interpolated lens samples at this fps
        readonly FrameRate k_MinSamplingFrameRate = StandardFrameRate.FPS_60_00;

        static readonly Snapshot[] k_EmptySnapshots = new Snapshot[0];

        static List<VirtualCameraDevice> s_Instances = new List<VirtualCameraDevice>();

        internal static IEnumerable<VirtualCameraDevice> instances => s_Instances;

        [SerializeField, HideInInspector]
        string m_Guid;

        [SerializeField]
        internal LensAsset m_DefaultLensAsset;

        [SerializeField]
        VirtualCameraActor m_Actor;

        [SerializeField, EnumFlagButtonGroup(100f)]
        VirtualCameraChannelFlags m_Channels = VirtualCameraChannelFlags.All;

        /// <summary>
        /// The current lens as set from the inspector or the companion app
        /// </summary>
        /// <remarks>
        /// Note that this is not necessarily the lens used on the actor, due to
        /// post-processing.
        /// </remarks>
        [SerializeField]
        Lens m_Lens = Lens.DefaultParams;

        [SerializeField]
        LensAsset m_LensAsset;

        [SerializeField]
        LensIntrinsics m_LensIntrinsics = LensIntrinsics.DefaultParams;

        [SerializeField]
        CameraBody m_CameraBody = CameraBody.DefaultParams;

        [SerializeField]
        AnchorDeviceSettings m_AnchorDeviceSettings = AnchorDeviceSettings.Default;

        [SerializeField]
        VirtualCameraRigState m_Rig = VirtualCameraRigState.Identity;

        [SerializeField]
        Settings m_Settings = Settings.DefaultData;

        [SerializeField]
        VirtualCameraRecorder m_Recorder = new VirtualCameraRecorder();

        [SerializeField]
        SampleProcessor m_Processor = new SampleProcessor();

        // Obsolete. Use m_SnapshotLibrary instead.
        [SerializeField, NonReorderable]
        internal List<Snapshot> m_Snapshots = new List<Snapshot>();

        [SerializeField]
        SnapshotLibrary m_SnapshotLibrary;

        VideoServer m_VideoServer = new VideoServer();

        [SerializeField, HideInInspector]
        FrameTime m_SyncPresentationOffset;

        [SerializeField]
        bool m_IsSynchronized;

        IRaycaster m_Raycaster;
        ICameraDriver m_Driver;
        FocusPlaneRenderer m_FocusPlaneRenderer;
        MeshIntersectionTracker m_MeshIntersectionTracker = new MeshIntersectionTracker();
        Lens m_LensMetadata;
        LensAsset m_LastLensAsset;
        Snapshot[] m_LastSnapshots;
        bool m_LensIntrinsicsDirty;
        bool m_LastScreenAFRaycastIsValid;
        IScreenshotImpl m_ScreenshotImpl = new ScreenshotImpl();
        bool m_ActorAlignRequested;
        VirtualCameraActor m_LastActor;
        FrameTime m_PresentationFrameTime;
        bool m_ReticlePositionChanged;
        AnchorDeviceSettings m_LastAnchorSettings = AnchorDeviceSettings.Default;
        Pose m_LastActorWorldPose = Pose.identity;

        internal VirtualCameraRecorder Recorder => m_Recorder;
        internal SampleProcessor Processor => m_Processor;

        /// <summary>
        /// Gets the <see cref="VirtualCameraActor"/> currently assigned to this device.
        /// </summary>
        /// <returns>The assigned actor, or null if none is assigned.</returns>
        public VirtualCameraActor Actor
        {
            get => m_Actor;
            set
            {
                if (m_Actor != value)
                {
                    m_Actor = value;
                    ValidateActor();
                }
            }
        }

        /// <summary>
        /// The position and rotation of the current device in world coordinates.
        /// </summary>
        public Pose Pose => m_Rig.Pose;

        /// <summary>
        /// The position and rotation of the world's origin.
        /// </summary>
        public Pose Origin => m_Rig.Origin;

        internal Pose LocalPose => m_Rig.LocalPose;

        /// <summary>
        /// The <see cref="VirtualCamera.Settings"/> of the current device.
        /// </summary>
        internal Settings Settings
        {
            get => m_Settings;
            set
            {
                if (m_Settings != value)
                {
                    m_Settings = value;
                    m_Settings.Validate();
                    RefreshRig();
                    SetFocusMode(m_Settings.FocusMode);
                    Refresh();
                }
            }
        }

        /// <summary>
        /// The <see cref="VirtualCamera.AnchorDeviceSettings"/> of the current device.
        /// </summary>
        internal AnchorDeviceSettings AnchorDeviceSettings
        {
            get => m_AnchorDeviceSettings;
            set => m_AnchorDeviceSettings = value;
        }

        /// <summary>
        /// Enables or disables anchoring.
        /// </summary>
        public bool AnchorEnabled
        {
            get => m_AnchorDeviceSettings.Enabled;
            set => m_AnchorDeviceSettings.Enabled = value;
        }

        /// <summary>
        /// The target to anchor to.
        /// </summary>
        public Transform AnchorTarget
        {
            get => m_AnchorDeviceSettings.Target;
            set => m_AnchorDeviceSettings.Target = value;
        }

        /// <summary>
        /// The <see cref="VirtualCamera.AnchorSettings"/> of the current device.
        /// </summary>
        public AnchorSettings AnchorSettings
        {
            get => m_AnchorDeviceSettings.Settings;
            set => m_AnchorDeviceSettings.Settings = value;
        }

        /// <summary>
        /// The <see cref="VirtualCamera.Lens"/> of the current device.
        /// </summary>
        public Lens Lens
        {
            get => m_Lens;
            set
            {
                if (m_Lens != value)
                {
                    m_Lens = value;
                    ValidateLensIntrinsics();
                    m_Processor.AddLensKeyframe(m_Processor.CurrentTime, m_Lens);
                }
            }
        }

        /// <summary>
        /// The <see cref="VirtualCamera.LensAsset"/> of the current device.
        /// </summary>
        public LensAsset LensAsset
        {
            get => m_LensAsset;
            set
            {
                if (m_LensAsset != value)
                {
                    m_LensAsset = value;
                    ValidateLensIntrinsics();
                    m_Processor.AddLensKeyframe(m_Processor.CurrentTime, m_Lens);
                }
            }
        }

        /// <summary>
        /// The <see cref="VirtualCamera.LensIntrinsics"/> of the current device.
        /// </summary>
        public LensIntrinsics LensIntrinsics => m_LensIntrinsics;

        /// <summary>
        /// The <see cref="VirtualCamera.CameraBody"/> of the current device.
        /// </summary>
        public CameraBody CameraBody => m_CameraBody;

        /// <summary>
        /// The <see cref="VirtualCamera.SnapshotLibrary"/> of the current device.
        /// </summary>
        internal SnapshotLibrary SnapshotLibrary
        {
            get => m_SnapshotLibrary;
            set => m_SnapshotLibrary = value;
        }

        bool TryGetInternalClient(out IVirtualCameraClientInternal client)
        {
            client = GetClient() as IVirtualCameraClientInternal;

            return client != null;
        }

        void InitializeDriver()
        {
            m_Driver = null;

            if (m_Actor != null)
            {
                m_Driver = m_Actor.GetComponent(typeof(ICameraDriver)) as ICameraDriver;
            }
        }

        void SetupProcessor()
        {
            Debug.Assert(m_Processor != null);

            m_Processor.GetLensTarget = () => m_Lens;
            m_Processor.GetRig = () => m_Rig;
            m_Processor.SetRig = (r) => m_Rig = r;
            m_Processor.ApplyDamping = () => !(m_Driver is ICustomDamping);
            m_Processor.GetSettings = () => m_Settings;
            m_Processor.ValidateLens = lens =>
            {
                lens.Validate(m_LensIntrinsics);
                return lens;
            };
        }

        void Reset()
        {
            m_LensAsset = m_DefaultLensAsset;

            if (m_LensAsset != null)
            {
                m_Lens = m_LensAsset.DefaultValues;
            }

            // Find & assign the first actor not already assigned to a device.
            // Do nothing if there's multiple potential candidates.
            if (m_Actor == null)
            {
                VirtualCameraActor validActor = null;
#if UNITY_2023_1_OR_NEWER
                var actors = FindObjectsByType<VirtualCameraActor>(FindObjectsSortMode.None);
#else
                var actors = FindObjectsOfType(typeof(VirtualCameraActor));
#endif

                foreach (var instance in actors)
                {
                    var actor = instance as VirtualCameraActor;

                    // Actors from a different scene aren't a valid object reference
                    if (actor.gameObject.scene == gameObject.scene)
                    {
                        var alreadyAssignedToADevice = false;

                        foreach (var device in s_Instances)
                        {
                            if (device.m_Actor == actor)
                            {
                                alreadyAssignedToADevice = true;
                                break;
                            }
                        }

                        if (alreadyAssignedToADevice)
                        {
                            continue;
                        }

                        if (validActor == null)
                        {
                            validActor = actor;
                        }
                        else
                        {
                            validActor = null;
                            break;
                        }
                    }
                }

                m_Actor = validActor;
            }
        }

        void OnValidate()
        {
            SetupProcessor();
            ValidateActor();
            ValidateLensIntrinsics();

            m_Settings.Validate();
            m_Recorder.Validate();
            m_CameraBody.Validate();
            m_Processor.Validate();

            RefreshRig();

            m_Processor.AddLensKeyframe(m_Processor.CurrentTime, m_Lens);
        }

        void ValidateActor()
        {
            InitializeDriver();

            // If actor has changed
            if (m_Actor != null && m_Actor != m_LastActor)
            {
                m_LastActor = m_Actor;

                UpdateRigOriginFromActor();
            }

            RegisterLiveProperties();
        }

        void UpdateOverlaysIfNeeded(Settings settings)
        {
            m_FocusPlaneRenderer.enabled = settings.FocusPlane;

            var currentCamera = GetCamera();
            if (currentCamera != null && FrameLinesMap.Instance.TryGetInstance(currentCamera, out var frameLines))
            {
                frameLines.GateFit = settings.GateFit;
                frameLines.GateMaskEnabled = settings.GateMask;
                frameLines.AspectRatioEnabled = settings.AspectRatioLines;
                frameLines.CenterMarkerEnabled = settings.CenterMarker;
            }
        }

        void ValidateLensIntrinsics()
        {
            var oldIntrinsics = m_LensIntrinsics;

            if (m_LensAsset != null)
            {
                m_LensIntrinsics = m_LensAsset.Intrinsics;
            }
            else if (m_DefaultLensAsset != null)
            {
                m_LensIntrinsics = m_DefaultLensAsset.Intrinsics;
            }
            else
            {
                m_LensIntrinsics = LensIntrinsics.DefaultParams;
            }

            m_LensIntrinsics.Validate();
            m_Lens.Validate(m_LensIntrinsics);
            m_LensIntrinsicsDirty = m_LensIntrinsics != oldIntrinsics;
        }

        /// <summary>
        /// The device calls this method when the shot has changed.
        /// </summary>
        /// <param name="shot">The <see cref="Shot"/> that changed.</param>
        protected override void OnShotChanged(Shot? shot)
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendVirtualCameraTrackMetadataListDescriptor(VcamTrackMetadataListDescriptor.Create(shot));
            }
        }

        /// <summary>
        /// The device calls this method when a live performance starts and properties are about to change.
        /// </summary>
        /// <param name="previewer">The <see cref="IPropertyPreviewer"/> to use to register live properties.</param>
        protected override void OnRegisterLiveProperties(IPropertyPreviewer previewer)
        {
            Debug.Assert(m_Actor != null);

            foreach (var previewable in m_Actor.GetComponents<IPreviewable>())
            {
                previewable.Register(previewer);
            }
        }

        internal void SetScreenshotImpl(IScreenshotImpl impl)
        {
            Debug.Assert(impl != null);

            m_ScreenshotImpl = impl;
        }

        internal int GetSnapshotCount()
        {
            return m_SnapshotLibrary == null ? 0 : m_SnapshotLibrary.Count;
        }

        internal Snapshot GetSnapshot(int index)
        {
            if (m_SnapshotLibrary == null)
            {
                return null;
            }

            return m_SnapshotLibrary.Get(index);
        }

        internal void TakeSnapshot()
        {
            if (IsRecording || !this.IsLiveAndReady())
            {
                return;
            }

            SnapshotLibraryUtility.EnforceSnapshotLibrary(this);

            var pose = AnchorDeviceSettings.IsActive
                ? m_Actor.transform.GetPose(Space.Self)
                : m_Rig.Pose;

            var snapshot = new Snapshot()
            {
                Pose = pose,
                LensAsset = m_LensAsset,
                Lens = m_Lens,
                CameraBody = m_CameraBody,
            };

            var sceneNumber = 0;
            var shotName = string.Empty;

            snapshot.Time = TakeRecorder.GetPreviewTime();
            snapshot.FrameRate = TakeRecorder.FrameRate;

            if (TakeRecorder.Context is DirectorContext context)
            {
                snapshot.Asset = context.GetShotStorage();
            }

            if (TakeRecorder.Shot is Shot shot)
            {
                shotName = shot.Name;
                sceneNumber = shot.SceneNumber;
            }

            snapshot.Screenshot = m_ScreenshotImpl.Take(
                GetCamera(),
                sceneNumber,
                shotName,
                snapshot.Time,
                snapshot.FrameRate);

            SnapshotLibraryUtility.AddSnapshot(m_SnapshotLibrary, snapshot);
        }

        internal void GoToSnapshot(int index)
        {
            if (m_SnapshotLibrary == null)
            {
                return;
            }

            GoToSnapshot(m_SnapshotLibrary.Get(index));
        }

        internal void GoToSnapshot(Snapshot snapshot)
        {
            if (snapshot == null || IsRecording)
            {
                return;
            }

            SetAnchorEnabled(false);
            SetOrigin(snapshot.Pose);

            if (TakeRecorder.Context is DirectorContext context)
            {
                var index = context.IndexOf(null, snapshot.Asset as PlayableAsset);

                if (index != -1)
                {
                    context.Selection = index;
                    context.SetTime(snapshot.Time);
                }
            }

            Refresh();
        }

        internal void LoadSnapshot(int index)
        {
            if (m_SnapshotLibrary == null)
            {
                return;
            }

            LoadSnapshot(m_SnapshotLibrary.Get(index));
        }

        internal void LoadSnapshot(Snapshot snapshot)
        {
            if (snapshot == null || IsRecording)
            {
                return;
            }

            var snapshotLensAsset = snapshot.LensAsset;

            if (snapshotLensAsset != null)
            {
                LensAsset = snapshotLensAsset;
            }

            Lens = snapshot.Lens;
            m_CameraBody = snapshot.CameraBody;

            GoToSnapshot(snapshot);

            m_Processor.Reset();
        }

        internal void DeleteSnapshot(int index)
        {
            if (m_SnapshotLibrary == null || IsRecording)
            {
                return;
            }

            m_SnapshotLibrary.RemoveAt(index);
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_FocusPlaneRenderer = GetComponent<FocusPlaneRenderer>();

            TimedDataSourceManager.Instance.EnsureIdIsValid(ref m_Guid);
            TimedDataSourceManager.Instance.Register(this);

            m_MeshIntersectionTracker.Initialize();
            m_Raycaster = RaycasterFactory.Create();

            InitializeDriver();
            SetupProcessor();

            s_Instances.Add(this);
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled.
        /// </summary>
        /// <remaks>
        /// This is also called when the object is destroyed and can be used for any cleanup code.
        ///  When scripts are reloaded after compilation has finished, OnDisable will be called, followed by an OnEnable after the script has been loaded.
        /// </remaks>
        protected override void OnDisable()
        {
            s_Instances.Remove(this);

            TimedDataSourceManager.Instance.Unregister(this);

            RaycasterFactory.Dispose(m_Raycaster);
            m_MeshIntersectionTracker.Dispose();

            base.OnDisable();

            Refresh();
        }

        /// <summary>
        /// Indicates whether a device is ready for recording.
        /// </summary>
        /// <returns>
        /// true if ready for recording; otherwise, false.
        /// </returns>
        public override bool IsReady()
        {
            return base.IsReady() && m_Actor != null;
        }

        /// <inheritdoc/>
        public override void Write(ITakeBuilder takeBuilder)
        {
            if (m_Actor == null || m_Recorder.IsEmpty())
            {
                return;
            }

            var metadata = new VirtualCameraTrackMetadata
            {
                Channels = m_Recorder.Channels,
                Lens = m_LensMetadata,
                CameraBody = m_CameraBody,
                LensAsset = m_LensAsset,
                CropAspect = m_Settings.AspectRatio
            };

            double? startTime = IsSynchronized ? m_Recorder.InitialTime : null;

            takeBuilder.CreateAnimationTrack(
                "Virtual Camera",
                m_Actor.Animator,
                m_Recorder.Bake(),
                metadata,
                startTime);

            if (m_AnchorDeviceSettings.IsActive)
            {
                var actorBinding = new AnimatorTakeBinding();
                var anchorTargetBinding = new TransformTakeBinding();

                actorBinding.SetName(m_Actor.Animator.name);
                anchorTargetBinding.SetName(m_AnchorDeviceSettings.Target.gameObject.name);

                takeBuilder.AddBinding(anchorTargetBinding, m_AnchorDeviceSettings.Target);

                var track = takeBuilder.CreateTrack<AnchorTrack>("Virtual Camera Anchor", actorBinding);
                var clip = track.CreateDefaultClip();
                var asset = clip.asset as AnchorPlayableAsset;

                clip.displayName = "Anchor";
                clip.start = takeBuilder.ContextStartTime;
                clip.duration = track.timelineAsset.duration;

                asset.Target = anchorTargetBinding;
                asset.Settings = m_AnchorDeviceSettings.Settings;
            }
        }

        /// <summary>
        /// Gets the video server used to stream the shot from to this virtual camera.
        /// </summary>
        /// <returns>The currently assigned video server, or null if none is assigned.</returns>
        internal VideoServer GetVideoServer()
        {
            return m_VideoServer;
        }

        internal void RequestAlignWithActor()
        {
            m_ActorAlignRequested = true;
        }

        internal void SetOrigin(Pose pose)
        {
            var originRotation = Quaternion.Euler(0f, pose.rotation.eulerAngles.y, 0f);

            m_Rig.Origin = new Pose()
            {
                position = pose.position,
                rotation = originRotation
            };

            m_Rig.WorldToLocal(pose);

            RefreshRig();
        }

        internal void SetARPose(Pose pose)
        {
            m_Rig.ARPose = pose;

            RefreshRig();
        }

        void RefreshRig()
        {
            m_Rig.Refresh(new VirtualCameraRigSettings
            {
                PositionLock = m_Settings.PositionLock,
                RotationLock = m_Settings.RotationLock,
                Rebasing = m_Settings.Rebasing,
                MotionScale = m_Settings.MotionScale,
                ErgonomicTilt = -m_Settings.ErgonomicTilt,
                ZeroDutch = m_Settings.AutoHorizon
            });
        }

        void SetFocusMode(FocusMode focusMode)
        {
            var lastDepthOfFieldEnabled = IsDepthOfFieldEnabled();
            m_Settings.FocusMode = focusMode;
            m_LastScreenAFRaycastIsValid = false;
            var depthOfFieldEnabled = IsDepthOfFieldEnabled();

            if (lastDepthOfFieldEnabled != depthOfFieldEnabled)
            {
                Refresh();

                if (IsRecording)
                {
                    m_Recorder.RecordEnableDepthOfField(depthOfFieldEnabled);
                }
            }
        }

        internal void SetReticlePosition(Vector2 reticlePosition)
        {
            m_Settings.ReticlePosition = reticlePosition;
            m_Settings.Validate();
            m_ReticlePositionChanged = true;

            SendSettings();
        }

        /// <summary>
        /// Called to send the device state to the client.
        /// </summary>
        public override void UpdateClient()
        {
            base.UpdateClient();

            SendChannelFlags();
            SendLensKitIfNeeded();
            SendLens();
            SendCameraBody();
            SendSettings();
            SendVideoStreamState();
            SendSnapshotsIfNeeded();
        }

        internal void MarkClientTime(double time)
        {
            m_PresentationFrameTime = FrameTime.FromSeconds(m_Processor.GetBufferFrameRate(), time);
        }

        /// <inheritdoc/>
        protected override void UpdateDevice()
        {
            if (m_ActorAlignRequested)
            {
                m_ActorAlignRequested = false;
                UpdateRigOriginFromActor();
            }

            ValidateLensIntrinsics();
            UpdateOverlaysIfNeeded(m_Settings);

            var camera = GetCamera();
            m_FocusPlaneRenderer.SetCamera(camera);
            m_VideoServer.Camera = camera;
            m_VideoServer.Update();

            UpdateClient();
        }

        /// <inheritdoc/>
        protected override void LiveUpdate()
        {
            UpdateFocusRigIfNeeded();
            Process();
            UpdateAnchor();
            UpdateActor(m_Rig.Pose, m_Processor.CurrentLens);
        }

        void UpdateActor(Pose pose, Lens lens)
        {
            Debug.Assert(m_Actor != null, "Actor is null");

            if (m_Driver is IAnchorable anchorable)
            {
                if (m_AnchorDeviceSettings.IsActive)
                {
                    anchorable.AnchorTo(m_AnchorDeviceSettings.Target, m_AnchorDeviceSettings.Settings);
                }
                else
                {
                    anchorable.AnchorTo(null, null);
                }
            }

            if (m_Channels.HasFlag(VirtualCameraChannelFlags.Position))
            {
                m_Actor.LocalPosition = pose.position;
                m_Actor.LocalPositionEnabled = true;
            }

            if (m_Channels.HasFlag(VirtualCameraChannelFlags.Rotation))
            {
                m_Actor.LocalEulerAngles = pose.rotation.eulerAngles;
                m_Actor.LocalEulerAnglesEnabled = true;
            }

            var newLens = m_Actor.Lens;
            var newLensIntrinsics = m_Actor.LensIntrinsics;

            if (m_Channels.HasFlag(VirtualCameraChannelFlags.FocalLength))
            {
                newLens.FocalLength = lens.FocalLength;
                newLensIntrinsics.FocalLengthRange = m_LensIntrinsics.FocalLengthRange;
            }

            if (m_Channels.HasFlag(VirtualCameraChannelFlags.FocusDistance))
            {
                m_Actor.DepthOfFieldEnabled = IsDepthOfFieldEnabled();

                newLens.FocusDistance = lens.FocusDistance;
                newLensIntrinsics.CloseFocusDistance = m_LensIntrinsics.CloseFocusDistance;
            }

            if (m_Channels.HasFlag(VirtualCameraChannelFlags.Aperture))
            {
                newLens.Aperture = lens.Aperture;
                newLensIntrinsics.ApertureRange = m_LensIntrinsics.ApertureRange;
            }

            newLensIntrinsics.LensShift = m_LensIntrinsics.LensShift;
            newLensIntrinsics.BladeCount = m_LensIntrinsics.BladeCount;
            newLensIntrinsics.Curvature = m_LensIntrinsics.Curvature;
            newLensIntrinsics.BarrelClipping = m_LensIntrinsics.BarrelClipping;
            newLensIntrinsics.Anamorphism = m_LensIntrinsics.Anamorphism;

            m_Actor.Lens = newLens;
            m_Actor.LensIntrinsics = newLensIntrinsics;
            m_Actor.CameraBody = m_CameraBody;
            m_Actor.CropAspect = m_Settings.AspectRatio;

            if (m_Driver is ICustomDamping customDamping)
                customDamping.SetDamping(m_Settings.Damping);
        }

        void SetAnchorEnabled(bool enable)
        {
            m_AnchorDeviceSettings.Enabled = enable;

            UpdateAnchor();
        }

        void UpdateAnchor()
        {
            var anchorSettingsChanged =
                m_AnchorDeviceSettings.Enabled != m_LastAnchorSettings.Enabled ||
                m_AnchorDeviceSettings.Target != m_LastAnchorSettings.Target ||
                m_AnchorDeviceSettings.Settings.PositionLock != m_LastAnchorSettings.Settings.PositionLock ||
                m_AnchorDeviceSettings.Settings.RotationLock != m_LastAnchorSettings.Settings.RotationLock;

            if (anchorSettingsChanged)
            {
                if (m_AnchorDeviceSettings.IsActive)
                {
                    ActivateAnchor(m_LastActorWorldPose);
                }
                else
                {
                    DeactivateAnchor(m_LastActorWorldPose);
                }
            }

            m_LastAnchorSettings = m_AnchorDeviceSettings;
            m_LastActorWorldPose = m_Actor.transform.GetPose(Space.World);
        }

        void ActivateAnchor(Pose actorPose)
        {
            Debug.Assert(m_AnchorDeviceSettings.IsActive);

            var target = m_AnchorDeviceSettings.Target;
            var settings = m_AnchorDeviceSettings.Settings;
            var anchorPosition = target.TransformPoint(settings.PositionOffset);
            var anchorPose = new Pose(
                AnchorUtility.GetAnchorPosition(anchorPosition, settings),
                AnchorUtility.GetAnchorOrientation(target.eulerAngles, settings));
            var invRotation = Quaternion.Inverse(anchorPose.rotation);

            m_Rig.Origin = Pose.identity;
            m_Rig.WorldToLocal(new Pose()
            {
                position = invRotation * (actorPose.position - anchorPose.position),
                rotation = invRotation * actorPose.rotation
            });
            m_Rig.Rebase(m_Rig.LastInput);

            RefreshRig();
        }

        void DeactivateAnchor(Pose actorPose)
        {
            Debug.Assert(!m_AnchorDeviceSettings.IsActive);

            SetOrigin(actorPose);

            m_Rig.Rebase(m_Rig.LastInput);
        }

        Camera GetCamera()
        {
            var camera = default(Camera);

            if (m_Driver != null)
            {
                camera = m_Driver.GetCamera();
            }

            return camera;
        }

        /// <summary>
        /// Gets the name used for the take asset name.
        /// </summary>
        /// <returns>The name of the asset.</returns>
        protected override string GetAssetName()
        {
            return m_Actor != null ? m_Actor.name : name;
        }

        /// <summary>
        /// The device calls this method when a new client is assigned.
        /// </summary>
        protected override void OnClientAssigned()
        {
            Register();

            m_LastLensAsset = null;
            m_Processor.MarkRigNeedsInitialize();

            StartVideoServer();
        }

        /// <summary>
        /// The device calls this method when the client is unassigned.
        /// </summary>
        protected override void OnClientUnassigned()
        {
            Unregister();
            StopVideoServer();
        }

        void Register()
        {
            if (TryGetInternalClient(out var client))
            {
                client.ChannelFlagsReceived += OnChannelFlagsReceived;
                client.InputSampleReceived += OnInputSampleReceived;
                client.FocalLengthSampleReceived += OnFocalLengthSampleReceived;
                client.FocusDistanceSampleReceived += OnFocusDistanceSampleReceived;
                client.ApertureSampleReceived += OnApertureSampleReceived;
                client.DampingEnabledReceived += OnDampingEnabledReceived;
                client.BodyDampingReceived += OnBodyDampingReceived;
                client.AimDampingReceived += OnAimDampingReceived;
                client.FocalLengthDampingReceived += OnFocalLengthDampingReceived;
                client.FocusDistanceDampingReceived += OnFocusDistanceDampingReceived;
                client.ApertureDampingReceived += OnApertureDampingReceived;
                client.PositionLockReceived += OnPositionLockReceived;
                client.RotationLockReceived += OnRotationLockReceived;
                client.AutoHorizonReceived += OnAutoHorizonReceived;
                client.ErgonomicTiltReceived += OnErgonomicTiltReceived;
                client.RebasingReceived += OnRebasingReceived;
                client.MotionScaleReceived += OnMotionScaleReceived;
                client.JoystickSensitivityReceived += OnJoystickSensitivityReceived;
                client.PedestalSpaceReceived += OnPedestalSpaceReceived;
                client.MotionSpaceReceived += OnMotionSpaceReceived;
                client.FocusModeReceived += OnFocusModeReceived;
                client.FocusReticlePositionReceived += OnFocusReticlePositionReceived;
                client.FocusDistanceOffsetReceived += OnFocusDistanceOffsetReceived;
                client.CropAspectReceived += OnCropAspectReceived;
                client.GateFitReceived += OnGateFitReceived;
                client.ShowGateMaskReceived += OnShowGateMaskReceived;
                client.ShowFrameLinesReceived += OnShowFrameLinesReceived;
                client.ShowCenterMarkerReceived += OnShowCenterMarkerReceived;
                client.ShowFocusPlaneReceived += OnShowFocusPlaneReceived;
                client.SetPoseToOrigin += OnSetPoseToOrigin;
                client.SetLensAsset += OnSetLensAsset;
                client.TakeSnapshot += OnTakeSnapshot;
                client.GoToSnapshot += OnGoToSnapshot;
                client.LoadSnapshot += OnLoadSnapshot;
                client.DeleteSnapshot += OnDeleteSnapshot;
            }
        }

        void Unregister()
        {
            if (TryGetInternalClient(out var client))
            {
                client.ChannelFlagsReceived -= OnChannelFlagsReceived;
                client.InputSampleReceived -= OnInputSampleReceived;
                client.FocalLengthSampleReceived -= OnFocalLengthSampleReceived;
                client.FocusDistanceSampleReceived -= OnFocusDistanceSampleReceived;
                client.ApertureSampleReceived -= OnApertureSampleReceived;
                client.DampingEnabledReceived -= OnDampingEnabledReceived;
                client.BodyDampingReceived -= OnBodyDampingReceived;
                client.AimDampingReceived -= OnAimDampingReceived;
                client.FocalLengthDampingReceived -= OnFocalLengthDampingReceived;
                client.FocusDistanceDampingReceived -= OnFocusDistanceDampingReceived;
                client.ApertureDampingReceived -= OnApertureDampingReceived;
                client.PositionLockReceived -= OnPositionLockReceived;
                client.RotationLockReceived -= OnRotationLockReceived;
                client.AutoHorizonReceived -= OnAutoHorizonReceived;
                client.ErgonomicTiltReceived -= OnErgonomicTiltReceived;
                client.RebasingReceived -= OnRebasingReceived;
                client.MotionScaleReceived -= OnMotionScaleReceived;
                client.JoystickSensitivityReceived -= OnJoystickSensitivityReceived;
                client.PedestalSpaceReceived -= OnPedestalSpaceReceived;
                client.MotionSpaceReceived -= OnMotionSpaceReceived;
                client.FocusModeReceived -= OnFocusModeReceived;
                client.FocusReticlePositionReceived -= OnFocusReticlePositionReceived;
                client.FocusDistanceOffsetReceived -= OnFocusDistanceOffsetReceived;
                client.CropAspectReceived -= OnCropAspectReceived;
                client.GateFitReceived -= OnGateFitReceived;
                client.ShowGateMaskReceived -= OnShowGateMaskReceived;
                client.ShowFrameLinesReceived -= OnShowFrameLinesReceived;
                client.ShowCenterMarkerReceived -= OnShowCenterMarkerReceived;
                client.ShowFocusPlaneReceived -= OnShowFocusPlaneReceived;
                client.SetPoseToOrigin -= OnSetPoseToOrigin;
                client.SetLensAsset -= OnSetLensAsset;
                client.TakeSnapshot -= OnTakeSnapshot;
                client.GoToSnapshot -= OnGoToSnapshot;
                client.LoadSnapshot -= OnLoadSnapshot;
                client.DeleteSnapshot -= OnDeleteSnapshot;
            }
        }

        /// <inheritdoc/>
        protected override void OnStartRecording()
        {
            base.OnStartRecording();

            m_LensMetadata = m_Lens;
            m_Recorder.OnReset = () =>
            {
                RecordCurrentValues();
            };
            m_Recorder.Prepare(m_Channels, TakeRecorder.FrameRate);

            // Lens interpolation requires continuous updates.
            Refresh();
        }

        void UpdateRigOriginFromActor()
        {
            if (m_Actor != null)
            {
                if (AnchorDeviceSettings.IsActive)
                {
                    ActivateAnchor(m_Actor.transform.GetPose(Space.World));
                }
                else
                {
                    SetOrigin(m_Actor.transform.GetPose(Space.Self));
                }
            }
        }

        void OnChannelFlagsReceived(VirtualCameraChannelFlags channelFlags)
        {
            m_Channels = channelFlags;
        }

        void OnInputSampleReceived(InputSample sample)
        {
            m_Processor.AddInputKeyframe(sample);

            Refresh();

            // We use this event to track the time on the client. It's the most frequent
            // provider of timed samples
            if (!IsSynchronized)
            {
                MarkClientTime(sample.Time);
            }
        }

        void OnFocalLengthSampleReceived(FocalLengthSample sample)
        {
            m_Lens.FocalLength = sample.FocalLength;
            m_Lens.Validate(m_LensIntrinsics);
            m_Processor.AddFocalLengthKeyframe(sample.Time, sample.FocalLength);
        }

        void OnFocusDistanceSampleReceived(FocusDistanceSample sample)
        {
            m_Lens.FocusDistance = sample.FocusDistance;
            m_Lens.Validate(m_LensIntrinsics);
            m_Processor.AddFocusDistanceKeyframe(sample.Time, m_Lens.FocusDistance);
        }

        void OnApertureSampleReceived(ApertureSample sample)
        {
            m_Lens.Aperture = sample.Aperture;
            m_Lens.Validate(m_LensIntrinsics);
            m_Processor.AddApertureKeyframe(sample.Time, sample.Aperture);
        }

        void OnDampingEnabledReceived(bool damping)
        {
            var settings = Settings;
            settings.Damping.Enabled = damping;
            Settings = settings;
        }

        void OnBodyDampingReceived(Vector3 damping)
        {
            var settings = Settings;
            settings.Damping.Body = damping;
            Settings = settings;
        }

        void OnAimDampingReceived(float damping)
        {
            var settings = Settings;
            settings.Damping.Aim = damping;
            Settings = settings;
        }

        void OnFocalLengthDampingReceived(float damping)
        {
            var settings = Settings;
            settings.FocalLengthDamping = damping;
            Settings = settings;
        }

        void OnFocusDistanceDampingReceived(float damping)
        {
            var settings = Settings;
            settings.FocusDistanceDamping = damping;
            Settings = settings;
        }

        void OnApertureDampingReceived(float damping)
        {
            var settings = Settings;
            settings.ApertureDamping = damping;
            Settings = settings;
        }

        void OnPositionLockReceived(PositionAxis axes)
        {
            var settings = Settings;
            settings.PositionLock = axes;
            Settings = settings;
        }

        void OnRotationLockReceived(RotationAxis axes)
        {
            var settings = Settings;
            settings.RotationLock = axes;
            Settings = settings;
        }

        void OnAutoHorizonReceived(bool autoHorizon)
        {
            var settings = Settings;
            settings.AutoHorizon = autoHorizon;
            Settings = settings;
        }

        void OnErgonomicTiltReceived(float tilt)
        {
            var settings = Settings;
            settings.ErgonomicTilt = tilt;
            Settings = settings;
        }

        void OnRebasingReceived(bool rebasing)
        {
            var settings = Settings;

            if (!settings.Rebasing && rebasing)
            {
                m_Rig.InitializeJoystickValues();
            }

            settings.Rebasing = rebasing;
            Settings = settings;
        }

        void OnMotionScaleReceived(Vector3 scale)
        {
            var settings = Settings;
            settings.MotionScale = scale;
            Settings = settings;
        }

        void OnJoystickSensitivityReceived(Vector3 sensitivity)
        {
            var settings = Settings;
            settings.JoystickSensitivity = sensitivity;
            Settings = settings;
        }

        void OnPedestalSpaceReceived(Space space)
        {
            var settings = Settings;
            settings.PedestalSpace = space;
            Settings = settings;
        }

        void OnMotionSpaceReceived(Space space)
        {
            var settings = Settings;
            settings.MotionSpace = space;
            Settings = settings;
        }

        void OnFocusModeReceived(FocusMode mode)
        {
            var settings = Settings;
            settings.FocusMode = mode;
            Settings = settings;
        }

        void OnFocusReticlePositionReceived(Vector2 position)
        {
            SetReticlePosition(position);
        }

        void OnFocusDistanceOffsetReceived(float offset)
        {
            var settings = Settings;
            settings.FocusDistanceOffset = offset;
            Settings = settings;
        }

        void OnCropAspectReceived(float aspect)
        {
            var settings = Settings;
            settings.AspectRatio = aspect;
            Settings = settings;
        }

        void OnGateFitReceived(GateFit gateFit)
        {
            var settings = Settings;
            settings.GateFit = gateFit;
            Settings = settings;
        }

        void OnShowGateMaskReceived(bool show)
        {
            var settings = Settings;
            settings.GateMask = show;
            Settings = settings;
        }

        void OnShowFrameLinesReceived(bool show)
        {
            var settings = Settings;
            settings.AspectRatioLines = show;
            Settings = settings;
        }

        void OnShowCenterMarkerReceived(bool show)
        {
            var settings = Settings;
            settings.CenterMarker = show;
            Settings = settings;
        }

        void OnShowFocusPlaneReceived(bool show)
        {
            var settings = Settings;
            settings.FocusPlane = show;
            Settings = settings;
        }

        void OnSetPoseToOrigin()
        {
            m_Rig.Reset();
            Refresh();
        }

        void OnSetLensAsset(SerializableGuid guid)
        {
#if UNITY_EDITOR
            var lensAsset = AssetDatabaseUtility.LoadAssetWithGuid<LensAsset>(guid.ToString());

            if (lensAsset == null)
            {
                lensAsset = m_DefaultLensAsset;
            }

            LensAsset = lensAsset;
#endif
        }

        void OnTakeSnapshot()
        {
            TakeSnapshot();
        }

        void OnGoToSnapshot(int index)
        {
            GoToSnapshot(index);
        }

        void OnLoadSnapshot(int index)
        {
            LoadSnapshot(index);
        }

        void OnDeleteSnapshot(int index)
        {
            DeleteSnapshot(index);
        }

        void RecordCurrentValues()
        {
            var currentPose = m_Rig.Pose;
            var currentLens = m_Processor.CurrentLens;

            m_Recorder.RecordPosition(currentPose.position);
            m_Recorder.RecordRotation(currentPose.rotation);
            m_Recorder.RecordEnableDepthOfField(IsDepthOfFieldEnabled());
            m_Recorder.RecordLensIntrinsics(m_LensIntrinsics);
            m_Recorder.RecordCropAspect(m_Settings.AspectRatio);
            m_Recorder.RecordAperture(currentLens.Aperture);
            m_Recorder.RecordFocalLength(currentLens.FocalLength);
            m_Recorder.RecordFocusDistance(currentLens.FocusDistance);
        }

        bool UpdateAutoFocusDistance(ref float distance, bool reticlePositionChanged)
        {
            var distanceUpdated = false;
            switch (m_Settings.FocusMode)
            {
                case FocusMode.Manual:
                case FocusMode.ReticleAF:
                {
                    if (m_Settings.FocusMode == FocusMode.Manual && !reticlePositionChanged)
                        throw new InvalidOperationException(
                            $"{nameof(UpdateAutoFocusDistance)} was invoked while focusMode is set to [{FocusMode.Manual}], " +
                            $"despite [{nameof(reticlePositionChanged)}] being false.");

                    m_LastScreenAFRaycastIsValid = m_Raycaster.Raycast(m_VideoServer.Camera, m_Settings.ReticlePosition, out distance);

                    if (!m_LastScreenAFRaycastIsValid)
                    {
                        distance = LensLimits.FocusDistance.y;
                    }
                    else if (m_Settings.FocusMode != FocusMode.Manual)
                    {
                        distance += m_Settings.FocusDistanceOffset;
                    }

                    distanceUpdated = true;
                    break;
                }
                case FocusMode.TrackingAF:
                {
                    if (reticlePositionChanged)
                    {
                        if (m_Raycaster.Raycast(m_VideoServer.Camera, m_Settings.ReticlePosition, out var ray, out var gameObject, out var hit))
                        {
                            m_MeshIntersectionTracker.TryTrack(gameObject, ray, hit.point);
                        }
                        else
                        {
                            m_MeshIntersectionTracker.Reset();
                            distance = LensLimits.FocusDistance.y;
                            distanceUpdated = true;
                        }
                    }

                    if (m_MeshIntersectionTracker.TryUpdate(out var worldPosition))
                    {
                        var cameraTransform = m_VideoServer.Camera.transform;
                        var hitVector = worldPosition - cameraTransform.position;
                        var depthVector = Vector3.Project(hitVector, cameraTransform.forward);
                        distance = depthVector.magnitude + m_Settings.FocusDistanceOffset;
                        distanceUpdated = true;
                    }

                    break;
                }
            }

            return distanceUpdated;
        }

        bool IsDepthOfFieldEnabled()
        {
            switch (m_Settings.FocusMode)
            {
                case FocusMode.Manual:
                    return true;
                case FocusMode.ReticleAF:
                    return m_LastScreenAFRaycastIsValid;
                case FocusMode.TrackingAF:
                    return m_MeshIntersectionTracker.CurrentMode != MeshIntersectionTracker.Mode.None;
            }

            return false;
        }

        void StartVideoServer()
        {
            m_VideoServer.StartServer();
            InitializeVideoResolution();
        }

        void StopVideoServer()
        {
            m_VideoServer.StopServer();
        }

        void InitializeVideoResolution()
        {
            if (TryGetInternalClient(out var client))
            {
                m_VideoServer.BaseResolution = client.ScreenResolution;
            }
        }

        void SendChannelFlags()
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendChannelFlags(m_Channels);
            }
        }

        void SendLens()
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendLens(m_Lens);
            }
        }

        void SendLensKitIfNeeded()
        {
            if (m_LensAsset == m_LastLensAsset && !m_LensIntrinsicsDirty)
            {
                return;
            }

            if (TryGetInternalClient(out var client))
            {
                var descriptor = LensKitDescriptor.Create(m_LensAsset);

                client.SendLensKitDescriptor(descriptor);

                m_LastLensAsset = m_LensAsset;
                m_LensIntrinsicsDirty = false;
            }
        }

        void SendSnapshotsIfNeeded()
        {
            //Library is valid and we send contents to the client for the first time,
            //or we update client for the first time with empty contents
            var requiresUpdate = m_LastSnapshots == null;

            if (!requiresUpdate)
            {
                if (m_SnapshotLibrary == null)
                {
                    //Library is set to null.
                    requiresUpdate = m_LastSnapshots != k_EmptySnapshots;
                }
                else
                {
                    //Library's contents are different from what we've sent previously.
                    requiresUpdate = !Enumerable.SequenceEqual(m_LastSnapshots, m_SnapshotLibrary);
                }
            }

            if (requiresUpdate && TryGetInternalClient(out var client))
            {
                if (m_SnapshotLibrary == null)
                {
                    m_LastSnapshots = k_EmptySnapshots;
                }
                else
                {
                    m_LastSnapshots = m_SnapshotLibrary.Snapshots;
                }

                var descriptor = SnapshotListDescriptor.Create(m_LastSnapshots);

                client.SendSnapshotListDescriptor(descriptor);
            }
        }

        void SendCameraBody()
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendCameraBody(m_CameraBody);
            }
        }

        void SendSettings()
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendSettings(m_Settings);
            }
        }

        void SendVideoStreamState()
        {
            if (TryGetInternalClient(out var client))
            {
                client.SendVideoStreamState(new VideoStreamState
                {
                    IsRunning = m_VideoServer.IsRunning,
                    Port = m_VideoServer.Port,
                });
            }
        }

        /// <inheritdoc />
        public bool IsSynchronized
        {
            get => m_IsSynchronized;
            set => m_IsSynchronized = value;
        }

        /// <inheritdoc/>
        public string Id => m_Guid;

        /// <inheritdoc/>
        string IRegistrable.FriendlyName => name;

        /// <inheritdoc/>
        public ISynchronizer Synchronizer { get; set; }

        /// <inheritdoc/>
        public FrameRate FrameRate => m_Processor.GetBufferFrameRate();

        /// <inheritdoc/>
        public int BufferSize
        {
            get => m_Processor.BufferSize;
            set => m_Processor.BufferSize = value;
        }

        /// <inheritdoc/>
        public int? MaxBufferSize => null;

        /// <inheritdoc/>
        public int? MinBufferSize => m_Processor.MinBufferSize;

        /// <inheritdoc/>
        public FrameTime Offset
        {
            get => m_SyncPresentationOffset;
            set => m_SyncPresentationOffset = value;
        }

        /// <inheritdoc />
        Object ITimedDataSource.UndoTarget => this;

        /// <inheritdoc />
        public bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample)
        {
            if (m_Processor.TryGetBufferRange(out oldestSample, out newestSample))
            {
                oldestSample += m_SyncPresentationOffset;
                newestSample += m_SyncPresentationOffset;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public TimedSampleStatus PresentAt(FrameTimeWithRate presentTime)
        {
            Debug.Assert(IsSynchronized, "Attempting to call PresentAt() when data source is not being synchronized");

            var requestedFrameTime = FrameTime.Remap(presentTime.Time, presentTime.Rate, m_Processor.GetBufferFrameRate());

            m_PresentationFrameTime = requestedFrameTime - m_SyncPresentationOffset;

            return m_Processor.GetStatusAt(m_PresentationFrameTime);
        }

        void Process()
        {
            // Note that is it not possible for lens keyframes to be behind, since the postprocessor
            // does extrapolation. However, it is possible for lens keyframes to be ahead or missing.
            foreach (var sample in m_Processor.ProcessTo(m_PresentationFrameTime))
            {
                // Record post-processed samples. Depending on the rate at which PresentAt is called,
                // we could potentially generate more than one sample per update.
                if (IsRecording)
                {
                    var time = sample.time;
                    var pose = sample.pose;
                    var lens = sample.lens;

                    m_Recorder.Update(time);

                    if (pose.HasValue)
                    {
                        m_Recorder.RecordPosition(pose.Value.position);
                        m_Recorder.RecordRotation(pose.Value.rotation);
                    }

                    if (lens.HasValue)
                    {
                        m_Recorder.RecordAperture(lens.Value.Aperture);
                        m_Recorder.RecordFocalLength(lens.Value.FocalLength);
                        m_Recorder.RecordFocusDistance(lens.Value.FocusDistance);
                    }
                }
            }
        }

        void UpdateFocusRigIfNeeded()
        {
            if (!m_ReticlePositionChanged &&
                (m_Settings.FocusMode == FocusMode.Clear || m_Settings.FocusMode == FocusMode.Manual))
            {
                return;
            }

            var distance = m_Lens.FocusDistance;

            if (UpdateAutoFocusDistance(ref distance, m_ReticlePositionChanged))
            {
                // Expose the target (unfiltered) focus distance for the UI
                m_Lens.FocusDistance = distance;
                m_Lens.Validate(m_LensIntrinsics);
                m_Processor.AddFocusDistanceKeyframe(m_Processor.CurrentTime, m_Lens.FocusDistance);
            }

            m_ReticlePositionChanged = false;
        }
    }
}
