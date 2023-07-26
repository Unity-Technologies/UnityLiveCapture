using UnityEngine;

namespace Unity.CompanionAppCommon
{
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas))]
    public class CustomScaler : MonoBehaviour
    {
        const float k_ReferenceDeviceDpi = 264;
        const float k_LogicalToPhysicalPixelsFactor = 2;
        const float k_ReferencePixelsPerUnit = 100;

#pragma warning disable 649
        [SerializeField, Tooltip("Reference device's dpi.")]
        float m_ReferenceDeviceDpi = k_ReferenceDeviceDpi;
        [SerializeField, Tooltip("Scalar factor converting from logical to physical pixels.")]
        float m_LogicalToPhysicalPixelsFactor = k_LogicalToPhysicalPixelsFactor;
        [SerializeField, Tooltip("If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.")]
        float m_ReferencePixelsPerUnit = k_ReferencePixelsPerUnit;
#pragma warning restore 649

        protected Canvas m_Canvas;
        bool m_NeedsUpdate = true;
#if UNITY_EDITOR
        float m_PrevDpi = -1;
#endif

        void Reset()
        {
            m_ReferenceDeviceDpi = k_ReferenceDeviceDpi;
            m_LogicalToPhysicalPixelsFactor = k_LogicalToPhysicalPixelsFactor;
            m_ReferencePixelsPerUnit = k_ReferencePixelsPerUnit;
        }

        void Awake() { m_Canvas = GetComponent<Canvas>(); }

        void OnEnable() { m_NeedsUpdate = true; }

        void OnValidate() { m_NeedsUpdate = true; }

        void Update()
        {
            // Screen properties may change in Editor but not on device,
            // we don't have dynamic orientation so far.
#if UNITY_EDITOR
            var dpi = Screen.dpi;
            m_NeedsUpdate |= dpi != m_PrevDpi;
            m_PrevDpi = dpi;
#endif
            // Scaling cannot happen at any point in a frame. Should be in update.
            if (m_NeedsUpdate)
                UpdateCanvasScaling();
            m_NeedsUpdate = false;
        }

        void UpdateCanvasScaling()
        {
            var screenDpi = Screen.dpi;
            var dpi = screenDpi != 0 ? screenDpi : m_ReferenceDeviceDpi;

            // The reason we cannot use uGUI's built-in Canvas Scaler is because our design (and UI) uses logical pixels.
            // We must convert those logical pixels to physical pixels then to physical units to determine scaling.
            var logicalPixelsToInchesReciprocal = m_ReferenceDeviceDpi / m_LogicalToPhysicalPixelsFactor;

            var scaleFactor = dpi / logicalPixelsToInchesReciprocal;

            m_Canvas.scaleFactor = scaleFactor;
            m_Canvas.referencePixelsPerUnit = m_ReferencePixelsPerUnit;
        }
    }
}
