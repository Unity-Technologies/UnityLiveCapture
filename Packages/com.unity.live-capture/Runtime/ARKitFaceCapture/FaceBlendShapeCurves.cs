using System;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// A class used to bake <see cref="FaceBlendShapePose"/> keyframes into a take.
    /// </summary>
    class FaceBlendShapeCurves : ICurve<FaceBlendShapePose>, IReduceableCurve
    {
        readonly FloatCurve[] m_Curves;

        /// <inheritdoc/>
        public float MaxError
        {
            get => m_Curves[0].MaxError;
            set
            {
                foreach (var curve in m_Curves)
                {
                    curve.MaxError = value;
                }
            }
        }

        /// <inheritdoc/>
        public FrameRate FrameRate
        {
            get => m_Curves[0].FrameRate;
            set
            {
                foreach (var curve in m_Curves)
                {
                    curve.FrameRate = value;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="FaceBlendShapePose"/> instance.
        /// </summary>
        public FaceBlendShapeCurves()
        {
            m_Curves = FaceBlendShapePose.Shapes
                .Select(shape => new FloatCurve())
                .ToArray();
        }

        /// <inheritdoc/>
        public void AddKey(double time, in FaceBlendShapePose value)
        {
            for (var i = 0; i < FaceBlendShapePose.ShapeCount; ++i)
            {
                m_Curves[i].AddKey(time, value[i]);
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return m_Curves[0].IsEmpty();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            foreach (var curve in m_Curves)
            {
                curve.Clear();
            }
        }

        /// <inheritdoc/>
        public void SetToAnimationClip(PropertyBinding binding, AnimationClip clip)
        {
            var bindings = FaceBlendShapePose.Shapes
                .Select(shape => new PropertyBinding(binding.RelativePath, $"{binding.PropertyName}.{shape}", binding.Type))
                .ToArray();

            for (var i = 0; i < FaceBlendShapePose.ShapeCount; ++i)
            {
                m_Curves[i].SetToAnimationClip(bindings[i], clip);
            }
        }
    }
}
