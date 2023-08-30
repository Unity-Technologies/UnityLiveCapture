using System;
using UnityEngine;

namespace Unity.LiveCapture.Ltc
{
    /// <summary>
    /// A component that reads timecodes from an LTC audio stream.
    /// </summary>
    [ExecuteAlways]
    [CreateTimecodeSourceMenuItemAttribute("LTC Timecode Source")]
    [AddComponentMenu("Live Capture/Timecode/LTC Timecode Source")]
    [HelpURL(Documentation.baseURL + "ref-component-ltc-timecode-source" + Documentation.endURL)]
    public class LtcTimecodeSource : TimecodeSource
    {
        /// <summary>
        /// The sampling rate to try to use.
        /// </summary>
        const int k_TargetSampleRate = 44100;

        /// <summary>
        /// The number of seconds of audio to buffer.
        /// </summary>
        /// <remarks>
        /// Changes to this value do not impact latency, just the behaviour if too many samples are recorded before they can
        /// be processed. Since we only need the last sampleRate / frameRate samples to read the most recent timecode,
        /// this is easily enough for our purposes.
        /// </remarks>
        const int k_BufferSize = 1;

        [SerializeField, Tooltip("The frame rate of the timecodes.")]
        [OnlyStandardFrameRates]
        FrameRate m_FrameRate = StandardFrameRate.FPS_24_00;

        [SerializeField, Tooltip("The LTC audio line in.")]
        string m_Device;

        [SerializeField, Tooltip("The audio channel containing the LTC audio.")]
        [Range(0, 15)]
        int m_Channel = 0;

        readonly LtcDecoder m_Decoder = new LtcDecoder();
        AudioClip m_Clip;
        int m_ClipPos;
        float[] m_Buffer;
        Timecode m_LastTimecode;

        /// <inheritdoc/>
        public override string FriendlyName => $"LTC ({name})";

        /// <inheritdoc />
        public override FrameRate FrameRate => m_FrameRate;

        void Reset()
        {
            var devices = Microphone.devices;

            if (devices.Length > 0)
            {
                m_Device = devices[0];
            }
        }

        void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                StopRecording();
                StartRecording();
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Decoder.FrameDecoded += OnFrameDecoded;

            if (string.IsNullOrEmpty(m_Device))
            {
                Reset();
            }

            StartRecording();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            m_Decoder.FrameDecoded -= OnFrameDecoded;

            StopRecording();
        }

        /// <summary>
        /// Sets the audio device used as the LTC audio input.
        /// </summary>
        /// <remarks>
        /// You can query the available devices using <c>Microphone.devices</c>.
        /// </remarks>
        /// <param name="device">The device to use. If null or empty the default audio input will be used.</param>
        public void SetDevice(string device)
        {
            if (m_Device == device)
            {
                return;
            }

            var wasRecording = m_Clip != null;

            StopRecording();

            m_Device = device;

            if (wasRecording)
            {
                StartRecording();
            }
        }

        void StartRecording()
        {
            if (string.IsNullOrEmpty(m_Device))
                return;

            StopRecording();

            // determine the sample rate suitable for the audio
            Microphone.GetDeviceCaps(m_Device, out var minFreq, out var maxFreq);
            var sampleRate = Mathf.Clamp(k_TargetSampleRate, minFreq, maxFreq);

            // prepare the decoder
            m_Decoder.Initialize(sampleRate, m_FrameRate.AsFloat());

            // start recording audio samples
            m_Clip = Microphone.Start(m_Device, true, k_BufferSize, sampleRate);
            m_ClipPos = 0;

            // create a buffer large enough to hold the buffered audio
            var bufferSize = k_BufferSize * sampleRate * m_Clip.channels;

            if (m_Buffer == null || m_Buffer.Length < bufferSize)
            {
                m_Buffer = new float[bufferSize];
            }
        }

        void StopRecording()
        {
            if (Microphone.IsRecording(m_Device))
            {
                Microphone.End(m_Device);
            }

            m_Clip = null;
            m_LastTimecode = default;
        }

        /// <inheritdoc />
        protected override bool TryPollTimecode(out FrameRate frameRate, out Timecode timecode)
        {
            frameRate = m_FrameRate;

            if (ProcessSamples())
            {
                timecode = m_LastTimecode;
                return true;
            }

            timecode = default;
            return false;
        }

        bool ProcessSamples()
        {
            if (m_Clip == null)
            {
                return false;
            }

            var clipPos = Microphone.GetPosition(m_Device);

            if (clipPos == m_ClipPos)
            {
                return true;
            }

            var channelCount = m_Clip.channels;
            var channel = Mathf.Clamp(m_Channel, 0, channelCount - 1);

            m_Clip.GetData(m_Buffer, m_ClipPos);

            var count = clipPos - m_ClipPos;

            if (count < 0)
                count += m_Clip.samples;

            for (var i = 0; i < count; i++)
            {
                var sample = m_Buffer[(i * channelCount) + channel];
                m_Decoder.Decode(sample);
            }

            m_ClipPos = clipPos;
            return true;
        }

        void OnFrameDecoded(LtcDecoder.Frame frame)
        {
            m_LastTimecode = Timecode.FromHMSF(m_FrameRate, frame.hour, frame.minute, frame.second, frame.frame);
        }
    }
}
