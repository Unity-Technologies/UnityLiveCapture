using System.Text;

namespace Unity.LiveCapture.Tentacle
{
    /// <summary>
    /// An enum containing the statuses a device may have.
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// The device is working normally.
        /// </summary>
        Normal,

        /// <summary>
        /// The device has not been seen in the last 10 seconds.
        /// </summary>
        Disappeared,

        /// <summary>
        /// The device has not been seen in the last 600 seconds.
        /// </summary>
        Unavailable,
    }

    /// <summary>
    /// A class that represents a detected Tentacle timecode source.
    /// </summary>
    public class TentacleDevice : ITimecodeSource
    {
        readonly byte[] m_IdentifierArray = new byte[DeviceState.k_IdentifierLengthMax];
        readonly int m_IdentifierLength;
        readonly string m_Identifier;

        readonly byte[] m_NameArray = new byte[DeviceState.k_NameLengthMax];
        int m_NameLength;
        string m_Name;

        /// <summary>
        /// A unique identifier representing the device.
        /// </summary>
        public string Id => m_Identifier;

        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name => m_Name;

        /// <summary>
        /// The current status of the device.
        /// </summary>
        public DeviceStatus Status { get; private set; }

        /// <summary>
        /// The battery level remaining in the device in the range [0,1].
        /// </summary>
        public float Battery { get; private set; }

        /// <summary>
        /// Is the device's battery currently charging.
        /// </summary>
        public bool IsCharging { get; private set; }

        /// <inheritdoc/>
        public string FriendlyName => $"Tentacle Sync ({m_Name})";

        /// <inheritdoc/>
        public FrameRate FrameRate { get; private set; }

        /// <inheritdoc/>
        public FrameTimeWithRate? CurrentTime { get; private set; }

        internal unsafe TentacleDevice(ref DeviceState state)
        {
            fixed(byte* ptr = state.identifier)
            {
                UpdateString(m_IdentifierArray, out m_IdentifierLength, out m_Identifier, ptr, state.identifierLength);
            }

            Update(ref state);
        }

        internal unsafe bool IsDevice(ref DeviceState state)
        {
            fixed(byte* ptr = state.identifier)
            {
                return AreEqual(m_IdentifierArray, m_IdentifierLength, ptr, state.identifierLength);
            }
        }

        internal unsafe void Update(ref DeviceState state)
        {
            fixed(byte* ptr = state.name)
            {
                if (!AreEqual(m_NameArray, m_NameLength, ptr, state.nameLength))
                {
                    UpdateString(m_NameArray, out m_NameLength, out m_Name, ptr, state.nameLength);
                }
            }

            if (state.isUnavailable)
            {
                Status = DeviceStatus.Unavailable;
            }
            else if (state.isDisappeared)
            {
                Status = DeviceStatus.Disappeared;
            }
            else
            {
                Status = DeviceStatus.Normal;
            }

            Battery = state.battery / 0.01f;
            IsCharging = state.isCharging;
            FrameRate = GetFrameRate(ref state);
            CurrentTime = new FrameTimeWithRate(FrameRate, FrameTime.FromSeconds(FrameRate, state.seconds));
        }

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>A string that represents the current instance.</returns>
        public override string ToString()
        {
            return $"ID={m_Identifier}, Name={m_Name}, Status={Status}, Rate={FrameRate}, TC={CurrentTime}";
        }

        static FrameRate GetFrameRate(ref DeviceState state)
        {
            if (!state.isNtsc)
            {
                return state.frameRate switch
                {
                    24 => StandardFrameRate.FPS_24_00,
                    25 => StandardFrameRate.FPS_25_00,
                    30 => StandardFrameRate.FPS_30_00,
                    48 => StandardFrameRate.FPS_48_00,
                    60 => StandardFrameRate.FPS_60_00,
                    _ => default,
                };
            }
            if (!state.isDropFrame)
            {
                return state.frameRate switch
                {
                    24 => StandardFrameRate.FPS_23_976,
                    30 => StandardFrameRate.FPS_29_97,
                    60 => StandardFrameRate.FPS_59_94,
                    _ => default,
                };
            }
            return state.frameRate switch
            {
                24 => StandardFrameRate.FPS_23_976_DF,
                30 => StandardFrameRate.FPS_29_97_DF,
                60 => StandardFrameRate.FPS_59_94_DF,
                _ => default,
            };
        }

        static unsafe bool AreEqual(byte[] a, int aLen, byte* b, int bLen)
        {
            if (aLen != bLen)
            {
                return false;
            }

            for (var i = 0; i < aLen; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        static unsafe void UpdateString(byte[] a, out int aLen, out string aStr, byte* b, int bLen)
        {
            for (var i = 0; i < bLen; i++)
            {
                a[i] = b[i];
            }

            aLen = bLen;
            aStr = Encoding.UTF8.GetString(b, bLen);
        }
    }
}
