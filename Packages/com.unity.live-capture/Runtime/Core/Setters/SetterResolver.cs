using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.LiveCapture.Setters;

namespace Unity.LiveCapture
{
    class SetterResolver
    {
        static Dictionary<PropertyBinding, ISetter> s_Setters = new Dictionary<PropertyBinding, ISetter>();

        static SetterResolver()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    a.FullName.StartsWith("Unity.LiveCapture,")
                    || a.GetReferencedAssemblies()
                        .Any(an => an.FullName.StartsWith("Unity.LiveCapture,")))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract
                    && !t.IsInterface
                    && typeof(ISetter).IsAssignableFrom(t)
                    && !t.IsGenericType)
                .ToArray();

            foreach (var type in types)
            {
                var setter = Activator.CreateInstance(type) as ISetter;
                var binding = new PropertyBinding(string.Empty, setter.PropertyName, setter.ComponentType);

                s_Setters[binding] = setter;
            }
        }

        public static bool TryResolve<TComponent, TValue>(string propertyName, out Setter<TComponent, TValue> setter)
            where TComponent : Component
            where TValue : struct
        {
            TryResolve(typeof(TComponent), propertyName, out var iSetter);

            setter = iSetter as Setter<TComponent, TValue>;

            return setter != null;
        }

        public static bool TryResolve(Type componentType, string propertyName, out ISetter setter)
        {
            var binding = new PropertyBinding(string.Empty, propertyName, componentType);

            return s_Setters.TryGetValue(binding, out setter);
        }
    }
}
