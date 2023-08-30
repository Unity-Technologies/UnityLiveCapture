using System;
using UnityEngine;

namespace Unity.LiveCapture.Ntp
{
    /// <summary>
    /// An enum defining the leap second warnings used to notify clients about upcoming leap seconds.
    /// </summary>
    enum NtpLeapIndicator
    {
        /// <summary>
        /// There is no upcoming leap second.
        /// </summary>
        NoWarning = 0,

        /// <summary>
        /// The last minute of the current day has 61 seconds.
        /// </summary>
        LastMinuteHas61Seconds = 1,

        /// <summary>
        /// The last minute of the current day has 59 seconds.
        /// </summary>
        LastMinuteHas59Seconds = 2,

        /// <summary>
        /// Indicates the server is not correctly synchronized to a primary clock and may drift.
        /// </summary>
        AlarmCondition = 3,
    }

    /// <summary>
    /// An enum defining the protocol modes.
    /// </summary>
    enum NtpMode
    {
        /// <summary>
        /// The message sender periodically sends messages to mutually synchronize with the message receiver.
        /// </summary>
        SymmetricActive = 1,

        /// <summary>
        /// The message sender mutually synchronizes with the message receiver.
        /// </summary>
        SymmetricPassive = 2,

        /// <summary>
        /// The message sender will be synchronized by, but not synchronize the message receiver.
        /// </summary>
        Client = 3,

        /// <summary>
        /// The message sender will synchronize, but not be synchronized by the message receiver.
        /// </summary>
        Server = 4,

        /// <summary>
        /// The message sender will synchronize, but not be synchronized by any message receivers.
        /// </summary>
        Broadcast = 5,
    }

    /// <summary>
    /// A class that represents a SNTP (RCF 4330) packet used for communication between a NTP server and client.
    /// </summary>
    /// <seealso href="https://datatracker.ietf.org/doc/html/rfc4330"/>
    /// <seealso href="https://datatracker.ietf.org/doc/html/rfc5905"/>
    class NtpPacket
    {
        /// <summary>
        /// The date from which time starts in the NTP date format.
        /// </summary>
        static readonly DateTime k_Epoch = new DateTime(1900, 1, 1);

        /// <summary>
        /// The number of ticks per second used by <see cref="DateTime"/>.
        /// </summary>
        const ulong k_TicksPerSecond = 1000UL * TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// The size of the packet buffer in bytes.
        /// </summary>
        const int k_BufferLength = 1024;

        /// <summary>
        /// The raw packet buffer.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// The leap second warning used to notify clients about upcoming leap seconds.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="Ntp.NtpLeapIndicator.NoWarning"/>.
        /// </remarks>
        public NtpLeapIndicator LeapIndicator
        {
            get => (NtpLeapIndicator)((Buffer[0] & 0xC0) >> 6);
            set => SetBits(0, 0xC0, (int)value << 6);
        }

        /// <summary>
        /// The protocol version.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="NtpConstants.CurrentVersion"/>.
        /// </remarks>
        public int Version
        {
            get => (Buffer[0] & 0x38) >> 3;
            set => SetBits(0, 0x38, value << 3);
        }

        /// <summary>
        /// The packet mode.
        /// </summary>
        /// <remarks>
        /// This is set to indicate the type of synchronization occuring with the message receiver.
        /// The default value is <see cref="NtpMode.Client"/>.
        /// </remarks>
        public NtpMode Mode
        {
            get => (NtpMode)(Buffer[0] & 0x07);
            set => SetBits(0, 0x07, (int)value);
        }

        /// <summary>
        /// The distance from the reference clock.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is only used by <see cref="NtpMode.Server"/> messages. Servers directly connected to primary
        /// reference clocks are indicated with a value of 1, with the value increasing for each secondary reference
        /// clock between the server and the primary reference clock.
        /// </para>
        /// <para>
        /// A value of 0 is special and indicates a "kiss-o'-death" message.
        /// </para>
        /// </remarks>
        public int Stratum
        {
            get => Buffer[1];
            set => Buffer[1] = (byte)value;
        }

        /// <summary>
        /// The maximum interval between successive messages in seconds.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="NtpMode.Server"/> messages.
        /// </remarks>
        public double PollInterval
        {
            get => 1 << Buffer[2];
            set => Buffer[2] = (byte)Mathf.Clamp((int)Math.Round(Math.Log(value, 2)), 4, 17);
        }

        /// <summary>
        /// The precision of the system clock in seconds.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="NtpMode.Server"/> messages.
        /// </remarks>
        public double Precision
        {
            get => Math.Pow(2, Buffer[3]);
            set => Buffer[3] = (byte)Mathf.Clamp((int)Math.Round(Math.Log(value, 2)), -20, -6);
        }

