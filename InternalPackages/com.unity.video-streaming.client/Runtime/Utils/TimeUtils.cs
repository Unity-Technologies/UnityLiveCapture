using System;
using System.Runtime.CompilerServices;

namespace Unity.LiveCapture.VideoStreaming.Client.Utils
{
    static class TimeUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTimeOver(int currentTicks, int previousTicks, int interval)
        {
            if (Math.Abs(currentTicks - previousTicks) >= interval)
                return true;

            return false;
        }
    }
}
