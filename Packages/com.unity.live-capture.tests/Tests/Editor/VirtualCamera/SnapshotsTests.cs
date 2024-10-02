using System.Collections;
using NUnit.Framework;
using NSubstitute;
using Unity.LiveCapture.VirtualCamera;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    class SnapshotsTests
    {
        protected VirtualCameraActor m_Actor;
        protected VirtualCameraDevice m_Device;
        protected IVirtualCameraClientInternal m_Client;
        protected ITakeRecorderContext m_Context;
        protected Shot m_Shot;
        protected IScreenshotImpl m_ScreenshotImpl;
        protected SnapshotLibrary m_SnapshotLibrary;

        [SetUp]
        public void Setup()
        {
            m_Context = Substitute.For<ITakeRecorderContext>();
            m_Context.Selection = 0;
            m_Context.Shots.Returns(c => new Shot[] { m_Shot });
            m_Context.SetShot(Arg.Is(0), Arg.Do<Shot>(s => m_Shot = s));

            var go = new GameObject("camera capture");

            m_ScreenshotImpl = Substitute.For<IScreenshotImpl>();
            m_Shot = default(Shot);

            TakeRecorder.SetContext(m_Context);
            TakeRecorder.IsLive = true;
            TakeRecorder.FrameRate = StandardFrameRate.FPS_30_00;

            m_Device = go.AddComponent<VirtualCameraDevice>();
            m_Device.SetScreenshotImpl(m_ScreenshotImpl);

            m_Actor = new GameObject("Actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();
            m_Device.Actor = m_Actor;

            m_Client = Substitute.For<IVirtualCameraClientInternal>();

            m_SnapshotLibrary = ScriptableObject.CreateInstance<SnapshotLibrary>();
            m_Device.SnapshotLibrary = m_SnapshotLibrary;

            m_Device.SetClient(m_Client, false);

            m_Client.ClearReceivedCalls();
        }

        [TearDown]
        public void TearDown()
        {
            TakeRecorder.SetContext(null);

            if (m_Device != null)
            {
                GameObject.DestroyImmediate(m_Device.gameObject);
            }

            if (m_Actor != null)
            {
                GameObject.DestroyImmediate(m_Actor.gameObject);
            }

            if (m_SnapshotLibrary != null)
            {
                GameObject.DestroyImmediate(m_SnapshotLibrary);
            }
        }
    }

    // These tests are failing intermittently (or freezing the editor) on Bokken because of a potential issue in AssetDatabase.
    // Disabling to unblock our ci pipeline but we will reevaluate from time to time.
#if UNITY_2020_3
    [UnityPlatform(exclude = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer})]
