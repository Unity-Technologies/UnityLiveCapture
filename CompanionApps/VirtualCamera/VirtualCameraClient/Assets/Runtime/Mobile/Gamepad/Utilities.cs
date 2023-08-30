using System;
using System.ComponentModel;
using System.Text;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Static utility functions for gamepad support.
    /// </summary>
    static class Utilities
    {
        /// <summary>
        /// Inserts spaces between lowercase and uppercase letters.
        /// MoveForward = Move Forward, TrackingAF = Tracking AF
        /// </summary>
        public static string InsertSpaceBetweenCapitals(string text)
        {
            var builder = new StringBuilder(text.Length * 2);

            for (var i = 0; i < text.Length; i++)
            {
                var current = text[i];

                if (i > 0 && i < text.Length - 1)
                {
                    var prev = text[i - 1];
                    if (Char.IsUpper(current) && !Char.IsUpper(prev))
                    {
                        builder.Append(' ');
                    }
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        static FocusMode[] s_FocusModeOrder =
        {
            FocusMode.Clear,
            FocusMode.Manual,
            FocusMode.ReticleAF,
            FocusMode.TrackingAF
        };

        /// <summary>
        /// Loops between focus modes, in the order defined by <see cref="FocusMode"/>.
        /// </summary>
        /// <param name="next">When true increments the focus mode, decrements otherwise</param>
        public static FocusMode GetNextFocusMode(FocusMode current, bool next)
        {
            var i = Array.IndexOf(s_FocusModeOrder, current);
            if (i < 0)
            {
                throw new InvalidEnumArgumentException(nameof(current), (int) current, typeof(FocusMode));
            }

            i = next ? i + 1 : i - 1;
            i = Mod(i, s_FocusModeOrder.Length);

            return (FocusMode) i;
        }

        static int Mod(int n, int m)
        {
            return ((n % m) + m) % m;
        }
    }
}
