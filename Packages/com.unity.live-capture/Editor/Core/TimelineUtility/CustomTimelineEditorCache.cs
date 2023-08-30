/// File borrowed from Timeline package, used to obtain the ClipEditor of a clip.
/// Using the ClipEditor we can obtain the list of sub-Timelines a TimelineClip controls.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Editor
{
    class CustomTimelineEditorCache
    {
        static class SubClassCache<TEditorClass> where TEditorClass : class, new()
        {
            private static Type[] s_SubClasses = null;
            private static readonly TEditorClass s_DefaultInstance = new TEditorClass();
            private static readonly Dictionary<System.Type, TEditorClass> s_TypeMap = new Dictionary<Type, TEditorClass>();

            public static TEditorClass DefaultInstance
            {
                get { return s_DefaultInstance; }
            }

            static Type[] SubClasses
            {
                get
                {
                    // order the subclass array by built-ins then user defined so built-in classes are chosen first
                    return s_SubClasses ??
                        (s_SubClasses = TypeCache.GetTypesDerivedFrom<TEditorClass>().OrderBy(t => t.Assembly == typeof(UnityEditor.Timeline.TimelineEditor).Assembly ? 1 : 0).ToArray());
                }
            }

            public static TEditorClass GetEditorForType(Type type)
            {
                TEditorClass editorClass = null;
                if (!s_TypeMap.TryGetValue(type, out editorClass) || editorClass == null)
                {
                    Type editorClassType = null;
                    Type searchType = type;
                    while (searchType != null)
                    {
                        // search our way up the runtime class hierarchy so we get the best match
                        editorClassType = GetExactEditorClassForType(searchType);
                        if (editorClassType != null)
                            break;
                        searchType = searchType.BaseType;
                    }

                    if (editorClassType == null)
                    {
                        editorClass = s_DefaultInstance;
                    }
                    else
                    {
                        try
                        {
                            editorClass = (TEditorClass)Activator.CreateInstance(editorClassType);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarningFormat("Could not create a Timeline editor class of type {0}: {1}", editorClassType, e.Message);
                            editorClass = s_DefaultInstance;
                        }
                    }

                    s_TypeMap[type] = editorClass;
                }

                return editorClass;
            }

            private static Type GetExactEditorClassForType(Type type)
            {
                foreach (var subClass in SubClasses)
                {
                    // first check for exact match
                    var attr = (CustomTimelineEditorAttribute)Attribute.GetCustomAttribute(subClass, typeof(CustomTimelineEditorAttribute), false);
                    if (attr != null && attr.classToEdit == type)
                    {
                        return subClass;
                    }
                }

                return null;
            }

            public static void Clear()
            {
                s_TypeMap.Clear();
                s_SubClasses = null;
            }
        }

        public static TEditorClass GetEditorForType<TEditorClass, TRuntimeClass>(Type type) where TEditorClass : class, new()
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeof(TRuntimeClass).IsAssignableFrom(type))
                throw new ArgumentException(type.FullName + " does not inherit from" + typeof(TRuntimeClass));

            return SubClassCache<TEditorClass>.GetEditorForType(type);
        }

        public static ClipEditor GetClipEditor(TimelineClip clip)
        {
            if (clip == null)
                throw new ArgumentNullException(nameof(clip));

            var type = typeof(IPlayableAsset);
            if (clip.asset != null)
                type = clip.asset.GetType();

            if (!typeof(IPlayableAsset).IsAssignableFrom(type))
                return GetDefaultClipEditor();

            return GetEditorForType<ClipEditor, IPlayableAsset>(type);
        }

        public static ClipEditor GetDefaultClipEditor()
        {
            return SubClassCache<ClipEditor>.DefaultInstance;
        }
    }
}
