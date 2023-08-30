using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.CompanionApps.FaceCapture
{
    [RequireComponent(typeof(ARCameraManager))]
    class CameraSnapshotSystem : MonoBehaviour, ICameraSnapshotSystem
    {
        ARCameraManager m_CameraManager;
        bool m_IsSupported;

        public bool Enabled
        {
            get => m_CameraManager.enabled;
            set => m_CameraManager.enabled = value;
        }

        public bool IsSupported => m_IsSupported;

        void Awake()
        {
            m_CameraManager = GetComponent<ARCameraManager>();

            m_IsSupported = m_CameraManager.subsystem != null;
        }

        /// <summary>
        /// Based on https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/cpu-camera-image.html
        /// </summary>
        public bool TryGetSnapshot(Allocator allocator, out Texture2D texture)
        {
            if (m_CameraManager.TryAcquireLatestCpuImage(out var image))
            {
                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, image.width, image.height),
                    outputDimensions = new Vector2Int(image.width, image.height),
                    outputFormat = TextureFormat.RGBA32,

                    // Flip across the vertical axis (mirror image)
                    transformation = XRCpuImage.Transformation.MirrorY
                };

                int size = image.GetConvertedDataSize(conversionParams);
                var buffer = new NativeArray<byte>(size, allocator);

                image.Convert(conversionParams, buffer);
                image.Dispose();

                texture = new Texture2D(
                    conversionParams.outputDimensions.x,
                    conversionParams.outputDimensions.y,
                    conversionParams.outputFormat,
                    false);

                texture.LoadRawTextureData(buffer);
                texture.Apply();

                buffer.Dispose();

                return true;
            }

            texture = null;
            return false;
        }
    }
}
