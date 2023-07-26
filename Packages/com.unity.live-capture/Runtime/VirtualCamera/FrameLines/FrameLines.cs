using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;

#endif

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Map between Camera and FrameLines components.
    /// </summary>
    class FrameLinesMap : ComponentMap<Camera, FrameLines> { }

    /// <summary>
    /// A Component that displays frame lines which helps visualize the gate crop of the sensor
    /// and the aspect ratio crop of the target screen.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Live Capture/Virtual Camera/Frame Lines")]
    [RequireComponent(typeof(Camera))]
    [HelpURL(Documentation.baseURL + "ref-component-frame-lines" + Documentation.endURL)]
    public class FrameLines : MonoBehaviour
    {
        // Name used for profiling.
        internal const string k_ProfilingSamplerLabel = "Frame Lines";

        [SerializeField]
        FrameLinesSettings m_Settings;

        Camera m_Camera;
        bool m_GeometryIsValid;
        readonly FrameLinesDrawer m_FrameLinesDrawer = new FrameLinesDrawer();

        // Caching so that we can update the geometry when needed.
        FrameLinesSettings m_CachedSettings;
        float m_CachedScreenAspect;
        float m_CachedGateAspect;
        Vector2 m_CameraPixelSize;

#if HDRP_14_0_OR_NEWER
        CustomPassManager.Handle<HdrpFrameLinesPass> m_CustomPassHandle;
#endif
        /// <summary>
        /// The gate fit mode.
        /// </summary>
        public GateFit GateFit
        {
            get => m_Settings.GateFit;
            set => m_Settings.GateFit = value;
        }

        /// <summary>
        /// Whether or not to show the crop aspect ratio.
        /// </summary>
        public bool GateMaskEnabled
        {
            get => m_Settings.GateMaskEnabled;
            set => m_Settings.GateMaskEnabled = value;
        }

        /// <summary>
        /// Whether or not to show the crop aspect ratio.
        /// </summary>
        public bool AspectRatioEnabled
        {
            get => m_Settings.AspectRatioLinesEnabled;
            set => m_Settings.AspectRatioLinesEnabled = value;
        }

        /// <summary>
        /// Whether or not to show the center marker.
        /// </summary>
        public bool CenterMarkerEnabled
        {
            get => m_Settings.CenterMarkerEnabled;
            set => m_Settings.CenterMarkerEnabled = value;
        }

        /// <summary>
        /// The aspect ratio of the crop.
        /// </summary>
        public float CropAspect
        {
            get => m_Settings.AspectRatio;
            set => m_Settings.AspectRatio = value;
        }

        /// <summary>
        ///  Checks whether the frame lines should be rendered.
        /// </summary>
        /// <returns>True if the frame lines should render</returns>
        internal bool ShouldRender() => isActiveAndEnabled && m_GeometryIsValid;

        void Reset()
        {
            m_Settings = FrameLinesSettings.GetDefault();
        }

        void OnValidate()
        {
            m_Settings.Validate();
        }

        void OnEnable()
        {
            m_Camera = GetComponent<Camera>();
            m_FrameLinesDrawer.Initialize();

            m_CachedSettings = m_Settings;

            // Force update
            m_CachedSettings.AspectFillOpacity = (m_CachedSettings.AspectFillOpacity + .5f) % 1f;

#if HDRP_14_0_OR_NEWER
            m_CustomPassHandle = new CustomPassManager.Handle<HdrpFrameLinesPass>(CustomPassInjectionPoint.AfterPostProcess);
            m_CustomPassHandle.GetPass().name = k_ProfilingSamplerLabel;
#endif
            FrameLinesMap.Instance.AddUniqueInstance(m_Camera, this);
        }

        void OnDisable()
        {
            FrameLinesMap.Instance.RemoveInstance(this);
#if HDRP_14_0_OR_NEWER
            m_CustomPassHandle.Dispose();
#endif
            m_FrameLinesDrawer.Dispose();
        }

        void LateUpdate()
        {
            UpdateCamera();
        }

        void UpdateCamera()
        {
            ValidateGateFit();

            m_Camera.gateFit = m_Settings.GateFit == GateFit.Overscan ? Camera.GateFitMode.Overscan : Camera.GateFitMode.Fill;
            m_CameraPixelSize = new Vector2(m_Camera.pixelWidth, m_Camera.pixelHeight);

            var screenAspect = m_Camera.pixelWidth / (float)m_Camera.pixelHeight;
            var sensorSize = m_Camera.sensorSize;
            var gateAspect = sensorSize.x / sensorSize.y;

            m_GeometryIsValid = screenAspect > float.Epsilon && gateAspect > float.Epsilon;
            var geometryChanged = false;

            if (m_GeometryIsValid)
            {
                geometryChanged = UpdateGeometryIfNeeded(gateAspect, screenAspect);
            }
        }

        void ValidateGateFit()
        {
            m_Settings.GateFit = m_Camera.usePhysicalProperties
                ? m_Settings.GateFit
                : GateFit.Fill;
        }

        /// <summary>
        /// Draw active frame lines.
        /// </summary>
        /// <param name="cmd">The command buffer to append frame lines drawing commands to.</param>
        internal void Render(CommandBuffer cmd)
        {
            Assert.IsTrue(m_GeometryIsValid, $"{nameof(Render)} should not be called when geometry is not valid.");

            // Geometry is submitted in pixel space.
            var projection = Matrix4x4.Ortho(0, m_CameraPixelSize.x, 0, m_CameraPixelSize.y, 1, -100);

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, projection);

            m_FrameLinesDrawer.Render(cmd);
        }

        bool UpdateGeometryIfNeeded(float gateAspect, float screenAspect)
        {
            var geometryNeedsUpdate =
                !Mathf.Approximately(gateAspect, m_CachedGateAspect) ||
                !Mathf.Approximately(screenAspect, m_CachedScreenAspect) ||
                m_CachedSettings != m_Settings;

            if (!geometryNeedsUpdate)
            {
                return false;
            }

            m_CachedGateAspect = gateAspect;
            m_CachedScreenAspect = screenAspect;
            m_CachedSettings = m_Settings;

            m_FrameLinesDrawer.Clear();

            // We start with a (1, 1) view size, we'll shrink it down according to gate and crop.
            // Coordinates are computed in NDC space.
            Vector2 gateViewSize;
            Vector2 cropViewSize;

            // The sensor gate mask is only visible using overscan.
            if (m_Settings.GateFit == GateFit.Overscan)
            {
                gateViewSize = ApplyAspectRatio(Vector2.one, screenAspect, gateAspect);
                // The crop mask is evaluated within the sensor gate.
                cropViewSize = ApplyAspectRatio(gateViewSize, screenAspect * gateViewSize.x / gateViewSize.y, m_Settings.AspectRatio);

                // In case the sensor gate mask is disabled but the crop mask is visible, render the sensor gate mask with the crop mask's opacity,
                // which amounts to having the crop mask cover the sensor gate mask area so that is reaches the edges of the screen.
                var showGateMask = m_Settings.GateMaskEnabled && m_Settings.GateMaskOpacity > 0f;
                var showCropFill = m_Settings.AspectRatioLinesEnabled && m_Settings.AspectFillOpacity > 0f;
                if (showGateMask || showCropFill)
                {
                    var opacity = showGateMask ? m_Settings.GateMaskOpacity : m_Settings.AspectFillOpacity;
                    // Gate mask letterbox.
                    m_FrameLinesDrawer.SetColor(new Color(0, 0, 0, opacity));

                    DrawLetterBox(Vector2.one, gateViewSize);
                }
            }
            else
            {
                // The sensor gate is ignored while evaluating the crop mask.
                gateViewSize = Vector2.one;
                cropViewSize = ApplyAspectRatio(Vector2.one, screenAspect, m_Settings.AspectRatio);
            }

            if (m_Settings.AspectRatioLinesEnabled)
            {
                // Crop mask letterbox.
                m_FrameLinesDrawer.SetColor(new Color(0, 0, 0, m_Settings.AspectFillOpacity));

                DrawLetterBox(gateViewSize, cropViewSize);

                // Also used for the marker.
                m_FrameLinesDrawer.SetColor(m_Settings.AspectLineColor);
                m_FrameLinesDrawer.SetLineWidth(m_Settings.AspectLineWidth);

                // Crop mask lines.
                if (m_Settings.AspectLineType != FrameLinesSettings.LineType.None)
                {
                    if (m_Settings.AspectLineType == FrameLinesSettings.LineType.Box)
                    {
                        var cropRect = Rect.MinMaxRect(-cropViewSize.x, -cropViewSize.y, cropViewSize.x, cropViewSize.y);

                        m_FrameLinesDrawer.DrawBox(NdcToPixels(cropRect, m_CameraPixelSize));
                    }
                    else if (m_Settings.AspectLineType == FrameLinesSettings.LineType.Corner)
                    {
                        var cropRect = Rect.MinMaxRect(-cropViewSize.x, -cropViewSize.y, cropViewSize.x, cropViewSize.y);
                        var pixelBox = NdcToPixels(cropRect, m_CameraPixelSize);
                        var extent = new Vector2(pixelBox.width * .1f, pixelBox.height * .1f);

                        m_FrameLinesDrawer.DrawCornerBox(pixelBox, extent);
                    }
                }
            }

            if (m_Settings.CenterMarkerEnabled)
            {
                m_FrameLinesDrawer.SetColor(m_Settings.AspectLineColor);
                m_FrameLinesDrawer.SetLineWidth(m_Settings.AspectLineWidth);

                // Marker.
                if (m_Settings.CenterMarkerType == FrameLinesSettings.MarkerType.Cross)
                {
                    var center = m_CameraPixelSize * .5f;
                    var inner = Mathf.Min(m_CameraPixelSize.x * .02f, m_CameraPixelSize.y * .02f);
                    var outer = Mathf.Min(m_CameraPixelSize.x * .06f, m_CameraPixelSize.y * .06f);
                    m_FrameLinesDrawer.DrawCross(center, inner, outer);
                }
                else if (m_Settings.CenterMarkerType == FrameLinesSettings.MarkerType.Dot)
                {
                    var center = m_CameraPixelSize * .5f;
                    var extent = Mathf.Max(2, Mathf.Min(m_CameraPixelSize.x * .02f, m_CameraPixelSize.y * .02f));

                    m_FrameLinesDrawer.DrawRect(Rect.MinMaxRect(
                        center.x - extent,
                        center.y - extent,
                        center.x + extent,
                        center.y + extent));
                }
            }

            m_FrameLinesDrawer.UpdateGeometry();

            return true;
        }

        void DrawLetterBox(Vector2 outerSize, Vector2 innerSize)
        {
            var screenAspect = outerSize.x / outerSize.y;
            var gateAspect = innerSize.x / innerSize.y;

            if (screenAspect > gateAspect) // Pillar box
            {
                var dx = outerSize.x - innerSize.x;

                m_FrameLinesDrawer.DrawRect(NdcToPixels(new Rect(-outerSize.x, -outerSize.y, dx, outerSize.y * 2), m_CameraPixelSize)); // left

                m_FrameLinesDrawer.DrawRect(NdcToPixels(new Rect(innerSize.x, -outerSize.y, dx, outerSize.y * 2), m_CameraPixelSize)); // right
            }
            else
            {
                var dy = outerSize.y - innerSize.y;

                m_FrameLinesDrawer.DrawRect(NdcToPixels(new Rect(-outerSize.x, -outerSize.y, outerSize.x * 2, dy), m_CameraPixelSize)); // top

                m_FrameLinesDrawer.DrawRect(NdcToPixels(new Rect(-outerSize.x, innerSize.y, outerSize.x * 2, dy), m_CameraPixelSize)); // bottom
            }
        }

        static Rect NdcToPixels(Rect value, Vector2 size)
        {
            var pxMin = (value.min + Vector2.one) * .5f * size;
            var pxMax = (value.max + Vector2.one) * .5f * size;

            return Rect.MinMaxRect(pxMin.x, pxMin.y, pxMax.x, pxMax.y);
        }

        static Vector2 ApplyAspectRatio(Vector2 size, float outerAspect, float innerAspect)
        {
            if (innerAspect < float.Epsilon || outerAspect < float.Epsilon)
            {
                return size;
            }

            if (outerAspect > innerAspect) // Vertical
            {
                size.x *= innerAspect / outerAspect;
            }
            else // Horizontal
            {
                size.y *= outerAspect / innerAspect;
            }

            return size;
        }
    }
}
