using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.Mocap
{
    /// <summary>
    /// Flags to enable or disable transform channels.
    /// </summary>
    enum TransformChannels
    {
        /// <summary>
        /// Uses no transform channels.
        /// </summary>
        None = 0,
        /// <summary>
        /// Uses the local position of the transform.
        /// </summary>
        Position = 1 << 0,
        /// <summary>
        /// Uses the local rotation of the transform.
        /// </summary>
        Rotation = 1 << 1,
        /// <summary>
        /// Uses the local scale of the transform.
        /// </summary>
        Scale = 1 << 2,
        /// <summary>
        /// Uses all possible transform channels.
        /// </summary>
        All = ~0
    }

    /// <summary>
    /// A class that manages the recording and preview values of a set of transforms.
    /// </summary>
    [Serializable]
    class MocapRecorder : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<Transform> m_Transforms = new List<Transform>();
        [SerializeField]
        SerializableDictionary<Transform, TransformChannels> m_Channels = new SerializableDictionary<Transform, TransformChannels>();
        Dictionary<Transform, (Vector3?, Quaternion?, Vector3?)> m_Frames = new Dictionary<Transform, (Vector3?, Quaternion?, Vector3?)>();
        Dictionary<Transform, TransformCurve> m_Curves = new Dictionary<Transform, TransformCurve>();
        FrameRate m_FrameRate = StandardFrameRate.FPS_24_00;
        PropertyPreviewer m_Previewer;
        HashSet<Transform> m_TransformSet = new HashSet<Transform>();
        Animator m_Animator;

        /// <summary>
        /// The root Animator component.
        /// </summary>
        public Animator Animator
        {
            get => m_Animator;
            set
            {
                if (m_Animator != value)
                {
                    m_Animator = value;
                    Reset();
                }
            }
        }

        /// <summary>
        /// The frame rate to use for recording.
        /// </summary>
        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set
            {
                m_FrameRate = value;

                foreach (var transform in m_Transforms)
                {
                    if (transform == null)
                        continue;

                    if (m_Curves.TryGetValue(transform, out var curve))
                    {
                        curve.FrameRate = m_FrameRate;
                    }
                }
            }
        }

        internal IEnumerable<Transform> GetTransforms()
        {
            return m_Transforms;
        }

        internal TransformChannels GetChannels(Transform transform)
        {
            Debug.Assert(transform != null);

            if (!m_Channels.TryGetValue(transform, out var channels))
            {
                channels = TransformChannels.All;
                m_Channels[transform] = channels;
            }

            return channels;
        }

        internal void SetChannels(Transform transform, TransformChannels channels)
        {
            Debug.Assert(transform != null);

            m_Channels[transform] = channels;
        }

        /// <summary>
        /// Checks if the recording contains no recorded samples.
        /// </summary>
        /// <returns>true if the recording contains no samples; otherwise, false.</returns>
        public bool IsEmpty()
        {
            foreach (var transform in m_Transforms)
            {
                if (transform == null)
                    continue;

                if (m_Curves.TryGetValue(transform, out var curve)
                    && !curve.IsEmpty())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clears the recorded values.
        /// </summary>
        public void Clear()
        {
            foreach (var transform in m_Transforms)
            {
                if (transform == null)
                    continue;

                if (m_Curves.TryGetValue(transform, out var curve))
                {
                    curve.Clear();
                }
            }
        }

        /// <summary>
        /// Registers the candidate position, rotation and scale to apply to a specified transform.
        /// </summary>
        /// <param name="position">The position to apply.</param>
        /// <param name="rotation">The rotation to apply.</param>
        /// <param name="scale">The scale to apply.</param>
        public void Present(Transform transform, Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            if (Animator == null || transform == null)
            {
                return;
            }

            if (!m_TransformSet.Contains(transform))
            {
                m_TransformSet.Add(transform);
                m_Transforms.Add(transform);

                RegisterLiveProperties(transform);
            }

            if (!m_Frames.TryGetValue(transform, out (Vector3? pos, Quaternion? rot, Vector3? s) value))
            {
                value = (null, null, null);
            }

            var channels = GetChannels(transform);

            // These checks are required to avoid overwriting values set by a previous source
            if (position.HasValue && channels.HasFlag(TransformChannels.Position))
                value.pos = position;

            if (rotation.HasValue && channels.HasFlag(TransformChannels.Rotation))
                value.rot = rotation;

            if (scale.HasValue && channels.HasFlag(TransformChannels.Scale))
                value.s = scale;

            m_Frames[transform] = value;
        }

        /// <summary>
        /// Records a single frame sample.
        /// </summary>
        /// <param name="time">The current time in seconds.</param>
        public void Record(double time)
        {
            foreach (var transform in m_Transforms)
            {
                if (transform == null)
                    continue;

                if (m_Frames.TryGetValue(transform, out (Vector3? pos, Quaternion? rot, Vector3? scale) value)
                    && TryGetCurve(transform, out var curve))
                {
                    curve.AddKey(time, value.pos, value.rot, value.scale);
                }
            }
        }

        /// <summary>
        /// Applies the keyframe data to the transforms.
        /// </summary>
        public void ApplyFrame()
        {
            foreach (var transform in m_Transforms)
            {
                if (transform == null)
                    continue;

                if (m_Frames.TryGetValue(transform, out (Vector3? pos, Quaternion? rot, Vector3? scale) value))
                {
                    if (value.pos.HasValue)
                        transform.localPosition = value.pos.Value;

                    if (value.rot.HasValue)
                        transform.localRotation = value.rot.Value;

                    if (value.scale.HasValue)
                        transform.localScale = value.scale.Value;

                    m_Frames[transform] = (null, null, null);
                }
            }
        }

        /// <summary>
        /// Produces an AnimationClip from the recording.
        /// </summary>
        /// <returns>An AnimationClip created from the recording.</returns>
        public AnimationClip Bake()
        {
            var clip = new AnimationClip();

            foreach (var transform in m_Transforms)
            {
                if (transform == null)
                    continue;

                if (m_Curves.TryGetValue(transform, out var curve))
                {
                    curve.SetToAnimationClip(clip);
                }
            }

            return clip;
        }

        bool TryGetCurve(Transform transform, out TransformCurve curve)
        {
            curve = null;

            if (!m_Curves.TryGetValue(transform, out curve)
                && m_Animator.TryGetAnimationPath(transform, out var path))
            {
                curve = new TransformCurve(path);
                curve.FrameRate = FrameRate;

                m_Curves[transform] = curve;
            }

            return curve != null;
        }

        void Reset()
        {
            m_TransformSet.Clear();
            m_Transforms.Clear();
            m_Channels.Clear();
            m_Frames.Clear();
            m_Curves.Clear();

            RestoreLiveProperties();
        }

        /// <summary>
        /// Registers the animated transforms to prevent Unity from marking Prefabs or the Scene
        /// as modified when you preview animations.
        /// </summary>
        /// <param name="driver">Key to identify the group of registered properties.</param>
        public void RegisterLiveProperties(UnityObject driver)
        {
            RestoreLiveProperties();

            if (m_Previewer != null && m_Previewer.Driver != driver)
            {
                m_Previewer = null;
            }

            if (m_Previewer == null)
            {
                m_Previewer = new PropertyPreviewer(driver);
            }

            foreach (var transform in m_Transforms)
            {
                RegisterLiveProperties(transform);
            }
        }

        /// <summary>
        /// Restores the properties previously registered.
        /// </summary>
        public void RestoreLiveProperties()
        {
            if (m_Previewer != null)
            {
                m_Previewer.Restore();
            }
        }

        void RegisterLiveProperties(Transform transform)
        {
            if (m_Previewer != null)
            {
                m_Previewer.Register(transform, "m_LocalPosition.x");
                m_Previewer.Register(transform, "m_LocalPosition.y");
                m_Previewer.Register(transform, "m_LocalPosition.z");
                m_Previewer.Register(transform, "m_LocalRotation.x");
                m_Previewer.Register(transform, "m_LocalRotation.y");
                m_Previewer.Register(transform, "m_LocalRotation.z");
                m_Previewer.Register(transform, "m_LocalRotation.w");
                m_Previewer.Register(transform, "m_LocalEulerAnglesHint.x");
                m_Previewer.Register(transform, "m_LocalEulerAnglesHint.y");
                m_Previewer.Register(transform, "m_LocalEulerAnglesHint.z");
                m_Previewer.Register(transform, "m_LocalScale.x");
                m_Previewer.Register(transform, "m_LocalScale.y");
                m_Previewer.Register(transform, "m_LocalScale.z");
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_TransformSet.Clear();

            foreach (var transform in m_Transforms)
            {
                if (transform != null)
                {
                    m_TransformSet.Add(transform);
                }
            }
        }
    }
}
