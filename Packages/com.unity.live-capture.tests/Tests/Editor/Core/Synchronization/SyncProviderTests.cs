using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    class SyncProviderTests
    {
        class TestSyncProvider : SyncProvider
        {
            /// <inheritdoc />
            public override string Name => "TestProvider";

            /// <inheritdoc />
            public override FrameRate SyncRate => StandardFrameRate.FPS_60_00.ToValue();

            protected override bool RunInEditMode => RunInEditModeValue;

            public bool RunInEditModeValue = true;

            public bool NextOnWaitForNextPulseSuccess = true;
            public int NextOnWaitForNextPulseCount = 1;

            /// <inheritdoc />
            protected override bool OnWaitForNextPulse(out int pulseCount)
            {
                pulseCount = NextOnWaitForNextPulseCount;
                return NextOnWaitForNextPulseSuccess;
            }
        }

        int m_vSyncCount;

        [SetUp]
        public void SetUp()
        {
            m_vSyncCount = QualitySettings.vSyncCount;
            QualitySettings.vSyncCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            QualitySettings.vSyncCount = m_vSyncCount;
        }

        [Test]
        public void TestRunInEditModePreventsStartInEditMode()
        {
            var provider = new TestSyncProvider
            {
                RunInEditModeValue = false,
            } as ISyncProvider;

            Assert.AreEqual(SyncStatus.Stopped, provider.Status);

            provider.StartSynchronizing();

            Assert.AreEqual(SyncStatus.Stopped, provider.Status);
        }

        [UnityTest]
        public IEnumerator TestRunInEditModePermitsStartInPlayMode()
        {
            yield return new EnterPlayMode();

            var provider = new TestSyncProvider
            {
                RunInEditModeValue = false,
            } as ISyncProvider;

            Assert.AreEqual(SyncStatus.Stopped, provider.Status);

            provider.StartSynchronizing();

            Assert.AreEqual(SyncStatus.Synchronized, provider.Status);

            yield return new ExitPlayMode();
        }

        [Test]
        public void TestStartSetsApplicationFrameRate()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            Application.targetFrameRate = 60;

            provider.StartSynchronizing();

            Assert.AreEqual(-1, Application.targetFrameRate);
        }

        [Test]
        public void TestStopWhenNotRunning()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            Assert.AreEqual(SyncStatus.Stopped, provider.Status);

            Assert.DoesNotThrow(() => provider.StopSynchronizing());

            Assert.AreEqual(SyncStatus.Stopped, provider.Status);
        }

        [Test]
        public void TestStop()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            provider.StartSynchronizing();

            Assert.AreEqual(SyncStatus.Synchronized, provider.Status);

            provider.StopSynchronizing();

            Assert.AreEqual(SyncStatus.Stopped, provider.Status);
        }

        [Test]
        public void TestWaitForNextPulseStopped()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            Assert.IsFalse(provider.WaitForNextPulse());
        }

        [Test]
        public void TestWaitForNextPulseSuccess()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            provider.StartSynchronizing();

            Assert.IsTrue(provider.WaitForNextPulse());

            Assert.AreEqual(SyncStatus.Synchronized, provider.Status);
            Assert.AreEqual(1, provider.LastPulseCountDelta);
        }

        [Test]
        public void TestWaitForNextPulseRecoversFromNotSynchronized()
        {
            var provider = new TestSyncProvider();

            (provider as ISyncProvider).StartSynchronizing();

            provider.NextOnWaitForNextPulseSuccess = false;

            Assert.IsFalse((provider as ISyncProvider).WaitForNextPulse());

            Assert.AreEqual(SyncStatus.NotSynchronized, provider.Status);

            provider.NextOnWaitForNextPulseSuccess = true;

            Assert.IsTrue((provider as ISyncProvider).WaitForNextPulse());

            Assert.AreEqual(SyncStatus.Synchronized, provider.Status);
        }

        [Test]
        public void TestWaitForNextPulseDetectsVsyncOn()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            provider.StartSynchronizing();

            Assert.IsTrue(provider.WaitForNextPulse());

            Assert.IsTrue(provider.WaitForNextPulse());

            QualitySettings.vSyncCount = 1;

            Assert.False(provider.WaitForNextPulse());
        }

        [Test]
        public void TestWaitForNextPulseDetectsCaptureDeltaTimeChanges()
        {
            var provider = new TestSyncProvider() as ISyncProvider;

            provider.StartSynchronizing();

            Assert.IsTrue(provider.WaitForNextPulse());

            Assert.IsTrue(provider.WaitForNextPulse());

            Time.captureDeltaTime = 1f / 20;

            Assert.False(provider.WaitForNextPulse());
        }

        [Test, Ignore("Unsable in CI")]
        public void TestWaitForNextPulseDropFrames()
        {
            var provider = new TestSyncProvider();

            (provider as ISyncProvider).StartSynchronizing();

            provider.NextOnWaitForNextPulseCount = 3;

            Assert.True((provider as ISyncProvider).WaitForNextPulse());

            Assert.AreEqual(SyncStatus.Synchronized, provider.Status);
            Assert.AreEqual(2, provider.DroppedFrameCount);

            Assert.True((provider as ISyncProvider).WaitForNextPulse());

            Assert.AreEqual(SyncStatus.Synchronized, provider.Status);
            Assert.AreEqual(4, provider.DroppedFrameCount);

        }
    }
}
