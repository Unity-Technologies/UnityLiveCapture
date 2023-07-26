using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    /// <summary>
    /// Provides access to an IVirtualCameraDeviceProxy, introduced for testing purposes.
    /// </summary>
    interface ILiveCaptureBridge
    {
        /// <summary>
        /// Provides access to a VirtualCameraDevice proxy, if a valid device exists.
        /// A VirtualCameraDevice is considered valid if it is currently streaming video previewed in the GameView.
        /// </summary>
        /// <param name="proxy">Proxy providing a controlled access to the currently streaming VirtualCameraDevice.</param>
        /// <returns>True if a valid proxy is available, false otherwise.</returns>
        bool TryGetVirtualCameraDevice(out IVirtualCameraDeviceProxy proxy);
    }

    /// <summary>
    /// Proxy to a VirtualCameraDevice, exposes only the subset of the API needed by the reticle feature.
    /// </summary>
    interface IVirtualCameraDeviceProxy
    {
        Vector2 ReticlePosition { get; set; }
        FocusMode FocusMode { get; }
        bool IsLive { get; }
    }

    class VirtualCameraDeviceProxy : IVirtualCameraDeviceProxy
    {
        public VirtualCameraDevice Device { get; set; }

        public Vector2 ReticlePosition
        {
            get => Device.Settings.ReticlePosition;
            set => Device.SetReticlePosition(value);
        }

        public FocusMode FocusMode => Device.Settings.FocusMode;

        public bool IsLive => TakeRecorder.IsLive;
    }

    class LiveCaptureBridge : ILiveCaptureBridge
    {
        readonly VirtualCameraDeviceProxy m_Proxy = new VirtualCameraDeviceProxy();

        /// <inheritdoc />
        public bool TryGetVirtualCameraDevice(out IVirtualCameraDeviceProxy proxy)
        {
            // Look for the device corresponding to the currently streamed preview if any.
            foreach (var device in VirtualCameraDevice.instances)
            {
                var videoServer = device.GetVideoServer();
                var camera = videoServer.Camera;

                if (videoServer.IsRunning && camera.isActiveAndEnabled)
                {
                    m_Proxy.Device = device;
                    proxy = m_Proxy;
                    return true;
                }
            }

            proxy = null;
            return false;
        }
    }

    class FocusReticleControllerImplementation : BaseFocusReticleControllerImplementation
    {
        Canvas m_Canvas;

        public Canvas Canvas
        {
            set => m_Canvas = value;
        }

        protected override void SetReticleActive(bool value)
        {
            base.SetReticleActive(value);
            m_Canvas.enabled = value;
        }
    }

    [ExecuteAlways]
    class GameViewReticleController : MonoBehaviour
    {
        class CoordinatesTransform : BaseFocusReticleControllerImplementation.ICoordinatesTransform
        {
            public Vector2 ScreenSize;

            public bool IsValid => ScreenSize.x * ScreenSize.y != 0;

            public Vector2 NormalizeScreenPoint(Vector2 screenPoint)
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException(
                        $"{typeof(CoordinatesTransform).FullName}, invalid {nameof(ScreenSize)}: {ScreenSize}.");
                }

                return screenPoint / ScreenSize;
            }

            public Vector2 NormalizedToScreen(Vector2 normalizedPoint)
            {
                return normalizedPoint * ScreenSize;
            }
        }

        const string k_FocusReticlePrefabPath = "Packages/com.unity.live-capture/Runtime/VirtualCamera/FocusReticle/FocusReticle.prefab";

        public event Action<Vector2> OnReticlePositionChanged = delegate { };

        readonly CoordinatesTransform m_CoordinatesTransform = new CoordinatesTransform();
        readonly FocusReticleControllerImplementation m_FocusReticleControllerImplementation = new FocusReticleControllerImplementation();

        void Awake()
        {
            // Build UI.
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.enabled = false;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_FocusReticlePrefabPath);
            Assert.IsNotNull(prefab, $"Could not load Focus Reticle prefab at [{k_FocusReticlePrefabPath}]");

            var reticleGO = Instantiate(prefab, gameObject.transform);

            // Since we use constant physical size with default settings we apply scaling to the reticle.
            reticleGO.transform.localScale = Vector3.one * .5f;
            var reticle = reticleGO.GetComponent<FocusReticle>();
            Assert.IsNotNull(reticle, $"Could not fetch {nameof(FocusReticle)} component from prefab.");

            m_FocusReticleControllerImplementation.Canvas = canvas;
            m_FocusReticleControllerImplementation.FocusReticle = reticle;
            m_FocusReticleControllerImplementation.CoordinatesTransform = m_CoordinatesTransform;
        }

        void OnEnable()
        {
            m_FocusReticleControllerImplementation.Initialize();
        }

        void OnDisable()
        {
            m_FocusReticleControllerImplementation.Dispose();
        }

        void Update()
        {
            // We need to be careful when updating the screen size,
            // values returned by the static API will be correct during MonoBehaviour Update,
            // but not necessarily during an Editor update for example.
            m_CoordinatesTransform.ScreenSize = GameViewController.GetScreenSize();
        }

        public void UpdateReticle(Vector2 position, FocusMode focusMode, bool visible)
        {
            m_FocusReticleControllerImplementation.UpdateView(position, focusMode, visible);
        }

        void OnGUI()
        {
            ProcessEvent(Event.current.type, Event.current.mousePosition);
        }

        // Introduced for testing purposes.
        internal void ProcessEvent(EventType type, Vector2 mousePosition)
        {
            // A valid screen size must have been assigned to the coordinates transform.
            if (!m_CoordinatesTransform.IsValid)
            {
                return;
            }

            var updatePointerPosition = false;

            switch (type)
            {
                case EventType.MouseDrag:
                    m_FocusReticleControllerImplementation.IsDragging = true;
                    m_FocusReticleControllerImplementation.PendingDrag = true;
                    updatePointerPosition = true;
                    break;
                case EventType.MouseDown:
                    m_FocusReticleControllerImplementation.PendingTap = true;
                    updatePointerPosition = true;
                    break;
                case EventType.MouseUp:
                case EventType.MouseLeaveWindow:
                    m_FocusReticleControllerImplementation.IsDragging = false;
                    break;
            }

            if (updatePointerPosition)
            {
                var lastPointerPosition = mousePosition;

                // IMGUI coords are start at top, flip Y axis.
                lastPointerPosition.y = m_CoordinatesTransform.ScreenSize.y - lastPointerPosition.y;

                m_FocusReticleControllerImplementation.LastPointerPosition = lastPointerPosition;

                if (m_FocusReticleControllerImplementation.ShouldSendPosition(out var position))
                {
                    OnReticlePositionChanged.Invoke(position);
                }
            }
        }
    }

    /// <summary>
    /// Singleton responsible for managing the GameView reticle.
    /// Manages the injected scene UI and communication with the VirtualCameraDevice.
    /// </summary>
    class GameViewController : ScriptableSingleton<GameViewController>
    {
        const string k_GameObjectName = "Live Capture In Game Components";
        static readonly ILiveCaptureBridge k_DefaultBridge = new LiveCaptureBridge();

        [SerializeReference]
        ILiveCaptureBridge m_CurrentBridge;

        GameViewReticleController m_ReticleController;
        bool m_IsActive;

        public bool IsActive => m_IsActive;

        [InitializeOnLoadMethod]
        static void RegisterCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                // Disable on playmode changes to prevent the interruption of an animation
                // leaving the reticle on the screen (OnDisable is not called due to HideFlags.HideAndDontSave being used)
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                {
                    instance.Disable();
                }
                break;
            }
        }

        static void OnBeforeAssemblyReload()
        {
            instance.Disable();
        }

        static void OnEditorUpdate()
        {
            instance.Update();
        }

        void Update()
        {
            if (m_IsActive && m_CurrentBridge.TryGetVirtualCameraDevice(out var device))
            {
                m_ReticleController.UpdateReticle(device.ReticlePosition, device.FocusMode, device.IsLive);
            }
        }

        public void Enable()
        {
            Enable(k_DefaultBridge);
        }

        internal void Enable(ILiveCaptureBridge bridge)
        {
            if (m_IsActive)
            {
                return;
            }

            m_IsActive = true;
            m_CurrentBridge = bridge;

            InjectSceneUI();
        }

        public void Disable()
        {
            if (!m_IsActive)
            {
                return;
            }

            DisposeSceneUI();

            m_CurrentBridge = null;
            m_IsActive = false;
        }

        void InjectSceneUI()
        {
            Assert.IsNull(m_ReticleController);
            var go = new GameObject(k_GameObjectName, typeof(GameViewReticleController));
            SetHideFlagsRecursively(go, HideFlags.HideAndDontSave);
            m_ReticleController = go.GetComponent<GameViewReticleController>();
            m_ReticleController.OnReticlePositionChanged += ReticlePositionChanged;
        }

        void DisposeSceneUI()
        {
            Assert.IsNotNull(m_ReticleController);

            m_ReticleController.OnReticlePositionChanged -= ReticlePositionChanged;

            if (Application.isPlaying)
            {
                Destroy(m_ReticleController.gameObject);
            }
            else
            {
                DestroyImmediate(m_ReticleController.gameObject);
            }

            m_ReticleController = null;
        }

        void ReticlePositionChanged(Vector2 normalizedPosition)
        {
            Assert.IsTrue(m_IsActive);

            if (m_CurrentBridge.TryGetVirtualCameraDevice(out var device))
            {
                device.ReticlePosition = normalizedPosition;
            }
        }

        static void SetHideFlagsRecursively(GameObject gameObject, HideFlags hideFlags)
        {
            gameObject.hideFlags = hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                SetHideFlagsRecursively(child.gameObject, hideFlags);
            }
        }

        // Using Func to make our coordinate conversion code testable.
        internal static Func<Vector2> GetScreenSize = () => new Vector2(Screen.width, Screen.height);
    }
}
