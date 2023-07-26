using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    // A base class for all render-pipeline specifics raycaster implementations.
    abstract class BaseRaycasterImpl : IRaycasterImpl
    {
        // Object picking is regular graphics raycasting with extra steps.
        // This temporary object implements these extra steps, namely:
        // - Before raycasting: assign object ids to renderers, and build a map of ids to objects.
        // - After raycasting: clear the map.
        protected class PickingScope : IDisposable
        {
            const string k_SamplerName = "Set Graphics Picking Ids";
            static readonly int k_ObjectIdProp = Shader.PropertyToID("_ObjectID");

            // No concurrent access expected.
            static Dictionary<int, GameObject> s_IdToGameObjectMap = new Dictionary<int, GameObject>();
            static Dictionary<object, PickingScope> s_OwnerToInstanceMap = new Dictionary<object, PickingScope>();
            static MaterialPropertyBlock s_PropertyBlock = new MaterialPropertyBlock();
            static CustomSampler s_Sampler;

            object m_Owner;

            public static bool IsActive(object owner) => s_OwnerToInstanceMap.ContainsKey(owner);

            public PickingScope(object owner)
            {
                if (s_OwnerToInstanceMap.ContainsKey(owner))
                    throw new InvalidOperationException(
                        $"At most one {nameof(PickingScope)} is expected to be instantiated for a given owner at any time.");

                m_Owner = owner;
                s_OwnerToInstanceMap.Add(m_Owner, this);

                if (s_Sampler == null)
                    s_Sampler = CustomSampler.Create(k_SamplerName);

                s_Sampler.Begin();
                UpdateObjectIds();
                s_Sampler.End();
            }

            public void Dispose()
            {
                s_OwnerToInstanceMap.Remove(m_Owner);
                s_IdToGameObjectMap.Clear();
                m_Owner = null;
            }

            public bool TryGetObject(int id, out GameObject gameObject)
            {
                return s_IdToGameObjectMap.TryGetValue(id, out gameObject);
            }

            void UpdateObjectIds()
            {
                s_IdToGameObjectMap.Clear();

                foreach (var renderer in Resources.FindObjectsOfTypeAll<Renderer>())
                {
                    if (renderer.gameObject.scene.IsValid())
                    {
                        var id = renderer.gameObject.GetInstanceID();
                        var encodedId = EncodeId(id);
                        renderer.GetPropertyBlock(s_PropertyBlock);
                        s_PropertyBlock.SetVector(k_ObjectIdProp, encodedId);
                        renderer.SetPropertyBlock(s_PropertyBlock);
                        s_IdToGameObjectMap.Add(id, renderer.gameObject);
                    }
                }
            }
        }

        const float k_MaxRawDepthFarPlaneError = 10e-6f;

        protected GameObject m_Raycaster;
        protected Camera m_Camera;
        protected RenderTexture m_DepthTexture;
        protected RenderTexture m_DepthAsColorTexture;
        protected RenderTexture m_ColorTexture;

        CustomSampler m_RaycastSampler;
        CustomSampler m_PickingSampler;
        Shader m_PickingShader;
        Material m_PickingMaterial;

        /// <inheritdoc/>
        public virtual void Initialize()
        {
            if (m_RaycastSampler == null)
            {
                m_RaycastSampler = CustomSampler.Create("Graphics Raycast", true);
            }

            if (m_PickingSampler == null)
            {
                m_PickingSampler = CustomSampler.Create("Graphics Picking", true);
            }

            m_Raycaster = new GameObject("Graphics Raycaster")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            m_ColorTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "Graphics Raycast Color Buffer",
                useMipMap = false,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            m_DepthTexture = new RenderTexture(1, 1, 24, RenderTextureFormat.Depth)
            {
                name = "Graphics Raycast Depth Buffer",
                useMipMap = false,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            m_DepthAsColorTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
            {
                name = "Graphics Raycast Depth Color Buffer",
                useMipMap = false,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            // Create a camera to render to the 1x1 depth buffer. Since the depth texture is just 1 texel,
            // the field of view can be set very small as allowed to achieve better culling.
            m_Camera = m_Raycaster.AddComponent<Camera>();
            m_Camera.clearFlags = CameraClearFlags.Depth;
            m_Camera.enabled = false;
            m_Camera.allowHDR = false;
            m_Camera.allowMSAA = false;
            m_Camera.usePhysicalProperties = false;
            m_Camera.fieldOfView = 0.01f;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            AdditionalCoreUtils.DestroyIfNeeded(ref m_Raycaster);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_DepthTexture);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_DepthAsColorTexture);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_ColorTexture);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_PickingMaterial);
        }

        // Lazily instantiated since it will be invoked rarely if at all.
        protected Shader GetPickingShader()
        {
            if (m_PickingShader == null)
            {
                m_PickingShader = Shader.Find("Hidden/LiveCapture/ObjectPicking");
                Assert.IsNotNull(m_PickingShader);
                Assert.IsTrue(m_PickingShader.isSupported);
            }

            return m_PickingShader;
        }

        // Lazily instantiated since it will be invoked rarely if at all.
        protected Material GetPickingMaterial()
        {
            if (m_PickingMaterial == null)
            {
                m_PickingMaterial = new Material(GetPickingShader());
                m_PickingMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return m_PickingMaterial;
        }

        /// <inheritdoc/>
        public bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float minDistance, float maxDistance, int layerMask)
        {
            using (new CustomSamplerScope(m_RaycastSampler))
            {
                // Place the camera at the ray origin, looking in its direction.
                m_Camera.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
                m_Camera.nearClipPlane = minDistance;
                m_Camera.farClipPlane = maxDistance;
                m_Camera.cullingMask = layerMask;
                // We need to reset the pixel rect, it is not stored directly in the camera,
                // but inferred from the viewport, screen size (since we do not assign a persistent targetTexture),
                // therefore it can change under our feet, and go below one pixel, leading to errors.
                m_Camera.pixelRect = new Rect(0, 0, 1, 1);

                Render();

                var rawDepth = ReadDepthSample();

                // Their may be *significant imprecisions* as depth gets closer to the far plane.
                // So we use the raw depth, which is zero at the far plane.
                if (rawDepth < k_MaxRawDepthFarPlaneError)
                {
                    hit = default;
                    return false;
                }

                var depth = LinearEyeDepth(rawDepth, minDistance, maxDistance);

                hit = new RaycastHit
                {
                    distance = depth,
                    point = origin + (depth * direction),
                    normal = -direction,
                };

                return true;
            }
        }

        /// <inheritdoc/>
        public bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, out GameObject gameObject, float minDistance, float maxDistance, int layerMask)
        {
            using (new CustomSamplerScope(m_PickingSampler))
            using (var pickingScope = new PickingScope(this))
            {
                if (Raycast(origin, direction, out hit, minDistance, maxDistance, layerMask))
                {
                    var color = ReadColorSample();
                    var id = DecodeId(color);
                    if (pickingScope.TryGetObject(id, out var result))
                    {
                        gameObject = result;
                        return true;
                    }
                }
            }

            hit = default;
            gameObject = null;
            return false;
        }

        protected abstract void Render();

        float ReadDepthSample()
        {
            var readback = AsyncGPUReadback.Request(m_DepthAsColorTexture);
            readback.WaitForCompletion();
            if (readback.hasError)
                return 0;
            return readback.GetData<float>()[0];
        }

        Color32 ReadColorSample()
        {
            var readback = AsyncGPUReadback.Request(m_ColorTexture);
            readback.WaitForCompletion();
            if (readback.hasError)
                return Color.clear;
            return readback.GetData<Color32>()[0];
        }

        static Vector4 EncodeId(int uid)
        {
            var x = ((uid >> 0) & 0xff) / 255.0f;
            var y = ((uid >> 8) & 0xff) / 255.0f;
            var z = ((uid >> 16) & 0xff) / 255.0f;
            var w = ((uid >> 24) & 0xff) / 255.0f;
            return (new Vector4(x, y, z, w));
        }

        static int DecodeId(Color32 color)
        {
            var x = (int)color.r;
            var y = (int)color.g;
            var z = (int)color.b;
            var w = (int)color.a;
            return ((w << 24) | (z << 16) | (y << 8) | x);
        }

        static float LinearEyeDepth(float rawDepth, float near, float far)
        {
            return 1.0f / ((far - near) * rawDepth / (near * far) + 1.0f / far);
        }
    }
}
