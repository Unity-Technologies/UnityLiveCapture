using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera.Raycasting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Tests.Editor
{
    public class GraphicsRaycasterDepthTests
    {
        static readonly Vector2 k_DefaultRange = new Vector2(0.3f, 1000);
        const float k_HalfCubeSize = 0.5f;
        const float k_MaxDistanceError = 0.005f;

        GraphicsRaycaster m_Raycaster;
        GameObject m_Cube;
        Camera m_MainCamera;

        [SetUp]
        public void Setup()
        {
            Utility.OpenGameView();

            m_Raycaster = new GraphicsRaycaster();

            var camera = new GameObject("Test Camera", typeof(Camera));
            camera.hideFlags = HideFlags.DontSave;
            m_MainCamera = camera.GetComponent<Camera>();
            m_MainCamera.usePhysicalProperties = false;

            // FOV is important, note that we set the horizontal FOV since we intend to move the camera horizontally.
            m_MainCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(90, m_MainCamera.aspect);

            m_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_Cube.hideFlags = HideFlags.DontSave;

            var bounds = m_Cube.GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.IsTrue(bounds.center == Vector3.zero);
            Assert.IsTrue(bounds.extents == Vector3.one * k_HalfCubeSize);

            ResetTransform(m_MainCamera.gameObject.transform);
            ResetTransform(m_Cube.transform);
        }

        [TearDown]
        public void TearDown()
        {
            m_Raycaster.Dispose();
            Object.DestroyImmediate(m_Cube);
            Object.DestroyImmediate(m_MainCamera.gameObject);
        }

        static void ResetTransform(Transform trs)
        {
            trs.localScale = Vector3.one;
            trs.localRotation = Quaternion.identity;
        }

        public struct RaycastTestInput
        {
            public Vector2 viewportPosition;
            public Vector3 cubePosition;
            public Vector3 cameraPosition;
            public Vector3 cameraForward;
            public Vector2 range;
            public bool expectHit;
        }

        static RaycastTestInput GetDefaultRaycastTestInput(float distance = 2)
        {
            return new RaycastTestInput
            {
                viewportPosition = Vector2.one * 0.5f,
                cubePosition = Vector3.zero,
                cameraPosition = Vector3.back * distance,
                cameraForward = Vector3.forward,
                range = k_DefaultRange,
                expectHit = true
            };
        }

        static RaycastTestInput CameraLooksAtCubeLeftFaceTangent(float offset, bool expectHit)
        {
            var data = GetDefaultRaycastTestInput();
            data.cameraPosition = Vector3.back * 2 + Vector3.left * (k_HalfCubeSize + offset);
            data.expectHit = expectHit;
            return data;
        }

        static RaycastTestInput CameraDoesNotLookAtTheCube(float near)
        {
            var data = GetDefaultRaycastTestInput(6);
            data.range = new Vector2(near, 100);
            data.cameraForward = Vector3.back;
            data.expectHit = false;
            return data;
        }

        static RaycastTestInput CameraMovesLeft(float xNormalizedPosition, bool expectHit)
        {
            var data = GetDefaultRaycastTestInput();
            data.cameraPosition = (Vector3.back + Vector3.left) * 4;
            data.viewportPosition = new Vector2(xNormalizedPosition, 0.5f);
            data.expectHit = expectHit;
            return data;
        }

        static bool DistanceToCube(Vector3 cubePosition, Ray cameraRay, out float distance)
        {
            var cubeBounds = new Bounds(cubePosition, Vector3.one * k_HalfCubeSize * 2);
            return cubeBounds.IntersectRay(cameraRay, out distance);
        }

        protected static IEnumerable<RaycastTestInput> RaycastDataSource()
        {
            // Camera is simply in front of the cube.
            yield return GetDefaultRaycastTestInput();
            yield return GetDefaultRaycastTestInput(6);

            // Camera moves leftwards,
            // cube is still covering screen center.
            yield return CameraLooksAtCubeLeftFaceTangent(-0.01f, true);

            // then screen center is not covered by the cube anymore.
            yield return CameraLooksAtCubeLeftFaceTangent(0.01f, false);

            // Camera moves left, we hit the screen center and miss the cube.
            yield return CameraMovesLeft(0.5f, false);

            // Then we hit the right side of the screen to still reach the cube.
            yield return CameraMovesLeft(1f, true);

            // Try a non axis-aligned raycast, camera looks at cube edge.
            {
                var d = 4;
                var data = GetDefaultRaycastTestInput();
                data.cubePosition = Vector3.left * d;
                data.cameraPosition = Vector3.back * d;
                data.cameraForward = (data.cubePosition - data.cameraPosition).normalized;
                yield return data;
            }

            // Make sure raycast fails in case the cube is out of the camera's field of view,
            // `near` is passed as an argument since it has been observed to have an impact on the precision of the `far` value,
            // by testing with multiple values of `near` we make sure our error tolerance is conservative enough.
            yield return CameraDoesNotLookAtTheCube(0.01f);
            yield return CameraDoesNotLookAtTheCube(1.5f);
            yield return CameraDoesNotLookAtTheCube(20);
        }

        [Test]
        public void RaycastReturnsCorrectDepth([ValueSource(nameof(RaycastDataSource))]
            RaycastTestInput data)
        {
            m_Cube.transform.position = data.cubePosition;
            m_MainCamera.transform.position = data.cameraPosition;
            m_MainCamera.transform.forward = data.cameraForward;
            m_MainCamera.nearClipPlane = data.range.x;
            m_MainCamera.farClipPlane = data.range.y;

            var screenPosition = new Vector2(m_MainCamera.pixelWidth * data.viewportPosition.x, m_MainCamera.pixelHeight * data.viewportPosition.y);
            var success = m_Raycaster.Raycast(m_MainCamera, screenPosition, out var hit);

            Assert.IsTrue(success == data.expectHit);

            if (success)
            {
                var ray = m_MainCamera.ViewportPointToRay(new Vector3(data.viewportPosition.x, data.viewportPosition.y, 0));
                Assert.IsTrue(DistanceToCube(data.cubePosition, ray, out var expectedDistance));

                var error = Mathf.Abs(expectedDistance - hit.distance);
                Assert.IsTrue(error < k_MaxDistanceError,
                    $"Distance error: [{error}] exceeds tolerance: [{k_MaxDistanceError}], distance: [{hit.distance}], expected: [{expectedDistance}].");
            }
        }
    }
}
