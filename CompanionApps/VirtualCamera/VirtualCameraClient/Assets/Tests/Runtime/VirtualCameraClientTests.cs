using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.CompanionApp;

namespace Unity.CompanionApps.VirtualCamera.Tests
{
    class VirtualCameraClientTests
    {
        /*
        const int k_Port = 8927;
        GameObject m_ClientGameObject;
        VirtualCameraClient m_VirtualCameraClient;
        VirtualProductionRemote m_VirtualProductionRemote;
        Transform m_VirtualCameraTransform;
        CompanionAppServer m_Server;
        */

        [SetUp]
        public void Setup()
        {
            /*
            m_Server = ScriptableObject.CreateInstance<CompanionAppServer>();
            m_Server.Port = k_Port;

            // setting the client
            m_ClientGameObject = new GameObject("Client");
            m_VirtualCameraClient = m_ClientGameObject.AddComponent<VirtualCameraClient>();
            m_VirtualProductionRemote = m_ClientGameObject.GetComponent<VirtualProductionRemote>();

            // setting the client virtual camera
            m_VirtualCameraTransform = new GameObject("Virtual Camera").transform;
            m_VirtualCameraClient.virtualCamera = m_VirtualCameraTransform;
            */
        }

        [TearDown]
        public void TearDown()
        {
            /*
            ScriptableObject.DestroyImmediate(m_Server);
            GameObject.DestroyImmediate(m_ClientGameObject);
            GameObject.DestroyImmediate(m_VirtualCameraTransform.gameObject);
            */
        }

        [UnityTest]
        public IEnumerator ClientAndServerAreConnected()
        {
            /*
            try
            {
                // starting server
                m_Server.StartServer();

                // starting the client;
                m_VirtualProductionRemote.ip = "127.0.0.1";
                m_VirtualProductionRemote.port = k_Port;
                m_VirtualProductionRemote.Connect();

                yield return Wait(50);
                m_Server.OnUpdate();
                yield return Wait(50);

                Assert.True(m_VirtualProductionRemote.isConnected, "The client is not connected to the server");

                // making sure that the server receives data sent from the client
                var sentSample = new FocalLengthSample { Time = 2.5, FocalLength = 17.3f};
                var receivedSample = default(FocalLengthSample);

                Action<FocalLengthSample> onReceive = (sample) =>
                {
                    receivedSample = sample;
                };

                var client = m_Server.GetClients().Cast<IVirtualCameraClientInternal>().First();

                client.FocalLengthSampleReceived += onReceive;

                // send a lens data from the client
                m_VirtualCameraClient.host.SendFocalLength(sentSample);

                yield return Wait(50);
                m_Server.OnUpdate();

                Assert.AreEqual(sentSample, receivedSample, "Data received by server does not match what client sent!");

                client.FocalLengthSampleReceived -= onReceive;
            }
            finally
            {
                m_VirtualProductionRemote.Disconnect();
                m_Server.StopServer();
            }
            */
            yield return null;
        }

        /*
        static IEnumerator Wait(int frames)
        {
            while (frames > 0)
            {
                yield return null;
                frames--;
            }
        }
        */
    }
}
