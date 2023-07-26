using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class RuntimePlaceholder
    {
        // A Test behaves as an ordinary method
        [Test]
        public void RuntimePlaceholderSimplePasses()
        {
            // Use the Assert class to test conditions
            Assert.Pass("Testing out a Placeholder test until I talk to someone about created tests for this package");
        }
    }
}
