using System;
using System.Collections;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.VirtualCamera.Editor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.LiveCapture.Tests.Editor.Pipelines.Hdrp
{
    public abstract class BaseCustomPassTests
    {
        protected IEnumerator CustomPassIsManagedProperly<TComponent, TCustomPass>(CustomPassInjectionPoint injectionPoint)
            where TComponent : Component
            where TCustomPass : CustomPass
        {
            // Create Actors.
            const int numActors = 2;
            for (var i = 0; i != numActors; ++i)
                VirtualCameraCreatorUtilities.CreateVirtualCameraActor();

            var actors = GameObject.FindObjectsOfType<VirtualCameraActor>();
            Assert.IsTrue(actors.Length == numActors);

            // Add Component for each Actor.
            foreach (var actor in actors)
                actor.gameObject.AddComponent<TComponent>();

            // With HDRP 12.1.2, a null sharedProfile on a Volume will lead to a null-ref error in the Volume Editor.
            // This is a bug. We introduce a workaround here.
            var sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            sharedProfile.hideFlags = HideFlags.DontSave;

            foreach (var actor in actors)
            {
                var volume = actor.GetComponent<Volume>();
                if (volume != null && volume.sharedProfile == null)
                {
                    volume.sharedProfile = sharedProfile;
                }
            }

            for (var i = 0; i != 256; ++i)
                yield return null;

            // Make sure the corresponding custom pass was added.
            {
                var pass = CustomPassManagerTests.FindPassUnique<TCustomPass>(injectionPoint);
                Assert.IsNotNull(pass);
            }

            // Destroy Actors.
            foreach (var actor in actors)
                GameObject.DestroyImmediate(actor.gameObject);

            // Destroy workaround sharedProfile.
            GameObject.DestroyImmediate(sharedProfile);

            // Make sure the corresponding custom pass was removed.
            {
                var pass = CustomPassManagerTests.FindPassUnique<TCustomPass>(injectionPoint);
                Assert.IsNull(pass);
            }
        }
    }
}
