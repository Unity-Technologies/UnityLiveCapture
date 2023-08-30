using System.Collections;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.Rendering;
using UnityEngine;
using UnityEngine.TestTools;

public class UrpFocusPlaneTests
{
    // The purpose of this test is to catch errors related to resources management,
    // such a a failure to instantiate a material for example.
    [UnityTest]
    public IEnumerator CanCreateAndDispose()
    {
        var cameraGameObject = new GameObject("Test Camera", typeof(Camera), typeof(FocusPlaneRenderer));

        RenderPipelineBridge.RequestRenderFeature<VirtualCameraScriptableRenderFeature>();

        yield return null;

        GameObject.DestroyImmediate(cameraGameObject);
    }
}
