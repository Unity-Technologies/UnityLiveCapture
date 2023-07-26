using System;
using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    public class CurveFactoryTests
    {
        struct UnknownType { }
        struct KnownType { }

        class KnownTypeCurve : ICurve<KnownType>
        {
            public FrameRate FrameRate { get; set; }

            public void AddKey(double time, in KnownType value) { }
            public void Clear() { }
            public bool IsEmpty() => true;
            public void SetToAnimationClip(PropertyBinding binding, AnimationClip clip) { }
        }

        [Test]
        public void CurveFactory_ThrowsInvalidOperationExceptionWhenTypeIsNotSupported()
        {
            Assert.Throws<InvalidOperationException>(() => CurveFactory.CreateCurve<UnknownType>());
        }

        [Test]
        public void CurveFactory_CreatesCurveWithCommonValueTypes()
        {
            var floatCurve = CurveFactory.CreateCurve<float>();
            var intCurve = CurveFactory.CreateCurve<int>();
            var boolCurve = CurveFactory.CreateCurve<bool>();
            var vector2Curve = CurveFactory.CreateCurve<Vector2>();
            var vector3Curve = CurveFactory.CreateCurve<Vector3>();
            var vector4Curve = CurveFactory.CreateCurve<Vector4>();
            var quaternionCurve = CurveFactory.CreateCurve<Quaternion>();

            Assert.IsNotNull(floatCurve, "CurveFactory.CreateCurve<float>() returned null");
            Assert.IsNotNull(intCurve, "CurveFactory.CreateCurve<int>() returned null");
            Assert.IsNotNull(boolCurve, "CurveFactory.CreateCurve<bool>() returned null");
            Assert.IsNotNull(vector2Curve, "CurveFactory.CreateCurve<Vector2>() returned null");
            Assert.IsNotNull(vector3Curve, "CurveFactory.CreateCurve<Vector3>() returned null");
            Assert.IsNotNull(vector4Curve, "CurveFactory.CreateCurve<Vector4>() returned null");
            Assert.IsNotNull(quaternionCurve, "CurveFactory.CreateCurve<Quaternion>() returned null");
        }

        [Test]
        public void CurveFactory_CreatesCurveWithCustomType()
        {
            var curve = CurveFactory.CreateCurve<KnownType>();

            Assert.IsNotNull(curve, "CurveFactory.CreateCurve<KnownType>() returned null");
        }
    }
}
