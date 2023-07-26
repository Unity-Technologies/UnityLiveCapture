using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.LiveCapture.Ntp
{
    /// <summary>
    /// A component that gets timecode from an network time protocol (NTP) server.
    /// </summary>
    [ExecuteAlways]
    [CreateTimecodeSourceMenuItemAttribute("NTP Timecode Source")]
    [AddComponentMenu("Live Capture/Timecode/NTP Timecode Source")]
    [HelpURL(Documentation.baseURL + "ref-component-ntp-timecode-source" + Documentation.endURL)]
    public class NtpTimecodeSource : TimecodeSource
    {
        [SerializeField, Tooltip("The frame rate of the timecodes.")]
        [OnlyStandardFrameRates]
        FrameRate m_FrameRate = StandardFrameRate.FPS_24_00;

        [SerializeField, Tooltip("The hostname or IP address of NTP server to get the time from.")]
        string m_ServerAddress = "pool.ntp.org";

        readonly NtpClient m_Client = new NtpClient();
        readonly object m_Lock = new object();
        DateTime? m_ReferenceTime;
        DateTime? m_LastUpdateLocalTime;
        Task m_UpdateReferenceTimeTask;

        /// <inheritdoc/>
        public override string FriendlyName => $"NTP ({name})";

        /// <inheritdoc />
        public override FrameRate FrameRate => m_FrameRate;

        void OnValidate()
        {
            ReInitialize();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            ReInitialize();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            ReInitialize();
        }

        void ReInitialize()
        {
            lock (m_Lock)
            {
                m_ReferenceTime = null;
                m_LastUpdateLocalTime = null;
            }

            m_UpdateReferenceTimeTask = null;

            m_Client.Disconnect();
        }

        /// <summary>
        /// Queries the NTP server to resynchronize the time with the server clock.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateReferenceTime(DateTime.Now);
        }

        /// <inheritdoc />
        protected override bool TryPollTimecode(out FrameRate frameRate, out Timecode timecode)
        {
            var localTime = DateTime.Now;

            // poll the server for the current time if required
            var needsUpdate = false;

            lock (m_Lock)
            {
                if (!m_LastUpdateLocalTime.HasValue)
                {
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
            {
                UpdateReferenceTime(localTime);
            }

            // calculate the timecode from the current NTP time
            var time = GetCurrentTime(localTime);

            frameRate = m_FrameRate;
            timecode = Timecode.FromSeconds(m_FrameRate, time?.TimeOfDay.TotalSeconds ?? 0.0);
            return time != null;
        }

        DateTime? GetCurrentTime(DateTime localTime)
        {
            lock (m_Lock)
            {
                if (!m_ReferenceTime.HasValue || !m_LastUpdateLocalTime.HasValue)
                {
                    return null;
                }

                // The last known ntp time combined with the local time since we received that ntp time
                return m_ReferenceTime + (localTime - m_LastUpdateLocalTime);
            }
        }

        void UpdateReferenceTime(DateTime localTime)
        {
            m_UpdateReferenceTimeTask ??= UpdateReferenceTimeAsync(localTime);
        }

        async Task UpdateReferenceTimeAsync(DateTime localTime)
        {
            var clientTime = GetCurrentTime(localTime) ?? localTime;
            var newClientTime = await m_Client.PollTimeAsync(m_ServerAddress, clientTime).ConfigureAwait(false);

            if (newClientTime.HasValue)
            {
                lock (m_Lock)
                {
                    m_ReferenceTime = newClientTime;
                    m_LastUpdateLocalTime = DateTime.Now;
                }
            }

            m_UpdateReferenceTimeTask = null;
        }
    }
}
