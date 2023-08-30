using System;
using UnityEngine;

namespace Unity.LiveCapture.TransformCapture
{
    [Serializable]
    class TransformRecorder
    {
        [SerializeField]
        [Tooltip("The relative tolerance, in percent, for reducing position keyframes")]
        float m_PositionError = 0.5f;

        [SerializeField]
        [Tooltip("The tolerance, in degrees, for reducing rotation keyframes")]
        float m_RotationError = 0.5f;

        Transform[] m_Transforms;
        string[] m_Paths;
        Vector3Curve[] m_PositionCurves;
        EulerCurve[] m_RotationCurves;

        public float PositionError
        {
            get => m_PositionError;
            set => m_PositionError = value;
        }

        public float RotationError
        {
            get => m_RotationError;
            set => m_RotationError = value;
        }

        public void Validate()
        {
            m_PositionError = Mathf.Clamp(m_PositionError, 0f, 100f);
            m_RotationError = Mathf.Clamp(m_RotationError, 0f, 10f);
        }

        public void Prepare(Animator animator, AvatarMask avatarMask, FrameRate frameRate)
        {
            if (avatarMask == null)
            {
                PrepareRootTransform(animator, frameRate);
            }
            else
            {
                PrepareWithAvatarMask(animator, avatarMask, frameRate);
            }
        }

        public void SetToAnimationClip(AnimationClip animationClip)
        {
            if (m_Transforms == null)
                return;

            Debug.Assert(m_Transforms.Length == m_PositionCurves.Length);
            Debug.Assert(m_Transforms.Length == m_RotationCurves.Length);

            for (var i = 0; i < m_Transforms.Length; ++i)
            {
                var transform = m_Transforms[i];

                if (transform == null)
                    continue;

                m_PositionCurves[i].SetToAnimationClip(
                    new PropertyBinding(m_Paths[i], "m_LocalPosition", typeof(Transform)), animationClip);
                m_RotationCurves[i].SetToAnimationClip(
                    new PropertyBinding(m_Paths[i], "m_LocalEuler", typeof(Transform)), animationClip);
            }
        }

        public void Record(float elapsedTime)
        {
            if (m_Transforms == null)
                return;

            Debug.Assert(m_Transforms.Length == m_PositionCurves.Length);
            Debug.Assert(m_Transforms.Length == m_RotationCurves.Length);

            for (var i = 0; i < m_Transforms.Length; ++i)
            {
                var transform = m_Transforms[i];

                if (transform == null)
                    continue;

                m_PositionCurves[i].AddKey(elapsedTime, transform.localPosition);
                m_RotationCurves[i].AddKey(elapsedTime, transform.localRotation);
            }
        }

        void PrepareRootTransform(Animator animator, FrameRate frameRate)
        {
            m_Paths = new string[1] { string.Empty };
            m_Transforms = new Transform[1];
            m_PositionCurves = new Vector3Curve[1];
            m_RotationCurves = new EulerCurve[1];

            m_Transforms[0] = animator.transform;
            m_PositionCurves[0] = new Vector3Curve();
            m_PositionCurves[0].FrameRate = frameRate;
            m_PositionCurves[0].MaxError = PositionError / 100f;
            m_RotationCurves[0] = new EulerCurve();
            m_RotationCurves[0].FrameRate = frameRate;
            m_RotationCurves[0].MaxError = RotationError;
        }

        void PrepareWithAvatarMask(Animator animator, AvatarMask avatarMask, FrameRate frameRate)
        {
            Debug.Assert(avatarMask != null);

            var transformCount = avatarMask.transformCount;

            m_Paths = new string[transformCount];
            m_Transforms = new Transform[transformCount];
            m_PositionCurves = new Vector3Curve[transformCount];
            m_RotationCurves = new EulerCurve[transformCount];

            for (var i = 0; i < transformCount; ++i)
            {
                var path = avatarMask.GetTransformPath(i);
                var active = avatarMask.GetTransformActive(i);

                m_Paths[i] = path;

                if (!active)
                    continue;

                m_Transforms[i] = animator.transform.Find(path);
                m_PositionCurves[i] = new Vector3Curve();
                m_PositionCurves[i].FrameRate = frameRate;
                m_PositionCurves[i].MaxError = PositionError / 100f;
                m_RotationCurves[i] = new EulerCurve();
                m_RotationCurves[i].FrameRate = frameRate;
                m_RotationCurves[i].MaxError = RotationError;

            }
        }
    }
}
