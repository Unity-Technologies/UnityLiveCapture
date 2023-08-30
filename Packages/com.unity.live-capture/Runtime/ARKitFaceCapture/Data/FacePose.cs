using System;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// The data representing a captured face pose.
    /// </summary>
    [Serializable]
    struct FacePose
    {
        /// <summary>
        /// The neutral pose.
        /// </summary>
        public static FacePose Identity { get; } = new FacePose
        {
            BlendShapes = default,
            HeadPosition = Vector3.zero,
            HeadOrientation = Quaternion.identity,
            LeftEyeOrientation = Quaternion.identity,
            RightEyeOrientation = Quaternion.identity,
        };

        /// <summary>
        /// The blend shapes weights defining the face expression.
        /// </summary>
        public FaceBlendShapePose BlendShapes;

        /// <summary>
        /// The local position of the head transform.
        /// </summary>
        public Vector3 HeadPosition;

        /// <summary>
        /// The orientation of the head transform.
        /// </summary>
        public Quaternion HeadOrientation;

        /// <summary>
        /// The orientation of the left eye transform.
        /// </summary>
        public Quaternion LeftEyeOrientation;

        /// <summary>
        /// The orientation of the right eye transform.
        /// </summary>
        public Quaternion RightEyeOrientation;

        /// <summary>
        /// Interpolates between <paramref name="a"/> and <paramref name="b"/> by factor <paramref name="t"/>.
        /// </summary>
        /// <remarks><br/>
        /// * When <paramref name="t"/> is 0 <paramref name="result"/> is set to <paramref name="a"/>.
        /// * When <paramref name="t"/> is 1 <paramref name="result"/> is set to  <paramref name="b"/>.
        /// * When <paramref name="t"/> is 0.5 <paramref name="result"/> is set to the midpoint of <paramref name="a"/> and <paramref name="b"/>.
        /// </remarks>
        /// <param name="a">The pose to interpolate from.</param>
        /// <param name="b">To pose to interpolate to.</param>
        /// <param name="t">The interpolation factor.</param>
        /// <param name="result">The interpolated pose.</param>
        public static void Interpolate(in FacePose a, in FacePose b, float t, out FacePose result)
        {
            FaceBlendShapePose.LerpUnclamped(a.BlendShapes, b.BlendShapes, t, out result.BlendShapes);
            result.HeadPosition = Vector3.LerpUnclamped(a.HeadPosition, b.HeadPosition, t);
            result.HeadOrientation = Quaternion.SlerpUnclamped(a.HeadOrientation, b.HeadOrientation, t);
            result.LeftEyeOrientation = Quaternion.SlerpUnclamped(a.LeftEyeOrientation, b.LeftEyeOrientation, t);
            result.RightEyeOrientation = Quaternion.SlerpUnclamped(a.RightEyeOrientation, b.RightEyeOrientation, t);
        }
    }
}
