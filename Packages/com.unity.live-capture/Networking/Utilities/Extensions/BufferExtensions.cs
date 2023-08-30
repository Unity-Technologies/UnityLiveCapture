using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A class containing extension methods used to marshal structs to/from byte arrays.
    /// </summary>
    static class BufferExtensions
    {
        /// <summary>
        /// Writes a blittable struct to the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <param name="offset">The offset into the buffer to start writing at.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The index in the buffer immediately following the last byte written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteStruct<T>(this byte[] buffer, T data, int offset = 0) where T : struct
        {
            return buffer.WriteStruct(ref data, offset);
        }

        /// <summary>
        /// Writes a blittable struct to the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <param name="offset">The offset into the buffer to start writing at.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The index in the buffer immediately following the last byte written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteStruct<T>(this byte[] buffer, ref T data, int offset = 0) where T : struct
        {
            var size = SizeOfCache<T>.Size;

            fixed (byte* ptr = &buffer[offset])
            {
                UnsafeUtility.MemClear(ptr, size);

                if (typeof(T).IsEnum || UnsafeUtility.IsBlittable<T>())
                {
                    UnsafeUtility.CopyStructureToPtr(ref data, ptr);
                }
                else
                {
                    Marshal.StructureToPtr(data, (IntPtr)ptr, false);
                }
            }

            return offset + size;
        }

        /// <summary>
        /// Reads a blittable struct from the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to read the struct from.</param>
        /// <param name="offset">The offset into the buffer to start reading from.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadStruct<T>(this byte[] buffer, int offset = 0) where T : struct
        {
            fixed (byte* ptr = &buffer[offset])
            {
                if (typeof(T).IsEnum || UnsafeUtility.IsBlittable<T>())
                {
                    UnsafeUtility.CopyPtrToStructure(ptr, out T value);
                    return value;
                }
                else
                {
                    return Marshal.PtrToStructure<T>((IntPtr)ptr);
                }
            }
        }

        /// <summary>
        /// Reads a blittable struct from the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to read the struct from.</param>
        /// <param name="offset">The offset into the buffer to start reading from.</param>
        /// <param name="nextOffset">The index in the buffer immediately following the last byte read.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadStruct<T>(this byte[] buffer, int offset, out int nextOffset) where T : struct
        {
            nextOffset = offset + SizeOfCache<T>.Size;

            fixed (byte* ptr = &buffer[offset])
            {
                if (typeof(T).IsEnum || UnsafeUtility.IsBlittable<T>())
                {
                    UnsafeUtility.CopyPtrToStructure(ptr, out T value);
                    return value;
                }
                else
                {
                    return Marshal.PtrToStructure<T>((IntPtr)ptr);
                }
            }
        }
    }
}
