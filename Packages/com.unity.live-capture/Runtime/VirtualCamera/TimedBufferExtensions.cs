using System;
using System.Diagnostics.CodeAnalysis;

namespace Unity.LiveCapture.VirtualCamera
{
    static class TimedBufferExtensions
    {
        /// <summary>
        /// Retrieve the latest element in the collection that precedes the cutoff time.
        /// </summary>
        /// <param name="buffer">The buffer to retrieve from.</param>
        /// <param name="cutoff">The cutoff time.</param>
        /// <typeparam name="T">The type of element contained in the collection.</typeparam>
        /// <returns>The element latest element. <see langword="null"/> if the buffer is empty or there
        /// are no elements before the cutoff.</returns>
        public static T? GetLatest<T>([NotNull] this ITimedDataBuffer<T> buffer, FrameTime cutoff) where T : struct
        {
            // LINQ equivalent:
            // return buffer
            //     .TakeWhile(x => x.frameTime <= cutoff)
            //     .LastOrDefault().value;

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Count == 0)
            {
                return null;
            }

            T? result = null;

            foreach (var (frameTime, value) in buffer)
            {
                if (frameTime <= cutoff)
                {
                    result = value;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}
