using Unity.CompanionAppCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.FaceCapture
{
    class BackgroundOverlayView : DialogView, IBackgroundOverlayView
    {
        [SerializeField]
        RawImage m_Image;

        AspectRatioFitter m_Fitter;

        public Texture2D Texture
        {
            set
            {
                var newTexture = value;
                m_Image.texture = newTexture;

                if (newTexture == null)
                {
                    Hide();
                    return;
                }

                Show();

                var aspectRatio = newTexture.width / (float)newTexture.height;
                m_Fitter.aspectRatio = aspectRatio;

                // To compensate for the 90 degree rotation that affects the fitter
                var scale = m_Image.rectTransform.localScale;
                scale.x = scale.y = 1f / aspectRatio;
                m_Image.rectTransform.localScale = scale;
            }
        }

        public void DisposeTexture()
        {
            if (m_Image.texture != null)
            {
                Destroy(m_Image.texture);
                m_Image.texture = null;
            }
        }

        void Awake()
        {
            m_Fitter = m_Image.GetComponent<AspectRatioFitter>();
        }
    }
}
