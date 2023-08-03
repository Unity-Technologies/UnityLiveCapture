using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using Unity.Collections;

namespace Unity.CompanionApps.FaceCapture
{
    interface IARFaceManagerSystem
    {
        event Action<ARFace> FaceRemoved;
        event Action<ARFace> FaceUpdated;
        event Action<ARFace> FaceAdded;

        bool Enabled { get; set; }
        bool IsSupported { get; }

        bool RequestWorldAlignment(ARWorldAlignment alignment);
        NativeArray<ARKitBlendShapeCoefficient> GetBlendShapeCoefficients(ARFace face, Allocator allocator);
    }
}
