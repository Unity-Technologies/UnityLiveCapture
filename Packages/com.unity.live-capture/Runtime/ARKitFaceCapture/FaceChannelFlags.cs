using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// The face capture data that can be recorded in a take and played back.
    /// </summary>
    [Flags]
    enum FaceChannelFlags
    {
        /// <summary>
        /// The flags used to indicate no channels.
        /// </summary>
        [Description("No channel recorded.")]
        None = 0,

        /// <summary>
        /// The channel used for the blend shape pose of the face.
        /// </summary>
        [Description("Record face blend shapes.")]
        BlendShapes = 1 << 0,

        /// <summary>
        /// The channel used for the position of the head.
        /// </summary>
        [Description("Record head position.")]
        HeadPosition = 1 << 1,

        /// <summary>
        /// The channel used for the orientation of the head.
        /// </summary>
        [Description("Record head orientation.")]
        HeadRotation = 1 << 2,

        /// <summary>
        /// The channel used for the orientations of the eyes.
        /// </summary>
        [Description("Record eye orientation.")]
        Eyes = 1 << 3,

        /// <summary>
        /// The flags used to indicate all channels.
        /// </summary>
        [Description("Record all supported channels.")]
        All = ~0,
    }
}
