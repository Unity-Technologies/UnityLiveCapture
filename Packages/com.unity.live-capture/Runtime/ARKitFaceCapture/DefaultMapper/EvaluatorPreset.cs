using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    /// <summary>
    /// Stores an <see cref="IEvaluator"/> in an asset to it can be shared by many mappings.
    /// </summary>
    abstract class EvaluatorPreset : ScriptableObject
    {
        /// <summary>
        /// The evaluator stored in this asset.
        /// </summary>
        public abstract IEvaluator Evaluator { get; }
    }
}
