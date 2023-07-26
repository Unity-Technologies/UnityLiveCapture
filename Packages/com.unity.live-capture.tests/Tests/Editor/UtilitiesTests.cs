using System;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Tests.Editor
{
    public class UtilitiesTests
    {
        [Test]
        public void CanCreateEmptyGameObject()
        {
            var gameObject = AdditionalCoreUtils.CreateEmptyGameObject();
            var components = gameObject.GetComponents<Component>();
            Assert.IsTrue(components.Length == 1);
            Assert.IsTrue(components[0] is Transform);
            Object.DestroyImmediate(gameObject);
        }
    }
}
