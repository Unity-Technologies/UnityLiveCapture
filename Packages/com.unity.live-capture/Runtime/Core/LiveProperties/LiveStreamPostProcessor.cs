using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// The base class for implementing a post-processor of a <see cref="LiveStream"/>.
    /// </summary>
    /// <seealso cref="LiveStream"/>
    [ExecuteAlways]
    [RequireComponent(typeof(LiveStreamCaptureDevice))]
    public abstract class LiveStreamPostProcessor : MonoBehaviour
    {
        LiveStreamCaptureDevice m_Device;

        /// <summary>
        /// The device containing the <see cref="LiveStream"/> to post-process.
        /// </summary>
        public LiveStreamCaptureDevice Device => m_Device;

        /// <summary>
        /// Unity calls this method when the component is about to get enabled.
        /// </summary>
        /// <remarks>
        /// If you override this method, call the base method in your implementation.
        /// </remarks>
        protected virtual void OnEnable()
        {
            m_Device = GetComponent<LiveStreamCaptureDevice>();
            m_Device.AddPostProcessor(this);
        }

        /// <summary>
        /// Unity calls this method when the component is about to get disabled.	
        /// </summary>	
        /// <remarks>	
        /// If you override this method, call the base method in your implementation.	
        /// </remarks>
        protected virtual void OnDisable()
        {
            m_Device.RemovePostProcessor(this);
        }

        /// <summary>
        /// Override this method to create new properties to the specified <see cref="LiveStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="LiveStream"/> to create properties to.</param>
        protected virtual void CreateLiveProperties(LiveStream stream) { }

        /// <summary>
        /// Override this method to remove existing properties from the specified <see cref="LiveStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="LiveStream"/> to remove properties from.</param>
        protected virtual void RemoveLiveProperties(LiveStream stream) { }

        /// <summary>
        /// Override this method to post-process the specified <see cref="LiveStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="LiveStream"/> to modify.</param>
        protected abstract void PostProcessFrame(LiveStream stream);

        internal void InvokeCreateLiveProperties(LiveStream stream) => CreateLiveProperties(stream);
        internal void InvokeRemoveLiveProperties(LiveStream stream) => RemoveLiveProperties(stream);
        internal void InvokePostProcessFrame(LiveStream stream) => PostProcessFrame(stream);
    }
}
