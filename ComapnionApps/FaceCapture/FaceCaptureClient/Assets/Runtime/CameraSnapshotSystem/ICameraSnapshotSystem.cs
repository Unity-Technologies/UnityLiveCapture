using Unity.Collections;
using UnityEngine;

namespace Unity.CompanionApps.FaceCapture
{
    interface ICameraSnapshotSystem
    {
        bool Enabled { get; set; }
        bool IsSupported { get; }

        bool TryGetSnapshot(Allocator allocator, out Texture2D texture);
    }
}
