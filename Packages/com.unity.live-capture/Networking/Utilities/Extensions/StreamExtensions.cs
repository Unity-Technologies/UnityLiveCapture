using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A class containing extension methods used to read/write data from streams.
    /// </summary>
    static class StreamExtensions
    {
        [ThreadStatic]
        static byte[] s_TempBuffer;
        static UTF8Encoding s_Encoding = new UTF8Encoding(false);

        /// <summary>
        /// Writes a blittable struct to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteStruct<T>(this Stream stream, T data) where T : struct
        {
            stream.WriteStruct(ref data);
        }

        /// <summary>
        /// Writes a blittable struct to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteStruct<T>(this Stream stream, ref T data) where T : struct
        {
            var size = SizeOfCache<T>.Size;

            EnsureBufferCapacity(size);
            s_TempBuffer.WriteStruct(ref data);

            stream.Write(s_TempBuffer, 0, size);
        }

        /// <summary>
        /// Reads a blittable struct from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the struct from.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var size = SizeOfCache<T>.Size;

            Read(stream, size);

            return s_TempBuffer.ReadStruct<T>();
        }

        /// <summary>
        /// Writes a length prefixed UTF-8 string to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the string into.</param>
        /// <param name="data">The struct to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(this Stream stream, string str)
        {
            var strLen = s_Encoding.GetByteCount(str);
            var size = sizeof(int) + strLen;

            EnsureBufferCapacity(size);
            var offset = s_TempBuffer.WriteStruct(ref strLen);
            s_Encoding.GetBytes(str, 0, str.Length, s_TempBuffer, offset);

            stream.Write(s_TempBuffer, 0, size);
        }

        /// <summary>
        /// Reads a length prefixed UTF-8 string from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the string from.</param>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream)
        {
            var strLen = stream.ReadStruct<int>();

            Read(stream, strLen);

            return s_Encoding.GetString(s_TempBuffer, 0, strLen);
        }

        static void Read(Stream stream, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must be non negative.");

            EnsureBufferCapacity(count);

            if (count == 0)
                return;

            var offset = 0;

            do
            {
                var readBytes = stream.Read(s_TempBuffer, offset, count);

                if (readBytes <= 0)
                    throw new EndOfStreamException();

                offset += readBytes;
            }
            while (offset < count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnsureBufferCapacity(int capacity)
        {
            if (s_TempBuffer == null || s_TempBuffer.Length < capacity)
                s_TempBuffer = new byte[capacity];
        }

        /// <summary>
        /// Copies a native array into a stream.
        /// </summary>
        /// <param name="stream">The stream to write the array into.</param>
        /// <param name="array">The array to write.</param>
        /// <typeparam name="T">The type of data in the native array.</typeparam>
        /// <returns><see langword="true"/> if the array was successfully written into the stream; otherwise, <see langword="false"/>.</returns>
        public static bool WriteArray<T>(this MemoryStream stream, NativeArray<T> array) where T : struct
        {
            stream.SetLength(stream.Length + array.Length);

            if (!stream.TryGetBuffer(out var buffer) || buffer.Array == null)
            {
                return false;
            }

            unsafe
            {
                fixed (void* streamPtr = &buffer.Array[buffer.Offset + stream.Position])
                {
                    UnsafeUtility.MemCpy(streamPtr, array.GetUnsafePtr(), array.Length);
                }
            }

            stream.Position += array.Length;
            return true;
        }
    }
}
