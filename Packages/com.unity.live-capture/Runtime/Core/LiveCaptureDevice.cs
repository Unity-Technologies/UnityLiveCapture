using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// The base class for implementing a capture device. It provides a mechanism to record live
    /// data from a source and preview it in the scene.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public abstract class LiveCaptureDevice : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        int m_SortingOrder;

        [SerializeField, HideInInspector]
        bool m_IsLive = true;

        internal int SortingOrder
        {
            get => m_SortingOrder;
            set
            {
                m_SortingOrder = value;
                LiveCaptureDeviceManager.Instance.SetNeedsSorting();
            }
        }

        internal bool IsLive
        {
            get => m_IsLive;
            set => m_IsLive = value;
        }

        /// <summary>
        /// The recording state of the device.
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// The device calls this method when the device is about to get enabled.
        /// </summary>
        /// <remarks>
        /// If you override this method, call the base method in your implementation.
        /// </remarks>
        protected virtual void OnEnable()
        {
            LiveCaptureDeviceManager.Instance.Register(this);
        }

        /// <summary>
        /// The device calls this method when the device is about to get disabled.	
        /// </summary>	
        /// <remarks>	
        /// If you override this method, call the base method in your implementation.	
        /// </remarks>
        protected virtual void OnDisable()
        {
            InvokeStopRecording();

            LiveCaptureDeviceManager.Instance.Unregister(this);
        }

        /// <summary>
        /// Updates the internal state of the device.
        /// </summary>
        protected virtual void UpdateDevice() { }

        /// <summary>
        /// Override this method to update the device during live mode.
        /// </summary>
        /// <remarks>
        /// This method is called after the animation system execution and before the script's LateUpdate.
        /// </remarks>
        protected virtual void LiveUpdate() { }

        /// <summary>
        /// Indicates whether a device is ready for recording.
        /// </summary>
        /// <returns>
        /// true if ready for recording; otherwise, false.
        /// </returns>
        public abstract bool IsReady();

        /// <summary>
        /// The device calls this method when a new recording started.
        /// </summary>
        protected virtual void OnStartRecording() { }

        /// <summary>
        /// The device calls this method when the ongoing recording stopped.
        /// </summary>
        protected virtual void OnStopRecording() { }

        /// <summary>
        /// Stores the recording into a take using a <see cref="ITakeBuilder"/>.
        /// </summary>
        /// <param name="takeBuilder">The take builder object.</param>
        public abstract void Write(ITakeBuilder takeBuilder);

        /// <summary>
        /// Updates the scene during edit mode.
        /// </summary>
        /// <remarks>
        /// Call this method every time your device changes the scene. This method is
        /// only effective during edit mode.
        /// </remarks>
        protected void Refresh()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }

        internal void InvokeUpdateDevice()
        {
            try
            {
                UpdateDevice();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        internal void InvokeLiveUpdate()
        {
            try
            {
                LiveUpdate();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        internal void InvokeStartRecording()
        {
            if (IsRecording || !isActiveAndEnabled || !IsLive || !IsReady())
            {
                return;
            }

            IsRecording = true;

            try
            {
                OnStartRecording();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        internal void InvokeStopRecording()
        {
            if (!IsRecording)
            {
                return;
            }

            IsRecording = false;

            try
            {
                OnStopRecording();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
