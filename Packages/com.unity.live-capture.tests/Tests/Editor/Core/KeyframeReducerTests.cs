using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.Tests.Editor
{
    public class KeyframeReducerTests
    {
        IEnumerable<T> ToEnumerable<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        [Test]
        public void FirstKeyIsNotReduced()
        {
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => true);

            for (var i = 0; i < 10; ++i)
                reducer.Add(i);

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(1, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");
        }

        [Test]
        public void LastKeyIsNotReduced()
        {
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => true);

            for (var i = 0; i < 10; ++i)
                reducer.Add(i);

            reducer.Flush();

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(2, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");
            Assert.AreEqual(9, values[1], "Wrong value");
        }

        [Test]
        public void FlushOutputsLastKey()
        {
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => true);

            for (var i = 0; i < 10; ++i)
                reducer.Add(i);

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(1, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");

            reducer.Flush();

            values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(1, values.Length, "Incorrect output count");
            Assert.AreEqual(9, values[0], "Wrong value");
        }

        [Test]
        public void ReduceInitialKeys()
        {
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => (int)(c.Args()[0]) < 5);

            for (var i = 0; i < 10; ++i)
                reducer.Add(i);

            reducer.Flush();

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(6, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");
            Assert.AreEqual(5, values[1], "Wrong value");
            Assert.AreEqual(6, values[2], "Wrong value");
            Assert.AreEqual(7, values[3], "Wrong value");
            Assert.AreEqual(8, values[4], "Wrong value");
            Assert.AreEqual(9, values[5], "Wrong value");
        }

        [Test]
        public void ReduceFinalKeys()
        {
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => (int)(c.Args()[0]) >= 5);

            for (var i = 0; i < 10; ++i)
                reducer.Add(i);

            reducer.Flush();

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(6, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");
            Assert.AreEqual(1, values[1], "Wrong value");
            Assert.AreEqual(2, values[2], "Wrong value");
            Assert.AreEqual(3, values[3], "Wrong value");
            Assert.AreEqual(4, values[4], "Wrong value");
            Assert.AreEqual(9, values[5], "Wrong value");
        }

        [Test]
        public void EnumerationStopsWhenNonReduceableKeyIsFound()
        {
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => (int)(c.Args()[0]) != 5);

            for (var i = 0; i < 10; ++i)
                reducer.Add(i);

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(2, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");
            Assert.AreEqual(5, values[1], "Wrong value");

            reducer.Flush();

            values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(1, values.Length, "Incorrect output count");
            Assert.AreEqual(9, values[0], "Wrong value");
        }

        [Test]
        public void UsesMaxSearchWindow()
        {
            var maxSearch = 5;
            var impl = Substitute.For<IKeyframeReducerImpl<int>>();
            var reducer = new KeyframeReducer<int>(impl, maxSearch);

            impl.CanReduce(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns((c) => true);

            for (var i = 0; i < maxSearch * 3; ++i)
                reducer.Add(i);

            var values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(1, values.Length, "Incorrect output count");
            Assert.AreEqual(0, values[0], "Wrong value");

            reducer.Flush();

            values = ToEnumerable(reducer).ToArray();

            Assert.AreEqual(1, values.Length, "Incorrect output count");
            Assert.AreEqual(maxSearch * 3 - 1, values[0], "Wrong value");
        }
    }

    public class ReduceFloatCurveTests
    {
        static IEnumerable<TestCaseData> Clips
        {
            get
            {
                var frameRate = StandardFrameRate.FPS_60_00.ToValue();

                yield return new TestCaseData("Reducer/Reduce01", "Reducer/Reduce01_E[0.1]", 0.1f, frameRate);
                yield return new TestCaseData("Reducer/Reduce01", "Reducer/Reduce01_E[0.5]", 0.5f, frameRate);
                yield return new TestCaseData("Reducer/Reduce02", "Reducer/Reduce02_E[0.1]", 0.1f, frameRate);
                yield return new TestCaseData("Reducer/Reduce02", "Reducer/Reduce02_E[0.5]", 0.5f, frameRate);
            }
        }

        [TestCaseSource(nameof(Clips))]
        public void FloatCurve(string path, string reducedPath, float maxError, FrameRate frameRate)
        {
            var comparer = new FloatEqualityComparer(0.0002f);
            var referenceClip = Resources.Load<AnimationClip>(path);
            var referenceCurves = GetCurves(referenceClip);
            var reducedClip = Resources.Load<AnimationClip>(reducedPath);
            var reducedCurves = GetCurves(reducedClip);
            var referenceCurve = referenceCurves[0];
            var reducedCurve = reducedCurves[0];
            var curve = new FloatCurve();
            curve.MaxError = maxError / 100f;

            for (var i = 0; i < referenceCurve.length; ++i)
            {
                var key = referenceCurve[i];
                curve.AddKey(key.time, key.value);
            }

            var outputClip = new AnimationClip();

            curve.SetToAnimationClip(new PropertyBinding(string.Empty, "m_FloatReduced", typeof(TestBehaviour)), outputClip);

            var outputCuve = GetCurves(outputClip)[0];

            var deltaTime = (float)frameRate.FrameInterval;
            var maxTime = reducedCurve[reducedCurve.length-1].time;
            var time = 0f;

            while (time < maxTime)
            {
                var reducedRefValue = reducedCurve.Evaluate(time);
                var reducedValue = outputCuve.Evaluate(time);

                Assert.That(reducedValue, Is.EqualTo(reducedRefValue).Using(comparer), $"Incorrect reduced value at time: {time}");

                time += deltaTime;
            }

            GameObject.DestroyImmediate(outputClip);
        }

        static AnimationCurve[] GetCurves(AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();
        }
    }

    public class ReduceVector2CurveTests
    {
        static IEnumerable<TestCaseData> Clips
        {
            get
            {
                var frameRate = StandardFrameRate.FPS_60_00.ToValue();

                yield return new TestCaseData("Reducer/Reduce04", "Reducer/Reduce04_Vector2_E[0.1]", 0.1f, frameRate);
                yield return new TestCaseData("Reducer/Reduce04", "Reducer/Reduce04_Vector2_E[0.5]", 0.5f, frameRate);
            }
        }

        [TestCaseSource(nameof(Clips))]
        public void Vector2Curve(string path, string reducedPath, float maxError, FrameRate frameRate)
        {
            var comparer = new FloatEqualityComparer(0.0001f);
            var referenceClip = Resources.Load<AnimationClip>(path);
            var referenceCurves = GetCurves(referenceClip);
            var reducedClip = Resources.Load<AnimationClip>(reducedPath);
            var reducedCurves = GetCurves(reducedClip);
            var referenceCurveX = referenceCurves[0];
            var referenceCurveY = referenceCurves[1];
            var curve = new Vector2Curve();
            curve.MaxError = maxError / 100f;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var vec3 = new Vector2(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value);
                curve.AddKey(referenceCurveX[i].time, vec3);
            }

            var outputClip = new AnimationClip();

            curve.SetToAnimationClip(new PropertyBinding(string.Empty, "m_Vector2Reduced", typeof(TestBehaviour)), outputClip);

            var outputCurves = GetCurves(outputClip);
            var deltaTime = (float)frameRate.FrameInterval;
            var maxTime = reducedCurves[0][reducedCurves[0].length-1].time;
            var time = 0f;

            while (time < maxTime)
            {
                for (var i = 0; i < 2; ++i)
                {
                    var reducedRefValue = reducedCurves[i].Evaluate(time);
                    var reducedValue = outputCurves[i].Evaluate(time);

                    Assert.That(reducedValue, Is.EqualTo(reducedRefValue).Using(comparer), $"Incorrect reduced value at time: {time}");
                }
                
                time += deltaTime;
            }

            GameObject.DestroyImmediate(outputClip);
        }

        static AnimationCurve[] GetCurves(AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();
        }
    }

    public class ReduceVector3CurveTests
    {
        static IEnumerable<TestCaseData> Clips
        {
            get
            {
                var frameRate = StandardFrameRate.FPS_60_00.ToValue();

                yield return new TestCaseData("Reducer/Reduce04", "Reducer/Reduce04_E[0.1]", 0.1f, frameRate);
                yield return new TestCaseData("Reducer/Reduce04", "Reducer/Reduce04_E[0.5]", 0.5f, frameRate);
            }
        }

        [TestCaseSource(nameof(Clips))]
        public void Vector3Curve(string path, string reducedPath, float maxError, FrameRate frameRate)
        {
            var comparer = new FloatEqualityComparer(0.0001f);
            var referenceClip = Resources.Load<AnimationClip>(path);
            var referenceCurves = GetCurves(referenceClip);
            var reducedClip = Resources.Load<AnimationClip>(reducedPath);
            var reducedCurves = GetCurves(reducedClip);
            var referenceCurveX = referenceCurves[0];
            var referenceCurveY = referenceCurves[1];
            var referenceCurveZ = referenceCurves[2];
            var curve = new Vector3Curve();
            curve.MaxError = maxError / 100f;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveZ.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var vec3 = new Vector3(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value,
                    referenceCurveZ[i].value);
                curve.AddKey(referenceCurveX[i].time, vec3);
            }

            var outputClip = new AnimationClip();

            curve.SetToAnimationClip(new PropertyBinding(string.Empty, "m_Vector3Reduced", typeof(TestBehaviour)), outputClip);

            var outputCurves = GetCurves(outputClip);
            var deltaTime = (float)frameRate.FrameInterval;
            var maxTime = reducedCurves[0][reducedCurves[0].length-1].time;
            var time = 0f;

            while (time < maxTime)
            {
                for (var i = 0; i < 3; ++i)
                {
                    var reducedRefValue = reducedCurves[i].Evaluate(time);
                    var reducedValue = outputCurves[i].Evaluate(time);

                    Assert.That(reducedValue, Is.EqualTo(reducedRefValue).Using(comparer), $"Incorrect reduced value at time: {time}");
                }
                
                time += deltaTime;
            }

            GameObject.DestroyImmediate(outputClip);
        }

        static AnimationCurve[] GetCurves(AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();
        }
    }

    public class ReduceVector4CurveTests
    {
        static IEnumerable<TestCaseData> Clips
        {
            get
            {
                var frameRate = StandardFrameRate.FPS_60_00.ToValue();

                yield return new TestCaseData("Reducer/Reduce05", "Reducer/Reduce05_E[0.1]", 0.1f, frameRate);
                yield return new TestCaseData("Reducer/Reduce05", "Reducer/Reduce05_E[0.5]", 0.5f, frameRate);
            }
        }

        [TestCaseSource(nameof(Clips))]
        public void Vector4Curve(string path, string reducedPath, float maxError, FrameRate frameRate)
        {
            var comparer = new FloatEqualityComparer(0.0001f);
            var referenceClip = Resources.Load<AnimationClip>(path);
            var referenceCurves = GetCurves(referenceClip);
            var reducedClip = Resources.Load<AnimationClip>(reducedPath);
            var reducedCurves = GetCurves(reducedClip);
            var referenceCurveX = referenceCurves[0];
            var referenceCurveY = referenceCurves[1];
            var referenceCurveZ = referenceCurves[2];
            var referenceCurveW = referenceCurves[3];
            var curve = new Vector4Curve();
            curve.MaxError = maxError / 100f;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveZ.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveW.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var vec3 = new Vector4(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value,
                    referenceCurveZ[i].value,
                    referenceCurveW[i].value);
                curve.AddKey(referenceCurveX[i].time, vec3);
            }

            var outputClip = new AnimationClip();

            curve.SetToAnimationClip(new PropertyBinding(string.Empty, "m_Vector4Reduced", typeof(TestBehaviour)), outputClip);

            var outputCurves = GetCurves(outputClip);
            var deltaTime = (float)frameRate.FrameInterval;
            var maxTime = reducedCurves[0][reducedCurves[0].length-1].time;
            var time = 0f;

            while (time < maxTime)
            {
                for (var i = 0; i < 4; ++i)
                {
                    var reducedRefValue = reducedCurves[i].Evaluate(time);
                    var reducedValue = outputCurves[i].Evaluate(time);

                    Assert.That(reducedValue, Is.EqualTo(reducedRefValue).Using(comparer), $"Incorrect reduced value at time: {time}");
                }
                
                time += deltaTime;
            }

            GameObject.DestroyImmediate(outputClip);
        }

        static AnimationCurve[] GetCurves(AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();
        }
    }

    public class ReduceEulerCurveTests
    {
        static IEnumerable<TestCaseData> Clips
        {
            get
            {
                var fps24 = StandardFrameRate.FPS_24_00.ToValue();
                var fps60 = StandardFrameRate.FPS_60_00.ToValue();

                yield return new TestCaseData("Reducer/Reduce03", "Reducer/Reduce03_E[0.1]", 0.1f, fps60);
                yield return new TestCaseData("Reducer/Reduce03", "Reducer/Reduce03_E[0.5]", 0.5f, fps60);
                yield return new TestCaseData("Reducer/Reduce06", "Reducer/Reduce06_E[0]", 0f, fps24);
                yield return new TestCaseData("Reducer/Reduce06", "Reducer/Reduce06_E[0.001]", 0.001f, fps24);
                yield return new TestCaseData("Reducer/Reduce06", "Reducer/Reduce06_E[0.01]", 0.01f, fps24);
                yield return new TestCaseData("Reducer/Reduce06", "Reducer/Reduce06_E[0.1]", 0.1f, fps24);
            }
        }

        [TestCaseSource(nameof(Clips))]
        public void EulerCurve(string path, string reducedPath, float maxError, FrameRate frameRate)
        {
            var comparer = new FloatEqualityComparer(1f);
            var referenceClip = Resources.Load<AnimationClip>(path);
            var referenceCurves = GetCurves(referenceClip);
            var reducedClip = Resources.Load<AnimationClip>(reducedPath);
            var reducedCurves = GetCurves(reducedClip);
            var referenceCurveX = referenceCurves[0];
            var referenceCurveY = referenceCurves[1];
            var referenceCurveZ = referenceCurves[2];
            var curve = new EulerCurve();
            curve.MaxError = maxError;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveZ.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var quat = Quaternion.Euler(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value,
                    referenceCurveZ[i].value);
                curve.AddKey(referenceCurveX[i].time, quat);
            }

            var outputClip = new AnimationClip();

            curve.SetToAnimationClip(new PropertyBinding(string.Empty, "m_EulerReduced", typeof(TestBehaviour)), outputClip);

            var outputCurves = GetCurves(outputClip);
            var deltaTime = (float)frameRate.FrameInterval;
            var maxTime = reducedCurves[0][reducedCurves[0].length-1].time;
            var time = 0f;

            while (time < maxTime)
            {
                var reducedRef = Quaternion.Euler(
                    reducedCurves[0].Evaluate(time),
                    reducedCurves[1].Evaluate(time),
                    reducedCurves[2].Evaluate(time));

                var reducedValue = Quaternion.Euler(
                    outputCurves[0].Evaluate(time),
                    outputCurves[1].Evaluate(time),
                    outputCurves[2].Evaluate(time));

                var angle = Quaternion.Angle(reducedRef, reducedValue);
                
                Assert.That(angle, Is.EqualTo(0f).Using(comparer), $"Incorrect reduced value at time: {time}");
                time += deltaTime;
            }

            GameObject.DestroyImmediate(outputClip);
        }

        static AnimationCurve[] GetCurves(AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();
        }
    }

    /*
    public class KeyframeReducerGenerationTests
    {
        static IEnumerable<TestCaseData> FloatClipsForGeneration
        {
            get
            {
                yield return new TestCaseData("Reducer/Reduce01", 0.1f);
                yield return new TestCaseData("Reducer/Reduce01", 0.5f);
                yield return new TestCaseData("Reducer/Reduce02", 0.1f);
                yield return new TestCaseData("Reducer/Reduce02", 0.5f);
            }
        }

        // Use this test localy to generate clips for testing purposes.
        [TestCaseSource(nameof(FloatClipsForGeneration))]
        public void GenerateFloatClip(string path, float maxError)
        {
            KeyframeReducerTestGenerator.GenerateFloatTestClip(path, maxError);
        }

        static IEnumerable<TestCaseData> Vector2ClipsForGeneration
        {
            get
            {
                yield return new TestCaseData("Reducer/Reduce04", 0.1f);
                yield return new TestCaseData("Reducer/Reduce04", 0.5f);
            }
        }

        // Use this test localy to generate clips for testing purposes.
        [TestCaseSource(nameof(Vector2ClipsForGeneration))]
        public void GenerateVector2Clip(string path, float maxError)
        {
            KeyframeReducerTestGenerator.GenerateVector2TestClip(path, maxError);
        }

        static IEnumerable<TestCaseData> Vector3ClipsForGeneration
        {
            get
            {
                yield return new TestCaseData("Reducer/Reduce04", 0.1f);
                yield return new TestCaseData("Reducer/Reduce04", 0.5f);
            }
        }

        // Use this test localy to generate clips for testing purposes.
        [TestCaseSource(nameof(Vector3ClipsForGeneration))]
        public void GenerateVector3Clip(string path, float maxError)
        {
            KeyframeReducerTestGenerator.GenerateVector3TestClip(path, maxError);
        }

        static IEnumerable<TestCaseData> Vector4ClipsForGeneration
        {
            get
            {
                yield return new TestCaseData("Reducer/Reduce05", 0.1f);
                yield return new TestCaseData("Reducer/Reduce05", 0.5f);
            }
        }

        // Use this test localy to generate clips for testing purposes.
        [TestCaseSource(nameof(Vector4ClipsForGeneration))]
        public void GenerateVector4Clip(string path, float maxError)
        {
            KeyframeReducerTestGenerator.GenerateVector4TestClip(path, maxError);
        }

        static IEnumerable<TestCaseData> EulerClipsForGeneration
        {
            get
            {
                yield return new TestCaseData("Reducer/Reduce03", 0.1f);
                yield return new TestCaseData("Reducer/Reduce03", 0.5f);
                yield return new TestCaseData("Reducer/Reduce06", 0f);
                yield return new TestCaseData("Reducer/Reduce06", 0.001f);
                yield return new TestCaseData("Reducer/Reduce06", 0.01f);
                yield return new TestCaseData("Reducer/Reduce06", 0.1f);
            }
        }

        // Use this test localy to generate clips for testing purposes.
        [TestCaseSource(nameof(EulerClipsForGeneration))]
        public void GenerateEulerClip(string path, float maxError)
        {
            KeyframeReducerTestGenerator.GenerateEulerTestClip(path, maxError);
        }
    }

    static class KeyframeReducerTestGenerator
    {
        static AnimationCurve[] GetCurves(AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();
        }

        public static void GenerateFloatTestClip(string path, float maxError)
        {
            var clip = Resources.Load<AnimationClip>(path);
            var curves = GetCurves(clip);
            var referenceCurve = curves[0];
            var curve = new FloatCurve();

            curve.MaxError = maxError / 100f;

            for (var i = 0; i < referenceCurve.length; ++i)
            {
                var key = referenceCurve[i];
                curve.AddKey(key.time, key.value);
            }

            var outputClip = new AnimationClip();

            curve.SetToAnimationClip(new PropertyBinding(string.Empty, "m_FloatReduced", typeof(TestBehaviour)), outputClip);

            var outputPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{clip.name}_E[{maxError}].anim");
            AssetDatabase.CreateAsset(outputClip, outputPath);
        }

        public static void GenerateVector2TestClip(string path, float maxError)
        {
            var clip = Resources.Load<AnimationClip>(path);
            var curves = AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();

            var referenceCurveX = curves[0];
            var referenceCurveY = curves[1];
            var vector2Curve = new Vector2Curve();

            vector2Curve.MaxError = maxError / 100f;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var vec3 = new Vector2(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value);

                vector2Curve.AddKey(referenceCurveX[i].time, vec3);
            }

            var outputClip = new AnimationClip();

            vector2Curve.SetToAnimationClip(new PropertyBinding("", "m_Vector2Reduced", typeof(TestBehaviour)), outputClip);

            var outputPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{clip.name}_E[{maxError}].anim");
            AssetDatabase.CreateAsset(outputClip, outputPath);
        }

        public static void GenerateVector3TestClip(string path, float maxError)
        {
            var clip = Resources.Load<AnimationClip>(path);
            var curves = AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();

            var referenceCurveX = curves[0];
            var referenceCurveY = curves[1];
            var referenceCurveZ = curves[2];
            var vector3Curve = new Vector3Curve();

            vector3Curve.MaxError = maxError / 100f;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveZ.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var vec3 = new Vector3(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value,
                    referenceCurveZ[i].value);

                vector3Curve.AddKey(referenceCurveX[i].time, vec3);
            }

            var outputClip = new AnimationClip();

            vector3Curve.SetToAnimationClip(new PropertyBinding("", "m_Vector3Reduced", typeof(TestBehaviour)), outputClip);

            var outputPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{clip.name}_E[{maxError}].anim");
            AssetDatabase.CreateAsset(outputClip, outputPath);
        }

        public static void GenerateVector4TestClip(string path, float maxError)
        {
            var clip = Resources.Load<AnimationClip>(path);
            var curves = AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();

            var referenceCurveX = curves[0];
            var referenceCurveY = curves[1];
            var referenceCurveZ = curves[2];
            var referenceCurveW = curves[3];
            var vec4Curve = new Vector4Curve();

            vec4Curve.MaxError = maxError / 100f;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveZ.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveW.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var vec4 = new Vector4(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value,
                    referenceCurveZ[i].value,
                    referenceCurveW[i].value);

                vec4Curve.AddKey(referenceCurveX[i].time, vec4);
            }

            var outputClip = new AnimationClip();

            vec4Curve.SetToAnimationClip(new PropertyBinding("", "m_Vector4Reduced", typeof(TestBehaviour)), outputClip);

            var outputPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{clip.name}_E[{maxError}].anim");
            AssetDatabase.CreateAsset(outputClip, outputPath);
        }

        public static void GenerateEulerTestClip(string path, float maxError)
        {
            var clip = Resources.Load<AnimationClip>(path);
            var curves = AnimationUtility.GetCurveBindings(clip)
                .Select(b => AnimationUtility.GetEditorCurve(clip, b))
                .ToArray();

            var referenceCurveX = curves[0];
            var referenceCurveY = curves[1];
            var referenceCurveZ = curves[2];
            var eulerCurve = new EulerCurve();

            eulerCurve.MaxError = maxError;

            Assert.AreEqual(referenceCurveX.length, referenceCurveY.length, "Incorrect key count");
            Assert.AreEqual(referenceCurveX.length, referenceCurveZ.length, "Incorrect key count");

            for (var i = 0; i < referenceCurveX.length; ++i)
            {
                var quat = Quaternion.Euler(
                    referenceCurveX[i].value,
                    referenceCurveY[i].value,
                    referenceCurveZ[i].value);

                eulerCurve.AddKey(referenceCurveX[i].time, quat);
            }

            var outputClip = new AnimationClip();

            eulerCurve.SetToAnimationClip(new PropertyBinding("", "m_EulerReduced", typeof(TestBehaviour)), outputClip);

            var outputPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{clip.name}_E[{maxError}].anim");
            AssetDatabase.CreateAsset(outputClip, outputPath);
        }
    }
    */
}
