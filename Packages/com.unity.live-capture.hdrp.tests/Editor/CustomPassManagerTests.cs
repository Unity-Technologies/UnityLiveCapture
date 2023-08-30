using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp
{
    public class CustomPassManagerTests
    {
        [CustomPassOrder(12)]
        sealed class CustomPassA : CustomPass
        {
            protected override void Execute(CustomPassContext ctx) { }
        }

        sealed class CustomPassB : CustomPass
        {
            protected override void Execute(CustomPassContext ctx) { }
        }

        [CustomPassOrder(3)]
        sealed class CustomPassC : CustomPass
        {
            protected override void Execute(CustomPassContext ctx) { }
        }

        sealed class CustomPassD : CustomPass
        {
            protected override void Execute(CustomPassContext ctx) { }
        }

        [Test]
        public void PassesAreAddedOnceAndRemovedProperly()
        {
            var injectionPoint = CustomPassInjectionPoint.BeforeRendering;
            using(new CustomPassManager.Handle<CustomPassA>(injectionPoint))
            using(new CustomPassManager.Handle<CustomPassA>(injectionPoint))
            {
                var pass = FindPassUnique<CustomPassA>(injectionPoint);
                Assert.IsNotNull(pass);
            }
            // Handles have been disposed, pass should have been removed.
            {
                var pass = FindPassUnique<CustomPassA>(injectionPoint);
                Assert.IsNull(pass);
            }
        }

        [UnityTest]
        public IEnumerator PlayModeDoesNotDuplicateManager()
        {
            CreatePassAndMakeSureManagerIsUnique();
            yield return new EnterPlayMode();
            CreatePassAndMakeSureManagerIsUnique();
            yield return new ExitPlayMode();
            CreatePassAndMakeSureManagerIsUnique();
        }

        [UnityTest]
        public IEnumerator DomainReloadDoesNotDuplicateManager()
        {
            CreatePassAndMakeSureManagerIsUnique();
            EditorUtility.RequestScriptReload();
            yield return new WaitForDomainReload();
            CreatePassAndMakeSureManagerIsUnique();
        }

        [Test]
        public void PassesAreOrderedProperly()
        {
            var injectionPoint = CustomPassInjectionPoint.BeforeRendering;
            using(var a = new CustomPassManager.Handle<CustomPassA>(injectionPoint))
            using(var b = new CustomPassManager.Handle<CustomPassB>(injectionPoint))
            using(var c = new CustomPassManager.Handle<CustomPassC>(injectionPoint))
            using(var d = new CustomPassManager.Handle<CustomPassD>(injectionPoint))
            {
                var volume = FindVolume(injectionPoint);
                // Expected order is b/d/c/a or d/b/c/a (since b and d have no order attribute)
                Assert.IsTrue(volume.customPasses[2] == c.GetPass());
                Assert.IsTrue(volume.customPasses[3] == a.GetPass());
            }
        }

        static void CreatePassAndMakeSureManagerIsUnique()
        {
            var injectionPoint = CustomPassInjectionPoint.BeforeRendering;
            using (new CustomPassManager.Handle<CustomPassA>(injectionPoint))
            {
                var managersCount = Resources.FindObjectsOfTypeAll<CustomPassManager>().Length;
                UnityEngine.Assertions.Assert.IsTrue(managersCount == 1);
            }
        }

        public static T FindPassUnique<T>(CustomPassInjectionPoint injectionPoint) where T : CustomPass
        {
            var volume = FindVolume(injectionPoint);
            if (volume == null)
                return null;

            var passes = volume.customPasses.OfType<T>().ToList();
            // Make sure passes are not added twice.
            Assert.IsTrue(passes.Count < 2);

            return passes.Count == 0 ? null : passes[0];
        }

        static CustomPassVolume FindVolume(CustomPassInjectionPoint injectionPoint)
        {
            var managers = Resources.FindObjectsOfTypeAll<CustomPassManager>();
            Assert.IsTrue(managers.Length == 1, $"Found [{managers.Length}] instances of [{nameof(CustomPassManager)}]");

            foreach (var volume in managers[0].GetComponents<CustomPassVolume>())
            {
                if (volume.injectionPoint == injectionPoint)
                    return volume;
            }

            return null;
        }
    }
}
