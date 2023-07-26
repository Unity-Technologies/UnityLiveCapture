using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    public class CompanionAppDeviceTests
    {
        const string k_TmpDir = "Assets/tmp";

        class TestDevice : CompanionAppDevice<ICompanionAppClient>
        {
            public bool updateClientCalled { get; set; }

            protected override void UpdateDevice()
            {
                UpdateClient();
            }

            protected override void LiveUpdate()
            {
            }

            public override void Write(ITakeBuilder takeBuilder)
            {
            }

            public override void UpdateClient()
            {
                base.UpdateClient();

                updateClientCalled = true;
            }
        }

        TestDevice m_Device;
        ICompanionAppClientInternal m_Client;
        ITakeRecorderContext m_Context;
        Shot m_Shot;
        IAssetPreview m_AssetPreview;

        [SetUp]
        public void Setup()
        {
            m_Shot = new Shot()
            {
                Directory = k_TmpDir
            };

            m_Context = Substitute.For<ITakeRecorderContext>();
            m_Context.Selection = 0;
            m_Context.Shots.Returns(c => new Shot[] { m_Shot });
            m_Context.SetShot(Arg.Is(0), Arg.Do<Shot>(s => m_Shot = s));

            TakeRecorder.SetContext(m_Context);

            m_Client = Substitute.For<ICompanionAppClientInternal>();

            m_AssetPreview = Substitute.For<IAssetPreview>();

            var go = new GameObject();
            m_Device = go.AddComponent<TestDevice>();
            m_Device.m_AssetManager = new AssetManager(m_AssetPreview);
        }

        [TearDown]
        public void TearDown()
        {
            TakeRecorder.SetContext(null);

            GameObject.DestroyImmediate(m_Device.gameObject);

            ClearTmpDir(k_TmpDir);
        }

        [Test]
        public void SetCompatibleClientWorks()
        {
            Assert.DoesNotThrow(() => { m_Device.SetClient(m_Client, false); },
                "Set compatible client should not throw");

            Assert.AreEqual(m_Client, m_Device.GetClient(), "Set client failed");
        }

        [Test]
        public void ClientReceivesDeviceModeChange()
        {
            TakeRecorder.IsLive = false;

            m_Device.SetClient(m_Client, false);

            m_Client.Received().SendDeviceMode(Arg.Is(DeviceMode.Playback));

            TakeRecorder.IsLive = true;

            m_Device.InvokeUpdateDevice();

            m_Client.Received().SendDeviceMode(Arg.Is(DeviceMode.LiveStream));
        }

        [Test]
        public void ReceiveSetModeCommandWithLiveStream_SetsTakeRecorderLive()
        {
            TakeRecorder.IsLive = false;

            m_Device.SetClient(m_Client, false);

            m_Client.SetDeviceMode += Raise.Event<Action<DeviceMode>>(DeviceMode.LiveStream);

            Assert.True(TakeRecorder.IsLive, "Incorrect Live state in TakeRecorder");
        }

        [Test]
        public void ReceiveSetModeCommandWithPlayback_SetsTakeRecorderPlayback()
        {
            TakeRecorder.IsLive = true;

            m_Device.SetClient(m_Client, false);

            m_Client.SetDeviceMode += Raise.Event<Action<DeviceMode>>(DeviceMode.Playback);

            Assert.False(TakeRecorder.IsLive, "Incorrect Live state in TakeRecorder");
        }

        // These tests are failing intermittently (or freezing the editor) on Bokken because of a potential issue in AssetDatabase.
        // Disabling to unblock our ci pipeline but we will reevaluate from time to time.
#if UNITY_2020_3
        [UnityPlatform(exclude = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer})]
