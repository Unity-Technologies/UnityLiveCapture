using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class RoundedCornerBox : UIBehaviour
    {
        const string k_ShaderName = "UI/RoundedCornersWithBorder";
        const string k_ShaderShadowKeyword = "SHADOW_ON";

        static readonly int k_ColorProp = Shader.PropertyToID("_Color");
        static readonly int k_BorderColorProp = Shader.PropertyToID("_BorderColor");
        static readonly int k_ShadowColorProp = Shader.PropertyToID("_ShadowColor");
        static readonly int k_RectTransformSizeProp = Shader.PropertyToID("_RectTransformSize");

        static readonly int k_BorderParams = Shader.PropertyToID("_BorderParams");
        static readonly int k_ShadowParams = Shader.PropertyToID("_ShadowParams");

        public float Border
        {
            get => m_Border;
            set
            {
                m_Border = value;
                m_Dirty = true;
            }
        }

        public float BorderRadius
        {
            get => m_BorderRadius;
            set
            {
                m_BorderRadius = value;
                m_Dirty = true;
            }
        }

        public Color Color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                m_Dirty = true;
            }
        }

        public Color BorderColor
        {
            get => m_BorderColor;
            set
            {
                m_BorderColor = value;
                m_Dirty = true;
            }
        }

        public bool EnableShadow
        {
            get => m_EnableShadow;
            set
            {
                m_EnableShadow = value;
                m_Dirty = true;
            }
        }

        public Color ShadowColor
        {
            get => m_ShadowColor;
            set
            {
                m_ShadowColor = value;
                m_Dirty = true;
            }
        }

        public float ShadowRadius
        {
            get => m_ShadowRadius;
            set
            {
                m_ShadowRadius = value;
                m_Dirty = true;
            }
        }

        public Vector2 ShadowOffset
        {
            get => m_ShadowOffset;
            set
            {
                m_ShadowOffset = value;
                m_Dirty = true;
            }
        }

        [SerializeField]
        float m_Border;
        [SerializeField]
        float m_BorderRadius;
        [SerializeField]
        Color m_Color;
        [SerializeField]
        Color m_BorderColor;
        [SerializeField]
        bool m_EnableShadow;
        [SerializeField]
        Color m_ShadowColor;
        [SerializeField]
        float m_ShadowRadius;
        [SerializeField]
        Vector2 m_ShadowOffset;

        RectTransform m_RectTransform_;
        Graphic m_Graphic_;
        Material m_Material_;
        bool m_Dirty;

        public RectTransform RectTransform
        {
            get { return m_RectTransform_ ?? (m_RectTransform_ = GetComponent<RectTransform>()); }
        }

        public Graphic Graphic
        {
            get { return m_Graphic_ ?? (m_Graphic_ = GetComponent<Graphic>()); }
        }

        public Material Material
        {
            get
            {
                if (m_Material_ == null)
                {
                    var shader = Shader.Find(k_ShaderName);
                    if (shader == null)
                    {
                        throw new InvalidOperationException(
                            $"Could not find shader \"{k_ShaderName}\"." +
                            "Is it added to the Always Included shader list?.");
                    }

                    m_Material_ = new Material(shader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }

                return m_Material_;
            }
        }

        protected override void Awake()
        {
            UpdateMaterial();
        }

        void LateUpdate()
        {
            if (m_Dirty)
            {
                m_Dirty = false;
                UpdateMaterial();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_BorderRadius = Mathf.Max(0, m_BorderRadius);
            m_Border = Mathf.Max(0, m_Border);
            m_ShadowRadius = Mathf.Max(0, m_ShadowRadius);
            UpdateMaterial();
        }
#endif

        protected override void OnDestroy()
        {
            if (m_Material_ != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_Material_);
                }
                else
                {
                    DestroyImmediate(m_Material_);
                }

                m_Material_ = null;
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateMaterial();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            UpdateMaterial();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            UpdateMaterial();
        }

        protected override void OnTransformParentChanged()
        {
            UpdateMaterial();
        }

        void UpdateMaterial()
        {
            var graphic = Graphic;
            graphic.material = Material;

            // We don't use vertex colors at the moment.
            // graphic.color = m_Color;

            UpdateMaterial(Material);

            graphic.SetMaterialDirty();
        }

        void UpdateMaterial(Material material)
        {
            if (m_EnableShadow)
            {
                material.EnableKeyword(k_ShaderShadowKeyword);
            }
            else
            {
                material.DisableKeyword(k_ShaderShadowKeyword);
            }

            material.SetColor(k_ColorProp, m_Color);
            material.SetColor(k_BorderColorProp, m_BorderColor);
            material.SetColor(k_ShadowColorProp, m_ShadowColor);
            material.SetVector(k_BorderParams, new Vector2(m_Border, m_BorderRadius));
            material.SetVector(k_ShadowParams, new Vector3(m_ShadowOffset.x, m_ShadowOffset.y, m_ShadowRadius));
            material.SetVector(k_RectTransformSizeProp, RectTransform.rect.size);
        }
    }
}
