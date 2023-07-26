using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    /// <summary>
    /// The default <see cref="FaceMapper"/> implementation.
    /// </summary>
    /// <remarks>
    /// The inspector is used to assign a prefab containing a face rig and configure the mapping for the rig.
    /// On assignment, all relevant skinned mesh renderers in the rig are detected and a best guess for the mapping
    /// is automatically generated. The mapping may be shared by different rigs, as long as the transform paths of
    /// all skinned meshes relative to the <see cref="FaceActor"/> components are consistent between the rigs.
    /// </remarks>
    [CreateAssetMenu(fileName = "NewFaceMapper", menuName = "Live Capture/ARKit Face Capture/Mapper")]
    [HelpURL(Documentation.baseURL + "ref-component-arkit-default-face-mapper" + Documentation.endURL)]
    public class DefaultFaceMapper : FaceMapper
    {
        internal enum EyeMovementDriverType
        {
            /// <summary>
            /// Use ARKit eye tracking to determine the eye orientation.
            /// </summary>
            EyeTracking,
            /// <summary>
            /// Calculate the eye orientation from ARKit blend shapes.
            /// </summary>
            BlendShapes,
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField]
        [Tooltip("Select the prefab containing the face rig to initialize the mapping with.")]
        GameObject m_RigPrefab = null;

        [SerializeField]
        [Tooltip("The threshold determining how close a mesh blend shape name must be to an ARKit blend shape name to automatically be bound.")]
        [Range(0f, 1f)]
        float m_ShapeMatchTolerance = 0.6f;

        [SerializeField]
        [Tooltip("The evaluator preset to assign when a new blend shape binding is created.")]
        EvaluatorPreset m_DefaultEvaluator = null;

        [SerializeField]
        [Tooltip("Change the blend shape editor to show the bindings from blend shapes on the meshes to the ARKit blend shape locations.")]
        bool m_InvertMappings = false;
#pragma warning restore CS0414
#endif

        [SerializeField]
        List<RendererMapping> m_Maps = new List<RendererMapping>();

        [SerializeField]
        [Tooltip("How to drive eye movement.")]
        EyeMovementDriverType m_EyeMovementDriver = EyeMovementDriverType.EyeTracking;

        [SerializeField]
        [Tooltip("The left eye bone transform.")]
        string m_LeftEye = string.Empty;

        [SerializeField]
        [Tooltip("The right eye bone transform.")]
        string m_RightEye = string.Empty;

        [SerializeField]
        [Tooltip("The horizontal and vertical arcs in degrees over which the eyes can rotate.")]
        Vector2 m_EyeAngleRange = new Vector2(45, 30);

        [SerializeField]
        [Tooltip("The horizontal and vertical angles in degrees by which to offset the resting position for the eyes.")]
        Vector2 m_EyeAngleOffset = new Vector2(0, 0);

        [SerializeField]
        [Tooltip("The amount of smoothing to apply to eye movement. " +
            "It can help reduce jitter in the face capture, but it will also smooth out fast motions.")]
        [Range(0f, 1f)]
        float m_EyeSmoothing = 0.1f;

        [SerializeField]
        [Tooltip("The head transform to drive position.")]
        string m_HeadPosition = string.Empty;

        [SerializeField]
        [Tooltip("The head transform to drive rotation.")]
        string m_HeadRotation = string.Empty;

        [SerializeField]
        [Tooltip("The amount of smoothing to apply to head movement. " +
            "It can help reduce jitter in the face capture, but it will also smooth out fast motions.")]
        [Range(0f, 1f)]
        float m_HeadSmoothing = 0.1f;

        [SerializeField]
        [Tooltip("The amount of smoothing to apply to all blend shape values unless overriden.")]
        [Range(0f, 1f)]
        float m_BlendShapeSmoothing = 0.1f;

        struct BlendShapeData
        {
            public int UseCount;
            public float Weight;
        }

        struct BindingData
        {
            public readonly int ShapeIndex;
            public readonly BindingConfig Config;
            public float LastWeight;

            public BindingData(int shapeIndex, BindingConfig config)
            {
                ShapeIndex = shapeIndex;
                Config = config;
                LastWeight = 0f;
            }
        }

        readonly struct MappingData
        {
            public readonly FaceBlendShape Location;
            public readonly BindingData[] Bindings;

            public MappingData(FaceBlendShape location, BindingData[] bindings)
            {
                Location = location;
                Bindings = bindings;
            }
        }

        readonly struct RendererData
        {
            public readonly SkinnedMeshRenderer Renderer;
            public readonly BlendShapeData[] BlendShapeData;
            public readonly MappingData[] Mappings;

            public RendererData(FaceActor actor, RendererMapping rendererMapping)
            {
                var target = actor == null ? null : actor.transform.Find(rendererMapping.Path);
                Renderer = target == null ? null : target.GetComponent<SkinnedMeshRenderer>();
                var mesh = Renderer == null ? null : Renderer.sharedMesh;

                if (mesh == null)
                {
                    BlendShapeData = new BlendShapeData[0];
                    Mappings = new MappingData[0];
                    return;
                }

                Mappings = rendererMapping.Bindings
                    .Where(b => b.ShapeIndex >= 0 && b.ShapeIndex < mesh.blendShapeCount)
                    .ToLookup(b => b.Location, b => (shapeIndex: b.ShapeIndex, config: b.Config))
                    .Select(m => new MappingData(m.Key, m.Select(b => new BindingData(b.shapeIndex, b.config)).ToArray()))
                    .ToArray();

                BlendShapeData = new BlendShapeData[mesh.blendShapeCount];

                // count how many face shapes influence each blend shape on the mesh
                foreach (var mapping in Mappings)
                {
                    foreach (var binding in mapping.Bindings)
                    {
                        BlendShapeData[binding.ShapeIndex].UseCount++;
                    }
                }
            }
        }

        class Cache : FaceMapperCache
        {
            readonly Dictionary<string, Transform> m_Bones = new Dictionary<string, Transform>();
            readonly List<RendererData> m_Renderers = new List<RendererData>();

            public IEnumerable<RendererData> Renderers => m_Renderers;

            public Cache(FaceActor actor, IEnumerable<RendererMapping> maps)
            {
                foreach (var map in maps)
                {
                    m_Renderers.Add(new RendererData(actor, map));
                }
            }

            public bool TryGetBone(FaceActor actor, string path, out Transform bone)
            {
                if (!m_Bones.TryGetValue(path, out bone))
                {
                    bone = actor.transform.Find(path);

                    if (bone != null)
                        m_Bones.Add(path, bone);
                }

                return bone != null;
            }
        }

        /// <summary>
        /// Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        /// </summary>
        protected virtual void OnValidate()
        {
            var usedPaths = new HashSet<string>();

            for (var i = 0; i < m_Maps.Count;)
            {
                var rendererMapping = m_Maps[i];
                var path = rendererMapping.Path;

                if (!string.IsNullOrEmpty(path) && usedPaths.Contains(path))
                {
                    m_Maps.RemoveAt(i);
                    Debug.LogError($"Face mapper \"{name}\" has multiple mappings for renderer \"{rendererMapping}\". The extra mapping has been removed.");
                    continue;
                }
                usedPaths.Add(path);

                rendererMapping.Validate(this);
                i++;
            }
        }

        /// <inheritdoc />
        public override FaceMapperCache CreateCache(FaceActor actor)
        {
            return new Cache(actor, m_Maps);
        }

        /// <inheritdoc />
        public override void ApplyBlendShapesToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref FaceBlendShapePose pose,
            bool continuous
        )
        {
            if (actor == null)
                return;

            var c = cache as Cache;

            foreach (var data in c.Renderers)
            {
                // ensure the references are still valid
                if (data.Renderer == null || data.Renderer.sharedMesh == null)
                    continue;

                // clear the blend shape values
                for (var i = 0; i < data.BlendShapeData.Length; i++)
                    data.BlendShapeData[i].Weight = 0f;

                // Compute the new weights. We sum the weights from all influences to get the final
                // result. It may be better to let the user select from multiple techniques instead.
                for (var i = 0; i < data.Mappings.Length; i++)
                {
                    var mapping = data.Mappings[i];
                    var bindings = mapping.Bindings;
                    var value = pose.GetValue(mapping.Location);

                    for (var j = 0; j < mapping.Bindings.Length; j++)
                    {
                        var binding = bindings[j];
                        var config = binding.Config;
                        var weight = config.GetEvaluator().Evaluate(value);
                        var smoothing = config.OverrideSmoothing ? config.Smoothing : m_BlendShapeSmoothing;

                        if (continuous)
                            weight = Mathf.Lerp(weight, binding.LastWeight, smoothing);

                        data.BlendShapeData[binding.ShapeIndex].Weight += weight;
                        data.Mappings[i].Bindings[j].LastWeight = weight;
                    }
                }

                // apply the blend shape weights to the skinned mesh
                for (var i = 0; i < data.BlendShapeData.Length; i++)
                {
                    if (data.BlendShapeData[i].UseCount > 0)
                        data.Renderer.SetBlendShapeWeight(i, data.BlendShapeData[i].Weight);
                }
            }
        }

        /// <inheritdoc />
        public override void ApplyHeadPositionToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref Vector3 headPosition,
            bool continuous
        )
        {
            if (actor == null)
                return;

            var c = cache as Cache;

            if (!string.IsNullOrEmpty(m_HeadPosition) && c.TryGetBone(actor, m_HeadPosition, out var head))
            {
                head.localPosition = continuous ? Vector3.Lerp(headPosition, head.localPosition, m_HeadSmoothing) : headPosition;
            }
        }

        /// <inheritdoc />
        public override void ApplyHeadRotationToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref Quaternion headOrientation,
            bool continuous
        )
        {
            if (actor == null)
                return;

            var c = cache as Cache;

            if (!string.IsNullOrEmpty(m_HeadRotation) && c.TryGetBone(actor, m_HeadRotation, out var head))
            {
                head.localRotation = continuous ? Quaternion.Slerp(headOrientation, head.localRotation, m_HeadSmoothing) : headOrientation;
            }
        }

        /// <inheritdoc />
        public override void ApplyEyeRotationToRig(
            FaceActor actor,
            FaceMapperCache cache,
            ref FaceBlendShapePose pose,
            ref Quaternion leftEyeRotation,
            ref Quaternion rightEyeRotation,
            bool continuous
        )
        {
            if (actor == null)
                return;

            var c = cache as Cache;

            if (!string.IsNullOrEmpty(m_LeftEye) && c.TryGetBone(actor, m_LeftEye, out var leftEye))
            {
                switch (m_EyeMovementDriver)
                {
                    case EyeMovementDriverType.BlendShapes:
                    {
                        var leftEyeH = pose.GetValue(FaceBlendShape.EyeLookInLeft) - pose.GetValue(FaceBlendShape.EyeLookOutLeft);
                        var leftEyeV = pose.GetValue(FaceBlendShape.EyeLookUpLeft) - pose.GetValue(FaceBlendShape.EyeLookDownLeft);

                        var leftEyeYaw = Quaternion.AngleAxis(leftEyeH * m_EyeAngleRange.x + m_EyeAngleOffset.x, Vector3.up);
                        var leftEyePitch = Quaternion.AngleAxis(leftEyeV * m_EyeAngleRange.y + m_EyeAngleOffset.y, Vector3.left);
                        var targetRotation = leftEyePitch * leftEyeYaw;

                        leftEye.localRotation = continuous ? Quaternion.Slerp(targetRotation, leftEye.localRotation, m_EyeSmoothing) : targetRotation;

                        break;
                    }
                    case EyeMovementDriverType.EyeTracking:
                    {
                        var targetRotation = continuous ? Quaternion.Slerp(leftEyeRotation, leftEye.localRotation, m_EyeSmoothing) : leftEyeRotation;

                        var offsetXRotation = Quaternion.AngleAxis(m_EyeAngleOffset.x, Vector3.up);
                        var offsetYRotation = Quaternion.AngleAxis(m_EyeAngleOffset.y, Vector3.left);
                        var offsetRotation = offsetXRotation * offsetYRotation;
                        targetRotation *= offsetRotation;

                        leftEye.localRotation = targetRotation;

                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(m_RightEye) && c.TryGetBone(actor, m_RightEye, out var rightEye))
            {
                switch (m_EyeMovementDriver)
                {
                    case EyeMovementDriverType.BlendShapes:
                    {
                        var rightEyeH = pose.GetValue(FaceBlendShape.EyeLookInRight) - pose.GetValue(FaceBlendShape.EyeLookOutRight);
                        var rightEyeV = pose.GetValue(FaceBlendShape.EyeLookUpRight) - pose.GetValue(FaceBlendShape.EyeLookDownRight);

                        var rightEyeYaw = Quaternion.AngleAxis(rightEyeH * m_EyeAngleRange.x + m_EyeAngleOffset.x, Vector3.down);
                        var rightEyePitch = Quaternion.AngleAxis(rightEyeV * m_EyeAngleRange.y + m_EyeAngleOffset.y, Vector3.left);
                        var targetRotation = rightEyePitch * rightEyeYaw;

                        rightEye.localRotation = continuous ? Quaternion.Slerp(targetRotation, rightEye.localRotation, m_EyeSmoothing) : targetRotation;

                        break;
                    }
                    case EyeMovementDriverType.EyeTracking:
                    {
                        var targetRotation = continuous ? Quaternion.Slerp(rightEyeRotation, rightEye.localRotation, m_EyeSmoothing) : rightEyeRotation;

                        var offsetXRotation = Quaternion.AngleAxis(m_EyeAngleOffset.x, Vector3.down);
                        var offsetYRotation = Quaternion.AngleAxis(m_EyeAngleOffset.y, Vector3.left);
                        var offsetRotation = offsetXRotation * offsetYRotation;
                        targetRotation *= offsetRotation;

                        rightEye.localRotation = targetRotation;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void RegisterPreviewableProperties(
            FaceActor actor,
            FaceMapperCache cache,
            IPropertyPreviewer previewer
        )
        {
            var c = cache as Cache;

            if (c.TryGetBone(actor, m_HeadPosition, out var headPosition))
            {
                previewer.Register(headPosition, "m_LocalPosition.x");
                previewer.Register(headPosition, "m_LocalPosition.y");
                previewer.Register(headPosition, "m_LocalPosition.z");
            }

            if (c.TryGetBone(actor, m_HeadRotation, out var headRotation))
            {
                previewer.Register(headRotation, "m_LocalRotation.x");
                previewer.Register(headRotation, "m_LocalRotation.y");
                previewer.Register(headRotation, "m_LocalRotation.z");
                previewer.Register(headRotation, "m_LocalRotation.w");
                previewer.Register(headRotation, "m_LocalEulerAnglesHint.x");
                previewer.Register(headRotation, "m_LocalEulerAnglesHint.y");
                previewer.Register(headRotation, "m_LocalEulerAnglesHint.z");
            }

            if (c.TryGetBone(actor, m_LeftEye, out var leftEye))
            {
                previewer.Register(leftEye, "m_LocalRotation.x");
                previewer.Register(leftEye, "m_LocalRotation.y");
                previewer.Register(leftEye, "m_LocalRotation.z");
                previewer.Register(leftEye, "m_LocalRotation.w");
                previewer.Register(leftEye, "m_LocalEulerAnglesHint.x");
                previewer.Register(leftEye, "m_LocalEulerAnglesHint.y");
                previewer.Register(leftEye, "m_LocalEulerAnglesHint.z");
            }

            if (c.TryGetBone(actor, m_RightEye, out var rightEye))
            {
                previewer.Register(rightEye, "m_LocalRotation.x");
                previewer.Register(rightEye, "m_LocalRotation.y");
                previewer.Register(rightEye, "m_LocalRotation.z");
                previewer.Register(rightEye, "m_LocalRotation.w");
                previewer.Register(rightEye, "m_LocalEulerAnglesHint.x");
                previewer.Register(rightEye, "m_LocalEulerAnglesHint.y");
                previewer.Register(rightEye, "m_LocalEulerAnglesHint.z");
            }

            foreach (var data in c.Renderers)
            {
                for (var i = 0; i < data.BlendShapeData.Length; i++)
                {
                    if (data.BlendShapeData[i].UseCount > 0)
                        previewer.Register(data.Renderer, $"m_BlendShapeWeights.Array.data[{i}]");
                }
            }
        }
    }
}
