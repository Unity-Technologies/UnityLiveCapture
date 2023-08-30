using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// The blend shapes supported by face capture.
    /// </summary>
    /// <seealso href="https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation"/>
    public enum FaceBlendShape : int
    {
        // names with values less than zero are not generated as a field in FaceBlendShapePose

        /// <summary>
        /// The value used to represent an invalid or unsupported face shape.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// The downward movement of the outer portion of the left eyebrow.
        /// </summary>
        BrowDownLeft = 0,
        /// <summary>
        /// The downward movement of the outer portion of the right eyebrow.
        /// </summary>
        BrowDownRight = 1,
        /// <summary>
        /// The upward movement of the inner portion of both eyebrows.
        /// </summary>
        BrowInnerUp = 2,
        /// <summary>
        /// The upward movement of the outer portion of the left eyebrow.
        /// </summary>
        BrowOuterUpLeft = 3,
        /// <summary>
        /// The upward movement of the outer portion of the right eyebrow.
        /// </summary>
        BrowOuterUpRight = 4,
        /// <summary>
        /// The outward movement of both cheeks.
        /// </summary>
        CheekPuff = 5,
        /// <summary>
        /// The upward movement of the cheek around and below the left eye.
        /// </summary>
        CheekSquintLeft = 6,
        /// <summary>
        /// The upward movement of the cheek around and below the right eye.
        /// </summary>
        CheekSquintRight = 7,
        /// <summary>
        /// The closure of the eyelids over the left eye.
        /// </summary>
        EyeBlinkLeft = 8,
        /// <summary>
        /// The closure of the eyelids over the right eye.
        /// </summary>
        EyeBlinkRight = 9,
        /// <summary>
        /// The movement of the left eyelids consistent with a downward gaze.
        /// </summary>
        EyeLookDownLeft = 10,
        /// <summary>
        /// The movement of the right eyelids consistent with a downward gaze.
        /// </summary>
        EyeLookDownRight = 11,
        /// <summary>
        /// The movement of the left eyelids consistent with a rightward gaze.
        /// </summary>
        EyeLookInLeft = 12,
        /// <summary>
        /// The movement of the right eyelids consistent with a leftward gaze.
        /// </summary>
        EyeLookInRight = 13,
        /// <summary>
        /// The movement of the left eyelids consistent with a leftward gaze.
        /// </summary>
        EyeLookOutLeft = 14,
        /// <summary>
        /// The movement of the right eyelids consistent with a rightward gaze.
        /// </summary>
        EyeLookOutRight = 15,
        /// <summary>
        /// The movement of the left eyelids consistent with an upward gaze.
        /// </summary>
        EyeLookUpLeft = 16,
        /// <summary>
        /// The movement of the right eyelids consistent with an upward gaze.
        /// </summary>
        EyeLookUpRight = 17,
        /// <summary>
        /// The contraction of the face around the left eye.
        /// </summary>
        EyeSquintLeft = 18,
        /// <summary>
        /// The coefficient of the face around the right eye.
        /// </summary>
        EyeSquintRight = 19,
        /// <summary>
        /// The widening of the eyelids around the left eye.
        /// </summary>
        EyeWideLeft = 20,
        /// <summary>
        /// The widening of the eyelids around the right eye.
        /// </summary>
        EyeWideRight = 21,
        /// <summary>
        /// The forward movement of the lower jaw.
        /// </summary>
        JawForward = 22,
        /// <summary>
        /// The leftward movement of the lower jaw.
        /// </summary>
        JawLeft = 23,
        /// <summary>
        /// The opening of the lower jaw.
        /// </summary>
        JawOpen = 24,
        /// <summary>
        /// The rightward movement of the lower jaw.
        /// </summary>
        JawRight = 25,
        /// <summary>
        /// The closure of the lips independent of jaw position.
        /// </summary>
        MouthClose = 26,
        /// <summary>
        /// The backward movement of the left corner of the mouth.
        /// </summary>
        MouthDimpleLeft = 27,
        /// <summary>
        /// The backward movement of the right corner of the mouth.
        /// </summary>
        MouthDimpleRight = 28,
        /// <summary>
        /// The downward movement of the left corner of the mouth.
        /// </summary>
        MouthFrownLeft = 29,
        /// <summary>
        /// The downward movement of the right corner of the mouth.
        /// </summary>
        MouthFrownRight = 30,
        /// <summary>
        /// The contraction of both lips into an open shape.
        /// </summary>
        MouthFunnel = 31,
        /// <summary>
        /// The leftward movement of both lips together.
        /// </summary>
        MouthLeft = 32,
        /// <summary>
        /// The downward movement of the lower lip on the left side.
        /// </summary>
        MouthLowerDownLeft = 33,
        /// <summary>
        /// The downward movement of the lower lip on the right side.
        /// </summary>
        MouthLowerDownRight = 34,
        /// <summary>
        /// The upward compression of the lower lip on the left side.
        /// </summary>
        MouthPressLeft = 35,
        /// <summary>
        /// The upward compression of the lower lip on the right side.
        /// </summary>
        MouthPressRight = 36,
        /// <summary>
        /// The contraction and compression of both closed lips.
        /// </summary>
        MouthPucker = 37,
        /// <summary>
        /// The rightward movement of both lips together.
        /// </summary>
        MouthRight = 38,
        /// <summary>
        /// The movement of the lower lip toward the inside of the mouth.
        /// </summary>
        MouthRollLower = 39,
        /// <summary>
        /// The movement of the upper lip toward the inside of the mouth.
        /// </summary>
        MouthRollUpper = 40,
        /// <summary>
        /// The outward movement of the lower lip.
        /// </summary>
        MouthShrugLower = 41,
        /// <summary>
        /// The outward movement of the upper lip.
        /// </summary>
        MouthShrugUpper = 42,
        /// <summary>
        /// The upward movement of the left corner of the mouth.
        /// </summary>
        MouthSmileLeft = 43,
        /// <summary>
        /// The upward movement of the right corner of the mouth.
        /// </summary>
        MouthSmileRight = 44,
        /// <summary>
        /// The leftward movement of the left corner of the mouth.
        /// </summary>
        MouthStretchLeft = 45,
        /// <summary>
        /// The rightward movement of the left corner of the mouth.
        /// </summary>
        MouthStretchRight = 46,
        /// <summary>
        /// The upward movement of the upper lip on the left side.
        /// </summary>
        MouthUpperUpLeft = 47,
        /// <summary>
        /// The upward movement of the upper lip on the right side.
        /// </summary>
        MouthUpperUpRight = 48,
        /// <summary>
        /// The raising of the left side of the nose around the nostril.
        /// </summary>
        NoseSneerLeft = 49,
        /// <summary>
        /// The raising of the right side of the nose around the nostril.
        /// </summary>
        NoseSneerRight = 50,
        /// <summary>
        /// The extension of the tongue.
        /// </summary>
        TongueOut = 51,
    }

    /// <summary>
    /// Stores a face pose as a set of blend shape weights.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct FaceBlendShapePose
    {
        /// <summary>
        /// The face blend shapes, excluding shapes with a negative value.
        /// </summary>
        public static FaceBlendShape[] Shapes { get; } = Enum.GetValues(typeof(FaceBlendShape))
            .Cast<FaceBlendShape>()
            .Where(s => (int)s >= 0)
            .ToArray();

        /// <summary>
        /// The pose value for a blend shape at the given index.
        /// </summary>
        /// <param name="index">The blend shape index.</param>
        public float this[int index]
        {
            get => GetValue(index);
            set => SetValue(index, value);
        }

        /// <summary>
        /// Gets the pose value for a blend shape.
        /// </summary>
        /// <param name="location">The blend shape to get the value of.</param>
        /// <returns>The normalized blend shape influence.</returns>
        public float GetValue(FaceBlendShape location) => GetValue((int)location);

        /// <summary>
        /// Sets the pose value for a blend shape.
        /// </summary>
        /// <param name="location">The blend shape to set the value of.</param>
        /// <param name="value">The normalized blend shape influence.</param>
        public void SetValue(FaceBlendShape location, float value) => SetValue((int)location, value);
    }
}
