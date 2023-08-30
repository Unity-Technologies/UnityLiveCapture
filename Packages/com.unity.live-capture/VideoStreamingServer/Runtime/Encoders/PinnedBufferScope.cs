using System;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    /// <summary>
    /// Use this pointer to temporarily pin a buffer with a using block.
    /// </summary>
    struct PinnedBufferScope : IDisposable
    {
        GCHandle m_Handle;

        public unsafe byte* pointer => (byte*)m_Handle.AddrOfPinnedObject();

        public PinnedBufferScope(ArraySegment<byte> buffer)
        {
            m_Handle = GCHandle.Alloc(buffer.Array, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            if (m_Handle.IsAllocated)
            {
                m_Handle.Free();
                m_Handle = default;
            }
        }
    }
}
