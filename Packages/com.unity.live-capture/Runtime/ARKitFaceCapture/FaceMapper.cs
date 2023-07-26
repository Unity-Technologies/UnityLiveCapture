using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// An asset used to apply a face pose to a character rig.
    /// </summary>
    /// <remarks>
    /// To use a mapper, it must be assigned to a <see cref="FaceActor"/> component.
    /// <see cref="DefaultMapper.DefaultFaceMapper"/> is the default mapper implementation, designed to work
    /// for rigs that can have their bone transforms and renderer blend shapes modified directly. For complex
    /// rigs that need more advanced re-targeting of the captured face animation, inherit from this class
    /// to implement custom mapper.
    /// </remarks>
    public abstract class FaceMapper : ScriptableObject
    {
        /// <summary>
        /// Creates a mapper state cache for the specified actor.
        /// </summary>
        /// <param name="actor">The face rig to create the cache for.</param>
        /// <returns>The new cache instance, or null if no cache is needed by the mapper.</returns>
        public virtual FaceMapperCache CreateCache(FaceActor actor)
        {
            return null;
        }

        /// <summary>
        /// Called by <see cref="FaceActor"/> to update a face rig to show a face pose.
        /// </summary>
        /// <param name="actor">The face rig the pose is applied to.</param>
        /// <param name="cache">The mapper state cache for the specified actor.</param>
        /// <param name="pose">The face pose to map from.</param>
        /// <param name="continuous">When true, the new pose follows the current pose and they
        /// can be smoothed between, while false corresponds to a seek in the animation where the
        /// previous pose is invalidated and should not influence the new pose.</param>
        public abstract void ApplyBlendShapesToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref FaceBlendShapePose pose,
            bool continuous
        );

        /// <summary>
        /// Called by <see cref="FaceActor"/> to update the head position of the character rig.
        /// </summary>
        /// <param name="actor">The face rig the pose is applied to.</param>
        /// <param name="cache">The mapper state cache for the specified actor.</param>
        /// <param name="headPosition">The head position to map from.</param>
        /// <param name="continuous">When true, the new pose follows the current pose and they
        /// can be smoothed between, while false corresponds to a seek in the animation where the
        /// previous pose is invalidated and should not influence the new pose.</param>
        public abstract void ApplyHeadPositionToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref Vector3 headPosition,
            bool continuous
        );

        /// <summary>
        /// Called by <see cref="FaceActor"/> to update the head rotation of the character rig.
        /// </summary>
        /// <param name="actor">The face rig the pose is applied to.</param>
        /// <param name="cache">The mapper state cache for the specified actor.</param>
        /// <param name="headOrientation">The head pose to map from.</param>
        /// <param name="continuous">When true, the new pose follows the current pose and they
        /// can be smoothed between, while false corresponds to a seek in the animation where the
        /// previous pose is invalidated and should not influence the new pose.</param>
        public abstract void ApplyHeadRotationToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref Quaternion headOrientation,
            bool continuous
        );

        /// <summary>
        /// Called by <see cref="FaceActor"/> to update the eye rotations of the face rig.
        /// </summary>
        /// <param name="actor">The face rig the pose is applied to.</param>
        /// <param name="cache">The mapper state cache for the specified actor.</param>
        /// <param name="pose">The face blend shapes to map from.</param>
        /// <param name="leftEyeRotation">The left eye rotation to map from.</param>
        /// <param name="rightEyeRotation">The right eye rotation to map from.</param>
        /// <param name="continuous">When true, the new pose follows the current pose and they
        /// can be smoothed between, while false corresponds to a seek in the animation where the
        /// previous pose is invalidated and should not influence the new pose.</param>
        public abstract void ApplyEyeRotationToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref FaceBlendShapePose pose,
            ref Quaternion leftEyeRotation,
            ref Quaternion rightEyeRotation,
            bool continuous
        );

        /// <summary>
        /// The preview system calls this method before playing animations.
        /// Use the specified <see cref="IPropertyPreviewer"/> to register animated properties.
        /// </summary>
        /// <param name="actor">The face rig the pose is applied to.</param>
        /// <param name="cache">The mapper state cache for the specified actor.</param>
        /// <param name="previewer">The <see cref="IPropertyPreviewer"/> to register animated properties to.</param>
        public virtual void RegisterPreviewableProperties(
            FaceActor actor,
            FaceMapperCache cache,
            IPropertyPreviewer previewer
        )
        { }
    }
}
