using System;

namespace Unity.LiveCapture.Editor
{
    class ContextEditor : Attribute
    {
        public Type Type { get; private set; }

        public ContextEditor(Type type)
        {
            Type = type;
        }
    }
}
