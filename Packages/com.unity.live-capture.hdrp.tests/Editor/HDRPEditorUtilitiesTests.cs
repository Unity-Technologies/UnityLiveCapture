using NUnit.Framework;
using Unity.LiveCapture.Rendering.Editor;

namespace Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp
{
    public class HDRPEditorUtilitiesTests
    {
#if !UNITY_2023_2_OR_NEWER
        [Test]
        public void HDRPEditorUtilities_ValidateReflection()
        {
            Assert.IsTrue(HDRPEditorUtilities.ValidateReflection());
        }
#endif
    }
}
