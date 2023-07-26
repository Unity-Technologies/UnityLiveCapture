using System;
using System.Collections;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp
{
    public class HdrpFrameLinesTests : BaseCustomPassTests
    {
        [UnityTest]
        public IEnumerator CustomPassIsManagedProperly()
        {
            var it = CustomPassIsManagedProperly<FrameLines, HdrpFrameLinesPass>(CustomPassInjectionPoint.AfterPostProcess);
            while (it.MoveNext())
                yield return null;
        }
    }
}
