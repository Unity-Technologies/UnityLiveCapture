using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    interface IVideoFrameRequest
    {
        /// <summary>
        /// The time in seconds at which this image was requested.
        /// </summary>
        public float elapsedTime { get; }

        /// <summary>
        /// The width of the video frame.
        /// </summary>
        public int width { get; }

        /// <summary>
        /// The height of the video frame.
        /// </summary>
        public int height { get; }

        /// <summary>
        /// The pixel format of the video texture.
        /// </summary>
        public EncoderFormat format { get; }
    }

    /// <summary>
    /// A request for texture data from the GPU that can be completed either synchronously or
    /// asynchronously. Only valid for the frame on which the request is completed, so be sure
    /// to poll <see cref="isDone"/> appropriately and copy the results as needed.
    /// </summary>
    struct AsyncGPUVideoFrameRequest : IVideoFrameRequest
    {
        AsyncGPUReadbackRequest m_Request;

        /// <inheritdoc/>
        public float elapsedTime { get; }

        /// <inheritdoc/>
        public int width { get; }

        /// <inheritdoc/>
        public int height { get; }

        /// <inheritdoc/>
        public EncoderFormat format { get; }

        /// <summary>
        /// Determines if the request has been processed.
        /// </summary>
        public bool isDone => m_Request.done;

        /// <summary>
        /// Did an error occur while retrieving the texture data.
        /// </summary>
        public bool hasError => m_Request.hasError;

        /// <summary>
        /// Creates a new <see cref="AsyncGPUVideoFrameRequest"/> instance.
        /// </summary>
        /// <param name="request">A read back request.</param>
        /// <param name="width">The width of the video frame.</param>
        /// <param name="height">The height of the video frame.</param>
        /// <param name="elapsedTime">The time in seconds at which this image was requested.</param>
        /// <param name="format">The pixel format of the video texture.</param>
        public AsyncGPUVideoFrameRequest(AsyncGPUReadbackRequest request, int width, int height, float elapsedTime, EncoderFormat format)
        {
            m_Request = request;
            this.width = width;
            this.height = height;
            this.elapsedTime = elapsedTime;
            this.format = format;
        }

        /// <summary>
        /// Returns the requested texture data if the request was successfully completed.
        /// </summary>
        /// <exception cref="InvalidOperationException">The async operation was not completed or encountered an error.</exception>
        /// <returns>A native array with the texture data. Only valid for the remainder of the current frame. The collection
        /// does not need to be disposed by the caller.</returns>
        public NativeArray<byte> GetData()
        {
            if (!isDone)
                throw new InvalidOperationException("Texture data may not be accessed if the request is not done.");
            if (hasError)
                throw new InvalidOperationException("Texture data may not be accessed if the request completed with an error.");

            return m_Request.GetData<byte>();
        }
    }

    /// <summary>
    /// A video frame request that directly uses the texture in the GPU memory.
    /// </summary>
    struct DirectAccessVideoFrameRequest : IVideoFrameRequest
    {
        /// <inheritdoc/>
        public float elapsedTime { get; }

        /// <inheritdoc/>
        public int width { get; }

        /// <inheritdoc/>
        public int height { get; }

        /// <inheritdoc/>
        public EncoderFormat format { get; }

        /// <summary>
        /// The captured render texture of the video Frame.
        /// </summary>
        public RenderTexture renderTexture { get; }

        /// <summary>
        /// Creates a new <see cref="DirectAccessVideoFrameRequest"/> instance.
        /// </summary>
        /// <param name="width">The width of the video frame.</param>
        /// <param name="height">The height of the video frame.</param>
        /// <param name="elapsedTime">The time in seconds at which this image was requested.</param>
        /// <param name="renderTexture">The texture containing the video frame.</param>
        /// <param name="format">The pixel format of the video texture.</param>
        public DirectAccessVideoFrameRequest(int width, int height, float elapsedTime, RenderTexture renderTexture, EncoderFormat format)
        {
            this.renderTexture = renderTexture;
            this.width = width;
            this.height = height;
            this.elapsedTime = elapsedTime;
            this.format = format;
        }
    }
}