#endif
    class SnapshotTestsUnstable : SnapshotsTests
    {
        [Test]
        public void TakeSnapshotDoesNotRequestScreenshotWhenRecording()
        {
            m_Device.InvokeStartRecording();
            m_Device.TakeSnapshot();

            m_ScreenshotImpl.Received(0)
                .Take(Arg.Any<Camera>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<FrameRate>());
        }

        [Test]
        public void TakeSnapshotDoesNotCreateSnapshotsWhenDeviceIsRecording()
        {
            m_Device.InvokeStartRecording();

            Assert.True(m_Device.IsRecording, "Incorrect recording state");

            m_Device.TakeSnapshot();

            Assert.AreEqual(0, m_Device.GetSnapshotCount(), "Incorrect snapshot count");
        }

        [Test]
        public void GoToSnapshotDoesNotSetOriginAndLocalPoseWhenRecording()
        {
            var pose = new Pose()
            {
                position = Vector3.one,
                rotation = Quaternion.Euler(10f, 15f, 0f)
            };

            var origin = new Pose()
            {
                position = Vector3.one,
                rotation = Quaternion.Euler(0f, 15f, 0f)
            };

            var localPose = new Pose()
            {
                position = Vector3.zero,
                rotation = Quaternion.Euler(10f, 0f, 0f)
            };

            var snapshot = new Snapshot()
            {
                Pose = pose
            };

            m_Device.InvokeStartRecording();
            m_Device.GoToSnapshot(snapshot);

            Assert.AreNotEqual(origin, m_Device.Origin, "Incorrect pose in origin");
            Assert.AreNotEqual(localPose, m_Device.LocalPose, "Incorrect local pose in origin");
        }
    }

    class SnapshotsTestsStable : SnapshotsTests
    {
        [Test]
        public void TakeSnapshotRequestsScreenshot()
        {
            m_Device.TakeSnapshot();

            m_ScreenshotImpl.Received(1)
                .Take(Arg.Any<Camera>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<FrameRate>());
        }

        [Test]
        public void TakeSnapshotRequestsScreenshotUsingSlateAndTakeRecorderData()
        {
            var time = 123d;
            var frameRate = StandardFrameRate.FPS_29_97.ToValue();

            m_Shot.SceneNumber = 123;
            m_Shot.Name = "testShotName";
            m_Context.GetTime().Returns((s) => time);
            TakeRecorder.FrameRate = frameRate;

            m_Device.TakeSnapshot();

            m_ScreenshotImpl.Received(1)
                .Take(Arg.Any<Camera>(), m_Shot.SceneNumber, m_Shot.Name, time, frameRate);
        }

        [Test]
        public void TakeSnapshotRequestsScreenshotWhenShotIsNull()
        {
            var time = 123d;
            var frameRate = StandardFrameRate.FPS_29_97.ToValue();

            m_Context.Selection = -1;
            m_Context.GetTime().Returns((s) => time);
            TakeRecorder.FrameRate = frameRate;

            m_Device.TakeSnapshot();

            m_ScreenshotImpl.Received(1)
                .Take(Arg.Any<Camera>(), 0, "", time, frameRate);
        }

        [Test]
        public void TakeSnapshotDoesNotRequestScreenshotWhenTakeRecorderIsNotLive()
        {
            TakeRecorder.IsLive = false;

            m_Device.TakeSnapshot();

            m_ScreenshotImpl.DidNotReceive()
                .Take(Arg.Any<Camera>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<FrameRate>());
        }

        [Test]
        public void TakeSnapshotStoresCurrentWorldPoseAndLensAssetAndLensAndCameraBody()
        {
            var rotation = Quaternion.Euler(90, 10, 15);
            var origin = new Pose()
            {
                position = Vector3.up,
                rotation = Quaternion.identity
            };

            var localPose = new Pose()
            {
                position = Vector3.right,
                rotation = rotation
            };

            var worldPose = new Pose()
            {
                position = Vector3.up + Vector3.right,
                rotation = rotation
            };

            m_Device.SetOrigin(origin);
            m_Device.SetARPose(localPose);

            Assert.AreEqual(worldPose, m_Device.Pose, "Incorrent pose");

            m_Device.TakeSnapshot();

            Assert.AreEqual(1, m_Device.GetSnapshotCount(), "Incorrect snapshot count");

            var snapshot = m_Device.GetSnapshot(0);

            Assert.AreEqual(worldPose, snapshot.Pose, "Incorrect pose");
            Assert.AreEqual(m_Device.Lens, snapshot.Lens, "Incorrect snapshot lens");
            Assert.AreEqual(m_Device.LensAsset, snapshot.LensAsset, "Incorrect snapshot lens asset");
            Assert.AreEqual(m_Device.CameraBody, snapshot.CameraBody, "Incorrect snapshot lens asset");
        }

        [Test]
        public void TakeSnapshotDoesNotCreateSnapshotsWhenDeviceIsNotReady()
        {
            m_Device.Actor = null;

            Assert.False(m_Device.IsReady(), "Incorrect ready state");

            m_Device.TakeSnapshot();

            Assert.AreEqual(0, m_Device.GetSnapshotCount(), "Incorrect snapshot count");
        }

        [Test]
        [Ignore("Disabled for UUM-48738, will be fixed in LC-1644")]
        public void GoToSnapshotSetsOriginAndLocalPose()
        {
            var pose = new Pose()
            {
                position = Vector3.one,
                rotation = Quaternion.Euler(10f, 15f, 0f)
            };

            var origin = new Pose()
            {
                position = Vector3.one,
                rotation = Quaternion.Euler(0f, 15f, 0f)
            };

            var localPose = new Pose()
            {
                position = Vector3.zero,
                rotation = Quaternion.Euler(10f, 0f, 0f)
            };

            var snapshot = new Snapshot()
            {
                Pose = pose
            };

            m_Device.GoToSnapshot(snapshot);

            // TODO: Update/fix and enable the local pose test
            // Currently, this test does not pass because the of localPose so it has been disabled. 
            // The quaternions are not equal to each other, even though they appear to be the same value.
            // When printing these values to console, the difference between the mismatched values is very small(10^-9 or 10^-10)

            Assert.AreEqual(origin, m_Device.Origin, "Incorrect pose in origin");
            Assert.AreEqual(localPose, m_Device.LocalPose, "Incorrect local pose in origin");
        }

        [Test]
        public void GoToSnapshotSetsShotAndTime()
        {
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            var track = timeline.CreateTrack<TakeRecorderTrack>();
            var clip = track.CreateClip<ShotPlayableAsset>();
            var director = new GameObject("director", typeof(PlayableDirector))
                .GetComponent<PlayableDirector>();

            clip.start = 10d;
            clip.duration = 20d;
            director.playableAsset = timeline;
            director.RebuildGraph();

            var context = new DirectorContext(new DirectorProvider(director));

            context.Selection = -1;

            TakeRecorder.SetContext(context);

            var snapshot = new Snapshot()
            {
                Asset = clip.asset,
                Time = 1d
            };

            m_Device.GoToSnapshot(snapshot);

            Assert.AreEqual(0, context.Selection, "Incorrect selection after restoring snapshot");
            Assert.AreEqual(11d, director.time, "Incorrect time after restoring snapshot");

            Object.DestroyImmediate(director.gameObject);

            using (TimelineDisableUndoScope.Create())
            {
                timeline.DeleteTrack(track);
            }

            ScriptableObject.DestroyImmediate(timeline);
        }

        [Test]
        public void LoadSnapshotSetsLensAndLensAssetAndCameraBody()
        {
            var lensAsset = ScriptableObject.CreateInstance<LensAsset>();

            lensAsset.Intrinsics = new LensIntrinsics()
            {
                FocalLengthRange = new Vector2(0f, 100f),
                ApertureRange = new Vector2(1, 3)
            };

            var lens = new Lens()
            {
                FocalLength = 55f,
                FocusDistance = 33,
                Aperture = 2
            };

            var cameraBody = new CameraBody()
            {
                SensorSize = Vector2.one
            };

            var snapshot = new Snapshot()
            {
                LensAsset = lensAsset,
                Lens = lens,
                CameraBody = cameraBody
            };

            m_Device.LoadSnapshot(snapshot);
            m_Device.InvokeLiveUpdate();

            Assert.AreEqual(lensAsset, m_Device.LensAsset, "Incorrect lens asset");
            Assert.AreEqual(lens, m_Device.Lens, "Incorrect lens values");
            Assert.AreEqual(cameraBody, m_Device.CameraBody, "Incorrect camera body");
            Assert.AreEqual(lens, m_Device.Actor.Lens, "Incorrect lens on actor");

            ScriptableObject.DestroyImmediate(lensAsset);
        }

        [Test]
        public void DeleteSnapshot()
        {
            m_Device.TakeSnapshot();

            Assert.AreEqual(1, m_Device.GetSnapshotCount(), "Incorrect snapshot count");

            m_Device.DeleteSnapshot(0);

            Assert.AreEqual(0, m_Device.GetSnapshotCount(), "Incorrect snapshot count");
        }

        [Test]
        public void TakeAndDeleteSendSnapshotsUpdatesClient()
        {
            m_Device.TakeSnapshot();

            Assert.AreEqual(1, m_Device.GetSnapshotCount(), "Incorrect snapshot count");
            m_Device.UpdateClient();
            m_Client.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(descriptor => descriptor.Snapshots.Length == 1));

            m_Device.DeleteSnapshot(0);

            Assert.AreEqual(0, m_Device.GetSnapshotCount(), "Incorrect snapshot count");
            m_Device.UpdateClient();
            m_Client.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(descriptor => descriptor.Snapshots.Length == 0));
        }

        [Test]
        public void SetNewSnapshotLibraryUpdatesClient()
        {
            var newSnapshotLibrary = ScriptableObject.CreateInstance<SnapshotLibrary>();
            newSnapshotLibrary.Add(new Snapshot());

            m_Device.SnapshotLibrary = newSnapshotLibrary;

            m_Device.UpdateClient();
            m_Client.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(descriptor => descriptor.Snapshots.Length == 1));
        }

        [Test]
        public void SetNullLibraryUpdatesClientWithEmptySnapshotDescriptor()
        {
            m_Device.TakeSnapshot();
            m_Device.UpdateClient();

            m_Client.ClearReceivedCalls();

            m_Device.SnapshotLibrary = null;
            m_Device.UpdateClient();
            m_Client.Received(1).SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(descriptor => descriptor.Snapshots.Length == 0));
        }

        [Test]
        public void SnapshotLibraryAssetCreatedOnDemand()
        {
            m_Device.SnapshotLibrary = null;

            Assert.IsNull(m_Device.SnapshotLibrary);

            m_Device.TakeSnapshot();

            Assert.IsNotNull(m_Device.SnapshotLibrary);

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_Device.SnapshotLibrary));
        }

        [Test]
        public void SnapshotLibraryContentsExternalChangeUpdatesClient()
        {
            m_SnapshotLibrary.Add(new Snapshot());
            m_SnapshotLibrary.Add(new Snapshot());
            m_SnapshotLibrary.Add(new Snapshot());

            m_Device.UpdateClient();
            m_Client.Received(1).SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(descriptor => descriptor.Snapshots.Length == 3));
        }

        [Test]
        public void SharedSnapshotLibraryUpdatesClients()
        {
            var go = new GameObject("second camera capture");

            var otherDevice = go.AddComponent<VirtualCameraDevice>();
            var otherActor = new GameObject("other Actor", typeof(VirtualCameraActor)).GetComponent<VirtualCameraActor>();
            otherDevice.SetScreenshotImpl(m_ScreenshotImpl);
            otherDevice.Actor = otherActor;

            var otherClient = Substitute.For<IVirtualCameraClientInternal>();
            otherDevice.SnapshotLibrary = m_SnapshotLibrary;

            otherDevice.SetClient(otherClient, false);
            otherDevice.UpdateClient();
            otherClient.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(snapshots => snapshots.Snapshots.Length == m_SnapshotLibrary.Count));

            m_Device.TakeSnapshot();
            m_Device.UpdateClient();
            m_Client.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(snapshots => snapshots.Snapshots.Length == m_SnapshotLibrary.Count));

            otherDevice.UpdateClient();
            otherClient.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(snapshots => snapshots.Snapshots.Length == m_SnapshotLibrary.Count));

            m_Device.DeleteSnapshot(0);
            m_Device.UpdateClient();
            m_Client.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(snapshots => snapshots.Snapshots.Length == m_SnapshotLibrary.Count));

            otherDevice.UpdateClient();
            otherClient.Received().SendSnapshotListDescriptor(Arg.Is<SnapshotListDescriptor>(snapshots => snapshots.Snapshots.Length == m_SnapshotLibrary.Count));

            GameObject.DestroyImmediate(otherDevice.gameObject);
            GameObject.DestroyImmediate(otherActor.gameObject);
        }

        [Test]
        public void MigrateObsoleteSnapshotsIntoExistingLibrary()
        {
            var serializedObject = new SerializedObject(m_Device);
            var obsoleteSnapshots = serializedObject.FindProperty("m_Snapshots");

            obsoleteSnapshots.arraySize = 3;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var newSnapshotLibrary = ScriptableObject.CreateInstance<SnapshotLibrary>();
            m_Device.SnapshotLibrary = newSnapshotLibrary;

            SnapshotLibraryUtility.MigrateSnapshotsToSnapshotLibrary(m_Device);

            Assert.AreEqual(0, m_Device.m_Snapshots.Count);
            Assert.AreEqual(3, newSnapshotLibrary.Count);

            ScriptableObject.DestroyImmediate(newSnapshotLibrary);
        }

        [Test]
        public void MigrateObsoleteSnapshotsWithoutLibrarySet_CreatesSnapshotLibrary()
        {
            var serializedObject = new SerializedObject(m_Device);
            var obsoleteSnapshots = serializedObject.FindProperty("m_Snapshots");

            obsoleteSnapshots.arraySize = 3;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            SnapshotLibraryUtility.MigrateSnapshotsToSnapshotLibrary(m_Device);

            Assert.AreEqual(0, m_Device.m_Snapshots.Count);
            Assert.IsNotNull(m_Device.SnapshotLibrary);
            Assert.AreEqual(3, m_Device.SnapshotLibrary.Count);

            ScriptableObject.DestroyImmediate(m_Device.SnapshotLibrary, true);
        }

        VirtualCameraDevice FindDeviceInHierarchy(GameObject root)
        {
            var device = root.GetComponentInChildren<VirtualCameraDevice>();

            Assert.IsNotNull(device, "Couldn't find a VirtualCameraDevice in the hierarchy");

            return device;
        }

        void ClearSnapshots(VirtualCameraDevice device)
        {
            var amountToDelete = device.GetSnapshotCount();
            while (amountToDelete > 0)
            {
                device.DeleteSnapshot(0);
                amountToDelete -= 1;
            }
            Assert.AreEqual(0, device.GetSnapshotCount(), "Couldn't clear snapshots");
        }

        [UnityTest]
        public IEnumerator SnapshotsPersistBetweenEditorModes()
        {
            var library = Resources.Load<SnapshotLibrary>("Snapshot/SnapshotLibrary");

            library.Clear();

            Assert.AreEqual(0, library.Count, "Incorrect snapshot count.");

            yield return new EnterPlayMode();

            library = Resources.Load<SnapshotLibrary>("Snapshot/SnapshotLibrary");

            m_Device.SnapshotLibrary = library;
            m_Device.TakeSnapshot();

            yield return new ExitPlayMode();

            library = Resources.Load<SnapshotLibrary>("Snapshot/SnapshotLibrary");

            Assert.AreEqual(1, library.Count, "Not all snapshots persisted after exiting Play Mode");

            library.Clear();
        }
    }
}
