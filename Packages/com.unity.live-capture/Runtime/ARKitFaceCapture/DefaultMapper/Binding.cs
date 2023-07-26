using System;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    /// <summary>
    /// The class that stores a mapping between a <see cref="FaceBlendShape"/> and a mesh blend shape.
    /// </summary>
    [Serializable]
    class Binding
    {
        [SerializeField]
        FaceBlendShape m_Location = FaceBlendShape.Invalid;
        [SerializeField]
        int m_ShapeIndex = -1;
        [SerializeField]
        BindingConfig m_Config = null;

#pragma warning disable CS0414
        [SerializeField, HideInInspector]
        bool m_IsExpanded = false;
#pragma warning restore CS0414

        /// <summary>
        /// The ARKit blend shape that influences the mesh blend shape associated with this binding.
        /// </summary>
        public FaceBlendShape Location => m_Location;

        /// <summary>
        /// The index of the mesh blend shape influenced by this binding.
        /// </summary>
        public int ShapeIndex => m_ShapeIndex;

        /// <summary>
        /// The properties that control how mapped value is applied.
        /// </summary>
        public BindingConfig Config => m_Config;

        /// <summary>
        /// Is this binding shown in the inspector.
        /// </summary>
        public bool IsExpanded => m_IsExpanded;

        /// <summary>
        /// Creates a new <see cref="Binding"/> instance.
        /// </summary>
        /// <param name="location">The ARKit blend shape that influences the mesh blend shape associated with this binding.</param>
        /// <param name="shapeIndex">The index of the mesh blend shape influenced by this binding.</param>
        /// <param name="config">The properties that control how mapped value is applied.</param>
        /// <param name="isExpanded">Is this binding shown in the inspector.</param>
        public Binding(FaceBlendShape location, int shapeIndex, BindingConfig config, bool isExpanded)
        {
            m_Location = location;
            m_ShapeIndex = shapeIndex;
            m_Config = config;
            m_IsExpanded = isExpanded;
        }

        /// <inheritdoc />
        public override string ToString() => $"Location: {m_Location}, Mesh Blend Shape Index: {m_ShapeIndex}";
    }
}
