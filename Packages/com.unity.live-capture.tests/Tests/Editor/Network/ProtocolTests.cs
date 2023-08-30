using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Protocols;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    public class ProtocolTests
    {
        const int k_Port = 8927;

        [Test]
        public void CreateProtocol()
        {
            Assert.Catch<ArgumentException>(() => new Protocol(null, new Version()));
            Assert.Catch<ArgumentException>(() => new Protocol(string.Empty, new Version()));
            Assert.Catch<ArgumentException>(() => new Protocol("  ", new Version()));
            Assert.Catch<ArgumentNullException>(() => new Protocol("TestProtocol", null));

            var name = "TestProtocol";
            var version = new Version(2, 5, 3, 8);
            var protocol = new Protocol(name, version);

            Assert.IsFalse(protocol.IsReadOnly);
            Assert.AreEqual(version, protocol.Version);
            Assert.AreEqual(name, protocol.Name);
        }

        [Test]
        public void AddMessage()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var message = new EventSender("TestMessage");

            Assert.AreEqual(0, protocol.Count());
            Assert.AreEqual(null, message.Protocol);

            protocol.Add(message);

            Assert.AreEqual(1, protocol.Count());
            Assert.AreEqual(message, protocol.FirstOrDefault());
            Assert.AreEqual(protocol, message.Protocol);
        }

        [Test]
        public void AddMessageMultiple()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var message0 = new EventSender("TestMessage0");
            var message1 = new EventSender("TestMessage1");

            Assert.AreEqual(0, protocol.Count());

            protocol.Add(message0);
            protocol.Add(message1);

            Assert.AreEqual(2, protocol.Count());
        }

        [Test]
        public void AddMessageNull()
        {
            var protocol = new Protocol("TestProtocol", new Version());

            Assert.Catch<ArgumentNullException>(() => protocol.Add<MessageBase>(null));
        }

        [Test]
        public void AddMessageDuplicateID()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var message0 = new EventSender("TestMessage");
            var message1 = new EventReceiver("TestMessage");

            protocol.Add(message0);

            Assert.Catch<ArgumentException>(() => protocol.Add(message1));
        }

        [Test]
        public void AddMessageAlreadyAdded()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var message = new EventSender("TestMessage");

            protocol.Add(message);

            Assert.Catch<ArgumentException>(() => protocol.Add(message));
        }

        [Test]
        public void AddMessageFromAnotherProtocol()
        {
            var protocol0 = new Protocol("TestProtocol0", new Version());
            var protocol1 = new Protocol("TestProtocol1", new Version());
            var message = new EventSender("TestMessage");

            protocol0.Add(message);

            Assert.Catch<ArgumentException>(() => protocol1.Add(message));
        }

        [Test]
        public void AddMessageMax()
        {
            var protocol = new Protocol("TestProtocol", new Version());

            for (var i = 0; i < ushort.MaxValue; i++)
                protocol.Add(new EventSender("TestMessage" + i));

            Assert.Catch<InvalidOperationException>(() => protocol.Add(new EventSender("TestMessage")));
        }

        [Test]
        public void AddMessageReadOnly()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var inverse = protocol.CreateInverse();

            Assert.Catch<InvalidOperationException>(() => inverse.Add(new EventSender("TestMessage")));
        }

        [Test]
        public void GetEventSender()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var messageSender = new EventSender("TestSender");
            var messageReceiver = new EventReceiver("TestReceiver");

            Assert.Catch<ArgumentNullException>(() => protocol.GetEventSender(null));
            Assert.Catch<ArgumentException>(() => protocol.GetEventSender(messageSender.ID));

            protocol.Add(messageSender);
            protocol.Add(messageReceiver);

            Assert.Catch<ArgumentException>(() => protocol.GetEventSender(messageReceiver.ID));

            var sender = protocol.GetEventSender(messageSender.ID);

            Assert.AreEqual(messageSender, sender);
        }

        [Test]
        public void GetEventReceiver()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var messageSender = new EventSender("TestSender");
            var messageReceiver = new EventReceiver("TestReceiver");

            Assert.Catch<ArgumentNullException>(() => protocol.GetEventReceiver(null));
            Assert.Catch<ArgumentException>(() => protocol.GetEventReceiver(messageReceiver.ID));

            protocol.Add(messageSender);
            protocol.Add(messageReceiver);

            Assert.Catch<ArgumentException>(() => protocol.GetEventReceiver(messageSender.ID));

            var receiver = protocol.GetEventReceiver(messageReceiver.ID);

            Assert.AreEqual(messageReceiver, receiver);
        }

        [Test]
        public void GetDataSender()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var messageSender = new BinarySender<int>("TestSender");
            var messageReceiver = new BinaryReceiver<int>("TestReceiver");

            Assert.Catch<ArgumentNullException>(() => protocol.GetDataSender<int, BinarySender<int>>(null));
            Assert.Catch<ArgumentException>(() => protocol.GetDataSender<int, BinarySender<int>>(messageSender.ID));

            protocol.Add(messageSender);
            protocol.Add(messageReceiver);

            Assert.Catch<ArgumentException>(() => protocol.GetDataSender<long, BinarySender<long>>(messageSender.ID));
            Assert.Catch<ArgumentException>(() => protocol.GetDataSender<int, BinarySender<int>>(messageReceiver.ID));

            var sender = protocol.GetDataSender<int, BinarySender<int>>(messageSender.ID);

            Assert.AreEqual(messageSender, sender);
        }

        [Test]
        public void GetDataReceiver()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var messageSender = new BinarySender<int>("TestSender");
            var messageReceiver = new BinaryReceiver<int>("TestReceiver");

            Assert.Catch<ArgumentNullException>(() => protocol.GetDataReceiver<int, BinaryReceiver<int>>(null));
            Assert.Catch<ArgumentException>(() => protocol.GetDataReceiver<int, BinaryReceiver<int>>(messageReceiver.ID));

            protocol.Add(messageSender);
            protocol.Add(messageReceiver);

            Assert.Catch<ArgumentException>(() => protocol.GetDataReceiver<long, BinaryReceiver<long>>(messageReceiver.ID));
            Assert.Catch<ArgumentException>(() => protocol.GetDataReceiver<int, BinaryReceiver<int>>(messageSender.ID));

            var receiver = protocol.GetDataReceiver<int, BinaryReceiver<int>>(messageReceiver.ID);

            Assert.AreEqual(messageReceiver, receiver);
        }

        [Test]
        public void CreateInverse()
        {
            var protocol = new Protocol("TestProtocol", new Version());

            var eventSender = new EventSender("EventSender");
            var eventReceiver = new EventReceiver("EventReceiver");
            var dataSender = new BinarySender<int>("DataSender");
            var dataReceiver = new BinaryReceiver<int>("DataReceiver");

            protocol.Add(eventSender);
            protocol.Add(eventReceiver);
            protocol.Add(dataSender);
            protocol.Add(dataReceiver);

            var inverse = protocol.CreateInverse();

            Assert.IsTrue(inverse.IsReadOnly);
            Assert.AreEqual(protocol.Name, inverse.Name);
            Assert.AreEqual(protocol.Count(), inverse.Count());

            Assert.DoesNotThrow(() => inverse.GetEventReceiver(eventSender.ID));
            Assert.DoesNotThrow(() => inverse.GetEventSender(eventReceiver.ID));
            Assert.DoesNotThrow(() => inverse.GetDataReceiver<int, BinaryReceiver<int>>(dataSender.ID));
            Assert.DoesNotThrow(() => inverse.GetDataSender<int, BinarySender<int>>(dataReceiver.ID));

            var inverseOfInverse = inverse.CreateInverse();

            Assert.IsTrue(inverseOfInverse.IsReadOnly);
            Assert.AreEqual(protocol.Name, inverseOfInverse.Name);
            Assert.AreEqual(protocol.Count(), inverseOfInverse.Count());

            Assert.DoesNotThrow(() => inverseOfInverse.GetEventSender(eventSender.ID));
            Assert.DoesNotThrow(() => inverseOfInverse.GetEventReceiver(eventReceiver.ID));
            Assert.DoesNotThrow(() => inverseOfInverse.GetDataSender<int, BinarySender<int>>(dataSender.ID));
            Assert.DoesNotThrow(() => inverseOfInverse.GetDataReceiver<int, BinaryReceiver<int>>(dataReceiver.ID));
        }

        [Test]
        public void Serialize()
        {
            var protocol = new Protocol("TestProtocol", new Version());

            var eventSender = new EventSender("EventSender");
            var eventReceiver = new EventReceiver("EventReceiver");
            var dataSender = new BinarySender<int>("DataSender");
            var dataReceiver = new BinaryReceiver<int>("DataReceiver");

            protocol.Add(eventSender);
            protocol.Add(eventReceiver);
            protocol.Add(dataSender);
            protocol.Add(dataReceiver);

            var stream = new MemoryStream();

            protocol.Serialize(stream);
            stream.Position = 0;

            var deserialized = new Protocol(stream);

            Assert.IsTrue(deserialized.IsReadOnly);
            Assert.AreEqual(protocol.Name, deserialized.Name);
            Assert.AreEqual(protocol.Version, deserialized.Version);
            Assert.AreEqual(protocol.Count(), deserialized.Count());

            Assert.DoesNotThrow(() => deserialized.GetEventSender(eventSender.ID));
            Assert.DoesNotThrow(() => deserialized.GetEventReceiver(eventReceiver.ID));
            Assert.DoesNotThrow(() => deserialized.GetDataSender<int, BinarySender<int>>(dataSender.ID));
            Assert.DoesNotThrow(() => deserialized.GetDataReceiver<int, BinaryReceiver<int>>(dataReceiver.ID));
        }

        [Test]
        public void DeserializeNull()
        {
            Assert.Catch<ArgumentNullException>(() => new Protocol((Stream)null));
        }

        [UnityTest]
        public IEnumerator SendEvent()
        {
            var protocol = new Protocol("TestProtocol", new Version());

            var eventSender0 = new EventSender("Event0");
            var eventReceiver1 = new EventReceiver("Event1");

            protocol.Add(eventSender0);
            protocol.Add(eventReceiver1);

            var inverse = protocol.CreateInverse();

            var eventReceiver0 = inverse.GetEventReceiver(eventSender0.ID);
            var eventSender1 = inverse.GetEventSender(eventReceiver1.ID);

            var receivedEvent0Count = 0;
            var receivedEvent1Count = 0;

            eventReceiver0.AddHandler(() => receivedEvent0Count++);
            eventReceiver1.AddHandler(() => receivedEvent1Count++);

            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                server.RemoteConnected += (remote) => protocol.SetNetwork(server, remote);
                client.RemoteConnected += (remote) => inverse.SetNetwork(client, remote);

                server.StartServer(k_Port);
                client.ConnectToServer("127.0.0.1", k_Port);

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                Assert.AreEqual(0, receivedEvent0Count);
                Assert.AreEqual(0, receivedEvent1Count);

                eventSender0.Send();
                eventSender1.Send();
                eventSender1.Send();

                server.Update();
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                Assert.AreEqual(1, receivedEvent0Count);
                Assert.AreEqual(2, receivedEvent1Count);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        [UnityTest]
        public IEnumerator SendData()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var dataSender = new BinarySender<int>("Data");

            protocol.Add(dataSender);

            var inverse = protocol.CreateInverse();

            var dataReceiver = inverse.GetDataReceiver<int, BinaryReceiver<int>>(dataSender.ID);

            var receivedData = 0;
            var receivedDataCount = 0;

            dataReceiver.AddHandler((data) =>
            {
                receivedData = data;
                receivedDataCount++;
            });

            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                server.RemoteConnected += (remote) => protocol.SetNetwork(server, remote);
                client.RemoteConnected += (remote) => inverse.SetNetwork(client, remote);

                server.StartServer(k_Port);
                client.ConnectToServer("127.0.0.1", k_Port);

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                Assert.AreEqual(0, receivedData);
                Assert.AreEqual(0, receivedDataCount);

                dataSender.Send(50);
                dataSender.Send(50);

                server.Update();
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                Assert.AreEqual(50, receivedData);
                Assert.AreEqual(1, receivedDataCount);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }

        [UnityTest]
        public IEnumerator SendTexture()
        {
            var protocol = new Protocol("TestProtocol", new Version());
            var textureSender = new TextureSender("Data", compression: TextureCompression.Raw);

            protocol.Add(textureSender);

            var inverse = protocol.CreateInverse();

            TextureReceiver.TryGet(inverse, textureSender.ID, out var dataReceiver);

            var receivedData = default(TextureData);
            var receivedDataCount = 0;

            dataReceiver.AddHandler((data) =>
            {
                receivedData = data;
                receivedDataCount++;
            });

            var server = new NetworkServer();
            var client = new NetworkClient();

            try
            {
                server.RemoteConnected += (remote) => protocol.SetNetwork(server, remote);
                client.RemoteConnected += (remote) => inverse.SetNetwork(client, remote);

                server.StartServer(k_Port);
                client.ConnectToServer("127.0.0.1", k_Port);

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                Assert.AreEqual(default(TextureData), receivedData);
                Assert.AreEqual(0, receivedDataCount);

                var tex = new Texture2D(2, 2, GraphicsFormat.R8G8B8A8_UNorm, 2, TextureCreationFlags.None);
                var data = new byte[]
                {
                    // mip 0: 2x2 size
                    255, 0, 0, 255, // red
                    0, 255, 0, 255, // green
                    0, 0, 255, 255, // blue
                    255, 235, 4, 255, // yellow
                    // mip 1: 1x1 size
                    0, 255, 255, 255 // cyan
                };
                tex.SetPixelData(data, 0, 0); // mip 0
                tex.SetPixelData(data, 1, 12); // mip 1
                tex.filterMode = FilterMode.Point;
                tex.Apply(false);

                textureSender.Send(new TextureData(tex, "Test Metadata"));

                server.Update();
                client.Update();

                yield return TestUtils.WaitForPlayerLoopUpdates(500);

                server.Update();
                client.Update();

                Assert.AreEqual(1, receivedDataCount);

                Assert.AreEqual(tex.mipmapCount, receivedData.texture.mipmapCount);
                Assert.AreEqual(tex.width, receivedData.texture.width);
                Assert.AreEqual(tex.height, receivedData.texture.height);
                Assert.AreEqual(tex.graphicsFormat, receivedData.texture.graphicsFormat);
                Assert.AreEqual(tex.filterMode, receivedData.texture.filterMode);
                Assert.AreEqual("Test Metadata", receivedData.metadata);

                var pixels = tex.GetRawTextureData<byte>();
                var receivedPixels = receivedData.texture.GetRawTextureData<byte>();

                Assert.AreEqual(pixels.Length, receivedPixels.Length);

                for (var i = 0; i < 16; i++)
                {
                    Assert.AreEqual(pixels[i], receivedPixels[i]);
                }

                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(receivedData.texture);
            }
            finally
            {
                server.Stop();
                client.Stop();
            }
        }
    }
}
