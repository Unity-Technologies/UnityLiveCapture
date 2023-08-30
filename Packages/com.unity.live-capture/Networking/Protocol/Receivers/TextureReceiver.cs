using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to receive textures from a remote.
    /// </summary>
    sealed class TextureReceiver : DataReceiver<TextureData>
    {
        readonly int m_Version;
        readonly TextureCompression m_Compression;

        /// <summary>
        /// Creates a new <see cref="TextureReceiver"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        /// <param name="compression">The compression format used when sending the textures over the network.</param>
        public TextureReceiver(
            string id,
            ChannelType channel = ChannelType.ReliableOrdered,
            DataOptions options = DataOptions.None,
            TextureCompression compression = TextureCompression.Raw
        )
            : base(id, channel, options)
        {
            m_Version = TextureSender.k_Version;
            m_Compression = compression;
        }

        /// <inheritdoc />
        [Preserve]
        internal TextureReceiver(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    m_Compression = stream.ReadStruct<TextureCompression>();
                    break;
                default:
                    throw new Exception($"{nameof(TextureReceiver)} version is not supported by this application version.");
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
                    throw new Exception($"{nameof(TextureReceiver)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new TextureSender(ID, Channel, m_Options, m_Compression);

        /// <inheritdoc />
        protected override TextureData OnRead(MemoryStream stream)
        {
            switch (m_Version)
            {
                case 0:
                {
                    var description = stream.ReadStruct<TextureDescription>();
                    var textureName = stream.ReadString();
                    var metadata = stream.ReadString();

#if UNITY_2023_2_OR_NEWER
                    if (SystemInfo.IsFormatSupported(description.GraphicsFormat, GraphicsFormatUsage.Sample))
#else
                    if (SystemInfo.IsFormatSupported(description.GraphicsFormat, FormatUsage.Sample))
#endif
                    {
                        var texture = new Texture2D(
                            description.Width,
                            description.Height,
                            description.GraphicsFormat,
                            description.MipCount,
                            TextureCreationFlags.None)
                        {
                            name = textureName,
                            anisoLevel = description.AnisoLevel,
                            wrapMode = description.WrapMode,
                            filterMode = description.FilterMode,
                        };

                        ReadTexture(stream, texture, m_Compression);

                        return new TextureData(texture, metadata);
                    }

                    return new TextureData(null, metadata);
                }
                default:
                    throw new Exception($"{nameof(TextureReceiver)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Gets a <see cref="TextureReceiver"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out TextureReceiver message)
        {
            try
            {
                message = protocol.GetDataReceiver<TextureData, TextureReceiver>(id);
                return true;
            }
            catch
            {
                message = default;
                return false;
            }
        }

        static void ReadTexture(MemoryStream stream, Texture2D texture, TextureCompression compression)
        {
            var length = stream.ReadStruct<int>();

            switch (compression)
            {
                case TextureCompression.Raw:
                {
                    var endPosition = (int)(stream.Position + length);

                    if (stream.Length >= endPosition && stream.TryGetBuffer(out var buffer) && buffer.Array != null)
                    {
                        unsafe
                        {
                            fixed (void* streamPtr = &buffer.Array[buffer.Offset + stream.Position])
                            {
                                texture.LoadRawTextureData((IntPtr)streamPtr, length);
                                texture.Apply(true);
                            }
                        }
                    }

                    stream.Position += length;
                    break;
                }
                case TextureCompression.PNG:
                case TextureCompression.JPEG:
                {
                    var data = new byte[length];
                    stream.Read(data, 0, data.Length);
                    texture.LoadImage(data, true);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(compression), compression, null);
            }
        }
    }
}
