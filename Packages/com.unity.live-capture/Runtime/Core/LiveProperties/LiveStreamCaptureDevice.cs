using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.LiveCapture.LiveProperties;

namespace Unity.LiveCapture
{
    /// <summary>
    /// The base class for implementing a capture device that manages a <see cref="LiveStream"/>.
    /// </summary>
    /// <seealso cref="LiveStream"/>
    public abstract class LiveStreamCaptureDevice : LiveCaptureDevice
    {
        LiveStream m_Stream = new LiveStream();
        List<LiveStreamPostProcessor> m_PostProcessors = new List<LiveStreamPostProcessor>();
        PropertyPreviewer m_Previewer;
        double? m_FirstFrameTime;
        FrameTimeWithRate? m_CurrentFrameTime;
        Transform m_CurrentRoot;
        internal LiveStream Stream => m_Stream;

        /// <summary>
        /// The time of the first recorded frame.
        /// </summary>
        protected double? FirstFrameTime => m_FirstFrameTime;

        /// <summary>
        /// The current frame time and frame rate of the <see cref="LiveStream"/>.
        /// </summary>
        public FrameTimeWithRate? CurrentFrameTime => m_CurrentFrameTime;

        /// <summary>
        /// Registers the specified <see cref="LiveStreamPostProcessor"/>.
        /// </summary>
        /// <param name="postProcessor">The post processor to register.</param>
        internal void AddPostProcessor(LiveStreamPostProcessor postProcessor)
        {
            if (m_PostProcessors.AddUnique(postProcessor))
            {
                postProcessor.InvokeCreateLiveProperties(m_Stream);
            }
        }

        /// <summary>
        /// Deregisters the specified <see cref="LiveStreamPostProcessor"/>.
        /// </summary>
        /// <param name="postProcessor">The post processor to deregister.</param>
        internal void RemovePostProcessor(LiveStreamPostProcessor postProcessor)
        {
            if (m_PostProcessors.Remove(postProcessor))
            {
                postProcessor.InvokeRemoveLiveProperties(m_Stream);
            }
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            try
            {
                CreateLiveProperties(m_Stream);

                foreach (var postProcessor in m_PostProcessors)
                {
                    postProcessor.InvokeCreateLiveProperties(m_Stream);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            RestoreLiveProperties();

            m_Stream.Reset();
            m_CurrentFrameTime = null;
        }

        /// <summary>
        /// Generates an animation clip from the last recording.
        /// </summary>
        /// <returns>An animation clip containing the last recording.</returns>
        protected AnimationClip BakeAnimationClip()
        {
            return m_Stream.Bake();
        }

        /// <inheritdoc/>
        protected override void OnStartRecording()
        {
            m_Stream.ClearCurves();
            m_FirstFrameTime = null;

            if (m_CurrentFrameTime.HasValue)
            {
                m_Stream.SetFrameRate(m_CurrentFrameTime.Value.Rate);

                Record(m_CurrentFrameTime.Value.ToSeconds());
            }
        }

        /// <inheritdoc/>
        protected override void LiveUpdate()
        {
            RebindIfNeeded();

            m_Stream.ApplyValues();
        }

        /// <summary>
        /// Updates the internal <see cref="LiveStream"/>.
        /// </summary>
        /// <param name="root">The root transform to bind.</param>
        /// <param name="frameTime">The frame time to use for recording.</param>
        protected void UpdateStream(Transform root, FrameTimeWithRate frameTime)
        {
            m_CurrentRoot = root;
            m_CurrentFrameTime = frameTime;

            try
            {
                ProcessFrame(m_Stream);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            foreach (var postProcessor in m_PostProcessors)
            {
                try
                {
                    postProcessor?.InvokePostProcessFrame(m_Stream);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (IsRecording)
            {
                Record(frameTime.ToSeconds());
            }
        }

        /// <summary>
        /// Override this method to create new properties to the specified <see cref="LiveStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="LiveStream"/> to modify.</param>
        protected virtual void CreateLiveProperties(LiveStream stream) { }

        /// <summary>
        /// Override this method to process the specified <see cref="LiveStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="LiveStream"/> to modify.</param>
        protected abstract void ProcessFrame(LiveStream stream);

        void RebindIfNeeded()
        {
            if (m_Stream.Root != m_CurrentRoot)
            {
                RestoreLiveProperties();

                m_Stream.Rebind(m_CurrentRoot);
            }
        }

        void Record(double frameTime)
        {
            m_FirstFrameTime ??= frameTime;
            m_Stream.Record(frameTime - m_FirstFrameTime.Value);
        }

        /// <summary>
        /// Registers the values of the live properties to prevent Unity from marking Prefabs or the Scene
        /// as modified when you preview animations.
        /// </summary>
        protected void RegisterLiveProperties()
        {
            RestoreLiveProperties();

            if (m_Previewer == null)
            {
                m_Previewer = new PropertyPreviewer(this);
            }

            foreach (var property in m_Stream.Properties)
            {
                RegisterLiveProperty(property);
            }

            if (m_Stream.Root != null)
            {
                foreach (var previewable in m_Stream.Root.GetComponents<IPreviewable>())
                {
                    previewable.Register(m_Previewer);
                }
            }
        }

        /// <summary>
        /// Restores the original values of any created live property.
        /// </summary>
        protected void RestoreLiveProperties()
        {
            if (m_Previewer != null)
            {
                m_Previewer.Restore();
            }
        }

        void RegisterLiveProperty(ILiveProperty property)
        {
            var target = property.Target;
            var propertyName = property.Binding.PropertyName;

            if (target == null || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            if (target is Transform && (propertyName.StartsWith("m_LocalEuler") || propertyName == "m_LocalRotation"))
            {
                m_Previewer.Register(target, "m_LocalRotation");
                m_Previewer.Register(target, "m_LocalEulerAnglesHint");
            }
            else
            {
                m_Previewer.Register(target, propertyName);
            }
        }
    }
}
