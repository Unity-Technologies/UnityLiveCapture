using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// An attribute placed on a serialized field to make it not editable from the inspector.
    /// </summary>
    /// <remarks>
    /// Adds a tint to the fields.
    /// </remarks>
    class ReadOnly : PropertyAttribute
    {
    }
}
