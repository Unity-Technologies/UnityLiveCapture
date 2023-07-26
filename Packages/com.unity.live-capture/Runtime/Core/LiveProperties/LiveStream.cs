using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Unity.LiveCapture.LiveProperties;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A container that manages a collection of properties in order to animate an actor in the
    /// scene using live data and record its performance into a clip.
    /// </summary>
    public class LiveStream
    {
        FrameRate m_FrameRate;
        Dictionary<PropertyBinding, LivePropertyHandle> m_Handles = new Dictionary<PropertyBinding, LivePropertyHandle>();
        Dictionary<LivePropertyHandle, ILiveProperty> m_Properties = new Dictionary<LivePropertyHandle, ILiveProperty>();

        internal Dictionary<LivePropertyHandle, ILiveProperty>.ValueCollection Properties => m_Properties.Values;

        /// <summary>
        /// The frame-rate to use for recording.
        /// </summary>
        public FrameRate FrameRate => m_FrameRate;

        /// <summary>
        /// The root transform to bind the animated properties to.
        /// </summary>
        public Transform Root { get; private set; }

        /// <summary>
        /// Creates a property from the specified binding information.
        /// </summary>
        /// <param name="relativePath">The path from the root transform where the component to bind to is.</param>
        /// <param name="propertyName">The name of the property to bind to.</param>
        /// <param name="setter">The action that sets the value to the component's property. This is needed for some
        /// builtin components where the property can't be set using reflection.</param>
        /// <typeparam name="TComponent">The type of the component to bind to.</typeparam>
        /// <typeparam name="TValue">The type of the data to bind to.</typeparam>
        /// <returns>A <see cref="LivePropertyHandle"/> representing the property.</returns>
        public LivePropertyHandle CreateProperty<TComponent, TValue>(string relativePath, string propertyName, Action<TComponent, TValue> setter = null)
            where TComponent : Component
            where TValue : struct
        {
            var binding = new PropertyBinding(relativePath, propertyName, typeof(TComponent));

            if (!m_Handles.TryGetValue(binding, out var handle))
            {
                handle = LivePropertyHandle.Create();
                m_Handles[binding] = handle;
                m_Properties[handle] = new LiveProperty<TComponent, TValue>(binding, setter);
            }

            return handle;
        }

        /// <summary>
        /// Removes a property from the stream.
        /// </summary>
        /// <param name="handle">The <see cref="LivePropertyHandle"/> representing the property.</param>
        public void RemoveProperty(LivePropertyHandle handle)
        {
            if (TryGetProperty(handle, out var property))
            {
                m_Handles.Remove(property.Binding);
                m_Properties.Remove(handle);
            }
        }

        /// <summary>
        /// Sets the frame-rate to use for recording.
        /// </summary>
        /// <param name="frameRate">The frame-rate to use for recording.</param>
        public void SetFrameRate(FrameRate frameRate)
        {
            m_FrameRate = frameRate;

            foreach (var property in Properties)
            {
                property.Curve.FrameRate = frameRate;
            }
        }

        /// <summary>
        /// Attempts to find all the target components from the root. Call this method if
        /// the hierarcy of the root changes.
        /// </summary>
        public void Rebind()
        {
            Rebind(Root);
        }

        /// <summary>
        /// Attempts to find all the target components from the specified root. Call this method
        /// to change the animated hierarchy.
        /// </summary>
        /// <param name="root">The root of the hierarchy to animate.</param>
        public void Rebind(Transform root)
        {
            Root = root;

            foreach (var property in Properties)
            {
                property.Rebind(Root);
            }
        }

        /// <summary>
        /// Checks if the specified property is managed by this stream.
        /// </summary>
        /// <param name="handle">The <see cref="LivePropertyHandle"/> representing the property.</param>
        /// <returns><see langword="true"/> if the handle is valid; otherwise, <see langword="false"/>.</returns>
        public bool IsHandleValid(LivePropertyHandle handle)
        {
            return m_Properties.ContainsKey(handle);
        }

        /// <summary>
        /// Attemps to retrieve an already created property from the stream.
        /// </summary>
        /// <param name="binding">The <see cref="PropertyBinding"/> of the property.</param>
        /// <param name="handle">The returned property handle instance, or <see langword="default"/> if the property handle was not found.</param>
        /// <returns><see langword="true"/> if the property handle was found, otherwise, <see langword="false"/>.</returns>
        public bool TryGetHandle(PropertyBinding binding, out LivePropertyHandle handle)
        {
            return m_Handles.TryGetValue(binding, out handle);
        }

        internal bool TryGetProperty(LivePropertyHandle handle, out ILiveProperty property)
        {
            return m_Properties.TryGetValue(handle, out property);
        }

        internal void Reset()
        {
            Root = null;
            m_Handles.Clear();
            m_Properties.Clear();
        }
    }

    /// <summary>
    /// Extension methods for <see cref="LiveStream"/>.
    /// </summary>
    public static class LiveStreamExtensions
    {
        /// <summary>
        /// Creates a property from the specified binding information at the root transform.
        /// </summary>
        /// <param name="stream">The stream to create the property to.</param>
        /// <param name="propertyName">The name of the property to bind to.</param>
        /// <param name="setter">The action that sets the value to the component's property. This is needed for some
        /// builtin components where the property can't be set using reflection.</param>
        /// <typeparam name="TComponent">The type of the component to bind to.</typeparam>
        /// <typeparam name="TValue">The type of the data to bind to.</typeparam>
        /// <returns>A <see cref="LivePropertyHandle"/> representing the property.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static LivePropertyHandle CreateProperty<TComponent, TValue>([NotNull] this LiveStream stream, string propertyName, Action<TComponent, TValue> setter = null)
            where TComponent : Component
            where TValue : struct
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return stream.CreateProperty(string.Empty, propertyName, setter);
        }

        /// <summary>
        /// Attemps to retrieve an already created property from the stream at the root transform.
        /// </summary>
        /// <param name="stream">The stream to create the property to.</param>
        /// <param name="propertyName">The name of the property to bind to.</param>
        /// <param name="type">The type of the component to bind to.</param>
        /// <param name="handle">The returned property handle instance, or <see langword="default"/> if the property handle was not found.</param>
        /// <returns><see langword="true"/> if the property handle was found, otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static bool TryGetHandle([NotNull] this LiveStream stream, string propertyName, Type type, out LivePropertyHandle handle)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return stream.TryGetHandle(string.Empty, propertyName, type, out handle);
        }

        /// <summary>
        /// Attemps to retrieve an already created property from the stream.
        /// </summary>
        /// <param name="stream">The stream to create the property to.</param>
        /// <param name="relativePath">The path from the root transform where the component to bind to is.</param>
        /// <param name="propertyName">The name of the property to bind to.</param>
        /// <param name="type">The type of the component to bind to.</param>
        /// <param name="handle">The returned property handle instance, or <see langword="default"/> if the property handle was not found.</param>
        /// <returns><see langword="true"/> if the property handle was found, otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static bool TryGetHandle([NotNull] this LiveStream stream, string relativePath, string propertyName, Type type, out LivePropertyHandle handle)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var binding = new PropertyBinding(relativePath, propertyName, type);

            return stream.TryGetHandle(binding, out handle);
        }

        /// <summary>
        /// Sets the live state of the property associated with the specified <see cref="LivePropertyHandle"/>.
        /// </summary>
        /// <remarks>
        /// A property with its live state set to <see langword="true"/> will animate its target component and
        /// participate in an ongoing recording. A live state set to <see langword="false"/>, won't.
        /// </remarks>
        /// <param name="stream">The stream containing the property.</param>
        /// <param name="handle">The <see cref="LivePropertyHandle"/> representing the property.</param>
        /// <param name="value">The live state to set to the property.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SetLive([NotNull] this LiveStream stream, LivePropertyHandle handle, bool value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.TryGetProperty(handle, out var property))
            {
                property.IsLive = value;
            }
        }

        /// <summary>
        /// Attempts to get the value of the property associated with the specified <see cref="LivePropertyHandle"/>.
        /// </summary>
        /// <param name="stream">The stream containing the property.</param>
        /// <param name="handle">The <see cref="LivePropertyHandle"/> representing the property.</param>
        /// <param name="value">The value stored in the property.</param>
        /// <typeparam name="TValue">The type of the data to retrieve.</typeparam>
        /// <returns><see langword="true"/> if the value was found, otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static bool TryGetValue<TValue>([NotNull] this LiveStream stream, LivePropertyHandle handle, out TValue value) where TValue : struct
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            value = default;

            if (stream.TryGetProperty<TValue>(handle, out var property))
            {
                return property.TryGetValue(out value);
            }

            return false;
        }

        /// <summary>
        /// Sets the value of the property associated with the specified <see cref="LivePropertyHandle"/>.
        /// </summary>
        /// <param name="stream">The stream containing the property.</param>
        /// <param name="handle">The <see cref="LivePropertyHandle"/> representing the property.</param>
        /// <param name="value">The value to set to the property.</param>
        /// <typeparam name="TValue">The type of the data to retrieve.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SetValue<TValue>([NotNull] this LiveStream stream, LivePropertyHandle handle, in TValue value) where TValue : struct
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.TryGetProperty<TValue>(handle, out var property))
            {
                property.SetValue(value);
            }
        }

        /// <summary>
        /// Sets the tolerance to use when reducing keyframes during a recording of the property associated
        /// with the specified <see cref="LivePropertyHandle"/>.
        /// </summary>
        /// <param name="stream">The stream containing the property.</param>
        /// <param name="handle">The <see cref="LivePropertyHandle"/> representing the property.</param>
        /// <param name="value">The tolerance to set expressed as an error mesurement.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SetMaxError([NotNull] this LiveStream stream, LivePropertyHandle handle, float value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.TryGetProperty(handle, out var property) && property.Curve is IReduceableCurve curve)
            {
                curve.MaxError = value;
            }
        }
    }

    static class LiveStreamExtensionsInternal
    {
        public static bool TryGetProperty<TComponent, TValue>([NotNull] this LiveStream stream, LivePropertyHandle handle, out LiveProperty<TComponent, TValue> property)
            where TComponent : Component
            where TValue : struct
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.TryGetProperty(handle, out var p);

            property = p as LiveProperty<TComponent, TValue>;

            return property != null;
        }

        public static bool TryGetProperty<TValue>([NotNull] this LiveStream stream, LivePropertyHandle handle, out ILiveProperty<TValue> property)
            where TValue : struct
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.TryGetProperty(handle, out var p);

            property = p as ILiveProperty<TValue>;

            return property != null;
        }

        public static void ApplyValues([NotNull] this LiveStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            foreach (var property in stream.Properties)
            {
                property.ApplyValue();
            }
        }

        public static void Record([NotNull] this LiveStream stream, double time)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            foreach (var property in stream.Properties)
            {
                property.Record(time);
            }
        }

        public static void ClearCurves([NotNull] this LiveStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            foreach (var property in stream.Properties)
            {
                property.Curve.Clear();
            }
        }

        public static AnimationClip Bake([NotNull] this LiveStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var animationClip = new AnimationClip()
            {
                frameRate = stream.FrameRate.AsFloat()
            };

            foreach (var property in stream.Properties)
            {
                property.Curve.SetToAnimationClip(property.Binding, animationClip);
            }

            return animationClip;
        }
    }
}
