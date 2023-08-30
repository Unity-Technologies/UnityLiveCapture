using System;
using UnityEngine;

namespace Unity.LiveCapture.Ltc
{
    class LtcDecoder
    {
        /// <summary>
        /// A struct containing the decoded frame info.
        /// </summary>
        public struct Frame
        {
            public byte hour;
            public byte minute;
            public byte second;
            public byte frame;
            public bool isDropFrame;
        }

        /// <summary>
        /// The size of an LTC code in bits.
        /// </summary>
        const int k_CodewordLength = 80;

        /// <summary>
        /// The bit pattern used to indicate the end of a codeword.
        /// </summary>
        /// <remarks>
        /// This pattern is guaranteed not to appear elsewhere in the codeword.
        /// </remarks>
        const ushort k_SyncWord = 0b_1011_1111_1111_1100;

        /// <summary>
        /// The bit pattern used to indicate the end of a codeword when the LTC signal is played backwards.
        /// </summary>
        /// <remarks>
        /// This pattern is guaranteed not to appear elsewhere in the codeword.
        /// </remarks>
        const ushort K_ReverseSyncWord = 0b_0011_1111_1111_1101;

        /// <summary>
        /// The factor by which the volume envelope is decreased each sample until a louder
        /// sample is detected.
        /// </summary>
        /// <remarks>
        /// A smaller value makes the volume envelope more sensitive to quick changes in the signal
        /// volume, but more impacted by noise. Must be in the range [0,1].
        /// </remarks>
        const float k_VolumeFalloff = 0.95f;

        /// <summary>
        /// The fraction of the volume that a sample must exceed to be detected as a hi/lo sample.
        /// </summary>
        /// <remarks>
        /// A lower value makes the biphase state switching more sensitive. Must be in the range [0,1].
        /// </remarks>
        const float k_Threshold = 0.5f;

        /// <summary>
        /// The factor by which the period length is adjusted based on the duration of the most
        /// recent period.
        /// </summary>
        /// <remarks>
        /// A larger value makes the decoder respond faster to changes in the frame rate.
        /// Must be in the range [0,1].
        /// </remarks>
        const float k_PeriodAdaptivity = 0.25f;

        /// <summary>
        /// Was the last transition to the hi state.
        /// </summary>
        bool m_IsHi;

        /// <summary>
        /// Transition counter for true bits.
        /// </summary>
        bool m_Tick;

        /// <summary>
        /// The number of samples since the last transition.
        /// </summary>
        int m_SampleCount;

        /// <summary>
        /// The estimated sample count per bit.
        /// </summary>
        float m_Period;

        /// <summary>
        /// The approximate peak sample value.
        /// </summary>
        float m_Volume;

        /// <summary>
        /// A 128 bit FIFO queue for the bit stream.
        /// </summary>
        /// <remarks>
        /// The queue size must not be less then <see cref="k_CodewordLength"/>.
        /// </remarks>
        (ulong lo, ulong hi) m_Queue;

        /// <summary>
        /// The signal volume in dB.
        /// </summary>
        public float Volume => 20f * Mathf.Log10(m_Volume);

        /// <summary>
        /// An event invoked when a Timecode frame is decoded.
        /// </summary>
        public event Action<Frame> FrameDecoded;

        /// <summary>
        /// Creates a new <see cref="LtcDecoder"/> instance.
        /// </summary>
        /// <param name="approxSampleRate">The approximate sample rate of the audio in Hz.</param>
        /// <param name="approxFrameRate">The approximate frame rate of the Timecodes in Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="approxSampleRate"/> or <paramref name="approxFrameRate"/>
        /// is invalid.</exception>
        public LtcDecoder(float approxSampleRate = 44100, float approxFrameRate = 24)
        {
            Initialize(approxSampleRate, approxFrameRate);
        }

        /// <summary>
        /// Initializes the decoder back to its default state.
        /// </summary>
        /// <param name="approxSampleRate">The approximate sample rate of the audio in Hz.</param>
        /// <param name="approxFrameRate">The approximate frame rate of the Timecodes in Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="approxSampleRate"/> or <paramref name="approxFrameRate"/>
        /// is invalid.</exception>
        public void Initialize(float approxSampleRate, float approxFrameRate)
        {
            if (approxSampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(approxSampleRate), approxSampleRate, $"Must be a positive number!");
            if (approxFrameRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(approxFrameRate), approxFrameRate, $"Must be a positive number!");

            // compute the approximate number of samples per bit in the ltc codeword
            m_Period = approxSampleRate / (approxFrameRate * k_CodewordLength);

            m_IsHi = false;
            m_Tick = false;
            m_SampleCount = 0;
            m_Volume = 0;
            m_Queue = default;
        }

        /// <summary>
        /// Decodes LTC audio samples.
        /// </summary>
        /// <param name="samples">The sample buffer, with values in the range [-1,1].</param>
        /// <param name="offset">The position in the buffer of the first sample to decode.</param>
        /// <param name="count">The number of samples in the buffer to decode.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="samples"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the combination of <paramref name="offset"/> and <paramref name="count"/>
        /// is invalid.</exception>
        public void Decode(float[] samples, int offset, int count)
        {
            if (samples == null)
                throw new ArgumentNullException(nameof(samples));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, $"Cannot be negative!");
            if (offset >= samples.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, $"Must not exceed sample buffer length ({samples.Length})!");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, $"Cannot be negative!");
            if (offset + count > samples.Length)
                throw new ArgumentOutOfRangeException($"This combination of {nameof(offset)} ({offset}) and {nameof(count)} ({count}) exceeds buffer length ({samples.Length})!");

