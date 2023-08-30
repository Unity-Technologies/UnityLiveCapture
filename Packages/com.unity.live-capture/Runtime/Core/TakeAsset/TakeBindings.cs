using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A serializable implementation of <see cref="ITakeBinding"/> that contains values of
    /// type UnityEngine.Animator.
    /// </summary>
    [Serializable]
    public class AnimatorTakeBinding : TakeBinding<Animator> { }

    /// <summary>
    /// A serializable implementation of <see cref="ITakeBinding"/> that contains values of
    /// type UnityEngine.Transform.
    /// </summary>
    [Serializable]
    public class TransformTakeBinding : TakeBinding<Transform> { }
}
