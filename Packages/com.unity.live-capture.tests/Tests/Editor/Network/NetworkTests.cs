using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Unity.LiveCapture.Networking;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    public class NetworkTests
    {
        const int k_Port1 = 8927;
        const int k_Port2 = 15042;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // ensure there is enough time for the system to close the sockets between tests
            yield return TestUtils.WaitForPlayerLoopUpdates(50);
        }

        [Test]
        public void TestServerStartStop()
        {
            var server = new NetworkServer();

            try
            {
                var startCount = 0;
                var stopCount = 0;
                server.Started += () => startCount++;
                server.Stopped += () => stopCount++;

                // test the server starts off
                Assert.IsFalse(server.IsRunning);
                Assert.AreEqual(-1, server.Port);

                // test starting the server
                server.StartServer(k_Port1);

                Assert.AreEqual(1, startCount);
                Assert.AreEqual(0, stopCount);
                Assert.IsTrue(server.IsRunning);
                Assert.AreEqual(k_Port1, server.Port);

                // test the server does not restart if the port is the same
                server.StartServer(k_Port1);

                Assert.AreEqual(1, startCount);
                Assert.AreEqual(0, stopCount);
                Assert.IsTrue(server.IsRunning);
                Assert.AreEqual(k_Port1, server.Port);

                // test the server restarts if a new port is given
                server.StartServer(k_Port2);

                Assert.AreEqual(2, startCount);
                Assert.AreEqual(1, stopCount);
                Assert.IsTrue(server.IsRunning);
                Assert.AreEqual(k_Port2, server.Port);

                // test the server stops correctly
                server.Stop();

                Assert.AreEqual(2, startCount);
                Assert.AreEqual(2, stopCount);
                Assert.IsFalse(server.IsRunning);
                Assert.AreEqual(-1, server.Port);
            }
            finally
            {
                server.Stop();
            }
        }

        // This test fails on Linux, in Bokken, but works manually. Disabling for now as Linux is a lower priority platform for now
        [UnityTest]
        [UnityPlatform(include = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor})]
        public IEnumerator TestClientStartStop()
        {
            var client = new NetworkClient();

            try
            {
                var startCount = 0;
                var stopCount = 0;
                client.Started += () => startCount++;
                client.Stopped += () => stopCount++;

                // test the client starts off
                Assert.IsFalse(client.IsRunning);
                Assert.IsFalse(client.IsConnecting);
                Assert.AreEqual(null, client.ServerEndPoint);

                // test starting the client
                var address1 = IPAddress.Parse("127.0.0.1");
                client.ConnectToServer(address1.ToString(), k_Port1, 0);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                Assert.AreEqual(1, startCount);
                Assert.AreEqual(0, stopCount);
                Assert.IsTrue(client.IsRunning);
                Assert.IsTrue(client.IsConnecting);
                Assert.AreEqual(new IPEndPoint(address1, k_Port1), client.ServerEndPoint);

                // test the client does not restart if the server end point is the same even if the local port is different
                client.ConnectToServer(address1.ToString(), k_Port1, 8000);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                Assert.AreEqual(1, startCount);
                Assert.AreEqual(0, stopCount);
                Assert.IsTrue(client.IsRunning);
                Assert.IsTrue(client.IsConnecting);
                Assert.AreEqual(new IPEndPoint(address1, k_Port1), client.ServerEndPoint);

                // test the server restarts if a new server port is given
                client.ConnectToServer(address1.ToString(), k_Port2, 0);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                Assert.AreEqual(2, startCount);
                Assert.AreEqual(1, stopCount);
                Assert.IsTrue(client.IsRunning);
                Assert.IsTrue(client.IsConnecting);
                Assert.AreEqual(new IPEndPoint(address1, k_Port2), client.ServerEndPoint);

                // test the server restarts if a new server address is given
                var address2 = IPAddress.Parse("192.128.0.1");
                client.ConnectToServer(address2.ToString(), k_Port2, 0);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                Assert.AreEqual(3, startCount);
                Assert.AreEqual(2, stopCount);
                Assert.IsTrue(client.IsRunning);
                Assert.IsTrue(client.IsConnecting);
                Assert.AreEqual(new IPEndPoint(address2, k_Port2), client.ServerEndPoint);

                // test the server stops correctly
                client.Stop();

                Assert.AreEqual(3, startCount);
                Assert.AreEqual(3, stopCount);
                Assert.IsFalse(client.IsRunning);
                Assert.IsFalse(client.IsConnecting);
                Assert.AreEqual(null, client.ServerEndPoint);
            }
            finally
            {
                client.Stop();
            }
        }

        /// <summary>
        /// This test ensures that if a client attempts to connect to the server before
        /// the server is started, the connection succeeds once the server is started.
        /// </summary>
        [UnityTest]
        public IEnumerator TestClientStartFirst()
        {
            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                var connectCount = 0;
                server.RemoteConnected += (remote) => connectCount++;

                client.ConnectToServer("127.0.0.1", k_Port1);

                // wait until a few connection attempts should have timed out
                yield return TestUtils.WaitForPlayerLoopUpdates(1500);

                server.StartServer(k_Port1);
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                Assert.AreEqual(1, server.RemoteCount);
                Assert.AreEqual(1, client.RemoteCount);
                Assert.AreEqual(1, connectCount);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        /// <summary>
        /// This test ensures that if a client reconnects to a server before the
        /// server is aware the client has disconnected (non-gracefully), the server
        /// properly cleans up the old connection when it accepts the new connection.
        /// </summary>
        [UnityTest]
        [Ignore("Disabled for Instability https://jira.unity3d.com/browse/LC-1650")]
        public IEnumerator TestReconnect()
        {
            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                var connectCount = 0;
                server.RemoteConnected += (remote) => connectCount++;

                var disconnectCount = 0;
                var lastDisconnectStatus = default(DisconnectStatus);
                server.RemoteDisconnected += (remote, status) =>
                {
                    disconnectCount++;
                    lastDisconnectStatus = status;
                };

                // connect the client to the server
                server.StartServer(k_Port1);
                client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                server.Update();
                client.Update();

                Assert.AreEqual(1, server.RemoteCount);
                Assert.AreEqual(1, client.RemoteCount);
                Assert.AreEqual(1, connectCount);
                Assert.AreEqual(0, disconnectCount);

                // disconnect the client without notifying the server
                client.Stop(false);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                server.Update();
                client.Update();

                Assert.AreEqual(0, client.RemoteCount);

                // reconnect the client with the server
                client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(10);

                server.Update();
                client.Update();

                Assert.AreEqual(1, server.RemoteCount);
                Assert.AreEqual(1, client.RemoteCount);
                Assert.AreEqual(2, connectCount);
                Assert.AreEqual(1, disconnectCount, "The previous connection was not terminated when the client joined again!");
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        // Tests that the client can reconnect to a server that gracefully restarted
        [UnityTest]
        public IEnumerator TestServerStartStopReconnect()
        {
            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                var connectCount = 0;
                client.RemoteConnected += (remote) => connectCount++;

                var disconnectCount = 0;
                var lastDisconnectStatus = default(DisconnectStatus);
                client.RemoteDisconnected += (remote, status) =>
                {
                    disconnectCount++;
                    lastDisconnectStatus = status;
                };

                // connect the client to the server
                server.StartServer(k_Port1);
                client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(50);

                server.Update();
                client.Update();

                Assert.AreEqual(1, server.RemoteCount);
                Assert.AreEqual(1, client.RemoteCount);
                Assert.AreEqual(1, connectCount);
                Assert.AreEqual(0, disconnectCount);

                // disconnect the server, notifying the client
                server.Stop();

                yield return TestUtils.WaitForPlayerLoopUpdates(50);

                server.Update();
                client.Update();

                Assert.AreEqual(0, client.RemoteCount);
                Assert.AreEqual(0, server.RemoteCount);
                Assert.AreEqual(1, connectCount);
                Assert.AreEqual(1, disconnectCount);

                // restart the server and reconnect the client with the server
                server.StartServer(k_Port1);
                client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(1000);

                server.Update();
                client.Update();

                Assert.AreEqual(1, server.RemoteCount);
                Assert.AreEqual(1, client.RemoteCount);
                Assert.AreEqual(2, connectCount);
                Assert.AreEqual(1, disconnectCount);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        [UnityTest]
        [Ignore("Disabled for Instability https://jira.unity3d.com/browse/LC-1650")]
        public IEnumerator TestMessageHandlerRegistration()
        {
            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                // track client connecting and disconnecting from the server
                var clientRemote = default(Remote);
                server.RemoteConnected += (remote) =>
                {
                    Assert.IsTrue(remote.ID == client.ID);
                    clientRemote = remote;
                };

                server.RemoteDisconnected += (remote, status) =>
                {
                    Assert.IsTrue(remote.ID == client.ID);
                    Assert.AreEqual(DisconnectStatus.Graceful, status);
                };

                // track the server connecting and disconnecting from client
                var serverRemote = default(Remote);
                client.RemoteConnected += (remote) =>
                {
                    Assert.IsTrue(remote.ID == server.ID);
                    serverRemote = remote;
                };

                client.RemoteDisconnected += (remote, status) =>
                {
                    Assert.IsTrue(remote.ID == server.ID);
                    Assert.AreEqual(DisconnectStatus.Graceful, status);
                };

                // connect and check if that only a valid remote can be registered
                server.StartServer(k_Port1);
                client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(50);

                server.Update();
                client.Update();

                Assert.AreEqual(1, server.RemoteCount);
                Assert.AreEqual(1, client.RemoteCount);

                var handler1 = (Action<Message>)((message) =>
                {
                    message.Dispose();
                });
                var handler2 = (Action<Message>)((message) =>
                {
                    message.Dispose();
                });

                Assert.Catch<ArgumentNullException>(() => server.RegisterMessageHandler(null, handler1));
                Assert.Catch<ArgumentNullException>(() => server.RegisterMessageHandler(clientRemote, null));
                Assert.Catch<ArgumentException>(() => server.RegisterMessageHandler(Remote.All, handler1));
                Assert.Catch<ArgumentException>(() => server.RegisterMessageHandler(serverRemote, handler1));

                Assert.IsTrue(server.RegisterMessageHandler(clientRemote, handler1));
                Assert.IsTrue(server.RegisterMessageHandler(clientRemote, handler1));
                Assert.IsFalse(server.RegisterMessageHandler(clientRemote, handler2));

                // test deregistering and registering a new handler
                Assert.Catch<ArgumentNullException>(() => server.DeregisterMessageHandler(null));
                Assert.Catch<ArgumentException>(() => server.DeregisterMessageHandler(Remote.All));
                Assert.Catch<ArgumentException>(() => server.DeregisterMessageHandler(serverRemote));

                Assert.IsTrue(server.DeregisterMessageHandler(clientRemote));
                Assert.IsTrue(server.DeregisterMessageHandler(clientRemote));

                Assert.IsTrue(server.RegisterMessageHandler(clientRemote, handler2));
                Assert.IsTrue(server.RegisterMessageHandler(clientRemote, handler2));
                Assert.IsFalse(server.RegisterMessageHandler(clientRemote, handler1));

                // test that (de)registering fails after the connection is stopped
                client.Stop();
                server.Stop();

                Assert.Catch<ArgumentException>(() => server.DeregisterMessageHandler(clientRemote));
                Assert.Catch<ArgumentException>(() => server.DeregisterMessageHandler(clientRemote));
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        [UnityTest]
        public IEnumerator TestDisconnect()
        {
            var server = new NetworkServer();
            var clients = new List<NetworkClient>();

            try
            {
                for (var i = 0; i < 10; i++)
                    clients.Add(new NetworkClient());

                // connect the clients to the server
                server.StartServer(k_Port1);
                foreach (var client in clients)
                    client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(50);

                server.Update();
                foreach (var client in clients)
                    client.Update();

                // check that we have the expected connections
                Assert.AreEqual(clients.Count, server.RemoteCount, "Not every client is connected to the server!");
                foreach (var client in clients)
                    Assert.AreEqual(1, client.RemoteCount, "Not every client is connected to the server!");

                // disconnect every other client
                for (var i = 0; i < clients.Count; i += 2)
                    server.Disconnect(server.Remotes.First(r => r.ID == clients[i].ID));

                yield return TestUtils.WaitForPlayerLoopUpdates(50);

                server.Update();
                foreach (var client in clients)
                    client.Update();

                // check that only the expected clients are connected
                Assert.AreEqual(clients.Count / 2, server.RemoteCount, "Only half of the clients should remain connected!");

                for (var i = 0; i < 10; i++)
                {
                    if (i % 2 == 0)
                        Assert.AreEqual(0, clients[i].RemoteCount, "Client is unexpectedly connected to server!");
                    else
                        Assert.AreEqual(1, clients[i].RemoteCount, "Client is unexpectedly disconnected from server!");
                }
            }
            finally
            {
                server.Stop();
                foreach (var client in clients)
                    client.Stop();
            }
        }

        [UnityTest]
        public IEnumerator TestSendManyClients()
        {
            var server = new NetworkServer();
            var clients = new List<NetworkClient>();

            try
            {
                for (var i = 0; i < 10; i++)
                    clients.Add(new NetworkClient());

                // ensure connections to the server are from the client
                server.RemoteConnected += (remote) =>
                {
                    Assert.IsTrue(clients.Any(client => remote.ID == client.ID));
                };
                server.RemoteDisconnected += (remote, status) =>
                {
                    Assert.IsTrue(clients.Any(client => remote.ID == client.ID));
                    Assert.AreEqual(DisconnectStatus.Graceful, status);
                };

                // ensure connections to each client are from the server
                foreach (var client in clients)
                {
                    client.RemoteConnected += (remote) =>
                    {
                        Assert.IsTrue(remote.ID == server.ID);
                    };
                    client.RemoteDisconnected += (remote, status) =>
                    {
                        Assert.IsTrue(remote.ID == server.ID);
                        Assert.AreEqual(DisconnectStatus.Graceful, status);
                    };
                }

                // connect the clients to the server
                server.StartServer(k_Port1);
                foreach (var client in clients)
                    client.ConnectToServer("127.0.0.1", k_Port1);

                yield return TestUtils.WaitForPlayerLoopUpdates(50);

                server.Update();
                foreach (var client in clients)
                    client.Update();

                // check that we have the expected connections
                Assert.AreEqual(clients.Count, server.RemoteCount, "Not every client is connected to the server!");
                foreach (var client in clients)
                    Assert.AreEqual(1, client.RemoteCount, "Not every client is connected to the server!");

                // Send a few messages of random lengths over all available channels between the server and clients,
                // checking that the received messages match what was sent.
                var random = new System.Random();

                var serverReliableMessages = new Dictionary<Remote, byte[]>();
                var serverUnreliableMessages = new Dictionary<Remote, byte[]>();

                var clientReliableMessages = new Dictionary<Remote, byte[]>();
                var clientUnreliableMessages = new Dictionary<Remote, byte[]>();

                var sentMessageCount = 0;
                var receivedReliableMessageCount = 0;
                var receivedUnreliableMessageCount = 0;

                foreach (var remoteClient in server.Remotes)
                    server.RegisterMessageHandler(remoteClient, (message) =>
                    {
                        var originalData = default(byte[]);

                        if (message.ChannelType == ChannelType.ReliableOrdered)
                        {
                            receivedReliableMessageCount++;
                            originalData = clientReliableMessages[remoteClient];
                        }
                        else
                        {
                            receivedUnreliableMessageCount++;
                            originalData = clientUnreliableMessages[remoteClient];
                        }

                        var receivedData = new byte[message.Data.Length];
                        message.Data.Read(receivedData, 0, receivedData.Length);

                        Assert.IsTrue(originalData.SequenceEqual(receivedData), $"A message the server received on the {message.ChannelType} channel does not match what the client sent!");
                    });

                foreach (var client in clients)
                    client.RegisterMessageHandler(client.Remotes[0], (message) =>
                    {
                        var originalData = default(byte[]);
                        var clientRemote = server.Remotes.First(r => r.ID == client.ID);

                        if (message.ChannelType == ChannelType.ReliableOrdered)
                        {
                            receivedReliableMessageCount++;
                            originalData = serverReliableMessages[clientRemote];
                        }
                        else
                        {
                            receivedUnreliableMessageCount++;
                            originalData = serverUnreliableMessages[clientRemote];
                        }

                        var receivedData = new byte[message.Data.Length];
                        message.Data.Read(receivedData, 0, receivedData.Length);

                        Assert.IsTrue(originalData.SequenceEqual(receivedData), $"A message a client received on the {message.ChannelType} channel does not match what the server sent!");
                    });

                for (var i = 0; i < 3; i++)
                {
                    serverReliableMessages.Clear();
                    serverUnreliableMessages.Clear();

                    foreach (var clientRemote in server.Remotes)
                    {
                        serverReliableMessages.Add(clientRemote, SendMessage(server, clientRemote, ChannelType.ReliableOrdered, random));
                        serverUnreliableMessages.Add(clientRemote, SendMessage(server, clientRemote, ChannelType.UnreliableUnordered, random));
                        sentMessageCount += 2;
                    }

                    clientReliableMessages.Clear();
                    clientUnreliableMessages.Clear();

                    foreach (var client in clients)
                    {
                        var serverRemote = client.Remotes[0];
                        var clientRemote = server.Remotes.First(r => r.ID == client.ID);

                        clientReliableMessages.Add(clientRemote, SendMessage(client, serverRemote, ChannelType.ReliableOrdered, random));
                        clientUnreliableMessages.Add(clientRemote, SendMessage(client, serverRemote, ChannelType.UnreliableUnordered, random));
                        sentMessageCount += 2;
                    }

                    // wait for the messages to be sent, with update triggering the handlers that check if the received messages are correct
                    yield return TestUtils.WaitForPlayerLoopUpdates(50);

                    server.Update();
                    foreach (var client in clients)
                        client.Update();
                }

                Assert.AreEqual(sentMessageCount, receivedReliableMessageCount + receivedUnreliableMessageCount, "Fewer messages were received than were sent!");
            }
            finally
            {
                server.Stop();
                foreach (var client in clients)
                    client.Stop();
            }
        }

        static byte[] SendMessage(NetworkBase network, Remote remote, ChannelType channelType, System.Random random)
        {
            var messageLength = 0;

            switch (channelType)
            {
                case ChannelType.ReliableOrdered:
                    // TCP supports any message size, so test up to MB in size
                    messageLength = random.Next(1, 2 * 1000 * 1000);
                    break;
                case ChannelType.UnreliableUnordered:
                    // the max UDP message size with room for the header
                    messageLength = random.Next(0, ushort.MaxValue - 100);
                    break;
            }

            var buffer = new byte[messageLength];
            random.NextBytes(buffer);

            var message = Message.Get(remote, channelType, buffer.Length);
            message.Data.Write(buffer, 0, buffer.Length);
            network.SendMessage(message);

            return buffer;
        }
    }
}
