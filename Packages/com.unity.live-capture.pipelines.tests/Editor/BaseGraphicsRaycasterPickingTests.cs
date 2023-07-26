using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera.Raycasting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Tests.Editor
{
    // This class is marked abstract so that we can control which of our SRP related test projects it runs in.
    public abstract class BaseGraphicsRaycasterPickingTests
    {
        static readonly Vector2 k_DefaultRange = new Vector2(0.3f, 1000);
        const int k_GridSize = 4;
        const string k_TestMaterialsPath = "TestMaterials";

        List<Material> m_Materials = new List<Material>();
        List<GameObject> m_Objects = new List<GameObject>();
        GraphicsRaycaster m_Raycaster;
        Camera m_MainCamera;

        [SetUp]
        public void Setup()
        {
            Utility.OpenGameView();

            m_Raycaster = new GraphicsRaycaster();

            var camera = new GameObject("Test Camera", typeof(Camera));
            camera.hideFlags = HideFlags.DontSave;
            m_MainCamera = camera.GetComponent<Camera>();
            m_MainCamera.nearClipPlane = k_DefaultRange.x;
            m_MainCamera.farClipPlane = k_DefaultRange.y;
            m_MainCamera.usePhysicalProperties = false;
            m_MainCamera.fieldOfView = 90;

            for (var y = 0; y != k_GridSize; ++y)
            {
                for (var x = 0; x != k_GridSize; ++x)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = $"Raycast Test Cube [{x}x{y}]";
                    cube.hideFlags = HideFlags.DontSave;
                    cube.transform.position = new Vector3(x, y, 0) * 1.5f;
                    m_Objects.Add(cube);
                }
            }

            var testMaterials = Resources.LoadAll<Material>(k_TestMaterialsPath);
            Assert.IsTrue(testMaterials.Length > 0,
                $"No tests materials found at path [{k_TestMaterialsPath}]");
            Assert.IsTrue(testMaterials.Length < m_Objects.Count,
                "There are more test materials than test objects. Each material should at least be used once.");

            var index = 0;
            foreach (var obj in m_Objects)
            {
                var material = new Material(testMaterials[index % testMaterials.Length]);
                material.hideFlags = HideFlags.HideAndDontSave;
                var renderer = obj.GetComponent<Renderer>();
                renderer.material = material;
                m_Materials.Add(material);
                ++index;
            }
        }

        [TearDown]
        public void TearDown()
        {
            m_Raycaster.Dispose();

            foreach (var material in m_Materials)
            {
                Object.DestroyImmediate(material);
            }

            m_Materials.Clear();

            foreach (var cube in m_Objects)
            {
                GameObject.DestroyImmediate(cube);
            }

            GameObject.DestroyImmediate(m_MainCamera.gameObject);
        }

        [Test]
        public void ObjectsArePickedCorrectly()
        {
            m_MainCamera.transform.forward = Vector3.forward;

            foreach (var cube in m_Objects)
            {
                m_MainCamera.transform.position = cube.transform.position + Vector3.back * 12;

                var screenPosition = new Vector2(m_MainCamera.pixelWidth * 0.5f, m_MainCamera.pixelHeight * 0.5f);
                var ray = m_MainCamera.ScreenPointToRay(screenPosition);
                var success = m_Raycaster.Raycast(ray.origin, ray.direction, out _, out var gameObject);

                Assert.IsTrue(success);
                Assert.IsTrue(gameObject == cube);
            }
        }
    }
}
