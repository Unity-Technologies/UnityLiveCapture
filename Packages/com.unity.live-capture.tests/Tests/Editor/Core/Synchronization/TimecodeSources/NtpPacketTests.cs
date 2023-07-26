using System;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.Ntp;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    class NtpPacketTests
    {
        static readonly DateTime k_Epoch = new DateTime(1900, 1, 1);

        [Test]
        public void TestVersion()
        {
            var packet = new NtpPacket();

            Assert.AreEqual(NtpConstants.CurrentVersion, packet.Version);
            packet.Version = 2;
            Assert.AreEqual(2, packet.Version);
        }

        [Test]
        public void TestMode()
        {
            var packet = new NtpPacket();

            Assert.AreEqual(NtpMode.Client, packet.Mode);
            packet.Mode = NtpMode.Server;
            Assert.AreEqual(NtpMode.Server, packet.Mode);
        }

        [Test]
        public void TestRootDelay()
        {
            var packet = new NtpPacket();
            var time = new TimeSpan(0, 0, 0, 2, 593);

            Assert.AreEqual(default(TimeSpan), packet.RootDelay);
            packet.RootDelay = time;
            Assert.AreEqual(time, packet.RootDelay);
        }

        [Test]
        public void TestRootDispersion()
        {
            var packet = new NtpPacket();
            var time = new TimeSpan(0, 0, 0, 2, 593);

            Assert.AreEqual(default(TimeSpan), packet.RootDispersion);
            packet.RootDispersion = time;
            Assert.AreEqual(time, packet.RootDispersion);
        }

        [Test]
        public void TestReferenceTimestamp()
        {
            var packet = new NtpPacket();
            var time = new DateTime(2023, 3, 23, 22, 13, 9, 137);

            Assert.AreEqual(k_Epoch, packet.ReferenceTimestamp);
            packet.ReferenceTimestamp = time;
            Assert.AreEqual(time.Ticks, packet.ReferenceTimestamp.Ticks, 0);
        }

        [Test]
        public void TestOriginateTimestamp()
        {
            var packet = new NtpPacket();
            var time = new DateTime(2023, 3, 23, 22, 13, 9, 137);

            Assert.AreEqual(k_Epoch, packet.OriginateTimestamp);
            packet.OriginateTimestamp = time;
            Assert.AreEqual(time.Ticks, packet.OriginateTimestamp.Ticks, 0);
        }

        [Test]
        public void TestReceiveTimestamp()
        {
            var packet = new NtpPacket();
            var time = new DateTime(2023, 3, 23, 22, 13, 9, 137);

            Assert.AreEqual(k_Epoch, packet.ReceiveTimestamp);
            packet.ReceiveTimestamp = time;
            Assert.AreEqual(time.Ticks, packet.ReceiveTimestamp.Ticks, 0);
        }

        [Test]
        public void TestTransmitTimestamp()
        {
            var packet = new NtpPacket();
            var time = new DateTime(2023, 3, 23, 22, 13, 9, 137);

            Assert.AreEqual(k_Epoch, packet.TransmitTimestamp);
            packet.TransmitTimestamp = time;
            Assert.AreEqual(time.Ticks, packet.TransmitTimestamp.Ticks, 0);
        }
    }
}
