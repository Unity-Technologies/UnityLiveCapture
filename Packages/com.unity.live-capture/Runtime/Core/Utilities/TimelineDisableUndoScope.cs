using System;
using System.Reflection;
using System.Linq;

namespace Unity.LiveCapture
{
    static class TimelineDisableUndoScope
    {
        public static IDisposable Create()
        {
#if TIMELINE_1_7_0_OR_NEWER
            return TimelineDisableUndoScope170.Create();
#else
            return new TimelineDisableUndoScopeLegacy();
#endif
        }
    }

#if TIMELINE_1_7_0_OR_NEWER
    static class TimelineDisableUndoScope170
    {
        const string s_AssemblyName = "Unity.Timeline.Editor";
        const string s_TypeName = "UnityEditor.Timeline.UndoExtensions+DisableTimelineUndoScope";
        static Type s_ScopeType;

        static TimelineDisableUndoScope170()
        {
            s_ScopeType = Type.GetType($"{s_TypeName}, {s_AssemblyName}");
        }

        static public IDisposable Create()
        {
            return Activator.CreateInstance(s_ScopeType) as IDisposable;
        }
    }
#else
    class TimelineDisableUndoScopeLegacy : IDisposable
    {
        const string s_TypeStr = "UnityEngine.Timeline.TimelineUndo+DisableUndoGuard";
        const string s_FieldStr = "enableUndo";
        static FieldInfo s_EnableUndo;

        bool m_Disposed;

        static TimelineDisableUndoScopeLegacy()
        {
            var disableUndoGuardType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName.Equals(s_TypeStr));

            s_EnableUndo = disableUndoGuardType.GetField(s_FieldStr, BindingFlags.NonPublic | BindingFlags.Static);
        }

        static void SetUndoEnabled(bool value)
        {
            s_EnableUndo.SetValue(null, value);
        }

        public TimelineDisableUndoScopeLegacy()
        {
            SetUndoEnabled(false);
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(TimelineDisableUndoScopeLegacy));
            }

            SetUndoEnabled(true);

            m_Disposed = true;
        }
    }
#endif
}
