using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A struct that contains the texture data transmitted over the network.
    /// </summary>
    readonly struct TextureData
    {
        /// <summary>
        /// The texture sent over the network.
        /// </summary>
        /// <remarks>May be null if the receiver couldn't interpret the transmitted texture.</remarks>
        public Texture2D texture { get; }

        /// <summary>
        /// The metadata string sent along with the texture.
        /// </summary>
        /// <remarks>
        /// You can use this to help identify the texture.
        /// </remarks>
        public string metadata { get; }

        /// <summary>
        /// Creates a new <see cref="TextureData"/> instance.
        /// </summary>
        /// <param name="texture">The texture sent over the network.</param>
        /// <param name="metadata">The metadata string sent along with the texture.</param>
        public TextureData(Texture2D texture, string metadata)
        {
            this.texture = texture;
            this.metadata = metadata ?? string.Empty;
        }
    }

    /// <summary>
    /// An enum defining the compression formats that can be used when sending the textures over the network.
    /// </summary>
    enum TextureCompression : int
    {
        /// <summary>
        /// Use the raw texture date.
        /// </summary>
        /// <remarks>
        /// This requires the least processing time and has the best quality, but uses the most bandwidth.
        /// It also supports any type of texture format.
        /// </remarks>
        Raw = 0,

        /// <summary>
        /// Compress the texture as a PNG.
        /// </summary>
        /// <remarks>
        /// Use this option to minimize bandwidth at the cost of performance.
        /// </remarks>
        PNG = 10,

        /// <summary>
        /// Compress the texture as a JPEG.
        /// </summary>
        /// <remarks>
        /// Use this option to minimize bandwidth at the cost of quality.
        /// </remarks>
        JPEG = 20,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TextureDescription
    {
        public int Width;
        public int Height;
        public GraphicsFormat GraphicsFormat;
        public int MipCount;
        public int AnisoLevel;
        public TextureWrapMode WrapMode;
        public FilterMode FilterMode;
    }

    /// <summary>
    /// A message used to send textures to a remote.
    /// </summary>
    sealed class TextureSender : DataSender<TextureData>
    {
        /// <summary>
        /// The latest version of the message serialized format.
        /// </summary>
        internal new const int k_Version = 0;

        readonly int m_Version;
        readonly TextureCompression m_Compression;

        /// <summary>
        /// Creates a new <see cref="TextureSender"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        /// <param name="compression">The compression format used when sending the textures over the network.</param>
        public TextureSender(
            string id,
            ChannelType channel = ChannelType.ReliableOrdered,
            DataOptions options = DataOptions.None,
            TextureCompression compression = TextureCompression.Raw
        )
            : base(id, channel, options)
        {
            m_Version = k_Version;
            m_Compression = compression;
        }

        /// <inheritdoc />
        [Preserve]
        internal TextureSender(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    m_Compression = stream.ReadStruct<TextureCompression>();
                    break;
                default:
                    throw new Exception($"{nameof(TextureSender)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc/>
        internal override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteStruct(m_Version);

            switch (m_Version)
            {
                case 0:
                    stream.WriteStruct(m_Compression);
                    break;
                default:
                    throw new Exception($"{nameof(TextureSender)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new TextureReceiver(ID, Channel, m_Options, m_Compression);

        /// <inheritdoc />
        protected override void OnWrite(MemoryStream stream, ref TextureData data)
        {
            switch (m_Version)
            {
                case 0:
                {
                    var texture = data.texture;

                    var description = new TextureDescription
                    {
                        Width = texture.width,
                        Height = texture.height,
                        GraphicsFormat = texture.graphicsFormat,
                        MipCount = texture.mipmapCount,
                        AnisoLevel = texture.anisoLevel,
                        WrapMode = texture.wrapMode,
                        FilterMode = texture.filterMode,
                    };

                    stream.WriteStruct(ref description);
                    stream.WriteString(texture.name);
                    stream.WriteString(data.metadata);
                    WriteTexture(stream, texture, m_Compression);
                    break;
                }
                default:
                    throw new Exception($"{nameof(TextureSender)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Gets a <see cref="TextureSender"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out TextureSender message)
        {
            try
            {
                message = protocol.GetDataSender<TextureData, TextureSender>(id);
                return true;
            }
            catch
            {
                message = default;
                return false;
            }
        }

        static void WriteTexture(MemoryStream stream, Texture2D texture, TextureCompression compression)
        {
            var array = texture.GetRawTextureData<byte>();

            switch (compression)
            {
                case TextureCompression.Raw:
                {
                    stream.WriteStruct(array.Length);
                    stream.WriteArray(array);
                    break;
                }
                case TextureCompression.PNG:
                {
                    using var png = ImageConversion.EncodeNativeArrayToPNG(
                        array,
                        texture.graphicsFormat,
                        (uint)texture.width,
                        (uint)texture.height
                    );

                    stream.WriteStruct(png.Length);
                    stream.WriteArray(png);
                    break;
                }
                case TextureCompression.JPEG:
                {
                    using var jpg = ImageConversion.EncodeNativeArrayToJPG(
                        array,
                        texture.graphicsFormat,
                        (uint)texture.width,
                        (uint)texture.height
                    );

                    stream.WriteStruct(jpg.Length);
                    stream.WriteArray(jpg);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(compression), compression, null);
            }
        }
    }
}