        /// <summary>
        /// Total round-trip delay to the reference clock.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="NtpMode.Server"/> messages. The value can be positive or negative
        /// depending on the relative time and frequency offsets.
        /// </remarks>
        public TimeSpan RootDelay
        {
            get => GetTimeSpanSigned(4);
            set => SetTimeSpanSigned(4, value);
        }

        /// <summary>
        /// The maximum error due to the clock frequency tolerance in seconds.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="NtpMode.Server"/> messages.
        /// </remarks>
        public TimeSpan RootDispersion
        {
            get => GetTimeSpanUnsigned(8);
            set => SetTimeSpanUnsigned(8, value);
        }

        /// <summary>
        /// A bit string identifying the particular reference source.
        /// </summary>
        /// <remarks>
        /// For <see cref="NtpMode.Server"/> messages when <see cref="Stratum"/> is 0 or 1, this value is a
        /// left justified and zero padded four-character ASCII string, and when <see cref="Stratum"/> is
        /// greater than 1, this value is the IPv4 address of the synchronization source.
        /// </remarks>
        public uint ReferenceIdentifier
        {
            get => (uint)GetInt32(12);
            set => SetInt32(12, (int)value);
        }

        /// <summary>
        /// The time the system clock was last set or corrected.
        /// </summary>
        /// <remarks>
        /// This generally lags the NTP time by several minutes, and should not be used as the current time.
        /// </remarks>
        public DateTime ReferenceTimestamp
        {
            get => GetDateTime(16);
            set => SetDateTime(16, value);
        }

        /// <summary>
        /// The time at the client when the request departed for the server.
        /// </summary>
        public DateTime OriginateTimestamp
        {
            get => GetDateTime(24);
            set => SetDateTime(24, value);
        }

        /// <summary>
        /// The time at the server when the request arrived from the client.
        /// </summary>
        public DateTime ReceiveTimestamp
        {
            get => GetDateTime(32);
            set => SetDateTime(32, value);
        }

        /// <summary>
        /// The time at which the request departed the client or the reply departed the server.
        /// </summary>
        /// <remarks>
        /// This property should be set for <see cref="NtpMode.Client"/> messages.
        /// </remarks>
        public DateTime TransmitTimestamp
        {
            get => GetDateTime(40);
            set => SetDateTime(40, value);
        }

        /// <summary>
        /// Creates a new <see cref="NtpPacket"/> instance.
        /// </summary>
        public NtpPacket()
        {
            Buffer = new byte[k_BufferLength];
            Reset();
        }

        /// <summary>
        /// Clears the packet to default values.
        /// </summary>
        public void Reset()
        {
            Array.Clear(Buffer, 0, Buffer.Length);

            Version = NtpConstants.CurrentVersion;
            Mode = NtpMode.Client;
        }

        void SetBits(int index, int mask, int value)
        {
            Buffer[index] = (byte)((Buffer[index] & (0xff ^ mask)) | (value & mask));
        }

        DateTime GetDateTime(int index)
        {
            var seconds = (ulong)(uint)GetInt32(index);
            var fraction = (ulong)(uint)GetInt32(index + sizeof(uint));
            var ticks = (seconds * k_TicksPerSecond) + ((fraction * k_TicksPerSecond) >> 32);
            return new DateTime(k_Epoch.Ticks + (long)ticks);
        }

        void SetDateTime(int index, DateTime value)
        {
            var ticks = (ulong)(value.Ticks - k_Epoch.Ticks);
            var seconds = (uint)(ticks / k_TicksPerSecond);
            var fraction = (uint)(((ticks % k_TicksPerSecond) << 32) / k_TicksPerSecond);
            SetInt32(index, (int)seconds);
            SetInt32(index + sizeof(uint), (int)fraction);
        }

        TimeSpan GetTimeSpanUnsigned(int index)
        {
            return TimeSpan.FromSeconds((uint)GetInt32(index) / (double)(1 << 16));
        }

        TimeSpan GetTimeSpanSigned(int index)
        {
            return TimeSpan.FromSeconds(GetInt32(index) / (double)(1 << 16));
        }

        void SetTimeSpanUnsigned(int index, TimeSpan timeSpan)
        {
            SetInt32(index, (int)(uint)(timeSpan.TotalSeconds * (1 << 16)));
        }

        void SetTimeSpanSigned(int index, TimeSpan timeSpan)
        {
            SetInt32(index, (int)(timeSpan.TotalSeconds * (1 << 16)));
        }

        int GetInt32(int index)
        {
            var value = 0;
            for (var i = 0; i < sizeof(int); i++)
            {
                value |= (Buffer[index + i] << ((sizeof(int) - 1 - i) * 8));
            }
            return value;
        }

        void SetInt32(int index, int value)
        {
            for (var i = 0; i < sizeof(int); i++)
            {
                Buffer[index + i] = (byte)(value >> ((sizeof(int) - 1 - i) * 8));
            }
        }
    }
}
