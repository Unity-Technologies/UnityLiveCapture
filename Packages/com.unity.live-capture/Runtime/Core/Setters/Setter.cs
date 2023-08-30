using System;
using UnityEngine;

namespace Unity.LiveCapture.Setters
{
    interface ISetter
    {
        Type ComponentType { get; }
        Type ValueType { get; }
        string PropertyName { get; }
    }

    abstract class Setter<TComponent, TValue> : ISetter
        where TComponent : Component
        where TValue : struct
    {
        public Type ComponentType => typeof(TComponent);
        public Type ValueType => typeof(TValue);
        public abstract string PropertyName { get; }

        public abstract void Set(TComponent component, in TValue value);
    }
}
