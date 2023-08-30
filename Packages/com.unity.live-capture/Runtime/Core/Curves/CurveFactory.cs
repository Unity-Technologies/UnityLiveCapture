using System;
using System.Linq;
using System.Collections.Generic;

namespace Unity.LiveCapture
{
    static class CurveFactory
    {
        static Dictionary<Type, Type> s_CurveTypes;

        static CurveFactory()
        {
            s_CurveTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    a.FullName.StartsWith("Unity.LiveCapture,")
                    || a.GetReferencedAssemblies()
                        .Any(an => an.FullName.StartsWith("Unity.LiveCapture,")))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract
                    && !t.IsInterface
                    && typeof(ICurve).IsAssignableFrom(t))
                .Select(t => (t, t.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICurve<>))))
                .Where(t => t.Item2 != null)
                .Select(t => (t.Item1, t.Item2.GetGenericArguments()[0]))
                .ToDictionary(t => t.Item2, t => t.Item1);
        }

        public static ICurve<T> CreateCurve<T>() where T : struct
        {
            if (s_CurveTypes.TryGetValue(typeof(T), out var type))
            {
                return Activator.CreateInstance(type) as ICurve<T>;
            }

            throw new InvalidOperationException($"Can't create a curve of type: {typeof(T)}");
        }
    }
}
