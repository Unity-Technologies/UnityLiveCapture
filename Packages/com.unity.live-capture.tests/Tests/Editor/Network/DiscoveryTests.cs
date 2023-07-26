using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Discovery;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    public class DiscoveryTests
    {
        // discovery name strings support any UCS-2 string less than 32 characters
        const string k_ValidProduct = "TestProduct 09 -@#$% ばか (バカ).";
        const string k_ValidInstance = "TestInstance 09 -@#$% бвгдж.";
        const string k_InvalidName = "This name is longer than 32 characters!";

        static readonly ServerData k_ServerData = new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, new Version(1, 1, 1, 1));

        static readonly IPEndPoint[] s_EndPoints = NetworkUtilities.GetIPAddresses(false)
            .Select(a => new IPEndPoint(a, 400))
            .ToArray();

        [Test]
        public void TestServerConfiguration()
        {
            const int validVersion = ushort.MaxValue / 2;
            const int invalidVersion = (int)ushort.MaxValue * 2;

            // check that the arguments cannot be null
            Assert.Catch<ArgumentNullException>(() => new ServerData(null, k_ValidInstance, Guid.Empty, new Version()));
            Assert.Catch<ArgumentNullException>(() => new ServerData(k_ValidProduct, null, Guid.Empty, new Version()));
            Assert.Catch<ArgumentNullException>(() => new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, null));

            // check that the names must not exceed a maximum number of characters
            Assert.Catch<ArgumentException>(() => new ServerData(k_InvalidName, k_ValidInstance, Guid.Empty, new Version()));
            Assert.Catch<ArgumentException>(() => new ServerData(k_ValidProduct, k_InvalidName, Guid.Empty, new Version()));

            // check that the version must be valid
            Assert.Catch<ArgumentException>(() => new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, new Version(invalidVersion, validVersion, validVersion, validVersion)));
            Assert.Catch<ArgumentException>(() => new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, new Version(validVersion, invalidVersion, validVersion, validVersion)));
            Assert.Catch<ArgumentException>(() => new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, new Version(validVersion, validVersion, invalidVersion, validVersion)));
            Assert.Catch<ArgumentException>(() => new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, new Version(validVersion, validVersion, validVersion, invalidVersion)));

            Assert.DoesNotThrow(() => new ServerData(k_ValidProduct, k_ValidInstance, Guid.Empty, new Version(validVersion, validVersion, validVersion, validVersion)));
        }

        [Test]
        public void TestServerStartStop()
        {
            var server = new DiscoveryServer();

            try
            {
                Assert.Catch<ArgumentNullException>(() => server.Start(k_ServerData, null));
                Assert.Catch<ArgumentException>(() => server.Start(k_ServerData, new IPEndPoint[300]));

                // check the server starts properly
                Assert.IsTrue(server.Start(k_ServerData, s_EndPoints));
                Assert.IsTrue(server.IsRunning);

                // should return true even if already running
                Assert.IsTrue(server.Start(k_ServerData, s_EndPoints));

                // check shutting down the server
                Assert.DoesNotThrow(() => server.Stop());
                Assert.IsFalse(server.IsRunning);
            }
            finally
            {
                server.Stop();
            }
        }

        [Test]
        public void TestMultipleServers()
        {
            var server1 = new DiscoveryServer();
            var server2 = new DiscoveryServer();

            try
            {
                // multiple servers sharing the same port is supported
                Assert.IsTrue(server1.Start(k_ServerData, s_EndPoints));
                Assert.IsTrue(server2.Start(k_ServerData, s_EndPoints));
            }
            finally
            {
                server1.Stop();
                server2.Stop();
            }
        }

        [Test]
        public void TestClientStartStop()
        {
            var client = new DiscoveryClient();

            try
            {
                Assert.Catch<ArgumentNullException>(() => client.Start(null));
                Assert.Catch<ArgumentException>(() => client.Start(k_InvalidName));

                // check the client starts properly
                Assert.IsTrue(client.Start(k_ValidProduct));
                Assert.IsTrue(client.IsRunning);

                // should return true even if already running
                Assert.IsTrue(client.Start(k_ValidProduct));

                // check shutting down the client
                Assert.DoesNotThrow(() => client.Stop());
                Assert.IsFalse(client.IsRunning);
            }
            finally
            {
                client.Stop();
            }
        }

        [Test]
        public void TestMultipleClients()
        {
            var client1 = new DiscoveryClient();
            var client2 = new DiscoveryClient();

            try
            {
                // multiple clients sharing the same port is supported
                Assert.IsTrue(client1.Start(k_ValidProduct));
                Assert.IsTrue(client2.Start(k_ValidProduct));
            }
            finally
            {
                client1.Stop();
                client2.Stop();
            }
        }

        /// <summary>
        /// Check that the server is not discovered by local clients by default.
        /// </summary>
        [UnityTest]
        public IEnumerator TestClientNonLocal()
        {
            var server = new DiscoveryServer();
            var client = new DiscoveryClient();

            try
            {
                var infos = new List<DiscoveryInfo>();

                client.ServerFound += (i) => infos.Add(i);
                client.ServerLost += (i) => infos.Remove(i);

                client.Start(k_ValidProduct);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server.Start(k_ServerData, s_EndPoints);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server.Update();
                client.Update();

                Assert.AreEqual(0, infos.Count);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        /// <summary>
        /// Check that the server is discovered by the client immediately after
        /// the server starts.
        /// </summary>
        [UnityTest]
        public IEnumerator TestClientStartBefore()
        {
            var server = new DiscoveryServer();
            var client = new DiscoveryClient();

            try
            {
                var infos = new List<DiscoveryInfo>();

                client.ServerFound += (i) => infos.Add(i);
                client.ServerLost += (i) => infos.Remove(i);

                client.Start(k_ValidProduct, true);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server.Start(k_ServerData, s_EndPoints);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server.Update();
                client.Update();

                Assert.AreEqual(1, infos.Count);

                CompareDiscoveryInfo(k_ServerData, s_EndPoints, infos.First());
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        /// <summary>
        /// Check that the server properly responds to the client discovery query and is
        /// discovered immediately after the clients starts.
        /// </summary>
        [UnityTest]
        public IEnumerator TestClientStartsAfter()
        {
            var server = new DiscoveryServer();
            var client = new DiscoveryClient();

            try
            {
                var infos = new List<DiscoveryInfo>();

                client.ServerFound += (i) => infos.Add(i);
                client.ServerLost += (i) => infos.Remove(i);

                server.Start(k_ServerData, s_EndPoints);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                client.Start(k_ValidProduct, true);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server.Update();
                client.Update();

                Assert.AreEqual(1, infos.Count);

                CompareDiscoveryInfo(k_ServerData, s_EndPoints, infos.First());
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        /// <summary>
        /// Test that the client detects the server stop immediately.
        /// </summary>
        [UnityTest]
        public IEnumerator TestServerServerLoss()
        {
            var server = new DiscoveryServer();
            var client = new DiscoveryClient();

            try
            {
                var infos = new List<DiscoveryInfo>();

                client.ServerFound += (i) => infos.Add(i);
                client.ServerLost += (i) => infos.Remove(i);

                server.Start(k_ServerData, s_EndPoints);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                client.Start(k_ValidProduct, true);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server.Update();
                client.Update();

                Assert.AreEqual(1, infos.Count);

                server.Stop();

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                client.Update();

                Assert.AreEqual(0, infos.Count);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        /// <summary>
        /// Check that the client can detect multiple servers.
        /// </summary>
        [UnityTest]
        public IEnumerator TestDiscoveryTwoServers()
        {
            var server1 = new DiscoveryServer();
            var server2 = new DiscoveryServer();
            var client = new DiscoveryClient();

            try
            {
                var infos = new List<DiscoveryInfo>();

                client.ServerFound += (i) => infos.Add(i);
                client.ServerLost += (i) => infos.Remove(i);

                var data1 = new ServerData(k_ValidProduct, k_ValidInstance, Guid.NewGuid(), new Version());
                var data2 = new ServerData(k_ValidProduct, k_ValidInstance, Guid.NewGuid(), new Version());

                client.Start(k_ValidProduct, true);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server1.Start(data1, s_EndPoints);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server2.Start(data2, s_EndPoints);

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server1.Update();
                server2.Update();
                client.Update();

                Assert.AreEqual(2, infos.Count);

                CompareDiscoveryInfo(data1, s_EndPoints, infos.First());
                CompareDiscoveryInfo(data2, s_EndPoints, infos.Last());

                server1.Stop();

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                server2.Update();
                client.Update();

                Assert.AreEqual(1, infos.Count);

                server2.Stop();

                yield return TestUtils.WaitForPlayerLoopUpdates(250);

                client.Update();

                Assert.AreEqual(0, infos.Count);
            }
            finally
            {
                server1.Stop();
                server2.Stop();
                client.Stop();
            }
        }

        static void CompareDiscoveryInfo(ServerData expectedServer, IPEndPoint[] expectedEndPoints, DiscoveryInfo actual)
        {
            Assert.AreEqual(expectedServer.ProductName, actual.ServerInfo.ProductName);
            Assert.AreEqual(expectedServer.InstanceName, actual.ServerInfo.InstanceName);
            Assert.AreEqual(expectedServer.ID, actual.ServerInfo.ID);
            Assert.AreEqual(expectedServer.GetVersion(), actual.ServerInfo.GetVersion());

            Assert.AreEqual(expectedEndPoints.Length, actual.EndPoints.Length);

            for (var i = 0; i < s_EndPoints.Length; i++)
            {
                Assert.AreEqual(expectedEndPoints[i], actual.EndPoints[i]);
            }
        }
    }
}