            for (var i = 0; i < count; i++)
            {
                Decode(samples[i]);
            }
        }

        /// <summary>
        /// Decodes an LTC audio sample.
        /// </summary>
        /// <param name="sample">A sample in the range [-1,1].</param>
        public void Decode(float sample)
        {
            m_SampleCount++;

            // update the signal volume envelope
            m_Volume = Mathf.Max(Mathf.Abs(sample), m_Volume * k_VolumeFalloff);

            // compute the hi/lo envelope
            var thresh = k_Threshold * m_Volume;

            // check if biphase state has not changed
            if ((m_IsHi && sample > -thresh) || (!m_IsHi && sample < thresh))
            {
                return;
            }

            // Each period either represents a 0 bit when there is one transition at the end of the period,
            // or a 1 bit when there is a transition in both the middle and end of the period. The following
            // graph shows what the bits would be given the shown samples.

            //----|----Period----|----Period----|----Period----|----Period----|----
            //    |       0      |       1      |       1      |       0      |
            //
            // --                  ----           ----           ------------
            //   \                /    \         /    \         /            \
            //----\--------------/------\-------/------\-------/--------------\----
            //     \            /        \     /        \     /                \
            //      ------------          -----          -----                  ---

            // determine if this transition relates to a 0 bit or a 1 bit
            if (m_SampleCount > m_Period * 0.75f)
            {
                // decode a 0 bit
                ProcessBit(false);
            }
            else
            {
                // determine if this transition is at the middle or end of a period
                if (m_Tick)
                {
                    // decode a 1 bit, 2nd transition has occured
                    ProcessBit(true);
                    m_Tick = false;
                }
                else
                {
                    // 1st transition has occured
                    m_Tick = true;
                }

                // Since only a half period has occured, double the sample count
                // so the adaptive period length estimation is updated correctly.
                m_SampleCount *= 2;
            }

            if (m_SampleCount > m_Period * 3f)
            {
                // if there was a long preceding silence reset the state
                m_Queue = default;
            }
            else
            {
                // update the estimated period length adaptively
                m_Period = Mathf.Lerp(m_Period, m_SampleCount, k_PeriodAdaptivity);
            }

            m_IsHi = sample > 0f;
            m_SampleCount = 0;
        }

        void ProcessBit(bool bit)
        {
            // Update the bit queue. We only need to use the first 80 bits of the queue to match the codeword length,
            // so we insert bits into the 80th position in the bit queue.
            var newValue = bit ? 1ul : 0ul;

            m_Queue.lo = (m_Queue.lo >> 1) | (m_Queue.hi << 63);
            m_Queue.hi = (m_Queue.hi >> 1) | (newValue << 15);

            // If we have encountered the sync word parse the timecode. The LTC audio can be played forward and backward,
            // so we must detect either case.
            if ((ushort)m_Queue.hi == k_SyncWord)
            {
                ParseTimecode(m_Queue.lo);
            }
            else if ((ushort)m_Queue.lo == K_ReverseSyncWord)
            {
                // if the audio is backwards flip the bit ordering so we can reuse the parsing logic
                var reversedCodeword = (m_Queue.lo >> 16) | (m_Queue.hi << 48);
                var codeword = ReverseBits(reversedCodeword);

                ParseTimecode(codeword);
            }
        }

        void ParseTimecode(ulong codeword)
        {
            var s1 = (int)((codeword) & 0xffff);
            var s2 = (int)((codeword >> 16) & 0xffff);
            var s3 = (int)((codeword >> 32) & 0xffff);
            var s4 = (int)((codeword >> 48) & 0xffff);

            var frame = new Frame
            {
                frame = (byte)((s1 & 0xf) + ((s1 >> 8) & 0x3) * 10),
                second = (byte)((s2 & 0xf) + ((s2 >> 8) & 0x7) * 10),
                minute = (byte)((s3 & 0xf) + ((s3 >> 8) & 0x7) * 10),
                hour = (byte)((s4 & 0xf) + ((s4 >> 8) & 0x3) * 10),
                isDropFrame = ((s1 >> 10) & 0x1) == 1,
            };

            FrameDecoded?.Invoke(frame);
        }

        static ulong ReverseBits(ulong n)
        {
            n = ((n >> 1) & 0x5555555555555555ul) | ((n << 1) & 0xaaaaaaaaaaaaaaaaul);
            n = ((n >> 2) & 0x3333333333333333ul) | ((n << 2) & 0xccccccccccccccccul);
            n = ((n >> 4) & 0x0f0f0f0f0f0f0f0ful) | ((n << 4) & 0xf0f0f0f0f0f0f0f0ul);
            n = ((n >> 8) & 0x00ff00ff00ff00fful) | ((n << 8) & 0xff00ff00ff00ff00ul);
            n = ((n >> 16) & 0x0000ffff0000fffful) | ((n << 16) & 0xffff0000ffff0000ul);
            n = ((n >> 32) & 0x00000000fffffffful) | ((n << 32) & 0xffffffff00000000ul);
            return n;
        }
    }
}
