using System;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// An attribute that allows to specify the rendering order of custom passes, used for HDRP.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class CustomPassOrderAttribute : Attribute
    {
        public int OrderHint { get; }

        public CustomPassOrderAttribute(int orderHint)
        {
            OrderHint = orderHint;
        }
    }
}
