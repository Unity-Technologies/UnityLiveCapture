using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Unity.LiveCapture.LiveProperties;

namespace Unity.LiveCapture.Tests.Editor
{
    public class LivePropertyTests
    {
        class TestMonoBehaviour : MonoBehaviour
        {
            [SerializeField]
            float m_Property;

            public float Property => m_Property;
        }

        [Test]
        public void LiveProperty_WhenCreated_ThenHasCorrectBinding()
        {
            var propertyName = "m_LocalPosition";
            var relativePath = "Child/Other";
            var binding = new PropertyBinding(relativePath, propertyName, typeof(Transform));
            var liveProperty = new LiveProperty<Transform, Vector3>(relativePath, propertyName, null);

            Assert.AreEqual(binding, liveProperty.Binding, "The binding should be correct.");
        }

        [Test]
        public void LiveProperty_WhenCreated_TrowsExceptionIfBindingTypeMismatch()
        {
            var binding = new PropertyBinding(string.Empty, "m_LocalPosition", typeof(Transform));

            Assert.Throws<ArgumentException>(() => new LiveProperty<Camera, Vector3>(binding, null));
        }

        [Test]
        public void LiveProperty_WhenCreated_ThenHasCorrectCurve()
        {
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", null);

            Assert.IsInstanceOf<ICurve<Vector3>>(liveProperty.Curve, "The curve should be correct.");
        }

        [Test]
        public void LiveProperty_WhenCreated_ThenHasCorrectTarget()
        {
            var gameObject = new GameObject();
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", null);

            liveProperty.Rebind(gameObject.transform);

            Assert.AreEqual(gameObject.transform, liveProperty.Target, "The target should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveProperty_WhenCreated_ThenHasCorrectSetter()
        {
            var gameObject = new GameObject();
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", null);

            liveProperty.Rebind(gameObject.transform);
            liveProperty.SetValue(Vector3.one);
            liveProperty.ApplyValue();

            Assert.AreEqual(Vector3.one, gameObject.transform.localPosition, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveProperty_WhenCreated_ThenHasCorrectSetterWithAction()
        {
            var gameObject = new GameObject();
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", (t, v) => t.localPosition = v);

            liveProperty.Rebind(gameObject.transform);
            liveProperty.SetValue(Vector3.one);
            liveProperty.ApplyValue();

            Assert.AreEqual(Vector3.one, gameObject.transform.localPosition, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveProperty_WhenCreated_ThenHasCorrectSetterWithActionAndNullTarget_DoesNotTrowException()
        {
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", (t, v) => t.localPosition = v);

            liveProperty.SetValue(Vector3.one);
            liveProperty.ApplyValue();
        }

        public void LiveProperty_ValueIsSetAfterCallingApplyValue()
        {
            var gameObject = new GameObject();
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", null);

            liveProperty.Rebind(gameObject.transform);
            liveProperty.SetValue(Vector3.one);

            Assert.AreEqual(Vector3.zero, gameObject.transform.localPosition, "The value should be correct.");

            liveProperty.ApplyValue();

            Assert.AreEqual(Vector3.one, gameObject.transform.localPosition, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void LiveProperty_RecordsValues()
        {
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", null);

            liveProperty.Curve.FrameRate = new FrameRate(1);
            liveProperty.SetValue(Vector3.one);
            liveProperty.Record(0f);
            liveProperty.SetValue(Vector3.zero);
            liveProperty.Record(1f);

            var clip = new AnimationClip();

            liveProperty.Curve.SetToAnimationClip(liveProperty.Binding, clip);

            var curveBindings = AnimationUtility.GetCurveBindings(clip);

            Assert.AreEqual(3, curveBindings.Length);

            var curveX = AnimationUtility.GetEditorCurve(clip, curveBindings[0]);
            var curveY = AnimationUtility.GetEditorCurve(clip, curveBindings[1]);
            var curveZ = AnimationUtility.GetEditorCurve(clip, curveBindings[2]);

            Assert.AreEqual(2, curveX.length, "The curve should have 2 keys.");
            Assert.AreEqual(2, curveY.length, "The curve should have 2 keys.");
            Assert.AreEqual(2, curveZ.length, "The curve should have 2 keys.");
            Assert.AreEqual(0f, curveX.keys[0].time, "The key should have the correct time.");
            Assert.AreEqual(1f, curveX.keys[1].time, "The key should have the correct time.");
            Assert.AreEqual(0f, curveY.keys[0].time, "The key should have the correct time.");
            Assert.AreEqual(1f, curveY.keys[1].time, "The key should have the correct time.");
            Assert.AreEqual(0f, curveZ.keys[0].time, "The key should have the correct time.");
            Assert.AreEqual(1f, curveZ.keys[1].time, "The key should have the correct time.");
            Assert.AreEqual(1f, curveX.keys[0].value, "The key should have the correct value.");
            Assert.AreEqual(0f, curveX.keys[1].value, "The key should have the correct value.");
            Assert.AreEqual(1f, curveY.keys[0].value, "The key should have the correct value.");
            Assert.AreEqual(0f, curveY.keys[1].value, "The key should have the correct value.");
            Assert.AreEqual(1f, curveZ.keys[0].value, "The key should have the correct value.");
            Assert.AreEqual(0f, curveZ.keys[1].value, "The key should have the correct value.");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void LiveProperty_WhenNotLive_DoesNotRecordValues()
        {
            var liveProperty = new LiveProperty<Transform, Vector3>(string.Empty, "m_LocalPosition", null);

            liveProperty.IsLive = false;
            liveProperty.SetValue(Vector3.one);
            liveProperty.Record(0f);
            liveProperty.SetValue(Vector3.zero);
            liveProperty.Record(1f);

            var clip = new AnimationClip();

            liveProperty.Curve.SetToAnimationClip(liveProperty.Binding, clip);

            var curveBindings = AnimationUtility.GetCurveBindings(clip);

            Assert.AreEqual(0, curveBindings.Length, "The clip should not have any curves.");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void LiveProperty_UsesGenericSetter()
        {
            var gameObject = new GameObject("Test", typeof(TestMonoBehaviour));
            var liveProperty = new LiveProperty<TestMonoBehaviour, float>(string.Empty, "m_Property", null);

            liveProperty.Rebind(gameObject.transform);
            liveProperty.SetValue(15f);
            liveProperty.ApplyValue();

            Assert.AreEqual(15f, gameObject.GetComponent<TestMonoBehaviour>().Property, "The value should be correct.");

            GameObject.DestroyImmediate(gameObject);
        }
    }
}
