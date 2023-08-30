using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.VirtualCamera.Editor;

namespace Unity.LiveCapture.Tests.Editor
{
    class VirtualCameraObjectCreationTests
    {
        const string k_PrefabPath = "Assets/Tmp/Prefab.prefab";

        [SetUp]
        public void Setup()
        {
            ObjectCreationTests.SetupCommon();
        }

        [TearDown]
        public void TearDown()
        {
            ObjectCreationTests.TeardownCommon();

            ObjectCreationTests.CleanupObjects<VirtualCameraActor>();
        }

        [Test]
        public void ActorCreateAsChild()
        {
            ObjectCreationTests.CreateAsChild(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }

        [Test]
        public void ActorCreateAsChildWithUniqueName()
        {
            ObjectCreationTests.CreateAsChildWithUniqueName(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor(),
                "Virtual Camera Actor"
            );
        }


        [Test]
        public void ActorCreateSelectsNewObject()
        {
            ObjectCreationTests.CreateSelectsNewObject(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }

        [Test]
        public void ActorCreateWithUniqueName()
        {
            ObjectCreationTests.CreateWithUniqueName(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor(),
                "Virtual Camera Actor"
            );
        }

        [Test]
        public void ActorRedoCreate()
        {
            ObjectCreationTests.RedoCreate<VirtualCameraActor>(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }

        [Test]
        public void ActorRedoCreateAsChild()
        {
            ObjectCreationTests.RedoCreateAsChild<VirtualCameraActor>(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }

        [Test]
        public void ActorUndoCreate()
        {
            ObjectCreationTests.UndoCreate<VirtualCameraActor>(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }

        [Test]
        public void ActorUndoCreateAsChild()
        {
            ObjectCreationTests.UndoCreateAsChild<VirtualCameraActor>(
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }

#if UNITY_2021_2_OR_NEWER
        [Test]
        public void ActorCreateInPrefabMode()
        {
            ObjectCreationTests.CreateInPrefabMode<VirtualCameraActor>(
                k_PrefabPath,
                () => VirtualCameraCreatorUtilities.CreateVirtualCameraActor()
            );
        }
#endif
    }
}