#endif
        [Test]
        public void ClientReceivesRecordingChange()
        {
            m_Device.SetClient(m_Client, false);
            m_Device.InvokeStartRecording();
            m_Client.Received().SendRecordingState(Arg.Is(true));
            m_Device.InvokeStopRecording();
            m_Client.Received().SendRecordingState(Arg.Is(false));
        }

        [Test]
        public void DerivedDeviceReceivesDoUpdateClientOnSetClient()
        {
            m_Device.SetClient(m_Client, false);
            Assert.True(m_Device.updateClientCalled, "UpdateClient was not called");
        }

        [Test]
        public void ClientUpdateWithNullShotDoesNotThrow()
        {
            m_Device.SetClient(m_Client, false);

            m_Device.InvokeUpdateDevice();

            m_Context.Selection = -1;

            Assert.False(TakeRecorder.Shot.HasValue, "Active shot should be null");

            Assert.DoesNotThrow(() => m_Device.InvokeUpdateDevice());
        }

        [Test]
        public void ClientUpdateWithNullShotSendsEmptySlate()
        {
            m_Device.SetClient(m_Client, false);
            m_Context.Selection = -1;
            m_Device.InvokeUpdateDevice();

            m_Client.Received(1).SendShotDescriptor(Arg.Is<ShotDescriptor>(
                d => d.Slate == Slate.Empty
            ));
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(7)]
        public void CanSelectTake(int takeIndex)
        {
            m_Device.SetClient(m_Client, false);

            Assert.IsNull(m_Shot.Take);

            CreateTakes(0, 8);

            var takes = GetTakes(m_Shot);
            var selectedTake = takes[takeIndex];

            m_Client.SetSelectedTake += Raise.Event<Action<Guid>>(selectedTake.Guid);

            Assert.IsNotNull(m_Shot.Take);
            Assert.IsTrue(GetGuid(m_Shot.Take) == selectedTake.Guid);
        }

        [Test]
        public void SelectTakeThatDoesNotExistDoesNothing()
        {
            m_Device.SetClient(m_Client, false);

            Assert.IsNull(m_Shot.Take);
            m_Client.SetSelectedTake += Raise.Event<Action<Guid>>(Guid.NewGuid());
            Assert.IsNull(m_Shot.Take);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(7)]
        public void CanDeleteTake(int takeIndex)
        {
            m_Device.SetClient(m_Client, false);

            CreateTakes(0, 8);

            var takes = GetTakes(m_Shot);
            var deletedTake = takes[takeIndex];

            m_Client.DeleteTake += Raise.Event<Action<Guid>>(deletedTake.Guid);

            var takesAfterDeletion = GetTakes(m_Shot);

            Assert.IsTrue(takesAfterDeletion.Length == takes.Length - 1);
            Assert.IsTrue(Array.FindIndex(takesAfterDeletion, x => x.Guid == deletedTake.Guid) == -1);

            foreach (var take in takesAfterDeletion)
            {
                Assert.IsTrue(Array.FindIndex(takes, x => x.Guid == take.Guid) != -1);
            }
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(7)]
        public void CanSetIterationBase(int takeIndex)
        {
            m_Device.SetClient(m_Client, false);

            Assert.IsNull(m_Shot.IterationBase);

            CreateTakes(0, 8);

            var takes = GetTakes(m_Shot);
            var iterationBase = takes[takeIndex];

            m_Client.SetIterationBase += Raise.Event<Action<Guid>>(iterationBase.Guid);

            Assert.IsNotNull(m_Shot.IterationBase);
            Assert.IsTrue(GetGuid(m_Shot.IterationBase) == iterationBase.Guid);
        }

        [Test]
        public void SetIterationBaseThatDoesNotExistDoesNothing()
        {
            m_Device.SetClient(m_Client, false);

            Assert.IsNull(m_Shot.IterationBase);
            m_Client.SetIterationBase += Raise.Event<Action<Guid>>(Guid.NewGuid());
            Assert.IsNull(m_Shot.IterationBase);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(7)]
        public void CanSetTakeData(int takeIndex)
        {
            m_Device.SetClient(m_Client, false);

            CreateTakes(0, 8);

            var takes = GetTakes(m_Shot);

            var take = takes[takeIndex];
            take.ShotName = "new_name";

            m_Client.SetTakeData += Raise.Event<Action<TakeDescriptor>>(take);

            var takesAfterUpdate = GetTakes(m_Shot);
            var updatedTake = Array.Find(takesAfterUpdate, x => x.Guid == take.Guid);

            Assert.IsNotNull(updatedTake);
            Assert.IsTrue(updatedTake.ShotName == take.ShotName);
        }

        [Test]
        public void CanLoadScreenshot()
        {
            m_Device.SetClient(m_Client, false);

            var texture = new Texture2D(256, 256);
            var texturePreview = new Texture2D(128, 128);
            var builder = CreateTakeBuilder(0, $"test", 0, texture);

            builder.Dispose();

            var takes = GetTakes(m_Shot);

            Assert.IsTrue(takes.Length == 1);

            var take = takes[0];
            var screenshotGuid = takes[0].Screenshot;

            m_AssetPreview.GetAssetPreview<Texture2D>(Arg.Is(screenshotGuid)).Returns(texturePreview);

            m_Client.TexturePreviewRequested += Raise.Event<Action<Guid>>(screenshotGuid);
            m_Client.Received().SendTexturePreview(Arg.Is(screenshotGuid), Arg.Is(texturePreview));

            GameObject.DestroyImmediate(texturePreview);
        }

        static TakeBuilder CreateTakeBuilder(int sceneNumber, string shotName, int takeNumber, Texture2D screenShot = null)
        {
            var resolver = Substitute.For<IExposedPropertyTable>();

            return new TakeBuilder(0d, 0d, 0d, 0d, sceneNumber, shotName, takeNumber, "", k_TmpDir, DateTime.Now, null, StandardFrameRate.FPS_30_00, screenShot, resolver);
        }

        static void CreateTakes(int sceneNumber, int numTakes)
        {
            for (var i = 0; i != numTakes; ++i)
            {
                var builder = CreateTakeBuilder(sceneNumber, $"test{i + 1}", i + 1);
                builder.Dispose();
            }
        }

        static TakeDescriptor[] GetTakes(Shot shot) => ShotDescriptor.Create(shot).Takes;

        static SerializableGuid GetGuid(ScriptableObject obj)
        {
            return SerializableGuid.FromString(AssetDatabaseUtility.GetAssetGUID(obj));
        }

        static void ClearTmpDir(string dir)
        {
            FileUtil.DeleteFileOrDirectory(dir);
            FileUtil.DeleteFileOrDirectory($"{dir}.meta");
            AssetDatabase.Refresh();
        }
    }
}
