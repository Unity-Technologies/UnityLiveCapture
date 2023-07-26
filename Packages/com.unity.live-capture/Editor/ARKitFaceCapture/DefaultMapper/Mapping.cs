using System;
using System.Collections.Generic;
using System.Linq;
using Unity.LiveCapture.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper.Editor
{
    abstract class Mapping : IDrawable
    {
        protected static class Contents
        {
            public static readonly GUIContent FaceShape = new GUIContent("Blend Shape", "The ARKit blend shape to influence this mesh blend shape with.");
            public static readonly GUIContent MeshShape = new GUIContent("Blend Shape", "The blend shape in the target mesh to influence.");

            public static readonly float LocationWidth = 160f;
            public static readonly float ButtonWidth = 18f;
            public static readonly float BindingSpacing = 6f * EditorGUIUtility.standardVerticalSpacing;
            public static readonly float BindingSeparatorHeight = 2f;
            public static readonly Vector2 OptionDropdownSize = new Vector2(300f, 250f);

            public static readonly Color ConfigBackground1 = new Color(0f, 0f, 0f, 0.04f);
            public static readonly Color ConfigBackground2 = new Color(1f, 1f, 1f, 0.04f);
            public static readonly Color SeparatorColor = new Color(0f, 0f, 0f, 0.1f);
            public static readonly GUIContent IconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add mapping.");
            public static readonly GUIContent IconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove mapping.");
        }

        protected bool m_IsExpanded;
        protected readonly MappingList m_MappingList;

        /// <summary>
        /// The number of bindings under this mapping.
        /// </summary>
        protected abstract int BindingCount { get; }

        protected Mapping(MappingList mappingList)
        {
            m_MappingList = mappingList;
        }

        /// <summary>
        /// Hide the bindings for this mapping in the inspector.
        /// </summary>
        public void Collapse()
        {
            if (m_IsExpanded)
            {
                m_IsExpanded = false;
                GUI.changed = true;
            }
        }

        /// <inheritdoc />
        public float GetHeight()
        {
            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (m_IsExpanded)
            {
                for (var i = 0; i < BindingCount; i++)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    height += GetConfig(i).GetHeight() + Contents.BindingSpacing;
                }
            }

            return height;
        }

        /// <inheritdoc />
        public void OnGUI(Rect rect)
        {
            var pos = new Rect(rect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            var locationRect = new Rect(pos)
            {
                width = Contents.LocationWidth,
            };
            var foldoutRect = new Rect(pos)
            {
                xMin = locationRect.xMax,
                xMax = pos.xMax - Contents.ButtonWidth,
            };
            var buttonRect = new Rect(pos)
            {
                xMin = foldoutRect.xMax,
            };

            OnMappingValueGUI(locationRect);
            OnFoldoutGUI(foldoutRect);
            OnAddBindingGUI(buttonRect);

            if (m_IsExpanded)
            {
                for (var i = 0; i < BindingCount; i++)
                {
                    var config = GetConfig(i);

                    pos.y = pos.yMax;

                    var separatorRect = new Rect
                    {
                        xMin = pos.xMin,
                        xMax = pos.xMax,
                        y = pos.y - Contents.BindingSeparatorHeight / 2f,
                        height = Contents.BindingSeparatorHeight,
                    };
                    var bindingRect = new Rect
                    {
                        xMin = pos.xMin,
                        xMax = pos.xMax - 20f,
                        y = pos.y + (Contents.BindingSpacing / 2f),
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    var configRect = new Rect
                    {
                        xMin = pos.xMin,
                        xMax = pos.xMax - 20f,
                        y = bindingRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                        height = config.GetHeight(),
                    };
                    buttonRect.y = bindingRect.y;

                    pos.yMax = configRect.yMax + (Contents.BindingSpacing / 2f);

                    // draw the shape config background with alternating colors
                    if (Event.current.type == EventType.Repaint)
                    {
                        EditorGUI.DrawRect(pos, i % 2 == 0 ? Contents.ConfigBackground1 : Contents.ConfigBackground2);
                        EditorGUI.DrawRect(separatorRect, Contents.SeparatorColor);
                    }

                    OnBindingGUI(bindingRect, i);
                    config.OnGUI(configRect);

                    if (GUI.Button(buttonRect, Contents.IconToolbarMinus, GUIStyle.none))
                    {
                        RemoveBinding(i);
                    }
                }
            }
        }

        void OnFoldoutGUI(Rect rect)
        {
            string text;
            switch (BindingCount)
            {
                case 0:
                    text = string.Empty;
                    break;
                case 1:
                    text = $"{BindingCount} binding";
                    break;
                default:
                    text = $"{BindingCount} bindings";
                    break;
            }

            var foldoutLabel = new GUIContent(text);

            if (BindingCount == 0)
            {
                EditorGUI.LabelField(rect, foldoutLabel);
            }
            else
            {
                // foldouts don't clip the label, so we do it manually
                using (new GUI.GroupScope(rect))
                {
                    m_IsExpanded = EditorGUI.Foldout(new Rect(0, 0, rect.width, rect.height), m_IsExpanded, foldoutLabel, true);
                }
            }
        }

        protected void DrawLocation(Rect rect, bool drawLabel, FaceBlendShape location, List<FaceBlendShape> unusedLocations, Action<FaceBlendShape> onSelect)
        {
            if (drawLabel)
            {
                rect = EditorGUI.PrefixLabel(rect, Contents.FaceShape);
            }

            var current = location.ToString();
            var content = new GUIContent(current, Contents.FaceShape.tooltip);

            if (GUI.Button(rect, content, EditorStyles.popup))
            {
                var options = unusedLocations
                    .Select(l => new GUIContent(l.ToString()))
                    .ToArray();

                OptionSelectWindow.SelectOption(rect, Contents.OptionDropdownSize, options, (index, value) =>
                {
                    onSelect.Invoke(BlendShapeUtility.GetLocation(value));
                    m_MappingList.ApplyToProperties();
                });
            }
        }

        protected void DrawShapeIndex(Rect rect, bool drawLabel, int shapeIndex, List<int> unusedShapeIndices, Action<int> onSelect)
        {
            var content = m_MappingList.GetShapeName(shapeIndex);

            if (content == null)
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    shapeIndex = EditorGUI.IntField(rect, drawLabel ? Contents.MeshShape : GUIContent.none, shapeIndex);

                    if (change.changed)
                        onSelect?.Invoke(shapeIndex);
                }
            }
            else
            {
                if (drawLabel)
                {
                    rect = EditorGUI.PrefixLabel(rect, Contents.MeshShape);
                }

                if (GUI.Button(rect, new GUIContent(content.text, Contents.MeshShape.tooltip), EditorStyles.popup))
                {
                    var options = unusedShapeIndices
                        .Select(i => m_MappingList.GetShapeName(i))
                        .ToArray();

                    OptionSelectWindow.SelectOption(rect, Contents.OptionDropdownSize, options, (index, value) =>
                    {
                        onSelect.Invoke(m_MappingList.GetShapeIndex(value));
                        m_MappingList.ApplyToProperties();
                    });
                }
            }
        }

        /// <summary>
        /// Gets the bindings defined under this mapping.
        /// </summary>
        public abstract Binding[] GetBindings();

        protected abstract BindingConfig GetConfig(int index);
        protected abstract void RemoveBinding(int index);
        protected abstract void OnMappingValueGUI(Rect rect);
        protected abstract void OnAddBindingGUI(Rect rect);
        protected abstract void OnBindingGUI(Rect rect, int index);
    }

    class LocationMapping : Mapping
    {
        class ShapeIndexBinding
        {
            public int ShapeIndex;
            public BindingConfig Config;
        }

        FaceBlendShape m_Location;
        readonly List<ShapeIndexBinding> m_Bindings;
        readonly List<int> m_UnusedShapeIndices = new List<int>();

        public FaceBlendShape Location => m_Location;

        /// <inheritdoc />
        protected override int BindingCount => m_Bindings.Count;

        public LocationMapping(MappingList mappingList, FaceBlendShape location) : base(mappingList)
        {
            m_Location = location;
            m_Bindings = new List<ShapeIndexBinding>();
            m_IsExpanded = true;

            RefreshUnusedShapeIndices();
        }

        public LocationMapping(MappingList mappingList, FaceBlendShape location, IEnumerable<(int shapeIndex, BindingConfig config, bool isExpanded)> bindings) : base(mappingList)
        {
            m_Location = location;
            m_Bindings = bindings.Select(binding => new ShapeIndexBinding
            {
                ShapeIndex = binding.shapeIndex,
                Config = binding.config,
            }).ToList();
            m_IsExpanded = bindings.Any(binding => binding.isExpanded);

            RefreshUnusedShapeIndices();
        }

        /// <inheritdoc />
        public override Binding[] GetBindings()
        {
            return m_Bindings
                .Select(b => new Binding(m_Location, b.ShapeIndex, b.Config, m_IsExpanded))
                .ToArray();
        }

        /// <inheritdoc />
        protected override BindingConfig GetConfig(int index)
        {
            return m_Bindings[index].Config;
        }

        /// <inheritdoc />
        protected override void RemoveBinding(int index)
        {
            m_Bindings.RemoveAt(index);
            RefreshUnusedShapeIndices();
        }

        /// <inheritdoc />
        protected override void OnMappingValueGUI(Rect rect)
        {
            DrawLocation(rect, false, m_Location, m_MappingList.UnusedLocations, (value) =>
            {
                m_Location = value;
                m_MappingList.RefreshUnusedMappings();
            });
        }

        /// <inheritdoc />
        protected override void OnAddBindingGUI(Rect rect)
        {
            using (new EditorGUI.DisabledScope(m_UnusedShapeIndices.Count == 0))
            {
                if (GUI.Button(rect, Contents.IconToolbarPlus, GUIStyle.none))
                {
                    var unusedShape = m_UnusedShapeIndices[0];

                    m_Bindings.Add(new ShapeIndexBinding
                    {
                        ShapeIndex = unusedShape,
                        Config = new BindingConfig(m_MappingList.DefaultPreset),
                    });

                    RefreshUnusedShapeIndices();
                }
            }
        }

        /// <inheritdoc />
        protected override void OnBindingGUI(Rect rect, int index)
        {
            var binding = m_Bindings[index];

            DrawShapeIndex(rect, true, binding.ShapeIndex, m_UnusedShapeIndices, (value) =>
            {
                binding.ShapeIndex = value;
                RefreshUnusedShapeIndices();
            });
        }

        void RefreshUnusedShapeIndices()
        {
            m_UnusedShapeIndices.Clear();

            for (var i = 0; i < m_MappingList.MeshBlendShapeCount; i++)
            {
                if (!m_Bindings.Any(b => b.ShapeIndex == i))
                {
                    m_UnusedShapeIndices.Add(i);
                }
            }
        }
    }

    class ShapeIndexMapping : Mapping
    {
        class LocationBinding
        {
            public FaceBlendShape Location;
            public BindingConfig Config;
        }

        int m_ShapeIndex;
        readonly List<LocationBinding> m_Bindings;
        readonly List<FaceBlendShape> m_UnusedLocations = new List<FaceBlendShape>();

        public int ShapeIndex => m_ShapeIndex;

        /// <inheritdoc />
        protected override int BindingCount => m_Bindings.Count;

        public ShapeIndexMapping(MappingList mappingList, int shapeIndex) : base(mappingList)
        {
            m_ShapeIndex = shapeIndex;
            m_Bindings = new List<LocationBinding>();
            m_IsExpanded = true;

            RefreshUnusedLocations();
        }

        public ShapeIndexMapping(MappingList mappingList, int shapeIndex, IEnumerable<(FaceBlendShape location, BindingConfig config, bool isExpanded)> bindings) : base(mappingList)
        {
            m_ShapeIndex = shapeIndex;
            m_Bindings = bindings.Select(binding => new LocationBinding
            {
                Location = binding.location,
                Config = binding.config,
            }).ToList();
            m_IsExpanded = bindings.Any(binding => binding.isExpanded);

            RefreshUnusedLocations();
        }

        /// <inheritdoc />
        public override Binding[] GetBindings()
        {
            return m_Bindings
                .Select(b => new Binding(b.Location, m_ShapeIndex, b.Config, m_IsExpanded))
                .ToArray();
        }

        /// <inheritdoc />
        protected override BindingConfig GetConfig(int index)
        {
            return m_Bindings[index].Config;
        }

        /// <inheritdoc />
        protected override void RemoveBinding(int index)
        {
            m_Bindings.RemoveAt(index);
            RefreshUnusedLocations();
        }

        /// <inheritdoc />
        protected override void OnMappingValueGUI(Rect rect)
        {
            DrawShapeIndex(rect, false, m_ShapeIndex, m_MappingList.UnusedShapeIndices, (value) =>
            {
                m_ShapeIndex = value;
                m_MappingList.RefreshUnusedMappings();
            });
        }

        /// <inheritdoc />
        protected override void OnAddBindingGUI(Rect rect)
        {
            using (new EditorGUI.DisabledScope(m_UnusedLocations.Count == 0))
            {
                if (GUI.Button(rect, Contents.IconToolbarPlus, GUIStyle.none))
                {
                    var unusedLocation = m_UnusedLocations[0];

                    m_Bindings.Add(new LocationBinding
                    {
                        Location = unusedLocation,
                        Config = new BindingConfig(m_MappingList.DefaultPreset),
                    });

                    RefreshUnusedLocations();
                }
            }
        }

        /// <inheritdoc />
        protected override void OnBindingGUI(Rect rect, int index)
        {
            var binding = m_Bindings[index];

            DrawLocation(rect, true, binding.Location, m_UnusedLocations, (value) =>
            {
                binding.Location = value;
                RefreshUnusedLocations();
            });
        }

        void RefreshUnusedLocations()
        {
            m_UnusedLocations.Clear();

            foreach (var location in FaceBlendShapePose.Shapes)
            {
                if (!m_Bindings.Any(b => b.Location == location))
                {
                    m_UnusedLocations.Add(location);
                }
            }
        }
    }
}
