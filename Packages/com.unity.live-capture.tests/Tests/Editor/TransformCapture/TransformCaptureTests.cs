using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Unity.LiveCapture.TransformCapture;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TransformCaptureTests
    {
        const string tmpDir = "Assets/tmp";

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(tmpDir);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            AssetDatabase.Refresh();
        }

        [Test]
        public void GetActiveProjectPathDoesNotThrow()
        {
            Assert.DoesNotThrow(() => TransformCaptureUtility.GetActiveProjectPath());
        }

        [Test]
        public void CreateAvatarMaskWithNullRootThrows()
        {
            Assert.Throws<ArgumentNullException>(() => TransformCaptureUtility.CreateAvatarMask(null));
        }

        [Test]
        public void CreateAvatarMaskWithValidRootDoesNotThrow()
        {
            var go = new GameObject();
            var mask = default(AvatarMask);

            Assert.DoesNotThrow(() =>
            {
                mask = TransformCaptureUtility.CreateAvatarMask(go.transform);
            });

            Assert.NotNull(mask);

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(mask);
        }

        [Test]
        public void CreateAvatarMaskGathersChildTransforms()
        {
            var go = new GameObject();
            var mask = default(AvatarMask);

            var child0 = new GameObject();
            var child1 = new GameObject();
            var child2 = new GameObject();

            child0.transform.SetParent(go.transform);
            child1.transform.SetParent(child0.transform);
            child2.transform.SetParent(child1.transform);

            Assert.DoesNotThrow(() =>
            {
                mask = TransformCaptureUtility.CreateAvatarMask(go.transform);
            });

            Assert.NotNull(mask);
            Assert.AreEqual(4, mask.transformCount, "Incorrect transform count");
            Assert.AreEqual(go.transform, go.transform.Find(mask.GetTransformPath(0)), "Incorrect transform");
            Assert.AreEqual(child0.transform, go.transform.Find(mask.GetTransformPath(1)), "Incorrect transform");
            Assert.AreEqual(child1.transform, go.transform.Find(mask.GetTransformPath(2)), "Incorrect transform");
            Assert.AreEqual(child2.transform, go.transform.Find(mask.GetTransformPath(3)), "Incorrect transform");
            Assert.True(mask.GetTransformActive(0), "Incorrect active state");
            Assert.True(mask.GetTransformActive(1), "Incorrect active state");
            Assert.True(mask.GetTransformActive(2), "Incorrect active state");
            Assert.True(mask.GetTransformActive(3), "Incorrect active state");

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(mask);
        }

        [Test]
        public void CreateAssetWithInvalidArgsThrows()
        {
            var asset = new AnimationClip();

            Assert.Throws<ArgumentNullException>(() =>
                TransformCaptureUtility.CreateAssetUsingFilePanel<ScriptableObject>(null, string.Empty, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentException>(() =>
                TransformCaptureUtility.CreateAssetUsingFilePanel(asset, string.Empty, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentException>(() =>
                TransformCaptureUtility.CreateAssetUsingFilePanel(asset, "clip", string.Empty, string.Empty, string.Empty));
            Assert.Throws<ArgumentException>(() =>
                TransformCaptureUtility.CreateAssetUsingFilePanel(asset, "clip", "anim", string.Empty, string.Empty));
            Assert.Throws<ArgumentException>(() =>
                TransformCaptureUtility.CreateAssetUsingFilePanel(asset, "clip", "anim", "Assets", string.Empty));

            GameObject.DestroyImmediate(asset);
        }

        [Test]
        public void CreateOrReplaceAssetWithInvalidArgsThrows()
        {
            var asset = new AnimationClip();

            Assert.Throws<ArgumentNullException>(() =>
                TransformCaptureUtility.CreateOrReplaceAsset<ScriptableObject>(null, string.Empty));
            Assert.Throws<ArgumentException>(() =>
                TransformCaptureUtility.CreateOrReplaceAsset(asset, string.Empty));

            GameObject.DestroyImmediate(asset);
        }

        [Test]
        public void CreateOrReplaceAsset()
        {
            var clip = new AnimationClip();
            var path = $"{tmpDir}/clip.anim";

            var clip0 = TransformCaptureUtility.CreateOrReplaceAsset(clip, path);

            Assert.NotNull(clip0, "Invalid asset");
            Assert.AreEqual(clip, clip0, "Incorrect clip");
            Assert.True(AssetDatabase.IsMainAsset(clip0), "Incorrect asset");

            var replacement = new AnimationClip();
            replacement.SetCurve(string.Empty, typeof(GameObject), "m_Enabled", new AnimationCurve());

            var clip1 = TransformCaptureUtility.CreateOrReplaceAsset(replacement, path);

            Assert.True(replacement == null, "Replacement should have been destroyed through DestroyImmediate");
            Assert.AreEqual(clip1, clip0, "Incorrect clip");
            Assert.AreEqual(1, AnimationUtility.GetCurveBindings(clip1).Length, "Incorrect curve count");
            Assert.True(AssetDatabase.IsMainAsset(clip1), "Incorrect asset");
        }

        [Test]
        public void TransformRecorderRecordsUsingAvatarMask()
        {
            var go = new GameObject("root", typeof(Animator));
            var child0 = new GameObject();
            var child1 = new GameObject();
            var child2 = new GameObject();

            child0.transform.SetParent(go.transform);
            child1.transform.SetParent(child0.transform);
            child2.transform.SetParent(child1.transform);

            var mask = TransformCaptureUtility.CreateAvatarMask(go.transform);
            var recorder = new TransformRecorder();

            recorder.Prepare(go.GetComponent<Animator>(), mask, StandardFrameRate.FPS_30_00);

            go.transform.localPosition = Vector3.zero;
            child0.transform.localPosition = Vector3.zero;
            child1.transform.localPosition = Vector3.zero;
            child2.transform.localPosition = Vector3.zero;

            recorder.Record(0f);

            go.transform.localPosition = Vector3.one;
            child0.transform.localPosition = Vector3.one;
            child1.transform.localPosition = Vector3.one;
            child2.transform.localPosition = Vector3.one;

            recorder.Record(1f);

            var clip = new AnimationClip();

            recorder.SetToAnimationClip(clip);

            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(go, clip, 0f);

            Assert.AreEqual(Vector3.zero, go.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.zero, child0.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.zero, child1.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.zero, child2.transform.localPosition, "Incorrect position");

            AnimationMode.SampleAnimationClip(go, clip, 1f);

            Assert.AreEqual(Vector3.one, go.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child0.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child1.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child2.transform.localPosition, "Incorrect position");

            AnimationMode.StopAnimationMode();

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(mask);
            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void TransformRecorderDestroyTransformWhileRecording()
        {
            var go = new GameObject("root", typeof(Animator));
            var child0 = new GameObject();
            var child1 = new GameObject();
            var child2 = new GameObject();

            child0.transform.SetParent(go.transform);
            child1.transform.SetParent(child0.transform);
            child2.transform.SetParent(child1.transform);

            var mask = TransformCaptureUtility.CreateAvatarMask(go.transform);
            var recorder = new TransformRecorder();

            recorder.Prepare(go.GetComponent<Animator>(), mask, StandardFrameRate.FPS_30_00);

            go.transform.localPosition = Vector3.zero;
            child0.transform.localPosition = Vector3.zero;
            child1.transform.localPosition = Vector3.zero;
            child2.transform.localPosition = Vector3.zero;

            recorder.Record(0f);

            go.transform.localPosition = Vector3.one;
            child0.transform.localPosition = Vector3.one;

            GameObject.DestroyImmediate(child1);

            recorder.Record(1f);

            var clip = new AnimationClip();

            recorder.SetToAnimationClip(clip);

            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(go, clip, 0f);

            Assert.AreEqual(Vector3.zero, go.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.zero, child0.transform.localPosition, "Incorrect position");

            AnimationMode.SampleAnimationClip(go, clip, 1f);

            Assert.AreEqual(Vector3.one, go.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child0.transform.localPosition, "Incorrect position");

            AnimationMode.StopAnimationMode();

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(mask);
            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void TransformRecorderRecordsUsingAvatarMaskWithInactiveTransform()
        {
            var go = new GameObject("root", typeof(Animator));
            var child0 = new GameObject();
            var child1 = new GameObject();
            var child2 = new GameObject();

            child0.transform.SetParent(go.transform);
            child1.transform.SetParent(child0.transform);
            child2.transform.SetParent(child1.transform);

            var mask = TransformCaptureUtility.CreateAvatarMask(go.transform);
            var recorder = new TransformRecorder();

            // Disable child1.
            mask.SetTransformActive(2, false);

            recorder.Prepare(go.GetComponent<Animator>(), mask, StandardFrameRate.FPS_30_00);

            go.transform.localPosition = Vector3.zero;
            child0.transform.localPosition = Vector3.zero;
            child1.transform.localPosition = Vector3.zero;
            child2.transform.localPosition = Vector3.zero;

            recorder.Record(0f);

            go.transform.localPosition = Vector3.one;
            child0.transform.localPosition = Vector3.one;
            child1.transform.localPosition = Vector3.one;
            child2.transform.localPosition = Vector3.one;

            recorder.Record(1f);

            var clip = new AnimationClip();

            recorder.SetToAnimationClip(clip);

            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(go, clip, 0f);

            Assert.AreEqual(Vector3.zero, go.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.zero, child0.transform.localPosition, "Incorrect position");
            // child1 should not be animated and keep the latest value.
            Assert.AreEqual(Vector3.one, child1.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.zero, child2.transform.localPosition, "Incorrect position");

            AnimationMode.SampleAnimationClip(go, clip, 1f);

            Assert.AreEqual(Vector3.one, go.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child0.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child1.transform.localPosition, "Incorrect position");
            Assert.AreEqual(Vector3.one, child2.transform.localPosition, "Incorrect position");

            AnimationMode.StopAnimationMode();

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(mask);
            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void TransformRecorderRecordsWithoutAvatarMask()
        {
            var go = new GameObject("root", typeof(Animator));
            var recorder = new TransformRecorder();

            recorder.Prepare(go.GetComponent<Animator>(), null, StandardFrameRate.FPS_30_00);

            go.transform.localPosition = Vector3.zero;

            recorder.Record(0f);

            go.transform.localPosition = Vector3.one;

            recorder.Record(1f);

            var clip = new AnimationClip();

            recorder.SetToAnimationClip(clip);

            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(go, clip, 0f);

            Assert.AreEqual(Vector3.zero, go.transform.localPosition, "Incorrect position");

            AnimationMode.SampleAnimationClip(go, clip, 1f);

            Assert.AreEqual(Vector3.one, go.transform.localPosition, "Incorrect position");

            AnimationMode.StopAnimationMode();

            GameObject.DestroyImmediate(go);
            GameObject.DestroyImmediate(clip);
        }
    } 
}
