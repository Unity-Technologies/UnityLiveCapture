using System;
using Unity.Collections;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    /// <summary>
    /// Indicates if a specific encoder is supported on the current platform and device or not, and why (when applicable).
    /// </summary>
    enum EncoderSupport
    {
        /// <summary>
        /// The encoder is supported on the current platform and device.
        /// </summary>
        Supported = 0,

        /// <summary>
        /// The encoder is not supported on the current platform.
        /// </summary>
        NotSupportedOnPlatform,

        /// <summary>
        /// The encoder is not supported on the current device, there is no driver installed.
        /// </summary>
        NoDriver,

        /// <summary>
        /// The encoder is not supported on the current device, the driver is deprecated or not up to date.
        /// </summary>
        DriverVersionNotSupported,
    }

    /// <summary>
    /// The supported encoder formats.
    /// </summary>
    enum EncoderFormat
    {
        /// <summary>
        /// A biplanar format with a full sized Y plane followed by a single chroma plane with weaved U and V values.
        /// </summary>
        NV12,

        /// <summary>
        /// An 8 bit monochrome format.
        /// </summary>
        R8G8B8,
    }

    /// <summary>
    /// Indicates the status of the current encoder instance.
    /// </summary>
    enum EncoderStatus
    {
        /// <summary>
        /// The encoder <see cref="IEncoder.Setup"/> function has not been called.
        /// </summary>
        NotInitialized = 0,

        /// <summary>
        /// The encoder enqueued a <see cref="IEncoder.Setup"/> command, which is progressing.
        /// </summary>
        InProgress,

        /// <summary>
        /// The encoder has been successfully initialized.
        /// </summary>
        Initialized,

        /// <summary>
        /// The encoder failed during the <see cref="IEncoder.Setup"/> call.
        /// </summary>
        Failed,
    }

    /// <summary>
    /// The interface that defines the base encoder functionality.
    /// </summary>
    interface IEncoder : IDisposable
    {
        /// <summary>
        /// Indicates if the encoder has been initialized or is valid.
        /// </summary>
        EncoderStatus initialized { get; }

        /// <summary>
        /// The texture format the encoder uses for input.
        /// </summary>
        EncoderFormat encoderFormat { get; }

        /// <summary>
        /// Configures a new native encoder instance.
        /// </summary>
        /// <param name="settings">The configuration of the encoder.</param>
        /// <param name="encoderFormat">The texture format the encoder uses for input.</param>
        void Setup(EncoderSettings settings, EncoderFormat encoderFormat);

        /// <summary>
        /// Updates the native encoder instance settings.
        /// </summary>
        /// <param name="settings">The configuration of the encoder.</param>
        void UpdateSettings(in EncoderSettings settings);
    }

    /// <summary>
    /// The interface that defines the base software encoder functionality.
    /// </summary>
    interface ISoftwareEncoder : IEncoder
    {
        /// <summary>
        /// Encodes an image into the video stream.
        /// </summary>
        /// <param name="imageData">The image data in bytes. It includes a width and a height that match the ones you configured through <see cref="IEncoder.Setup"/>.</param>
        /// <param name="timeStamp">The time in nanoseconds the image was sampled at since the start of the video stream.</param>
        /// <param name="frame">The encoded image frame.</param>
        void Encode(in NativeArray<byte> imageData, ulong timeStamp, H264EncodedFrame frame);
    }

    /// <summary>
    /// The interface that defines the base hardware encoder functionality.
    /// </summary>
    interface IHardwareEncoder : IEncoder
    {
        /// <summary>
        /// Queues a command on the render thread to encode a texture.
        /// </summary>
        /// <param name="renderTexture">The texture to encode.</param>
        /// <param name="timestamp">The time in nanoseconds the image was sampled at since the start of the video stream.</param>
        void Encode(RenderTexture renderTexture, ulong timestamp);

        /// <summary>
        /// Retrieves the data of the first encoded frame found in the plugin.
        /// </summary>
        /// <param name="frame">The returned frame containing the encoded data.</param>
        /// <param name="timestamp">The time in nanoseconds the image was sampled at since the start of the video stream.</param>
        /// <returns>True if an encoded frame has been found; false otherwise.</returns>
        bool ConsumeData(H264EncodedFrame frame, out ulong timestamp);
    }
}
