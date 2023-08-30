using System;
using System.Collections;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp
{
    public class HdrpFocusPlaneTests : BaseCustomPassTests
    {
        [UnityTest]
        public IEnumerator CustomPassesAreManagedProperly()
        {
            {
                var it = CustomPassIsManagedProperly<FocusPlaneRenderer, HdrpFocusPlaneRenderPass>(CustomPassInjectionPoint.BeforePostProcess);
                while (it.MoveNext())
                    yield return null;
            }
            {
                // Do one test in play mode, to check for possible behavior changes of the CustomPassManager.
                yield return new EnterPlayMode();

                var it = CustomPassIsManagedProperly<FocusPlaneRenderer, HdrpFocusPlaneComposePass>(CustomPassInjectionPoint.AfterPostProcess);
                while (it.MoveNext())
                    yield return null;

                yield return new ExitPlayMode();
            }
        }
    }
}
