using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class EditorPlaceholder
    {
        // A Test behaves as an ordinary method
        [Test]
        public void EditorPlaceholderSimplePasses()
        {
            // Use the Assert class to test conditions
            Assert.Pass("Testing out a Placeholder test until I talk to someone about created tests for this package");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator EditorPlaceholderWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            Assert.Pass("Testing out a Placeholder test until I talk to someone about created tests for this package");
            yield return null;
        }
    }
}
