using System;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.Cameras;

namespace Unity.LiveCapture.Tests.Editor
{
    public class CameraTrackingDeviceTests
    {
        class CameraTracker : CameraTrackingDevice
        {
            public event Action<int, FrameTime> FrameProcessed;
    
            public override bool IsReady()
            {
                return true;
            }

            public void InvokeAddFrame(int value, FrameTimeWithRate frameTime)
            {
                AddFrame(value, frameTime);
            }

            protected override ITimedDataBuffer CreateTimedDataBuffer()
            {
                return TimedDataBuffer.Create<int>();
            }

            protected override void ProcessFrame(LiveStream stream)
            {
                var value = GetCurrentFrame<int>();

                FrameProcessed?.Invoke(value, CurrentFrameTime.Value.Time);
            }
        }

        [Test]
        public void CameraTrackingDevice_WhenNotRecording_DoesNotProcessFramesInBetweenSamples()
        {
            var tracker = new GameObject().AddComponent<CameraTracker>();
            var frameTime = new FrameTimeWithRate(new FrameRate(30), new FrameTime(0));
            var processFrameCallCount = 0;

            tracker.FrameProcessed += (value, time) =>
            {
                processFrameCallCount++;
            };

            Assert.IsFalse(tracker.IsRecording, "Tracker should not be recording.");

            tracker.InvokeAddFrame(0, frameTime);
            frameTime++;
            // Skip a frame.
            frameTime++;
            tracker.InvokeAddFrame(2, frameTime);

            Assert.AreEqual(2, processFrameCallCount);

            GameObject.DestroyImmediate(tracker.gameObject);
        }

        [Test]
        public void CameraTrackingDevice_WhenRecording_CallsProcessFrame_ForEveryFrameTimeInBetweenSamples()
        {
            var tracker = new GameObject().AddComponent<CameraTracker>();
            var frameTime = new FrameTimeWithRate(new FrameRate(30), new FrameTime(0));
            var processFrameCallCount = 0;

            tracker.FrameProcessed += (value, time) =>
            {
                processFrameCallCount++;
            };

            tracker.InvokeStartRecording();

            Assert.IsTrue(tracker.IsRecording, "Tracker should be recording.");

            tracker.InvokeAddFrame(0, frameTime);
            frameTime++;
            // Skip a frame.
            frameTime++;
            tracker.InvokeAddFrame(2, frameTime);

            Assert.AreEqual(3, processFrameCallCount);

            GameObject.DestroyImmediate(tracker.gameObject);
        }

        [Test]
        public void CameraTrackingDevice_AddFrameCalledFromProcessFrame_CantReenter()
        {
            var tracker = new GameObject().AddComponent<CameraTracker>();
            var frameTime = new FrameTimeWithRate(new FrameRate(30), new FrameTime(0));
            var processFrameCallCount = 0;

            tracker.FrameProcessed += (value, time) =>
            {
                processFrameCallCount++;

                tracker.InvokeAddFrame(100, frameTime++);
            };

            tracker.InvokeAddFrame(0, frameTime);

            Assert.AreEqual(1, processFrameCallCount);

            GameObject.DestroyImmediate(tracker.gameObject);
        }
    }
}
