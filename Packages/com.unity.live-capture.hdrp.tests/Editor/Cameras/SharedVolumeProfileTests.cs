using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.LiveCapture.Cameras;
using DepthOfFieldHdrp = UnityEngine.Rendering.HighDefinition.DepthOfField;

namespace Unity.LiveCapture.Tests.Editor
{
    public class SharedVolumeProfileTests
    {
        [Test]
        public void SharedVolumeProfile_WhenComponentGetsDestroyed_DestroysProfile()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var profile = sharedVolumeProfile.GetOrCreateProfile();

            GameObject.DestroyImmediate(sharedVolumeProfile);

            Assert.IsTrue(profile.Equals(null), "The profile should have been destroyed.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_GetOrCreateProfile_ReturnsProfile()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var profile = sharedVolumeProfile.GetOrCreateProfile();

            Assert.IsNotNull(profile, "The profile should not be null.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_GetOrCreateProfile_ReturnsSameProfile()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var profile = sharedVolumeProfile.GetOrCreateProfile();
            var newProfile = sharedVolumeProfile.GetOrCreateProfile();

            Assert.AreEqual(profile, newProfile, "The profile should be the same.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_TryGetProfile_ChecksIfProfileExists()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var profile = sharedVolumeProfile.GetOrCreateProfile();

            Assert.IsTrue(sharedVolumeProfile.TryGetProfile(out var newProfile), "The profile should exist.");
            Assert.AreEqual(profile, newProfile, "The profile should be the same.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_TryGetProfile_ReturnsFalseIfProfileDoesNotExist()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();

            Assert.IsFalse(sharedVolumeProfile.TryGetProfile(out var newProfile), "The profile should not exist.");
            Assert.IsNull(newProfile, "The profile should be null.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_WhenGameObjectGetsDuplicated_TheNewInstanceGetsANewProfile()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var profile = sharedVolumeProfile.GetOrCreateProfile();

            var newGameObject = GameObject.Instantiate(gameObject);
            var newSharedVolumeProfile = newGameObject.GetComponent<SharedVolumeProfile>();
            var newProfile = newSharedVolumeProfile.GetOrCreateProfile();

            Assert.AreNotEqual(profile, newProfile, "The profile should not be the same.");

            GameObject.DestroyImmediate(gameObject);
            GameObject.DestroyImmediate(newGameObject);
        }

        [Test]
        public void SharedVolumeProfile_TryGetVolumeComponent_ChecksIfComponentExists()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var volumeComponent = sharedVolumeProfile.GetOrCreateVolumeComponent<DepthOfFieldHdrp>();

            Assert.IsTrue(sharedVolumeProfile.TryGetVolumeComponent<DepthOfFieldHdrp>(out var newVolumeComponent), "The component should exist.");
            Assert.AreEqual(volumeComponent, newVolumeComponent, "The component should be the same.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_GetOrCreateVolumeComponent_ReturnsComponent()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var volumeComponent = sharedVolumeProfile.GetOrCreateVolumeComponent<DepthOfFieldHdrp>();

            Assert.IsNotNull(volumeComponent, "The component should not be null.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_TryGetVolumeComponent_ReturnsFalseIfComponentDoesNotExist()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();

            Assert.IsFalse(sharedVolumeProfile.TryGetVolumeComponent<DepthOfFieldHdrp>(out var newVolumeComponent), "The component should not exist.");
            Assert.IsNull(newVolumeComponent, "The component should be null.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_DestroyVolumeComponent_DestroysComponent()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();
            var volumeComponent = sharedVolumeProfile.GetOrCreateVolumeComponent<DepthOfFieldHdrp>();

            sharedVolumeProfile.DestroyVolumeComponent<DepthOfFieldHdrp>();

            Assert.IsTrue(volumeComponent.Equals(null), "The component should have been destroyed.");

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SharedVolumeProfile_WhenCreated_RequiresVolumeAndColliderComponents()
        {
            var gameObject = new GameObject();
            var sharedVolumeProfile = gameObject.AddComponent<SharedVolumeProfile>();

            Assert.IsTrue(sharedVolumeProfile.TryGetComponent<Volume>(out _), "The component should have a Volume component.");
            Assert.IsTrue(sharedVolumeProfile.TryGetComponent<SphereCollider>(out var collider), "The component should have a Collider component.");
            Assert.AreEqual(0.01f, collider.radius, "The collider radius should be 0.01.");
            Assert.IsTrue(collider.isTrigger, "The collider should be a trigger.");

            GameObject.DestroyImmediate(gameObject);
        }
    }
}
