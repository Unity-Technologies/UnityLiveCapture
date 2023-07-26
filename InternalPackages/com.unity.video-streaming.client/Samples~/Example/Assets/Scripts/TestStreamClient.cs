using System.Collections;
using System.Collections.Generic;
using Unity.Ig.VideoStreaming.Client;
using UnityEngine;

[RequireComponent(typeof(VideoStreamingClient))]
public class TestStreamClient : MonoBehaviour
{
    VideoStreamingClient m_StreamClient;
    
    void Awake()
    {
        m_StreamClient = GetComponent<VideoStreamingClient>();
    }

    void OnGUI()
    {
        var tex = m_StreamClient.Target;
        if (tex != null)
        {
            GUI.DrawTexture(new Rect(0, 0, 
                Mathf.Min(Screen.width, tex.width), 
                Mathf.Min(Screen.height, tex.height)), tex);
        }
    }
}
