using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.Ltc;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    class LtcDecoderTests
    {
        static IEnumerable TestForwardSignalData
        {
            get
            {
                yield return new TestCaseData(
                    "LTC_21_58_12_00__44kHz_1mins_24",
                    StandardFrameRate.FPS_24_00,
                    21, 58, 12, 0,
                    false,
                    (24 * 60) - 1
                );
                yield return new TestCaseData(
                    "LTC_21_58_12_00__44kHz_1mins_24_reversed",
                    StandardFrameRate.FPS_24_00,
                    21, 59, 11, 23,
                    true,
                    (24 * 60) - 1
                );
                yield return new TestCaseData(
                    "LTC_21_58_12_00__44kHz_1mins_23976",
                    StandardFrameRate.FPS_23_976,
                    21, 58, 12, 0,
                    false,
                    (24 * 60) - 1
                );
                yield return new TestCaseData(
                    "LTC_21_58_12_00__44kHz_1mins_30",
                    StandardFrameRate.FPS_30_00,
                    21, 58, 12, 0,
                    false,
                    (30 * 60) - 1
                );
                yield return new TestCaseData(
                    "LTC_21_58_12_00__48kHz_1mins_30",
                    StandardFrameRate.FPS_30_00,
                    21, 58, 12, 0,
                    false,
                    (30 * 60) - 1
                );
                yield return new TestCaseData(
                    "LTC_21_58_12_00__48kHz_1mins_2997_df",
                    StandardFrameRate.FPS_29_97_DF,
                    21, 58, 12, 0,
                    false,
                    (30 * 60) - 1
                );
                yield return new TestCaseData(
                    "LTC_21_58_12_00__48kHz_1mins_2997_ndf",
                    StandardFrameRate.FPS_29_97,
                    21, 58, 12, 0,
                    false,
                    (30 * 60)
                );
                yield return new TestCaseData(
                    "Tentacle_17_30_45_16__44kHz_128frames_2997_df",
                    StandardFrameRate.FPS_29_97_DF,
                    17, 29, 42, 16,
                    false,
                    128
                );
            }
        }

        [Test, TestCaseSource(nameof(TestForwardSignalData))]
        public void TestLtcDecode(
            string audioClip,
            StandardFrameRate frameRate,
            int hour, int minute, int second, int frame,
            bool reversed,
            int expectedTimecodes
        )
        {
            // load the audio
            var data = LoadClip(audioClip, out var sampleRate);

            // prepare the decoder
            var decoder = new LtcDecoder(sampleRate, frameRate.ToValue().AsFloat());

            // test if the decoder detects all timecodes
            var count = 0;
            var timecode = Timecode.FromHMSF(frameRate, hour, minute, second, frame);

            decoder.FrameDecoded += (frame) =>
            {
                count++;

                var decodedTC = Timecode.FromHMSF(frameRate, frame.hour, frame.minute, frame.second, frame.frame);
                Assert.AreEqual(timecode, decodedTC);

                // compute the next timecode expected in the sequence
                var frameTime = timecode.ToFrameTime(frameRate);
                timecode = Timecode.FromFrameTime(frameRate, reversed ? --frameTime : ++frameTime);
            };

            decoder.Decode(data, 0, data.Length);

            Assert.AreEqual(expectedTimecodes, count, "The decoder did not detect all expected timecodes!");
        }

        static float[] LoadClip(string name, out int sampleRate)
        {
            var clip = Resources.Load<AudioClip>("TimecodeSources/Ltc/" + name);
            sampleRate = clip.frequency;

            var data = new float[clip.samples];
            clip.GetData(data, 0);
            return data;
        }
    }
}
