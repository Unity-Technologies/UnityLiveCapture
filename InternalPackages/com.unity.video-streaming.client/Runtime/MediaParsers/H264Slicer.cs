using System;
using System.Diagnostics;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames.Video;
using Unity.LiveCapture.VideoStreaming.Client.Utils;
using UnityDbg = UnityEngine.Debug;

namespace Unity.LiveCapture.VideoStreaming.Client.MediaParsers
{
    static class H264Slicer
    {
        public static void Slice(ArraySegment<byte> byteSegment, Action<ArraySegment<byte>> nalUnitHandler)
        {
            Debug.Assert(byteSegment.Array != null, "byteSegment.Array != null");
            Debug.Assert(ArrayUtils.StartsWith(byteSegment.Array, byteSegment.Offset, byteSegment.Count,
                RawH264Frame.StartMarker));

            int endIndex = byteSegment.Offset + byteSegment.Count;

            int nalUnitStartIndex = ArrayUtils.IndexOfBytes(byteSegment.Array, RawH264Frame.StartMarker,
                byteSegment.Offset, byteSegment.Count);

            if (nalUnitStartIndex == -1)
                // Start pattern not found, assuming the whole content is a single nalu.
                nalUnitHandler?.Invoke(byteSegment);
            // Shouldn't we return right here???

            while (nalUnitStartIndex != -1)
            {
                int tailLength = endIndex - nalUnitStartIndex;

                if (tailLength == RawH264Frame.StartMarker.Length)
                    // If what's left is exactly the marker lenghth, we know there cannot be a nalu in there.
                    return;

                int nalUnitType = byteSegment.Array[nalUnitStartIndex + RawH264Frame.StartMarker.Length] & 0x1F;

                int nextNalUnitStartIndex = ArrayUtils.IndexOfBytes(byteSegment.Array, RawH264Frame.StartMarker,
                    nalUnitStartIndex + RawH264Frame.StartMarker.Length, tailLength - RawH264Frame.StartMarker.Length);

                int nalUnitLength = nextNalUnitStartIndex == -1
                    ? tailLength
                    : (nextNalUnitStartIndex - nalUnitStartIndex);

                // When we find I (5) or P (1) frame, invoke the nalu handler. Ignore anything else. Wondering if
                // this will end up forgetting important stuff?
                if (nalUnitType == 5 || nalUnitType == 1)
                {
                    //UnityDbg.LogFormat("Slicer found nalu type {0} at {1}, {2} bytes", nalUnitType, nalUnitStartIndex, nalUnitLength);
                    nalUnitHandler?.Invoke(new ArraySegment<byte>(byteSegment.Array, nalUnitStartIndex, nalUnitLength));

                    // Whaaat? Feels like we should continue iterating, no?
                    //return;
                }
                else
                {
                    //UnityDbg.LogFormat("Slicer ignoring NALu {0}", nalUnitType);
                }
                // Found a nalu type we don't care about, keep searching.
#if TESTING_SLICE_LOGIC
                if (nextNalUnitStartIndex > 0)
                {
                    // Found a nalu. If it's longer than just the marker, report it.
                    nalUnitLength = nextNalUnitStartIndex - nalUnitStartIndex;

                    if (nalUnitLength != RawH264Frame.StartMarker.Length)
                        nalUnitHandler?.Invoke(new ArraySegment<byte>(byteSegment.Array, nalUnitStartIndex, nalUnitLength));
                }
                else
                {
                    // No next nalu. Report the rest of the buffer as a single nalu.
                    nalUnitHandler?.Invoke(new ArraySegment<byte>(byteSegment.Array, nalUnitStartIndex, tailLength));
                    return;
                }
#endif
                nalUnitStartIndex = nextNalUnitStartIndex;
            }
        }
    }
}
