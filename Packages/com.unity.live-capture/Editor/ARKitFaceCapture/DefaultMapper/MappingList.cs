using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper.Editor
{
    class MappingList
    {
        readonly SerializedObject m_SerializedObject;
        readonly SerializedProperty m_Bindings;
        readonly SerializedProperty m_IsExpanded;
        readonly SerializedProperty m_DefaultEvaluator;
        readonly GUIContent[] m_MeshBlendShapeNames;

        bool m_IsInverted;

        ReorderableList m_ReorderableList;
        readonly Dictionary<int, float> m_ElementHeightCache = new Dictionary<int, float>();

        List<LocationMapping> m_LocationMappings;
        List<ShapeIndexMapping> m_ShapeIndexMappings;

        /// <summary>
        /// The preset evaluation function to assign to new bindings.
        /// </summary>
        public EvaluatorPreset DefaultPreset => m_DefaultEvaluator.objectReferenceValue as EvaluatorPreset;

        /// <summary>
        /// The number of blend shapes defined in the mesh this mapping applies to.
        /// </summary>
        public int MeshBlendShapeCount => m_MeshBlendShapeNames.Length;

        public List<FaceBlendShape> UnusedLocations { get; } = new List<FaceBlendShape>();
        public List<int> UnusedShapeIndices { get; } = new List<int>();

        /// <summary>
        /// Is the mapping list shown in the inspector.
        /// </summary>
        public bool IsExpanded
        {
            get => m_IsExpanded.boolValue;
            set => m_IsExpanded.boolValue = value;
        }

        public MappingList(
            SerializedProperty rendererMapping,
            SerializedProperty defaultEvaluator,
            bool isInverted,
            GUIContent[] meshBlendShapeNames)
        {
            m_SerializedObject = rendererMapping.serializedObject;
            m_Bindings = rendererMapping.FindPropertyRelative("m_Bindings");
            m_IsExpanded = rendererMapping.FindPropertyRelative("m_IsExpanded");

            m_DefaultEvaluator = defaultEvaluator;
            m_IsInverted = isInverted;
            m_MeshBlendShapeNames = meshBlendShapeNames;

            // The serialized bindings array changes order depending on if the list is inverted or not.
            // However, we need to load, overwrite, and reload the bindings so that the binding config instances
            // are applied to the serialized bindings array in the order they currently exist in the serialized array.
            // This is a result of us using working directly with the actual binding config references via reflection.
            // Other work around could be to deep copy the binding configs, use serialized references, or make the binding
            // config a value type, but this approach is the best performance.
            UpdateFromProperties();
            ApplyToProperties();
            UpdateFromProperties();
        }

        /// <summary>
        /// Gets the index of a blend shape in the mesh.
        /// </summary>
        /// <param name="shapeName">The name of the blend shape.</param>
        /// <returns>The blend shape index, or -1 if not valid for the current mesh.</returns>
        public int GetShapeIndex(string shapeName)
        {
            for (var i = 0; i < m_MeshBlendShapeNames.Length; i++)
            {
                if (m_MeshBlendShapeNames[i].text == shapeName)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the name of a mesh blend shape.
        /// </summary>
        /// <param name="shapeIndex">The index of the blend shape in the mesh.</param>
        /// <returns>The blend shape name, or null if the index is not valid for the current mesh.</returns>
        public GUIContent GetShapeName(int shapeIndex)
        {
            if (m_MeshBlendShapeNames == null || shapeIndex < 0 || shapeIndex >= m_MeshBlendShapeNames.Length)
                return null;

            return m_MeshBlendShapeNames[shapeIndex];
        }

        /// <summary>
        /// Determines which locations or mesh blend shapes are not assigned to any mapping in the list.
        /// </summary>
        public void RefreshUnusedMappings()
        {
            if (m_IsInverted)
            {
                UnusedShapeIndices.Clear();

                for (var i = 0; i < m_MeshBlendShapeNames.Length; i++)
                {
                    if (!m_ShapeIndexMappings.Any(b => b.ShapeIndex == i))
                    {
                        UnusedShapeIndices.Add(i);
                    }
                }
            }
            else
            {
                UnusedLocations.Clear();

                foreach (var location in FaceBlendShapePose.Shapes)
                {
                    if (!m_LocationMappings.Any(m => m.Location == location))
                    {
                        UnusedLocations.Add(location);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the bindings GUI.
        /// </summary>
        public void OnGUI()
        {
            m_ElementHeightCache.Clear();

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                Profiler.BeginSample($"DoLayoutList()");

                m_ReorderableList.DoLayoutList();

                Profiler.EndSample();

                if (change.changed)
                {
                    ApplyToProperties();
                }
            }
        }

        /// <summary>
        /// Applies the current bindings to the serialized object.
        /// </summary>
        public void ApplyToProperties()
        {
            var mappings = m_IsInverted ? m_ShapeIndexMappings.Cast<Mapping>() : m_LocationMappings.Cast<Mapping>();

            var bindings = mappings
                .SelectMany(m => m.GetBindings())
                .ToArray();

            m_Bindings.SetValue(bindings);
            m_SerializedObject.ApplyModifiedProperties();
        }

        void UpdateFromProperties()
        {
            var bindings = m_Bindings.GetValue<Binding[]>();

            if (m_IsInverted)
            {
                m_LocationMappings = null;

                m_ShapeIndexMappings = bindings
                    .ToLookup(b => b.ShapeIndex, b => (location: b.Location, config: b.Config, isExpanded: b.IsExpanded))
                    .Select(mapping => new ShapeIndexMapping(this, mapping.Key, mapping))
                    .ToList();

                CreateReorderableList(m_ShapeIndexMappings);
            }
            else
            {
                m_ShapeIndexMappings = null;

                m_LocationMappings = bindings
                    .ToLookup(b => b.Location, b => (shapeIndex: b.ShapeIndex, config: b.Config, isExpanded: b.IsExpanded))
                    .Select(mapping => new LocationMapping(this, mapping.Key, mapping))
                    .ToList();

                CreateReorderableList(m_LocationMappings);
            }

            RefreshUnusedMappings();
        }

        void CreateReorderableList<T>(List<T> list) where T : Mapping
        {
            m_ReorderableList = new ReorderableList(list, typeof(T), true, false, true, true)
            {
                headerHeight = 0f,
                elementHeightCallback = (index) =>
                {
                    if (!m_ElementHeightCache.TryGetValue(index, out var height))
                    {
                        height = list[index].GetHeight();
                        m_ElementHeightCache.Add(index, height);
                    }
                    return height;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    list[index].OnGUI(rect);

                    // block dragging/selection of the list item in the item rect
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                    }
                },
                onMouseDragCallback = (_) =>
                {
                    // Collapse all elements so that they all have same height wile dragging,
                    // since the list doesn't behave well with heterogeneous element sizes.
                    foreach (var item in list)
                        item.Collapse();
                },
                onReorderCallback = (_) =>
                {
                    GUI.changed = true;
                },
                onCanAddCallback = (_) => CanAddMapping(),
                onAddCallback = (_) => AddMapping(),
                onRemoveCallback = (l) => RemoveMapping(l.index),
            };
        }

        bool CanAddMapping()
        {
            return m_IsInverted ? UnusedShapeIndices.Count > 0 : UnusedLocations.Count > 0;
        }

        void AddMapping()
        {
            if (m_IsInverted)
            {
                m_ShapeIndexMappings.Add(new ShapeIndexMapping(this, UnusedShapeIndices[0]));
            }
            else
            {
                m_LocationMappings.Add(new LocationMapping(this, UnusedLocations[0]));
            }

            RefreshUnusedMappings();
        }

        void RemoveMapping(int index)
        {
            if (m_IsInverted)
            {
                m_ShapeIndexMappings.RemoveAt(index);
            }
            else
            {
                m_LocationMappings.RemoveAt(index);
            }

            RefreshUnusedMappings();
        }
    }
}
