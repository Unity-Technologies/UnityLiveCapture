using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.LiveCapture.Tests.Editor
{
    class ShaderTests
    {
        [Test]
        public void FocusPlaneShadersAreSupported()
        {
            bool ShaderIsSupported(string path)
            {
                var shader = Shader.Find(path);
                Assert.IsNotNull(shader);
                return shader.isSupported;
            }

            Assert.IsTrue(ShaderIsSupported(FocusPlaneRenderer.k_RenderShaderPath));
            Assert.IsTrue(ShaderIsSupported(FocusPlaneRenderer.k_ComposeShaderPath));
        }
    }
}
