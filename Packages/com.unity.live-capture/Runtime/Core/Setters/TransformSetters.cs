using UnityEngine;

namespace Unity.LiveCapture.Setters
{
    sealed class TransformLocalPositionSetter : Setter<Transform, Vector3>
    {
        public override string PropertyName => "m_LocalPosition";
        public override void Set(Transform transform, in Vector3 value) => transform.localPosition = value;
    }

    sealed class TransformLocalRotationSetter : Setter<Transform, Quaternion>
    {
        public override string PropertyName => "m_LocalRotation";
        public override void Set(Transform transform, in Quaternion value) => transform.localRotation = value;
    }

    sealed class TransformLocalEulerAnglesSetter : Setter<Transform, Quaternion>
    {
        public override string PropertyName => "m_LocalEuler";
        public override void Set(Transform transform, in Quaternion value) => transform.localEulerAngles = value.eulerAngles;
    }

    sealed class TransformLocalScaleSetter : Setter<Transform, Vector3>
    {
        public override string PropertyName => "m_LocalScale";
        public override void Set(Transform transform, in Vector3 value) => transform.localScale = value;
    }
}
