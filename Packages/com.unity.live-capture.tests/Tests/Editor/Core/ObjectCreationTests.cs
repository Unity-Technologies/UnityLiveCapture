using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.Tests.Editor
{
    class ObjectCreationTests
    {
        const string k_PrefabPath = "Assets/Tmp/Prefab.prefab";

        [SetUp]
        public void Setup()
        {
            SetupCommon();
        }

        [TearDown]
        public void TearDown()
        {
            TeardownCommon();

            CleanupObjects<SynchronizerComponent>();
        }

        public static void SetupCommon()
        {
            Selection.activeGameObject = null;

            AssetDatabase.DeleteAsset("Assets/Tmp");
            AssetDatabase.CreateFolder("Assets", "Tmp");
        }

        public static void TeardownCommon()
        {
            AssetDatabase.DeleteAsset("Assets/Tmp");
        }

        public static void CleanupObjects<T>() where T : Component
        {
            var stage = StageUtility.GetCurrentStage();

            foreach (var obj in stage.FindComponentsOfType<T>())
            {
                var rootGO = obj.transform.root.gameObject;

                GameObject.DestroyImmediate(rootGO);
            }
        }

        public static void CreateAsChild(Func<GameObject> create)
        {
            var root = new GameObject("root");

            Selection.activeGameObject = root;

            var go = create();

            Assert.AreEqual(root.transform, go.transform.parent, "Failed to create as child.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();

            GameObject.DestroyImmediate(root);
        }

        public static void CreateAsChildWithUniqueName(Func<GameObject> create, string expectedName)
        {
            var root = new GameObject("root");

            Selection.activeGameObject = root;

            var go1 = create();

            Selection.activeGameObject = root;

            var go2 = create();

            Assert.AreNotEqual(go1.name, go2.name, "Siblings created with the same name.");
            Assert.AreEqual(expectedName, go1.name, "Created with the wrong name.");
            Assert.AreEqual($"{expectedName} (1)", go2.name, "Sibling created with the wrong name.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();

            GameObject.DestroyImmediate(root);
        }

        public static void CreateSelectsNewObject(Func<GameObject> create)
        {
            var go = create();

            Assert.AreEqual(go, Selection.activeGameObject, "Incorrect selected object after creation.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();
        }

        public static void CreateWithUniqueName(Func<GameObject> create, string expectedName)
        {
            var go1 = create();

            Selection.activeGameObject = null;

            var go2 = create();

            Assert.AreNotEqual(go1.name, go2.name, "Root objects created with the same name.");
            Assert.AreEqual(expectedName, go1.name, "Created with the wrong name.");
            Assert.AreEqual($"{expectedName} (1)", go2.name, "Sibling created with the wrong name.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();
        }

        public static void RedoCreate<T>(Func<GameObject> create) where T : Component
        {
            var go = create();
            var stage = StageUtility.GetCurrentStage();

            Undo.PerformUndo();
            Undo.PerformRedo();

            var component = stage.FindComponentOfType<T>();

            Assert.NotNull(component, "Failed to redo create operation.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();
        }

        public static void RedoCreateAsChild<T>(Func<GameObject> create) where T : Component
        {
            var root = new GameObject("root");

            Selection.activeGameObject = root;

            var go = create();
            var stage = StageUtility.GetCurrentStage();

            Undo.PerformUndo();
            Undo.PerformRedo();

            var component = stage.FindComponentOfType<T>();

            Assert.NotNull(component, "Failed to redo create operation.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();

            GameObject.DestroyImmediate(root);
        }

        public static void UndoCreate<T>(Func<GameObject> create) where T : Component
        {
            var go = create();
            var stage = StageUtility.GetCurrentStage();
            var component = stage.FindComponentOfType<T>();

            var components = stage.FindComponentsOfType<T>();

            Assert.AreEqual(1, components.Length, "Unexpected amount of components in the scene.");
            Assert.AreEqual(go, component.gameObject, "Unexpected component in the scene.");
            Assert.NotNull(component, "Failed to create object.");

            Undo.PerformUndo();

            components = stage.FindComponentsOfType<T>();

            Assert.AreEqual(0, components.Length, "Unexpected amount of components in the scene.");

            component = stage.FindComponentOfType<T>();

            Assert.IsNull(component, "Failed to undo create operation.");
        }

        public static void UndoCreateAsChild<T>(Func<GameObject> create) where T : Component
        {
            var stage = StageUtility.GetCurrentStage();
            var root = new GameObject("root");

            Selection.activeGameObject = root;

            create();

            Undo.PerformUndo();

            var component = stage.FindComponentOfType<T>();

            Assert.IsNull(component, "Failed to undo create operation.");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();

            GameObject.DestroyImmediate(root);
        }

        [Test]
        public void SynchronizerCreateAsChild()
        {
            CreateAsChild(
                () => ObjectCreatorUtilities.CreateSynchronizer()
            );
        }

        [Test]
        public void SynchronizerCreateAsChildWithUniqueName()
        {
            CreateAsChildWithUniqueName(
                () => ObjectCreatorUtilities.CreateSynchronizer(),
                "Timecode Synchronizer"
            );
        }


        [Test]
        public void SynchronizerCreateSelectsNewObject()
        {
            CreateSelectsNewObject(
                () => ObjectCreatorUtilities.CreateSynchronizer()
            );
        }

        [Test]
        public void SynchronizerCreateWithUniqueName()
        {
            CreateWithUniqueName(
                () => ObjectCreatorUtilities.CreateSynchronizer(),
                "Timecode Synchronizer"
            );
        }

        [Test]
        public void SynchronizerRedoCreate()
        {
            RedoCreate<SynchronizerComponent>(
                () => ObjectCreatorUtilities.CreateSynchronizer()
            );
        }

        [Test]
        public void SynchronizerRedoCreateAsChild()
        {
            RedoCreateAsChild<SynchronizerComponent>(
                () => ObjectCreatorUtilities.CreateSynchronizer()
            );
        }

        [Test]
        public void SynchronizerUndoCreate()
        {
            UndoCreate<SynchronizerComponent>(
                () => ObjectCreatorUtilities.CreateSynchronizer()
            );
        }

        [Test]
        public void SynchronizerUndoCreateAsChild()
        {
            UndoCreateAsChild<SynchronizerComponent>(
                () => ObjectCreatorUtilities.CreateSynchronizer()
            );
        }

#if UNITY_2021_2_OR_NEWER

        public static void CreateInPrefabMode<T>(string prefabPath, Action create) where T : Component
        {
            var gameObject = new GameObject("Prefab");
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, prefabPath, InteractionMode.AutomatedAction);
            var prefabStage = PrefabStageUtility.OpenPrefab(prefabPath, gameObject);

            Assert.AreEqual(prefabStage, StageUtility.GetCurrentStage(), "Incorrect current stage");

            create();

            var component = prefabStage.FindComponentOfType<T>();

            Assert.NotNull(component, $"Failed to create a {nameof(T)} inside the prefab");

            StageUtility.GoToMainStage();

            var mainStage = StageUtility.GetMainStage();

            component = mainStage.FindComponentOfType<T>();

            Assert.NotNull(component, $"Prefab does not contain a {nameof(T)}");

            // Prevent Undo stack from getting dirty with deleted objects;
            Undo.PerformUndo();

            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void SynchronizerCreateInPrefabMode()
        {
            CreateInPrefabMode<SynchronizerComponent>(
                k_PrefabPath,
                () => ObjectCreatorUtilities.CreateSynchronizer());
        }
#endif
    }
}
