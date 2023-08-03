using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture.VideoStreaming.Client;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    public class VideoStreamingSystem : IInitializable, ITickable
    {
        [Inject]
        ConnectionModel m_Connection;
        [Inject(Id = "Video Streaming")]
        RawImage m_RawImage;
        [Inject(Id = "Main Camera")]
        Camera m_Camera;

        VideoStreamingClient m_VideoStreamingClient;
        RectTransform m_RectTransform;
        Rect m_UvRect = new Rect(0, 0, 1, 1);

        public Texture2D texture => m_VideoStreamingClient?.texture;

        public bool active => m_Connection.State == ConnectionState.Connected && texture != null;

        public void Initialize()
        {
            if (VideoStreamingClient.IsSupported())
                m_VideoStreamingClient = new VideoStreamingClient();

            m_RectTransform = m_RawImage.GetComponent<RectTransform>();
        }

        public void Tick()
        {
            m_VideoStreamingClient?.Update();

            m_Camera.enabled = !active;

            m_RawImage.enabled = active;
            m_RawImage.texture = texture;

            if (texture != null)
            {
                m_UvRect = VideoStreamUtility.GetUvRect(m_RectTransform, texture, false);

                m_RawImage.uvRect = m_UvRect;
            }
        }

        public void StartVideoStream(int port)
        {
            var url = $"rtsp://{m_Connection.Ip}:{port}";

            m_VideoStreamingClient?.Connect(url);
        }

        public void StopVideoStream()
        {
            m_VideoStreamingClient?.Disconnect();
        }

        public Vector2 ScreenPointToVideoStreamNormalized(Vector2 screenPoint)
        {
            return VideoStreamUtility.ScreenPointToVideoStreamNormalized(screenPoint, m_RectTransform, m_UvRect);
        }

        public Vector2 VideoStreamNormalizedPointToScreen(Vector2 normalizedPoint)
        {
            return VideoStreamUtility.VideoStreamNormalizedPointToScreen(normalizedPoint, m_RectTransform, m_UvRect);
        }
    }
}
