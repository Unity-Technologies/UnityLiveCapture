using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera
{
    static class SensorPresetsCacheProxy
    {
        public static Func<Vector2, string> GetSensorSizeName = x => String.Empty;
    }
}
