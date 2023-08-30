using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    public class LiveStreamTests
    {
        [Test]
        public void LiveStream_CreateProperty_CreatesAProperty()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            Assert.IsTrue(stream.TryGetProperty(handle, out var property), "The handle should be valid.");
            Assert.IsNotNull(property, "The property should not be null.");
        }

        [Test]
        public void LiveStream_RemoveProperty_RemovesAProperty()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.RemoveProperty(handle);

            Assert.IsFalse(stream.TryGetProperty(handle, out var property), "The handle should not be valid.");
            Assert.IsNull(property, "The property should be null.");
        }

        [Test]
        public void LiveStream_TryGetValue_ReturnsTheValue()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var value = new Vector3(1, 2, 3);

            stream.SetValue(handle, value);
            stream.ApplyValues();

            Assert.IsTrue(stream.TryGetValue<Vector3>(handle, out var result), "The value should be returned.");
            Assert.AreEqual(value, result, "The value should be correct.");
        }

        [Test]
        public void LiveStream_SetValue_SetsTheValue()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var gameObject = new GameObject();
            var transform = gameObject.transform;
            var value = new Vector3(1, 2, 3);

            stream.Rebind(transform);
            stream.SetValue(handle, value);
            stream.ApplyValues();

            Assert.AreEqual(value, transform.localPosition, "The value should be set.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStream_ValueIsSetAfterApplyValues()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var gameObject = new GameObject();
            var transform = gameObject.transform;
            var value = new Vector3(1, 2, 3);

            stream.Rebind(transform);
            stream.ApplyValues();

            Assert.AreEqual(Vector3.zero, transform.localPosition, "The value should not be set.");

            stream.SetValue(handle, value);
            stream.ApplyValues();

            Assert.AreEqual(value, transform.localPosition, "The value should be set.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStream_SetMaxError_SetsTheMaxErrorInPropertyCurve()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.SetMaxError(handle, 0.1f);
            stream.TryGetProperty(handle, out var property);

            var reduceableCurve = property.Curve as IReduceableCurve;

            Assert.AreEqual(0.1f, reduceableCurve.MaxError, "The max error should be set.");
        }

        [Test]
        public void LiveStream_RecordsValues()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.SetFrameRate(new FrameRate(1));
            stream.SetValue(handle, new Vector3(1, 2, 3));
            stream.Record(0f);
            stream.SetValue(handle, new Vector3(4, 5, 6));
            stream.Record(1f);

            var clip = stream.Bake();

            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            var curveX = AnimationUtility.GetEditorCurve(clip, curveBindings[0]);
            var curveY = AnimationUtility.GetEditorCurve(clip, curveBindings[1]);
            var curveZ = AnimationUtility.GetEditorCurve(clip, curveBindings[2]);

            Assert.AreEqual(3, curveBindings.Length, "There should be 3 curves.");
            Assert.AreEqual(2, curveX.length, "There should be 2 keys.");
            Assert.AreEqual(2, curveY.length, "There should be 2 keys.");
            Assert.AreEqual(2, curveZ.length, "There should be 2 keys.");

            Assert.AreEqual(0f, curveX.keys[0].time, "The first key should be at time 0.");
            Assert.AreEqual(1f, curveX.keys[1].time, "The second key should be at time 1.");
            Assert.AreEqual(0f, curveY.keys[0].time, "The first key should be at time 1.");
            Assert.AreEqual(1f, curveY.keys[1].time, "The second key should be at time 4.");
            Assert.AreEqual(0f, curveZ.keys[0].time, "The first key should be at time 2.");
            Assert.AreEqual(1f, curveZ.keys[1].time, "The second key should be at time 5.");

            Assert.AreEqual(1f, curveX.keys[0].value, "The first key should be at value 1.");
            Assert.AreEqual(4f, curveX.keys[1].value, "The second key should be at value 4.");
            Assert.AreEqual(2f, curveY.keys[0].value, "The first key should be at value 2.");
            Assert.AreEqual(5f, curveY.keys[1].value, "The second key should be at value 5.");
            Assert.AreEqual(3f, curveZ.keys[0].value, "The first key should be at value 3.");
            Assert.AreEqual(6f, curveZ.keys[1].value, "The second key should be at value 6.");
        }

        [Test]
        public void LiveStream_Reset_ClearsTheStream()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.Reset();

            Assert.IsFalse(stream.TryGetProperty(handle, out var property), "The handle should not be valid.");
        }

        [Test]
        public void LiveStream_TryGetHandle_ReturnsTheHandle()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var binding = new PropertyBinding(string.Empty, "m_LocalPosition", typeof(Transform));

            Assert.IsTrue(stream.TryGetHandle(binding, out var handle2), "The handle should be valid.");
            Assert.AreEqual(handle, handle2, "The handles should be equal.");
        }

        [Test]
        public void LiveStream_TryGetHandle_ReturnsFalseIfTheHandleIsInvalid()
        {
            var stream = new LiveStream();
            var binding = new PropertyBinding(string.Empty, "m_LocalPosition", typeof(Transform));

            Assert.IsFalse(stream.TryGetHandle(binding, out var handle), "The handle should not be valid.");
        }

        [Test]
        public void LiveStream_Rebind_RebindsTheStream()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var gameObject = new GameObject();
            var transform = gameObject.transform;
            var value = new Vector3(1, 2, 3);

            stream.Rebind(transform);
            stream.SetValue(handle, value);
            stream.ApplyValues();

            Assert.AreEqual(value, transform.localPosition, "The value should be set.");

            GameObject.DestroyImmediate(gameObject);

            gameObject = new GameObject();
            transform = gameObject.transform;

            stream.Rebind(transform);
            stream.ApplyValues();

            Assert.AreEqual(value, transform.localPosition, "The value should be set.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveStream_SetFrameRate_SetsTheFrameRate()
        {
            var stream = new LiveStream();
            var frameRate = new FrameRate(1);

            stream.SetFrameRate(frameRate);

            Assert.AreEqual(frameRate, stream.FrameRate, "The frame rate should be set.");
        }

        [Test]
        public void LiveStream_SetFrameRate_SetsFrameRateInCreatedProperties()
        {
            var stream = new LiveStream();
            var frameRate = new FrameRate(1);
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.SetFrameRate(frameRate);
            stream.TryGetProperty(handle, out var property);

            Assert.AreEqual(frameRate, property.Curve.FrameRate, "The frame rate should be set.");
        }

        [Test]
        public void LiveStream_IsHandleValid_ChecksIfTheHandleIsValid()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            Assert.IsTrue(stream.IsHandleValid(handle), "The handle should be valid.");
        }

        [Test]
        public void LiveStream_IsHandleValid_ReturnsFalseIfTheHandleIsInvalid()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.RemoveProperty(handle);

            Assert.IsFalse(stream.IsHandleValid(handle), "The handle should not be valid.");
        }

        [Test]
        public void LiveStream_ClearCurves_ClearsTheCurveKeyframes()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var value = new Vector3(1, 2, 3);

            stream.SetValue(handle, value);
            stream.Record(0f);
            stream.ClearCurves();

            var clip = stream.Bake();
            var curveBindings = AnimationUtility.GetCurveBindings(clip);

            Assert.AreEqual(0, curveBindings.Length, "There should be no curves.");
        }

        [Test]
        public void LiveStream_Bake_CreatesAnAnimationClip_WithFrameRate()
        {
            var stream = new LiveStream();
            var frameRate = new FrameRate(1);
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");
            var value = new Vector3(1, 2, 3);

            stream.SetFrameRate(frameRate);
            stream.SetValue(handle, value);
            stream.Record(0f);

            var clip = stream.Bake();

            Assert.AreEqual(frameRate.AsFloat(), clip.frameRate, "The frame rate should be set.");
        }

        [Test]
        public void LiveStream_SetLive_SetsACreatedPropertyToLive()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.SetLive(handle, true);

            stream.TryGetProperty(handle, out var property);

            Assert.IsTrue(property.IsLive, "The property should be live.");
        }

        [Test]
        public void LiveStream_SetLive_SetsACreatedPropertyToNotLive()
        {
            var stream = new LiveStream();
            var handle = stream.CreateProperty<Transform, Vector3>("m_LocalPosition");

            stream.SetLive(handle, false);

            stream.TryGetProperty(handle, out var property);

            Assert.IsFalse(property.IsLive, "The property should not be live.");
        }
    }
}
